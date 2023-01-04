using System.Buffers.Binary;
using System.Text;

namespace AssEmbly
{
    public static class Disassembler
    {
        /// <summary>
        /// Disassemble a program to AssEmbly code from it's assembled bytecode.
        /// </summary>
        public static string DisassembleProgram(byte[] program, bool detectStrings, bool detectPads)
        {
            int offset = 0;
            List<string> result = new();
            Dictionary<int, int> offsetToLine = new();
            List<(ulong, int)> references = new();
            while (offset < program.Length)
            {
                offsetToLine[offset] = result.Count;
                (string line, int additionalOffset, List<ulong> referencedAddresses) = DisassembleInstruction(program[offset..]);
                offset += additionalOffset;
                references.AddRange(referencedAddresses.Select(x => (x, result.Count)));
                result.Add(line);
            }
            // Insert label definitions
            references.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            references = references.DistinctBy(a => a.Item1).ToList();
            int inserted = 0;
            foreach ((ulong address, int sourceLineIndex) in references)
            {
                if (offsetToLine.TryGetValue((int)address, out int destLineIndex))
                {
                    result.Insert(destLineIndex + inserted, $":ADDR_{address:X}");
                    inserted++;
                }
                else
                {
                    result = result.Select(s => s.Replace($":ADDR_{address:X}", ":INVALID-LABEL")).ToList();
                }
            }
            if (detectPads || detectStrings)
            {
                for (int start = 0; start < result.Count; start++)
                {
                    if (detectStrings && result[start].StartsWith("DAT ") && result[start][4] != '"'
                        && (char)byte.Parse(result[start].Split()[1]) is not '\\' and >= ' ' and <= '~')
                    {
                        int end = result.Count;
                        for (int j = start + 1; j < result.Count; j++)
                        {
                            if (!result[j].StartsWith("DAT ") || result[j][4] == '"'
                                || (char)byte.Parse(result[j].Split()[1]) is '\\' or < ' ' or > '~')
                            {
                                end = j;
                                break;
                            }
                        }
                        if (start < end)
                        {
                            string newLine = "DAT \"";
                            newLine += Encoding.UTF8.GetString(result.GetRange(start, end - start)
                                .Select(x => byte.Parse(x.Split(' ')[1])).ToArray()).Replace("\"", "\\\"");
                            newLine += '"';
                            result.RemoveRange(start, end - start);
                            result.Insert(start, newLine);
                        }
                    }
                    if (detectPads && result[start] == "HLT")
                    {
                        if (start < result.Count - 1 && result[start + 1] == "HLT")
                        {
                            int end = start + 1;
                            for (int j = start + 1; j < result.Count; j++)
                            {
                                if (result[j] != "HLT")
                                {
                                    end = j;
                                    break;
                                }
                            }
                            string newLine = $"PAD {end - start - 1}";
                            result.RemoveRange(start + 1, end - start - 1);
                            result.Insert(start + 1, newLine);
                        }
                    }
                }
            }
            return string.Join("\n", result);
        }

        /// <summary>
        /// Disassemble a single line of AssEmbly code from it's assembled bytecode.
        /// </summary>
        /// <param name="instruction">The instruction to disassemble. More bytes than needed may be given.</param>
        /// <returns>(Disassembled line, Number of bytes instruction was, Referenced addresses [if present])</returns>
        public static (string, int, List<ulong>) DisassembleInstruction(byte[] instruction)
        {
            bool fallbackToDat = false;

            byte opcode = instruction[0];
            IEnumerable<KeyValuePair<(string, Data.OperandType[]), byte>> matching = Data.Mnemonics.Where(x => x.Value == opcode);
            if (!matching.Any())
            {
                fallbackToDat = true;
            }
            else
            {
                (string mnemonic, Data.OperandType[] operandTypes) = matching.First().Key;
                List<string> operandStrings = new();
                int totalBytes = 1;
                List<ulong> referencedAddresses = new();
                foreach (Data.OperandType type in operandTypes)
                {
                    if (totalBytes >= instruction.Length)
                    {
                        fallbackToDat = true;
                        break;
                    }
                    switch (type)
                    {
                        case Data.OperandType.Register:
                            if (Enum.IsDefined((Data.Register)instruction[totalBytes]))
                            {
                                operandStrings.Add(((Data.Register)instruction[totalBytes]).ToString());
                                totalBytes++;
                            }
                            else
                            {
                                fallbackToDat = true;
                            }
                            break;
                        case Data.OperandType.Literal:
                            if (totalBytes + 8 > instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            operandStrings.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction.AsSpan()[totalBytes..(totalBytes + 8)]).ToString());
                            totalBytes += 8;
                            break;
                        case Data.OperandType.Address:
                            if (totalBytes + 8 > instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            referencedAddresses.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction.AsSpan()[totalBytes..(totalBytes + 8)]));
                            operandStrings.Add($":ADDR_{referencedAddresses[^1]:X}");
                            totalBytes += 8;
                            break;
                        case Data.OperandType.Pointer:
                            if (Enum.IsDefined((Data.Register)instruction[totalBytes]))
                            {
                                operandStrings.Add("*" + ((Data.Register)instruction[totalBytes]).ToString());
                                totalBytes++;
                            }
                            else
                            {
                                fallbackToDat = true;
                            }
                            break;
                        default:
                            fallbackToDat = true;
                            break;
                    }
                    if (fallbackToDat)
                    {
                        break;
                    }
                }
                if (!fallbackToDat)
                {
                    return ($"{mnemonic} {string.Join(", ", operandStrings)}".Trim(), totalBytes, referencedAddresses);
                }
            }

            return ($"DAT {instruction[0]}", 1, new());
        }
    }
}
