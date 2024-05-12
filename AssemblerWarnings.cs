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
        public readonly FilePosition Position;
        public readonly string MacroName;
        public readonly string OriginalLine;

        /// <summary>
        /// Index 0 will always be mnemonic. Index 1 and onwards are opcodes, if any.
        /// </summary>
        public readonly string[] InstructionElements;

        public readonly string Message;

        public Warning(WarningSeverity severity, int code, FilePosition filePosition, string mnemonic, string[] operands,
            string originalLine, string? macroName, string? message = null)
        {
            Severity = severity;
            Code = code;
            Position = filePosition;
            MacroName = macroName ?? "";
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
        private string preVariableLine = "";
        private string[] operands = Array.Empty<string>();
        private FilePosition filePosition;
        private int firstLine;
        private string? macroName;
        private int macroLineDepth;
        private bool labelled;
        private bool isEntry;
        private readonly bool usingV1Format;
        private Stack<Assembler.ImportStackFrame> importStack = new();

        private readonly Dictionary<(FilePosition Position, int MacroDepth), string> lineText = new();

        private readonly Dictionary<string, (FilePosition Position, string? MacroName)> labelDefinitionPositions = new();

        private byte[] finalProgram = Array.Empty<byte>();
        private ulong entryPoint = 0;
        private HashSet<string> referencedLabels = new();

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
        /// <param name="preVariableLine">The text content of the assembled line before parsing assembler variable names.</param>
        /// <param name="filePosition">The line and file that the instruction was assembled from.</param>
        /// <param name="labelled">Was this instruction preceded by one or more label definitions?</param>
        /// <param name="isEntry">Is this instruction the entry point?</param>
        /// <param name="importStack">The current state of the program's import stack.</param>
        /// <param name="macroName">The name of the current macro being expanded, or <see langword="null"/> if no macro is.</param>
        /// <param name="macroLineDepth">The number of lines of any macro already assembled for the current line in the file.</param>
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes, string mnemonic, string[] operands, string preVariableLine, FilePosition filePosition, bool labelled,
            bool isEntry, string rawLine, IEnumerable<Assembler.ImportStackFrame> importStack, string? macroName, int macroLineDepth)
        {
            this.newBytes = newBytes;
            this.mnemonic = mnemonic;
            this.operands = operands;
            this.preVariableLine = preVariableLine;
            this.filePosition = filePosition;
            this.macroName = macroName;
            this.macroLineDepth = macroLineDepth;
            this.labelled = labelled;
            this.isEntry = isEntry;
            this.importStack = new Stack<Assembler.ImportStackFrame>(importStack);
            lineText[(filePosition, macroLineDepth)] = rawLine;

            if (firstLine == 0)
            {
                firstLine = filePosition.Line;
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
                        new Warning(WarningSeverity.NonFatalError, code, filePosition, mnemonic, operands, rawLine, macroName));
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
                        new Warning(WarningSeverity.Warning, code, filePosition, mnemonic, operands, rawLine, macroName));
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
                        new Warning(WarningSeverity.Suggestion, code, filePosition, mnemonic, operands, rawLine, macroName));
                }
            }
            PostAnalyzeStateUpdate();

            return warnings.ToArray();
        }

        /// <summary>
        /// Call this whenever a new label is defined.
        /// </summary>
        /// <param name="filePosition">The line and file that the instruction was assembled from.</param>
        /// <param name="macroName">The name of the current macro being expanded, or <see langword="null"/> if no macro is.</param>
        public void NewLabel(string labelName, FilePosition filePosition, string? macroName)
        {
            labelDefinitionPositions[labelName] = (filePosition, macroName);
        }

        /// <summary>
        /// Call this after all program instructions have been given to <see cref="NextInstruction"/>
        /// to run analyzers that need the entire program to work.
        /// </summary>
        /// <param name="finalProgram">The fully assembled program, with all label locations inserted.</param>
        /// <param name="entryPoint">The address that the program will start executing from.</param>
        /// <param name="referencedLabels">A set of all label names referenced at any point by the program.</param>
        /// <returns>An array of any warnings caused by final analysis.</returns>
        public Warning[] Finalize(byte[] finalProgram, ulong entryPoint, HashSet<string> referencedLabels)
        {
            this.finalProgram = finalProgram;
            this.entryPoint = entryPoint;
            this.referencedLabels = referencedLabels;

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
                { 0004, Analyzer_Rolling_NonFatalError_0004 },
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
                { 0018, Analyzer_Rolling_Warning_0018 },
                { 0019, Analyzer_Rolling_Warning_0019 },
                { 0020, Analyzer_Rolling_Warning_0020 },
                { 0021, Analyzer_Rolling_Warning_0021 },
                { 0022, Analyzer_Rolling_Warning_0022 },
                { 0023, Analyzer_Rolling_Warning_0023 },
                { 0024, Analyzer_Rolling_Warning_0024 },
                { 0025, Analyzer_Rolling_Warning_0025 },
                { 0026, Analyzer_Rolling_Warning_0026 },
                { 0027, Analyzer_Rolling_Warning_0027 },
                { 0028, Analyzer_Rolling_Warning_0028 },
                { 0029, Analyzer_Rolling_Warning_0029 },
                { 0030, Analyzer_Rolling_Warning_0030 },
                { 0031, Analyzer_Rolling_Warning_0031 },
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
                { 0015, Analyzer_Rolling_Suggestion_0015 },
                { 0016, Analyzer_Rolling_Suggestion_0016 },
                { 0017, Analyzer_Rolling_Suggestion_0017 },
                { 0019, Analyzer_Rolling_Suggestion_0019 },
                { 0020, Analyzer_Rolling_Suggestion_0020 },
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
                { 0017, Analyzer_Final_Warning_0017 },
            };
            suggestionFinalAnalyzers = new Dictionary<int, FinalWarningAnalyzer>
            {
                { 0003, Analyzer_Final_Suggestion_0003 },
                { 0004, Analyzer_Final_Suggestion_0004 },
                { 0018, Analyzer_Final_Suggestion_0018 },
            };
        }

        // Analyzer state variables

        private Opcode instructionOpcode;
        private ulong operandStart;
        private readonly Dictionary<(FilePosition Position, int MacroDepth), ulong> lineAddresses = new();
        private readonly Dictionary<(FilePosition Position, int MacroDepth), string> lineMnemonics = new();
        private readonly Dictionary<(FilePosition Position, int MacroDepth), string[]> lineOperands = new();
        private bool instructionIsData;
        private bool instructionIsImport;
        private bool instructionIsString;
        private bool instructionIsExecutable;
        private bool instructionIsAsmOnce;
        private (FilePosition Position, string? MacroName, int MacroLineDepth)? entryPointDefinitionPosition = null;
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth)> dataInsertionLines = new();
        private readonly HashSet<ulong> executableAddresses = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth)> endingStringInsertionLines = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth)> importLines = new();
        private readonly Dictionary<string, int> lastExecutableLine = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth, ulong Address)> jumpCallToAddress = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth, ulong Address)> writesToAddress = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth, ulong Address)> readsFromAddress = new();
        private readonly List<(FilePosition Position, string? MacroName, int MacroLineDepth, ulong Address)> jumpsCalls = new();
        private readonly Dictionary<string, int> firstAsmOnceLineInFiles = new();

        private ulong currentAddress;
        private bool lastInstructionWasTerminator;
        private bool lastInstructionWasData;
        private bool lastInstructionWasImport;
        private bool lastInstructionWasString;
        private bool lastInstructionWasExecutable;
        private string lastMnemonic = "";
        private string[] lastOperands = Array.Empty<string>();
        private string? lastMacroName;
        private int lastMacroLineDepth;
        private Stack<Assembler.ImportStackFrame> lastImportStack = new();

        private void PreAnalyzeStateUpdate()
        {
            if (newBytes.Length > 0)
            {
                operandStart = 0;
                if (newBytes[0] == Opcode.FullyQualifiedMarker && newBytes.Length < 3)
                {
                    // We can't parse this data as an opcode properly,
                    // as it starts with 0xFF but there are not enough bytes for it to be a fully qualified opcode.
                    // Can happen with non-instruction statements like "%DAT 0xFF".
                    instructionOpcode = new Opcode(0x00, Opcode.FullyQualifiedMarker);
                    operandStart = 1;
                }
                else
                {
                    instructionOpcode = Opcode.ParseBytes(newBytes, ref operandStart);
                }
                operandStart++;
            }

            lineAddresses[(filePosition, macroLineDepth)] = currentAddress;
            lineMnemonics[(filePosition, macroLineDepth)] = mnemonic;
            lineOperands[(filePosition, macroLineDepth)] = operands;

            instructionIsData = dataInsertionDirectives.Contains(mnemonic);
            instructionIsImport = mnemonic.Equals("%IMP", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("IMP", StringComparison.OrdinalIgnoreCase);
            instructionIsAsmOnce = mnemonic.Equals("%ASM_ONCE", StringComparison.OrdinalIgnoreCase);

            instructionIsString = false;
            instructionIsExecutable = false;

            if (instructionIsData)
            {
                dataInsertionLines.Add((filePosition, macroName, macroLineDepth));
                if (operands[0][0] == '"' && (
                    mnemonic.Equals("%DAT", StringComparison.OrdinalIgnoreCase)
                    || mnemonic.Equals("DAT", StringComparison.OrdinalIgnoreCase)))
                {
                    instructionIsString = true;
                    if (lastInstructionWasString)
                    {
                        // Only store the last in a chain of string insertions
                        endingStringInsertionLines.RemoveAt(endingStringInsertionLines.Count - 1);
                    }
                    endingStringInsertionLines.Add((filePosition, macroName, macroLineDepth));
                }
            }
            else if (instructionIsImport)
            {
                importLines.Add((filePosition, macroName, macroLineDepth));
            }
            else if (newBytes.Length > 0)
            {
                lastExecutableLine[filePosition.File] = filePosition.Line;
                _ = executableAddresses.Add(currentAddress);
                instructionIsExecutable = true;
                if (jumpCallToAddressOpcodes.Contains(instructionOpcode))
                {
                    jumpCallToAddress.Add((filePosition, macroName, macroLineDepth, currentAddress + operandStart));
                }
                if (writeToMemory.Contains(instructionOpcode))
                {
                    writesToAddress.Add((filePosition, macroName, macroLineDepth, currentAddress + operandStart));
                }
                if (readValueFromMemory.TryGetValue(instructionOpcode, out ulong addressOpcodeOffset))
                {
                    readsFromAddress.Add((filePosition, macroName, macroLineDepth, currentAddress + operandStart + addressOpcodeOffset));
                }
                if (jumpCallToAddressOpcodes.Contains(instructionOpcode))
                {
                    jumpsCalls.Add((filePosition, macroName, macroLineDepth, currentAddress + operandStart));
                }
            }

            if (isEntry)
            {
                entryPointDefinitionPosition = (filePosition, macroName, macroLineDepth);
            }

            if (instructionIsAsmOnce)
            {
                firstAsmOnceLineInFiles.TryAdd(filePosition.File, filePosition.Line);
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
            lastInstructionWasImport = instructionIsImport;
            lastInstructionWasString = instructionIsString;
            lastInstructionWasExecutable = instructionIsExecutable;
            lastMnemonic = mnemonic;
            lastOperands = operands;
            lastImportStack = importStack;
            lastMacroName = macroName;
            lastMacroLineDepth = macroLineDepth;
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
                    if (operands[operandIndex].Equals("rpo", StringComparison.OrdinalIgnoreCase))
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

        private bool Analyzer_Rolling_NonFatalError_0004()
        {
            // Non-Fatal Error 0004: Allocating constant 0 bytes.
            if (allocationOfLiteral.Contains(instructionOpcode))
            {
                _ = Assembler.ParseLiteral(operands[1], false, out ulong number);
                return number == 0;
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0001()
        {
            // Warning 0001: Data insertion is not directly preceded by unconditional jump, return, or halt instruction.
            return instructionIsData && lastInstructionWasExecutable && !lastInstructionWasTerminator;
        }

        private List<Warning> Analyzer_Final_Warning_0002()
        {
            // Warning 0002: Jump/Call target address does not point to executable code.
            List<Warning> warnings = new();
            foreach ((FilePosition jumpPosition, string? jumpMacroName, int jumpMacroLineDepth, ulong jumpAddress) in jumpCallToAddress)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)jumpAddress..]);
                if (!executableAddresses.Contains(address))
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0002, jumpPosition,
                        lineMnemonics[(jumpPosition, jumpMacroLineDepth)], lineOperands[(jumpPosition, jumpMacroLineDepth)],
                        lineText[(jumpPosition, jumpMacroLineDepth)], jumpMacroName));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0003()
        {
            // Warning 0003: Jump/Call target address points to end of file, not executable code.
            List<Warning> warnings = new();
            foreach ((FilePosition jumpPosition, string? jumpMacroName, int jumpMacroLineDepth, ulong jumpAddress) in jumpCallToAddress)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)jumpAddress..]);
                if (address >= currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0003, jumpPosition,
                        lineMnemonics[(jumpPosition, jumpMacroLineDepth)], lineOperands[(jumpPosition, jumpMacroLineDepth)],
                        lineText[(jumpPosition, jumpMacroLineDepth)], jumpMacroName));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0004()
        {
            // Warning 0004: Instruction writes to an address pointing to executable code.
            List<Warning> warnings = new();
            foreach ((FilePosition writePosition, string? writeMacroName, int writeMacroLineDepth, ulong writeAddress) in writesToAddress)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)writeAddress..]);
                if (executableAddresses.Contains(address) && address < currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0004, writePosition,
                        lineMnemonics[(writePosition, writeMacroLineDepth)], lineOperands[(writePosition, writeMacroLineDepth)],
                        lineText[(writePosition, writeMacroLineDepth)], writeMacroName));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0005()
        {
            // Warning 0005: Instruction reads from an address pointing to executable code in a context that likely expects data.
            List<Warning> warnings = new();
            foreach ((FilePosition writePosition, string? writeMacroName, int writeMacroLineDepth, ulong writeAddress) in readsFromAddress)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)writeAddress..]);
                if (executableAddresses.Contains(address) && address < currentAddress)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0005, writePosition,
                        lineMnemonics[(writePosition, writeMacroLineDepth)], lineOperands[(writePosition, writeMacroLineDepth)],
                        lineText[(writePosition, writeMacroLineDepth)], writeMacroName));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Warning_0006()
        {
            // Warning 0006: String insertion is not immediately followed by a 0 (null) byte.
            List<Warning> warnings = new();
            foreach ((FilePosition stringPosition, string? stringMacroName, int stringMacroLineDepth) in endingStringInsertionLines)
            {
                string[] stringOperands = lineOperands[(stringPosition, stringMacroLineDepth)];
                byte[] stringBytes = Assembler.ParseLiteral(stringOperands[0], true);
                if (stringBytes[^1] == 0)
                {
                    // String itself is terminated with null (likely '\0')
                    continue;
                }
                ulong address = lineAddresses[(stringPosition, stringMacroLineDepth)] + (uint)stringBytes.Length;
                if (address >= (uint)finalProgram.Length || finalProgram[(int)address] != 0)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0006, stringPosition,
                        lineMnemonics[(stringPosition, stringMacroLineDepth)], stringOperands,
                        lineText[(stringPosition, stringMacroLineDepth)], stringMacroName));
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
            return !labelled && lastInstructionWasTerminator && !instructionIsData && !instructionIsImport && newBytes.Length > 0;
        }

        private List<Warning> Analyzer_Final_Warning_0009()
        {
            // Warning 0009: Program runs to end of file without being terminated by unconditional jump, return, or halt.
            if (!lastInstructionWasTerminator && lastInstructionWasExecutable && finalProgram.Length > 0)
            {
                return new List<Warning>
                {
                    new(WarningSeverity.Warning, 0009, filePosition, mnemonic, operands, lineText[(filePosition, lastMacroLineDepth)], lastMacroName)
                };
            }
            return new List<Warning>();
        }

        private bool Analyzer_Rolling_Warning_0010()
        {
            // Warning 0010: File import is not directly preceded by unconditional jump, return, or halt instruction.
            return !lastInstructionWasTerminator && lastInstructionWasExecutable && instructionIsImport;
        }

        private bool Analyzer_Rolling_Warning_0011()
        {
            // Warning 0011: Instruction writes to the rsf register.
            if (newBytes.Length > 0 && !instructionIsData
                && writingInstructions.TryGetValue(instructionOpcode, out int[]? writtenOperands))
            {
                return writtenOperands.Any(operandIndex => operands[operandIndex].Equals("rsf", StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0012()
        {
            // Warning 0012: Instruction writes to the rsb register.
            if (newBytes.Length > 0 && !instructionIsData
                && writingInstructions.TryGetValue(instructionOpcode, out int[]? writtenOperands))
            {
                return writtenOperands.Any(operandIndex => operands[operandIndex].Equals("rsb", StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        private List<Warning> Analyzer_Final_Warning_0013()
        {
            // Warning 0013: Jump/Call target address points to itself, resulting in an unbreakable infinite loop.
            List<Warning> warnings = new();
            foreach ((FilePosition jumpPosition, string? jumpMacroName, int jumpMacroLineDepth, ulong jumpAddress) in jumpsCalls)
            {
                ulong address = BinaryPrimitives.ReadUInt64LittleEndian(finalProgram.AsSpan()[(int)jumpAddress..]);
                if (address == jumpAddress - 1)
                {
                    warnings.Add(new Warning(WarningSeverity.Warning, 0013, jumpPosition,
                        lineMnemonics[(jumpPosition, jumpMacroLineDepth)], lineOperands[(jumpPosition, jumpMacroLineDepth)],
                        lineText[(jumpPosition, jumpMacroLineDepth)], jumpMacroName));
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
            return importStack.Count < lastImportStack.Count && !lastInstructionWasTerminator && !lastInstructionWasData
                && !lastInstructionWasImport && instructionIsExecutable;
        }

        private bool Analyzer_Rolling_Warning_0016()
        {
            // Warning 0016: Addresses are 64-bit values, however this move instruction moves less than 64 bits.
            return newBytes.Length > 0 && !instructionIsData && moveLiteral.Contains(instructionOpcode) && operands[1][0] == ':'
                && moveLimits.TryGetValue(instructionOpcode, out (ulong MaxValue, long MinValue) limit) && limit.MaxValue != ulong.MaxValue;
        }

        private List<Warning> Analyzer_Final_Warning_0017()
        {
            // Warning 0017: Entry point does not point to executable code.
            if (entryPointDefinitionPosition is not null && !executableAddresses.Contains(entryPoint))
            {
                FilePosition entryPointPosition = entryPointDefinitionPosition.Value.Position;
                int entryPointMacroLineDepth = entryPointDefinitionPosition.Value.MacroLineDepth;
                return new List<Warning>()
                {
                    new(WarningSeverity.Warning, 0017, entryPointPosition,
                        lineMnemonics[(entryPointPosition, entryPointMacroLineDepth)],
                        lineOperands[(entryPointPosition, entryPointMacroLineDepth)],
                        lineText[(entryPointPosition, entryPointMacroLineDepth)], entryPointDefinitionPosition.Value.MacroName)
                };
            }
            return new List<Warning>();
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
            return usingV1Format && newBytes.Length > 0 && !instructionIsData && newBytes[0] == Opcode.FullyQualifiedMarker;
        }

        private bool Analyzer_Rolling_Warning_0026()
        {
            // Warning 0026: %LABEL_OVERRIDE directive does not have any effect as it is not directly preceded by any label definitions.
            return mnemonic.Equals("%LABEL_OVERRIDE", StringComparison.OrdinalIgnoreCase) && !labelled;
        }

        private bool Analyzer_Rolling_Warning_0027()
        {
            // Warning 0027: Addresses are always positive integers, but a signed or floating point literal was given as the label address to the %LABEL_OVERRIDE directive.
            return mnemonic.Equals("%LABEL_OVERRIDE", StringComparison.OrdinalIgnoreCase) && operands[0][0] != ':' && (operands[0][0] == '-' || operands[0].Contains('.'));
        }

        private bool Analyzer_Rolling_Warning_0028()
        {
            // Warning 0028: The '@' prefix on the target assembler variable name is not required for this directive.
            //               Including it will result in the current value of the directive being used as the target variable name instead.
            return takesLiteralVariableName.TryGetValue(mnemonic, out int operandIndex) && Assembler.ParseLine(preVariableLine)[operandIndex + 1][0] == '@'
                // Only apply to %IF and %ELSE_IF for the DEF and NDEF operations
                && ((!mnemonic.Equals("%IF", StringComparison.OrdinalIgnoreCase) && !mnemonic.Equals("%ELSE_IF", StringComparison.OrdinalIgnoreCase)) || operands.Length == 2);
        }

        private bool Analyzer_Rolling_Warning_0029()
        {
            // Warning 0029: The value of assembler variables is always interpreted as an integer, but the provided value is floating point.
            return assemblerVariableLiteral.TryGetValue(mnemonic, out int[]? operandIndexes) && operandIndexes.Any(i => i < operands.Length && operands[i].Contains('.'));
        }

        private bool Analyzer_Rolling_Warning_0030()
        {
            // Warning 0030: This assembler variable operation will not work as expected with negative values.
            return (mnemonic.Equals("%VAROP", StringComparison.OrdinalIgnoreCase)
                    || mnemonic.Equals("%IF", StringComparison.OrdinalIgnoreCase)
                    || mnemonic.Equals("%ELSE_IF", StringComparison.OrdinalIgnoreCase))
                && operands.Length >= 3 && noNegativeVarop.Contains(operands[0]) && operands[2][0] == '-';
        }

        private bool Analyzer_Rolling_Warning_0031()
        {
            // Warning 0031: Both operands to this comparison are numeric literals, so the result will never change.
            return (mnemonic.Equals("%IF", StringComparison.OrdinalIgnoreCase)
                    || mnemonic.Equals("%ELSE_IF", StringComparison.OrdinalIgnoreCase))
                && operands.Length >= 3 && Assembler.ParseLine(preVariableLine).All(o => o[0] != '@');
        }

        private bool Analyzer_Rolling_Suggestion_0001()
        {
            // Suggestion 0001: Avoid use of NOP instruction.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x01);
        }

        private bool Analyzer_Rolling_Suggestion_0002()
        {
            // Suggestion 0002: Use the `%PAD` directive instead of chaining `%DAT 0` directives.
            if ((mnemonic.Equals("%DAT", StringComparison.OrdinalIgnoreCase)
                    || mnemonic.Equals("DAT", StringComparison.OrdinalIgnoreCase))
                && (lastMnemonic.Equals("%DAT", StringComparison.OrdinalIgnoreCase)
                    || lastMnemonic.Equals("DAT", StringComparison.OrdinalIgnoreCase)))
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
            // Suggestion 0003: Put importing directives at the end of the file,
            // unless the position of the directive is important given the file's contents.
            List<Warning> warnings = new();
            foreach ((FilePosition impPosition, string? impMacroName, int impMacroLineDepth) in importLines)
            {
                if (lastExecutableLine.TryGetValue(impPosition.File, out int execLine) && impPosition.Line < execLine)
                {
                    warnings.Add(new Warning(WarningSeverity.Suggestion, 0003, impPosition,
                        lineMnemonics[(impPosition, impMacroLineDepth)], lineOperands[(impPosition, impMacroLineDepth)],
                        lineText[(impPosition, impMacroLineDepth)], impMacroName));
                }
            }
            return warnings;
        }

        private List<Warning> Analyzer_Final_Suggestion_0004()
        {
            // Suggestion 0004: Put data at the end of the file, unless the position of the data is important.
            List<Warning> warnings = new();
            foreach ((FilePosition dataPosition, string? dataMacroName, int dataMacroLineDepth) in dataInsertionLines)
            {
                if (lastExecutableLine.TryGetValue(dataPosition.File, out int execLine) && dataPosition.Line < execLine)
                {
                    warnings.Add(new Warning(WarningSeverity.Suggestion, 0004, dataPosition,
                        lineMnemonics[(dataPosition, dataMacroLineDepth)], lineOperands[(dataPosition, dataMacroLineDepth)],
                        lineText[(dataPosition, dataMacroLineDepth)], dataMacroName));
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
            if (mnemonic.Equals("%VAROP", StringComparison.OrdinalIgnoreCase))
            {
                _ = Assembler.ParseLiteral(operands[2], false, out ulong parsedNumber);
                switch (operands[0].ToUpperInvariant())
                {
                    // Add, Subtract, Shift, Or, or Xor by 0
                    case "ADD":
                    case "SUB":
                    case "SHR":
                    case "SHL":
                    case "BIT_OR":
                    case "BIT_XOR":
                        if (parsedNumber == 0)
                        {
                            return true;
                        }
                        break;
                    // Multiply by 1
                    case "MUL":
                        if (parsedNumber == 1)
                        {
                            return true;
                        }
                        break;
                    // And by all 1 bits
                    case "BIT_AND":
                        if (parsedNumber == ulong.MaxValue)
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }
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
                    // Move, Or, or And a register by itself
                    case 0x60:
                    case 0x64:
                    case 0x98:
                        if (newBytes[(int)operandStart] == newBytes[(int)operandStart + 1])
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
            if (mnemonic.Equals("%ANALYZER", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("ANALYZER", StringComparison.OrdinalIgnoreCase))
            {
                // Analyzer codes are usually given with leading zeros, so don't suggest removing them
                return false;
            }
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
            // Suggestion 0012: Remove useless `%PAD 0` directive.
            if (mnemonic.Equals("%PAD", StringComparison.OrdinalIgnoreCase)
                || mnemonic.Equals("PAD", StringComparison.OrdinalIgnoreCase))
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
                && BinaryPrimitives.ReadInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == -1;
        }

        private bool Analyzer_Rolling_Suggestion_0014()
        {
            // Suggestion 0014: Use `ICR {reg}` instead of `SUB {reg}, -1`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x21) && operands[1][0] != ':'
                && BinaryPrimitives.ReadInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == -1;
        }

        private bool Analyzer_Rolling_Suggestion_0015()
        {
            // Suggestion 0015: Use `MVB {reg}, {reg}` instead of `AND {reg}, 0xFF`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x61) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0xFF;
        }

        private bool Analyzer_Rolling_Suggestion_0016()
        {
            // Suggestion 0016: Use `MVW {reg}, {reg}` instead of `AND {reg}, 0xFFFF`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x61) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0xFFFF;
        }

        private bool Analyzer_Rolling_Suggestion_0017()
        {
            // Suggestion 0017: Use `MVD {reg}, {reg}` instead of `AND {reg}, 0xFFFFFFFF`, as it results in less bytes.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x00, 0x61) && operands[1][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[((int)operandStart + 1)..]) == 0xFFFFFFFF;
        }

        private List<Warning> Analyzer_Final_Suggestion_0018()
        {
            // Suggestion 0018: Label "{label}" is defined but never used.
            List<Warning> warnings = new();
            // Iterate label definitions that are not present in the referenced labels set
            foreach ((string labelName, (FilePosition labelPosition, string? labelMacroName))
                in labelDefinitionPositions.ExceptBy(referencedLabels, kv => kv.Key))
            {
                string labelDefinitionText = ':' + labelName;
                warnings.Add(new Warning(WarningSeverity.Suggestion, 0018, labelPosition,
                    labelDefinitionText, Array.Empty<string>(), labelDefinitionText, labelMacroName));
            }
            return warnings;
        }

        private bool Analyzer_Rolling_Suggestion_0019()
        {
            // Suggestion 0019: Uses of %ASM_ONCE beyond the first in a file will never be reached.
            return instructionIsAsmOnce &&
                firstAsmOnceLineInFiles.TryGetValue(filePosition.File, out int firstAsmOnceLine) && firstAsmOnceLine < filePosition.Line;
        }

        private bool Analyzer_Rolling_Suggestion_0020()
        {
            // Suggestion 0020: Use the `HLT` instruction instead of `EXTD_HLT` when the exit code is always 0.
            return newBytes.Length > 0 && !instructionIsData && instructionOpcode == new Opcode(0x03, 0x21) && operands[0][0] != ':'
                && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[(int)operandStart..]) == 0;
        }
    }
}
