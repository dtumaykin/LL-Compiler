using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLCompiler.Lexer
{

    public class Lexer
    {
        public static IEnumerable<IToken> ProcessString(string str)
        {
            char[] ops = { '+', '-', '/', '*' };
            for(int i = 0; i < str.Length; i++)
            {
                if(char.IsWhiteSpace(str[i]))
                    continue;

                // identifier token
                if(char.IsLetter(str[i]) || Array.IndexOf(ops, str[i]) != -1)
                {
                    StringBuilder tk = new StringBuilder(str[i].ToString());
                    while (i + 1 < str.Length && char.IsLetter(str[i + 1]))
                        tk.Append(str[++i]);

                    yield return new IdentifierToken { name = tk.ToString() };
                    continue;
                }

                // digit token
                if(char.IsDigit(str[i]))
                {
                    StringBuilder integer = new StringBuilder(str[i].ToString());
                    while (i + 1 < str.Length && char.IsDigit(str[i + 1]))
                        integer.Append(str[++i]);

                    yield return new IntegerConstantToken { value = int.Parse(integer.ToString()) };
                    continue;
                }

                // parenthese token
                if (str[i] == '(' || str[i] == ')')
                {
                    switch (str[i])
                    {
                        case '(':
                            yield return new ParentheseToken { isOpening = true };
                            break;
                        case ')':
                            yield return new ParentheseToken { isOpening = false };
                            break;
                    }
                    continue;
                }

                // char token
                if (str[i] == '\'')
                {
                    yield return new CharConstantToken { value = str[++i] }; // yield char
                    i++; // remove last '
                    continue;
                }

                // string token
                if (str[i] == '\"')
                {
                    StringBuilder strtk = new StringBuilder();
                    while (i + 1 < str.Length && str[i + 1] != '\"')
                        strtk.Append(str[++i]);
                    yield return new StringConstantToken { value = strtk.ToString() };
                    i++; // remove last "
                    continue;
                }

                // if we reach this point, the token is unknown -> exception
                throw new LexerException("Lexer: Unkown token!");
            }

            

            yield break;
        }
    }
}
