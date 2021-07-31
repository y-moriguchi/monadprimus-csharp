﻿using System;
using System.Linq;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Jack parser from The Elements of Computing Systems (NAND to Tetris)
    /// </summary>
    public class Jack
    {
        static Parser<string> Keyword(string key)
        {
            return Key(key).Select(x => "<keyword> " + x + " </keyword>\n");
        }

        static string Escape(string aString)
        {
            var result = aString;
            result = result.Replace("&", "&amp;");
            result = result.Replace("<", "&lt;");
            result = result.Replace(">", "&gt;");
            return result;
        }

        static Parser<string> Symbol(string symbol)
        {
            return Str(symbol).Select(x => "<symbol> " + Escape(x) + " </symbol>\n");
        }

        static Parser<string> BuildParser()
        {
            var identifier = Regex("[A-Za-z0-9_][A-Za-z0-9_]*").Select(x => "<identifier> " + x + " </identifier>\n");
            var varName = identifier;
            var subroutineName = identifier;
            var className = identifier;
            var integerConstant = Regex("[0-9]+").Select(x => "<integerConstant> " + x + " </integerConstant>\n");
            var stringConstant = Regex("\"[^\"\n]*\"").Select(x => "<stringConstant> " + x.Substring(1, x.Length - 2) + " </stringConstant>\n");
            var keywordConstant = Keyword("true").Choice(Keyword("false")).Choice(Keyword("null")).Choice(Keyword("this"));
            var op = Symbol("+").Choice(Symbol("-")).Choice(Symbol("*")).Choice(Symbol("/"))
                     .Choice(Symbol("&")).Choice(Symbol("|"))
                     .Choice(Symbol("<")).Choice(Symbol(">")).Choice(Symbol("="));
            var unaryOp = Symbol("-").Choice(Symbol("~"));

            var statements0 = Letrec<string>(
                (Parser<string> statements, Parser<string> statement) =>
                {
                    return statement.ZeroOrMore((x, y) => x + y, "");
                },
                (Parser<string> statements, Parser<string> statement) =>
                {
                    return Letrec<string>(
                        (statement0, expression, subroutineCall) =>
                        {
                            var letStatement = from letKey in Keyword("let")
                                               from name0 in varName
                                               from index in (from lbracket in Symbol("[")
                                                              from expr in expression
                                                              from rbracket in Symbol("]")
                                                              select lbracket + expr + rbracket).Option("")
                                               from eq in Symbol("=")
                                               from expr in expression
                                               from semicolon in Symbol(";")
                                               select letKey + name0 + index + eq + expr + semicolon;
                            var elseClause = from elseKey in Keyword("else")
                                             from lbracket1 in Symbol("{")
                                             from stmts in statements
                                             from rbracket1 in Symbol("}")
                                             select elseKey + lbracket1 + stmts + rbracket1;
                            var ifStatement = from ifKey in Keyword("if")
                                              from lparen1 in Symbol("(")
                                              from expr1 in expression
                                              from rparen1 in Symbol(")")
                                              from lbracket1 in Symbol("{")
                                              from stmts in statements
                                              from rbracket1 in Symbol("}")
                                              from elseClause0 in elseClause.Option("")
                                              select ifKey + lparen1 + expr1 + rparen1 + lbracket1 + stmts + rbracket1 + elseClause0;
                            var whileStatement = from whileKey in Keyword("while")
                                                 from lparen1 in Symbol("(")
                                                 from expr1 in expression
                                                 from rparen1 in Symbol(")")
                                                 from lbracket1 in Symbol("{")
                                                 from stmts in statements
                                                 from rbracket1 in Symbol("}")
                                                 select whileKey + lparen1 + expr1 + rparen1 + lbracket1 + stmts + rbracket1;
                            var doStatement = from doKey in Keyword("do")
                                              from subCall in subroutineCall
                                              from semicolon in Symbol(";")
                                              select doKey + subCall + semicolon;
                            var returnStatement = from returnKey in Keyword("return")
                                                  from expr in expression.Option("")
                                                  from semicolon in Symbol(";")
                                                  select returnKey + expr + semicolon;
                            return letStatement.Choice(ifStatement).Choice(whileStatement).Choice(doStatement).Choice(returnStatement);
                        },
                        (statement0, expression, subroutineCall) =>
                        {
                            var term = Letrec<string>(
                                           x => integerConstant
                                                .Choice(stringConstant)
                                                .Choice(keywordConstant)
                                                .Choice(from name in varName
                                                        from lbracket in Symbol("[")
                                                        from expr in expression
                                                        from rbracket in Symbol("]")
                                                        select name + lbracket + expr + rbracket)
                                                .Choice(subroutineCall)
                                                .Choice(varName)
                                                .Choice(from lparen in Symbol("(")
                                                        from expr in expression
                                                        from rparen in Symbol(")")
                                                        select lparen + expr + rparen)
                                                .Choice(from uop in unaryOp
                                                        from term0 in x
                                                        select uop + term0));
                            return term.Delimit(op, (x, op0, y) => x + op0 + y);
                        },
                        (statement0, expression, subroutineCall) =>
                        {
                            var expressionList = expression.Delimit(Symbol(","), (x, op0, y) => x + op0 + y).Option("");

                            return (from name in subroutineName
                                    from lparen in Symbol("(")
                                    from exprList in expressionList
                                    from rparen in Symbol(")")
                                    select name + lparen + exprList + rparen)
                                   .Choice(from clsName in className.Choice(varName)
                                           from dot in Symbol(".")
                                           from name in subroutineName
                                           from lparen in Symbol("(")
                                           from exprList in expressionList
                                           from rparen in Symbol(")")
                                           select clsName + dot + name + lparen + exprList + rparen);
                        });
                });
            var primitiveType = Keyword("int").Choice(Keyword("char")).Choice(Keyword("boolean"));
            var type = primitiveType.Choice(identifier);
            var varDec = from key in Keyword("var")
                         from type0 in type
                         from varNames in varName.Delimit(Symbol(","), (x, op0, y) => x + op0 + y)
                         from semicolon in Symbol(";")
                         select key + type0 + varNames + semicolon;
            var subroutineBody = from lbracket in Symbol("{")
                                 from varDecs in varDec.ZeroOrMore((x, y) => x + y, "")
                                 from stat in statements0
                                 from rbracket in Symbol("}")
                                 select lbracket + varDecs + stat + rbracket;
            var typeName = from type0 in type
                           from name0 in varName
                           select type0 + name0;
            var parameterList = typeName.Delimit(Symbol(","), (x, op0, y) => x + op0 + y);
            var subroutineDec = from key1 in Keyword("constructor").Choice(Keyword("function")).Choice(Keyword("method"))
                                from type0 in Keyword("void").Choice(type)
                                from subName in subroutineName
                                from lparen in Symbol("(")
                                from plist in parameterList.Option("")
                                from rparen in Symbol(")")
                                from subBody in subroutineBody
                                select key1 + type0 + subName + lparen + plist + rparen + subBody;
            var classVarDec = from key in Keyword("static").Choice(Keyword("field"))
                              from type0 in type
                              from varNames in varName.Delimit(Symbol(","), (x, op0, y) => x + op0 + y)
                              from semicolon in Symbol(";")
                              select key + type0 + varNames + semicolon;
            var classe = from classKey in Keyword("class")
                         from name in className
                         from lbracket in Symbol("{")
                         from classVarDecs in classVarDec.ZeroOrMore((x, y) => x + y, "")
                         from subroutineDecs in subroutineDec.ZeroOrMore((x, y) => x + y, "")
                         from rbracket in Symbol("}")
                         select classKey + name + lbracket + classVarDecs + subroutineDecs + rbracket;
            return classe;
        }

        public static string Parse(string input)
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
                return "<tokens>\n" + result.Value + "</tokens>";
            }
        }
    }
}
