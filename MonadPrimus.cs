/*
 * This source code is under the Unlicense
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morilib
{
    public static partial class MonadPrimus
    {
        /// <summary>
        /// An instance of continuation monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="Ans">result type</typeparam>
        /// <param name="k">continuation</param>
        /// <returns>answer</returns>
        public delegate Ans Cont<T, Ans>(Func<T, Ans> k);

        /// <summary>
        /// A class of identity monad.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        public class Identity<T>
        {
            /// <summary>
            /// A wrapped value.
            /// </summary>
            public T Value { get; private set; }

            /// <summary>
            /// constructs a identity monad.
            /// </summary>
            /// <param name="value">value to wrap</param>
            public Identity(T value)
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// An unit function of identity monad.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit identity monad</returns>
        public static Identity<T> ToIdentity<T>(this T value)
        {
            return new Identity<T>(value);
        }

        /// <summary>
        /// maps the identity monad with the given function
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <param name="m">monad to map</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Identity<U> Select<T, U>(this Identity<T> m, Func<T, U> k)
        {
            return k(m.Value).ToIdentity();
        }

        /// <summary>
        /// A bind function of identity monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <param name="id">monad</param>
        /// <param name="k">function to bind</param>
        /// <returns>bound monad</returns>
        public static Identity<U> SelectMany<T, U>(this Identity<T> id, Func<T, Identity<U>> k)
        {
            return k(id.Value);
        }

        /// <summary>
        /// A bind function of identity monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <typeparam name="V">mapped type</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">function to bind</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Identity<V> SelectMany<T, U, V>(this Identity<T> m, Func<T, Identity<U>> k, Func<T, U, V> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToIdentity()));
        }

        /// <summary>
        /// A class of maybe monad.
        /// </summary>
        /// <typeparam name="T">type to wrap</typeparam>
        public class Maybe<T>
        {
            /// <summary>
            /// An instance of empty maybe monad.
            /// </summary>
            public static readonly Maybe<T> Nothing = new Maybe<T>();

            private readonly T _value;

            /// <summary>
            /// A wrapped value.
            /// If the monad has no values, this property throws the InvalidOperationException.
            /// </summary>
            public T Value
            {
                get
                {
                    if (!HasValue)
                    {
                        throw new InvalidOperationException("This has not value");
                    }
                    return _value;
                }
            }

            /// <summary>
            /// True if the monad has a value.
            /// </summary>
            public bool HasValue { get; private set; }

            private Maybe()
            {
                this.HasValue = false;
            }

            /// <summary>
            /// A constructor of maybe monad.
            /// </summary>
            /// <param name="value">value to wrap</param>
            public Maybe(T value)
            {
                this.HasValue = true;
                this._value = value;
            }
        }

        /// <summary>
        /// An unit function of maybe monad.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit maybe monad</returns>
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return new Maybe<T>(value);
        }

        /// <summary>
        /// returns the given monad if result of value applied to predicate function is true.
        /// otherwise returns Nothing.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="m">monad to test</param>
        /// <param name="pred">predicate function</param>
        /// <returns>the given monad or Nothing</returns>
        public static Maybe<T> Where<T>(this Maybe<T> m, Predicate<T> pred)
        {
            return !m.HasValue || pred(m.Value) ? m : Maybe<T>.Nothing;
        }

        /// <summary>
        /// maps the maybe monad with the given function
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <param name="m">monad to map</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Maybe<U> Select<T, U>(this Maybe<T> m, Func<T, U> k)
        {
            return m.HasValue ? k(m.Value).ToMaybe() : Maybe<U>.Nothing;
        }

        /// <summary>
        /// A bind function of maybe monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">function to bind</param>
        /// <returns>bound monad</returns>
        public static Maybe<U> SelectMany<T, U>(this Maybe<T> m, Func<T, Maybe<U>> k)
        {
            return m.HasValue ? k(m.Value) : Maybe<U>.Nothing;
        }

        /// <summary>
        /// A bind function of maybe monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <typeparam name="V">mapped type</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">function to bind</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Maybe<V> SelectMany<T, U, V>(this Maybe<T> m, Func<T, Maybe<U>> k, Func<T, U, V> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToMaybe()));
        }

        /// <summary>
        /// An unit function of list monad (LINQ to objects).
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit list monad</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T value)
        {
            yield return value;
        }

        /// <summary>
        /// A class of either monad.
        /// </summary>
        /// <typeparam name="L">type of left value</typeparam>
        /// <typeparam name="R">type of right value</typeparam>
        public class Either<L, R>
        {
            private readonly R _right;
            private readonly L _left;

            /// <summary>
            /// The right value of this monad.
            /// If the monad has no right values, this property throws the InvalidOperationException.
            /// </summary>
            public R Right
            {
                get
                {
                    if (!IsRight)
                    {
                        throw new InvalidOperationException("This has not right value");
                    }
                    return _right;
                }
            }

            /// <summary>
            /// The left value of this monad.
            /// If the monad has no left values, this property throws the InvalidOperationException.
            /// </summary>
            public L Left
            {
                get
                {
                    if (IsRight)
                    {
                        throw new InvalidOperationException("This has not left value");
                    }
                    return _left;
                }
            }

            /// <summary>
            /// True if the monad has right value.
            /// </summary>
            public bool IsRight { get; private set; }

            private Either(bool isRight, L left, R right)
            {
                this.IsRight = isRight;
                if(isRight)
                {
                    this._right = right;
                }
                else
                {
                    this._left = left;
                }
            }

            /// <summary>
            /// creates a monad which has the right value.
            /// </summary>
            /// <param name="value">right value</param>
            /// <returns>monad</returns>
            public static Either<L, R> ToRight(R value)
            {
                return new Either<L, R>(true, default(L), value);
            }

            /// <summary>
            /// creates a monad which has the left value.
            /// </summary>
            /// <param name="value">left value</param>
            /// <returns>monad</returns>
            public static Either<L, R> ToLeft(L value)
            {
                return new Either<L, R>(false, value, default(R));
            }

            /// <summary>
            /// If this monad has a right value, returns this monad.
            /// Otherwise returns the given monad.
            /// </summary>
            /// <param name="m">monad</param>
            public Either<L, R> Or(Either<L, R> m)
            {
                return IsRight ? this : m;
            }
        }

        /// <summary>
        /// An unit function of either monad.
        /// </summary>
        /// <typeparam name="L">type of left value</typeparam>
        /// <typeparam name="R">type of right value</typeparam>
        /// <param name="value">right value</param>
        /// <returns>unit either monad</returns>
        public static Either<L, R> ToEither<L, R>(this R value)
        {
            return Either<L, R>.ToRight(value);
        }

        /// <summary>
        /// maps the either monad with the given function
        /// </summary>
        /// <typeparam name="L">type of left value</typeparam>
        /// <typeparam name="R1">type of input right value</typeparam>
        /// <typeparam name="R2">type of output right value</typeparam>
        /// <param name="m">monad to map</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Either<L, R2> Select<L, R1, R2>(this Either<L, R1> m, Func<R1, R2> k)
        {
            if (m.IsRight)
            {
                return k(m.Right).ToEither<L, R2>();
            }
            else
            {
                return Either<L, R2>.ToLeft(m.Left);
            }
        }

        /// <summary>
        /// A bind function of either monad.
        /// </summary>
        /// <typeparam name="L">type of left value</typeparam>
        /// <typeparam name="R1">type of input right value</typeparam>
        /// <typeparam name="R2">type of output right value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">function to bind</param>
        /// <returns>bound monad</returns>
        public static Either<L, R2> SelectMany<L, R1, R2>(this Either<L, R1> m, Func<R1, Either<L, R2>> k)
        {
            if(!m.IsRight)
            {
                return Either<L, R2>.ToLeft(m.Left);
            }
            return k(m.Right);
        }

        /// <summary>
        /// A bind function of either monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="L">type of left value</typeparam>
        /// <typeparam name="R1">type of input right value</typeparam>
        /// <typeparam name="R2">type of output right value</typeparam>
        /// <typeparam name="R3">mapped type of right value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">function to bind</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Either<L, R3> SelectMany<L, R1, R2, R3>(this Either<L, R1> m, Func<R1, Either<L, R2>> k, Func<R1, R2, R3> s)
        {
            if (!m.IsRight)
            {
                return Either<L, R3>.ToLeft(m.Left);
            }
            var m2 = k(m.Right);
            if (!m2.IsRight)
            {
                return Either<L, R3>.ToLeft(m2.Left);
            }
            else
            {
                return s(m.Right, m2.Right).ToEither<L, R3>();
            }
        }

        /// <summary>
        /// A result tuple of state monad.
        /// </summary>
        /// <typeparam name="A">type of value</typeparam>
        /// <typeparam name="S">type of state</typeparam>
        public class StateTuple<A, S>
        {
            /// <summary>
            /// A state.
            /// </summary>
            public S State { get; private set; }

            /// <summary>
            /// A value.
            /// </summary>
            public A Value { get; private set; }

            /// <summary>
            /// constructs the result tuple of state monad.
            /// </summary>
            /// <param name="value">value</param>
            /// <param name="state">state</param>
            public StateTuple(A value, S state)
            {
                this.State = state;
                this.Value = value;
            }

            /// <summary>
            /// A string representation of the tuple.
            /// </summary>
            /// <returns>string representation</returns>
            public override string ToString()
            {
                return "(" + Value.ToString() + ", " + State.ToString() + ")";
            }
        }

        /// <summary>
        /// A class of state monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        public class State<S, A>
        {
            private Func<S, StateTuple<A, S>> Func { get; set; }

            /// <summary>
            /// gets the state as value from this monad.
            /// </summary>
            public static readonly State<S, S> Get = new State<S, S>(s => new StateTuple<S, S>(s, s));

            /// <summary>
            /// puts the state to this monad.
            /// </summary>
            /// <param name="x">state to put</param>
            /// <returns>new monad</returns>
            public static State<S, A> Put(S x)
            {
                return new State<S, A>(z => new StateTuple<A, S>(default(A), x));
            }

            /// <summary>
            /// modifies the state by the given function.
            /// </summary>
            /// <param name="f">modify function</param>
            /// <returns>new monad</returns>
            public static State<S, A> Modify(Func<S, S> f)
            {
                return new State<S, A>(z => new StateTuple<A, S>(default(A), f(z)));
            }

            /// <summary>
            /// constructs a state monad.
            /// </summary>
            /// <param name="func">state function to wrap</param>
            public State(Func<S, StateTuple<A, S>> func)
            {
                this.Func = func;
            }

            /// <summary>
            /// runs this monad by the given initial state.
            /// </summary>
            /// <param name="s">initial state</param>
            /// <returns>result tuple</returns>
            public StateTuple<A, S> RunState(S s)
            {
                return Func(s);
            }

            /// <summary>
            /// runs this monad by the given initial state and gets the result value.
            /// </summary>
            /// <param name="s">initial state</param>
            /// <returns>result value</returns>
            public A EvalState(S s)
            {
                return RunState(s).Value;
            }

            /// <summary>
            /// runs this monad by the given initial state and gets the result state
            /// </summary>
            /// <param name="s">initial state</param>
            /// <returns>result state</returns>
            public S ExecState(S s)
            {
                return RunState(s).State;
            }
        }

        /// <summary>
        /// An unit function of state monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit state monad</returns>
        public static State<S, A> ToState<S, A>(this A value)
        {
            return new State<S, A>(s => new StateTuple<A, S>(value, s));
        }

        /// <summary>
        /// maps the state monad with the given function
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of mapped value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static State<S, B> Select<S, A, B>(this State<S, A> m, Func<A, B> k)
        {
            return new State<S, B>(s =>
            {
                var s0 = m.RunState(s);
                return new StateTuple<B, S>(k(s0.Value), s0.State);
            });
        }

        /// <summary>
        /// A bind function of state monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <param name="a">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <returns>bound monad</returns>
        public static State<S, B> SelectMany<S, A, B>(this State<S, A> a, Func<A, State<S, B>> k)
        {
            return new State<S, B>(s0 =>
            {
                var s1 = a.RunState(s0);
                var s2 = k(a.RunState(s0).Value).RunState(s1.State);
                return s2;
            });
        }

        /// <summary>
        /// A bind function of state monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <typeparam name="C">type of mapped value</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static State<S, C> SelectMany<S, A, B, C>(this State<S, A> m, Func<A, State<S, B>> k, Func<A, B, C> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToState<S, C>()));
        }

        /// <summary>
        /// A class of reader monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        public class Reader<S, A>
        {
            private Func<S, A> Func { get; set; }

            /// <summary>
            /// gets the state of this monad.
            /// </summary>
            public static readonly Reader<S, S> Ask = new Reader<S, S>(s => s);

            /// <summary>
            /// creates the local context of the reader monad.
            /// </summary>
            /// <param name="f">local state modify function</param>
            /// <param name="reader">reader monad</param>
            /// <returns>new reader monad</returns>
            public static Reader<S, A> Local(Func<S, S> f, Reader<S, A> reader)
            {
                return new Reader<S, A>(s => reader.RunReader(f(s)));
            }

            /// <summary>
            /// creates the local context of the reader monad.
            /// result is curried.
            /// </summary>
            /// <param name="f">local state modify function</param>
            /// <param name="reader">reader monad</param>
            /// <returns>curried function</returns>
            public static Func<Reader<S, A>, Reader<S, A>> Local(Func<S, S> f)
            {
                Func<Reader<S, A>, Reader<S, A>> res;

                res = r => new Reader<S, A>(s => r.RunReader(f(s)));
                return res;
            }

            /// <summary>
            /// constructs a reader monad.
            /// </summary>
            /// <param name="func">state function to wrap</param>
            public Reader(Func<S, A> func)
            {
                this.Func = func;
            }

            /// <summary>
            /// runs this monad by the given initial state.
            /// </summary>
            /// <param name="s">initial state</param>
            /// <returns>result value</returns>
            public A RunReader(S s)
            {
                return Func(s);
            }
        }

        /// <summary>
        /// An unit function of reader monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit reader monad</returns>
        public static Reader<S, A> ToReader<S, A>(this A value)
        {
            return new Reader<S, A>(s => value);
        }

        /// <summary>
        /// maps the reader monad with the given function.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of mapped value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Reader<S, B> Select<S, A, B>(this Reader<S, A> m, Func<A, B> k)
        {
            return new Reader<S, B>(s => k(m.RunReader(s)));
        }

        /// <summary>
        /// A bind function of reader monad.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <param name="a">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <returns>bound monad</returns>
        public static Reader<S, B> SelectMany<S, A, B>(this Reader<S, A> a, Func<A, Reader<S, B>> b)
        {
            return new Reader<S, B>(r => b(a.RunReader(r)).RunReader(r));
        }

        /// <summary>
        /// A bind function of reader monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="S">type of state</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <typeparam name="C">type of mapped value</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Reader<S, C> SelectMany<S, A, B, C>(this Reader<S, A> m, Func<A, Reader<S, B>> k, Func<A, B, C> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToReader<S, C>()));
        }

        /// <summary>
        /// A result tuple of writer monad.
        /// </summary>
        /// <typeparam name="A">type of value</typeparam>
        /// <typeparam name="W">type of message</typeparam>
        public class WriterTuple<A, W>
        {
            /// <summary>
            /// An Enumerable of state.
            /// </summary>
            public IEnumerable<W> State { get; private set; }

            /// <summary>
            /// A value.
            /// </summary>
            public A Value { get; private set; }

            /// <summary>
            /// constructs a result type of writer monad.
            /// </summary>
            /// <param name="value">value</param>
            /// <param name="state">messages</param>
            public WriterTuple(A value, IEnumerable<W> state)
            {
                this.State = state;
                this.Value = value;
            }

            /// <summary>
            /// A string representation of this tuple.
            /// </summary>
            /// <returns>string representation</returns>
            public override string ToString()
            {
                return "(" + Value.ToString() + ", " + State.ToString() + ")";
            }
        }

        /// <summary>
        /// A class of writer monad.
        /// </summary>
        /// <typeparam name="W">type of message</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        public class Writer<W, A>
        {
            private IEnumerable<W> State { get; set; }
            private A Value { get; set; }

            /// <summary>
            /// appends the given message to this monad.
            /// </summary>
            /// <param name="w">message</param>
            /// <returns>new monad</returns>
            public static Writer<W, A> Tell(W w)
            {
                return new Writer<W, A>(default(A), new W[] { w });
            }

            /// <summary>
            /// appends the given messages to this monad.
            /// </summary>
            /// <param name="w">messages</param>
            /// <returns>new monad</returns>
            public static Writer<W, A> Tell(IEnumerable<W> w)
            {
                return new Writer<W, A>(default(A), w);
            }

            /// <summary>
            /// constructs a writer monad from the value and messages.
            /// </summary>
            /// <param name="value">value</param>
            /// <param name="state">messages</param>
            public Writer(A value, IEnumerable<W> state)
            {
                this.State = state;
                this.Value = value;
            }

            /// <summary>
            /// runs this monad.
            /// </summary>
            /// <returns>writer tuple of result</returns>
            public WriterTuple<A, W> RunWriter()
            {
                return new WriterTuple<A, W>(Value, State);
            }
        }

        /// <summary>
        /// An unit function of writer monad.
        /// </summary>
        /// <typeparam name="W">type of message</typeparam>
        /// <typeparam name="A">type of value</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit writer monad</returns>
        public static Writer<W, A> ToWriter<W, A>(this A value)
        {
            return new Writer<W, A>(value, new W[] { });
        }

        /// <summary>
        /// maps the writer monad with the given function.
        /// </summary>
        /// <typeparam name="W">type of message</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of mapped value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Writer<W, B> Select<W, A, B>(this Writer<W, A> m, Func<A, B> k)
        {
            var s0 = m.RunWriter();
            var s1 = k(s0.Value);

            return new Writer<W, B>(s1, s0.State);
        }

        /// <summary>
        /// A bind function of reader monad.
        /// </summary>
        /// <typeparam name="W">type of message</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <param name="a">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <returns>bound monad</returns>
        public static Writer<W, B> SelectMany<W, A, B>(this Writer<W, A> a, Func<A, Writer<W, B>> k)
        {
            var s1 = a.RunWriter();
            var s2 = k(s1.Value).RunWriter();
            return new Writer<W, B>(s2.Value, s1.State.Concat(s2.State));
        }

        /// <summary>
        /// A bind function of reader monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="W">type of message</typeparam>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <typeparam name="C">type of mapped value</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Writer<W, C> SelectMany<W, A, B, C>(this Writer<W, A> m, Func<A, Writer<W, B>> k, Func<A, B, C> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToWriter<W, C>()));
        }

        /// <summary>
        /// A result tuple of writer monad.
        /// Type of state is fixed to string.
        /// </summary>
        /// <typeparam name="A">type of value</typeparam>
        public class WriterTuple<A>
        {
            /// <summary>
            /// A state (messages).
            /// </summary>
            public string State { get; private set; }

            /// <summary>
            /// A value.
            /// </summary>
            public A Value { get; private set; }

            /// <summary>
            /// constructs the result tuple.
            /// </summary>
            /// <param name="value">value</param>
            /// <param name="state">message</param>
            public WriterTuple(A value, string state)
            {
                this.State = state;
                this.Value = value;
            }

            /// <summary>
            /// A string representation of this tuple.
            /// </summary>
            /// <returns>string representation</returns>
            public override string ToString()
            {
                return "(" + Value.ToString() + ", " + State.ToString() + ")";
            }
        }

        /// <summary>
        /// A class of writer monad.
        /// Type of state is fixed to string.
        /// </summary>
        /// <typeparam name="A">type of value</typeparam>
        public class Writer<A>
        {
            private string State { get; set; }
            private A Value { get; set; }

            /// <summary>
            /// appends the given message to this monad.
            /// </summary>
            /// <param name="w">message</param>
            /// <returns>new monad</returns>
            public static Writer<A> Tell(string w)
            {
                return new Writer<A>(default(A), w);
            }

            /// <summary>
            /// constructs a writer monad from the value and message.
            /// </summary>
            /// <param name="value">value</param>
            /// <param name="state">message</param>
            public Writer(A value, string state)
            {
                this.State = state;
                this.Value = value;
            }

            /// <summary>
            /// runs this monad.
            /// </summary>
            /// <returns>writer tuple of result</returns>
            public WriterTuple<A> RunWriter()
            {
                return new WriterTuple<A>(Value, State);
            }
        }

        /// <summary>
        /// An unit function of writer monad.
        /// </summary>
        /// <typeparam name="A">type of value</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit writer monad</returns>
        public static Writer<A> ToWriter<A>(this A value)
        {
            return new Writer<A>(value, "");
        }

        /// <summary>
        /// maps the writer monad with the given function.
        /// </summary>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of mapped value</typeparam>
        /// <param name="m">monad</param>
        /// <param name="k">map function</param>
        /// <returns>mapped monad</returns>
        public static Writer<B> Select<A, B>(this Writer<A> m, Func<A, B> k)
        {
            var s0 = m.RunWriter();
            var s1 = k(s0.Value);

            return new Writer<B>(s1, s0.State);
        }

        /// <summary>
        /// A bind function of reader monad.
        /// </summary>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <param name="a">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <returns>bound monad</returns>
        public static Writer<B> SelectMany<A, B>(this Writer<A> a, Func<A, Writer<B>> k)
        {
            var s1 = a.RunWriter();
            var s2 = k(s1.Value).RunWriter();
            return new Writer<B>(s2.Value, s1.State + s2.State);
        }

        /// <summary>
        /// A bind function of reader monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="A">type of input value</typeparam>
        /// <typeparam name="B">type of output value</typeparam>
        /// <typeparam name="C">type of mapped value</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Writer<C> SelectMany<A, B, C>(this Writer<A> m, Func<A, Writer<B>> k, Func<A, B, C> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToWriter()));
        }

        /// <summary>
        /// An unit function of continuation monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="Ans">result type</typeparam>
        /// <param name="value">value to wrap</param>
        /// <returns>unit continuation monad</returns>
        public static Cont<T, Ans> ToCont<T, Ans>(this T value)
        {
            return k => k(value);
        }

        /// <summary>
        /// A bind function of continuation monad.
        /// </summary>
        /// <typeparam name="T">input type before bind</typeparam>
        /// <typeparam name="U">input type after bound</typeparam>
        /// <typeparam name="Ans">result type</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <returns>bound monad</returns>
        public static Cont<U, Ans> SelectMany<T, U, Ans>(this Cont<T, Ans> m, Func<T, Cont<U, Ans>> k)
        {
            return s => m(t => k(t)(s));
        }

        /// <summary>
        /// A bind function of continuation monad.
        /// This form is for LINQ query syntax.
        /// </summary>
        /// <typeparam name="T">input type before bind</typeparam>
        /// <typeparam name="U">input type after bound</typeparam>
        /// <typeparam name="U">input type after mapped</typeparam>
        /// <typeparam name="Ans">result type</typeparam>
        /// <param name="m">monad to bind</param>
        /// <param name="k">bind function</param>
        /// <param name="s">map function</param>
        /// <returns>bound monad</returns>
        public static Cont<V, Ans> SelectMany<T, U, V, Ans>(this Cont<T, Ans> m, Func<T, Cont<U, Ans>> k, Func<T, U, V> s)
        {
            return m.SelectMany(t => k(t).SelectMany(u => s(t, u).ToCont<V, Ans>()));
        }

        /// <summary>
        /// gets the current continuation.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="Ans">result type</typeparam>
        /// <param name="f">a function to which the countinuation passes as first argument</param>
        /// <returns>new monad</returns>
        public static Cont<T, Ans> CallCC<T, Ans>(Func<Func<T, Cont<T, Ans>>, Cont<T, Ans>> f)
        {
            return (Func<T, Ans> k) => f((T value) => (Func<T, Ans> x) => k(value))(k);
        }
    }
}
