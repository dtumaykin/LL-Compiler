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
                yield return ProcessConds(ProcessCurrentToken(iterator));
        }

        /// <summary>
        /// Process func and changes appropriate S Expressions to cond expressions.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static IParsedValue ProcessConds(IParsedValue func)
        {
            if (func.ParsedValueType != ParsedValuesTypes.PARSEDSEXPR) return func;

            if ((func as ParsedSExpr).Members[0].ParsedValueType != ParsedValuesTypes.PARSEDIDENTIFIER)
                throw new Exception("Something strange happened during cond conversion.\n");
            else if (((func as ParsedSExpr).Members[0] as ParsedIdentifier).Name != "cond")
            {
                List<IParsedValue> newMembers = new List<IParsedValue>();
                foreach (var i in (func as ParsedSExpr).Members)
                {
                    newMembers.Add(ProcessConds(i));
                }
                (func as ParsedSExpr).Members = newMembers;
                return func;
            }
            else
            {
                ParsedCondExpression pce = new ParsedCondExpression();
                var mem = (func as ParsedSExpr).Members;
                for (int i = 1; i < mem.Count; i++)
                {
                    if (mem[i].ParsedValueType != ParsedValuesTypes.PARSEDSEXPR) throw new Exception("Incorrect cond clause.");
                    var clause = mem[i] as ParsedSExpr;
                    if (clause.Members.Count != 2) throw new Exception("Incorrect cond clause.");
                    pce.Clauses.Add(new CondClause { Condition = ProcessConds(clause.Members[0]), Result = ProcessConds(clause.Members[1]) });
                }
                return pce;
            }
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
