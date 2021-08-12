/*
 * This source code is under the Unlicense
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Jack compiler from The Elements of Computing Systems (NAND to Tetris)
    /// </summary>
    public class JackAnalyzer
    {
        enum Kind
        {
            STATIC, FIELD, ARG, VAR
        }

        class SymbolInfo
        {
            public string Type { get; private set; }
            public Kind Kind { get; private set; }
            public int No { get; private set; }

            public SymbolInfo(string type, Kind kind, int no)
            {
                Type = type;
                Kind = kind;
                No = no;
            }
        }

        class SymbolNotFoundException : Exception
        {
            public SymbolNotFoundException(string msg) : base(msg) { }
        }

        class SymbolTable
        {
            Dictionary<string, SymbolInfo> classTable = new Dictionary<string, SymbolInfo>();
            Dictionary<string, SymbolInfo> subroutineTable = new Dictionary<string, SymbolInfo>();
            private readonly int[] kindNo = new int[4];

            public SymbolTable()
            {
                kindNo[0] = kindNo[1] = kindNo[2] = kindNo[3] = 0;
            }

            public void StartSubroutine()
            {
                kindNo[(int)Kind.ARG] = kindNo[(int)Kind.VAR] = 0;
                subroutineTable = new Dictionary<string, SymbolInfo>();
            }

            public void Define(string name, string type, Kind kind)
            {
                if(kind == Kind.STATIC || kind == Kind.FIELD)
                {
                    classTable[name] = new SymbolInfo(type, kind, kindNo[(int)kind]++);
                }
                else
                {
                    subroutineTable[name] = new SymbolInfo(type, kind, kindNo[(int)kind]++);
                }
            }

            public int VarCount(Kind kind)
            {
                return kindNo[(int)kind];
            }

            SymbolInfo GetInfo(string name)
            {
                if (classTable.ContainsKey(name))
                {
                    return classTable[name];
                }
                else if(subroutineTable.ContainsKey(name))
                {
                    return subroutineTable[name];
                }
                else
                {
                    throw new SymbolNotFoundException("symbol " + name + " is not defined");
                }
            }

            public Kind KindOf(string name)
            {
                return GetInfo(name).Kind;
            }

            public string TypeOf(string name)
            {
                return GetInfo(name).Type;
            }

            public int IndexOf(string name)
            {
                return GetInfo(name).No;
            }
        }

        interface IExpr
        {
            string PutVMCode();
        }

        class NullExpr : IExpr
        {
            public string PutVMCode()
            {
                return "push constant 0\n";
            }
        }


        class ThisExpr : IExpr
        {
            public string PutVMCode()
            {
                return "push pointer 0\n";
            }
        }


        class BoolExpr : IExpr
        {
            public bool Value { get; private set; }

            public BoolExpr(bool value)
            {
                Value = value;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                if(Value)
                {
                    builder.Append("push constant 1\n");
                    builder.Append("neg\n");
                }
                else
                {
                    builder.Append("push constant 0\n");
                }
                return builder.ToString();
            }
        }

        class StringConst : IExpr
        {
            public string Value { get; private set; }

            public StringConst(string aString)
            {
                Value = aString;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                builder.Append("push constant " + Value.Length + "\n");
                builder.Append("call String.new 1\n");
                for(var i = 0; i < Value.Length; i++)
                {
                    builder.Append("push constant " + (int)Value[i] + "\n");
                    builder.Append("call String.appendChar 2\n");
                }
                return builder.ToString();
            }
        }

        class IntConst : IExpr
        {
            public int Value { get; private set; }

            public IntConst(int value)
            {
                Value = value;
            }

            public string PutVMCode()
            {
                return "push constant " + Value + "\n";
            }
        }

        class ArrayIndex : IExpr
        {
            public string ArrayName { get; private set; }
            public IExpr Expr { get; private set; }
            readonly SymbolTable symbolTable;

            public ArrayIndex(string name, IExpr expr, SymbolTable table)
            {
                ArrayName = name;
                Expr = expr;
                symbolTable = table;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                builder.Append(PushPopRefer(symbolTable, "push", ArrayName));
                builder.Append(Expr.PutVMCode());
                builder.Append("add\n");
                builder.Append("pop pointer 1\n");
                builder.Append("push that 0\n");
                return builder.ToString();
            }
        }

        class UnaryOp : IExpr
        {
            public string Op { get; private set; }
            public IExpr Expr { get; private set; }

            public UnaryOp(string op, IExpr expr)
            {
                Op = op;
                Expr = expr;
            }

            public string PutVMCode()
            {
                return Expr.PutVMCode() + Op + "\n";
            }
        }

        class BinaryOp : IExpr
        {
            public string Op { get; private set; }
            public IExpr Expr1 { get; private set; }
            public IExpr Expr2 { get; private set; }

            public BinaryOp(IExpr expr1, string op, IExpr expr2)
            {
                Expr1 = expr1;
                Op = op;
                Expr2 = expr2;
            }

            public string PutVMCode()
            {
                return Expr1.PutVMCode() + Expr2.PutVMCode() + Op + "\n";
            }
        }

        class ReferVar : IExpr
        {
            public string Name { get; private set; }
            readonly SymbolTable symbolTable;

            public ReferVar(string name, SymbolTable table)
            {
                Name = name;
                symbolTable = table;
            }

            public string PutVMCode()
            {
                return PushPopRefer(symbolTable, "push", Name);
            }
        }

        class Call : IExpr
        {
            public string Name { get; private set; }
            public string ClassName { get; private set; }
            public IEnumerable<IExpr> Args { get; private set; }
            readonly SymbolTable symbolTable;
            readonly bool inner;

            public Call(string name, string className, IEnumerable<IExpr> args, bool _inner, SymbolTable table)
            {
                Name = name;
                ClassName = className;
                Args = args;
                symbolTable = table;
                inner = _inner;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();
                string type;
                bool instanceMethod = false;

                try
                {
                    type = symbolTable.TypeOf(ClassName);
                    instanceMethod = true;
                }
                catch(SymbolNotFoundException)
                {
                    type = ClassName;
                }

                if(instanceMethod || inner)
                {
                    builder.Append("push pointer 0\n");
                    if(inner)
                    {
                        builder.Append("push pointer 0\n");
                    }
                    else
                    {
                        builder.Append(PushPopRefer(symbolTable, "push", ClassName));
                    }
                    builder.Append(PutVMCodes(Args));
                    builder.Append("call " + type + "." + Name + " " + (Args.Count() + 1) + "\n");
                    builder.Append("pop temp 1\n");
                    builder.Append("pop pointer 0\n");
                    builder.Append("push temp 1\n");
                }
                else
                {
                    builder.Append(PutVMCodes(Args));
                    builder.Append("call " + type + "." + Name + " " + Args.Count() + "\n");
                }
                return builder.ToString();
            }
        }

        class Let : IExpr
        {
            public string VarName { get; private set; }
            public IExpr Index { get; private set; }
            public IExpr RValue { get; private set; }
            private SymbolTable symbolTable;

            public Let(string varName, IExpr index, IExpr rValue, SymbolTable table)
            {
                VarName = varName;
                Index = index;
                RValue = rValue;
                symbolTable = table;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                if (Index == null)
                {
                    builder.Append(RValue.PutVMCode());
                    builder.Append(PushPopRefer(symbolTable, "pop", VarName));
                }
                else
                {
                    builder.Append(PushPopRefer(symbolTable, "push", VarName));
                    builder.Append(Index.PutVMCode());
                    builder.Append("add\n");
                    builder.Append(RValue.PutVMCode());
                    builder.Append("pop temp 2\n");
                    builder.Append("pop pointer 1\n");
                    builder.Append("push temp 2\n");
                    builder.Append("pop that 0\n");
                }
                return builder.ToString();
            }
        }

        class IfStmt : IExpr
        {
            public IExpr Cond { get; private set; }
            public IEnumerable<IExpr> IfClause { get; private set; }
            public IEnumerable<IExpr> ElseClause { get; private set; }

            public IfStmt(IExpr cond, IEnumerable<IExpr> ifClause, IEnumerable<IExpr> elseClause)
            {
                Cond = cond;
                IfClause = ifClause;
                ElseClause = elseClause;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();
                var label1 = GetLabel();
                var label2 = GetLabel();

                builder.Append(Cond.PutVMCode());
                builder.Append("if-goto " + label1 + "\n");
                builder.Append(PutVMCodes(ElseClause));
                builder.Append("goto " + label2 + "\n");
                builder.Append("label " + label1 + "\n");
                builder.Append(PutVMCodes(IfClause));
                builder.Append("label " + label2 + "\n");
                return builder.ToString();
            }
        }

        class WhileStmt : IExpr
        {
            public IExpr Cond { get; private set; }
            public IEnumerable<IExpr> Stmts { get; private set; }

            public WhileStmt(IExpr cond, IEnumerable<IExpr> stmts)
            {
                Cond = cond;
                Stmts = stmts;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();
                var label1 = GetLabel();
                var label2 = GetLabel();
                var label3 = GetLabel();

                builder.Append("label " + label1 + "\n");
                builder.Append(Cond.PutVMCode());
                builder.Append("if-goto " + label3 + "\n");
                builder.Append("goto " + label2 + "\n");
                builder.Append("label " + label3 + "\n");
                builder.Append(PutVMCodes(Stmts));
                builder.Append("goto " + label1 + "\n");
                builder.Append("label " + label2 + "\n");
                return builder.ToString();
            }
        }

        class DoStmt : IExpr
        {
            public IExpr Call { get; private set; }

            public DoStmt(IExpr call)
            {
                Call = call;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                builder.Append(Call.PutVMCode());
                builder.Append("pop temp 0\n");
                return builder.ToString();
            }
        }

        class ReturnStmt : IExpr
        {
            public IExpr Expr { get; private set; }

            public ReturnStmt(IExpr expr)
            {
                Expr = expr;
            }

            public string PutVMCode()
            {
                var builder = new StringBuilder();

                if(Expr == null)
                {
                    builder.Append("push constant 0\n");
                }
                else
                {
                    builder.Append(Expr.PutVMCode());
                }
                builder.Append("return\n");
                return builder.ToString();
            }
        }

        static int labelNo = 0;
        static readonly NullExpr Null = new NullExpr();
        static readonly ThisExpr This = new ThisExpr();
        static readonly BoolExpr True = new BoolExpr(true);
        static readonly BoolExpr False = new BoolExpr(false);

        SymbolTable symbolTable;
        string thisClassName;
        int fields = 0;

        static string GetLabel()
        {
            return "LABEL:" + labelNo++;
        }

        static string PushPopRefer(SymbolTable symbolTable, string pushPop, string name)
        {
            switch (symbolTable.KindOf(name))
            {
                case Kind.STATIC: return pushPop + " static " + symbolTable.IndexOf(name) + "\n";
                case Kind.FIELD: return pushPop + " this " + symbolTable.IndexOf(name) + "\n";
                case Kind.ARG: return pushPop + " argument " + symbolTable.IndexOf(name) + "\n";
                case Kind.VAR: return pushPop + " local " + symbolTable.IndexOf(name) + "\n";
                default: throw new Exception("internal error");
            }
        }

        static string PutVMCodes(IEnumerable<IExpr> exprs)
        {
            var builder = new StringBuilder();

            foreach(var expr in exprs)
            {
                builder.Append(expr.PutVMCode());
            }
            return builder.ToString();
        }

        Parser<string> Symbol(string symbol)
        {
            return Str(symbol);
        }

        string DefineArg(string type, string name)
        {
            symbolTable.Define(name, type, Kind.ARG);
            return "";
        }

        int DefineVariable(string type, IEnumerable<string> varNames, Kind kind)
        {
            foreach (var varName in varNames)
            {
                symbolTable.Define(varName, type, kind);
            }
            return varNames.Count();
        }

        string StartSubroutine(string key)
        {
            symbolTable.StartSubroutine();
            if(key == "method")
            {
                symbolTable.Define("", "int", Kind.ARG);
            }
            return "";
        }

        string InitClass(string name)
        {
            symbolTable = new SymbolTable();
            return thisClassName = name;
        }

        string CreateSubroutine(string key, string name, IEnumerable<IExpr> stmt, int localVars)
        {
            var builder = new StringBuilder();

            builder.Append("function " + thisClassName + "." + name + " " + localVars + "\n");
            if (key == "constructor")
            {
                builder.Append("push constant " + fields + "\n");
                builder.Append("call Memory.alloc 1\n");
                builder.Append("pop pointer 0\n");
            }
            else if(key == "method")
            {
                builder.Append("push argument 0\n");
                builder.Append("pop pointer 0\n");
            }
            builder.Append(PutVMCodes(stmt));
            return builder.ToString();
        }

        Parser<string> BuildParser()
        {
            var identifier = Regex("[A-Za-z0-9_][A-Za-z0-9_]*");
            var varName = identifier;
            var subroutineName = identifier;
            var className = identifier;
            var integerConstant = Regex("[0-9]+").Select(x => int.Parse(x));
            var stringConstant = Regex("\"[^\"\n]*\"").Select(x => x.Substring(1, x.Length - 2));
            var keywordConstant = Key("true").Select(x => (IExpr)True)
                                  .Choice(Key("false").Select(x => (IExpr)False))
                                  .Choice(Key("null").Select(x => (IExpr)Null))
                                  .Choice(Key("this").Select(x => (IExpr)This));
            var op = Symbol("+").Select(x => "add")
                     .Choice(Symbol("-").Select(x => "sub"))
                     .Choice(Symbol("*").Select(x => "call Math.multiply 2"))
                     .Choice(Symbol("/").Select(x => "call Math.divide 2"))
                     .Choice(Symbol("&").Select(x => "and"))
                     .Choice(Symbol("|").Select(x => "or"))
                     .Choice(Symbol("<").Select(x => "lt"))
                     .Choice(Symbol(">").Select(x => "gt"))
                     .Choice(Symbol("=").Select(x => "eq"));
            var unaryOp = Symbol("-").Select(x => "neg").Choice(Symbol("~").Select(x => "not"));

            var statements0 = Letrec(
                (Parser<IEnumerable<IExpr>> statements, Parser<IExpr> statement) =>
                {
                    return statement.Select(x => new IExpr[] { x }.AsEnumerable())
                           .ZeroOrMore((x, y) => x.Concat(y), new IExpr[0]);
                },
                (Parser<IEnumerable<IExpr>> statements, Parser<IExpr> statement) =>
                {
                    return Letrec<IExpr>(
                        (Parser<IExpr> statement0, Parser<IExpr> expression, Parser<IExpr> subroutineCall) =>
                        {
                            var letStatement = from letKey in Key("let")
                                               from name0 in varName
                                               from index in (from lbracket in Symbol("[")
                                                              from expr in expression
                                                              from rbracket in Symbol("]")
                                                              select expr).Option(null)
                                               from eq in Symbol("=")
                                               from expr in expression
                                               from semicolon in Symbol(";")
                                               select (IExpr)new Let(name0, index, expr, symbolTable);
                            var elseClause = from elseKey in Key("else")
                                             from lbracket1 in Symbol("{")
                                             from stmts in statements
                                             from rbracket1 in Symbol("}")
                                             select stmts;
                            var ifStatement = from ifKey in Key("if")
                                              from lparen1 in Symbol("(")
                                              from expr1 in expression
                                              from rparen1 in Symbol(")")
                                              from lbracket1 in Symbol("{")
                                              from stmts in statements
                                              from rbracket1 in Symbol("}")
                                              from elseClause0 in elseClause.Option(new IExpr[0])
                                              select (IExpr)new IfStmt(expr1, stmts, elseClause0);
                            var whileStatement = from whileKey in Key("while")
                                                 from lparen1 in Symbol("(")
                                                 from expr1 in expression
                                                 from rparen1 in Symbol(")")
                                                 from lbracket1 in Symbol("{")
                                                 from stmts in statements
                                                 from rbracket1 in Symbol("}")
                                                 select (IExpr)new WhileStmt(expr1, stmts);
                            var doStatement = from doKey in Key("do")
                                              from subCall in subroutineCall
                                              from semicolon in Symbol(";")
                                              select (IExpr)new DoStmt(subCall);
                            var returnStatement = from returnKey in Key("return")
                                                  from expr in expression.Option(null)
                                                  from semicolon in Symbol(";")
                                                  select (IExpr)new ReturnStmt(expr);
                            return letStatement.Choice(ifStatement).Choice(whileStatement).Choice(doStatement).Choice(returnStatement);
                        },
                        (Parser<IExpr> statement0, Parser<IExpr> expression, Parser<IExpr> subroutineCall) =>
                        {
                            var term = Letrec<IExpr>(
                                           x => from t in integerConstant.Select(a => (IExpr)new IntConst(a))
                                                          .Choice(stringConstant.Select(a => (IExpr)new StringConst(a)))
                                                          .Choice(keywordConstant)
                                                          .Choice(from name in varName
                                                                  from lbracket in Symbol("[")
                                                                  from expr in expression
                                                                  from rbracket in Symbol("]")
                                                                  select (IExpr)new ArrayIndex(name, expr, symbolTable))
                                                          .Choice(subroutineCall)
                                                          .Choice(varName.Select(a => (IExpr)new ReferVar(a, symbolTable)))
                                                          .Choice(from lparen in Symbol("(")
                                                                  from expr in expression
                                                                  from rparen in Symbol(")")
                                                                  select expr)
                                                          .Choice(from uop in unaryOp
                                                                  from term0 in x
                                                                  select (IExpr)new UnaryOp(uop, term0))
                                                select t);

                            return term.Delimit(op, (x, op0, y) => new BinaryOp(x, op0, y));
                        },
                        (Parser<IExpr> statement0, Parser<IExpr> expression, Parser<IExpr> subroutineCall) =>
                        {
                            var expressionList = expression.Select(x => new IExpr[] { x }.AsEnumerable())
                                                 .Delimit(Symbol(","), (x, op0, y) => x.Concat(y)).Option(new IExpr[0]);

                        return (from name in subroutineName
                                from lparen in Symbol("(")
                                from exprList in expressionList
                                from rparen in Symbol(")")
                                select (IExpr)new Call(name, thisClassName, exprList, true, symbolTable))
                               .Choice(from clsName in className.Choice(varName)
                                       from dot in Symbol(".")
                                       from name in subroutineName
                                       from lparen in Symbol("(")
                                       from exprList in expressionList
                                       from rparen in Symbol(")")
                                       select (IExpr)new Call(name, clsName, exprList, false, symbolTable));
                        });
                });
            var primitiveType = Key("int").Choice(Key("char")).Choice(Key("boolean"));
            var type = primitiveType.Choice(identifier);
            var varDec = from key in Key("var")
                         from type0 in type
                         from varNames in varName.Select(x => new string[] { x }.AsEnumerable()).Delimit(Symbol(","), (x, op0, y) => x.Concat(y))
                         from semicolon in Symbol(";")
                         select DefineVariable(type0, varNames, Kind.VAR);
            var subroutineBody = from lbracket in Symbol("{")
                                 from varDecs in varDec.ZeroOrMore((x, y) => x + y, 0)
                                 from stat in statements0
                                 from rbracket in Symbol("}")
                                 select new Tuple<IEnumerable<IExpr>, int>(stat, varDecs);
            var typeName = from type0 in type
                           from name0 in varName
                           select DefineArg(type0, name0);
            var parameterList = from lst in typeName.Delimit(Symbol(","), (x, op0, y) => "").Option("")
                                select lst;
            var subroutineDec = from key1 in Key("constructor").Choice(Key("function")).Choice(Key("method"))
                                from type0 in Key("void").Choice(type)
                                let start = StartSubroutine(key1)
                                from subName in subroutineName
                                from lparen in Symbol("(")
                                from plist in parameterList
                                from rparen in Symbol(")")
                                from subBody in subroutineBody
                                select CreateSubroutine(key1, subName, subBody.Item1, subBody.Item2);
            var classVarDec = from key in Key("static").Select(x => Kind.STATIC).Choice(Key("field").Select(x => Kind.FIELD))
                              from type0 in type
                              from varNames in varName.Select(x => new string[] { x }.AsEnumerable()).Delimit(Symbol(","), (x, op0, y) => x.Concat(y))
                              from semicolon in Symbol(";")
                              let args = DefineVariable(type0, varNames, key)
                              select key == Kind.FIELD ? args : 0;
            var classe = from classKey in Key("class")
                         from name in className.Select(x => InitClass(x))
                         from lbracket in Symbol("{")
                         from classVarDecs in classVarDec.ZeroOrMore((x, y) => x + y, 0).Select(x => fields = x)
                         from subroutineDecs in subroutineDec.ZeroOrMore((x, y) => x + y, "")
                         from rbracket in Symbol("}")
                         select subroutineDecs;
            return classe;
        }

        public string Parse(string input)
        {
            string comment = "([ \t\n]+|//[^\n]*\n|/\\*[\\s\\S]*?\\*/)+";
            var jackParser = BuildParser();
            var result = jackParser.Run(input, comment, "[;\\)]");
            
            if(result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }
            else
            {
                return result.Value;
            }
        }
    }
}
