/*
 * This source code is under the Unlicense
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Morilib.MonadPrimus;

namespace Morilib
{
    [TestClass]
    public class MonadPrimusParserTest
    {
        private void Match<T>(Parser<T> expr, string toParse, Parser<string> skip, Parser<string> follow, int position, int positionExpected, T valueExpected)
        {
            var config = new Env(toParse, skip, follow);
            var result = expr(config, position);

            Assert.IsNull(result.ErrorMessage, result.ErrorMessage);
            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(valueExpected, result.Value);
        }

        private void Match<T>(Parser<T> expr, string toParse, Parser<string> skip, int position, int positionExpected, T valueExpected)
        {
            var config = new Env(toParse, skip);
            var result = expr(config, position);

            Assert.IsNull(result.ErrorMessage, result.ErrorMessage);
            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(valueExpected, result.Value);
        }

        private void NoMatch<T>(Parser<T> expr, string toParse, Parser<string> skip, Parser<string> follow, int position, int positionExpected, string errorMessage)
        {
            var config = new Env(toParse, skip, follow);
            var result = expr(config, position);

            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }

        private void NoMatch<T>(Parser<T> expr, string toParse, Parser<string> skip, int position, int positionExpected, string errorMessage)
        {
            var config = new Env(toParse, skip);
            var result = expr(config, position);

            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }

        private void Match<T>(Parser<T> expr, string toParse, string skip, string follow, int position, int positionExpected, T valueExpected)
        {
            Match(expr, toParse, Regex(skip), Regex(follow), position, positionExpected, valueExpected);
        }

        private void Match<T>(Parser<T> expr, string toParse, string skip, int position, int positionExpected, T valueExpected)
        {
            Match(expr, toParse, Regex(skip), position, positionExpected, valueExpected);
        }

        private void NoMatch<T>(Parser<T> expr, string toParse, string skip, string follow, int position, int positionExpected, string errorMessage)
        {
            NoMatch(expr, toParse, Regex(skip), Regex(follow), position, positionExpected, errorMessage);
        }

        private void NoMatch<T>(Parser<T> expr, string toParse, string skip, int position, int positionExpected, string errorMessage)
        {
            NoMatch(expr, toParse, Regex(skip), position, positionExpected, errorMessage);
        }

        private void Match<T>(Parser<T> expr, string toParse, int position, int positionExpected, T valueExpected)
        {
            var config = new Env(toParse);
            var result = expr(config, position);

            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(valueExpected, result.Value);
        }

        private void NoMatch<T>(Parser<T> expr, string toParse, int position, int positionExpected, string errorMessage)
        {
            var config = new Env(toParse);
            var result = expr(config, position);

            Assert.AreEqual(positionExpected, result.Position);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }

        [TestMethod]
        public void StrTest()
        {
            var expr1 = Str("765");
            var skip2 = Letrec<string>(x => from a in Str("#|")
                                            from b in Str("|#").Not().Concat(x.Choice(Regex("."))).ZeroOrMore()
                                            from c in Str("|#")
                                            select b);

            Match(expr1, "000765", 3, 6, "765");
            NoMatch(expr1, "000961", 3, 3, "Does not match 765");
            Match(expr1, "000   765", " +", 3, 9, "765");
            Match(expr1, "000#|aaa#|aaa|#aaa|#765", skip2, 3, 23, "765");
        }

        [TestMethod]
        public void KeyTest()
        {
            var expr1 = Key("765");

            Match(expr1, "000765", " +", "[\\);]", 3, 6, "765");
            NoMatch(expr1, "000961", " +", "[\\);]", 3, 3, "Does not match keyword 765");
            Match(expr1, "000   765  346", " +", "[\\);]", 3, 9, "765");
            NoMatch(expr1, "000   765pro", " +", "[\\);]", 3, 9, "Does not match keyword 765");
            Match(expr1, "000   765; 346", " +", "[\\);]", 3, 9, "765");
            Match(expr1, "000   765) 346", " +", "[\\);]", 3, 9, "765");
        }

        [TestMethod]
        public void IgnoreCaseTest()
        {
            var expr1 = IgnoreCase("Abc");

            Match(expr1, "000Abc", 3, 6, "Abc");
            Match(expr1, "000ABC", 3, 6, "ABC");
            Match(expr1, "000abc", 3, 6, "abc");
            NoMatch(expr1, "000961", 3, 3, "Does not match Abc");
            Match(expr1, "000   aBC", " +", 3, 9, "aBC");
        }

        [TestMethod]
        public void KeyIgnoreCaseTest()
        {
            var expr1 = KeyIgnoreCase("Abc");

            Match(expr1, "000Abc", " +", "[\\);]", 3, 6, "Abc");
            Match(expr1, "000ABC", " +", "[\\);]", 3, 6, "ABC");
            NoMatch(expr1, "000961", " +", "[\\);]", 3, 3, "Does not match keyword Abc");
            Match(expr1, "000   ABC  346", " +", "[\\);]", 3, 9, "ABC");
            NoMatch(expr1, "000   ABCpro", " +", "[\\);]", 3, 9, "Does not match keyword Abc");
            Match(expr1, "000   ABC; 346", " +", "[\\);]", 3, 9, "ABC");
            Match(expr1, "000   ABC) 346", " +", "[\\);]", 3, 9, "ABC");
        }

        [TestMethod]
        public void RegexTest()
        {
            var expr1 = Regex("8?765");

            Match(expr1, "000765", 3, 6, "765");
            Match(expr1, "0008765", 3, 7, "8765");
            NoMatch(expr1, "000961", 3, 3, "Does not match pattern 8?765");
            Match(expr1, "000   765", " +", 3, 9, "765");
        }

        [TestMethod]
        public void EndTest()
        {
            var expr1 = End();

            Match(expr1, "000", 3, 3, 0);
            NoMatch(expr1, "000961", 3, 3, "Not reached to end of parsing");
            Match(expr1, "000   ", " +", 3, 6, 0);
        }

        [TestMethod]
        public void RealTest()
        {
            var expr1 = Real();

            Match(expr1, "765", 0, 3, 765.0);
        }

        [TestMethod]
        public void SelectTest()
        {
            var expr1 = Str("765").Select(x => int.Parse(x));

            Match(expr1, "000765", 3, 6, 765);
            NoMatch(expr1, "000961", 3, 3, "Does not match 765");
        }

        [TestMethod]
        public void SelectErrorTest()
        {
            var expr1 = Str("765").SelectError(x => "not match", x => 0);

            Match(expr1, "000765", 3, 6, "765");
            NoMatch(expr1, "000961", 3, 0, "not match");
        }

        [TestMethod]
        public void SelectManyMonadRule1Test()
        {
            Func<int, Parser<int>> f1 = x => Str((x + 1).ToString()).Select(y => int.Parse(y) + 1);
            var expr1 = 765.ToParser().SelectMany(f1);
            var expr2 = f1(765);

            Match(expr1, "766", 0, 3, 767);
            Match(expr2, "766", 0, 3, 767);
            NoMatch(expr1, "961", 0, 0, "Does not match 766");
            NoMatch(expr2, "961", 0, 0, "Does not match 766");
        }

        [TestMethod]
        public void SelectManyMonadRule2Test()
        {
            var expr1 = Str("765").Select(y => int.Parse(y) + 1);
            var expr2 = expr1.SelectMany(MonadPrimus.ToParser);

            Match(expr1, "765", 0, 3, 766);
            Match(expr2, "765", 0, 3, 766);
            NoMatch(expr1, "961", 0, 0, "Does not match 765");
            NoMatch(expr2, "961", 0, 0, "Does not match 765");
        }

        [TestMethod]
        public void SelectManyMonadRule3Test()
        {
            Func<int, Parser<int>> f1 = x => Str((x + 1).ToString()).Select(y => int.Parse(y) + 2);
            Func<int, Parser<int>> f2 = x => Str((x - 1).ToString()).Select(y => int.Parse(y) - 2);
            var expr1 = 765.ToParser().SelectMany(f1).SelectMany(f2);
            var expr2 = 765.ToParser().SelectMany(x => f1(x).SelectMany(f2));

            Match(expr1, "766767", 0, 6, 765);
            Match(expr2, "766767", 0, 6, 765);
            NoMatch(expr1, "961767", 0, 0, "Does not match 766");
            NoMatch(expr2, "961767", 0, 0, "Does not match 766");
            NoMatch(expr1, "766961", 0, 3, "Does not match 767");
            NoMatch(expr2, "766961", 0, 3, "Does not match 767");
        }

        [TestMethod]
        public void SelectMany1Test()
        {
            var expr1 = from x in Str("7")
                        from y in Str("6")
                        from z in Str("5")
                        select x + y + z;

            Match(expr1, "765", 0, 3, "765");
            Match(expr1, "7  6  5", " +", 0, 7, "765");
            NoMatch(expr1, "7  5  5", " +", 0, 1, "Does not match 6");
            NoMatch(expr1, "7  6  6", " +", 0, 4, "Does not match 5");
        }

        [TestMethod]
        public void OneOrMoreTest()
        {
            var expr1 = from x in 0.ToParser()
                        from y in Regex("[0-9]").Select(a => int.Parse(a)).OneOrMore((a, b) => a + b)
                        select y;

            Match(expr1, "7", 0, 1, 7);
            Match(expr1, "765", 0, 3, 18);
            NoMatch(expr1, "", 0, 0, "Does not match pattern [0-9]");
        }

        [TestMethod]
        public void ZeroOrMoreTest()
        {
            var expr1 = from x in 0.ToParser()
                        from y in Regex("[0-9]").Select(a => int.Parse(a)).ZeroOrMore((a, b) => a + b, 876)
                        select y;

            Match(expr1, "7", 0, 1, 7);
            Match(expr1, "765", 0, 3, 18);
            Match(expr1, "", 0, 0, 876);
        }

        [TestMethod]
        public void OptionTest()
        {
            var expr1 = Str("765").Option();

            Match(expr1, "765", 0, 3, "765");
            Match(expr1, "765765765876", 0, 3, "765");
            Match(expr1, "876", 0, 0, default(string));
        }

        [TestMethod]
        public void LookaheadTest()
        {
            var expr1 = from a in Str("765").Lookahead()
                        from b in Regex("[0-9]+")
                        select b;

            Match(expr1, "765", 0, 3, "765");
            NoMatch(expr1, "961", 0, 0, "Does not match 765");
        }

        [TestMethod]
        public void NotTest()
        {
            var expr1 = from a in Str("961").Not()
                        from b in Regex("[0-9]+")
                        select b;

            Match(expr1, "765", 0, 3, "765");
            NoMatch(expr1, "961", 0, 0, "Unexpected match");
        }

        [TestMethod]
        public void MatchIfTest()
        {
            var expr1 = Regex("[0-9]+").MatchIf(x => x == "765");

            Match(expr1, "765", 0, 3, "765");
            NoMatch(expr1, "666", 0, 0, "Value is not matched");
            NoMatch(expr1, "aaa", 0, 0, "Does not match pattern [0-9]+");
        }

        [TestMethod]
        public void Letrec1Test()
        {
            var expr1 = Letrec<string>(x => (from a in Str("(")
                                             from b in x.Choice(Str(""))
                                             from c in Str(")")
                                             select a + b + c).SelectError(t => "Unbalanced parenthesis"));

            Match(expr1, "((()))", 0, 6, "((()))");
            NoMatch(expr1, "((())", 0, 5, "Unbalanced parenthesis");
        }

        [TestMethod]
        public void Letrec2Test()
        {
            var expr1 = Letrec<string, string>((x, y) => from a in Str("(")
                                                         from b in y.Choice(Str(""))
                                                         from c in Str(")")
                                                         select a + b + c,
                                               (x, y) => from a in Str("[")
                                                         from b in x
                                                         from c in Str("]")
                                                         select a + b + c);

            Match(expr1, "([([()])])", 0, 10, "([([()])])");
            NoMatch(expr1, "([([()])]", 0, 9, "Does not match )");
        }

        private static dynamic Cons(dynamic car, dynamic cdr)
        {
            return new
            {
                Car = car,
                Cdr = cdr
            };
        }

        [TestMethod]
        public void STest1()
        {
            var expr1 = Letrec<dynamic>((x, y) => (from a in Str("(")
                                                   from b in y
                                                   from c in Str(")")
                                                   select b).Choice(Regex("[^\\s\\(\\)]+").Select(d => (dynamic)d)),
                                        (x, y) => (from a in x
                                                   from b in y
                                                   select Cons(a, b)).Choice(Str("").Select(d => (dynamic)null)));

            var result = expr1.Run("(+ 1 (* 2 3))", " +");
            Assert.AreEqual("+", result.Value.Car);
            Assert.AreEqual("1", result.Value.Cdr.Car);
            Assert.AreEqual("*", result.Value.Cdr.Cdr.Car.Car);
            Assert.AreEqual("2", result.Value.Cdr.Cdr.Car.Cdr.Car);
            Assert.AreEqual("3", result.Value.Cdr.Cdr.Car.Cdr.Cdr.Car);
            Assert.IsNull(result.Value.Cdr.Cdr.Cdr);
            Assert.IsNull(result.Value.Cdr.Cdr.Car.Cdr.Cdr.Cdr);
        }

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

        [TestMethod]
        public void CalcTest()
        {
            var expr1 = Letrec<double>((x, y, z) => y.Delimit(Regex("[\\+\\-]"), Operator),
                (x, y, z) => z.Delimit(Regex("[\\*\\/]"), Operator),
                (x, y, z) => Real().Choice(Str("(").Concat(x).ConcatLeft(Str(")"))));

            Assert.AreEqual(expr1.Run("1-2*3", " +").Value, -5);
            Assert.AreEqual(expr1.Run(" ( 1 - 2) * 3 ", " +").Value, -3);
        }

        [TestMethod]
        public void TagTest1()
        {
            var expr1 = from a1 in Str("<")
                        from a2 in Regex("[A-Za-z0-9]+")
                        from a3 in Str(">")
                        from a4 in (from b1 in Str("</").Concat(IgnoreCase(a2)).ConcatLeft(Str(">")).Not()
                                    from b2 in Regex(".")
                                    select b2).ZeroOrMore((a, b) => a + b, "")
                        from a5 in Str("</")
                        from a6 in IgnoreCase(a2)
                        from a7 in Str(">")
                        select a4;

            Assert.AreEqual(expr1.Run("<script>aaaaaa</script>").Value, "aaaaaa");
            Assert.AreEqual(expr1.Run("<a>aa<b>a</b>aaa</A>").Value, "aa<b>a</b>aaa");
            Assert.AreEqual(expr1.Run("<a></a>").Value, "");
        }

        [TestMethod]
        public void TagTest2()
        {
            var expr1 = Letrec<string>(x => from a1 in Str("<")
                                            from a2 in Regex("[A-Za-z0-9]+")
                                            from a3 in Str(">")
                                            from a4 in (from b1 in Str("</").Concat(IgnoreCase(a2)).ConcatLeft(Str(">")).Not()
                                                        from b2 in x.Choice(Regex("."))
                                                        select b2).ZeroOrMore((a, b) => a + b, "")
                                            from a5 in Str("</")
                                            from a6 in IgnoreCase(a2)
                                            from a7 in Str(">")
                                            select a4);

            Assert.AreEqual(expr1.Run("<script>a<a>a</a>a<b>a</b>aa</script>").Value, "aaaaaa");
            Assert.AreEqual(expr1.Run("<a></a>").Value, "");
        }

        [TestMethod]
        public void LocalTest()
        {
            var expr1 = from x in Str("a")
                        from y in Local(Str("b").Concat(Str("c")), s => new Env(s.ParseString, null))
                        from z in Str("d")
                        select y;

            Match(expr1, "abc    d", " +", 0, 8, "c");
            NoMatch(expr1, "ab c    d", 0, 2, "Does not match c");
        }

        [TestMethod]
        public void ChangeInputTest()
        {
            var expr1 = from x in Str("a")
                        from y in ChangeInput(Str("A").Concat(Str("C")).Concat(Str("D")), "ACD")
                        from z in Str("c")
                        select y;

            Match(expr1, "ac", 0, 2, "D");
        }
    }
}