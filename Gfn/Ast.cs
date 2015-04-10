using System.Data.Common;

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
    public Print( Expr expr)
    {
        Expr = expr;
    }
   public Expr Expr { get; set; }
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
    public string Ident { get; set; }
}

// read_string <ident>
public class ReadString : Stmt
{
    public string Ident { get; set; }
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
}

// <string> := " <string_elem>* "
public class StringLiteral : Expr
{
    public StringLiteral(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }
}

// <int> := <digit>+
public class IntLiteral : Expr
{
    public IntLiteral(int value)
    {
        Value = value;
    }

    public int Value { get; private set; }
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
}

// <bin_expr> := <expr> <bin_op> <expr>
public class BinExpr : Expr
{
    public Expr Left { get; set; }
    public BinOp Op { get; set; }
    public Expr Right { get; set; }
}

// <bin_op> := + | - | * | /
public enum BinOp
{
    Add,
    Sub,
    Mul,
    Div
}