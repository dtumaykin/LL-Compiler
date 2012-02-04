using System;//
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Lexer
{
    public enum TokenTypes
    {
        PARANTHESE,
        INTEGERCONST,
        CHARCONST,
        STRINGCONST,
        IDENTIFIER
    }

    public interface IToken
    {
        TokenTypes TokenType { get; }
    }

    class ParentheseToken : IToken
    {
        public bool isOpening { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return  TokenTypes.PARANTHESE;}
        }

        public override string ToString()
        {
            return "ParentheseToken { isOpening = \"" + isOpening.ToString() + "\" }";
        }
    }

    class IntegerConstantToken : IToken
    {
        public int Value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.INTEGERCONST; }
        }

        public override string ToString()
        {
            return "IntegerConstantToken { Value = \"" + Value.ToString()  + "\" }";
        }
    }

    class CharConstantToken : IToken
    {
        public char Value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.CHARCONST; }
        }

        public override string ToString()
        {
            return "CharConstantToken { Value = \"" + Value + "\" }";
        }
    }

    class StringConstantToken : IToken
    {
        public string Value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.STRINGCONST; }
        }

        public override string ToString()
        {
            return "StringConstantToken { Value = \"" + Value + "\" }";
        }
    }

    class IdentifierToken : IToken
    {
        public string Name { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.IDENTIFIER; }
        }

        public override string ToString()
        {
            return "IdentifierToken { Name = \"" + Name + "\" }";
        }
    }
}
