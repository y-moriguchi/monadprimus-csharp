/*
 * This source code is under the Unlicense
 */
using System;
using System.Linq;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Calculator sample.
    /// </summary>
    public class Calculator
    {
        private static double Operator(double x, string op, double y)
        {
            switch (op)
            {
                case "+": return x + y;
                case "-": return x - y;
                case "*": return x * y;
                case "/": return x / y;
                default: throw new Exception("Internal error");
            }
        }

        private static Parser<double> OperatorParser(Parser<double> x, Parser<double> elem)
        {
            var term = elem.Delimit(Regex("[\\*\\/]"), Operator);
            var expr = term.Delimit(Regex("[\\+\\-]"), Operator);

            return expr;
        }

        private static Parser<double> ElementParser(Parser<double> x, Parser<double> elem)
        {
            var element = Real();

            return element.Choice(Str("(").Concat(x).ConcatLeft(Str(")")));
        }

        public static double Calculate(string aString)
        {
            var parser = Letrec<double>(OperatorParser, ElementParser);

            return parser.Run(aString, " +").Value;
        }
    }
}
