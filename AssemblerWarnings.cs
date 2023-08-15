using System.Buffers.Binary;

namespace AssEmbly
{
    public enum WarningSeverity
    {
        FatalError,
        NonFatalError,
        Warning,
        Suggestion
    }

    [Serializable]
    public readonly struct Warning
    {
        public readonly WarningSeverity Severity;
        public readonly int Code;
        public readonly string File;
        public readonly int Line;

        /// <summary>
        /// Index 0 will always be mnemonic. Index 1 and onwards are opcodes, if any.
        /// </summary>
        public readonly string[] InstructionElements;

        public readonly string Message => string.Format(AssemblerWarnings.GetMessagesForSeverity(Severity)[Code], InstructionElements);

        public Warning(WarningSeverity severity, int code, string file, int line, string mnemonic, string[] operands)
        {
            Severity = severity;
            Code = code;
            File = file;
            Line = line;

            InstructionElements = new string[operands.Length + 1];
            InstructionElements[0] = mnemonic;
            Array.Copy(operands, 0, InstructionElements, 1, operands.Length);
        }
    }

    public delegate bool RollingWarningAnalyzer();
    public delegate List<Warning> FinalWarningAnalyzer();

    public partial class AssemblerWarnings
    {
        private readonly Dictionary<int, RollingWarningAnalyzer> nonFatalErrorRollingAnalyzers;
        private readonly Dictionary<int, RollingWarningAnalyzer> warningRollingAnalyzers;
        private readonly Dictionary<int, RollingWarningAnalyzer> suggestionRollingAnalyzers;

        private readonly Dictionary<int, FinalWarningAnalyzer> nonFatalErrorFinalAnalyzers;
        private readonly Dictionary<int, FinalWarningAnalyzer> warningFinalAnalyzers;
        private readonly Dictionary<int, FinalWarningAnalyzer> suggestionFinalAnalyzers;

        public HashSet<int> DisabledNonFatalErrors = new();
        public HashSet<int> DisabledWarnings = new();
        public HashSet<int> DisabledSuggestions = new();

        // Variables updated by parameters of the NextInstruction method
        private byte[] newBytes = Array.Empty<byte>();
        private string mnemonic = "";
        private string[] operands = Array.Empty<string>();
        private int line = 0;
        private string file = "";
        private bool labelled = false;

        /// <summary>
        /// Update the state of the class instance with the next instruction in the program being analyzed.
        /// </summary>
        /// <param name="newBytes">The bytes of the next instruction to check for warnings.</param>
        /// <param name="mnemonic">
        /// The mnemonic that was used in the instruction. It should be stripped of whitespace before being passed.
        /// </param>
        /// <param name="operands">
        /// The operands that were used in the instruction. They should be stripped of whitespace before being passed.
        /// </param>
        /// <param name="line">The file-based 0-indexed line that the instruction was assembled from.</param>
        /// <param name="file">
        /// The path to the file that the instruction was assembled from, or <see cref="string.Empty"/> for the base file.
        /// </param>
        /// <param name="labelled">Was this instruction preceded by one or more label definitions?</param>
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes, string mnemonic, string[] operands, int line, string file, bool labelled)
        {
            this.newBytes = newBytes;
            this.mnemonic = mnemonic;
            this.operands = operands;
            this.line = line;
            this.file = file;
            this.labelled = labelled;

            List<Warning> warnings = new();

            PreAnalyzeStateUpdate();
            foreach ((int code, RollingWarningAnalyzer rollingAnalyzer) in nonFatalErrorRollingAnalyzers)
            {
                if (DisabledNonFatalErrors.Contains(code))
                {
                    continue;
                }
                if (rollingAnalyzer())
                {
                    warnings.Add(new Warning(WarningSeverity.NonFatalError, code, file, line, mnemonic, operands));
                }
            }
            foreach ((int code, RollingWarningAnalyzer rollingAnalyzer) in warningRollingAnalyzers)
            {
                if (DisabledWarnings.Contains(code))
                {
                    continue;
                }
                if (rollingAnalyzer())
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, code, file, line, mnemonic, operands));
                }
            }
            foreach ((int code, RollingWarningAnalyzer rollingAnalyzer) in suggestionRollingAnalyzers)
            {
                if (DisabledSuggestions.Contains(code))
                {
                    continue;
                }
                if (rollingAnalyzer())
                {
                    warnings.Add(new Warning(WarningSeverity.Suggestion, code, file, line, mnemonic, operands));
                }
            }
            PostAnalyzeStateUpdate();

            return warnings.ToArray();
        }

        /// <summary>
        /// Call this after all program instructions have been given to <see cref="NextInstruction"/>
        /// to run analyzers that need the entire program to work.
        /// </summary>
        /// <returns>An array of any warnings caused by final analysis.</returns>
        public Warning[] Finalize()
        {
            List<Warning> warnings = new();

            foreach ((int code, FinalWarningAnalyzer finalAnalyzer) in nonFatalErrorFinalAnalyzers)
            {
                if (DisabledNonFatalErrors.Contains(code))
                {
                    continue;
                }
                warnings.AddRange(finalAnalyzer());
            }
            foreach ((int code, FinalWarningAnalyzer finalAnalyzer) in warningFinalAnalyzers)
            {
                if (DisabledWarnings.Contains(code))
                {
                    continue;
                }
                warnings.AddRange(finalAnalyzer());
            }
            foreach ((int code, FinalWarningAnalyzer finalAnalyzer) in suggestionFinalAnalyzers)
            {
                if (DisabledSuggestions.Contains(code))
                {
                    continue;
                }
                warnings.AddRange(finalAnalyzer());
            }

            return warnings.ToArray();
        }

        public AssemblerWarnings()
        {
            nonFatalErrorRollingAnalyzers = new()
            {
                { 0001, Analyzer_Rolling_NonFatalError_0001 },
                { 0002, Analyzer_Rolling_NonFatalError_0002 },
            };
            warningRollingAnalyzers = new()
            {
                { 0001, Analyzer_Rolling_Warning_0001 },
                { 0007, Analyzer_Rolling_Warning_0007 },
                { 0008, Analyzer_Rolling_Warning_0008 },
                { 0009, Analyzer_Rolling_Warning_0009 },
                { 0010, Analyzer_Rolling_Warning_0010 },
                { 0011, Analyzer_Rolling_Warning_0011 },
                { 0012, Analyzer_Rolling_Warning_0012 },
                { 0013, Analyzer_Rolling_Warning_0013 },
                { 0014, Analyzer_Rolling_Warning_0014 },
            };
            suggestionRollingAnalyzers = new()
            {
                { 0001, Analyzer_Rolling_Suggestion_0001 },
                { 0002, Analyzer_Rolling_Suggestion_0002 },
                { 0005, Analyzer_Rolling_Suggestion_0005 },
                { 0006, Analyzer_Rolling_Suggestion_0006 },
                { 0007, Analyzer_Rolling_Suggestion_0007 },
                { 0008, Analyzer_Rolling_Suggestion_0008 },
                { 0009, Analyzer_Rolling_Suggestion_0009 },
                { 0010, Analyzer_Rolling_Suggestion_0010 },
                { 0011, Analyzer_Rolling_Suggestion_0011 },
                { 0012, Analyzer_Rolling_Suggestion_0012 },
            };

            nonFatalErrorFinalAnalyzers = new();
            warningFinalAnalyzers = new()
            {
                { 0002, Analyzer_Final_Warning_0002 },
                { 0003, Analyzer_Final_Warning_0003 },
                { 0004, Analyzer_Final_Warning_0004 },
                { 0005, Analyzer_Final_Warning_0005 },
                { 0006, Analyzer_Final_Warning_0006 },
                { 0009, Analyzer_Final_Warning_0009 },
            };
            suggestionFinalAnalyzers = new()
            {
                { 0003, Analyzer_Final_Suggestion_0003 },
                { 0004, Analyzer_Final_Suggestion_0004 },
            };
        }

        // Analyzer state variables

        private List<byte> finalProgram = new();
        private Dictionary<(string File, int Line), ulong> lineAddresses = new();
        private Dictionary<(string File, int Line), string> lineMnemonics = new();
        private Dictionary<(string File, int Line), string[]> lineOperands = new();
        private bool instructionIsData = false;
        private bool instructionIsImport = false;
        private List<(int Line, string File)> dataInsertionLines = new();
        private HashSet<ulong> dataAddresses = new();
        private List<(int Line, string File)> stringInsertionLines = new();
        private List<(int Line, string File)> importLines = new();
        private Dictionary<string, int> lastExecutableLine = new();
        private Dictionary<(int Line, string File), ulong> jumpCallToLabels = new();
        private Dictionary<(int Line, string File), ulong> writesToLabels = new();
        private Dictionary<(int Line, string File), ulong> readsFromLabels = new();

        private ulong currentAddress = 0;
        private bool lastInstructionWasTerminator = false;
        private bool lastInstructionWasData = false;
        private string lastFilePath = "";
        private string lastMnemonic = "";
        private string[] lastOperands = Array.Empty<string>();

        private void PreAnalyzeStateUpdate()
        {
            finalProgram.AddRange(newBytes);
            lineAddresses[(file, line)] = currentAddress;
            lineMnemonics[(file, line)] = mnemonic;
            lineOperands[(file, line)] = operands;

            instructionIsData = dataInsertionDirectives.Contains(mnemonic.ToUpper());
            instructionIsImport = mnemonic.ToUpper() == "IMP";

            if (instructionIsData)
            {
                dataInsertionLines.Add((line, file));
                _ = dataAddresses.Add(currentAddress);
                if (operands[0][0] == '"')
                {
                    stringInsertionLines.Add((line, file));
                }
            }
            else if (instructionIsImport)
            {
                importLines.Add((line, file));
            }
            else
            {
                lastExecutableLine[file] = line;
                if (jumpCallToLabelOpcodes.Contains(newBytes[0]))
                {
                    jumpCallToLabels[(line, file)] = currentAddress + 1;
                }
                else if (writeToMemory.Contains(newBytes[0]))
                {
                    writesToLabels[(line, file)] = currentAddress + 1;
                }
                else if (readValueFromMemory.TryGetValue(newBytes[0], out int addressOpcodeIndex))
                {
                    readsFromLabels[(line, file)] = currentAddress + (uint)addressOpcodeIndex + 1;
                }
            }
        }

        private void PostAnalyzeStateUpdate()
        {
            currentAddress += (uint)newBytes.Length;
            lastInstructionWasTerminator = terminators.Contains(newBytes[0]);
            lastInstructionWasData = instructionIsData;
            lastFilePath = file;
            lastMnemonic = mnemonic;
            lastOperands = operands;
        }

        // Analyzer methods
        // (Rolling = runs as each instruction is processed, Final = runs after all instructions have been processed)

        private bool Analyzer_Rolling_NonFatalError_0001()
        {
            // Non-Fatal Error 0001: Instruction writes to the rpo register.
            if (!instructionIsData && writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
            {
                foreach (int operandIndex in writtenOperands)
                {
                    if (operands[operandIndex].ToLower() == "rpo")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_NonFatalError_0002()
        {
            // Non-Fatal Error 0002: Division by constant 0.
            if (!instructionIsData && divisionByLiteral.TryGetValue(newBytes[0], out int literalOperandIndex))
            {
                _ = Assembler.ParseLiteral(operands[literalOperandIndex], false, out ulong number);
                return number == 0;
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0001()
        {
            // Warning 0001: Data insertion is not directly preceded by unconditional jump, return, or halt instruction.
            if (instructionIsData && !lastInstructionWasTerminator)
            {
                return true;
            }
            return false;
        }

        private List<Warning> Analyzer_Final_Warning_0002()
        {
            // Warning 0002: Jump/Call target label points to data, not executable code.
            List<Warning> warnings = new();
            foreach (((int jumpLine, string jumpFile), ulong labelAddress) in jumpCallToLabels)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.Skip((int)labelAddress).ToArray());
                if (dataAddresses.Contains(address))
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, jumpFile, jumpLine,
                        lineMnemonics[(jumpFile, jumpLine)], lineOperands[(jumpFile, jumpLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0003()
        {
            // Warning 0003: Jump/Call target label points to end of file, not executable code.
            List<Warning> warnings = new();
            foreach (((int jumpLine, string jumpFile), ulong labelAddress) in jumpCallToLabels)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.Skip((int)labelAddress).ToArray());
                if (address >= currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, jumpFile, jumpLine,
                        lineMnemonics[(jumpFile, jumpLine)], lineOperands[(jumpFile, jumpLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0004()
        {
            // Warning 0004: Instruction writes to a label pointing to executable code.
            List<Warning> warnings = new();
            foreach (((int writeLine, string writeFile), ulong labelAddress) in writesToLabels)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.Skip((int)labelAddress).ToArray());
                if (!dataAddresses.Contains(address))
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, writeFile, writeLine,
                        lineMnemonics[(writeFile, writeLine)], lineOperands[(writeFile, writeLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0005()
        {
            // Warning 0005: Instruction reads from a label pointing to executable code in a context that likely expects data.
            List<Warning> warnings = new();
            foreach (((int writeLine, string writeFile), ulong labelAddress) in readsFromLabels)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.Skip((int)labelAddress).ToArray());
                if (!dataAddresses.Contains(address))
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, writeFile, writeLine,
                        lineMnemonics[(writeFile, writeLine)], lineOperands[(writeFile, writeLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0006()
        {
            // Warning 0006: String insertion is not immediately followed by a 0 (null) byte.
            List<Warning> warnings = new();
            foreach ((int stringLine, string stringFile) in stringInsertionLines)
            {
                string[] stringOperands = lineOperands[(stringFile, stringLine)];
                int stringLength = Assembler.ParseLiteral(stringOperands[1], true).Length;
                ulong address = lineAddresses[(stringFile, stringLine)] + (uint)stringLength;
                if (address >= (uint)finalProgram.Count || finalProgram[(int)address] != 0)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0006, stringFile, stringLine,
                        lineMnemonics[(stringFile, stringLine)], stringOperands));
                }
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Warning_0007()
        {
            // Warning 0007: Numeric literal is too large for the given move instruction. Upper bits will be truncated at runtime.
            if (!instructionIsData && moveLiteral.Contains(newBytes[0]) && moveBitCounts.TryGetValue(newBytes[0], out int maxBits))
            {
                _ = Assembler.ParseLiteral(operands[1], false, out ulong number);
                if (number == 0)
                {
                    return false;
                }
                return (int)Math.Log(number, 2) + 1 > maxBits;
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0008()
        {
            // Warning 0008: Unreachable code detected.
            return !labelled && lastInstructionWasTerminator && !instructionIsData;
        }

        private bool Analyzer_Rolling_Warning_0009()
        {
            // Warning 0009: Program runs to end of file without being terminated by unconditional jump, return, or halt.
            return file != lastFilePath && !lastInstructionWasTerminator && !lastInstructionWasData;
        }

        private List<Warning> Analyzer_Final_Warning_0009()
        {
            // Warning 0009: Program runs to end of file without being terminated by unconditional jump, return, or halt.
            if (!lastInstructionWasTerminator && !lastInstructionWasData)
            {
                return new List<Warning> { new Warning(WarningSeverity.Warning, 0009, file, line, mnemonic, operands) };
            }
            return new List<Warning>();
        }

        private bool Analyzer_Rolling_Warning_0010()
        {
            // Warning 0010: File import is not directly preceded by unconditional jump, return, or halt instruction.
            return !lastInstructionWasTerminator && !lastInstructionWasData && instructionIsImport;
        }

        private bool Analyzer_Rolling_Warning_0011()
        {
            // Warning 0011: Instruction writes to the rsf register.
            if (!instructionIsData && writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
            {
                foreach (int operandIndex in writtenOperands)
                {
                    if (operands[operandIndex].ToLower() == "rsf")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0012()
        {
            // Warning 0012: Instruction writes to the rsb register.
            if (!instructionIsData && writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
            {
                foreach (int operandIndex in writtenOperands)
                {
                    if (operands[operandIndex].ToLower() == "rsb")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0013()
        {
            // Warning 0013: Jump/Call target label points to itself, resulting in an unbreakable infinite loop.
            return !instructionIsData && jumpCallToLabelOpcodes.Contains(newBytes[0])
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[1..]) == currentAddress;
        }

        private bool Analyzer_Rolling_Warning_0014()
        {
            // Warning 0014: Unlabelled executable code found after data insertion.
            return !instructionIsData && lastInstructionWasData && !labelled;
        }

        private bool Analyzer_Rolling_Suggestion_0001()
        {
            // Suggestion 0001: Avoid use of NOP instruction.
            return !instructionIsData && newBytes[0] == 0x01;
        }

        private bool Analyzer_Rolling_Suggestion_0002()
        {
            // Suggestion 0002: Use the `PAD` directive instead of chaining `DAT 0` directives.
            if (mnemonic.ToUpper() == "DAT" && lastMnemonic.ToUpper() == "DAT")
            {
                _ = Assembler.ParseLiteral(operands[0], false, out ulong thisNumber);
                _ = Assembler.ParseLiteral(lastOperands[0], false, out ulong lastNumber);
                return thisNumber == 0 && lastNumber == 0;
            }
            return false;
        }

        private List<Warning> Analyzer_Final_Suggestion_0003()
        {
            // Suggestion 0003: Put IMP directives at the end of the file,
            // unless the position of the directive is important given the file's contents.
            List<Warning> warnings = new();
            foreach ((int impLine, string impFile) in importLines)
            {
                if (lastExecutableLine.TryGetValue(impFile, out int execLine) && impLine < execLine)
                {
                    warnings.Add(new Warning(WarningSeverity.Suggestion, 0003, impFile, impLine,
                        lineMnemonics[(impFile, impLine)], lineOperands[(impFile, impLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Suggestion_0004()
        {
            // Suggestion 0004: Put data at the end of the file, unless the position of the data is important.
            List<Warning> warnings = new();
            foreach ((int dataLine, string dataFile) in dataInsertionLines)
            {
                if (lastExecutableLine.TryGetValue(dataFile, out int execLine) && dataLine < execLine)
                {
                    warnings.Add(new Warning(WarningSeverity.Suggestion, 0003, dataFile, dataLine,
                        lineMnemonics[(dataFile, dataLine)], lineOperands[(dataFile, dataLine)]));
                }
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Suggestion_0005()
        {
            // Suggestion 0005: Use `TST {reg}, {reg}` instead of `CMP {reg}, 0`, as it results in less bytes.
            return !instructionIsData && newBytes[0] == 0x75
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0006()
        {
            // Suggestion 0006: Use `XOR {reg}, {reg}` instead of `MV{B|W|D|Q} {reg}, 0`, as it results in less bytes.
            return !instructionIsData && moveRegLit.Contains(newBytes[0])
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0007()
        {
            // Suggestion 0007: Use `INC {reg}` instead of `ADD {reg}, 1`, as it results in less bytes.
            return !instructionIsData && newBytes[0] == 0x11
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0008()
        {
            // Suggestion 0008: Use `DEC {reg}` instead of `SUB {reg}, 1`, as it results in less bytes.
            return !instructionIsData && newBytes[0] == 0x21
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0009()
        {
            // Suggestion 0009: Operation has no effect.
            if (instructionIsData)
            {
                return false;
            }
            switch (newBytes[0])
            {
                // Add, Subtract, Shift, Or, or Xor by 0
                case 0x11:
                case 0x21:
                case 0x51:
                case 0x55:
                case 0x65:
                case 0x69:
                    if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 0)
                    {
                        return true;
                    }
                    break;
                // Multiply by 1
                case 0x31:
                    if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 1)
                    {
                        return true;
                    }
                    break;
                // And by all 1 bits
                case 0x61:
                    if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == ulong.MaxValue)
                    {
                        return true;
                    }
                    break;
            }
            if (divisionByLiteral.TryGetValue(newBytes[0], out int literalOperandIndex))
            {
                _ = Assembler.ParseLiteral(operands[literalOperandIndex], false, out ulong number);
                // Division by 1
                return number == 1;
            }
            return false;
        }

        private bool Analyzer_Rolling_Suggestion_0010()
        {
            // Suggestion 0010: Shift operation shifts by 64 bits or more, which will always result in 0. Use `XOR {reg}, {reg}` instead.
            return !instructionIsData && shiftByLiteral.Contains(newBytes[0])
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) >= 64;
        }

        private bool Analyzer_Rolling_Suggestion_0011()
        {
            // Suggestion 0011: Remove leading 0 digits from denary number.
            foreach (string operand in operands)
            {
                string operandClean = operand.Replace("_", "");
                if (ulong.TryParse(operandClean, out _) && operandClean[0] == '0')
                {
                    return true;
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_Suggestion_0012()
        {
            // Suggestion 0012: Remove useless `PAD 0` directive.
            if (mnemonic.ToUpper() == "PAD")
            {
                _ = Assembler.ParseLiteral(operands[0], false, out ulong number);
                return number == 0;
            }
            return false;
        }
    }
}
