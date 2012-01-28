using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LLCompiler;
using LLCompiler.Lexer;
using LLCompiler.Parser;

namespace LLCompilerTests
{
    [TestClass]
    public class GlobalTests
    {
        [TestMethod]
        public void HelloWorld()
        {
            Console.Write("Hello world!");
            Console.Read();
        }

        [TestMethod]
        public void LexerStringTest()
        {
            foreach (IToken tk in Lexer.ProcessString("(+ a c 12 'b' \"testing\")"))
            {
                Console.Write(tk.TokenType.ToString());
            }
        }

        [TestMethod]
        public void ParserStringTest()
        {
            foreach (IParsedValue pvl in Parser.ProcessTokens(Lexer.ProcessString("(defun rvrs (l1 l2)  (cond    ((null l1) l2)    (T (cons (car l1) l2))  ))")))
            {
                Console.WriteLine(pvl.ParsedValueType.ToString());
            }
        }
    }
}
    