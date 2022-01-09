/*
 * This source code is under the Unlicense
 */
using System;
using System.Collections.Generic;
using System.Linq;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// sample: CSV parser.
    /// </summary>
    public class CSV
    {
        private static readonly Parser<IEnumerable<IEnumerable<string>>> Parser = CreateParser();

        private static Parser<IEnumerable<IEnumerable<string>>> CreateParser()
        {
            var parseCell = (from _1 in Str("\"")
                             from x in Str("\"\"").Select(x => "\"").Choice(Regex("[^\"]")).ZeroOrMore((a, b) => a + b, "")
                             from _2 in Str("\"")
                             select x).Choice(Regex("[^\",\n]*"));
            var parseRow = from _1 in Str("\n").Choice(End().Select(x => "")).Not()
                           from x in parseCell.Select(x => (IEnumerable<string>)new string[] { x }).Delimit(Str(","), (a, d, b) => a.Concat(b))
                           select x;
            var parseColumn =
                parseRow.Select(x => (IEnumerable<IEnumerable<string>>)new IEnumerable<string>[] { x }).Delimit(Str("\n"), (a, d, b) => a.Concat(b));

            return from a in parseColumn
                   from b in Str("\n").Option()
                   from c in End()
                   select a;
        }

        public static IEnumerable<IEnumerable<string>> Parse(string data)
        {
            var result = Parser.Run(data);

            if(result.IsError)
            {
                throw new Exception("Invalid CSV");
            }
            else
            {
                return result.Value;
            }
        }
    }
}
