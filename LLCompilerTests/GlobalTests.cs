using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
