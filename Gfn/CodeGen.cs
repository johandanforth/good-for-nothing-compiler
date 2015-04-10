using System;
using System.IO;
using System.Reflection.Emit;
using Collections = System.Collections.Generic;
using Reflect = System.Reflection;

public sealed class CodeGen
{
    private readonly AssemblyBuilder _asmb;
    private readonly MethodBuilder _methb;
    private readonly ModuleBuilder _modb;
    private readonly string _moduleName;
    private readonly Stmt _stmt;
    private readonly TypeBuilder _typeBuilder;
    private ILGenerator _il;
    private Collections.Dictionary<string, LocalBuilder> _symbolTable;

    public CodeGen(Stmt stmt, string moduleName)
    {
        _stmt = stmt;
        _moduleName = moduleName;
        if (Path.GetFileName(moduleName) != moduleName)
        {
            throw new Exception("can only output into current directory!");
        }

        var filename = Path.GetFileNameWithoutExtension(moduleName);
        var asmName = new Reflect.AssemblyName(filename);
        _asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save);
        _modb = _asmb.DefineDynamicModule(moduleName);
        _typeBuilder = _modb.DefineType("Foo");

        _methb = _typeBuilder.DefineMethod("Main", Reflect.MethodAttributes.Static, typeof (void),
            Type.EmptyTypes);

        // CodeGenerator
        _il = _methb.GetILGenerator();
        _symbolTable = new Collections.Dictionary<string, LocalBuilder>();
    }

    public void Compile()
    {
        // Go Compile!
        GenStmt(_stmt);

        _il.Emit(OpCodes.Ret);
        _typeBuilder.CreateType();
        _modb.CreateGlobalFunctions();
        _asmb.SetEntryPoint(_methb);
        _asmb.Save(_moduleName);
    }


    private void GenStmt(Stmt stmt)
    {
        if (stmt is Sequence)
        {
            var seq = (Sequence) stmt;
            GenStmt(seq.First);
            GenStmt(seq.Second);
        }

        else if (stmt is DeclareVar)
        {
            // declare a local
            var declare = (DeclareVar) stmt;
            _symbolTable[declare.Ident] = _il.DeclareLocal(TypeOfExpr(declare.Expr));

            // set the initial value
            var assign = new Assign();
            assign.Ident = declare.Ident;
            assign.Expr = declare.Expr;
            GenStmt(assign);
        }

        else if (stmt is Assign)
        {
            var assign = (Assign) stmt;
            GenerateLoadToStackForExpr(assign.Expr, TypeOfExpr(assign.Expr));
            GenerateStoreFromStack(assign.Ident, TypeOfExpr(assign.Expr));
        }
        else if (stmt is Print)
        {
            // the "print" statement is an alias for System.Console.WriteLine. 
            // it uses the string case
            GenerateLoadToStackForExpr(((Print) stmt).Expr, typeof (string));
            //Generate console.writeline
            _il.Emit(OpCodes.Call, typeof (Console).GetMethod("WriteLine", new[] {typeof (string)}));
        }

        else if (stmt is ReadInt)
        {
            _il.Emit(OpCodes.Call,
                typeof (Console).GetMethod("ReadLine", Reflect.BindingFlags.Public | Reflect.BindingFlags.Static, null,
                    new Type[] {}, null));
            _il.Emit(OpCodes.Call,
                typeof (int).GetMethod("Parse", Reflect.BindingFlags.Public | Reflect.BindingFlags.Static, null,
                    new[] {typeof (string)}, null));
            GenerateStoreFromStack(((ReadInt) stmt).Ident, typeof (int));
        }
        else if (stmt is ReadString)
        {
            _il.Emit(OpCodes.Call,
                typeof (Console).GetMethod("ReadLine", Reflect.BindingFlags.Public | Reflect.BindingFlags.Static, null,
                    new Type[] {}, null));
            GenerateStoreFromStack(((ReadString) stmt).Ident, typeof (string));
        }
        else if (stmt is ForLoop)
        {
            // example: 
            // for x = 0 to 100 do
            //   print "hello";
            // end;

            // x = 0
            var forLoop = (ForLoop) stmt;
            var assign = new Assign();
            assign.Ident = forLoop.Ident;
            assign.Expr = forLoop.From;
            GenStmt(assign);
            // jump to the test
            var test = _il.DefineLabel();
            _il.Emit(OpCodes.Br, test);

            // statements in the body of the for loop
            var body = _il.DefineLabel();
            _il.MarkLabel(body);
            GenStmt(forLoop.Body);

            // to (increment the value of x)
            _il.Emit(OpCodes.Ldloc, _symbolTable[forLoop.Ident]);
            _il.Emit(OpCodes.Ldc_I4, 1);
            _il.Emit(OpCodes.Add);
            GenerateStoreFromStack(forLoop.Ident, typeof (int));

            // **test** does x equal 100? (do the test)
            _il.MarkLabel(test);
            _il.Emit(OpCodes.Ldloc, _symbolTable[forLoop.Ident]);
            GenerateLoadToStackForExpr(forLoop.To, typeof (int));
            _il.Emit(OpCodes.Blt, body);
        }
        else
        {
            throw new Exception("don't know how to gen a " + stmt.GetType().Name);
        }
    }

    private void GenerateStoreFromStack(string name, Type type)
    {
        if (_symbolTable.ContainsKey(name))
        {
            var locb = _symbolTable[name];

            if (locb.LocalType == type)
            {
                _il.Emit(OpCodes.Stloc, _symbolTable[name]);
            }
            else
            {
                throw new Exception("'" + name + "' is of type " + locb.LocalType.Name +
                                    " but attempted to store value of type " + type.Name);
            }
        }
        else
        {
            throw new Exception("undeclared variable '" + name + "'");
        }
    }

    private void GenerateLoadToStackForExpr(Expr expr, Type expectedType)
    {
        Type deliveredType;

        if (expr is StringLiteral)
        {
            deliveredType = typeof (string);
            _il.Emit(OpCodes.Ldstr, ((StringLiteral) expr).Value);
        }
        else if (expr is IntLiteral)
        {
            deliveredType = typeof (int);
            _il.Emit(OpCodes.Ldc_I4, ((IntLiteral) expr).Value);
        }
        else if (expr is Variable)
        {
            var ident = ((Variable) expr).Ident;
            deliveredType = TypeOfExpr(expr);

            if (!_symbolTable.ContainsKey(ident))
            {
                throw new Exception("undeclared variable '" + ident + "'");
            }

            _il.Emit(OpCodes.Ldloc, _symbolTable[ident]);
        }
        else if (expr is BinExpr)
        {
            var binExpr = (BinExpr) expr;
            var left = binExpr.Left;
            var right = binExpr.Right;
            deliveredType = TypeOfExpr(expr);

            GenerateLoadToStackForExpr(left, expectedType);
            GenerateLoadToStackForExpr(right, expectedType);
            switch (binExpr.Op)
            {
                case BinOp.Add:
                    _il.Emit(OpCodes.Add);
                    break;
                case BinOp.Sub:
                    _il.Emit(OpCodes.Sub);
                    break;
                case BinOp.Mul:
                    _il.Emit(OpCodes.Mul);
                    break;
                case BinOp.Div:
                    _il.Emit(OpCodes.Div);
                    break;
                default:
                    throw new NotImplementedException("Don't know how to generate il load code for " + binExpr.Op +
                                                      " yet!");
            }
        }
        else
        {
            throw new Exception("don't know how to generate " + expr.GetType().Name);
        }

        if (deliveredType != expectedType)
        {
            if (deliveredType == typeof (int) &&
                expectedType == typeof (string))
            {
                _il.Emit(OpCodes.Box, typeof (int));
                _il.Emit(OpCodes.Callvirt, typeof (object).GetMethod("ToString"));
            }
            else
            {
                throw new Exception("can't coerce a " + deliveredType.Name + " to a " + expectedType.Name);
            }
        }
    }


    private Type TypeOfExpr(Expr expr)
    {
        if (expr is StringLiteral)
        {
            return typeof (string);
        }
        if (expr is IntLiteral)
        {
            return typeof (int);
        }
        if (expr is Variable)
        {
            var var = (Variable) expr;
            if (_symbolTable.ContainsKey(var.Ident))
            {
                var locb = _symbolTable[var.Ident];
                return locb.LocalType;
            }
            throw new Exception("undeclared variable '" + var.Ident + "'");
        }
        if (expr is BinExpr)
        {
            //figure out what this expression returns by checking the left
            var leftExpr = ((BinExpr) expr).Left;
            return TypeOfExpr(leftExpr);
        }
        throw new Exception("don't know how to calculate the type of " + expr.GetType().Name);
    }
}