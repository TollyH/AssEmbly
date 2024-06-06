using System.Text;
using System.Text.RegularExpressions;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public static class DebugInfo
    {
        public readonly record struct DebugInfoFile(
            Dictionary<ulong, string> AssembledInstructions,
            Dictionary<ulong, string[]> AddressLabels,
            Dictionary<ulong, string> ImportLocations,
            Dictionary<ulong, FilePosition> FileLineMap);

        public static readonly string FormatVersion = "1.1";

        public static readonly string Separator = "===============================================================================";

        public static readonly string DebugInfoFileHeader = $$"""
            AssEmbly Debug Information File
            Format Version: {{FormatVersion}}
            Date: {0:yyyy-MM-dd HH:mm:ss}
            Command Line: {1}
            Total Program Size: {2} bytes
            {{Separator}}
            
            """.ReplaceLineEndings("\n");

        public static readonly string AssembledInstructionsHeader = $"\n[1]: Assembled Instructions\n{Separator}";

        public static readonly string AddressLabelsHeader = $"\n[2]: Address Labels\n{Separator}";

        public static readonly string ResolvedImportsHeader = $"\n[3]: Resolved Imports\n{Separator}";

        public static readonly string FileLineMapHeader = $"\n[4]: File and Line Mapping\n{Separator}";

        public static readonly Regex DebugFileRegex = new("""
            AssEmbly Debug Information File
            Format Version: (?<Version>.+)
            Date: .*
            Command Line: .*
            Total Program Size: .*
            ===============================================================================
            
            \[1\]: Assembled Instructions
            ===============================================================================
            (?<Instructions>(?:\n|.)*?)(?:\n)?===============================================================================
            
            \[2\]: Address Labels
            ===============================================================================
            (?<Labels>(?:\n|.)*?)(?:\n)?===============================================================================
            
            \[3\]: Resolved Imports
            ===============================================================================
            (?<Imports>(?:\n|.)*?)(?:\n)?===============================================================================
            
            \[4\]: File and Line Mapping
            ===============================================================================
            (?<FileLineMap>(?:\n|.)*?)(?:\n)?===============================================================================
            """.ReplaceLineEndings("\n"));

        /// <summary>
        /// Generates the contents of a debug information file based on the provided parameters.
        /// </summary>
        /// <param name="totalProgramSize">The total size in bytes of the generated file</param>
        /// <param name="assembledInstructions">An array of addresses and the corresponding line of AssEmbly that generated them</param>
        /// <param name="addressLabels">An array of addresses combined with an array of label names pointing to that address</param>
        /// <param name="resolvedImports">
        /// An array of imported file names as seen in AssEmbly code along with the full file path to each one, and the address they were inserted to
        /// </param>
        /// <returns>A completely formatted debug info file string ready to be saved.</returns>
        public static string GenerateDebugInfoFile(ulong totalProgramSize,
            IEnumerable<(ulong Address, string Line)> assembledInstructions, IEnumerable<(ulong Address, List<string> LabelNames)> addressLabels,
            IEnumerable<(string LocalPath, string FullPath, ulong Address)> resolvedImports, IEnumerable<(ulong Address, FilePosition Position)> fileLineMap)
        {
            StringBuilder fileText = new();
            _ = fileText.AppendFormat(DebugInfoFileHeader, DateTime.Now, Environment.CommandLine, totalProgramSize);

            _ = fileText.Append(AssembledInstructionsHeader);
            foreach ((ulong address, string instruction) in assembledInstructions)
            {
                _ = fileText.Append($"\n{address:X16} @ {instruction}");
            }

            _ = fileText.Append($"\n{Separator}\n{AddressLabelsHeader}");
            foreach ((ulong address, List<string> labels) in addressLabels)
            {
                _ = fileText.Append($"\n{address:X16} @ {string.Join(',', labels)}");
            }

            _ = fileText.Append($"\n{Separator}\n{ResolvedImportsHeader}");
            foreach ((string sourceName, string resolvedName, ulong address) in resolvedImports.DistinctBy(x => x.Address))
            {
                _ = fileText.Append($"\n{address:X16} @ \"{sourceName}\" -> \"{resolvedName}\"");
            }

            _ = fileText.Append($"\n{Separator}\n{FileLineMapHeader}");
            foreach ((ulong address, FilePosition position) in fileLineMap)
            {
                _ = fileText.Append($"\n{address:X16} @ {position.Line}:{position.File}");
            }

            _ = fileText.Append('\n').Append(Separator);

            return fileText.ToString();
        }

        public static DebugInfoFile ParseDebugInfoFile(string fileText)
        {
            Match fileMatch = DebugFileRegex.Match(fileText);
            if (!fileMatch.Success)
            {
                throw new DebugFileException(Strings.DebugInfo_Error_Invalid_Format);
            }
            if (fileMatch.Groups["Version"].Value != FormatVersion)
            {
                throw new DebugFileException(Strings.DebugInfo_Error_Wrong_Version);
            }

            List<(ulong Address, string Line)> assembledInstructions = new();
            List<(ulong Address, string[] LabelNames)> addressLabels = new();
            List<(ulong Address, string ImportName)> importLocations = new();
            List<(ulong Address, FilePosition Position)> fileLineMap = new();

            foreach (string line in fileMatch.Groups["Instructions"].Value.Split('\n'))
            {
                if (line.Trim() == string.Empty)
                {
                    continue;
                }
                string[] split = line.Split(" @ ");
                assembledInstructions.Add((Convert.ToUInt64(split[0], 16), split[1]));
            }

            foreach (string line in fileMatch.Groups["Labels"].Value.Split('\n'))
            {
                if (line.Trim() == string.Empty)
                {
                    continue;
                }
                string[] split = line.Split(" @ ");
                addressLabels.Add((Convert.ToUInt64(split[0], 16), split[1].Split(',')));
            }

            foreach (string line in fileMatch.Groups["Imports"].Value.Split('\n'))
            {
                if (line.Trim() == string.Empty)
                {
                    continue;
                }
                string[] split = line.Split(" @ ");
                importLocations.Add((Convert.ToUInt64(split[0], 16), split[1].Split(" -> ")[0].Trim('"')));
            }

            foreach (string line in fileMatch.Groups["FileLineMap"].Value.Split('\n'))
            {
                if (line.Trim() == string.Empty)
                {
                    continue;
                }
                string[] split = line.Split(" @ ");
                string[] positionSplit = split[1].Split(':', 2);
                fileLineMap.Add((Convert.ToUInt64(split[0], 16), new FilePosition(int.Parse(positionSplit[0]), positionSplit[1])));
            }

            return new DebugInfoFile(
                assembledInstructions.ToDictionary(x => x.Address, x => x.Line),
                addressLabels.ToDictionary(x => x.Address, x => x.LabelNames),
                importLocations.ToDictionary(x => x.Address, x => x.ImportName),
                fileLineMap.ToDictionary(x => x.Address, x => x.Position));
        }
    }
}
