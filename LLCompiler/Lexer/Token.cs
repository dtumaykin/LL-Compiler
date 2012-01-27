using System;
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
    }

    class IntegerConstantToken : IToken
    {
        public int value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.INTEGERCONST; }
        }
    }

    class CharConstantToken : IToken
    {
        public char value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.CHARCONST; }
        }
    }

    class StringConstantToken : IToken
    {
        public string value { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.STRINGCONST; }
        }
    }

    class IdentifierToken : IToken
    {
        public string name { get; set; }

        TokenTypes IToken.TokenType
        {
            get { return TokenTypes.IDENTIFIER; }
        }
    }
}
