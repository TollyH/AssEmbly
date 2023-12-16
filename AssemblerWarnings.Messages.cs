using System.ComponentModel;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public partial class AssemblerWarnings
    {
        [Localizable(true)]
        public static readonly Dictionary<int, string> NonFatalErrorMessages = new()
        {
            { 0001, Strings.AssemblerWarnings_NonFatal_0001 },
            { 0002, Strings.AssemblerWarnings_NonFatal_0002 },
            { 0003, Strings.AssemblerWarnings_NonFatal_0003 },
            { 0004, Strings.AssemblerWarnings_NonFatal_0004 },
        };

        [Localizable(true)]
        public static readonly Dictionary<int, string> WarningMessages = new()
        {
            { 0001, Strings.AssemblerWarnings_Warning_0001 },
            { 0002, Strings.AssemblerWarnings_Warning_0002 },
            { 0003, Strings.AssemblerWarnings_Warning_0003 },
            { 0004, Strings.AssemblerWarnings_Warning_0004 },
            { 0005, Strings.AssemblerWarnings_Warning_0005 },
            { 0006, Strings.AssemblerWarnings_Warning_0006 },
            { 0007, Strings.AssemblerWarnings_Warning_0007 },
            { 0008, Strings.AssemblerWarnings_Warning_0008 },
            { 0009, Strings.AssemblerWarnings_Warning_0009 },
            { 0010, Strings.AssemblerWarnings_Warning_0010 },
            { 0011, Strings.AssemblerWarnings_Warning_0011 },
            { 0012, Strings.AssemblerWarnings_Warning_0012 },
            { 0013, Strings.AssemblerWarnings_Warning_0013 },
            { 0014, Strings.AssemblerWarnings_Warning_0014 },
            { 0015, Strings.AssemblerWarnings_Warning_0015 },
            { 0016, Strings.AssemblerWarnings_Warning_0016 },
            { 0017, Strings.AssemblerWarnings_Warning_0017 },
            { 0018, Strings.AssemblerWarnings_Warning_0018 },
            { 0019, Strings.AssemblerWarnings_Warning_0019 },
            { 0020, Strings.AssemblerWarnings_Warning_0020 },
            { 0021, Strings.AssemblerWarnings_Warning_0021 },
            { 0022, Strings.AssemblerWarnings_Warning_0022 },
            { 0023, Strings.AssemblerWarnings_Warning_0023 },
            { 0024, Strings.AssemblerWarnings_Warning_0024 },
            { 0025, Strings.AssemblerWarnings_Warning_0025 },
        };

        [Localizable(true)]
        public static readonly Dictionary<int, string> SuggestionMessages = new()
        {
            { 0001, Strings.AssemblerWarnings_Suggestion_0001 },
            { 0002, Strings.AssemblerWarnings_Suggestion_0002 },
            { 0003, Strings.AssemblerWarnings_Suggestion_0003 },
            { 0004, Strings.AssemblerWarnings_Suggestion_0004 },
            { 0005, Strings.AssemblerWarnings_Suggestion_0005 },
            { 0006, Strings.AssemblerWarnings_Suggestion_0006 },
            { 0007, Strings.AssemblerWarnings_Suggestion_0007 },
            { 0008, Strings.AssemblerWarnings_Suggestion_0008 },
            { 0009, Strings.AssemblerWarnings_Suggestion_0009 },
            { 0010, Strings.AssemblerWarnings_Suggestion_0010 },
            { 0011, Strings.AssemblerWarnings_Suggestion_0011 },
            { 0012, Strings.AssemblerWarnings_Suggestion_0012 },
            { 0013, Strings.AssemblerWarnings_Suggestion_0013 },
            { 0014, Strings.AssemblerWarnings_Suggestion_0014 },
            { 0015, Strings.AssemblerWarnings_Suggestion_0015 },
            { 0016, Strings.AssemblerWarnings_Suggestion_0016 },
            { 0017, Strings.AssemblerWarnings_Suggestion_0017 },
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
