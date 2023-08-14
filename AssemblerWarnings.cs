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

    public delegate bool RollingWarningAnalyzer(byte[] newBytes, string mnemonic, string[] operands);
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
            List<Warning> warnings = new();

            foreach ((int code, RollingWarningAnalyzer rollingAnalyzer) in nonFatalErrorRollingAnalyzers)
            {
                if (DisabledNonFatalErrors.Contains(code))
                {
                    continue;
                }
                if (rollingAnalyzer(newBytes, mnemonic, operands))
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
                if (rollingAnalyzer(newBytes, mnemonic, operands))
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
                if (rollingAnalyzer(newBytes, mnemonic, operands))
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
                { 0014, Analyzer_Rolling_Suggestion_0014 },
            };

            nonFatalErrorFinalAnalyzers = new();
            warningFinalAnalyzers = new()
            {
                { 0002, Analyzer_Final_Warning_0002 },
                { 0003, Analyzer_Final_Warning_0003 },
                { 0004, Analyzer_Final_Warning_0004 },
                { 0005, Analyzer_Final_Warning_0005 },
                { 0008, Analyzer_Final_Warning_0008 },
                { 0009, Analyzer_Final_Warning_0009 },
            };
            suggestionFinalAnalyzers = new()
            {
                { 0003, Analyzer_Final_Suggestion_0003 },
                { 0004, Analyzer_Final_Suggestion_0004 },
            };
        }

        private bool Analyzer_Rolling_NonFatalError_0001(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_NonFatalError_0002(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0001(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0002(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0002()
        {

        }

        private bool Analyzer_Rolling_Warning_0003(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0003()
        {

        }

        private bool Analyzer_Rolling_Warning_0004(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0004()
        {

        }

        private bool Analyzer_Rolling_Warning_0005(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0005()
        {

        }

        private bool Analyzer_Rolling_Warning_0006(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0007(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0008(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0008()
        {

        }

        private bool Analyzer_Rolling_Warning_0009(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Warning_0009()
        {

        }

        private bool Analyzer_Rolling_Warning_0010(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0011(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0012(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Warning_0013(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0001(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0002(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0003(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Suggestion_0003()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0004(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private List<Warning> Analyzer_Final_Suggestion_0004()
        {

        }

        private bool Analyzer_Rolling_Suggestion_0005(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0006(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0007(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0008(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0009(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0010(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0011(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0012(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0013(byte[] newBytes, string mnemonic, string[] operands)
        {

        }

        private bool Analyzer_Rolling_Suggestion_0014(byte[] newBytes, string mnemonic, string[] operands)
        {

        }
    }
}
