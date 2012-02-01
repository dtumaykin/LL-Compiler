using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.SemanticAnalyzer;
using LLCompiler.Parser;

namespace LLCompiler.CodeGenerator
{
    public class CodeGenerator
    {
        private Dictionary<string, FunctionDefinition> FuncDefs;
        private Dictionary<string, GeneratedCFunction> GeneratedCFuncTable;

        private const string wsp = " ";
        private const string nln = "\n";

        public CodeGenerator(Dictionary<string, FunctionDefinition> funcs)
        {
            this.FuncDefs = funcs;
            this.GeneratedCFuncTable = new Dictionary<string, GeneratedCFunction>();
        }

        public void GenerateCFunctions()
        {
            foreach (var f in FuncDefs.Values)
            {
                GenerateCFunction(f);
            }
        }

        public void WriteCPrototypesToFile(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteCFunctionsToFile(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates C code of a function from a funcion definition.
        /// </summary>
        /// <param name="f">Function definition.</param>
        private void GenerateCFunction(FunctionDefinition f)
        {
            // don't generate for library functions
            if (f.Body == null)
                return;

            GeneratedCFunction func = new GeneratedCFunction();
            func.FuncDef = f;

            // generating function prototype
            string proto = GetTypeName(f.RetType) + wsp + f.Name + "(";

            bool x = false;

            foreach (var a in f.Arguments)
            {
                if (x) proto += ",";
                x = true;
                proto += GetTypeName(a.Value) + wsp + a.Key;
            }
            proto += ")";

            func.CFuncPrototype = proto;

            // generating function body
            string body = "";

            if (f.Body.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR) // function call
            {
                body += GenerateCFunctionCall(f.Body as ParsedSExpr);
            }
            else if (f.Body.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
            { }
            else // primitive type
            {
                body += "{" + nln + "return ";

                switch (f.Body.ParsedValueType)
                {
                    case ParsedValuesTypes.PARSEDCHARCONST:
                        body += "'" + (f.Body as ParsedCharConst).Value.ToString() + "'";
                        break;
                    case ParsedValuesTypes.PARSEDIDENTIFIER:
                        body += (f.Body as ParsedIdentifier).Name;
                        break;
                    case ParsedValuesTypes.PARSEDINTEGERCONST:
                        body += (f.Body as ParsedIntegerConst).Value.ToString();
                        break;
                    case ParsedValuesTypes.PARSEDSTRINGCONST:
                        body += "\"" + (f.Body as ParsedStringConst).Value + "\"";
                        break;
                    default:
                        throw new NotImplementedException();
                }

                body += ";" + nln + "}";
            }

            // generating function body


            func.CFuncBody = body;

            this.GeneratedCFuncTable.Add(func.FuncDef.Name, func);
        }

        /// <summary>
        /// Generates C code of function call from ParsedSExpr.
        /// </summary>
        /// <param name="pse"></param>
        /// <returns>Function C code.</returns>
        private string GenerateCFunctionCall(ParsedSExpr pse)
        {
            List<IParsedValue> temp = new List<IParsedValue>(pse.Members);
            string result = "";

            if(temp[0].ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                throw new Exception("CG.GenerateCFunctionCall: Not a function call!"); 


            result += (temp[0] as ParsedIdentifier).Name;
            result += "( ";
            bool x = false;
            foreach (var v in temp)
            {
                if(x)
                    result += ", ";
                x = true;
                switch (v.ParsedValueType)
                {
                    case ParsedValuesTypes.PARSEDCHARCONST:
                        result += "'" + (v as ParsedCharConst).Value.ToString() + "'";
                        break;
                    case ParsedValuesTypes.PARSEDIDENTIFIER:
                        result += (v as ParsedIdentifier).Name;
                        break;
                    case ParsedValuesTypes.PARSEDINTEGERCONST:
                        result += (v as ParsedIntegerConst).Value.ToString();
                        break;
                    case ParsedValuesTypes.PARSEDSEXPR:
                        result += GenerateCFunctionCall(v as ParsedSExpr);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            result += ")";

            return result;
        }

        /// <summary>
        /// Returns C name for generated type.
        /// TODO: Implement String, List, Any.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private string GetTypeName(VarType t)
        {
            switch (t)
            {
                case VarType.Integer:
                    return "int";
                case VarType.Char:
                    return "char";
                case VarType.String:
                    return "LL_String";
                case VarType.List:
                    return "LL_List";
                case VarType.Any:
                    return "LL_Any";
                default:
                    throw new Exception("CD.GetTypeName: Unknown type!");
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
                if (FuncDefs.ContainsKey(funcName))
                    return FuncDefs[funcName];
                throw new Exception("CG.FindFunction: Unknown symbol " + funcName + "!");
            }
            else
                throw new Exception("CG.FindFunction: Not a function call!");

        }
    }
}
