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
        public readonly string OriginalLine;

        /// <summary>
        /// Index 0 will always be mnemonic. Index 1 and onwards are opcodes, if any.
        /// </summary>
        public readonly string[] InstructionElements;

        public readonly string Message;

        public Warning(WarningSeverity severity, int code, string file, int line, string mnemonic, string[] operands,
            string originalLine, string? message = null)
        {
            Severity = severity;
            Code = code;
            File = file;
            Line = line;
            OriginalLine = originalLine;

            InstructionElements = new string[operands.Length + 1];
            InstructionElements[0] = mnemonic;
            Array.Copy(operands, 0, InstructionElements, 1, operands.Length);

            Message = message ?? string.Format(AssemblerWarnings.GetMessagesForSeverity(Severity)[Code], InstructionElements);
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

        public readonly HashSet<int> DisabledNonFatalErrors = new();
        public readonly HashSet<int> DisabledWarnings = new();
        public readonly HashSet<int> DisabledSuggestions = new();

        // Variables updated by parameters of the NextInstruction method
        private byte[] newBytes = Array.Empty<byte>();
        private string mnemonic = "";
        private string[] operands = Array.Empty<string>();
        private int line;
        private int firstLine;
        private string file = "";
        private bool labelled;
        private bool isEntry;
        private readonly bool usingV1Format;
        private Stack<Assembler.ImportStackFrame> importStack = new();

        private readonly Dictionary<(string File, int Line), string> lineText = new();

        private byte[] finalProgram = Array.Empty<byte>();

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
        /// <param name="isEntry">Is this instruction the entry point?</param>
        /// <param name="importStack">The current state of the program's import stack.</param>
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes, string mnemonic, string[] operands, int line, string file, bool labelled,
            bool isEntry, string rawLine, IEnumerable<Assembler.ImportStackFrame> importStack)
        {
            this.newBytes = newBytes;
            this.mnemonic = mnemonic;
            this.operands = operands;
            this.line = line;
            this.file = file;
            this.labelled = labelled;
            this.isEntry = isEntry;
            this.importStack = new Stack<Assembler.ImportStackFrame>(importStack);
            lineText[(file, line)] = rawLine;

            if (firstLine == 0)
            {
                firstLine = line;
            }

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
                    warnings.Add(
                        new Warning(WarningSeverity.NonFatalError, code, file, line, mnemonic, operands, rawLine));
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
                    warnings.Add(
                        new Warning(WarningSeverity.Warning, code, file, line, mnemonic, operands, rawLine));
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
                    warnings.Add(
                        new Warning(WarningSeverity.Suggestion, code, file, line, mnemonic, operands, rawLine));
                }
            }
            PostAnalyzeStateUpdate();

            return warnings.ToArray();
        }

        /// <summary>
        /// Call this after all program instructions have been given to <see cref="NextInstruction"/>
        /// to run analyzers that need the entire program to work.
        /// </summary>
        /// <param name="finalProgram">The fully assembled program, with all label locations inserted.</param>
        /// <returns>An array of any warnings caused by final analysis.</returns>
        public Warning[] Finalize(byte[] finalProgram)
        {
            this.finalProgram = finalProgram;

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

        public AssemblerWarnings(bool usingV1Format)
        {
            this.usingV1Format = usingV1Format;

            nonFatalErrorRollingAnalyzers = new Dictionary<int, RollingWarningAnalyzer>
            {
                { 0001, Analyzer_Rolling_NonFatalError_0001 },
                { 0002, Analyzer_Rolling_NonFatalError_0002 },
                { 0003, Analyzer_Rolling_NonFatalError_0003 },
            };
            warningRollingAnalyzers = new Dictionary<int, RollingWarningAnalyzer>
            {
                { 0001, Analyzer_Rolling_Warning_0001 },
                { 0007, Analyzer_Rolling_Warning_0007 },
                { 0008, Analyzer_Rolling_Warning_0008 },
                { 0010, Analyzer_Rolling_Warning_0010 },
                { 0011, Analyzer_Rolling_Warning_0011 },
                { 0012, Analyzer_Rolling_Warning_0012 },
                { 0014, Analyzer_Rolling_Warning_0014 },
                { 0015, Analyzer_Rolling_Warning_0015 },
                { 0016, Analyzer_Rolling_Warning_0016 },
                { 0017, Analyzer_Rolling_Warning_0017 },
                { 0018, Analyzer_Rolling_Warning_0018 },
                { 0019, Analyzer_Rolling_Warning_0019 },
                { 0020, Analyzer_Rolling_Warning_0020 },
                { 0021, Analyzer_Rolling_Warning_0021 },
                { 0022, Analyzer_Rolling_Warning_0022 },
                { 0023, Analyzer_Rolling_Warning_0023 },
                { 0024, Analyzer_Rolling_Warning_0024 },
                { 0025, Analyzer_Rolling_Warning_0025 },
            };
            suggestionRollingAnalyzers = new Dictionary<int, RollingWarningAnalyzer>
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
                { 0013, Analyzer_Rolling_Suggestion_0013 },
                { 0014, Analyzer_Rolling_Suggestion_0014 },
            };

            nonFatalErrorFinalAnalyzers = new Dictionary<int, FinalWarningAnalyzer>();
            warningFinalAnalyzers = new Dictionary<int, FinalWarningAnalyzer>
            {
                { 0002, Analyzer_Final_Warning_0002 },
                { 0003, Analyzer_Final_Warning_0003 },
                { 0004, Analyzer_Final_Warning_0004 },
                { 0005, Analyzer_Final_Warning_0005 },
                { 0006, Analyzer_Final_Warning_0006 },
                { 0009, Analyzer_Final_Warning_0009 },
                { 0013, Analyzer_Final_Warning_0013 },
            };
            suggestionFinalAnalyzers = new Dictionary<int, FinalWarningAnalyzer>
            {
                { 0003, Analyzer_Final_Suggestion_0003 },
                { 0004, Analyzer_Final_Suggestion_0004 },
            };
        }

        // Analyzer state variables

        private Opcode instructionOpcode;
        private ulong operandStart;
        private readonly Dictionary<(string File, int Line), ulong> lineAddresses = new();
        private readonly Dictionary<(string File, int Line), string> lineMnemonics = new();
        private readonly Dictionary<(string File, int Line), string[]> lineOperands = new();
        private bool instructionIsData;
        private bool instructionIsImport;
        private bool instructionIsString;
        private readonly List<(int Line, string File)> dataInsertionLines = new();
        private readonly HashSet<ulong> dataAddresses = new();
        private readonly List<(int Line, string File)> endingStringInsertionLines = new();
        private readonly List<(int Line, string File)> importLines = new();
        private readonly Dictionary<string, int> lastExecutableLine = new();
        private readonly Dictionary<(int Line, string File), ulong> jumpCallToLabels = new();
        private readonly Dictionary<(int Line, string File), ulong> writesToLabels = new();
        private readonly Dictionary<(int Line, string File), ulong> readsFromLabels = new();
        private readonly Dictionary<(int Line, string File), ulong> jumpsCalls = new();

        private ulong currentAddress;
        private bool lastInstructionWasTerminator;
        private bool lastInstructionWasData;
        private bool lastInstructionWasString;
        private string lastMnemonic = "";
        private string[] lastOperands = Array.Empty<string>();
        private Stack<Assembler.ImportStackFrame> lastImportStack = new();
        private bool insertedAnyExecutable;

        private void PreAnalyzeStateUpdate()
        {
            if (newBytes.Length > 0)
            {
                operandStart = 0;
                instructionOpcode = Opcode.ParseBytes(newBytes, ref operandStart);
                operandStart++;
            }

            lineAddresses[(file, line)] = currentAddress;
            lineMnemonics[(file, line)] = mnemonic;
            lineOperands[(file, line)] = operands;

            instructionIsData = dataInsertionDirectives.Contains(mnemonic.ToUpper());
            instructionIsImport = mnemonic.ToUpper() == "IMP";
            instructionIsString = false;

            if (instructionIsData)
            {
                dataInsertionLines.Add((line, file));
                _ = dataAddresses.Add(currentAddress);
                if (operands[0][0] == '"')
                {
                    instructionIsString = true;
                    if (lastInstructionWasString)
                    {
                        // Only store the last in a chain of string insertions
                        endingStringInsertionLines.RemoveAt(endingStringInsertionLines.Count - 1);
                    }
                    endingStringInsertionLines.Add((line, file));
                }
            }
            else if (instructionIsImport)
            {
                importLines.Add((line, file));
            }
            else if (newBytes.Length > 0)
            {
                lastExecutableLine[file] = line;
                if (jumpCallToLabelOpcodes.Contains(instructionOpcode))
                {
                    jumpCallToLabels[(line, file)] = currentAddress + operandStart;
                }
                if (writeToMemory.Contains(instructionOpcode))
                {
                    writesToLabels[(line, file)] = currentAddress + operandStart;
                }
                if (readValueFromMemory.TryGetValue(instructionOpcode, out ulong addressOpcodeOffset))
                {
                    readsFromLabels[(line, file)] = currentAddress + operandStart + addressOpcodeOffset;
                }
                if (jumpCallToLabelOpcodes.Contains(instructionOpcode))
                {
                    jumpsCalls[(line, file)] = currentAddress + operandStart;
                }
            }
        }

        private void PostAnalyzeStateUpdate()
        {
            currentAddress += (uint)newBytes.Length;
            if (newBytes.Length > 0)
            {
                lastInstructionWasTerminator = terminators.Contains(instructionOpcode);
            }
            lastInstructionWasData = instructionIsData;
            lastInstructionWasString = instructionIsString;
            lastMnemonic = mnemonic;
            lastOperands = operands;
            lastImportStack = importStack;
            if (!instructionIsData && !instructionIsImport)
            {
                insertedAnyExecutable = true;
            }
        }

        // Analyzer methods
        // (Rolling = runs as each instruction is processed, Final = runs after all instructions have been processed)

        private bool Analyzer_Rolling_NonFatalError_0001()
        {
            // Non-Fatal Error 0001: Instruction writes to the rpo register.
            if (newBytes.Length > 0 && !instructionIsData
                && writingInstructions.TryGetValue(instructionOpcode, out int[]? writtenOperands))
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
            if (newBytes.Length > 0 && !instructionIsData
                && divisionByLiteral.TryGetValue(instructionOpcode, out int literalOperandIndex))
            {
                if (operands[literalOperandIndex][0] == ':')
                {
                    return false;
                }
                _ = Assembler.ParseLiteral(operands[literalOperandIndex], false, out ulong number);
                return number == 0;
            }
            return false;
        }

        private bool Analyzer_Rolling_NonFatalError_0003()
        {
            // Non-Fatal Error 0003: File has an entry point explicitly defined,
            // but the program is being assembled into v1 format which doesn't support them.
            return isEntry && usingV1Format;
        }

        private bool Analyzer_Rolling_Warning_0001()
        {
            // Warning 0001: Data insertion is not directly preceded by unconditional jump, return, or halt instruction.
            return instructionIsData && insertedAnyExecutable && !lastInstructionWasTerminator && !lastInstructionWasData;
        }

        private List<Warning> Analyzer_Final_Warning_0002()
        {
            // Warning 0002: Jump/Call target label points to data, not executable code.
            List<Warning> warnings = new();
            foreach (((int jumpLine, string jumpFile), ulong labelAddress) in jumpCallToLabels)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)labelAddress..]);
                if (dataAddresses.Contains(address))
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, jumpFile, jumpLine,
                        lineMnemonics[(jumpFile, jumpLine)], lineOperands[(jumpFile, jumpLine)],
                        lineText[(jumpFile, jumpLine)]));
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
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)labelAddress..]);
                if (address >= currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0003, jumpFile, jumpLine,
                        lineMnemonics[(jumpFile, jumpLine)], lineOperands[(jumpFile, jumpLine)],
                        lineText[(jumpFile, jumpLine)]));
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
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)labelAddress..]);
                if (!dataAddresses.Contains(address) && address < currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0004, writeFile, writeLine,
                        lineMnemonics[(writeFile, writeLine)], lineOperands[(writeFile, writeLine)],
                        lineText[(writeFile, writeLine)]));
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
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)labelAddress..]);
                if (!dataAddresses.Contains(address) && address < currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0005, writeFile, writeLine,
                        lineMnemonics[(writeFile, writeLine)], lineOperands[(writeFile, writeLine)],
                        lineText[(writeFile, writeLine)]));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0006()
        {
            // Warning 0006: String insertion is not immediately followed by a 0 (null) byte.
            List<Warning> warnings = new();
            foreach ((int stringLine, string stringFile) in endingStringInsertionLines)
            {
                string[] stringOperands = lineOperands[(stringFile, stringLine)];
                byte[] stringBytes = Assembler.ParseLiteral(stringOperands[0], true);
                if (stringBytes[^1] == 0)
                {
                    // String itself is terminated with null (likely '\0')
                    continue;
                }
                ulong address = lineAddresses[(stringFile, stringLine)] + (uint)stringBytes.Length;
                if (address >= (uint)finalProgram.Length || finalProgram[(int)address] != 0)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0006, stringFile, stringLine,
                        lineMnemonics[(stringFile, stringLine)], stringOperands, lineText[(stringFile, stringLine)]));
                }
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Warning_0007()
        {
            // Warning 0007: Numeric literal is too large for the given move instruction. Upper bits will be truncated at runtime.
            if (newBytes.Length > 0 && !instructionIsData && moveLiteral.Contains(instructionOpcode)
                && moveLimits.TryGetValue(instructionOpcode, out (ulong MaxValue, long MinValue) limit))
            {
                if (operands[1][0] == ':')
                {
                    return false;
                }
                _ = Assembler.ParseLiteral(operands[1], false, out ulong number);
                return operands[1][0] == '-'
                    ? (long)number < limit.MinValue
                    : number > limit.MaxValue;
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0008()
        {
            // Warning 0008: Unreachable code detected.
            return !labelled && lastInstructionWasTerminator && !instructionIsData && !instructionIsImport;
        }

        private List<Warning> Analyzer_Final_Warning_0009()
        {
            // Warning 0009: Program runs to end of file without being terminated by unconditional jump, return, or halt.
            if (!lastInstructionWasTerminator && !lastInstructionWasData)
            {
                return new List<Warning>
                {
                    new(WarningSeverity.Warning, 0009, file, line, mnemonic, operands, lineText[(file, line)])
                };
            }
            return new List<Warning>();
        }

        private bool Analyzer_Rolling_Warning_0010()
        {
            // Warning 0010: File import is not directly preceded by unconditional jump, return, or halt instruction.
            return !lastInstructionWasTerminator && !lastInstructionWasData && insertedAnyExecutable && instructionIsImport;
        }

        private bool Analyzer_Rolling_Warning_0011()
        {
            // Warning 0011: Instruction writes to the rsf register.
            if (newBytes.Length > 0 && !instructionIsData
                && writingInstructions.TryGetValue(instructionOpcode, out int[]? writtenOperands))
            {
                return writtenOperands.Any(operandIndex => operands[operandIndex].ToLower() == "rsf");
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0012()
        {
            // Warning 0012: Instruction writes to the rsb register.
            if (newBytes.Length > 0 && !instructionIsData
                && writingInstructions.TryGetValue(instructionOpcode, out int[]? writtenOperands))
            {
                return writtenOperands.Any(operandIndex => operands[operandIndex].ToLower() == "rsb");
            }
            return false;
        }

        private List<Warning> Analyzer_Final_Warning_0013()
        {
            // Warning 0013: Jump/Call target label points to itself, resulting in an unbreakable infinite loop.
            List<Warning> warnings = new();
            foreach (((int jumpLine, string jumpFile), ulong labelAddress) in jumpsCalls)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)labelAddress..]);
                if (address == labelAddress - 1)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0013, jumpFile, jumpLine,
                        lineMnemonics[(jumpFile, jumpLine)], lineOperands[(jumpFile, jumpLine)],
                        lineText[(jumpFile, jumpLine)]));
                }
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Warning_0014()
        {
            // Warning 0014: Unlabelled executable code found after data insertion.
            return newBytes.Length > 0 && !instructionIsData && lastInstructionWasData && !labelled;
        }

        private bool Analyzer_Rolling_Warning_0015()
        {
            // Warning 0015: Code follows an imported file that is not terminated by unconditional jump, return, or halt instruction.
            return importStack.Count < lastImportStack.Count && !lastInstructionWasTerminator && !lastInstructionWasData;
        }

        private bool Analyzer_Rolling_Warning_0016()
        {
            // Warning 0016: Addresses are 64-bit values, however this move instruction moves less than 64 bits.
            return newBytes.Length > 0 && !instructionIsData && moveLiteral.Contains(instructionOpcode) && operands[1][0] == ':'
                && moveLimits.TryGetValue(instructionOpcode, out (ulong MaxValue, long MinValue) limit) && limit.MaxValue != ulong.MaxValue;
        }

        private bool Analyzer_Rolling_Warning_0017()
        {
            // Warning 0017: Entry point points to data, not executable code.
            return isEntry && instructionIsData;
        }

        private bool Analyzer_Rolling_Warning_0018()
        {
            // Warning 0018: Entry point points to an import.
            return isEntry && instructionIsImport;
        }

        private bool Analyzer_Rolling_Warning_0019()
        {
            // Warning 0019: Signed literal given to an instruction that expects an unsigned literal.
            return newBytes.Length > 0 && !instructionIsData && !signedLiteralAccepting.Contains(instructionOpcode) && !floatLiteralOnly.Contains(instructionOpcode)
                && operands.Any(o => Assembler.DetermineOperandType(o) == OperandType.Literal && o[0] != ':' && o[0] == '-' && !o.Contains('.'));
        }

        private bool Analyzer_Rolling_Warning_0020()
        {
            // Warning 0020: Floating point literal given to an instruction that expects an integer literal.
            return newBytes.Length > 0 && !instructionIsData && !floatLiteralAccepting.Contains(instructionOpcode)
                && operands.Any(o => Assembler.DetermineOperandType(o) == OperandType.Literal && o[0] != ':' && o.Contains('.'));
        }

        private bool Analyzer_Rolling_Warning_0021()
        {
            // Warning 0021: Integer literal given to an instruction that expects a floating point literal. Put `.0` at the end of the literal to make it floating point.
            return newBytes.Length > 0 && !instructionIsData && floatLiteralOnly.Contains(instructionOpcode)
                && operands.Any(o => Assembler.DetermineOperandType(o) == OperandType.Literal && o[0] != ':' && !o.Contains('.'));
        }

        private bool Analyzer_Rolling_Warning_0022()
        {
            // Warning 0022: Value is too large for a signed instruction. This positive value will overflow into a negative one.
            if (newBytes.Length > 0 && !instructionIsData && signedLiteralOnly.Contains(instructionOpcode))
            {
                foreach (string operand in operands)
                {
                    if (Assembler.DetermineOperandType(operand) == OperandType.Literal && operand[0] != ':')
                    {
                        _ = Assembler.ParseLiteral(operand, false, out ulong number);
                        if (number > long.MaxValue)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0023()
        {
            // Warning 0023: Addresses are unsigned, however this operation is signed.
            return newBytes.Length > 0 && !instructionIsData && signedLiteralOnly.Contains(instructionOpcode)
                && operands.Any(o => Assembler.DetermineOperandType(o) == OperandType.Literal && o[0] == ':');
        }

        private bool Analyzer_Rolling_Warning_0024()
        {
            // Warning 0024: Addresses are integers, however this operation is floating point.
            return newBytes.Length > 0 && !instructionIsData && floatLiteralOnly.Contains(instructionOpcode)
                && operands.Any(o => Assembler.DetermineOperandType(o) == OperandType.Literal && o[0] == ':');
        }

        private bool Analyzer_Rolling_Warning_0025()
        {
            // Warning 0025: Use of an extension instruction when assembling to v1 format.
            return usingV1Format && newBytes.Length > 0 && !instructionIsData && newBytes[0] == 0xFF;
        }

        private bool Analyzer_Rolling_Suggestion_0001()
        {
            // Suggestion 0001: Avoid use of NOP instruction.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x01);
        }

        private bool Analyzer_Rolling_Suggestion_0002()
        {
            // Suggestion 0002: Use the `PAD` directive instead of chaining `DAT 0` directives.
            if (mnemonic.ToUpper() == "DAT" && lastMnemonic.ToUpper() == "DAT")
            {
                if (operands[0][0] is ':' or '"' || lastOperands[0][0] is ':' or '"')
                {
                    return false;
                }
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
                        lineMnemonics[(impFile, impLine)], lineOperands[(impFile, impLine)],
                        lineText[(impFile, impLine)]));
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
                    warnings.Add(new Warning(WarningSeverity.Suggestion, 0004, dataFile, dataLine,
                        lineMnemonics[(dataFile, dataLine)], lineOperands[(dataFile, dataLine)],
                        lineText[(dataFile, dataLine)]));
                }
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Suggestion_0005()
        {
            // Suggestion 0005: Use `TST {reg}, {reg}` instead of `CMP {reg}, 0`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x75) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0006()
        {
            // Suggestion 0006: Use `XOR {reg}, {reg}` instead of `MV{B|W|D|Q} {reg}, 0`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && moveRegLit.Contains(instructionOpcode) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0007()
        {
            // Suggestion 0007: Use `ICR {reg}` instead of `ADD {reg}, 1`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x11) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0008()
        {
            // Suggestion 0008: Use `DCR {reg}` instead of `SUB {reg}, 1`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x21) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0009()
        {
            // Suggestion 0009: Operation has no effect.
            if (instructionIsData || newBytes.Length == 0 || (operands.Length > 0 && operands[^1][0] == ':'))
            {
                return false;
            }
            if (instructionOpcode.ExtensionSet == 0x0)
            {
                switch (instructionOpcode.InstructionCode)
                {
                    // Add, Subtract, Shift, Or, or Xor by 0
                    case 0x11:
                    case 0x21:
                    case 0x51:
                    case 0x55:
                    case 0x65:
                    case 0x69:
                        if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0)
                        {
                            return true;
                        }
                        break;
                    // Multiply by 1
                    case 0x31:
                        if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 1)
                        {
                            return true;
                        }
                        break;
                    // And by all 1 bits
                    case 0x61:
                        if (BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == ulong.MaxValue)
                        {
                            return true;
                        }
                        break;
                }
            }
            if (divisionByLiteral.TryGetValue(instructionOpcode, out int literalOperandIndex))
            {
                if (operands[literalOperandIndex][0] == ':')
                {
                    return false;
                }
                _ = Assembler.ParseLiteral(operands[literalOperandIndex], false, out ulong number);
                // Division by 1
                return number == 1;
            }
            return false;
        }

        private bool Analyzer_Rolling_Suggestion_0010()
        {
            // Suggestion 0010: Shift operation shifts by 64 bits or more, which will always shift out all bits.
            return newBytes.Length > 0 && !instructionIsData && shiftByLiteral.Contains(instructionOpcode) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) >= 64;
        }

        private bool Analyzer_Rolling_Suggestion_0011()
        {
            // Suggestion 0011: Remove leading 0 digits from denary number.
            foreach (string operand in operands)
            {
                string operandClean = operand.Replace("_", "");
                if (ulong.TryParse(operandClean, out ulong number) && operandClean[0] == '0')
                {
                    // If number is 0, don't create suggestion unless there is more than one 0 digit.
                    return number != 0 || operandClean.Length > 1;
                }
            }
            return false;
        }

        private bool Analyzer_Rolling_Suggestion_0012()
        {
            // Suggestion 0012: Remove useless `PAD 0` directive.
            if (mnemonic.ToUpper() == "PAD")
            {
                if (operands[0][0] == ':')
                {
                    return false;
                }
                _ = Assembler.ParseLiteral(operands[0], false, out ulong number);
                return number == 0;
            }
            return false;
        }

        private bool Analyzer_Rolling_Suggestion_0013()
        {
            // Suggestion 0013: Use `DCR {reg}` instead of `ADD {reg}, -1`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x11) && operands[1][0] != ':'
                && (long)BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == -1;
        }

        private bool Analyzer_Rolling_Suggestion_0014()
        {
            // Suggestion 0014: Use `ICR {reg}` instead of `SUB {reg}, -1`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x21) && operands[1][0] != ':'
                && (long)BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == -1;
        }
    }
}
