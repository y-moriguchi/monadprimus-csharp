/*
 * This source code is under the Unlicense
 */
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// a parser of script like Bourne shell.
    /// </summary>
    public class ShellParser
    {
        private static Parser<string> CreateHereDocument(string eof)
        {
            var here = Local(from a in (from _1 in Str(eof).Not()
                                        from b in Regex("[\\s\\S]")
                                        select b).ZeroOrMore((a, b) => a + b, "")
                             from b in Str(eof)
                             select a + b,
                             env => new Env(env.ParseString));

            return from a in Str("\n")
                   from b in here
                   select a + b;
        }

        private static Parser<string> CreateShellParser()
        {
            Parser<string> command = null;
            Parser<string> commandList = null;

            var separator = Regex("[\n;]");
            var variable = Regex("[A-Za-z][A-Za-z0-9]*");

            var notCommand = Str("fi")
                .Choice(Str("then"))
                .Choice(Str("do"))
                .Choice(Str("done"))
                .Choice(Str("esac"))
                .Choice(Str("elif"))
                .Choice(Str("else"));
            var word = Regex("(\\.|\'(\\.|[^\'])*\'|\"(\\.|[^\"])*\"|[^\\s\\(\\)\\{\\}<>&\\|;])+");
            var wordList = word.OneOrMore((a, b) => a + " " + b);
            var redirectKey = from a in Regex("(<|>>|>)(&[0-9]+)?")
                              from b in word
                              select a + b;
            var commandElement = redirectKey.Choice(word);
            var simpleCommand = from _1 in notCommand.Not()
                                from x in commandElement
                                from y in commandElement.ZeroOrMore((a, b) => a + " " + b, null)
                                select y == null ? x : x + " " + y;
            var parenthesisCommand = (from a in Str("(")
                                      from b in commandList
                                      from c in Str(")")
                                      select a + b + c)
                                     .Choice(from a in Str("{")
                                             from b in commandList
                                             from c in Str("}")
                                             select a + b + c)
                                     .Choice(simpleCommand);

            // This implementation is not satisfy full of Bourne shell specification.
            var hereDocument = from a in parenthesisCommand
                               from b in (from c in Str("<<")
                                          from d in word
                                          from e in Regex("\n")
                                          from f in Local(from e in (from f in Str(d + "\n").Not()
                                                                     from g in Regex("[\\s\\S]")
                                                                     select g).ZeroOrMore((x, y) => x + y, "")
                                                          from h in Str(d + "\n")
                                                          select e + h,
                                                          env => new Env(env.ParseString))
                                          select " " + c + " " + d + e + f)
                                         .Choice(from c in Regex("(<|>>|>)(&[0-9]+)?")
                                                 from d in word
                                                 select c + d).OneOrMore((x, y) => " " + x + " " + y)
                                         .Option("")
                               select a + b;

            var ifCommand = from a in Str("if")
                            from b in command
                            from _1 in separator
                            from c in Str("then")
                            from _2 in Regex("\n*")
                            from d in commandList
                            from e in (from _3 in separator
                                       from f in Str("elif")
                                       from g in command
                                       from _4 in separator
                                       from h in Str("then")
                                       from _5 in Regex("\n*")
                                       from i in commandList
                                       select "; " + f + " " + g + "; " + h + " " + i).ZeroOrMore((a, b) => a + b)
                            from i in (from _3 in separator
                                       from j in Str("else")
                                       from _5 in Regex("\n*")
                                       from k in commandList
                                       select "; " + j + " " + k).Option("")
                            from _3 in separator
                            from m in Str("fi")
                            select a.ToUpper() + " " + b + "; " + c + " " + d + e + i + "; " + m;
            var forCommand = from a in Str("for")
                             from b in variable
                             from c in (from d in Str("in")
                                        from e in wordList
                                        select " " + d + " " + e).Option("")
                             from _1 in separator
                             from d in Str("do")
                             from _2 in separator
                             from e in commandList
                             from _3 in separator
                             from f in Str("done")
                             select a.ToUpper() + " " + b + c + "; " + d + "; " + e + "; " + f;
            var caseCommand = from a in Str("case")
                              from b in word
                              from c in Str("in")
                              from _1 in Regex("\n*")
                              from d in (from e in word.Delimit(Str("|"), (a, d, b) => a + d + b)
                                         from f in Str(")")
                                         from g in commandList
                                         from h in Str(";;")
                                         from _2 in Regex("\n*")
                                         select e + f + " " + g + h).OneOrMore((a, b) => a + " " + b)
                              from h in Str("esac")
                              select a.ToUpper() + " " + b + " " + c + " " + d + " " + h;
            var whileUntilCommand = from a in Str("while").Choice(Str("until"))
                                    from b in command
                                    from _1 in separator
                                    from c in Str("do")
                                    from _2 in separator
                                    from d in commandList
                                    from _3 in separator
                                    from e in Str("done")
                                    select a.ToUpper() + " " + b + "; " + c + "; " + d + "; " + e;

            var compoundCommand = ifCommand
                                  .Choice(forCommand)
                                  .Choice(caseCommand)
                                  .Choice(whileUntilCommand)
                                  .Choice(hereDocument);

            var pipe = compoundCommand.Delimit(Str("|"), (a, d, b) => a + " " + d + " " + b);
            var andOrCommand = pipe.Delimit(Str("||").Choice(Str("&&")), (a, d, b) => a + " " + d + " " + b);

            command = andOrCommand;
            commandList = command.Delimit(separator.Select(x => ";").Choice(Str("&")), (a, d, b) => a + d + " " + b);

            return from a in command
                   from b in End()
                   select a;
        }

        public static string Parse(string script)
        {
            var skip = Regex("[ \\t]+");
            var parser = CreateShellParser();
            var result = parser.Run(script, skip);
            
            if(result.IsError)
            {
                return result.ErrorMessage + "(" + result.Position + ")";
            }
            else
            {
                return result.Value;
            }
        }
    }
}
