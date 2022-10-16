using System.Collections.Generic;

namespace Ash.Lexing
{
    public class Lexer
    {
        private string source;

        private List<Token> tokens;

        private int start;
        private int current;
        private int line;

        private int commentNest = 0;

        private bool AtEnd => current >= source.Length;

        private Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "as", TokenType.As },
            { "bool", TokenType.BoolKW },
            { "break", TokenType.Break },
            { "catch", TokenType.Catch },
            { "case", TokenType.Case },
            { "class", TokenType.Class },
            { "const", TokenType.Const },
            { "decimal", TokenType.DecimalKW },
            { "else", TokenType.Else },
            { "enum", TokenType.Enum },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "foreach", TokenType.Foreach },
            { "function", TokenType.Function },
            { "if", TokenType.If },
            { "import", TokenType.Import },
            { "int", TokenType.IntKW },
            { "namespace", TokenType.Namespace },
            { "null", TokenType.Null },
            { "out", TokenType.Out },
            { "private", TokenType.Private },
            { "return", TokenType.Return },
            { "string", TokenType.StringKW },
            { "switch", TokenType.Switch },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "try", TokenType.Try },
            { "use", TokenType.Use },
            { "while", TokenType.While }
        };

        public Lexer(string source)
        {
            this.source = source;
            tokens = new List<Token>();
            start = 0;
            current = 0;
            line = 1;
        }

        public List<Token> TokenizeAll()
        {
            while (!AtEnd)
            {
                start = current;
                Tokenize();
            }

            MakeToken(TokenType.EoF);
            return tokens;
        }

        private void Tokenize()
        {
            char c = Advance();
            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    line++;
                    break;

                case '.': MakeToken(TokenType.Dot); break;
                case ',': MakeToken(TokenType.Comma); break;
                case ';': MakeToken(TokenType.Semicolon); break;
                case '(': MakeToken(TokenType.LeftParen); break;
                case ')': MakeToken(TokenType.RightParen); break;
                case '{': MakeToken(TokenType.LeftBrace); break;
                case '}': MakeToken(TokenType.RightBrace); break;
                case '[': MakeToken(TokenType.LeftBracket); break;
                case ']': MakeToken(TokenType.RightBracket); break;
                case '<': MakeToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
                case '>': MakeToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
                case '+': MakeToken(TokenType.Plus); break;
                case '-': MakeToken(TokenType.Minus); break;
                case '?': MakeToken(TokenType.QuestionMark); break;
                case ':': MakeToken(Match(':') ? TokenType.DoubleColon : TokenType.Colon); break;
                case '*':
                    {
                        if (Match('*'))
                        {
                            MakeToken(TokenType.Xor);
                        }
                        else if (Match('/'))
                        {
                            commentNest--;
                        }
                        else
                        {
                            MakeToken(TokenType.Star);
                        }

                        break;
                    }
                case '/':
                    {
                        if (Match('/'))
                        {
                            while (Peek() != '\n' && !AtEnd) Advance();
                        }
                        else if (Match('*'))
                        {
                            commentNest++;
                        }
                        else
                        {
                            MakeToken(TokenType.Slash);
                            break;
                        }

                        if (commentNest > 0)
                        {
                            Comment();
                        }

                        break;
                    }
                case '=':
                    {
                        if (Match('=')) MakeToken(TokenType.IsEqual);
                        else if (Match('>')) MakeToken(TokenType.RightArrow);
                        else MakeToken(TokenType.Assignment);
                        break;
                    }
                case '&':
                    {
                        if (Match('&')) MakeToken(TokenType.And);
                        else AshCompiler.Error(line, "Unexpected character: " + Peek());
                        break;
                    }
                case '|':
                    {
                        if (Match('|')) MakeToken(TokenType.Or);
                        else AshCompiler.Error(line, "Unexpected character: " + Peek());
                        break;
                    }
                case '!':
                    {
                        if (Match('!')) MakeToken(TokenType.Nor);
                        else if (Match('=')) MakeToken(TokenType.NotEqual);
                        else MakeToken(TokenType.Bang);
                        break;
                    }
                case '"':
                    MakeString();
                    break;
                default:
                    if (char.IsDigit(c)) MakeNumber();
                    else if (char.IsLetter(c) || c == '_') MakeIdentifier();
                    else
                    {
                        AshCompiler.Error(line, "Unexpected character: " + c);
                    }
                    break;
            }
        }

        private void Comment()
        {
            char c = Advance();

            if (AtEnd)
            {
                AshCompiler.Error(line, "Unterminated comment.");
                return;
            }

            while (commentNest > 0 && !AtEnd)
            {
                if (c == '*' && Match('/'))
                {
                    commentNest--;
                }
                if (c == '\n') line++;
                c = Advance();
            }
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private char Peek()
        {
            if (AtEnd) return '\0';
            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool Match(char c)
        {
            if (AtEnd) return false;
            if (source[current] != c) return false;
            current++;
            return true;
        }

        private void MakeToken(TokenType type) => MakeToken(type, null);

        private void MakeToken(TokenType type, object value)
        {
            tokens.Add(new Token(type, value, source[start..current], line));
        }

        private void MakeNumber()
        {
            TokenType type = TokenType.Int;

            while (char.IsDigit(Peek())) Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                type = TokenType.Decimal;
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }

            object value = type == TokenType.Int ? int.Parse(source[start..current]) : double.Parse(source[start..current]);

            MakeToken(type, value);
        }

        private void MakeIdentifier()
        {
            while (char.IsDigit(Peek()) || char.IsLetter(Peek()) || Peek() == '_') Advance();

            string text = source[start..current];

            if (keywords.ContainsKey(text))
            {
                MakeToken(keywords[text]);
            }
            else
            {
                MakeToken(TokenType.Identifier);
            }
        }

        private void MakeString()
        {
            while (Peek() != '"' && !AtEnd)
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (AtEnd)
            {
                AshCompiler.Error(line, "Unterminated string.");
            }

            Advance();

            MakeToken(TokenType.String, source.Substring(start + 1, current - start - 2));
        }
    }
}
