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

    public delegate bool WarningAnalyzer(byte[] newBytes);

    public partial class AssemblerWarnings
    {

        private static readonly Dictionary<int, WarningAnalyzer> nonFatalErrorAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, WarningAnalyzer> warningAnalyzers = new()
        {
            // TODO: Implement
        };

        private static readonly Dictionary<int, WarningAnalyzer> suggestionAnalyzers = new()
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
        /// <returns>An array of any warnings caused by the new instruction.</returns>
        public Warning[] NextInstruction(byte[] newBytes)
        {

        }
    }
}
