using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    public class HackVMTranslator
    {
        int labelno = 1;

        string GetLabel()
        {
            return "LABEL" + (labelno++);
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

        Parser<string> BuildParser()
        {
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
                        from a2 in Key("constant")
                        from num in Regex("[0-9]+").Select(x => int.Parse(x))
                        select Constant(num))
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
