using System;
using System.IO;
using System.Text;
using Collections = System.Collections.Generic;

public sealed class Scanner
{
    public enum ArithToken
    {
        Add,
        Sub,
        Mul,
        Div,
        Semi,
        Equal,
    }

    private readonly Collections.IList<object> _result;

    public Scanner(TextReader input)
    {
        _result = new Collections.List<object>();
        Scan(input);
    }

    public Collections.IList<object> Tokens
    {
        get { return _result; }
    }

    private void Scan(TextReader input)
    {
        while (input.Peek() != -1)
        {
            var ch = (char) input.Peek();

            // Scan individual tokens

            if (char.IsWhiteSpace(ch))
            {
                // eat the current char and skip ahead!
                input.Read();
            }
            else if (char.IsLetter(ch) || ch == '_')
            {
                // keyword or identifier

                var accum = new StringBuilder();

                while (char.IsLetter(ch) || ch == '_')
                {
                    accum.Append(ch);
                    input.Read();

                    if (input.Peek() == -1)
                    {
                        break;
                    }
                    ch = (char) input.Peek();
                }

                _result.Add(accum.ToString());
            }
            else if (ch == '"')
            {
                // string literal
                var accum = new StringBuilder();

                input.Read(); // skip the '"'

                if (input.Peek() == -1)
                {
                    throw new Exception("unterminated string literal");
                }

                while ((ch = (char) input.Peek()) != '"')
                {
                    accum.Append(ch);
                    input.Read();

                    if (input.Peek() == -1)
                    {
                        throw new Exception("unterminated string literal");
                    }
                }

                // skip the terminating "
                input.Read();
                _result.Add(accum);
            }
            else if (char.IsDigit(ch))
            {
                // numeric literal

                var accum = new StringBuilder();

                while (char.IsDigit(ch))
                {
                    accum.Append(ch);
                    input.Read();

                    if (input.Peek() == -1)
                    {
                        break;
                    }
                    ch = (char) input.Peek();
                }

                _result.Add(int.Parse(accum.ToString()));
            }
            else
                switch (ch)
                {
                    case '+':
                        input.Read();
                        _result.Add(ArithToken.Add);
                        break;

                    case '-':
                        input.Read();
                        _result.Add(ArithToken.Sub);
                        break;

                    case '*':
                        input.Read();
                        _result.Add(ArithToken.Mul);
                        break;

                    case '/':
                        input.Read();
                        _result.Add(ArithToken.Div);
                        break;

                    case '=':
                        input.Read();
                        _result.Add(ArithToken.Equal);
                        break;

                    case ';':
                        input.Read();
                        _result.Add(ArithToken.Semi);
                        break;

                    default:
                        throw new Exception("Scanner encountered unrecognized character '" + ch + "'");
                }
        }
    }
}