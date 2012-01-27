using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Parser
{
    public enum ParsedValuesTypes
    {
        PARSEDFUNCALL,
        PARSEDINTEGERCONST,
        PARSEDCHARCONST,
        PARSEDSTRINGCONST,
        PARSEDIDENTIFIER
    }

    public interface IParsedValue
    {
        public ParsedValuesTypes ParsedValueType { get; }
    }

    class ParsedFunCall : IParsedValue
    {
        public ParsedIdentifier Name { get; set; }

        public ParsedValuesTypes ParsedValueType
        {
            get { return ParsedValuesTypes.PARSEDFUNCALL; }
        }

        public List<IParsedValue> Parameteres { get; set; }
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
