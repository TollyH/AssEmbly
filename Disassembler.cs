using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace AssEmbly
{
    public static class Disassembler
    {
        /// <summary>
        /// Disassemble a program to AssEmbly code from it's assembled bytecode.
        /// </summary>
        public static string DisassembleProgram(byte[] program, bool detectStrings, bool detectPads, bool allowFullyQualifiedBaseOpcodes)
        {
            ulong offset = 0;
            List<string> result = new();
            Dictionary<ulong, int> offsetToLine = new();
            // address -> referencing instruction start offset
            Dictionary<ulong, List<ulong>> addressReferences = new();

            // %DAT insertions that correspond to ASCII values awaiting insertion as a full string
            List<byte> pendingStringCharacters = new();
            // The number of chained "%DAT 0" directives awaiting merge into a single %PAD
            int pendingZeroPad = 0;

            while (offset < (ulong)program.LongLength)
            {
                (string line, ulong additionalOffset, List<ulong> referencedAddresses, bool datFallback) =
                    DisassembleInstruction(program.AsSpan()[(int)offset..], allowFullyQualifiedBaseOpcodes, true);

                foreach (ulong address in referencedAddresses)
                {
                    _ = addressReferences.TryAdd(address, new List<ulong>());
                    addressReferences[address].Add(offset);
                }

                if (detectStrings && char.IsAscii((char)program[offset]) && !char.IsControl((char)program[offset]) && datFallback)
                {
                    DumpPendingZeroPad(ref pendingZeroPad, result, offset, offsetToLine);
                    pendingStringCharacters.Add(program[offset]);
                }
                else if (detectPads && program[offset] == 0 && additionalOffset == 1)
                {
                    DumpPendingString(pendingStringCharacters, result, offset, offsetToLine);
                    pendingZeroPad++;
                }
                else
                {
                    DumpPendingString(pendingStringCharacters, result, offset, offsetToLine);
                    DumpPendingZeroPad(ref pendingZeroPad, result, offset, offsetToLine);

                    offsetToLine[offset] = result.Count;
                    result.Add(line);
                }

                offset += additionalOffset;
            }

            DumpPendingString(pendingStringCharacters, result, offset, offsetToLine);
            DumpPendingZeroPad(ref pendingZeroPad, result, offset, offsetToLine);

            // Insert label definitions
            foreach ((ulong address, List<ulong> startOffsets) in addressReferences)
            {
                if (offsetToLine.TryGetValue(address, out int destLineIndex))
                {
                    result[destLineIndex] = $":ADDR_{address:X}\n" + result[destLineIndex];
                }
                else
                {
                    foreach (ulong start in startOffsets)
                    {
                        if (offsetToLine.TryGetValue(start, out int lineIndex))
                        {
                            result[lineIndex] = result[lineIndex].Replace($":ADDR_{address:X}", $":0x{address:X}") + "  ; Address does not align to a disassembled instruction";
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
        /// <returns>(Disassembled line, Number of bytes instruction was, Referenced addresses [if present], Used %DAT directive)</returns>
        public static (string Line, ulong AdditionalOffset, List<ulong> References, bool DatFallback) DisassembleInstruction(
            Span<byte> instruction, bool allowFullyQualifiedBaseOpcodes, bool useLabelNames)
        {
            if (instruction.Length == 0)
            {
                return ("", 0, new List<ulong>(), false);
            }
            bool fallbackToDat = false;

            ulong totalBytes = 0;
            Opcode opcode;
            if (instruction[0] == Opcode.FullyQualifiedMarker && instruction.Length < 3)
            {
                // We can't parse this data as an opcode properly,
                // as it starts with 0xFF but there are not enough bytes for it to be a fully qualified opcode.
                // Can happen with non-instruction statements like "%DAT 0xFF".
                fallbackToDat = true;
                totalBytes = 1;
                opcode = default;
            }
            else
            {
                opcode = Opcode.ParseBytes(instruction, ref totalBytes);
                totalBytes++;
            }
            if (!fallbackToDat && !allowFullyQualifiedBaseOpcodes && instruction[0] == Opcode.FullyQualifiedMarker && instruction[1] == 0x00)
            {
                // Opcode is fully qualified but is for the base instruction set
                // - technically valid but never done by the assembler, so interpret as data.
                // Will ensure that a re-assembly of the disassembled program remains byte-perfect.
                fallbackToDat = true;
            }
            if (!fallbackToDat && Data.MnemonicsReverse.TryGetValue(opcode, out (string Mnemonic, OperandType[] OperandTypes) matching))
            {
                (string mnemonic, OperandType[] operandTypes) = matching;
                List<string> operandStrings = new();
                List<ulong> referencedAddresses = new();
                foreach (OperandType type in operandTypes)
                {
                    if (totalBytes >= (uint)instruction.Length)
                    {
                        fallbackToDat = true;
                        break;
                    }
                    switch (type)
                    {
                        case OperandType.Register:
                            if (Enum.IsDefined((Register)instruction[(int)totalBytes]))
                            {
                                operandStrings.Add(((Register)instruction[(int)totalBytes]).ToString());
                                totalBytes++;
                            }
                            else
                            {
                                fallbackToDat = true;
                            }
                            break;
                        case OperandType.Literal:
                            if (totalBytes + 8 > (uint)instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            operandStrings.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction[(int)totalBytes..]).ToString());
                            totalBytes += 8;
                            break;
                        case OperandType.Address:
                            if (totalBytes + 8 > (uint)instruction.Length)
                            {
                                fallbackToDat = true;
                                break;
                            }
                            referencedAddresses.Add(BinaryPrimitives.ReadUInt64LittleEndian(instruction[(int)totalBytes..]));
                            operandStrings.Add(useLabelNames ? $":ADDR_{referencedAddresses[^1]:X}" : $":{referencedAddresses[^1]:X}");
                            totalBytes += 8;
                            break;
                        case OperandType.Pointer:
                            if (Enum.IsDefined((Register)instruction[(int)totalBytes]))
                            {
                                operandStrings.Add("*" + (Register)instruction[(int)totalBytes]);
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
                    return ($"{mnemonic} {string.Join(", ", operandStrings)}".Trim(), totalBytes, referencedAddresses, false);
                }
            }

            return ($"%DAT {instruction[0]}", 1, new List<ulong>(), true);
        }

        private static void DumpPendingString(List<byte> pendingStringCharacters, List<string> result, ulong offset, Dictionary<ulong, int> offsetToLine)
        {
            if (pendingStringCharacters.Count > 0)
            {
                string newString = Encoding.ASCII.GetString(CollectionsMarshal.AsSpan(pendingStringCharacters));
                newString = newString.Replace(@"\", @"\\");
                newString = newString.Replace("\"", "\\\"");
                newString = newString.Replace("@", @"\@");

                offsetToLine[offset - (ulong)pendingStringCharacters.Count] = result.Count;

                result.Add($"%DAT \"{newString}\"");
                pendingStringCharacters.Clear();
            }
        }

        private static void DumpPendingZeroPad(ref int pendingZeroPad, List<string> result, ulong offset, Dictionary<ulong, int> offsetToLine)
        {
            if (pendingZeroPad > 0)
            {
                offsetToLine[offset - (ulong)pendingZeroPad] = result.Count;
                result.Add("HLT");
                if (pendingZeroPad > 1)
                {
                    offsetToLine[offset - (ulong)pendingZeroPad + 1] = result.Count;
                    result.Add($"%PAD {pendingZeroPad - 1}");
                }
                pendingZeroPad = 0;
            }
        }
    }
}
