using System;
using System.Collections.Generic;
using static Morilib.MonadPrimus;

namespace Morilib.Sample
{
    /// <summary>
    /// Hack Assembler from The Elements of Computing Systems (NAND to Tetris)
    /// </summary>
    public static class HackAssembler
    {
        class SymbolTable
        {
            private Dictionary<string, int> table = new Dictionary<string, int>();

            public SymbolTable()
            {
                table.Add("SP", 0);
                table.Add("LCL", 1);
                table.Add("ARG", 2);
                table.Add("THIS", 3);
                table.Add("THAT", 4);
                for(int i = 0; i <= 15; i++)
                {
                    table.Add("R" + i, i);
                }
                table.Add("SCREEN", 16384);
                table.Add("KBD", 24576);
            }

            public void AddEntry(string name, int value)
            {
                table[name] = value;
            }

            public bool Contains(string name)
            {
                return table.ContainsKey(name);
            }

            public int GetAddress(string name)
            {
                if(!table.ContainsKey(name))
                {
                    throw new Exception("Symbol not defined");
                }
                return table[name];
            }
        }

        static readonly SymbolTable table = new SymbolTable();

        static Parser<string> BuildParserPass1()
        {
            var aInstruction = from at in Str("@")
                               from val in Regex("[0-9]+").Select(x => "")
                                           .Choice(Regex("[A-Za-z\\.\\$\\:_][A-Za-z\\.\\$\\:_0-9]*").Select(x => "@" + x))
                               select val;
            var dest = (from code in Str("AMD").Choice(Str("AM")).Choice(Str("AD")).Choice(Str("MD"))
                                     .Choice(Str("M")).Choice(Str("D")).Choice(Str("A"))
                        from eq in Str("=")
                        select "").Option("");
            var comp = Str("0").Choice(Str("1")).Choice(Str("-1")).Choice(Str("!D")).Choice(Str("!A")).Choice(Str("-D"))
                       .Choice(Str("-A")).Choice(Str("D+1")).Choice(Str("A+1")).Choice(Str("D-1")).Choice(Str("A-1"))
                       .Choice(Str("-A")).Choice(Str("D+A")).Choice(Str("D-A")).Choice(Str("A-D")).Choice(Str("D&A"))
                       .Choice(Str("D|A")).Choice(Str("!M")).Choice(Str("-M")).Choice(Str("-M")).Choice(Str("M+1"))
                       .Choice(Str("M-1")).Choice(Str("D+M")).Choice(Str("D-M")).Choice(Str("M-D")).Choice(Str("D&M"))
                       .Choice(Str("D|M")).Choice(Str("D")).Choice(Str("A")).Choice(Str("M"));
            var jump = (from sc in Str(";")
                        from code in Str("JGT").Choice(Str("JEQ")).Choice(Str("JGE")).Choice(Str("JLT")).Choice(Str("JNE"))
                                     .Choice(Str("JLE")).Choice(Str("JMP"))
                        select "").Option("");
            var cInstruction = from d in dest
                               from c in comp
                               from j in jump
                               select "";
            var label = from lparen in Str("(")
                         from sym in Regex("[A-Za-z\\.\\$\\:_][A-Za-z\\.\\$\\:_0-9]*")
                         from rparen in Str(")")
                         select sym;
            var instruction = aInstruction.Choice(cInstruction).Choice(label).Choice(End().Select(x => (string)null));
            return instruction;
        }

        static Parser<int> BuildParserPass2()
        {
            var aInstruction = from at in Str("@")
                               from val in Regex("[0-9]+").Select(x => int.Parse(x))
                                           .Choice(Regex("[A-Za-z\\.\\$\\:_][A-Za-z\\.\\$\\:_0-9]*").Select(x => table.GetAddress(x)))
                               select val;
            var dest = (from code in Str("AMD").Select(x => 7)
                                     .Choice(Str("AM").Select(x => 5))
                                     .Choice(Str("AD").Select(x => 6))
                                     .Choice(Str("MD").Select(x => 3))
                                     .Choice(Str("M").Select(x => 1))
                                     .Choice(Str("D").Select(x => 2))
                                     .Choice(Str("A").Select(x => 4))
                         from eq in Str("=")
                         select code).Option(0);
            var comp = Str("0").Select(x => 0b0101010)
                       .Choice(Str("1").Select(x => 0b0111111))
                       .Choice(Str("-1").Select(x => 0b0111010))
                       .Choice(Str("!D").Select(x => 0b0001101))
                       .Choice(Str("!A").Select(x => 0b0110001))
                       .Choice(Str("-D").Select(x => 0b0001111))
                       .Choice(Str("-A").Select(x => 0b0110011))
                       .Choice(Str("D+1").Select(x => 0b0011111))
                       .Choice(Str("A+1").Select(x => 0b0110111))
                       .Choice(Str("D-1").Select(x => 0b0001110))
                       .Choice(Str("A-1").Select(x => 0b0110010))
                       .Choice(Str("-A").Select(x => 0b0110011))
                       .Choice(Str("D+A").Select(x => 0b0000010))
                       .Choice(Str("D-A").Select(x => 0b0010011))
                       .Choice(Str("A-D").Select(x => 0b0000111))
                       .Choice(Str("D&A").Select(x => 0b0000000))
                       .Choice(Str("D|A").Select(x => 0b0010101))
                       .Choice(Str("!M").Select(x => 0b1110001))
                       .Choice(Str("-M").Select(x => 0b1110011))
                       .Choice(Str("-M").Select(x => 0b1110011))
                       .Choice(Str("M+1").Select(x => 0b1110111))
                       .Choice(Str("M-1").Select(x => 0b1110010))
                       .Choice(Str("D+M").Select(x => 0b1000010))
                       .Choice(Str("D-M").Select(x => 0b1010011))
                       .Choice(Str("M-D").Select(x => 0b1000111))
                       .Choice(Str("D&M").Select(x => 0b1000000))
                       .Choice(Str("D|M").Select(x => 0b1010101))
                       .Choice(Str("D").Select(x => 0b0001100))
                       .Choice(Str("A").Select(x => 0b0110000))
                       .Choice(Str("M").Select(x => 0b1110000));
            var jump = (from sc in Str(";")
                        from code in Str("JGT").Select(x => 1)
                                     .Choice(Str("JEQ").Select(x => 2))
                                     .Choice(Str("JGE").Select(x => 3))
                                     .Choice(Str("JLT").Select(x => 4))
                                     .Choice(Str("JNE").Select(x => 5))
                                     .Choice(Str("JLE").Select(x => 6))
                                     .Choice(Str("JMP").Select(x => 7))
                        select code).Option(0);
            var cInstruction = from d in dest
                               from c in comp
                               from j in jump
                               select (0xe000 | (c << 6) | (d << 3) | j);
            var label = from lparen in Str("(")
                         from sym in Regex("[A-Za-z\\.\\$\\:_][A-Za-z\\.\\$\\:_0-9]*")
                         from rparen in Str(")")
                         select -1;
            var instruction = aInstruction.Choice(cInstruction).Choice(label).Choice(End().Select(x => -1));
            return instruction;
        }

        public static IEnumerable<short> Assemble(string input)
        {
            var assemblerPass1 = BuildParserPass1();
            var assemblerPass2 = BuildParserPass2();
            var skip = "([ \t]+|//[^\n]*)+";
            var lines = input.Split('\n');
            var result = new List<short>();
            int newAddress = 16;
            int pc = 0;
            var variables = new List<string>();

            foreach (var line in lines)
            {
                if (line != "\n")
                {
                    var lineResult = assemblerPass1.Run(line, skip);

                    if (lineResult.IsError)
                    {
                        throw new Exception(lineResult.ErrorMessage);
                    }
                    else if (lineResult.Value != null && lineResult.Value != "")
                    {
                        if(lineResult.Value[0] != '@')
                        {
                            table.AddEntry(lineResult.Value, pc);
                        }
                        else if(!variables.Contains(lineResult.Value.Substring(1)))
                        {
                            variables.Add(lineResult.Value.Substring(1));
                        }
                    }

                    if(lineResult.Value == "" || (lineResult.Value != null && lineResult.Value[0] == '@'))
                    {
                        pc++;
                    }
                }
            }

            foreach(var variable in variables)
            {
                if(!table.Contains(variable))
                {
                    table.AddEntry(variable, newAddress++);
                }
            }

            foreach (var line in lines)
            {
                if(line != "\n")
                {
                    var lineResult = assemblerPass2.Run(line, skip);

                    if(lineResult.IsError)
                    {
                        throw new Exception(lineResult.ErrorMessage);
                    }
                    else if(lineResult.Value >= 0)
                    {
                        result.Add((short)lineResult.Value);
                    }
                }
            }
            return result;
        }
    }
}
