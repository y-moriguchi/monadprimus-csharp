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
var result = monad(961);

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
            from c in a + b < 10 ? exit(961) : 6.ToCont<int, int>()
            from d in 5.ToCont<int, int>()
            select a + b + c + d);
var m12 = MonadPrimus.CallCC<int, int>(
    exit => from a in 8.ToCont<int, int>()
            from b in 7.ToCont<int, int>()
            from c in a + b < 10 ? exit(961) : 6.ToCont<int, int>()
            from d in 5.ToCont<int, int>()
            select a + b + c + d);

// m11(x => x) = 961
// m12(x => x) = 26
```

## Parser

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
