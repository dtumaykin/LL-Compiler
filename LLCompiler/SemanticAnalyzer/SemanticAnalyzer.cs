using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.Parser;

namespace LLCompiler.SemanticAnalyzer
{
    public class SemanticAnalyzer
    {
        private Dictionary<string, FunctionDefinition> FuncTable;
        private bool FuncTablePopulated;

        public SemanticAnalyzer()
        {
            FuncTable = new Dictionary<string, FunctionDefinition>();
            FuncTablePopulated = false;
        }

        /// <summary>
        /// Creates symbol table from a list of IParsedValue
        /// </summary>
        /// <param name="funcs"></param>
        public void CreateSymbolTable(IEnumerable<IParsedValue> funcs)
        {
            InitFuncTable();

            foreach (IParsedValue func in funcs)
            {
                // not a func definition
                if (func.ParsedValueType != ParsedValuesTypes.PARSEDSEXPR)
                    throw new Exception("Not a Func definition!");

                ParsedSExpr pse = func as ParsedSExpr;
                FunctionDefinition newFunc = new FunctionDefinition();

                // not a defun
                if (pse.Members[0].ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                    throw new Exception("Not a defun!");

                ParsedIdentifier calledFuncName = pse.Members[0] as ParsedIdentifier;
                
                // not a defun
                if (calledFuncName.Name.ToLower() != "defun")
                    throw new Exception("Not a defun!");

                // not a function name definition
                if (pse.Members[1].ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                    throw new Exception("Not a function name definition!");

                ParsedIdentifier newFuncName = pse.Members[1] as ParsedIdentifier;
                newFunc.Name = newFuncName.Name.ToLower();

                // adding arguments
                if (pse.Members[2].ParsedValueType != ParsedValuesTypes.PARSEDSEXPR)
                    throw new Exception("Not an argument list!");
                Dictionary<string, VarType> arguments = new Dictionary<string, VarType>();
                //List<IParsedValue> argList = (pse.Members[2] as ParsedSExpr).Members;
                foreach (IParsedValue ipv in (pse.Members[2] as ParsedSExpr).Members)
                {
                    if (ipv.ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                        throw new Exception("Not an identifier!");
                    arguments[(ipv as ParsedIdentifier).Name] = VarType.Any;
                }

                newFunc.Arguments = arguments;

                // adding a body
                newFunc.Body = pse.Members[3];

                // add to symbol table
                FuncTable[newFunc.Name] = newFunc;
            }

            FuncTablePopulated = true;
        }

        /// <summary>
        /// Validates function calls, looks for undefined symbols.
        /// </summary>
        public void ValidateFuncCalls()
        {
            foreach(var func in FuncTable.Values)
            {
                // library function
                if (func.Body == null)
                    continue;


            }
        }

        /// <summary>
        /// Adds standart library function into symbol table
        /// </summary>
        private void InitFuncTable()
        {
            FuncTable = new Dictionary<string, FunctionDefinition>();

            FuncTable["+"] = new FunctionDefinition
            {
                Name = "+",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            
        }

    }
}