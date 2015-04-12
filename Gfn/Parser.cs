using System;
using System.Collections.Generic;
using System.Text;

public sealed class Parser
{
    private Stmt _result;
    private readonly IList<object> _tokens;
    private int _index;

    public Parser(IList<object> tokens)
    {
        _tokens = tokens;
        _index = 0;
    }

    public void Parse()
    {
        _result = ParseNextStmt();

        if (_index != _tokens.Count)
            throw new Exception("expected EOF");
    }

    public Stmt Result
    {
        get { return _result; }
    }

    private Stmt ParseNextStmt()
    {
        Stmt result;

        if (_index == _tokens.Count)
        {
            throw new Exception("expected statement, got EOF");
        }

        if (_tokens[_index].Equals("print"))
        {
            result = ParsePrint();
        }
        else if (_tokens[_index].Equals("var"))
        {
            result = ParseVar();
        }
        else if (_tokens[_index].Equals("read_int"))
        {
            result = ParseReadInt();
        }
        else if (_tokens[_index].Equals("read_string"))
        {
            result = ParseReadString();
        }
        else if (_tokens[_index].Equals("for"))
        {
            result = ParseForLoop();
        }
        else if (_tokens[_index] is string) // variable assignment
        {
            result = ParseAssignment();
        }
        else
        {
            throw new Exception("parse error at token " + _index + ": " + _tokens[_index]);
        }

        _index++;


        if (_index >= _tokens.Count)    // no more to parse
            return result;

        if (_tokens[_index].Equals("end")) // end of for-loop NOTE: Can this be moved closer to the "for" part above?
            return result;

        return new Sequence { First = result, Second = ParseNextStmt() };
    }

    private Stmt ParseAssignment()
    {
        var ident = (string)_tokens[_index++];

        if (_index == _tokens.Count ||
            (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            throw new Exception("expected '='");

        _index++;

        return new Assign { Ident = ident, Expr = ParseExpr() };
    }

    private Stmt ParseForLoop()
    {
        _index++;
        var forLoop = new ForLoop();

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected identifier after 'for'");

        forLoop.Ident = (string)_tokens[_index];

        _index++;

        if (_index == _tokens.Count ||
            (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            throw new Exception("for missing '='");

        _index++;

        forLoop.From = ParseExpr();

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("to"))
            throw new Exception("expected 'to' after for");

        _index++;

        forLoop.To = ParseExpr();   //TODO: Change compiler - loop ends one step early

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("do"))
            throw new Exception("expected 'do' after from expression in for loop");

        _index++;

        forLoop.Body = ParseNextStmt();

        if (_index == _tokens.Count ||
            !_tokens[_index].Equals("end"))
            throw new Exception("unterminated 'for' loop body");

        _index++;

        return forLoop;
    }

    private Stmt ParseReadString()
    {
        _index++;

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected variable name after 'read_string'");

        return new ReadString((string)_tokens[_index++]);
    }

    private Stmt ParseReadInt()
    {
        _index++;

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected variable name after 'read_int'");

        return new ReadInt((string)_tokens[_index++]);
    }

    private Stmt ParsePrint()
    {
        _index++;
        return new Print(ParseExpr());
    }

    private Stmt ParseVar()
    {
        _index++;

        if (_index >= _tokens.Count || !(_tokens[_index] is string))
            throw new Exception("expected variable name after 'var'");

        var ident = (string)_tokens[_index];

        _index++;

        if (_index == _tokens.Count ||
            (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            throw new Exception(string.Format("expected = after 'var {0}'", ident));

        _index++;

        var expr = ParseExpr();
        return new DeclareVar(ident, expr);
    }

    private Expr ParseExpr()
    {
        if (_index == _tokens.Count) throw new Exception("expected expression, got EOF");

        var nexttoken = _tokens[_index + 1];

        //check if this is a arithmetic-expr or simple expr
        if (
            (nexttoken is string && (string)nexttoken == "to") ||   // loop
            (nexttoken is string && (string)nexttoken == "do") ||   // loop
            (nexttoken is Scanner.ArithToken && (Scanner.ArithToken)nexttoken == Scanner.ArithToken.Semi)
            )
        {
            return ParseSimpleExpr();
        }

        var binexpr = new ArithExpr();
        switch ((Scanner.ArithToken)nexttoken)
        {
            case Scanner.ArithToken.Add:
                binexpr.Op = ArithOp.Add;
                break;
            case Scanner.ArithToken.Sub:
                binexpr.Op = ArithOp.Sub;
                break;
            case Scanner.ArithToken.Mul:
                binexpr.Op = ArithOp.Mul;
                break;
            case Scanner.ArithToken.Div:
                binexpr.Op = ArithOp.Div;
                break;
        }
        binexpr.Left = ParseSimpleExpr();
        _index++;
        binexpr.Right = ParseExpr();
        return binexpr;
    }

    private Expr ParseSimpleExpr()
    {
        if (_index == _tokens.Count)
            throw new Exception("expected expression, got EOF");

        if (_tokens[_index] is StringBuilder)
        {
            var value = _tokens[_index++].ToString();
            var stringLiteral = new StringLiteral(value);
            return stringLiteral;
        }

        if (_tokens[_index] is int)
        {
            var intValue = (int)_tokens[_index++];
            var intLiteral = new IntLiteral(intValue);
            return intLiteral;
        }

        if (_tokens[_index] is string)
        {
            var ident = (string)_tokens[_index++];
            var var = new Variable(ident);
            return var;
        }

        throw new Exception("expected string literal, int literal, or variable");
    }
}