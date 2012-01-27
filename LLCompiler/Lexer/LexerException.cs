using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Lexer
{
    class LexerException : Exception
    {
        public LexerException()
            : base()
        { }

        public LexerException(string msg) 
            : base(msg) 
        { }
    }
}
