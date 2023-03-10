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
            ulong offset = 0;
            List<string> result = new();
            Dictionary<ulong, int> offsetToLine = new();
            List<(ulong, int)> references = new();
            while (offset < (ulong)program.LongLength)
            {
                offsetToLine[offset] = result.Count;
                (string line, ulong additionalOffset, List<ulong> referencedAddresses) = DisassembleInstruction(program.AsSpan()[(int)offset..]);
                offset += additionalOffset;
                references.AddRange(referencedAddresses.Select(x => (x, result.Count)));
                result.Add(line);
            }
            // Insert label definitions
            references.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            List<int> inserted = new();
            foreach ((ulong address, int sourceLineIndex) in references.DistinctBy(a => a.Item1))
            {
                if (offsetToLine.TryGetValue(address, out int destLineIndex))
                {
                    result.Insert(destLineIndex + inserted.Count, $":ADDR_{address:X}");
                    inserted.Add(destLineIndex);
                }
                else
                {
                    foreach (int lineIndex in references.Where(a => a.Item1 == address).Select(a => a.Item2))
                    {
                        int toReplaceIndex = lineIndex + inserted.Count(l => l <= lineIndex);
                        result[toReplaceIndex] = result[toReplaceIndex].Replace($":ADDR_{address:X}", ":INVALID-LABEL");
                    }
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
                            int end = result.Count;
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
        public static (string, ulong, List<ulong>) DisassembleInstruction(Span<byte> instruction)
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
                ulong totalBytes = 1;
                List<ulong> referencedAddresses = new();
                foreach (Data.OperandType type in operandTypes)
                {
                    if (totalBytes >= (uint)instruction.Length)
                    {
                        fallbackToDat = true;
                        break;
                    }
                    switch (type)
                    {
                        case Data.OperandType.Register:
                            if (Enum.IsDefined((Data.Register)instruction[(int)totalBytes]))
                            {
                                operandStrings.Add(((Data.Register)instruction[(int)totalBytes]).ToString());
                                totalBytes++;
                            }
                            else
                            {
                                fallbackToDat = true;
                            }
                            break;
                        case Data.OperandType.Literal:
                            if (totalBytes + 8 > (uint)instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            operandStrings.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction[(int)totalBytes..((int)totalBytes + 8)]).ToString());
                            totalBytes += 8;
                            break;
                        case Data.OperandType.Address:
                            if (totalBytes + 8 > (uint)instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            referencedAddresses.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction[(int)totalBytes..((int)totalBytes + 8)]));
                            operandStrings.Add($":ADDR_{referencedAddresses[^1]:X}");
                            totalBytes += 8;
                            break;
                        case Data.OperandType.Pointer:
                            if (Enum.IsDefined((Data.Register)instruction[(int)totalBytes]))
                            {
                                operandStrings.Add("*" + ((Data.Register)instruction[(int)totalBytes]).ToString());
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
