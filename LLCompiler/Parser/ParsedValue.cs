using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Parser
{
    public enum ParsedValuesTypes
    {
        PARSEDSEXPR,
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
    }

    class ParsedIntegerConst : IParsedValue
    {
        public int Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDINTEGERCONST; }
        }
    }

    class ParsedCharConst : IParsedValue
    {
        public char Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDCHARCONST; }
        }
    }

    class ParsedStringConst : IParsedValue
    {
        public string Value { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDSTRINGCONST; }
        }
    }

    class ParsedIdentifier: IParsedValue
    {
        public string Name { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDIDENTIFIER; }
        }
    }
}
