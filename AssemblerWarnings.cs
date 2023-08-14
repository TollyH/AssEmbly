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
        public readonly int Line;
        public readonly int Column;

        public string Message => AssemblerWarnings.GetMessagesForSeverity(Severity)[Code];

        public Warning(WarningSeverity severity, int code, int line, int column)
        {
            Severity = severity;
            Code = code;
            Line = line;
            Column = column;
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
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes, string mnemonic, string[] operands)
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
