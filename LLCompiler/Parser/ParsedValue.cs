using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Parser
{
    public enum ParsedValuesTypes
    {
        PARSEDSEXPR,
        PARSEDCOND,
        PARSEDINTEGERCONST,
        PARSEDCHARCONST,
        PARSEDSTRINGCONST,
        PARSEDIDENTIFIER
    }

    public interface IParsedValue
    {
        ParsedValuesTypes ParsedValueType { get; }
    } 

    class ParsedSExpr : IParsedValue
    {
        public List<IParsedValue> Members { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDSEXPR; }
        }

        public override string ToString()
        {
            string t = "[";
            foreach (var x in Members) t += x.ToString() + " ";
            t += "]";
            return t;
        }
    }

    class ParsedIntegerConst : IParsedValue
    {
        public int Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDINTEGERCONST; }
        }
        public override string ToString()
        {
            return "i"+Value.ToString();
        }
    }

    class ParsedCharConst : IParsedValue
    {
        public char Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDCHARCONST; }
        }
        public override string ToString()
        {
            return "c'" + Value.ToString()+"'";
        }
    }

    class ParsedStringConst : IParsedValue
    {
        public string Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDSTRINGCONST; }
        }
        public override string ToString()
        {
            return "s\"" + Value + "\"";
        }        
    }

    class ParsedIdentifier: IParsedValue
    {
        public string Name { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDIDENTIFIER; }
        }

        public override string ToString()
        {
            return "@" + Name;
        }        
    }


    public struct CondClause
    {
        public IParsedValue Condition { get; set; }
        public IParsedValue Result { get; set; }
    }

    public class ParsedCondExpression : IParsedValue
    {
        public List<CondClause> Clauses { get; set; }

        public ParsedCondExpression() { Clauses = new List<CondClause>(); }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDCOND; }
        }
        public override string ToString()
        {
            return "cond";
        }        
    }
}
