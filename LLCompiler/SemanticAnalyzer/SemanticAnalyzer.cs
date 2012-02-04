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
                
                ValidateFuncCall(func.Body);
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

                var t = FuncTable.Values.Where(x => x.Body != null);
                foreach (var func in t)
                {
                    // deriving arguments types
                    if (DeriveFuncArgsType(func))
                        changed = true;
                    // deriving return type
                    VarType oldType = func.RetType;
                    DeriveFuncRetType(func);
                    if (oldType != func.RetType)
                        changed = true;
                }
                itrCount++;
            }
        }

        private VarType DeriveIPVRetType(FunctionDefinition context, IParsedValue ipv)
        {
            var Result = VarType.Nothing;
            switch (ipv.ParsedValueType)
            {
                case ParsedValuesTypes.PARSEDCHARCONST: return VarType.Char;
                case ParsedValuesTypes.PARSEDIDENTIFIER: 
                    var an = (ipv as ParsedIdentifier).Name;
                    if (an == "nil" || an == "T") return VarType.Any;
                    return context.Arguments[(ipv as ParsedIdentifier).Name];
                case ParsedValuesTypes.PARSEDINTEGERCONST: return VarType.Integer;
                case ParsedValuesTypes.PARSEDSTRINGCONST: return VarType.String;
                case ParsedValuesTypes.PARSEDCOND:                    
                    foreach (var s in (ipv as ParsedCondExpression).Clauses)
                        Result = Inf(Result, DeriveIPVRetType(context, s.Result));
                    return Result;
                case ParsedValuesTypes.PARSEDSEXPR:
                    var se = ipv as ParsedSExpr;
                    if ((se.Members[0] as ParsedIdentifier).Name == "if") // Crutch
                      return Inf(Result, Inf(DeriveIPVRetType(context, se.Members[1]), DeriveIPVRetType(context, se.Members[1])));
                    return FindFunction(ipv as ParsedSExpr).RetType;
                default: // Serious shit. You wont ever see that
                    return VarType.Nothing;
            }
        }

        /// <summary>
        /// Derives function return types, basing on called functions return type.
        /// </summary>
        /// <param name="func"></param>
        private void DeriveFuncRetType(FunctionDefinition func) 
        {
            if (func.Body == null)
                return;

            func.RetType = DeriveIPVRetType(func, func.Body);
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
                func.Arguments[arg] = GetArgumentType(arg, (func.Body));
                if (func.Arguments[arg] != oldType)
                    changed = true;
            }

            return changed;
        }
                              
        private VarType Inf(VarType v1, VarType v2) {
            if (v1 == VarType.Nothing) return v2;
            if (v2 == VarType.Nothing) return v1;

            return v1 == v2 ? v1 : VarType.Any; 
        }

        private VarType Sup(VarType v1, VarType v2)
        {
            if (v1 == VarType.Any) return v2;
            if (v2 == VarType.Any) return v1;

            return v1 == v2 ? v1 : VarType.Nothing; 
        }

        private VarType GetArgumentType(string arg, IParsedValue val)
        {
            VarType result = VarType.Any;
            if (val.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR)
            {
                var pse = val as ParsedSExpr;            
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
                        result = Sup(result, calledFunc.Arguments.ElementAt(idx).Value);
                    }
                }

                // search in function calls
                foreach (var fa in temp.Where(x => x.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR ||x.ParsedValueType == ParsedValuesTypes.PARSEDCOND))
                {
                    result = Sup(result, GetArgumentType(arg, fa));
                }
            }
            else if (val.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
            {
                var pse = val as ParsedCondExpression;
                foreach (var x in pse.Clauses)
                {
                    result = Sup(result, GetArgumentType(arg, x.Condition));
                    result = Sup(result, GetArgumentType(arg, x.Result));
                }
            }
            return result;
        }
        /// <summary>
        /// Rercursive function, valdating function call.
        /// Raises Exception in case of failure.
        /// </summary>
        /// <param name="sexpr">Should contain: FuncName, Params.</param>
        private void ValidateFuncCall(IParsedValue call)
        {
            if (call.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR)
            {
                var sexpr = call as ParsedSExpr;
                FunctionDefinition func = FindFunction(sexpr);
                List<VarType> callArgList = GetFuncCallArguments(sexpr);
                List<VarType> funcArgList = func.Arguments.Values.ToList();

                if (callArgList.Count != funcArgList.Count)
                    throw new Exception("SA: Wrong arguments list length at " + func.Name + " function call!");

                for (int i = 0; i < callArgList.Count; i++)
                    if (!IsTypesCompatible(funcArgList[i], callArgList[i]))
                        throw new Exception("SA: Wrong arguments type at " + func.Name + " function call!");
            }
            else if (call.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
            {
                var ccl = call as ParsedCondExpression;
                foreach (var cl in ccl.Clauses)
                {
                    ValidateFuncCall(cl.Condition);
                    ValidateFuncCall(cl.Result);
                }
            }
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
            return t1 == VarType.Any || t2 == VarType.Any || t1 == t2;
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
                    { "op1", VarType.Any},
                    { "op2", VarType.List}
                },
                Body = null
            };
        }

    }
}