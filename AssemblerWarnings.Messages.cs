namespace AssEmbly
{
    public partial class AssemblerWarnings
    {
        public static readonly Dictionary<int, string> NonFatalErrorMessages = new()
        {
            { 0001, "Instruction writes to the rpo register." },
            { 0002, "Division by constant 0." },
        };

        public static readonly Dictionary<int, string> WarningMessages = new()
        {
            { 0001, "Data insertion is not directly preceded by an unconditional jump, return, or halt instruction." },
            { 0002, "Jump/Call target label points to data, not executable code." },
            { 0003, "Jump/Call target label points to end of file, not executable code." },
            { 0004, "Instruction writes to a label pointing to executable code." },
            { 0005, "Instruction reads from a label pointing to executable code in a context that likely expects data." },
            { 0006, "String insertion is not immediately followed by a 0 (null) byte." },
            { 0007, "Numeric literal is too large for the given move instruction. Upper bits will be truncated at runtime." },
            { 0008, "Unreachable code detected." },
            { 0009, "Program runs to end of file without being terminated by an unconditional jump, return, or halt instruction." },
            { 0010, "File import is not directly preceded by an unconditional jump, return, or halt instruction." },
            { 0011, "Instruction writes to the rsf register." },
            { 0012, "Instruction writes to the rsb register." },
            { 0013, "Jump/Call target label points to itself, resulting in an unbreakable infinite loop." },
            { 0014, "Unlabelled executable code found after data insertion." },
            { 0015, "Code follows an imported file that is not terminated by unconditional jump, return, or halt instruction." },
            { 0016, "Addresses are 64-bit values, however this move instruction moves less than 64 bits." },
        };

        public static readonly Dictionary<int, string> SuggestionMessages = new()
        {
            { 0001, "Avoid use of NOP instruction." },
            { 0002, "Use the `PAD` directive instead of chaining `DAT 0` directives." },
            { 0003, "Put IMP directives at the end of the file, unless the position of the directive is important given the file's contents." },
            { 0004, "Put data at the end of the file, unless the position of the data is important." },
            { 0005, "Use `TST {1}, {1}` instead of `CMP {1}, 0`, as it results in less bytes." },
            { 0006, "Use `XOR {1}, {1}` instead of `{0} {1}, 0`, as it results in less bytes." },
            { 0007, "Use `INC {1}` instead of `ADD {1}, 1`, as it results in less bytes." },
            { 0008, "Use `DEC {1}` instead of `SUB {1}, 1`, as it results in less bytes." },
            { 0009, "Operation has no effect." },
            { 0010, "Shift operation shifts by 64 bits or more, which will always result in 0. Use `XOR {1}, {1}` instead." },
            { 0011, "Remove leading 0 digits from denary number." },
            { 0012, "Remove useless `PAD 0` directive." },
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
