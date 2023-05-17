namespace AssEmbly
{
    public static class DebugInfo
    {
        public static readonly string Separator = "===============================================================================";

        public static readonly string DebugInfoFileHeader = @$"AssEmbly Debug Information File
Format Version: 0.1
Date: {{0:yyyy-MM-dd HH:mm:ss}}
Command Line: {{1}}
Total Program Size: {{2}} bytes
{Separator}
";

        public static readonly string AssembledInstructionsHeader = $"\n[1]: Assembled Instructions\n{Separator}";

        public static readonly string AddressLabelsHeader = $"\n[2]: Address Labels\n{Separator}";

        public static readonly string ResolvedImportsHeader = $"\n[3]: Resolved Imports\n{Separator}";

        /// <summary>
        /// Generates the contents of a debug information file based on the provided parameters.
        /// </summary>
        /// <param name="sourcePath">The path to the AssEmbly file that was assembled</param>
        /// <param name="destinationPath">The path to the generated executable file</param>
        /// <param name="totalProgramSize">The total size in bytes of the generated file</param>
        /// <param name="assembledInstructions">An array of addresses and the corresponding line of AssEmbly that generated them</param>
        /// <param name="addressLabels">An array of addresses combined with an array of label names pointing to that address</param>
        /// <param name="resolvedImports">An array of imported file names as seen in AssEmbly code along with the full file path to each one</param>
        /// <returns>A completely formatted debug info file string ready to be saved.</returns>
        public static string GenerateDebugInfoFile(ulong totalProgramSize,
            IList<(ulong, string)> assembledInstructions, IList<(ulong, List<string>)> addressLabels, IList<(string, string)> resolvedImports)
        {
            string fileText = string.Format(DebugInfoFileHeader, DateTime.Now, Environment.CommandLine, totalProgramSize);
            
            fileText += AssembledInstructionsHeader;
            foreach ((ulong address, string instruction) in assembledInstructions)
            {
                fileText += $"\n{address:X16} @ {instruction}";
            }

            fileText += $"\n{Separator}\n{AddressLabelsHeader}";
            foreach ((ulong address, List<string> labels) in addressLabels)
            {
                fileText += $"\n{address:X16} @ {string.Join(',', labels)}";
            }

            fileText += $"\n{Separator}\n{ResolvedImportsHeader}";
            foreach ((string sourceName, string resolvedName) in resolvedImports)
            {
                fileText += $"\n\"{sourceName}\" -> \"{resolvedName}\"";
            }

            fileText += '\n' + Separator;

            return fileText;
        }
    }
}
