using System;
using System.Linq;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Parsing C declaration
    /// </summary>
    public class CDeclaration
    {
        static Parser<string> BuildParser(Func<string, string> stringSetter)
        {
            var identifier = Regex("[A-Za-z0-9_]+");
            var type = identifier;
            var pointer = Str("*").Select(x => " pointer of").ZeroOrMore((x, y) => " pointer of" + x, "");
            var dcl1 = Letrec<string>(
                (dcl, directDcl) => from p in pointer
                                    from d in directDcl
                                    select d + p,
                (dcl, directDcl) =>
                {
                    var dclElem = identifier.Select(x => stringSetter(x)).Select(x => "")
                                  .Choice(Str("(").Concat(dcl).ConcatLeft(Str(")")));
                    var afterDcl = Str("(").Concat(Str(")")).Select(x => " function returning")
                                   .Choice(Str("[").Concat(Regex("[0-9]+").Option("")).ConcatLeft(Str("]"))
                                           .Select(x => " array[" + x + "] of"));

                    return from d in dclElem
                           from a in afterDcl.ZeroOrMore((x, y) => x + y, "")
                           select d + a;
                });

            return from t in type
                   from d in dcl1
                   from e in End()
                   select d + " " + t;
        }

        public static string Parse(string decl)
        {
            string name = null;
            var parser = BuildParser(x => name = x);
            var result = parser.Run(decl, "[ \t\n]+");

            if(result.IsError)
            {
                return "Error: " + result.ErrorMessage;
            }
            else
            {
                return name + ":" + result.Value;
            }
        }
    }
}
