using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LLCompiler;
using LLCompiler.Lexer;

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
            Lexer lx = new Lexer();
            foreach (IToken tk in lx.ProcessString("(a c 12 'b' \"testing\")"))
            {
                Console.Write(tk.TokenType.ToString());
            }
        }
    }
}
    