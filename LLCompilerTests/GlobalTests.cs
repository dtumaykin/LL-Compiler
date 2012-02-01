using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LLCompiler;
using LLCompiler.Lexer;
using LLCompiler.Parser;
using LLCompiler.SemanticAnalyzer;

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

        [TestMethod]
        public void ReadFromFileAndTryToParse()
        {
            string content = File.ReadAllText(@"D:\sources\intel_ws2012\LL-Compiler\LLCompilerTests\input.txt");

            SemanticAnalyzer an = new SemanticAnalyzer();

            var tokens = Lexer.ProcessString(content);

            var values = Parser.ProcessTokens(tokens);

            an.CreateSymbolTable(values);
            an.DeriveTypes();
            an.ValidateFuncCalls();
        }

        [TestMethod]
        public void TryParseCond()
        {
            var content = "(defun rvrs (l1 l2) (cond ((null l1) l2) (T (cons (car l1) l2))))";

            SemanticAnalyzer an = new SemanticAnalyzer();

            var tokens = Lexer.ProcessString(content);

            var values = Parser.ProcessTokens(tokens);

            an.CreateSymbolTable(values);
            an.DeriveTypes();
            an.ValidateFuncCalls();
        }
    }
}