using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.SemanticAnalyzer;

namespace LLCompiler.CodeGenerator
{
    public class GeneratedCFunction
    {
        public FunctionDefinition FuncDef { get; set; }
        public string CFuncPrototype { get; set; }
        public string CFuncBody { get; set; }
    }
}
