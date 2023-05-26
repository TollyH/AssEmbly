using System.Text.RegularExpressions;

namespace AssEmbly
{
    public static class DebugInfo
    {
        public static readonly string FormatVersion = "0.1";

        public static readonly string Separator = "===============================================================================";

        public static readonly string DebugInfoFileHeader = @$"AssEmbly Debug Information File
Format Version: {FormatVersion}
Date: {{0:yyyy-MM-dd HH:mm:ss}}
Command Line: {{1}}
Total Program Size: {{2}} bytes
{Separator}
";

        public static readonly string AssembledInstructionsHeader = $"\r\n[1]: Assembled Instructions\r\n{Separator}";

        public static readonly string AddressLabelsHeader = $"\r\n[2]: Address Labels\r\n{Separator}";

        public static readonly string ResolvedImportsHeader = $"\r\n[3]: Resolved Imports\r\n{Separator}";

        public static readonly Regex DebugFileRegex = new(@"AssEmbly Debug Information File
Format Version: (?<Version>.+)
Date: .*
Command Line: .*
Total Program Size: .*
===============================================================================

\[1\]: Assembled Instructions
===============================================================================
(?<Instructions>(?:\r\n|.)*?)(?:\r\n)?===============================================================================

\[2\]: Address Labels
===============================================================================
(?<Labels>(?:\r\n|.)*?)(?:\r\n)?===============================================================================

\[3\]: Resolved Imports
===============================================================================
(?:\r\n|.)*?(?:\r\n)?===============================================================================");

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
            IList<(ulong Address, string Line)> assembledInstructions, IList<(ulong Address, List<string> LabelNames)> addressLabels,
            IList<(string LocalPath, string FullPath)> resolvedImports)
        {
            string fileText = string.Format(DebugInfoFileHeader, DateTime.Now, Environment.CommandLine, totalProgramSize);
            
            fileText += AssembledInstructionsHeader;
            foreach ((ulong address, string instruction) in assembledInstructions)
            {
                fileText += $"\r\n{address:X16} @ {instruction}";
            }

            fileText += $"\r\n{Separator}\r\n{AddressLabelsHeader}";
            foreach ((ulong address, List<string> labels) in addressLabels)
            {
                fileText += $"\r\n{address:X16} @ {string.Join(',', labels)}";
            }

            fileText += $"\r\n{Separator}\r\n{ResolvedImportsHeader}";
            foreach ((string sourceName, string resolvedName) in resolvedImports)
            {
                fileText += $"\r\n\"{sourceName}\" -> \"{resolvedName}\"";
            }

            fileText += "\r\n" + Separator;

            return fileText;
        }

        public readonly record struct DebugInfoFile(
            Dictionary<ulong, string> AssembledInstructions,
            Dictionary<ulong, string[]> AddressLabels);

        public static DebugInfoFile ParseDebugInfoFile(string fileText)
        {
            Match fileMatch = DebugFileRegex.Match(fileText);
            if (!fileMatch.Success)
            {
                throw new FormatException("The provided debug information file was in an invalid format");
            }
            if (fileMatch.Groups["Version"].Value != FormatVersion)
            {
                throw new FormatException("The provided debug information file was created for a different version of AssEmbly");
            }

            List<(ulong Address, string Line)> assembledInstructions = new();
            List<(ulong Address, string[] LabelNames)> addressLabels = new();

            foreach (string line in fileMatch.Groups["Instructions"].Value.Split('\n'))
            {
                string[] split = line.Split(" @ ");
                assembledInstructions.Add((Convert.ToUInt64(split[0], 16), split[1]));
            }

            foreach (string line in fileMatch.Groups["Labels"].Value.Split('\n'))
            {
                string[] split = line.Split(" @ ");
                addressLabels.Add((Convert.ToUInt64(split[0], 16), split[1].Split(',')));
            }

            return new DebugInfoFile(
                assembledInstructions.ToDictionary(x => x.Address, x => x.Line),
                addressLabels.ToDictionary(x => x.Address, x => x.LabelNames));
        }
    }
}
