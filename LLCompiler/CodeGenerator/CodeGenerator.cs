using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.SemanticAnalyzer;
using LLCompiler.Parser;
using System.IO;

namespace LLCompiler.CodeGenerator
{
    public class CodeGenerator
    {
        private Dictionary<string, Function> FuncDefs;
        private Dictionary<string, GeneratedCFunction> GeneratedCFuncTable;

        private const string wsp = " ";
        private const string nln = "\n";

        public CodeGenerator(Dictionary<string, Function> funcs)
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
            foreach (var f in GeneratedCFuncTable.Keys)
            {
                GeneratedCFunction func = GeneratedCFuncTable[f];
                File.AppendAllText(path, func.CFuncPrototype);
                File.AppendAllText(path, func.CFuncBody + "\n");
            }
        }

        public string WriteCFunctionsToString()
        {
            string res = "";
            foreach (var f in GeneratedCFuncTable.Keys)
            {
                GeneratedCFunction func = GeneratedCFuncTable[f];
                res += func.CFuncPrototype;
                res += func.CFuncBody + "\n";
            }
            return res;
        }


        /// <summary>
        /// Generates C code of a function from a funcion definition.
        /// </summary>
        /// <param name="f">Function definition.</param>
        private void GenerateCFunction(Function f)
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
            body += "{\n\treturn ";

            if (f.Body.ParsedValueType == ParsedValuesTypes.PARSEDSEXPR) // function call
            {
                body += GenerateCCode(f.Body);
            }
            else if (f.Body.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
            {
                // remove return etc
                body = GenerateCCode(f.Body);
            }
            else // primitive type
            {


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

               
            }

            // don't need it for cond
            if(f.Body.ParsedValueType != ParsedValuesTypes.PARSEDCOND)
                body += ";" + nln + "}";

            // generating function body


            func.CFuncBody = body;

            this.GeneratedCFuncTable.Add(func.FuncDef.Name, func);
        }

        /// <summary>
        /// Generates C code from a IParsedValue.
        /// </summary>
        /// <param name="pse"></param>
        /// <returns>Function C code.</returns>
        private string GenerateCCode(IParsedValue pse)
        {
            string result = "";

            switch(pse.ParsedValueType)
            {
                case ParsedValuesTypes.PARSEDSEXPR:
                    List<IParsedValue> temp = new List<IParsedValue>((pse as ParsedSExpr).Members);

                    if (temp[0].ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                        throw new Exception("CG.GenerateCCode: Not a function call!");

                    string calledFuncName = (temp[0] as ParsedIdentifier).Name;
                    temp.RemoveAt(0);

                    // standart function call
                    if (FuncDefs[calledFuncName].Body == null)
                    {
                        result += GenerateStandartCFunctionCall(calledFuncName, temp);
                    }
                    else
                    {
                        result += calledFuncName + "( ";

                        bool x = false;
                        foreach (var a in temp)
                        {
                            if (x) result += ", ";
                            x = true;
                            result += GenerateCCode(a);
                        }

                        result += ")";
                    }
                    break;
                case ParsedValuesTypes.PARSEDINTEGERCONST:
                    result += (pse as ParsedIntegerConst).Value.ToString();
                    break;
                case ParsedValuesTypes.PARSEDIDENTIFIER:
                    result += (pse as ParsedIdentifier).Name;
                    break;
                case ParsedValuesTypes.PARSEDCHARCONST:
                    result += "'" + (pse as ParsedCharConst).Value + "'";
                    break;
                case ParsedValuesTypes.PARSEDSTRINGCONST:
                    result += "\"" + (pse as ParsedStringConst).Value + "\"";
                    break;
                case ParsedValuesTypes.PARSEDCOND:
                    ParsedCondExpression cond = pse as ParsedCondExpression;

                    result += "{\n";

                    foreach (var cl in cond.Clauses)
                    {
                        result += "\n\t if (";
                        
                        if (cl.Condition.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
                            throw new Exception("CG.GenerateCCode: Cond is not accepted as cond condition!");

                        result += GenerateCCode(cl.Condition) + " )";

                        if (cl.Result.ParsedValueType == ParsedValuesTypes.PARSEDCOND)
                            result += GenerateCCode(cl.Result);
                        else
                            result += "\n return " + GenerateCCode(cl.Result) + ";\n";
                    }

                    result += "\n}";
                    break;
                default:
                    break;
            }


            return result;
        }


        /// <summary>
        /// Generates C code for standart functions.
        /// </summary>
        /// <param name="calledFuncName"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private string GenerateStandartCFunctionCall(string calledFuncName, List<IParsedValue> temp)
        {
            string result = "";

            switch (calledFuncName)
            {
                case "if":
                    if(temp.Count != 3)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad if condition!");
                    switch (temp[0].ParsedValueType)
                    {
                        case ParsedValuesTypes.PARSEDINTEGERCONST:
                            int val = (temp[0] as ParsedIntegerConst).Value;
                            if (val != 0)
                                return GenerateCCode(temp[1]);
                            else
                                return GenerateCCode(temp[2]);

                        case ParsedValuesTypes.PARSEDSEXPR:
                            result += GenerateCCode(temp[0]) + " ? " + GenerateCCode(temp[1]) + " : " + GenerateCCode(temp[2]);
                            break;
                        default:
                            throw new Exception("CG.GenerateStandartCFunctionCall: Bad if condition!");
                    }
                    break;

                case ">":
                    if(temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad > !");
                    result += "( " + GenerateCCode(temp[0]) + " > " + GenerateCCode(temp[1]) + " )";
                    break;

                case "<":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad < !");
                    result += "( " + GenerateCCode(temp[0]) + " < " + GenerateCCode(temp[1]) + " )";
                    break;

                case ">=":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad >= !");
                    result += "( " + GenerateCCode(temp[0]) + " >= " + GenerateCCode(temp[1]) + " )";
                    break;

                case "<=":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad <= !");
                    result += "( " + GenerateCCode(temp[0]) + " <= " + GenerateCCode(temp[1]) + " )";
                    break;

                case "=":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad = !");
                    result += "( " + GenerateCCode(temp[0]) + " == " + GenerateCCode(temp[1]) + " )";
                    break;

                case "!=":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad != !");
                    result += "(" + GenerateCCode(temp[0]) + " != " + GenerateCCode(temp[1]) + ")";
                    break;

                case "*":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad * !");
                    result += GenerateCCode(temp[0]) + " * " + GenerateCCode(temp[1]);
                    break;

                case "/":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad / !");
                    result += GenerateCCode(temp[0]) + " / " + GenerateCCode(temp[1]);
                    break;

                case "+":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad + !");
                    result += "( " + GenerateCCode(temp[0]) + " + " + GenerateCCode(temp[1]) + " )";
                    break;

                case "-":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad - !");
                    result += "( " + GenerateCCode(temp[0]) + " - " + GenerateCCode(temp[1]) + " )";
                    break;

                case "car":
                    if (temp.Count != 1)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad `car` call!");
                    result += "LL_Car(" + GenerateCCode(temp[0])  + ")";
                    break;

                case "cdr":
                    if (temp.Count != 1)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad `cdr` call!");
                    result += "LL_Cdr(" + GenerateCCode(temp[0])  + ")";
                    break;

                case "null":
                    if (temp.Count != 1)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad `null` call!");
                    result += "LL_Null(" + GenerateCCode(temp[0])  + ")";
                    break;

                case "cons":
                    if (temp.Count != 2)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad `cons` call!");
                    result += "LL_Cons(" + GenerateCCode(temp[0]) + ", "+ GenerateCCode(temp[1]) + ")";
                    break;


                case "atom":
                    if (temp.Count != 1)
                        throw new Exception("CG.GenerateStandartCFunctionCall: Bad `atom` call!");
                    result += "LL_Atom(" + GenerateCCode(temp[0])  + ")";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns C name for generated type.
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
        private Function FindFunction(ParsedSExpr sexpr)
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
