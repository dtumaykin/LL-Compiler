using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.Parser;

namespace LLCompiler.SemanticAnalyzer
{

    public enum VarType
    {
        Any,
        Integer,
        Char,
        String,
        List,
        Nothing
    }

    public class FunctionDefinition
    {
        public string Name { get; set; }

        public VarType RetType { get; set; }

        public Dictionary<string, VarType> Arguments { get; set; }

        public IParsedValue Body { get; set; }

        public FunctionDefinition()
        {
            RetType = VarType.Nothing;
        }
    }

}
