/*
 * This source code is under the Unlicense
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Morilib.MonadPrimus;

namespace Morilib
{
    [TestClass]
    public class MonadPrimusParserOptTest
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
        public void JavaIdentifierTest()
        {
            var expr = JavaIdentifier();

            Match(expr, "abcd", 0, 4, "abcd");
            Match(expr, "abc1", 0, 4, "abc1");
            Match(expr, "ABC1", 0, 4, "ABC1");
            Match(expr, "$abc", 0, 4, "$abc");
            Match(expr, "_abc", 0, 4, "_abc");
            Match(expr, "$123", 0, 4, "$123");
            Match(expr, "_123", 0, 4, "_123");
            Match(expr, "あずき", 0, 3, "あずき");
            Match(expr, "日本語", 0, 3, "日本語");
            Match(expr, "Früh", 0, 4, "Früh");
            Match(expr, "Noël", 0, 4, "Noël");
            Match(expr, "니나", 0, 2, "니나");
            Match(expr, "НИНА", 0, 4, "НИНА");
            Match(expr, "αζυκι", 0, 5, "αζυκι");
            Match(expr, "Quốc_Ngữ", 0, 8, "Quốc_Ngữ");
            Match(expr, "رسالة", 0, 5, "رسالة");
            Match(expr, "עברית", 0, 5, "עברית");
            Match(expr, "ไทย", 0, 3, "ไทย");
            NoMatch(expr, "666", 0, 0, "Does not match identifier");
            NoMatch(expr, "#123", 0, 0, "Does not match identifier");
            NoMatch(expr, "18니나", 0, 0, "Does not match identifier");
            NoMatch(expr, "１２３", 0, 0, "Does not match identifier");
        }

        [TestMethod]
        public void StringLiteralTest()
        {
            var expr1 = StringLiteral();

            Match(expr1, @"""\a\b\f\n\r\t\v""", 0, 16, "\a\b\f\n\r\t\v");
            Match(expr1, "\"\\\"\\\\\\\"\"", 0, 8, "\"\\\"");
            Match(expr1, @"""\x4a\x4A""", 0, 10, "JJ");
            Match(expr1, @"""\x305f\x305F""", 0, 14, "\x305f\x305F");
            Match(expr1, "\"This is \\na string\\n.\"", 0, 23, "This is \na string\n.");
            NoMatch(expr1, "666", 0, 0, "Does not match a string literal");
        }
    }
}
