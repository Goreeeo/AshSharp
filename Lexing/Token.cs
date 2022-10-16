namespace Ash.Lexing
{
    public enum TokenType
    {
        // Single Character Tokens
        Dot, Comma, Semicolon,
        LeftParen, RightParen, LeftBrace, RightBrace, LeftBracket, RightBracket,
        Plus, Minus, Star, Slash, QuestionMark, Colon, Assignment,

        // Double Character Tokens
        RightArrow, DoubleColon,

        // Comparison
        And, Or, Nor, Xor,
        Bang, Greater, Less,
        GreaterEqual, LessEqual,
        IsEqual, NotEqual,

        //Keywords
        As, BoolKW, Break, Catch, Case, Class, Const, DecimalKW, DynamicKW, Else, Enum, False, For, Foreach,
        Function, If, Import, IntKW, Namespace, Out, Private, Return, StringKW, Switch, This, True, Try, Use, While,

        // Other
        Bool, Decimal, Identifier, Int, String,

        Null,

        EoF
    }

    public struct Token
    {
        public Token(TokenType type, object value, string lexeme, int line)
        {
            this.type = type;
            this.value = value;
            this.lexeme = lexeme;
            this.line = line;
        }

        public readonly TokenType type;
        public readonly object value;
        public readonly string lexeme;
        public readonly int line;

        public override string ToString()
        {
            return $"{line} | {type} | {value} | { lexeme }";
        }
    }
}
