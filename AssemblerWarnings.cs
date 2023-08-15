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
                { 0002, Analyzer_Rolling_Warning_0002 },
                { 0003, Analyzer_Rolling_Warning_0003 },
                { 0004, Analyzer_Rolling_Warning_0004 },
                { 0005, Analyzer_Rolling_Warning_0005 },
                { 0006, Analyzer_Rolling_Warning_0006 },
                { 0007, Analyzer_Rolling_Warning_0007 },
                { 0008, Analyzer_Rolling_Warning_0008 },
                { 0009, Analyzer_Rolling_Warning_0009 },
                { 0010, Analyzer_Rolling_Warning_0010 },
                { 0011, Analyzer_Rolling_Warning_0011 },
                { 0012, Analyzer_Rolling_Warning_0012 },
                { 0013, Analyzer_Rolling_Warning_0013 },
            };
            suggestionRollingAnalyzers = new()
            {
                { 0001, Analyzer_Rolling_Suggestion_0001 },
                { 0002, Analyzer_Rolling_Suggestion_0002 },
                { 0003, Analyzer_Rolling_Suggestion_0003 },
                { 0004, Analyzer_Rolling_Suggestion_0004 },
                { 0005, Analyzer_Rolling_Suggestion_0005 },
                { 0006, Analyzer_Rolling_Suggestion_0006 },
                { 0007, Analyzer_Rolling_Suggestion_0007 },
                { 0008, Analyzer_Rolling_Suggestion_0008 },
                { 0009, Analyzer_Rolling_Suggestion_0009 },
                { 0010, Analyzer_Rolling_Suggestion_0010 },
                { 0011, Analyzer_Rolling_Suggestion_0011 },
                { 0012, Analyzer_Rolling_Suggestion_0012 },
                { 0013, Analyzer_Rolling_Suggestion_0013 },
            };

            nonFatalErrorFinalAnalyzers = new();
            warningFinalAnalyzers = new()
            {
                { 0002, Analyzer_Final_Warning_0002 },
                { 0003, Analyzer_Final_Warning_0003 },
                { 0004, Analyzer_Final_Warning_0004 },
                { 0005, Analyzer_Final_Warning_0005 },
                { 0009, Analyzer_Final_Warning_0009 },
            };
            suggestionFinalAnalyzers = new()
            {
                { 0003, Analyzer_Final_Suggestion_0003 },
                { 0004, Analyzer_Final_Suggestion_0004 },
            };
        }

        private bool Analyzer_Rolling_NonFatalError_0001()
        {
            // Non-Fatal Error 0001: Instruction writes to the rpo register.
            if (writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
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
            if (divisionByLiteral.TryGetValue(newBytes[0], out int literalOperandIndex))
            {
                _ = Assembler.ParseLiteral(operands[literalOperandIndex], false, out ulong number);
                return number == 0;
            }
            return false;
        }

        private bool Analyzer_Rolling_Warning_0001()
        {

        }

        private bool Analyzer_Rolling_Warning_0002()
        {

        }

        private List<Warning> Analyzer_Final_Warning_0002()
        {

        }

        private bool Analyzer_Rolling_Warning_0003()
        {

        }

        private List<Warning> Analyzer_Final_Warning_0003()
        {

        }

        private bool Analyzer_Rolling_Warning_0004()
        {

        }

        private List<Warning> Analyzer_Final_Warning_0004()
        {

        }

        private bool Analyzer_Rolling_Warning_0005()
        {

        }

        private List<Warning> Analyzer_Final_Warning_0005()
        {

        }

        private bool Analyzer_Rolling_Warning_0006()
        {

        }

        private bool Analyzer_Rolling_Warning_0007()
        {
            // Warning 0007: Numeric literal is too large for the given move instruction. Upper bits will be truncated at runtime.
            if (moveLiteral.Contains(newBytes[0]) && moveBitCounts.TryGetValue(newBytes[0], out int maxBits))
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

        }

        private bool Analyzer_Rolling_Warning_0009()
        {

        }

        private List<Warning> Analyzer_Final_Warning_0009()
        {

        }

        private bool Analyzer_Rolling_Warning_0010()
        {

        }

        private bool Analyzer_Rolling_Warning_0011()
        {
            // Warning 0011: Instruction writes to the rsf register.
            if (writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
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
            if (writingInstructions.TryGetValue(newBytes[0], out int[]? writtenOperands))
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
            
        }

        private bool Analyzer_Rolling_Suggestion_0001()
        {
            // Suggestion 0001: Avoid use of NOP instruction.
            return newBytes[0] == 0x01;
        }

        private bool Analyzer_Rolling_Suggestion_0002()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0003()
        {

        }

        private List<Warning> Analyzer_Final_Suggestion_0003()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0004()
        {

        }

        private List<Warning> Analyzer_Final_Suggestion_0004()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0005()
        {
            // Suggestion 0005: Use `TST {reg}, {reg}` instead of `CMP {reg}, 0`, as it results in less bytes.
            return newBytes[0] == 0x75 && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0006()
        {
            // Suggestion 0006: Use `XOR {reg}, {reg}` instead of `MV{B|W|D|Q} {reg}, 0`, as it results in less bytes.
            return moveRegLit.Contains(newBytes[0]) && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 0;
        }

        private bool Analyzer_Rolling_Suggestion_0007()
        {
            // Suggestion 0007: Use `INC {reg}` instead of `ADD {reg}, 1`, as it results in less bytes.
            return newBytes[0] == 0x11 && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0008()
        {
            // Suggestion 0008: Use `DEC {reg}` instead of `SUB {reg}, 1`, as it results in less bytes.
            return newBytes[0] == 0x21 && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) == 1;
        }

        private bool Analyzer_Rolling_Suggestion_0009()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0010()
        {
            // Suggestion 0010: Operation has no effect.
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

        private bool Analyzer_Rolling_Suggestion_0011()
        {
            // Suggestion 0011: Shift operation shifts by 64 bits or more, which will always result in 0. Use `XOR {reg}, {reg}` instead.
            return shiftByLiteral.Contains(newBytes[0]) && BinaryPrimitives.ReadUInt64LittleEndian(newBytes.AsSpan()[2..]) >= 64;
        }

        private bool Analyzer_Rolling_Suggestion_0012()
        {
            // Suggestion 0012: Remove leading 0 digits from denary number.
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

        private bool Analyzer_Rolling_Suggestion_0013()
        {
            // Suggestion 0013: Remove useless `PAD 0` directive.
            if (mnemonic.ToUpper() == "PAD")
            {
                _ = Assembler.ParseLiteral(operands[0], false, out ulong number);
                return number == 0;
            }
            return false;
        }
    }
}
