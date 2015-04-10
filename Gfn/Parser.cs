using System;
using System.Text;
using Collections = System.Collections.Generic;

public sealed class Parser
{
    private readonly Stmt _result;
    private readonly Collections.IList<object> _tokens;
    private int _index;

    public Parser(Collections.IList<object> tokens)
    {
        _tokens = tokens;
        _index = 0;
        _result = ParseStmt();

        if (_index != _tokens.Count)
            throw new Exception("expected EOF");
    }

    public Stmt Result
    {
        get { return _result; }
    }

    private Stmt ParseStmt()
    {
        Stmt result;

        if (_index == _tokens.Count)
        {
            throw new Exception("expected statement, got EOF");
        }

        // <stmt> := print <expr> 

        // <expr> := <string>
        // | <int>
        // | <bin_expr>
        // | <ident>
        if (_tokens[_index].Equals("print"))
        {
            _index++;
            result = new Print(ParseExpr());
        }
        else if (_tokens[_index].Equals("var"))
        {
            _index++;

            if (_index < _tokens.Count && _tokens[_index] is string)
            {
                var ident = (string)_tokens[_index];
                
                _index++;

                if (_index == _tokens.Count ||
                    (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
                {
                    throw new Exception(string.Format("expected = after 'var {0}'", ident));
                }

                _index++;

                var expr = ParseExpr();
                result = new DeclareVar(ident, expr);
            }
            else
            {
                throw new Exception("expected variable name after 'var'");
            }

        }
        else if (_tokens[_index].Equals("read_int"))
        {
            _index++;
            var readInt = new ReadInt();

            if (_index < _tokens.Count &&
                _tokens[_index] is string)
            {
                readInt.Ident = (string)_tokens[_index++];
                result = readInt;
            }
            else
            {
                throw new Exception("expected variable name after 'read_int'");
            }
        }
        else if (_tokens[_index].Equals("read_string"))
        {
            _index++;
            var readString = new ReadString();

            if (_index < _tokens.Count &&
                _tokens[_index] is string)
            {
                readString.Ident = (string)_tokens[_index++];
                result = readString;
            }
            else
            {
                throw new Exception("expected variable name after 'read_string'");
            }
        }
        else if (_tokens[_index].Equals("for"))
        {
            _index++;
            var forLoop = new ForLoop();

            if (_index < _tokens.Count &&
                _tokens[_index] is string)
            {
                forLoop.Ident = (string)_tokens[_index];
            }
            else
            {
                throw new Exception("expected identifier after 'for'");
            }

            _index++;

            if (_index == _tokens.Count ||
                (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            {
                throw new Exception("for missing '='");
            }

            _index++;

            forLoop.From = ParseExpr();

            if (_index == _tokens.Count ||
                !_tokens[_index].Equals("to"))
            {
                throw new Exception("expected 'to' after for");
            }

            _index++;

            forLoop.To = ParseExpr();

            if (_index == _tokens.Count ||
                !_tokens[_index].Equals("do"))
            {
                throw new Exception("expected 'do' after from expression in for loop");
            }

            _index++;

            forLoop.Body = ParseStmt();
            result = forLoop;

            if (_index == _tokens.Count ||
                !_tokens[_index].Equals("end"))
            {
                throw new Exception("unterminated 'for' loop body");
            }

            _index++;
        }
        else if (_tokens[_index] is string)
        {
            // assignment

            var assign = new Assign { Ident = (string)_tokens[_index++] };

            if (_index == _tokens.Count ||
                (Scanner.ArithToken)_tokens[_index] != Scanner.ArithToken.Equal)
            {
                throw new Exception("expected '='");
            }

            _index++;

            assign.Expr = ParseExpr();
            result = assign;
        }
        else
        {
            throw new Exception("parse error at token " + _index + ": " + _tokens[_index]);
        }

        if (_index < _tokens.Count && (Scanner.ArithToken)_tokens[_index] == Scanner.ArithToken.Semi)
        {
            _index++;

            if (_index < _tokens.Count &&
                !_tokens[_index].Equals("end"))
            {
                var sequence = new Sequence { First = result, Second = ParseStmt() };
                result = sequence;
            }
        }

        return result;
    }

    private Expr ParseExpr()
    {
        if (_index == _tokens.Count)
        {
            throw new Exception("expected expression, got EOF");
        }

        var nexttoken = _tokens[_index + 1];

        //check if this is a bin-expr or simple expr
        if (
            (nexttoken is string && (string)nexttoken == "to") ||
            (nexttoken is string && (string)nexttoken == "do") ||
            (nexttoken is Scanner.ArithToken && (Scanner.ArithToken)nexttoken == Scanner.ArithToken.Semi)
            )
        {
            return ParseSimpleExpr();
        }

        var binexpr = new BinExpr();
        switch ((Scanner.ArithToken)nexttoken)
        {
            case Scanner.ArithToken.Add:
                binexpr.Op = BinOp.Add;
                break;
            case Scanner.ArithToken.Sub:
                binexpr.Op = BinOp.Sub;
                break;
            case Scanner.ArithToken.Mul:
                binexpr.Op = BinOp.Mul;
                break;
            case Scanner.ArithToken.Div:
                binexpr.Op = BinOp.Div;
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
        {
            throw new Exception("expected expression, got EOF");
        }

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