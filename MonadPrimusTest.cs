/*
 * This source code is under the Unlicense
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Morilib
{
    [TestClass]
    public class MonadPrimusTest
    {
        [TestMethod]
        public void IdentityToIdentityTest()
        {
            var test = 27.ToIdentity();

            Assert.AreEqual(27, test.Value);
        }

        [TestMethod]
        public void IdentityToEnumerableTest()
        {
            Assert.IsTrue(27.ToIdentity().ToEnumerable().SequenceEqual(new int[] { 27 }));
        }

        [TestMethod]
        public void IdentityEqualsMonadTest()
        {
            var test1 = "27".ToIdentity();
            var test2 = "27".ToIdentity();
            var test3 = "961".ToIdentity();
            var test4 = ((string)null).ToIdentity();

            Assert.IsTrue(test1.MonadEqual(test1));
            Assert.IsTrue(test1.MonadEqual(test2));
            Assert.IsTrue(test2.MonadEqual(test1));
            Assert.IsTrue(test4.MonadEqual(test4));
            Assert.IsFalse(test1.MonadEqual(test3));
            Assert.IsFalse(test1.MonadEqual(test4));
            Assert.IsFalse(test4.MonadEqual(test1));
        }

        [TestMethod]
        public void IdentitySelectTest()
        {
            var test = 876.ToIdentity();
            var res1 = from x in test select x.ToString() + "5";

            Assert.AreEqual("8765", test.Select(x => x.ToString() + "5").Value);
            Assert.AreEqual("8765", res1.Value);
        }

        [TestMethod]
        public void IdentitySelectMany1Test()
        {
            var res1 = 876.ToIdentity().SelectMany(x => (x.ToString() + "5").ToIdentity());

            Assert.AreEqual("8765", res1.Value);
        }

        [TestMethod]
        public void IdentitySelectMany2Test()
        {
            var res1 = from x in 7.ToIdentity()
                       from y in 6.ToIdentity()
                       from z in 5.ToIdentity()
                       select (x - y).ToString() + z.ToString();

            Assert.AreEqual("15", res1.Value);
        }

        [TestMethod]
        public void IdentityMonadRule1Test()
        {
            Func<int, MonadPrimus.Identity<int>> f = x => (x + 111).ToIdentity();
            var res1 = 765.ToIdentity().SelectMany(f);
            var res2 = f(765);

            Assert.AreEqual(res1.Value, res2.Value);
        }

        [TestMethod]
        public void IdentityMonadRule2Test()
        {
            var res1 = 765.ToIdentity().SelectMany(MonadPrimus.ToIdentity);
            var res2 = 765.ToIdentity();

            Assert.AreEqual(res1.Value, res2.Value);
        }

        [TestMethod]
        public void IdentityMonadRule3Test()
        {
            Func<int, MonadPrimus.Identity<int>> f = x => (x + 111).ToIdentity();
            Func<int, MonadPrimus.Identity<int>> g = x => (x - 222).ToIdentity();
            var res1 = 765.ToIdentity().SelectMany(f).SelectMany(g);
            var res2 = 765.ToIdentity().SelectMany(x => f(x).SelectMany(g));

            Assert.AreEqual(res1.Value, res2.Value);
        }

        [TestMethod]
        public void MaybeToMaybeTest()
        {
            var res1 = 27.ToMaybe();
            var res2 = MonadPrimus.Maybe<int>.Nothing;

            Assert.IsTrue(res1.HasValue);
            Assert.AreEqual(27, res1.Value);
            Assert.IsFalse(res2.HasValue);
        }

        [TestMethod]
        public void MaybeToEnumerableTest()
        {
            Assert.IsTrue(27.ToMaybe().ToEnumerable().SequenceEqual(new int[] { 27 }));
            Assert.IsTrue(MonadPrimus.Maybe<int>.Nothing.ToEnumerable().SequenceEqual(new int[0]));
        }

        [TestMethod]
        public void MaybeMonadEqualTest()
        {
            var test1 = "27".ToMaybe();
            var test2 = "27".ToMaybe();
            var test3 = "961".ToMaybe();
            var test4 = MonadPrimus.Maybe<string>.Nothing;
            var test5 = ((string)null).ToMaybe();

            Assert.IsTrue(test1.MonadEqual(test1));
            Assert.IsTrue(test1.MonadEqual(test2));
            Assert.IsTrue(test2.MonadEqual(test1));
            Assert.IsTrue(test4.MonadEqual(test4));
            Assert.IsTrue(test5.MonadEqual(test5));
            Assert.IsFalse(test1.MonadEqual(test3));
            Assert.IsFalse(test1.MonadEqual(test4));
            Assert.IsFalse(test4.MonadEqual(test1));
            Assert.IsFalse(test1.MonadEqual(test5));
            Assert.IsFalse(test5.MonadEqual(test1));
        }

        [TestMethod]
        public void MaybeSelectTest()
        {
            var res1 = 876.ToMaybe().Select(x => x.ToString() + "5");
            var res2 = MonadPrimus.Maybe<int>.Nothing.Select(x => x.ToString() + "5");
            var res3 = from x in 876.ToMaybe() select x.ToString() + "5";
            var res4 = from x in MonadPrimus.Maybe<int>.Nothing select x.ToString() + "5";

            Assert.IsTrue(res1.HasValue);
            Assert.AreEqual("8765", res1.Value);
            Assert.IsFalse(res2.HasValue);
            Assert.IsTrue(res3.HasValue);
            Assert.AreEqual(res3.Value, "8765");
            Assert.IsFalse(res4.HasValue);
        }

        [TestMethod]
        public void MaybeSelectMany1Test()
        {
            var res1 = 876.ToMaybe().SelectMany(x => (x.ToString() + "5").ToMaybe());
            var res2 = MonadPrimus.Maybe<int>.Nothing.SelectMany(x => (x.ToString() + "5").ToMaybe());

            Assert.IsTrue(res1.HasValue);
            Assert.AreEqual("8765",res1.Value);
            Assert.IsFalse(res2.HasValue);
        }

        [TestMethod]
        public void MaybeSelectMany2Test()
        {
            var res1 = from x in 7.ToMaybe()
                       from y in 6.ToMaybe()
                       from z in 5.ToMaybe()
                       select (x - y).ToString() + z.ToString();
            var res2 = from x in MonadPrimus.Maybe<int>.Nothing
                       from y in 6.ToMaybe()
                       from z in 5.ToMaybe()
                       select (x - y).ToString() + z.ToString();
            var res3 = from x in 7.ToMaybe()
                       from y in 6.ToMaybe()
                       from z in MonadPrimus.Maybe<int>.Nothing
                       select (x - y).ToString() + z.ToString();

            Assert.IsTrue(res1.HasValue);
            Assert.AreEqual("15", res1.Value);
            Assert.IsFalse(res2.HasValue);
            Assert.IsFalse(res3.HasValue);
        }

        [TestMethod]
        public void MaybeMonadRule1Test()
        {
            Func<int, MonadPrimus.Maybe<int>> f1 = x => (x + 111).ToMaybe();
            var res11 = 765.ToMaybe().SelectMany(f1);
            var res12 = f1(765);
            Func<int, MonadPrimus.Maybe<int>> f2 = x => MonadPrimus.Maybe<int>.Nothing;
            var res21 = 765.ToMaybe().SelectMany(f2);
            var res22 = f2(765);

            Assert.IsTrue(res11.HasValue);
            Assert.IsTrue(res12.HasValue);
            Assert.AreEqual(res11.Value, res12.Value);
            Assert.IsFalse(res21.HasValue);
            Assert.IsFalse(res22.HasValue);
        }

        [TestMethod]
        public void MaybeMonadRule2Test()
        {
            var res11 = 765.ToMaybe().SelectMany(MonadPrimus.ToMaybe);
            var res12 = 765.ToMaybe();
            var res21 = MonadPrimus.Maybe<int>.Nothing.SelectMany(MonadPrimus.ToMaybe);
            var res22 = MonadPrimus.Maybe<int>.Nothing;

            Assert.IsTrue(res11.HasValue);
            Assert.IsTrue(res12.HasValue);
            Assert.AreEqual(res11.Value, res12.Value);
            Assert.IsFalse(res21.HasValue);
            Assert.IsFalse(res22.HasValue);
        }

        [TestMethod]
        public void MaybeMonadRule3Test()
        {
            Func<int, MonadPrimus.Maybe<int>> f1 = (int x) => (x + 111).ToMaybe();
            Func<int, MonadPrimus.Maybe<int>> g1 = (int x) => (x - 222).ToMaybe();
            var res11 = 765.ToMaybe().SelectMany(f1).SelectMany(g1);
            var res12 = 765.ToMaybe().SelectMany(x => f1(x).SelectMany(g1));
            var res21 = MonadPrimus.Maybe<int>.Nothing.SelectMany(f1).SelectMany(g1);
            var res22 = MonadPrimus.Maybe<int>.Nothing.SelectMany(x => f1(x).SelectMany(g1));
            Func<int, MonadPrimus.Maybe<int>> f3 = (int x) => MonadPrimus.Maybe<int>.Nothing;
            Func<int, MonadPrimus.Maybe<int>> g3 = (int x) => (x - 222).ToMaybe();
            var res31 = 765.ToMaybe().SelectMany(f3).SelectMany(g3);
            var res32 = 765.ToMaybe().SelectMany(x => f3(x).SelectMany(g3));
            Func<int, MonadPrimus.Maybe<int>> f4 = (int x) => (x + 111).ToMaybe();
            Func<int, MonadPrimus.Maybe<int>> g4 = (int x) => MonadPrimus.Maybe<int>.Nothing;
            var res41 = 765.ToMaybe().SelectMany(f4).SelectMany(g4);
            var res42 = 765.ToMaybe().SelectMany(x => f4(x).SelectMany(g4));

            Assert.IsTrue(res11.HasValue);
            Assert.IsTrue(res12.HasValue);
            Assert.AreEqual(res11.Value, res12.Value);
            Assert.IsFalse(res21.HasValue);
            Assert.IsFalse(res22.HasValue);
            Assert.IsFalse(res31.HasValue);
            Assert.IsFalse(res32.HasValue);
            Assert.IsFalse(res41.HasValue);
            Assert.IsFalse(res42.HasValue);
        }

        [TestMethod]
        public void EitherToEitherTest()
        {
            var res1 = 765.ToEither<string, int>();
            var res2 = MonadPrimus.Either<string, int>.ToLeft("961pro");

            Assert.IsTrue(res1.IsRight);
            Assert.AreEqual(765, res1.Right);
            Assert.IsFalse(res2.IsRight);
            Assert.AreEqual("961pro", res2.Left);
        }

        [TestMethod]
        public void EitherMonadEqualTest()
        {
            var test1 = "27".ToEither<string, string>();
            var test2 = "27".ToEither<string, string>();
            var test3 = "961".ToEither<string, string>();
            var test4 = MonadPrimus.Either<string, string>.ToLeft("27");
            var test5 = MonadPrimus.Either<string, string>.ToLeft("27");
            var test6 = ((string)null).ToEither<string, string>();
            var test7 = MonadPrimus.Either<string, string>.ToLeft(null);

            Assert.IsTrue(test1.MonadEqual(test1));
            Assert.IsTrue(test1.MonadEqual(test2));
            Assert.IsTrue(test2.MonadEqual(test1));
            Assert.IsTrue(test4.MonadEqual(test4));
            Assert.IsTrue(test4.MonadEqual(test5));
            Assert.IsTrue(test5.MonadEqual(test4));
            Assert.IsTrue(test6.MonadEqual(test6));
            Assert.IsTrue(test7.MonadEqual(test7));
            Assert.IsFalse(test1.MonadEqual(test3));
            Assert.IsFalse(test1.MonadEqual(test4));
            Assert.IsFalse(test4.MonadEqual(test1));
            Assert.IsFalse(test1.MonadEqual(test6));
            Assert.IsFalse(test6.MonadEqual(test1));
            Assert.IsFalse(test4.MonadEqual(test7));
            Assert.IsFalse(test7.MonadEqual(test4));
            Assert.IsFalse(test1.MonadEqual(test7));
            Assert.IsFalse(test7.MonadEqual(test1));
            Assert.IsFalse(test4.MonadEqual(test6));
            Assert.IsFalse(test6.MonadEqual(test4));
        }

        [TestMethod]
        public void EitherSelectTest()
        {
            var res1 = 876.ToEither<string, int>().Select(x => x.ToString() + "5");
            var res2 = MonadPrimus.Either<string, int>.ToLeft("961pro").Select(x => x.ToString() + "5");
            var res3 = from x in 876.ToEither<string, int>() select x.ToString() + "5";
            var res4 = from x in MonadPrimus.Either<string, int>.ToLeft("961pro") select x.ToString() + "5";

            Assert.IsTrue(res1.IsRight);
            Assert.AreEqual("8765", res1.Right);
            Assert.IsFalse(res2.IsRight);
            Assert.AreEqual("961pro", res2.Left);
            Assert.IsTrue(res3.IsRight);
            Assert.AreEqual("8765", res3.Right);
            Assert.IsFalse(res4.IsRight);
            Assert.AreEqual("961pro", res4.Left);
        }

        [TestMethod]
        public void EitherSelectMany1Test()
        {
            var res1 = 876.ToEither<string, int>().SelectMany(x => (x.ToString() + "5").ToEither<string, string>());
            var res2 = MonadPrimus.Either<string, int>.ToLeft("961pro").SelectMany(x => (x.ToString() + "5").ToEither<string, string>());

            Assert.IsTrue(res1.IsRight);
            Assert.AreEqual("8765", res1.Right);
            Assert.IsFalse(res2.IsRight);
            Assert.AreEqual("961pro", res2.Left);
        }

        [TestMethod]
        public void EitherSelectMany2Test()
        {
            var res1 = from x in 7.ToEither<string, int>()
                       from y in 6.ToEither<string, int>()
                       from z in 5.ToEither<string, int>()
                       select (x - y).ToString() + z.ToString();
            var res2 = from x in MonadPrimus.Either<string, int>.ToLeft("961pro")
                       from y in 6.ToEither<string, int>()
                       from z in 5.ToEither<string, int>()
                       select (x - y).ToString() + z.ToString();
            var res3 = from x in 7.ToEither<string, int>()
                       from y in 6.ToEither<string, int>()
                       from z in MonadPrimus.Either<string, int>.ToLeft("961pro")
                       select (x - y).ToString() + z.ToString();

            Assert.IsTrue(res1.IsRight);
            Assert.AreEqual("15", res1.Right);
            Assert.IsFalse(res2.IsRight);
            Assert.AreEqual("961pro", res2.Left);
            Assert.IsFalse(res3.IsRight);
            Assert.AreEqual("961pro", res3.Left);
        }

        [TestMethod]
        public void EitherMonadRule1Test()
        {
            Func<int, MonadPrimus.Either<string, int>> f1 = x => (x + 111).ToEither<string, int>();
            var res11 = 765.ToEither<string, int>().SelectMany(f1);
            var res12 = f1(765);
            Func<int, MonadPrimus.Either<string, int>> f2 = x => MonadPrimus.Either<string, int>.ToLeft("961pro");
            var res21 = 765.ToEither<string, int>().SelectMany(f2);
            var res22 = f2(765);

            Assert.IsTrue(res11.IsRight);
            Assert.IsTrue(res12.IsRight);
            Assert.AreEqual(res11.Right, res12.Right);
            Assert.IsFalse(res21.IsRight);
            Assert.IsFalse(res22.IsRight);
            Assert.AreEqual(res21.Left, res22.Left);
        }

        [TestMethod]
        public void EitherMonadRule2Test()
        {
            var res11 = 765.ToEither<string, int>().SelectMany(MonadPrimus.ToEither<string, int>);
            var res12 = 765.ToEither<string, int>();
            var res21 = MonadPrimus.Either<string, int>.ToLeft("961pro").SelectMany(MonadPrimus.ToEither<string, int>);
            var res22 = MonadPrimus.Either<string, int>.ToLeft("961pro");

            Assert.IsTrue(res11.IsRight);
            Assert.IsTrue(res12.IsRight);
            Assert.AreEqual(res11.Right, res12.Right);
            Assert.IsFalse(res21.IsRight);
            Assert.IsFalse(res22.IsRight);
            Assert.AreEqual(res21.Left, res22.Left);
        }

        [TestMethod]
        public void EitherMonadRule3Test()
        {
            Func<int, MonadPrimus.Either<string, int>> f1 = (int x) => (x + 111).ToEither<string, int>();
            Func<int, MonadPrimus.Either<string, int>> g1 = (int x) => (x - 222).ToEither<string, int>();
            var res11 = 765.ToEither<string, int>().SelectMany(f1).SelectMany(g1);
            var res12 = 765.ToEither<string, int>().SelectMany(x => f1(x).SelectMany(g1));
            var res21 = MonadPrimus.Either<string, int>.ToLeft("961pro").SelectMany(f1).SelectMany(g1);
            var res22 = MonadPrimus.Either<string, int>.ToLeft("961pro").SelectMany(x => f1(x).SelectMany(g1));
            Func<int, MonadPrimus.Either<string, int>> f3 = (int x) => MonadPrimus.Either<string, int>.ToLeft("961pro");
            Func<int, MonadPrimus.Either<string, int>> g3 = (int x) => (x - 222).ToEither<string, int>();
            var res31 = 765.ToEither<string, int>().SelectMany(f3).SelectMany(g3);
            var res32 = 765.ToEither<string, int>().SelectMany(x => f3(x).SelectMany(g3));
            Func<int, MonadPrimus.Either<string, int>> f4 = (int x) => (x + 111).ToEither<string, int>();
            Func<int, MonadPrimus.Either<string, int>> g4 = (int x) => MonadPrimus.Either<string, int>.ToLeft("961pro");
            var res41 = 765.ToEither<string, int>().SelectMany(f4).SelectMany(g4);
            var res42 = 765.ToEither<string, int>().SelectMany(x => f4(x).SelectMany(g4));

            Assert.IsTrue(res11.IsRight);
            Assert.IsTrue(res12.IsRight);
            Assert.AreEqual(res11.Right, res12.Right);
            Assert.IsFalse(res21.IsRight);
            Assert.IsFalse(res22.IsRight);
            Assert.AreEqual(res21.Left, res22.Left);
            Assert.IsFalse(res31.IsRight);
            Assert.IsFalse(res32.IsRight);
            Assert.AreEqual(res31.Left, res32.Left);
            Assert.IsFalse(res41.IsRight);
            Assert.IsFalse(res42.IsRight);
            Assert.AreEqual(res41.Left, res42.Left);
        }

        [TestMethod]
        public void StateToStateTest()
        {
            var m11 = 765.ToState<string, int>();
            var res11 = m11("s1");

            Assert.AreEqual("s1", res11.State);
            Assert.AreEqual(765, res11.Value);
        }

        [TestMethod]
        public void StatePutTest()
        {
            var m11 = MonadPrimus.Put<string, int>("s1");
            var res11 = m11("s2");

            Assert.AreEqual("s1", res11.State);
            Assert.AreEqual(default(int), res11.Value);
        }

        [TestMethod]
        public void StateModifyTest()
        {
            var m11 = MonadPrimus.Modify<string, int>(x => x + "1");
            var res11 = m11("s1");

            Assert.AreEqual("s11", res11.State);
            Assert.AreEqual(default(int), res11.Value);
        }

        [TestMethod]
        public void StateRunStateTest()
        {
            var m11 = new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + 1, x.ToString() + "1"));
            var res11 = m11("1");
            var res12 = m11.EvalState("1");
            var res13 = m11.ExecState("1");

            Assert.AreEqual(2, res11.Value);
            Assert.AreEqual("11", res11.State);
            Assert.AreEqual(2, res12);
            Assert.AreEqual("11", res13);
        }

        [TestMethod]
        public void StateSelectTest()
        {
            var m11 = new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + 1, x.ToString() + "1"));
            var m12 = m11.Select(x => x.ToString() + "2");
            var res11 = m12("1");

            Assert.AreEqual("22", res11.Value);
            Assert.AreEqual("11", res11.State);
        }

        [TestMethod]
        public void StateSelectManyTest01()
        {
            var m11 = new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + 1, x.ToString() + "1"));
            var m12 = m11.SelectMany(x => new MonadPrimus.State<string, string>(s => new MonadPrimus.StateTuple<string, string>(x + s, s + x)));
            var res11 = m12("1");

            Assert.AreEqual("211", res11.Value);
            Assert.AreEqual("112", res11.State);
        }

        [TestMethod]
        public void StateSelectManyTest02()
        {
            var m11 = from x in new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + 1, x.ToString() + "1"))
                      from y in new MonadPrimus.State<string, string>(s => new MonadPrimus.StateTuple<string, string>(x + s, s + x))
                      select x + y;
            var res11 = m11("1");

            Assert.AreEqual("2211", res11.Value);
            Assert.AreEqual("112", res11.State);
        }

        [TestMethod]
        public void StateMonadRule1Test()
        {
            Func<int, MonadPrimus.State<string, int>> f1 =
                y => new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + y, x.ToString() + y));
            var m11 = 765.ToState<string, int>().SelectMany(f1);
            var m12 = f1(765);
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual(766, res11.Value);
            Assert.AreEqual("1765", res11.State);
            Assert.AreEqual(766, res12.Value);
            Assert.AreEqual("1765", res12.State);
        }

        [TestMethod]
        public void StateMonadRule2Test()
        {
            var m00 = new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + 1, x.ToString() + "1"));
            var m11 = m00.SelectMany(MonadPrimus.ToState<string, int>);
            var m12 = m00;
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual(2, res11.Value);
            Assert.AreEqual("11", res11.State);
            Assert.AreEqual(2, res12.Value);
            Assert.AreEqual("11", res12.State);
        }

        [TestMethod]
        public void StateMonadRule3Test()
        {
            Func<int, MonadPrimus.State<string, int>> f1 =
                y => new MonadPrimus.State<string, int>(x => new MonadPrimus.StateTuple<int, string>(int.Parse(x) + y, x.ToString() + y));
            Func<int, MonadPrimus.State<string, string>> g1 =
                x => new MonadPrimus.State<string, string>(s => new MonadPrimus.StateTuple<string, string>(x + s, s + x));
            var m11 = 765.ToState<string, int>().SelectMany(f1).SelectMany(g1);
            var m12 = 765.ToState<string, int>().SelectMany(x => f1(x).SelectMany(g1));
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual("7661765", res11.Value);
            Assert.AreEqual("1765766", res11.State);
            Assert.AreEqual("7661765", res12.Value);
            Assert.AreEqual("1765766", res12.State);
        }

        [TestMethod]
        public void ReaderLocalTest()
        {
            var m11 = new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1);
            var m12 = MonadPrimus.Local(x => x + x, m11);
            var res11 = m12("1");

            Assert.AreEqual(12, res11);
        }

        [TestMethod]
        public void ReaderToReaderTest()
        {
            var m11 = 765.ToReader<string, int>();
            var res11 = m11("s1");

            Assert.AreEqual(res11, 765);
        }

        [TestMethod]
        public void ReaderSelectTest()
        {
            var m11 = new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1);
            var m12 = m11.Select(x => x.ToString() + "2");
            var res11 = m12("1");

            Assert.AreEqual("22", res11);
        }

        [TestMethod]
        public void ReaderSelectManyTest01()
        {
            var m11 = new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1);
            var m12 = m11.SelectMany(x => new MonadPrimus.Reader<string, string>(s => x + s));
            var res11 = m12("1");

            Assert.AreEqual("21", res11);
        }

        [TestMethod]
        public void ReaderSelectManyTest02()
        {
            var m11 = from x in new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1)
                      from y in new MonadPrimus.Reader<string, string>(s => x + s)
                      select x + y;
            var res11 = m11("1");

            Assert.AreEqual("221", res11);
        }

        [TestMethod]
        public void ReaderMonadRule1Test()
        {
            Func<int, MonadPrimus.Reader<string, int>> f1 = y => new MonadPrimus.Reader<string, int>(x => int.Parse(x) + y);
            var m11 = 765.ToReader<string, int>().SelectMany(f1);
            var m12 = f1(765);
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual(766, res11);
            Assert.AreEqual(766, res12);
        }

        [TestMethod]
        public void ReaderMonadRule2Test()
        {
            var m00 = new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1);
            var m11 = m00.SelectMany(MonadPrimus.ToReader<string, int>);
            var m12 = m00;
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual(2, res11);
            Assert.AreEqual(2, res12);
        }

        [TestMethod]
        public void ReaderMonadRule3Test()
        {
            Func<int, MonadPrimus.Reader<string, int>> f1 = y => new MonadPrimus.Reader<string, int>(x => int.Parse(x) + y);
            Func<int, MonadPrimus.Reader<string, string>> g1 = x => new MonadPrimus.Reader<string, string>(s => x + s);
            var m11 = 765.ToReader<string, int>().SelectMany(f1).SelectMany(g1);
            var m12 = 765.ToReader<string, int>().SelectMany(x => f1(x).SelectMany(g1));
            var res11 = m11("1");
            var res12 = m12("1");

            Assert.AreEqual("7661", res11);
            Assert.AreEqual("7661", res12);
        }

        [TestMethod]
        public void WriterSelectTest()
        {
            var m11 = MonadPrimus.Create(1, new string[] { "init1", "init2" });
            var m12 = m11.Select(x => x.ToString() + "2");
            var res11 = m12();

            Assert.AreEqual("12", res11.Value);
            Assert.AreEqual("init1", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual(2, res11.State.Count());
        }

        [TestMethod]
        public void WriterSelectManyTest01()
        {
            var m11 = MonadPrimus.Create(1, new string[] { "init1", "init2" });
            var m12 = m11.SelectMany(x => MonadPrimus.Create(x + "1", new string[] { "next" }));
            var res11 = m12();

            Assert.AreEqual("11", res11.Value);
            Assert.AreEqual("init1", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual("next", res11.State.ElementAt(2));
            Assert.AreEqual(3, res11.State.Count());
        }

        [TestMethod]
        public void WriterSelectManyTest02()
        {
            var m11 = from x in MonadPrimus.Create(1, new string[] { "init1", "init2" })
                      from y in MonadPrimus.Create(x + "1", new string[] { "next" })
                      select x + y;
            var res11 = m11();

            Assert.AreEqual("111", res11.Value);
            Assert.AreEqual("init1", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual("next", res11.State.ElementAt(2));
            Assert.AreEqual(3, res11.State.Count());
        }

        [TestMethod]
        public void WriterMonadRule1Test()
        {
            Func<int, MonadPrimus.Writer<string, int>> f1 = y => MonadPrimus.Create(y, new string[] { y.ToString(), "init2" });
            var m11 = 765.ToWriter<string, int>().SelectMany(f1);
            var m12 = f1(765);
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual(765, res11.Value);
            Assert.AreEqual("765", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual(2, res11.State.Count());
            Assert.AreEqual(765, res12.Value);
            Assert.AreEqual("765", res12.State.ElementAt(0));
            Assert.AreEqual("init2", res12.State.ElementAt(1));
            Assert.AreEqual(2, res12.State.Count());
        }

        [TestMethod]
        public void WriterMonadRule2Test()
        {
            var m00 = MonadPrimus.Create(1, new string[] { "init1", "init2" });
            var m11 = m00.SelectMany(MonadPrimus.ToWriter<string, int>);
            var m12 = m00;
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual(1, res11.Value);
            Assert.AreEqual("init1", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual(2, res11.State.Count());
            Assert.AreEqual(1, res12.Value);
            Assert.AreEqual("init1", res12.State.ElementAt(0));
            Assert.AreEqual("init2", res12.State.ElementAt(1));
            Assert.AreEqual(2, res12.State.Count());
        }

        [TestMethod]
        public void WriterMonadRule3Test()
        {
            Func<int, MonadPrimus.Writer<string, int>> f1 = y => MonadPrimus.Create(y, new string[] { y.ToString(), "init2" });
            Func<int, MonadPrimus.Writer<string, string>> g1 = x => MonadPrimus.Create(x + "1", new string[] { x.ToString() });
            var m11 = 765.ToWriter<string, int>().SelectMany(f1).SelectMany(g1);
            var m12 = 765.ToWriter<string, int>().SelectMany(x => f1(x).SelectMany(g1));
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual("7651", res11.Value);
            Assert.AreEqual("765", res11.State.ElementAt(0));
            Assert.AreEqual("init2", res11.State.ElementAt(1));
            Assert.AreEqual("765", res11.State.ElementAt(2));
            Assert.AreEqual(3, res11.State.Count());
            Assert.AreEqual("7651", res12.Value);
            Assert.AreEqual("765", res12.State.ElementAt(0));
            Assert.AreEqual("init2", res12.State.ElementAt(1));
            Assert.AreEqual("765", res12.State.ElementAt(2));
            Assert.AreEqual(3, res12.State.Count());
        }

        [TestMethod]
        public void WriterStringSelectTest()
        {
            var m11 = MonadPrimus.Create(1, "init1");
            var m12 = m11.Select(x => x.ToString() + "2");
            var res11 = m12();

            Assert.AreEqual("12", res11.Value);
            Assert.AreEqual("init1", res11.State);
        }

        [TestMethod]
        public void WriterStringSelectManyTest01()
        {
            var m11 = MonadPrimus.Create(1, "init1");
            var m12 = m11.SelectMany(x => MonadPrimus.Create(x + "1", "next"));
            var res11 = m12();

            Assert.AreEqual("11", res11.Value);
            Assert.AreEqual("init1next", res11.State);
        }

        [TestMethod]
        public void WriterStringSelectManyTest02()
        {
            var m11 = from x in MonadPrimus.Create(1, "init1")
                      from y in MonadPrimus.Create(x + "1", "next")
                      select x + y;
            var res11 = m11();

            Assert.AreEqual("111", res11.Value);
            Assert.AreEqual("init1next", res11.State);
        }

        [TestMethod]
        public void WriterStringMonadRule1Test()
        {
            Func<int, MonadPrimus.Writer<int>> f1 = y => MonadPrimus.Create(y, y.ToString());
            var m11 = 765.ToWriter().SelectMany(f1);
            var m12 = f1(765);
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual(765, res11.Value);
            Assert.AreEqual("765", res11.State);
            Assert.AreEqual(765, res12.Value);
            Assert.AreEqual("765", res12.State);
        }

        [TestMethod]
        public void WriterStringMonadRule2Test()
        {
            var m00 = MonadPrimus.Create(1, "init1");
            var m11 = m00.SelectMany(MonadPrimus.ToWriter);
            var m12 = m00;
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual(1, res11.Value);
            Assert.AreEqual("init1", res11.State);
            Assert.AreEqual(1, res12.Value);
            Assert.AreEqual("init1", res12.State);
        }

        [TestMethod]
        public void WriterStringMonadRule3Test()
        {
            Func<int, MonadPrimus.Writer<int>> f1 = y => MonadPrimus.Create(y, y.ToString());
            Func<int, MonadPrimus.Writer<string>> g1 = x => MonadPrimus.Create(x + "1", x.ToString());
            var m11 = 765.ToWriter().SelectMany(f1).SelectMany(g1);
            var m12 = 765.ToWriter().SelectMany(x => f1(x).SelectMany(g1));
            var res11 = m11();
            var res12 = m12();

            Assert.AreEqual("7651", res11.Value);
            Assert.AreEqual("765765", res11.State);
            Assert.AreEqual("7651", res12.Value);
            Assert.AreEqual("765765", res12.State);
        }

        [TestMethod]
        public void ContSelect01Test()
        {
            MonadPrimus.Cont<int, int> m01 = x => x(765);
            var m11 = m01.Select(x => x + 100);

            Assert.AreEqual(876, m11(x => x + 11));
        }

        [TestMethod]
        public void ContSelect02Test()
        {
            var m11 = from x in 765.ToCont<int, int>()
                      select x + 100;

            Assert.AreEqual(876, m11(x => x + 11));
        }

        [TestMethod]
        public void ContSelectMany01Test()
        {
            MonadPrimus.Cont<int, int> m01 = x => x(765);
            var m11 = m01.SelectMany<int, int, int>(x => y => y(x + 100));

            Assert.AreEqual(876, m11(x => x + 11));
        }

        [TestMethod]
        public void ContSelectMany02Test()
        {
            var m11 = from x in 765.ToCont<int, int>()
                      from y in 100.ToCont<int, int>()
                      select x + y;

            Assert.AreEqual(876, m11(x => x + 11));
        }

        [TestMethod]
        public void ContCallCC01Test()
        {
            var m11 = MonadPrimus.CallCC<int, int>(
                exit => from a in 1.ToCont<int, int>()
                        from b in 7.ToCont<int, int>()
                        from c in a + b < 10 ? exit(961) : 6.ToCont<int, int>()
                        from d in 5.ToCont<int, int>()
                        select a + b + c + d);
            var m12 = MonadPrimus.CallCC<int, int>(
                exit => from a in 8.ToCont<int, int>()
                        from b in 7.ToCont<int, int>()
                        from c in a + b < 10 ? exit(961) : 6.ToCont<int, int>()
                        from d in 5.ToCont<int, int>()
                        select a + b + c + d);

            Assert.AreEqual(961, m11(x => x));
            Assert.AreEqual(26, m12(x => x));
        }

        [TestMethod]
        public void ContMonadRule1Test()
        {
            Func<int, MonadPrimus.Cont<int, int>> f1 = y => x => x(y + 100);
            var m11 = 765.ToCont<int, int>().SelectMany(f1);
            var m12 = f1(765);
            var res11 = m11(x => x + 11);
            var res12 = m12(x => x + 11);

            Assert.AreEqual(876, res11);
            Assert.AreEqual(876, res12);
        }

        [TestMethod]
        public void ContMonadRule2Test()
        {
            MonadPrimus.Cont<int, int> m00 = x => x(765);
            var m11 = m00.SelectMany(MonadPrimus.ToCont<int, int>);
            var m12 = m00;
            var res11 = m11(x => x + 111);
            var res12 = m12(x => x + 111);

            Assert.AreEqual(876, res11);
            Assert.AreEqual(876, res12);
        }

        [TestMethod]
        public void ContMonadRule3Test()
        {
            Func<int, MonadPrimus.Cont<int, int>> f1 = y => x => x(y + 100);
            Func<int, MonadPrimus.Cont<int, int>> g1 = y => x => x(y + 10);
            var m11 = 765.ToCont<int, int>().SelectMany(f1).SelectMany(g1);
            var m12 = 765.ToCont<int, int>().SelectMany(x => f1(x).SelectMany(g1));
            var res11 = m11(x => x + 1);
            var res12 = m12(x => x + 1);

            Assert.AreEqual(876, res11);
            Assert.AreEqual(876, res12);
        }
    }
}
