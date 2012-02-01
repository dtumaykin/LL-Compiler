using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.Parser;

namespace LLCompiler.SemanticAnalyzer
{
    public class SemanticAnalyzer
    {
        public Dictionary<string, FunctionDefinition> FuncTable;

        public SemanticAnalyzer()
        {
            FuncTable = new Dictionary<string, FunctionDefinition>();
            //FuncTablePopulated = false;
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

                ValidateFuncCall(func.Body as ParsedSExpr);
            }
        }

        /// <summary>
        /// Updates function return types in base of it's arguments
        /// </summary>
        public void DeriveTypes()
        {
            bool changed = true;
            int itrCount = 0; // fix possible infinite loop
            while (changed && itrCount < 20)
            {
                changed = false;

                foreach (var func in FuncTable.Values)
                {
                    // don't derive for library functions
                    if (func.Body == null)
                        continue;

                    // deriving return type
                    VarType oldType = func.RetType;
                    DeriveFuncRetType(func);
                    if (oldType != func.RetType)
                        changed = true;

                    // deriving arguments types
                    if (DeriveFuncArgsType(func))
                        changed = true;
                }
                itrCount++;
            }
        }

        /// <summary>
        /// Derives function return types, basing on called functions return type.
        /// TODO: refactor interface(rval)-
        /// </summary>
        /// <param name="func"></param>
        private void DeriveFuncRetType(FunctionDefinition func)
        {
            if (func.Body == null)
                return;

            FunctionDefinition funcCalled = FindFunction(func.Body as ParsedSExpr);
            if (func.RetType != funcCalled.RetType)
                func.RetType = funcCalled.RetType;

            // already done, don't need to derive
            if (funcCalled.RetType != VarType.Any)
                return;

            List<VarType> callArgTypesList = GetFuncCallArguments(func.Body as ParsedSExpr);
            bool sameType = true; VarType type = callArgTypesList[0];
            foreach (VarType t in callArgTypesList)
                if (t != type)
                    sameType = false;

            if (sameType)
                func.RetType = type;
        }

        /// <summary>
        /// Derives function argumets types, basing on called functions return type.
        /// </summary>
        /// <param name="func"></param>
        /// <returns>True, if func args type is changed</returns>
        private bool DeriveFuncArgsType(FunctionDefinition func)
        {
            bool changed = false;
            foreach (var arg in func.Arguments.Keys.ToList())
            {
                VarType oldType = func.Arguments[arg];
                func.Arguments[arg] = GetArgumentType(arg, (func.Body as ParsedSExpr));
                if (func.Arguments[arg] != oldType)
                    changed = true;
            }

            return changed;
        }

        private VarType GetArgumentType(string arg, ParsedSExpr pse)
        {
            VarType result = VarType.Any;
            List<IParsedValue> temp = new List<IParsedValue>(pse.Members);

            // get function
            FunctionDefinition calledFunc = FindFunction(pse);

            // remove function name
            temp.RemoveAt(0);

            // search in direct parameters
            foreach (var a in temp.Where(x => x.ParsedValueType == ParsedValuesTypes.PARSEDIDENTIFIER))
            {
                // found our parameter
                if ((a as ParsedIdentifier).Name == arg)
                {
                    int idx = temp.IndexOf(a);
                    result = calledFunc.Arguments.ElementAt(idx).Value;
                }
            }

            // search in function calls
            foreach (var fa in pse.Members.Where(x => x.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR))
            {
                result = GetArgumentType(arg, (fa as ParsedSExpr));
            }

            return result;
        }

        /// <summary>
        /// Rercursive function, valdating function call.
        /// Raises Exception in case of failure.
        /// </summary>
        /// <param name="sexpr">Should contain: FuncName, Params.</param>
        private void ValidateFuncCall(ParsedSExpr sexpr)
        {
            FunctionDefinition func = FindFunction(sexpr);
            List<VarType> callArgList = GetFuncCallArguments(sexpr);
            List<VarType> funcArgList = func.Arguments.Values.ToList();

            if (callArgList.Count != funcArgList.Count)
                throw new Exception("SA: Wrong arguments list length at " + func.Name + " function call!");

            for(int i = 0; i < callArgList.Count; i++)
                if(!IsTypesCompatible(funcArgList[i], callArgList[i]))
                    throw new Exception("SA: Wrong arguments type at " + func.Name + " function call!");
        }

        /// <summary>
        /// Looks for a function in a FuncTable.
        /// Raises Exception if func is not found.
        /// </summary>
        /// <param name="sexpr"></param>
        /// <returns></returns>
        private FunctionDefinition FindFunction(ParsedSExpr sexpr)
        {
            if (sexpr.Members[0].ParsedValueType == ParsedValuesTypes.PARSEDIDENTIFIER)
            {
                string funcName = (sexpr.Members[0] as ParsedIdentifier).Name;
                if(!FuncTable.ContainsKey(funcName))
                    throw new Exception("SA: Unknown symbol " + funcName + "!");
                return FuncTable[funcName];
            }
            else
                throw new Exception("SA: Not a function call!");

        }

        /// <summary>
        /// Recursively gets function call arguments types list.
        /// </summary>
        /// <param name="sexpr"></param>
        /// <returns></returns>
        private List<VarType> GetFuncCallArguments(ParsedSExpr sexpr)
        {
            List<VarType> result = new List<VarType>();

            foreach (IParsedValue val in sexpr.Members)
            {
                switch (val.ParsedValueType)
                {
                    case ParsedValuesTypes.PARSEDCHARCONST:
                        result.Add(VarType.Char);
                        break;
                    case ParsedValuesTypes.PARSEDIDENTIFIER:
                        result.Add(VarType.Any);
                        break;
                    case ParsedValuesTypes.PARSEDINTEGERCONST:
                        result.Add(VarType.Integer);
                        break;
                    case ParsedValuesTypes.PARSEDSEXPR:
                        result.Add(FindFunction(val as ParsedSExpr).RetType);
                        break;
                    case ParsedValuesTypes.PARSEDSTRINGCONST:
                        result.Add(VarType.String);
                        break;
                    default:
                        break;
                }
            }

            // remove function name
            result.RemoveAt(0);

            return result;
        }

        /// <summary>
        /// Verifies types compatibility.
        /// Any -> Int, Char, String, List -> Null
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        private bool IsTypesCompatible(VarType t1, VarType t2)
        {
            if (t1 == VarType.Any)
                return true;

            if (t1 == t2)
                return true;

            return false;
        }

        /// <summary>
        /// Adds standard library function into symbol table
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

            FuncTable["-"] = new FunctionDefinition
            {
                Name = "-",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["*"] = new FunctionDefinition
            {
                Name = "*",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["/"] = new FunctionDefinition
            {
                Name = "/",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable[">"] = new FunctionDefinition
            {
                Name = ">",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable[">="] = new FunctionDefinition
            {
                Name = ">=",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["<"] = new FunctionDefinition
            {
                Name = "<",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["<="] = new FunctionDefinition
            {
                Name = "<=",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["="] = new FunctionDefinition
            {
                Name = "=",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["!="] = new FunctionDefinition
            {
                Name = "!=",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer },
                    { "op2", VarType.Integer }
                },
                Body = null
            };

            FuncTable["car"] = new FunctionDefinition
            {
                Name = "car",
                RetType = VarType.Any,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.List}
                },
                Body = null
            };

            FuncTable["cdr"] = new FunctionDefinition
            {
                Name = "cdr",
                RetType = VarType.List,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.List}
                },
                Body = null
            };

            FuncTable["null"] = new FunctionDefinition
            {
                Name = "car",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.List}
                },
                Body = null
            };

            FuncTable["atom"] = new FunctionDefinition
            {
                Name = "atom",
                RetType = VarType.Integer,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Any}
                },
                Body = null
            };

            // from here down -> verify
            FuncTable["if"] = new FunctionDefinition
            {
                Name = "if",
                RetType = VarType.Any,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Integer},
                    { "op2", VarType.Any},
                    { "op3", VarType.Any}
                },
                Body = null
            };

            FuncTable["cons"] = new FunctionDefinition
            {
                Name = "cons",
                RetType = VarType.Any,
                Arguments = new Dictionary<string, VarType>() { 
                    { "op1", VarType.Any}
                },
                Body = null
            };
        }

    }
}