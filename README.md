# MonadPrimus C#

## What is MonadPrimus C#
MonadPrimus C# is an implementation of monads using C# LINQ.  
Monads shown as follows are supported.

* Identity monad
* Maybe monad
* Either monad
* State Monad
* Reader and Writer monad
* Continuation monad
* Parser

## Identity monad
To convert a value to identity monad, ToIdentity method is available.  
ToIdentity is a Unit function of identity monad.

```csharp
var monad1 = 765.ToIdentity()
```

To get a value from identity monad, Value property is available.

To bind two identity monad, SelectMany method is avaliable.

```csharp
var monad = 765.ToIdentity().SelectMany(x => (x + 346).ToIdentity());
// monad.Value = 1111
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var monad = from a in 765.ToIdentity()
            from b in 346.ToIdentity()
            select a + b;
// monad.Value = 1111
```

This supports Select method to map a value of identity monad.

```csharp
var monad = 29.ToIdentity().Select(x => x * x);
// monad.Value = 841
```

## Maybe monad
To convert a value to Maybe monad, ToMaybe method is available.  
ToIdentity is a Unit function of Maybe monad.

```csharp
var monad1 = 765.ToMaybe();
```

To get a value from Maybe monad, Value property is available.  
To check the Maybe monad has a value, HasValue property is available.

A Maybe monad which has no value, Nothing field is available.

```csharp
var monad1 = Maybe<int>.Nothing;
```

To bind two Maybe monad, SelectMany method is avaliable.

```csharp
var monad = 765.ToMaybe().SelectMany(x => (x + 346).ToMaybe());
// monad.Value = 1111
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var monad = from a in 765.ToMaybe()
            from b in 346.ToMaybe()
            select a + b;
// monad.Value = 1111
```

This supports Select method to map a value of Maybe monad.

```csharp
var monad = 29.ToMaybe().Select(x => x * x);
// monad.Value = 841
```

This also supports Where method.

```csharp
var monad1 = 27.ToMaybe().Where(x => x < 100);
var monad2 = 27.ToMaybe().Where(x => x > 100);
// monad1.Value    = 27
// monad2.HasValue = False
```

## Either monad
To create the Either monad which has right value, ToRight method is supported.  
To create the Either monad which has left value, ToLeft method is supproted.

```csharp
var monad1 = Either.ToRight<string, int>(27);
var monad2 = Either.ToLeft<string, int>("nothing");
// monad1.Right = 27
// monad2.Left  = "nothing"

monad1.Or(monad2).Right;   // 27
monad2.Or(monad1).Right;   // 27
```

To get a right value from Maybe monad, Right property is available.  
To get a left value from Maybe monad, Left property is available.  
To check the Maybe monad has a right value, IsRight property is available.

To convert a value to Either monad, ToEither method is available.
ToIdentity is a Unit function of Either monad.

```csharp
var monad = 765.ToEither<string, int>();
```

To bind two Either monad, SelectMany method is avaliable.

```csharp
var monad = 765.ToEither<string, int>().SelectMany(x => (x + 346).ToEither<int, string>());
// monad.Right = 1111
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var monad = from a in 765.ToEither<string, int>()
            from b in 346.ToEither<string, int>()
            select a + b;
// monad.Right = 1111
```

This supports Select method to map a value of Either monad.

```csharp
var monad = 29.ToEither<string, int>().Select(x => x * x);
// monad.Right = 841
```

## State monad
To convert a value to State monad, ToState method is available.  
ToIdentity is a Unit function of State monad.

```csharp
var monad = "765".ToState<int, string>();
// monad(2) = ("765", 2)
```

To bind two State monad, SelectMany method is avaliable.

```csharp
State<string, int> monad1 = x => new MonadPrimus.StateTuple<int, string>(
    int.Parse(x) + 1, x.ToString() + "1");
var monad2 = monad1.SelectMany(x => new MonadPrimus.State<string, string>(
    s => new MonadPrimus.StateTuple<string, string>(x + s, s + x)));
var result = monad2("1");

// result.State = "112"
// result.Value = "211"
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var m11 = from x in new MonadPrimus.State<string, int>(
                        x => new MonadPrimus.StateTuple<int, string>(
                            int.Parse(x) + 1, x.ToString() + "1"))
          from y in new MonadPrimus.State<string, string>(
                        s => new MonadPrimus.StateTuple<string, string>(x + s, s + x))
          select x + y;
var res11 = m11("1");

// result.State = "112"
// result.Value = "2211"
```

To get a state from the State monad, Get method is available.

```csharp
var monad = from x in "765".ToState<int, string>()
            from y in MonadPrimus.Get<int>()
            select y;
var result = monad(346);

// result.Value = 346
// result.State = 346
```

To set a state to the State monad, Put method is available.

```csharp
var monad = from x in "765".ToState<int, string>()
            from y in MonadPrimus.Put<int, string>(876)
            select x;
var result = monad(666);

// result.Value = "765"
// result.State = 876
```

To modify a state of the State monad, Modify method is available.

```csharp
var monad = from x in "765".ToState<int, string>()
            from y in MonadPrimus.Modify<int, string>(x => x * x)
            select x;
var result = monad(29);

// result.Value = "765"
// result.State = 841
```

This supports Select method to map a value of State monad.

```csharp
var monad = 29.ToState<int, int>().Select(x => x * x);
var result = monad(765);

// result.Value = 841
// result.State = 765
```

## Reader monad
To convert a value to Reader monad, ToState method is available.  
ToIdentity is a Unit function of Reader monad.

```csharp
var monad = 765.ToReader<int, int>();
// monad(27) = 765
```

To bind two Reader monad, SelectMany method is avaliable.

```csharp
var m11 = new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1);
var m12 = m11.SelectMany(x => new MonadPrimus.Reader<string, string>(s => x + s));
// m12("1") = "21"
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var m11 = from x in new MonadPrimus.Reader<string, int>(x => int.Parse(x) + 1)
          from y in new MonadPrimus.Reader<string, string>(s => x + s)
          select x + y;
// m11("1") = "221"
```

To get environment from the Reader monad, Ask method is available.

```csharp
var monad = from x in "765".ToReader<string, int>()
            from y in MonadPrimus.Ask<int, int>()
            select y;
// monad(346) = 346
```

To change environment temporary, Local method is avaiable.

```csharp
var monad = from x in MonadPrimus.Ask<string>()
            from y in MonadPrimus.Local(
                e => "," + e, MonadPrimus.Ask<string>())
            from z in MonadPrimus.Ask<string>()
            select z + y;
// monad("a") = "a,a"
```

This supports Select method to map a value of Reader monad.

```csharp
var monad = 29.ToReader<int, int>().Select(x => x * x);
// moand(27) = 841
```

## Writer monad
To create a Writer monad, Create method is available.

```csharp
var monad = MonadPrimus.Create(765, new string[] { "init" });
var result = monad();

// result.Value = 765
// result.State = [ "init" ]
```

To convert a value to Writer monad, ToWriter method is available.  
ToIdentity is a Unit function of Writer monad.

```csharp
var monad = 765.ToWriter<int, string>()
var result = monad();

// result.Value = 765
// result.State = []
```

To bind two Writer monad, SelectMany method is avaliable.

```csharp
var m11 = MonadPrimus.Create(1, new string[] { "init1", "init2" });
var m12 = m11.SelectMany(x => MonadPrimus.Create(x + "1", new string[] { "next" }));
var res11 = m12();

// res11.Value = "11"
// res11.State = [ "init1", "init2", "next" ]
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var m11 = from x in MonadPrimus.Create(1, new string[] { "init1", "init2" })
          from y in MonadPrimus.Create(x + "1", new string[] { "next" })
          select x + y;
var res11 = m11();

// res11.Value = "111"
// res11.State = [ "init1", "init2", "next" ]
```

To append the result to the Writer monad, Tell method is available.

```csharp
var monad = from x in 765.ToWriter<int, string>()
            from y in MonadPrimus.Tell<int, string>("append")
            select x;
var result = monad();

// result.Value = 765
// result.State = [ "append" ]
```

This supports Select method to map a value of Reader monad.

```csharp
var monad = 29.ToWriter<int, string>().Select(x => x * x);
var result = moand();

// result.Value = 841
// result.State = []
```

## Continuation monad
To convert a value to Continuation monad, ToCont method is available.  
ToIdentity is a Unit function of Continuation monad.

```csharp
765.ToCont<int, int>()(x => x + 346);   // 1111
```

To bind two Continuation monad, SelectMany method is avaliable.

```csharp
MonadPrimus.Cont<int, int> m01 = x => x(765);
var m11 = m01.SelectMany<int, int, int>(x => y => y(x + 100));

// m11(x => x + 11) = 876
```

MonadPrimus C# supports SelectMany method which can use LINQ query syntax.

```csharp
var m11 = from x in 765.ToCont<int, int>()
          from y in 100.ToCont<int, int>()
          select x + y;

// m11(x => x + 11) = 876
```

To get current continuation from Continuation monad, CallCC method is available.  

```csharp
var m11 = MonadPrimus.CallCC<int, int>(
    exit => from a in 1.ToCont<int, int>()
            from b in 7.ToCont<int, int>()
            from c in a + b < 10 ? exit(666) : 6.ToCont<int, int>()
            from d in 5.ToCont<int, int>()
            select a + b + c + d);
var m12 = MonadPrimus.CallCC<int, int>(
    exit => from a in 8.ToCont<int, int>()
            from b in 7.ToCont<int, int>()
            from c in a + b < 10 ? exit(666) : 6.ToCont<int, int>()
            from d in 5.ToCont<int, int>()
            select a + b + c + d);

// m11(x => x) = 666
// m12(x => x) = 26
```

## Parser

### Parser delegate
Parser delegate plays a role of MonadPrimus C# Parser.  
Arguments of Parser delegate are environment (Env class) and position (int).  
And Parser delegate return the result (Result class).

```csharp
public delegate Result<T> Parser<T>(Env env, int position);
```

Parser delegate has Run extension methods to pass input string and skip pattern.

```csharp
        // input string, position and Skip pattern by Parser
        public static Result<T> Run<T>(this Parser<T> parser,
            string toParse, int position, Parser<string> skip) { ... }

        // input string, position and Skip pattern by Regex
        public static Result<T> Run<T>(this Parser<T> parser,
             string toParse, int position, string skip) { ... }

        // input string and Skip pattern by Parser
        // position specifies 0
        public static Result<T> Run<T>(this Parser<T> parser,
             string toParse, Parser<string> skip) { ... }

        // input string and Skip pattern by Regex
        // position specifies 0
        public static Result<T> Run<T>(this Parser<T> parser,
             string toParse, string skip) { ... }

        // input string and position
        // This parser does not skip
        public static Result<T> Run<T>(this Parser<T> parser,
             string toParse, int position) { ... }

        // input string only
        // position specifies 0
        // This parser does not skip
        public static Result<T> Run<T>(this Parser<T> parser,
             string toParse) { ... }
```

### Env class
Env class has infomation which does not change in parsing.  
The information consists of input string to parser and Skip pattern (Parser delegate).

```csharp
        public class Env
        {
            public string ParseString { get; private set; }
            public Parser<string> Skip { get; private set; }

            public Env(string parseString, Parser<string> skip)
            {
                ParseString = parseString;
                Skip = skip;
            }

            public Env(string parseString)
            {
                ParseString = parseString;
                Skip = null;
            }
        }
```

### Result class
Result class has three properties that are Env class, position, value and error message.

```csharp
        public class Result<T>
        {
            public Env Env { get; private set; }
            public int Position { get; private set; }
            public T Value { get; private set; }
            public string ErrorMessage { get; private set; }

            public bool IsError
            {
                get => ErrorMessage != null;
            }

            public Result(Env env, int position, T value)
            {
                Env = env;
                Position = position;
                Value = value;
                ErrorMessage = null;
            }

            public Result(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }
```

### Str method
Str method matches the given string and input string are matched,
otherwise returns with error message.  
Error message is optional.

```csharp
var res1 = MonadPrimus.Str("765");

//res1.Run("765");   // Match
//res1.Run("666");   // No Match
```

### IgnoreCase method
IgnoreCase method matches the given string and input string are matched without case.

```csharp
var res1 = MonadPrimus.IgnoreCase("765pro");

//res1.Run("765pro");   // Match
//res1.Run("765PRO");   // Match
```

### Regex method
Regex method matches the given regex matches the given string.

```csharp
var res1 = MonadPrimus.Regex("[1-8]+");

//res1.Run("765");   // Match
//res1.Run("666");   // No Match
```

### End method
End method matches end of input string.

```csharp
var res1 = MonadPrimus.End();

//res1.Run("");      // Match
//res1.Run("666");   // No Match
```

### Real method
Real method matches any float value and the value of parsing is the matched double value.

```csharp
var res1 = MonadPrimus.Real();

//res1.Run("76.5");   // Match, value: 76.5
//res1.Run("aaaa");   // No Match
```

### ToParser method
ToParser method returns the given value itself.  
ToParser method is monadic Unit function.

```csharp
var res1 = MonadPrimus.ToParser(765);

//res1.Run("");   // Match, value: 765
```

### Select method
Select method maps result value of the given Parser delegate.

```csharp
var res1 = MonadPrimus.Real().Select(x => x + 346);

//res1.Run("765");   // Match, value: 1111.0
```

### SelectMany method
SelectMany method binds the two Parser delegate.  
SelectMany method is monadic Bind function.

```csharp
var res1 = MonadPrimus.Regex("[0-9][0-9]").SelectMany(x => MonadPrimus.Str(x));

//res1.Run("2727");   // Match
//res1.Run("2728");   // No Match
```

SelectMany method has an override which can use LINQ query syntax.

```csharp
var res1 = from a in MonadPrimus.Real()
           from b in MonadPrimus.Real()
           select a + b;

//res1.Run("765  346", " +");   // Match, value: 1111.0
```

### SelectError method
SelectError method maps the error message.

```csharp
var res1 = MonadPrimus.Str("765").SelectError(x => "Not 765");

//res1.Run("666");   // No Match, error message: "Not 765"
```

### ChangeError method
SelectError method changes the error message.

```csharp
var res1 = MonadPrimus.Str("765").ChangeError("Not 765");

res1.Run("666");   // No Match, error message: "Not 765"
```

### Choice method
Choice method returns the result of first argument if it is matched,
otherwise returns the result of second argument.

```csharp
var res1 = MonadPrimus.Str("765").Choice(MonadPrimus.Str("346"));

//res1.Run("765");   // Match
//res1.Run("346");   // Match
//res1.Run("666");   // No Match
```

### Option method
Option method returns the result of argument if it is matched,
otherwise return the second argument as a value.

```csharp
var res1 = MonadPrimus.Str("765").Option("000");

//res1.Run("765");   // Match, value: 765
//res1.Run("876");   // Match, value: 000
```

### Delimit method
Delimit method aggregates the value of first argument by function given the third argument
and the second argument as a delimiter.  
This method aggregates left associative.

```csharp
var res1 = MonadPrimus.Real().Delimit(
             MonadPrimus.Str("+"), (x, op, y) => x + y);

//res1.Run("1+2+3");   // Match, value: 6
```

### DelimitRight method
DelimitRight method is similar to Delimit method but it is right associative.

### OneOrMore method
OneOrMore method aggregates the value of first argument by function given the second argument.

```csharp
var res1 = MonadPrimus.Real().OneOrMore((x, y) => x + y);

//res1.Run("1 2 3", " +");   // Match, value: 6
```

### ZeroOrMore method
ZeroOrMore method aggregates the value of first argument by function given the second argument.  
If input string does not match first argument then returns the value of third argument.  
The third argument is optional, then the third value is default(T).

```csharp
var res1 = MonadPrimus.Real().ZeroOrMore((x, y) => x + y, -1);

//res1.Run("1 2 3", " +");   // Match, value: 6
//res1.Run("", " +");        // Match, value: -1
```

### Lookahead method
Lookahead method matches if pattern of the argument is matched but position does not advance.

```csharp
var res1 = MonadPrimus.Str("876").LookAhead()
            .SelectMany(x => MonadPrimus.Real());

//res1.Run("876.5");   // Match
//res1.Run("666");     // No Match
```

### Not method
Not method matches pattern if pattern of the argument is not matched.

```csharp
var res1 = MonadPrimus.Str("666").Not()
            .SelectMany(x => MonadPrimus.Real());

//res1.Run("876.5");   // Match
//res1.Run("666");     // No Match
```

### Concat method
Concat method concatenates two Parser delegate.  
The result of value is the value of second argument.

```csharp
var res1 = MonadPrimus.Str("76").Concat(MonadPrimus.Str("5"));

//res1.Run("765");   // Match
```

### ConcatLeft method
Concat method concatenates two Parser delegate.  
The result of value is the value of first argument.

### Local method
Local method changes the environment temporarily.

### GetPosition method
GetPosition method returns the current position as a value.

### PutPosition method
PutPosition method changes the position to the given position and result the current position
as a value.

### ChangeInput method
ChangeInput method changes the input string temporarily.  
ChangeInput is available to including the file.  
The implementation of ChangeInput shows as follows.

```csharp
        public static Parser<T> ChangeInput<T>(Parser<T> parser, string newInput)
        {
                   // set position to 0 and get current position as a value
            return from pos1 in PutPosition(0)
                   // change input string temporarily
                   from result in Local(parser, x => new Env(newInput, x.Skip))
                   // restore position to saved position
                   from pos0 in PutPosition(pos1)
                   select result;
        }
```

### Letrec method
Letrec method can recurse in the method.  
Arguments of Letrec method is a function whose arguments are Parser delegale and
returns a Parser delegate.

```csharp
var expr1 = Letrec<string, string>((x, y) => from a in Str("(")
                                             from b in y.Choice(Str(""))
                                             from c in Str(")")
                                             select a + b + c,
                                   (x, y) => from a in Str("[")
                                             from b in x
                                             from c in Str("]")
                                             select a + b + c);

//expr1("([([()])])");   // Match
//expr1("([([()])]");    // Not Match
```

### Tiny calculator

```csharp
using System;
using System.Linq;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
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
```
