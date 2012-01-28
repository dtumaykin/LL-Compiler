using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLCompiler.Lexer;

namespace LLCompiler.Parser
{
    public class Parser
    {
        public static IEnumerable<IParsedValue> ProcessTokens(IEnumerable<IToken> tokens)
        {
            IEnumerator<IToken> iterator = tokens.GetEnumerator();
            while (iterator.MoveNext())
                yield return ProcessCurrentToken(iterator);
        }

        private static IParsedValue ProcessCurrentToken(IEnumerator<IToken> iterator)
        {
            IToken tk = iterator.Current;
            switch(tk.TokenType)
            {
                case TokenTypes.CHARCONST:
                    return new ParsedCharConst { Value = (tk as CharConstantToken).Value };
                case TokenTypes.INTEGERCONST:
                    return new ParsedIntegerConst { Value = (tk as IntegerConstantToken).Value };
                case TokenTypes.IDENTIFIER:
                    return new ParsedIdentifier { Name = (tk as IdentifierToken).Name };
                case TokenTypes.STRINGCONST:
                    return new ParsedStringConst { Value = (tk as StringConstantToken).Value };
                case TokenTypes.PARANTHESE:
                    if((tk as ParentheseToken).isOpening)
                    {
                        List<IParsedValue> content = new List<IParsedValue>();
                        iterator.MoveNext();
                        while(!((iterator.Current.TokenType == TokenTypes.PARANTHESE) && !((iterator.Current as ParentheseToken).isOpening))) 
                        {
                            content.Add(ProcessCurrentToken(iterator));
                            iterator.MoveNext();
                        }
                        return new ParsedSExpr { Members = content };
                    }
                    else
                        throw new Exception("Parser: Shit just got real!/Unexpected parentesis.");
                default:
                    return null;
            }
        }
    }
}
