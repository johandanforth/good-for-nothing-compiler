
using System;

public abstract class Stmt
{
}

// var <ident> = <expr>
public class DeclareVar : Stmt
{
    public DeclareVar(string ident, Expr expr)
    {
        Expr = expr;
        Ident = ident;
    }

    public Expr Expr { get; private set; }
    public string Ident { get; private set; }
}

// print <expr>
public class Print : Stmt
{
    public Print(Expr expr)
    {
        Expr = expr;
    }
    public Expr Expr { get; private set; }
}

// <ident> = <expr>
public class Assign : Stmt
{
    public Expr Expr { get; set; }
    public string Ident { get; set; }
}

// for <ident> = <expr> to <expr> do <stmt> end
public class ForLoop : Stmt
{
    public Stmt Body { get; set; }
    public Expr From { get; set; }
    public string Ident { get; set; }
    public Expr To { get; set; }
}

// read_int <ident>
public class ReadInt : Stmt
{
    public ReadInt(string ident)
    {
        Ident = ident;
    }

    public string Ident { get; private set; }
}

// read_string <ident>
public class ReadString : Stmt
{
    public ReadString(string ident)
    {
        Ident = ident;
    }

    public string Ident { get; private set; }
}

// <stmt> ; <stmt>
public class Sequence : Stmt
{
    public Stmt First { get; set; }
    public Stmt Second { get; set; }
}

/* <expr> := <string>
 *  | <int>
 *  | <bin_expr>
 *  | <ident>
 */

public abstract class Expr
{
    public abstract Type GetType();
}

// <string> := " <string_elem>* "
public class StringLiteral : Expr
{
    public StringLiteral(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }
    public override Type GetType()
    {
        return typeof(string);
    }
}

// <int> := <digit>+
public class IntLiteral : Expr
{
    public IntLiteral(int value)
    {
        Value = value;
    }

    public int Value { get; private set; }
    public override Type GetType()
    {
        return typeof(int);
    }
}

// <ident> := <char> <ident_rest>*
// <ident_rest> := <char> | <digit>
public class Variable : Expr
{
    public Variable(string ident)
    {
        Ident = ident;
    }

    public string Ident { get; private set; }
    public override Type GetType()
    {
        if (!CodeGen.SymbolTable.ContainsKey(Ident)) throw new Exception("undeclared variable '" + Ident + "'");

        var locb = CodeGen.SymbolTable[Ident];
        return locb.LocalType;
    }
}

// <arith_expr> := <expr> <arith_op> <expr>
public class ArithExpr : Expr
{
    public Expr Left { get; set; }
    public ArithOp Op { get; set; }
    public Expr Right { get; set; }

    public override Type GetType()
    {
        return Left.GetType();
    }
}

// <arith_op> := + | - | * | /
public enum ArithOp
{
    Add,
    Sub,
    Mul,
    Div
}