/*
 * This source code is under the Unlicense
 */
using System;
using System.Text.RegularExpressions;

namespace Morilib
{
    /// <summary>
    /// Parser Monad Library.
    /// </summary>
    public static partial class MonadPrimus
    {
        private static readonly string PatternReal = @"[\+\-]?(?:[0-9]+(?:\.[0-9]+)?|\.[0-9]+)(?:[eE][\+\-]?[0-9]+)?";

        /// <summary>
        /// Parser delegate.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="config">initial parameter</param>
        /// <param name="position">position</param>
        /// <returns>result of parsing</returns>
        public delegate Result<T> Parser<T>(Config config, int position);

        /// <summary>
        /// Initial configuration.
        /// </summary>
        public class Config
        {
            /// <summary>
            /// A string to parse.
            /// </summary>
            public string ParseString { get; private set; }

            /// <summary>
            /// A regex pattern of skip string.
            /// </summary>
            public Regex Skip { get; private set; }

            /// <summary>
            /// constructs initial configuration.
            /// </summary>
            /// <param name="parseString">string to parse</param>
            /// <param name="skip">pattern of skip</param>
            public Config(string parseString, string skip)
            {
                ParseString = parseString;
                Skip = skip == null ? null : new Regex(skip);
            }

            /// <summary>
            /// constructs initial configuration with no skip pattern.
            /// </summary>
            /// <param name="parseString">string to parse</param>
            public Config(string parseString)
            {
                ParseString = parseString;
                Skip = null;
            }
        }

        /// <summary>
        /// A result of parsing.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        public class Result<T>
        {
            /// <summary>
            /// Initial configuration.
            /// </summary>
            public Config Config { get; private set; }

            /// <summary>
            /// Position of parsing.
            /// </summary>
            public int Position { get; private set; }

            /// <summary>
            /// Result value.
            /// </summary>
            public T Value { get; private set; }

            /// <summary>
            /// Error message.
            /// </summary>
            public string ErrorMessage { get; private set; }

            /// <summary>
            /// True if the result is error.
            /// </summary>
            public bool IsError
            {
                get => ErrorMessage != null;
            }

            /// <summary>
            /// constructs result of parsing.
            /// </summary>
            /// <param name="config">initial configuration</param>
            /// <param name="position">position</param>
            /// <param name="value">value</param>
            public Result(Config config, int position, T value)
            {
                Config = config;
                Position = position;
                Value = value;
                ErrorMessage = null;
            }

            /// <summary>
            /// constructs result of parsing with error message.
            /// </summary>
            /// <param name="errorMessage"></param>
            public Result(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }

        private static void CheckNull(dynamic parser, string name)
        {
            if(parser == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// runs the parser with given condition.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="toParse">string to parse</param>
        /// <param name="position">starting position of parsing</param>
        /// <param name="skip">skip pattern</param>
        /// <returns></returns>
        public static Result<T> Run<T>(this Parser<T> parser, string toParse, int position, string skip)
        {
            CheckNull(parser, nameof(parser));
            CheckNull(toParse, nameof(toParse));
            if (toParse.Length < position || position < 0)
            {
                throw new ArgumentOutOfRangeException("invalid position");
            }
            return parser(new Config(toParse, skip), position);
        }

        /// <summary>
        /// runs the parser with given condition.
        /// Starting position is beginning of the string.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="toParse">string to parse</param>
        /// <param name="skip">skip pattern</param>
        /// <returns></returns>
        public static Result<T> Run<T>(this Parser<T> parser, string toParse, string skip)
        {
            return parser.Run(toParse, 0, skip);
        }

        /// <summary>
        /// runs the parser with given condition and no skipping pattern.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="toParse">string to parse</param>
        /// <param name="position">starting position of parsing</param>
        /// <returns></returns>
        public static Result<T> Run<T>(this Parser<T> parser, string toParse, int position)
        {
            return parser.Run(toParse, position, null);
        }

        /// <summary>
        /// runs the parser with given condition and no skipping pattern.
        /// Starting position is beginning of the string.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="toParse">string to parse</param>
        /// <returns></returns>
        public static Result<T> Run<T>(this Parser<T> parser, string toParse)
        {
            return parser.Run(toParse, 0, null);
        }

        private static int Skip(string aString, int position, Regex regex)
        {
            if (regex == null)
            {
                return position;
            }
            else
            {
                var restString = aString.Substring(position);
                var matchRe = regex.Match(restString);
                var positionNew = position + (matchRe.Success && matchRe.Index == 0 ? matchRe.Length : 0);

                return positionNew;
            }
        }

        /// <summary>
        /// creates a parser of matching the string.
        /// </summary>
        /// <param name="aString">expected string</param>
        /// <param name="errorMessage">error message if this does not match</param>
        /// <returns>parser of matching the string</returns>
        public static Parser<string> Str(string aString, string errorMessage)
        {
            CheckNull(aString, nameof(aString));
            return (config, position) =>
            {
                var pos2 = Skip(config.ParseString, position, config.Skip);
                var toParse = config.ParseString;

                if (toParse.Length >= pos2 + aString.Length && toParse.Substring(pos2, aString.Length) == aString)
                {
                    return new Result<string>(config, pos2 + aString.Length, aString);
                }
                else
                {
                    return new Result<string>(errorMessage);
                }
            };
        }

        /// <summary>
        /// creates a parser of matching the string.
        /// </summary>
        /// <param name="aString">expected string</param>
        /// <returns>parser of matching the string</returns>
        public static Parser<string> Str(string aString)
        {
            return Str(aString, "Does not match " + aString);
        }

        /// <summary>
        /// creates a parser of matching the string with ignoring case.
        /// </summary>
        /// <param name="aString">expected string</param>
        /// <param name="errorMessage">error message if this does not match</param>
        /// <returns>parser of matching the string</returns>
        public static Parser<string> IgnoreCase(string aString, string errorMessage)
        {
            CheckNull(aString, nameof(aString));
            var aStringCase = aString.ToLower();

            return (config, position) =>
            {
                var pos2 = Skip(config.ParseString, position, config.Skip);
                var toParse = config.ParseString;

                if (toParse.Length >= pos2 + aString.Length && toParse.Substring(pos2, aString.Length).ToLower() == aStringCase)
                {
                    return new Result<string>(config, pos2 + aString.Length, toParse.Substring(pos2, aString.Length));
                }
                else
                {
                    return new Result<string>(errorMessage);
                }
            };
        }

        /// <summary>
        /// creates a parser of matching the string with ignoring case.
        /// </summary>
        /// <param name="aString">expected string</param>
        /// <returns>parser of matching the string</returns>
        public static Parser<string> IgnoreCase(string aString)
        {
            return IgnoreCase(aString, "Does not match " + aString);
        }

        /// <summary>
        /// creates a parser of matching the regex.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="errorMessage">error message if this does not match</param>
        /// <returns>parser of matching the regex</returns>
        public static Parser<string> Regex(string pattern, string errorMessage)
        {
            CheckNull(pattern, nameof(pattern));
            return (config, position) =>
            {
                var pos2 = Skip(config.ParseString, position, config.Skip);
                var toParse = config.ParseString.Substring(pos2);
                var regex = new Regex(pattern);
                var match = regex.Match(toParse);

                if (match.Success && match.Index == 0)
                {
                    return new Result<string>(config, pos2 + match.Length, match.Value);
                }
                else
                {
                    return new Result<string>(errorMessage);
                }
            };
        }

        /// <summary>
        /// creates a parser of matching the regex.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>parser of matching the regex</returns>
        public static Parser<string> Regex(string pattern)
        {
            return Regex(pattern, "Does not match pattern " + pattern);
        }

        /// <summary>
        /// creates a parser of matching the end of string.
        /// </summary>
        /// <param name="errorMessage">error message if this does not match</param>
        /// <returns>parser of mathing the end of string</returns>
        public static Parser<int> End(string errorMessage)
        {
            return (config, position) =>
            {
                var pos2 = Skip(config.ParseString, position, config.Skip);

                if (pos2 >= config.ParseString.Length)
                {
                    return new Result<int>(config, pos2, 0);
                }
                else
                {
                    return new Result<int>(errorMessage);
                }
            };
        }

        /// <summary>
        /// creates a parser of matching the end of string.
        /// </summary>
        /// <returns>parser of mathing the end of string</returns>
        public static Parser<int> End()
        {
            return End("Not reached to end of parsing");
        }

        /// <summary>
        /// creates a parser of matching the real number.
        /// </summary>
        /// <param name="errorMessage">error message if this does not match</param>
        /// <returns>parser of matching the real number</returns>
        public static Parser<double> Real(string errorMessage)
        {
            return Regex(PatternReal, errorMessage).Select(x => double.Parse(x));
        }

        /// <summary>
        /// creates a parser of matching the real number.
        /// </summary>
        /// <returns>parser of matching the real number</returns>
        public static Parser<double> Real()
        {
            return Regex(PatternReal, "Does not match the real number").Select(x => double.Parse(x));
        }

        /// <summary>
        /// The monadic unit function of Parser.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="value">value</param>
        /// <returns>unit monad</returns>
        public static Parser<T> ToParser<T>(this T value)
        {
            return (config, position) => new Result<T>(config, position, value);
        }

        /// <summary>
        /// maps the result value of parser with the given function.
        /// </summary>
        /// <typeparam name="T">input value</typeparam>
        /// <typeparam name="U">output value</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="f">mapping function</param>
        /// <returns></returns>
        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> f)
        {
            CheckNull(parser, nameof(parser));
            CheckNull(f, nameof(f));
            return (config, position) =>
            {
                var result = parser(config, position);

                if (result.IsError)
                {
                    return new Result<U>(result.ErrorMessage);
                }
                else
                {
                    return new Result<U>(result.Config, result.Position, f(result.Value));
                }
            };
        }

        /// <summary>
        /// Monadic bind function of parser monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <param name="parser1">parser</param>
        /// <param name="m">function returns monad</param>
        /// <returns>bound monad</returns>
        public static Parser<U> SelectMany<T, U>(this Parser<T> parser1, Func<T, Parser<U>> m)
        {
            CheckNull(parser1, nameof(parser1));
            CheckNull(m, nameof(m));
            return (config, position) =>
            {
                var result1 = parser1(config, position);

                if (result1.IsError)
                {
                    return new Result<U>(result1.ErrorMessage);
                }
                else
                {
                    return m(result1.Value)(result1.Config, result1.Position);
                }
            };
        }

        /// <summary>
        /// Monadic bind function of parser monad.
        /// </summary>
        /// <typeparam name="T">input type</typeparam>
        /// <typeparam name="U">output type</typeparam>
        /// <typeparam name="V">mapped type</typeparam>
        /// <param name="parser1">parser</param>
        /// <param name="m">function returns monad</param>
        /// <param name="f">mapping function</param>
        /// <returns>bound monad</returns>
        public static Parser<V> SelectMany<T, U, V>(this Parser<T> parser1, Func<T, Parser<U>> m, Func<T, U, V> f)
        {
            CheckNull(parser1, nameof(parser1));
            CheckNull(m, nameof(m));
            CheckNull(f, nameof(f));
            return parser1.SelectMany(t => m(t).SelectMany(u => f(t, u).ToParser()));
        }

        /// <summary>
        /// maps error message.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="f">mapping function of error message</param>
        /// <returns>parser with changed error message</returns>
        public static Parser<T> SelectError<T>(this Parser<T> parser, Func<string, string> f)
        {
            CheckNull(parser, nameof(parser));
            CheckNull(f, nameof(f));
            return (config, position) =>
            {
                var result = parser(config, position);

                if (result.IsError)
                {
                    var message = f(result.ErrorMessage);
                    
                    if(message == null)
                    {
                        throw new InvalidOperationException("message cannot be null");
                    }
                    return new Result<T>(message);
                }
                else
                {
                    return result;
                }
            };
        }

        /// <summary>
        /// changes error message
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="message">error message to change</param>
        /// <returns>parser with changed message</returns>
        public static Parser<T> ChangeError<T>(this Parser<T> parser, string message)
        {
            CheckNull(message, nameof(message));
            return parser.SelectError(x => message);
        }

        /// <summary>
        /// returns the result of first parser if first parser is matched,
        /// otherwise returns the result of second parser.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser1">first parser</param>
        /// <param name="parser2">second parser</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> Choice<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            CheckNull(parser1, nameof(parser1));
            CheckNull(parser2, nameof(parser2));
            return (config, position) =>
            {
                var result1 = parser1(config, position);

                return result1.IsError ? parser2(config, position) : result1;
            };
        }

        /// <summary>
        /// returns the result of the given parser if it is matched,
        /// otherwise previous result with the default value.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="defaultValue">default value if the parser is not matched</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> Option<T>(this Parser<T> parser, T defaultValue)
        {
            CheckNull(parser, nameof(parser));
            return (config, position) =>
            {
                Result<T> result = new Result<T>(config, position, defaultValue);
                Result<T> resultNew = parser(result.Config, result.Position);

                return resultNew.IsError ? result : resultNew;
            };
        }

        /// <summary>
        /// returns the result of the given parser if it is matched,
        /// otherwise previous result with default(T).
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> Option<T>(this Parser<T> parser)
        {
            return parser.Option(default(T));
        }

        /// <summary>
        /// repeats the parser delimited the delimiter parser.
        /// The value is aggregated by aggregator function.
        /// This aggregates the values from left to right.
        /// </summary>
        /// <typeparam name="T">type of parser value</typeparam>
        /// <typeparam name="D">type of delimiter value</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="delimiter">delimiter</param>
        /// <param name="aggregator">aggregator function</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> Delimit<T, D>(this Parser<T> parser, Parser<D> delimiter, Func<T, D, T, T> aggregator)
        {
            return parser.SelectMany(attr => DelimitRest(attr, parser, delimiter, aggregator));
        }

        private static Parser<T> DelimitRest<T, D>(T attr1, Parser<T> parser, Parser<D> delimiter, Func<T, D, T, T> aggregator)
        {
            return delimiter.SelectMany(d => parser.SelectMany(
                attr2 => DelimitRest(aggregator(attr1, d, attr2), parser, delimiter, aggregator))).Choice(attr1.ToParser());
        }

        /// <summary>
        /// repeats the parser delimited the delimiter parser.
        /// The value is aggregated by aggregator function.
        /// This aggregates the values from right to left.
        /// </summary>
        /// <typeparam name="T">type of parser value</typeparam>
        /// <typeparam name="D">type of delimiter value</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="delimiter">delimiter</param>
        /// <param name="aggregator">aggregator function</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> DelimitRight<T, D>(this Parser<T> parser, Parser<D> delimiter, Func<T, D, T, T> aggregator)
        {
            return parser.SelectMany(attr => DelimitRightRest(attr, parser, delimiter, aggregator));
        }

        private static Parser<T> DelimitRightRest<T, D>(T attr1, Parser<T> parser, Parser<D> delimiter, Func<T, D, T, T> aggregator)
        {
            return delimiter.SelectMany(d => parser.SelectMany(
                attr2 => DelimitRightRest(attr2, parser, delimiter, aggregator)).SelectMany(
                r => aggregator(attr1, d, r).ToParser()).Choice(attr1.ToParser()));
        }

        /// <summary>
        /// repeats the parser one or more times.
        /// The values are aggregated from left to right.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="aggregator">aggragator function</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> OneOrMore<T>(this Parser<T> parser, Func<T, T, T> aggregator)
        {
            return Delimit(parser, Str(""), (a, d, b) => aggregator(a, b));
        }

        /// <summary>
        /// repeats the parser zero or more times.
        /// The values are aggregated from left to right.
        /// If the parser matched zero times, the value will be the default value.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="aggregator">aggragator function</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> ZeroOrMore<T>(this Parser<T> parser, Func<T, T, T> aggregator, T defaultValue)
        {
            return OneOrMore(parser, aggregator).Choice(Str("").Select(x => defaultValue));
        }

        /// <summary>
        /// repeats the parser zero or more times.
        /// The values are aggregated from left to right.
        /// If the parser matched zero times, the value will be default(T).
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <param name="aggregator">aggragator function</param>
        /// <returns>synchronized parser</returns>
        public static Parser<T> ZeroOrMore<T>(this Parser<T> parser, Func<T, T, T> aggregator)
        {
            return ZeroOrMore(parser, aggregator, default(T));
        }

        /// <summary>
        /// matches the given parser without advancing the position.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <returns>syncronized parser</returns>
        public static Parser<T> Lookahead<T>(this Parser<T> parser)
        {
            return (config, position) =>
            {
                var result = parser(config, position);

                if(result.IsError)
                {
                    return result;
                }
                else
                {
                    return new Result<T>(result.Config, position, result.Value);
                }
            };
        }

        /// <summary>
        /// If the parser is not matched, matches without advancing the position.
        /// If the parser is matched, this match fails.
        /// </summary>
        /// <typeparam name="T">type</typeparam>
        /// <param name="parser">parser</param>
        /// <returns>syncronized parser</returns>
        public static Parser<T> Not<T>(this Parser<T> parser)
        {
            return (config, position) =>
            {
                var result = parser(config, position);

                if (result.IsError)
                {
                    return new Result<T>(config, position, default(T));
                }
                else
                {
                    return new Result<T>("Unexpected match");
                }
            };
        }

        /// <summary>
        /// A method which can refer a return values of the function itself.<br>
        /// This method will be used for defining a expression with recursion.
        /// </summary>
        /// <param name="func">a function which is a return value itself</param>
        /// <returns>first parser</returns>
        public static Parser<T> Letrec1<T>(Func<Parser<T>, Parser<T>> func)
        {
            Parser<T> delay = null;
            Parser<T> memo = null;

            CheckNull(func, nameof(func));
            delay = (config, position) =>
            {
                if (memo == null)
                {
                    memo = func(delay);
                }
                return memo(config, position);
            };
            return delay;
        }

        /// <summary>
        /// A method which can refer a return values of the function itself.<br>
        /// This method will be used for defining a expression with recursion.
        /// </summary>
        /// <param name="func1">a function whose first argument is a return value itself</param>
        /// <param name="func2">a function whose second argument is a return value itself</param>
        /// <returns>first parser</returns>
        public static Parser<T> Letrec2<T, U>(Func<Parser<T>, Parser<U>, Parser<T>> func1, Func<Parser<T>, Parser<U>, Parser<U>> func2)
        {
            Parser<T> delay1 = null;
            Parser<T> memo1 = null;
            Parser<U> delay2 = null;
            Parser<U> memo2 = null;

            CheckNull(func1, nameof(func1));
            CheckNull(func2, nameof(func2));
            delay1 = (config, position) =>
            {
                if (memo1 == null)
                {
                    memo1 = func1(delay1, delay2);
                }
                return memo1(config, position);
            };
            delay2 = (config, position) =>
            {
                if (memo2 == null)
                {
                    memo2 = func2(delay1, delay2);
                }
                return memo2(config, position);
            };
            return delay1;
        }

        /// <summary>
        /// A method which can refer a return values of the function itself.<br>
        /// This method will be used for defining a expression with recursion.
        /// </summary>
        /// <param name="func1">a function whose first argument is a return value itself</param>
        /// <param name="func2">a function whose second argument is a return value itself</param>
        /// <returns>first parser</returns>
        public static Parser<T> Letrec2<T>(Func<Parser<T>, Parser<T>, Parser<T>> func1, Func<Parser<T>, Parser<T>, Parser<T>> func2)
        {
            return Letrec2<T, T>(func1, func2);
        }

        /// <summary>
        /// A method which can refer a return values of the function itself.<br>
        /// This method will be used for defining a expression with recursion.
        /// </summary>
        /// <param name="func1">a function whose first argument is a return value itself</param>
        /// <param name="func2">a function whose second argument is a return value itself</param>
        /// <param name="func3">a function whose third argument is a return value itself</param>
        /// <returns>first parser</returns>
        public static Parser<T> Letrec3<T, U, V>(
            Func<Parser<T>, Parser<U>, Parser<V>, Parser<T>> func1,
            Func<Parser<T>, Parser<U>, Parser<V>, Parser<U>> func2,
            Func<Parser<T>, Parser<U>, Parser<V>, Parser<V>> func3)
        {
            Parser<T> delay1 = null;
            Parser<T> memo1 = null;
            Parser<U> delay2 = null;
            Parser<U> memo2 = null;
            Parser<V> delay3 = null;
            Parser<V> memo3 = null;

            CheckNull(func1, nameof(func1));
            CheckNull(func2, nameof(func2));
            CheckNull(func3, nameof(func3));
            delay1 = (config, position) =>
            {
                if (memo1 == null)
                {
                    memo1 = func1(delay1, delay2, delay3);
                }
                return memo1(config, position);
            };
            delay2 = (config, position) =>
            {
                if (memo2 == null)
                {
                    memo2 = func2(delay1, delay2, delay3);
                }
                return memo2(config, position);
            };
            delay3 = (config, position) =>
            {
                if (memo3 == null)
                {
                    memo3 = func3(delay1, delay2, delay3);
                }
                return memo3(config, position);
            };
            return delay1;
        }

        /// <summary>
        /// A method which can refer a return values of the function itself.<br>
        /// This method will be used for defining a expression with recursion.
        /// </summary>
        /// <param name="func1">a function whose first argument is a return value itself</param>
        /// <param name="func2">a function whose second argument is a return value itself</param>
        /// <param name="func3">a function whose third argument is a return value itself</param>
        /// <returns>first parser</returns>
        public static Parser<T> Letrec3<T>(
            Func<Parser<T>, Parser<T>, Parser<T>, Parser<T>> func1,
            Func<Parser<T>, Parser<T>, Parser<T>, Parser<T>> func2,
            Func<Parser<T>, Parser<T>, Parser<T>, Parser<T>> func3)
        {
            return Letrec3<T, T, T>(func1, func2, func3);
        }
    }
}
