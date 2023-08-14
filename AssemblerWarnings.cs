namespace AssEmbly
{
    public enum WarningSeverity
    {
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
        public readonly int ColumnStart;
        public readonly int ColumnEnd;

        /// <summary>
        /// Index 0 will always be mnemonic. Index 1 and onwards are opcodes, if any.
        /// </summary>
        public readonly string[] InstructionElements;

        public readonly string Message => string.Format(AssemblerWarnings.GetMessagesForSeverity(Severity)[Code], InstructionElements);

        public Warning(WarningSeverity severity, int code, string file, int line, int columnStart, int columnEnd, string mnemonic, string[] operands)
        {
            Severity = severity;
            Code = code;
            File = file;
            Line = line;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;

            InstructionElements = new string[operands.Length + 1];
            InstructionElements[0] = mnemonic;
            Array.Copy(operands, 0, InstructionElements, 1, operands.Length);
        }
    }

    public delegate bool RollingWarningAnalyzer(byte[] newBytes, string mnemonic, string[] operands);
    public delegate bool FinalWarningAnalyzer();

    public partial class AssemblerWarnings
    {

        private static readonly Dictionary<int, RollingWarningAnalyzer> nonFatalErrorRollingAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, RollingWarningAnalyzer> warningRollingAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, RollingWarningAnalyzer> suggestionRollingAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, FinalWarningAnalyzer> nonFatalErrorFinalAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, FinalWarningAnalyzer> warningFinalAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, FinalWarningAnalyzer> suggestionFinalAnalyzers = new()
        {
            // TODO: Implement
        };

        public HashSet<bool> EnabledNonFatalErrors = new()
        {
            // TODO: All by default
        };

        public HashSet<bool> EnabledWarnings = new()
        {
            // TODO: All by default
        };

        public HashSet<bool> EnabledSuggestions = new()
        {
            // TODO: All by default
        };

        /// <summary>
        /// Update the state of the class instance with the next instruction in the program being analyzed.
        /// </summary>
        /// <param name="newBytes">The bytes of the next instruction to check for warnings.</param>
        /// <param name="mnemonic">The mnemonic that was used in the instruction.</param>
        /// <param name="operands">The operands that were used in the instruction.</param>
        /// <param name="line">The file-based 0-indexed line that the instruction was assembled from.</param>
        /// <param name="file">
        /// The path to the file that the instruction was assembled from, or <see cref="string.Empty"/> for the base file.
        /// </param>
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes, string mnemonic, string[] operands, int line, string file)
        {

        }

        /// <summary>
        /// Call this after all program instructions have been given to <see cref="NextInstruction"/>
        /// to run analyzers that need the entire program to work.
        /// </summary>
        /// <returns>An array of any warnings caused by final analysis.</returns>
        public Warning[] Finalize()
        {

        }
    }
}
