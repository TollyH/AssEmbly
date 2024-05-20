using System.ComponentModel;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public partial class AssemblerWarnings
    {
        [Localizable(true)]
        public static readonly Dictionary<int, string> NonFatalErrorMessages = new()
        {
            { 0000, Strings_AssemblerWarnings.NonFatal_0000 },
            { 0001, Strings_AssemblerWarnings.NonFatal_0001 },
            { 0002, Strings_AssemblerWarnings.NonFatal_0002 },
            { 0003, Strings_AssemblerWarnings.NonFatal_0003 },
            { 0004, Strings_AssemblerWarnings.NonFatal_0004 },
            { 0005, Strings_AssemblerWarnings.NonFatal_0005 },
        };

        [Localizable(true)]
        public static readonly Dictionary<int, string> WarningMessages = new()
        {
            { 0000, Strings_AssemblerWarnings.Warning_0000 },
            { 0001, Strings_AssemblerWarnings.Warning_0001 },
            { 0002, Strings_AssemblerWarnings.Warning_0002 },
            { 0003, Strings_AssemblerWarnings.Warning_0003 },
            { 0004, Strings_AssemblerWarnings.Warning_0004 },
            { 0005, Strings_AssemblerWarnings.Warning_0005 },
            { 0006, Strings_AssemblerWarnings.Warning_0006 },
            { 0007, Strings_AssemblerWarnings.Warning_0007 },
            { 0008, Strings_AssemblerWarnings.Warning_0008 },
            { 0009, Strings_AssemblerWarnings.Warning_0009 },
            { 0010, Strings_AssemblerWarnings.Warning_0010 },
            { 0011, Strings_AssemblerWarnings.Warning_0011 },
            { 0012, Strings_AssemblerWarnings.Warning_0012 },
            { 0013, Strings_AssemblerWarnings.Warning_0013 },
            { 0014, Strings_AssemblerWarnings.Warning_0014 },
            { 0015, Strings_AssemblerWarnings.Warning_0015 },
            { 0016, Strings_AssemblerWarnings.Warning_0016 },
            { 0017, Strings_AssemblerWarnings.Warning_0017 },
            { 0018, Strings_AssemblerWarnings.Warning_0018 },
            { 0019, Strings_AssemblerWarnings.Warning_0019 },
            { 0020, Strings_AssemblerWarnings.Warning_0020 },
            { 0021, Strings_AssemblerWarnings.Warning_0021 },
            { 0022, Strings_AssemblerWarnings.Warning_0022 },
            { 0023, Strings_AssemblerWarnings.Warning_0023 },
            { 0024, Strings_AssemblerWarnings.Warning_0024 },
            { 0025, Strings_AssemblerWarnings.Warning_0025 },
            { 0026, Strings_AssemblerWarnings.Warning_0026 },
            { 0027, Strings_AssemblerWarnings.Warning_0027 },
            { 0028, Strings_AssemblerWarnings.Warning_0028 },
            { 0029, Strings_AssemblerWarnings.Warning_0029 },
            { 0030, Strings_AssemblerWarnings.Warning_0030 },
            { 0031, Strings_AssemblerWarnings.Warning_0031 },
            { 0032, Strings_AssemblerWarnings.Warning_0032 },
            { 0033, Strings_AssemblerWarnings.Warning_0033 },
            { 0034, Strings_AssemblerWarnings.Warning_0034 },
        };

        [Localizable(true)]
        public static readonly Dictionary<int, string> SuggestionMessages = new()
        {
            { 0000, Strings_AssemblerWarnings.Suggestion_0000 },
            { 0001, Strings_AssemblerWarnings.Suggestion_0001 },
            { 0002, Strings_AssemblerWarnings.Suggestion_0002 },
            { 0003, Strings_AssemblerWarnings.Suggestion_0003 },
            { 0004, Strings_AssemblerWarnings.Suggestion_0004 },
            { 0005, Strings_AssemblerWarnings.Suggestion_0005 },
            { 0006, Strings_AssemblerWarnings.Suggestion_0006 },
            { 0007, Strings_AssemblerWarnings.Suggestion_0007 },
            { 0008, Strings_AssemblerWarnings.Suggestion_0008 },
            { 0009, Strings_AssemblerWarnings.Suggestion_0009 },
            { 0010, Strings_AssemblerWarnings.Suggestion_0010 },
            { 0011, Strings_AssemblerWarnings.Suggestion_0011 },
            { 0012, Strings_AssemblerWarnings.Suggestion_0012 },
            { 0013, Strings_AssemblerWarnings.Suggestion_0013 },
            { 0014, Strings_AssemblerWarnings.Suggestion_0014 },
            { 0015, Strings_AssemblerWarnings.Suggestion_0015 },
            { 0016, Strings_AssemblerWarnings.Suggestion_0016 },
            { 0017, Strings_AssemblerWarnings.Suggestion_0017 },
            { 0018, Strings_AssemblerWarnings.Suggestion_0018 },
            { 0019, Strings_AssemblerWarnings.Suggestion_0019 },
            { 0020, Strings_AssemblerWarnings.Suggestion_0020 },
            { 0021, Strings_AssemblerWarnings.Suggestion_0021 },
            { 0022, Strings_AssemblerWarnings.Suggestion_0022 },
            { 0023, Strings_AssemblerWarnings.Suggestion_0023 },
        };

        public static Dictionary<int, string> GetMessagesForSeverity(WarningSeverity severity)
        {
            return severity switch
            {
                WarningSeverity.NonFatalError => NonFatalErrorMessages,
                WarningSeverity.Warning => WarningMessages,
                WarningSeverity.Suggestion => SuggestionMessages,
                _ => throw new ArgumentException("Given severity is not valid.")
            };
        }
    }
}
