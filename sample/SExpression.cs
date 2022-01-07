/*
 * This source code is under the Unlicense
 */
using System;
using System.Linq;
using System.Text;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// base class of datum.
    /// </summary>
    public abstract class Datum
    {
        public abstract bool IsPair { get; }

        public abstract bool IsNull { get; }

        public abstract object Value { get; }

        public abstract Datum Car { get; }

        public abstract Datum Cdr { get; }
            
        public override string ToString()
        {
            if (IsPair)
            {
                StringBuilder builder = new StringBuilder();
                bool first = true;
                Datum ptr = this;

                builder.Append("(");
                for(; ptr.IsPair; ptr = ptr.Cdr)
                {
                    if(!first)
                    {
                        builder.Append(" ");
                    }
                    builder.Append(ptr.Car.ToString());
                    first = false;
                }
                if (!ptr.IsNull)
                {
                    builder.Append(" . ").Append(ptr.ToString());
                }
                builder.Append(")");
                return builder.ToString();
            }
            else if(IsNull)
            {
                return "()";
            }
            else if(Value is bool && (bool)Value == true)
            {
                return "#t";
            }
            else if (Value is bool && (bool)Value == false)
            {
                return "#f";
            }
            else
            {
                return Value.ToString();
            }
        }
    }

    /// <summary>
    /// Class of cons cell.
    /// </summary>
    public class Cons : Datum
    {
        public Cons(Datum car, Datum cdr)
        {
            Car = car ?? throw new ArgumentNullException();
            Cdr = cdr ?? throw new ArgumentNullException();
        }

        public override bool IsPair => true;

        public override bool IsNull => Car == null;

        public override object Value => throw new InvalidOperationException();

        public override Datum Car { get; }

        public override Datum Cdr { get; }
    }

    /// <summary>
    /// Class of nil.
    /// </summary>
    public class Nil : Datum
    {
        public static Nil Instance = new Nil();

        private Nil() { }

        public override bool IsPair => false;

        public override bool IsNull => true;

        public override object Value => throw new InvalidOperationException();

        public override Datum Car => throw new InvalidOperationException();

        public override Datum Cdr => throw new InvalidOperationException();
    }

    /// <summary>
    /// Class of atom.
    /// </summary>
    public class Atom : Datum
    {
        public Atom(object value)
        {
            Value = value;
        }

        public override bool IsPair => false;

        public override bool IsNull => false;

        public override object Value { get; }

        public override Datum Car => throw new InvalidOperationException();

        public override Datum Cdr => throw new InvalidOperationException();
    }

    /// <summary>
    /// Example: S-Expression parser.
    /// </summary>
    public class SExpression
    {
        public static Datum Parse(string aString)
        {
            var atom = Str("#t").Select(d => (Datum)new Atom(true))
                       .Choice(Str("#f").Select(d => (Datum)new Atom(false)))
                       .Choice(Regex(@"[^\s\(\)#\.]+").Select(d => (Datum)new Atom(d)));
            var expr = Letrec<Datum>((x, y) => (from a in Str("(")
                                                from b in y.Option(Nil.Instance)
                                                from c in Str(")")
                                                select b)
                                               .Choice(atom),
                                     (x, y) => (from a in x
                                                from b in (from d in Str(".")
                                                           from c in x
                                                           select c)
                                                          .Choice(y.Option(Nil.Instance))
                                                select (Datum)new Cons(a, b)));
            var skip1 = Regex(@"[\s]+")
                        .Choice(from a in Str("#;")
                                from b in Letrec<string>(x => from _a in Regex(@"[\s]*\(")
                                                              from _b in atom.Select(_d => "").Choice(Regex(@"[\.\s]+")).ZeroOrMore()
                                                              from _c in x.Option()
                                                              from _d in atom.Select(_d => "").Choice(Regex(@"[\.\s]+")).ZeroOrMore()
                                                              from _e in Str(")")
                                                              select _b)
                                          .Choice(atom.Select(_x => ""))
                                select b)
                        .Choice(Letrec<string>((x, y) => from a in Str("#|")
                                                         from b in y
                                                         from c in Str("|#")
                                                         select c,
                                               (x, y) => Str("|#").Lookahead()
                                                         .Choice(from a in x.Choice(Regex("[\\s\\S]"))
                                                                 from b in y
                                                                 select b)));
            var skip = skip1.OneOrMore();

            var result = expr.Run(aString, skip);
            if(result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }
            else
            {
                return expr.Run(aString, skip).Value;
            }
        }
    }
}
