/*
 * This source code is under the Unlicense
 */
using System;
using System.Text;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Hack VM Translator from The Elements of Computing Systems (nand2tetris)
    /// </summary>
    public class HackVMTranslator
    {
        const int THIS = 3;
        const int TEMP = 5;

        static readonly Parser<string> Label = Regex("[A-Za-z_\\.\\:][A-Za-z0-9_\\.\\:]*");
        static readonly Parser<int> Offset = Regex("[0-9]+").Select(x => int.Parse(x));
        static readonly Parser<string> ReferKeys = Key("local").Select(x => "LCL")
                                                   .Choice(Key("argument").Select(x => "ARG"))
                                                   .Choice(Key("this").Select(x => "THIS"))
                                                   .Choice(Key("that").Select(x => "THAT"));
        static readonly Parser<int> PointerKeys = Key("pointer").Select(x => THIS)
                                                  .Choice(Key("temp").Select(x => TEMP));
        int labelno = 1;
        static string functionName = "";

        string GetLabel()
        {
            return "LABEL$" + labelno++;
        }

        string GetInnerLabel(string label)
        {
            return functionName + "$" + label;
        }

        string GetStaticLabel(string label)
        {
            var className = functionName.Split('.')[0];

            return className + "." + label;
        }

        Func<string, string> Arithmetic1(string op)
        {
            return x =>
            {
                var builder = new StringBuilder();

                builder.Append("@SP\n");
                builder.Append("A=M-1\n");
                builder.Append("D=M\n");
                builder.Append("MD=" + op + "\n");
                return builder.ToString();
            };
        }

        Func<string, string> Arithmetic2(string op)
        {
            return x =>
            {
                var builder = new StringBuilder();

                builder.Append("@SP\n");
                builder.Append("AM=M-1\n");
                builder.Append("D=M\n");
                builder.Append("A=A-1\n");
                builder.Append("MD=" + op + "\n");
                return builder.ToString();
            };
        }

        Func<string, string> Compare2(string op)
        {
            return x =>
            {
                var builder = new StringBuilder();
                var label1 = GetLabel();
                var label2 = GetLabel();

                builder.Append("@SP\n");
                builder.Append("AM=M-1\n");
                builder.Append("D=M\n");
                builder.Append("A=A-1\n");
                builder.Append("D=M-D\n");
                builder.Append("@" + label1 + "\n");
                builder.Append("D;" + op + "\n");
                builder.Append("@SP\n");
                builder.Append("A=M-1\n");
                builder.Append("M=0\n");
                builder.Append("@" + label2 + "\n");
                builder.Append("0;JMP\n");
                builder.Append("(" + label1 + ")\n");
                builder.Append("@SP\n");
                builder.Append("A=M-1\n");
                builder.Append("M=-1\n");
                builder.Append("(" + label2 + ")\n");
                return builder.ToString();
            };
        }

        string Constant(int num)
        {
            var builder = new StringBuilder();

            builder.Append("@" + num + "\n");
            builder.Append("D=A\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
            return builder.ToString();
        }

        string ReferPush(string key, int num)
        {
            var builder = new StringBuilder();

            builder.Append("@" + key + "\n");
            builder.Append("D=M\n");
            builder.Append("@" + num + "\n");
            builder.Append("A=D+A\n");
            builder.Append("D=M\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
            return builder.ToString();
        }

        string ReferPop(string key, int num)
        {
            var builder = new StringBuilder();

            builder.Append("@SP\n");
            builder.Append("AM=M-1\n");
            builder.Append("D=M\n");
            builder.Append("@R13\n");
            builder.Append("M=D\n");
            builder.Append("@" + key + "\n");
            builder.Append("D=M\n");
            builder.Append("@" + num + "\n");
            builder.Append("D=D+A\n");
            builder.Append("@R14\n");
            builder.Append("M=D\n");
            builder.Append("@R13\n");
            builder.Append("D=M\n");
            builder.Append("@R14\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            return builder.ToString();
        }

        string PointerPush(int start, int offset)
        {
            var builder = new StringBuilder();

            builder.Append("@" + (start + offset) + "\n");
            builder.Append("D=M\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
            return builder.ToString();
        }

        string PointerPop(int start, int offset)
        {
            var builder = new StringBuilder();

            builder.Append("@SP\n");
            builder.Append("AM=M-1\n");
            builder.Append("D=M\n");
            builder.Append("@" + (start + offset) + "\n");
            builder.Append("M=D\n");
            return builder.ToString();
        }

        string StaticPush(int lbl)
        {
            var builder = new StringBuilder();

            builder.Append("@" + GetStaticLabel(lbl.ToString()) + "\n");
            builder.Append("D=M\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
            return builder.ToString();
        }

        string StaticPop(int lbl)
        {
            var builder = new StringBuilder();

            builder.Append("@SP\n");
            builder.Append("AM=M-1\n");
            builder.Append("D=M\n");
            builder.Append("@" + GetStaticLabel(lbl.ToString()) + "\n");
            builder.Append("M=D\n");
            return builder.ToString();
        }

        string IfGoto(string label)
        {
            var builder = new StringBuilder();

            builder.Append("@SP\n");
            builder.Append("AM=M-1\n");
            builder.Append("D=M\n");
            builder.Append("@" + GetInnerLabel(label) + "\n");
            builder.Append("D;JNE\n");
            return builder.ToString();
        }

        string DefineFunction(string f, int n)
        {
            var builder = new StringBuilder();

            functionName = f;
            builder.Append("(" + f + ")\n");
            for(int i = 0; i < n; i++)
            {
                builder.Append("@LCL\n");
                builder.Append("D=M\n");
                builder.Append("@" + i + "\n");
                builder.Append("A=D+A\n");
                builder.Append("M=0\n");
            }
            return builder.ToString();
        }

        void PushLabel(StringBuilder builder, string label)
        {
            builder.Append("@" + label + "\n");
            builder.Append("D=M\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
        }

        public string CallFunction(string f, int n)
        {
            var builder = new StringBuilder();
            var returnLabel = GetLabel();

            builder.Append("@" + returnLabel + "\n");
            builder.Append("D=A\n");
            builder.Append("@SP\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("M=M+1\n");
            PushLabel(builder, "LCL");
            PushLabel(builder, "ARG");
            PushLabel(builder, "THIS");
            PushLabel(builder, "THAT");
            builder.Append("@SP\n");
            builder.Append("D=M\n");
            builder.Append("@" + (n + 5) + "\n");
            builder.Append("D=D-A\n");
            builder.Append("@ARG\n");
            builder.Append("M=D\n");
            builder.Append("@SP\n");
            builder.Append("D=M\n");
            builder.Append("@LCL\n");
            builder.Append("M=D\n");
            builder.Append("@" + f + "\n");
            builder.Append("0;JMP\n");
            builder.Append("(" + returnLabel + ")\n");
            return builder.ToString();
        }

        void RestoreLabel(StringBuilder builder, string label, int offset)
        {
            builder.Append("@LCL\n");
            builder.Append("D=M\n");
            builder.Append("@" + offset + "\n");
            builder.Append("A=D-A\n");
            builder.Append("D=M\n");
            builder.Append("@" + label + "\n");
            builder.Append("M=D\n");
        }

        string ReturnFunction()
        {
            var builder = new StringBuilder();

            RestoreLabel(builder, "15", 5);
            builder.Append("@SP\n");
            builder.Append("M=M-1\n");
            builder.Append("A=M\n");
            builder.Append("D=M\n");
            builder.Append("@ARG\n");
            builder.Append("A=M\n");
            builder.Append("M=D\n");
            builder.Append("@ARG\n");
            builder.Append("D=M+1\n");
            builder.Append("@SP\n");
            builder.Append("M=D\n");
            RestoreLabel(builder, "THAT", 1);
            RestoreLabel(builder, "THIS", 2);
            RestoreLabel(builder, "ARG", 3);
            RestoreLabel(builder, "LCL", 4);
            builder.Append("@15\n");
            builder.Append("A=M\n");
            builder.Append("0;JMP\n");
            return builder.ToString();
        }

        Parser<string> BuildPush()
        {
            var constant = from a2 in Key("constant")
                           from num in Offset
                           select Constant(num);
            var refer = from key in ReferKeys
                        from num in Offset
                        select ReferPush(key, num);
            var pointer = from key in PointerKeys
                          from num in Offset
                          select PointerPush(key, num);
            var staticDef = from a2 in Key("static")
                            from lbl in Offset
                            select StaticPush(lbl);

            return constant.Choice(refer).Choice(pointer).Choice(staticDef);
        }

        Parser<string> BuildPop()
        {
            var refer = from key in ReferKeys
                        from num in Offset
                        select ReferPop(key, num);
            var pointer = from key in PointerKeys
                          from num in Offset
                          select PointerPop(key, num);
            var staticDef = from a2 in Key("static")
                            from lbl in Offset
                            select StaticPop(lbl);

            return refer.Choice(pointer).Choice(staticDef);
        }

        Parser<string> BuildParser()
        {
            var push = BuildPush();
            var pop = BuildPop();
            var mnemonic =
                Key("add").Select(Arithmetic2("D+M"))
                .Choice(Key("sub").Select(Arithmetic2("M-D")))
                .Choice(Key("neg").Select(Arithmetic1("-D")))
                .Choice(Key("eq").Select(Compare2("JEQ")))
                .Choice(Key("gt").Select(Compare2("JGT")))
                .Choice(Key("lt").Select(Compare2("JLT")))
                .Choice(Key("and").Select(Arithmetic2("D&M")))
                .Choice(Key("or").Select(Arithmetic2("D|M")))
                .Choice(Key("not").Select(Arithmetic1("!D")))
                .Choice(from a1 in Key("push")
                        from asm in push
                        select asm)
                .Choice(from a1 in Key("pop")
                        from asm in pop
                        select asm)
                .Choice(from v in Key("label")
                        from lbl in Label
                        select "(" + GetInnerLabel(lbl) + ")\n")
                .Choice(from v in Key("goto")
                        from lbl in Label
                        select "@" + GetInnerLabel(lbl) + "\n0;JMP\n")
                .Choice(from v in Key("if-goto")
                        from lbl in Label
                        select IfGoto(lbl))
                .Choice(from v in Key("function")
                        from f in Label
                        from k in Offset
                        select DefineFunction(f, k))
                .Choice(from v in Key("call")
                        from f in Label
                        from n in Offset
                        select CallFunction(f, n))
                .Choice(Key("return").Select(x => ReturnFunction()))
                .Choice(Str(""));

            return from a in mnemonic
                   from b in End()
                   select a;
        }

        public string Translate(string input)
        {
            var parser = BuildParser();
            var skip = "([ \t]+|//[^\n]*)+";
            var lines = input.Split('\n');
            var builder = new StringBuilder();

            foreach (var line in lines)
            {
                if (line != "\n")
                {
                    var lineResult = parser.Run(line, skip);

                    if (lineResult.IsError)
                    {
                        throw new Exception(lineResult.ErrorMessage);
                    }
                    else
                    {
                        builder.Append(lineResult.Value);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
