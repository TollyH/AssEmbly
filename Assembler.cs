using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public readonly record struct AssemblyResult
    (
        byte[] Program,
        string DebugInfo,
        List<Warning> Warnings,
        ulong EntryPoint,
        AAPFeatures UsedExtensions,
        int AssembledLines,
        int AssembledFiles
    );

    /// <summary>
    /// Assembles text based AssEmbly programs to compiled AssEmbly bytes.
    /// </summary>
    public class Assembler
    {
        public class ImportStackFrame(string importPath, int currentLine, int totalLines)
        {
            public string ImportPath { get; } = importPath;
            public int CurrentLine { get; set; } = currentLine;
            public int TotalLines { get; } = totalLines;
        }

        public class MacroStackFrame(string macroName, int remainingLines)
        {
            public string MacroName { get; } = macroName;
            public int RemainingLines { get; set; } = remainingLines;
        }

        public bool Finalized { get; private set; }

        // Final compiled byte list
        private readonly List<byte> program = new();
        // Map of label names to final memory addresses
        private readonly Dictionary<string, ulong> labels = new();
        // Map of label names that link to another label name, along with the file and line the link was made on
        private readonly Dictionary<string, (string Target, string? FilePath, int Line)> labelLinks = new();
        // List of references to labels by name along with the address to insert the relevant address in to.
        // Also has the line and path to the file (if imported) that the reference was assembled from for use in error messages.
        private readonly List<(string LabelName, ulong Address, string? FilePath, int Line)> labelReferences = new();
        private readonly HashSet<string> overriddenLabels = new();
        // string -> replacement. All single-line macros are expanded before multi-line macros.
        private readonly Dictionary<string, string> singleLineMacros = new();
        private readonly Dictionary<string, string[]> multiLineMacros = new();
        // Sorted from the longest name to the shortest name - should always match the keys of the above dictionaries
        private List<string> singleLineMacroNames = new();
        private List<string> multiLineMacroNames = new();
        private AAPFeatures usedExtensions = AAPFeatures.None;

        // For detecting circular imports and tracking imported line numbers
        private readonly Stack<ImportStackFrame> importStack = new();
        private ImportStackFrame? currentImport = null;
        private int baseFileLine = 0;

        // Used to keep track of multi-line macros
        private readonly Stack<MacroStackFrame> macroStack = new();

        // Used for debug files
        private readonly List<(ulong Address, string Line)> assembledLines = new();
        private readonly Dictionary<ulong, List<string>> addressLabelNames = new();
        private readonly List<(string LocalPath, string FullPath, ulong Address)> resolvedImports = new();

        private readonly AssemblerWarnings warningGenerator;
        private readonly List<Warning> warnings = new();

        private readonly HashSet<int> initialDisabledNonFatalErrors;
        private readonly HashSet<int> initialDisabledWarnings;
        private readonly HashSet<int> initialDisabledSuggestions;

        private bool lineIsLabelled = false;
        private bool lineIsEntry = false;

        private ulong entryPoint = 0;

        private int processedLines = 0;
        private int visitedFiles = 1;

        /// <param name="usingV1Format">
        /// Whether or not a v1 executable will be generated from this assembly.
        /// Used only for warning generation. Does not affect the resulting bytes.
        /// </param>
        /// <param name="disabledNonFatalErrors">A set of non-fatal error codes to disable.</param>
        /// <param name="disabledWarnings">A set of warning codes to disable.</param>
        /// <param name="disabledSuggestions">A set of suggestion codes to disable.</param>
        public Assembler(bool usingV1Format, IEnumerable<int> disabledNonFatalErrors, IEnumerable<int> disabledWarnings, IEnumerable<int> disabledSuggestions)
        {
            warningGenerator = new AssemblerWarnings(usingV1Format);
            warningGenerator.DisabledNonFatalErrors.UnionWith(disabledNonFatalErrors);
            warningGenerator.DisabledWarnings.UnionWith(disabledWarnings);
            warningGenerator.DisabledSuggestions.UnionWith(disabledSuggestions);

            initialDisabledNonFatalErrors = warningGenerator.DisabledNonFatalErrors.ToHashSet();
            initialDisabledWarnings = warningGenerator.DisabledWarnings.ToHashSet();
            initialDisabledSuggestions = warningGenerator.DisabledSuggestions.ToHashSet();
        }

        public Assembler()
        {
            warningGenerator = new AssemblerWarnings(false);

            initialDisabledNonFatalErrors = new HashSet<int>();
            initialDisabledWarnings = new HashSet<int>();
            initialDisabledSuggestions = new HashSet<int>();
        }

        /// <returns>
        /// <see langword="false"/> if the warning with the given severity and code was already disabled, else <see langword="true"/>
        /// </returns>
        public bool DisableAssemblerWarning(WarningSeverity severity, int code)
        {
            return severity switch
            {
                WarningSeverity.NonFatalError => warningGenerator.DisabledNonFatalErrors.Add(code),
                WarningSeverity.Warning => warningGenerator.DisabledWarnings.Add(code),
                WarningSeverity.Suggestion => warningGenerator.DisabledSuggestions.Add(code),
                _ => throw new ArgumentException(Strings.Assembler_Error_Invalid_Severity),
            };
        }

        /// <returns>
        /// <see langword="false"/> if the warning with the given severity and code was already enabled, else <see langword="true"/>
        /// </returns>
        public bool EnableAssemblerWarning(WarningSeverity severity, int code)
        {
            return severity switch
            {
                WarningSeverity.NonFatalError => warningGenerator.DisabledNonFatalErrors.Remove(code),
                WarningSeverity.Warning => warningGenerator.DisabledWarnings.Remove(code),
                WarningSeverity.Suggestion => warningGenerator.DisabledSuggestions.Remove(code),
                _ => throw new ArgumentException(Strings.Assembler_Error_Invalid_Severity),
            };
        }

        /// <summary>
        /// Reset the enable/disable state of the given warning to its state when the Assembler was initialized.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the warning with the given severity and code was disabled, else <see langword="false"/>
        /// </returns>
        public bool ResetAssemblerWarning(WarningSeverity severity, int code)
        {
            switch (severity)
            {
                case WarningSeverity.NonFatalError:
                    _ = initialDisabledNonFatalErrors.Contains(code) ? warningGenerator.DisabledNonFatalErrors.Add(code) : warningGenerator.DisabledNonFatalErrors.Remove(code);
                    return warningGenerator.DisabledNonFatalErrors.Contains(code);
                case WarningSeverity.Warning:
                    _ = initialDisabledWarnings.Contains(code) ? warningGenerator.DisabledWarnings.Add(code) : warningGenerator.DisabledWarnings.Remove(code);
                    return warningGenerator.DisabledWarnings.Contains(code);
                case WarningSeverity.Suggestion:
                    _ = initialDisabledSuggestions.Contains(code) ? warningGenerator.DisabledSuggestions.Add(code) : warningGenerator.DisabledSuggestions.Remove(code);
                    return warningGenerator.DisabledSuggestions.Contains(code);
                default:
                    throw new ArgumentException(Strings.Assembler_Error_Invalid_Severity);
            }
        }

        /// <summary>
        /// Get the result of the assembly, including the assembled program bytes.
        /// </summary>
        /// <remarks>
        /// If <paramref name="finalize"/> is <see langword="true"/>, label references will be resolved and final warning analyzers will be run.
        /// The assembler will no longer be able to assemble additional lines.
        /// If <see langword="false"/>, uses of labels will be filled with 00 bytes, and warnings that require final warning analyzers will be missing.
        /// </remarks>
        public AssemblyResult GetAssemblyResult(bool finalize)
        {
            byte[] programBytes;
            if (finalize)
            {
                ResolveLabelReferences();
                programBytes = program.ToArray();
                warnings.AddRange(warningGenerator.Finalize(programBytes, entryPoint));
                Finalized = true;
            }
            else
            {
                programBytes = program.ToArray();
            }
            string debugInfo = DebugInfo.GenerateDebugInfoFile((uint)program.Count, assembledLines,
                // Convert dictionary to sorted list
                addressLabelNames.Select(x => (x.Key, x.Value)).OrderBy(x => x.Key).ToList(),
                resolvedImports);
            return new AssemblyResult(programBytes, debugInfo, warnings, entryPoint, usedExtensions, processedLines, visitedFiles);
        }

        /// <summary>
        /// Assemble multiple AssEmbly lines at once into executable bytecode.
        /// </summary>
        /// <param name="lines">Each line of the program to assemble. Newline characters should not be included.</param>
        /// <exception cref="SyntaxError">Thrown when there is an error with how a line of AssEmbly has been written.</exception>
        /// <exception cref="LabelNameException">
        /// Thrown when the same label name is defined multiple times, or when a reference is made to a non-existent label.
        /// </exception>
        /// <exception cref="OperandException">Thrown when a mnemonic is given an invalid number or type of operands.</exception>
        /// <exception cref="ImportException">
        /// Thrown when an error occurs whilst attempting to import the contents of another AssEmbly file.
        /// </exception>
        /// <exception cref="OpcodeException">Thrown when a particular combination of mnemonic and operand types is not recognised.</exception>
        public void AssembleLines(IEnumerable<string> lines)
        {
            if (Finalized)
            {
                throw new InvalidOperationException(Strings.Assembler_Error_Finalized);
            }

            // The lines to assemble may change during assembly, for example importing a file
            // will extend the list of lines to assemble as and when the import is reached.
            List<string> dynamicLines = lines.ToList();

            importStack.Clear();
            baseFileLine = 0;

            for (int lineIndex = 0; lineIndex < dynamicLines.Count; lineIndex++)
            {
                IncrementCurrentLine();
                string rawLine = CleanLine(dynamicLines[lineIndex]);
                foreach (string macro in singleLineMacroNames)
                {
                    rawLine = CleanLine(rawLine.Replace(macro, singleLineMacros[macro]));
                }
                try
                {
                    bool multiLineMacroMatched = false;
                    foreach (string macro in multiLineMacroNames)
                    {
                        if (rawLine == macro)
                        {
                            if (macroStack.Any(m => m.MacroName == macro))
                            {
                                throw new MacroExpansionException(string.Format(Strings.Assembler_Error_Circular_Macro, macro));
                            }
                            string[] replacement = multiLineMacros[macro];
                            dynamicLines.InsertRange(lineIndex + 1, replacement);
                            macroStack.Push(new MacroStackFrame(macro, replacement.Length));
                            multiLineMacroMatched = true;
                            break;
                        }
                    }
                    if (multiLineMacroMatched)
                    {
                        continue;
                    }

                    string[] line = ParseLine(rawLine);
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    processedLines++;
                    string mnemonic = line[0];
                    // Lines starting with ':' are label definitions
                    if (mnemonic.StartsWith(':'))
                    {
                        if (line.Length > 1)
                        {
                            throw new SyntaxError(string.Format(
                                Strings.Assembler_Error_Label_Spaces_Contained, mnemonic, line[1], new string(' ', mnemonic.Length)));
                        }
                        // Will throw an error if label is not valid
                        _ = DetermineOperandType(mnemonic, false);
                        if (mnemonic[1] == '&')
                        {
                            throw new SyntaxError(string.Format(Strings.Assembler_Error_Invalid_Literal_Label, rawLine));
                        }
                        string labelName = mnemonic[1..];
                        if (labels.ContainsKey(labelName))
                        {
                            throw new LabelNameException(string.Format(Strings.Assembler_Error_Label_Already_Defined, labelName));
                        }
                        if (labelName.Equals("ENTRY", StringComparison.OrdinalIgnoreCase))
                        {
                            lineIsEntry = true;
                        }
                        labels[labelName] = (uint)program.Count;

                        lineIsLabelled = true;
                        continue;
                    }
                    string[] operands = line[1..];

                    if (mnemonic[0] == '%' && ProcessStateDirective(mnemonic, operands, rawLine, dynamicLines, ref lineIndex))
                    {
                        // Directive found and processed, move onto next statement
                        continue;
                    }

                    (byte[] newBytes, List<(string LabelName, ulong AddressOffset)> newLabels) =
                        AssembleStatement(mnemonic, operands, out AAPFeatures newFeatures);

                    usedExtensions |= newFeatures;

                    foreach ((string label, ulong relativeOffset) in newLabels)
                    {
                        labelReferences.Add((label, relativeOffset + (uint)program.Count, currentImport?.ImportPath,
                            currentImport?.CurrentLine ?? baseFileLine));
                    }

                    assembledLines.Add(((uint)program.Count, rawLine));
                    program.AddRange(newBytes);

                    warnings.AddRange(warningGenerator.NextInstruction(
                        newBytes, mnemonic, operands,
                        currentImport?.CurrentLine ?? baseFileLine,
                        currentImport?.ImportPath ?? string.Empty, lineIsLabelled, lineIsEntry, rawLine, importStack));

                    lineIsLabelled = false;
                    lineIsEntry = false;
                }
                catch (AssemblerException e)
                {
                    // Some directives change the current line index, so get the raw line contents again.
                    // Also includes comments on original line that were removed by CleanLine.
                    rawLine = dynamicLines[lineIndex];
                    if (currentImport is null)
                    {
                        e.ConsoleMessage = string.Format(Strings.Assembler_Error_Message_Base_File, baseFileLine, rawLine, e.Message);
                        e.WarningObject = new Warning(WarningSeverity.FatalError, 0000, "", baseFileLine, "", Array.Empty<string>(), rawLine, e.Message);
                    }
                    else
                    {
                        e.WarningObject = new Warning(WarningSeverity.FatalError, 0000,
                            currentImport.ImportPath, currentImport.CurrentLine, "", Array.Empty<string>(), rawLine, e.Message);
                        string newMessage = string.Format(Strings.Assembler_Error_Message_Imported, currentImport.CurrentLine, currentImport.ImportPath, rawLine);
                        _ = importStack.Pop();  // Remove already printed frame from stack
                        while (importStack.TryPop(out ImportStackFrame? nestedImport))
                        {
                            newMessage += string.Format(Strings.Assembler_Error_Message_Imported_Import, nestedImport.CurrentLine, nestedImport.ImportPath);
                        }
                        newMessage += string.Format(Strings.Assembler_Error_Message_Imported_Base, baseFileLine, e.Message);
                        e.ConsoleMessage = newMessage;
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Assemble a single line of AssEmbly to bytecode.
        /// </summary>
        /// <param name="usedExtensions">The feature flags for any extension sets used in this instruction.</param>
        /// <returns>The assembled bytes, along with a list of label names and the offset the addresses of the labels need to be inserted into.</returns>
        /// <exception cref="OperandException">Thrown when a mnemonic is given an invalid number or type of operands.</exception>
        /// <exception cref="OpcodeException">Thrown when a particular combination of mnemonic and operand types is not recognised.</exception>
        public static (byte[], List<(string LabelName, ulong AddressOffset)>) AssembleStatement(string mnemonic, string[] operands, out AAPFeatures usedExtensions)
        {
            OperandType[] operandTypes = new OperandType[operands.Length];
            List<byte> operandBytes = new();
            List<(string LabelName, ulong AddressOffset)> referencedLabels = new();
            usedExtensions = AAPFeatures.None;

            if (mnemonic[0] == '%' && ProcessDataDirective(mnemonic, operands, referencedLabels, out byte[]? newBytes))
            {
                // Directive found and processed, move onto next statement
                return (newBytes, referencedLabels);
            }

            for (int i = 0; i < operands.Length; i++)
            {
                operandTypes[i] = DetermineOperandType(operands[i]);
                switch (operandTypes[i])
                {
                    case OperandType.Register:
                        operandBytes.Add((byte)Enum.Parse<Register>(operands[i].ToLowerInvariant()));
                        break;
                    case OperandType.Literal:
                        if (operands[i].StartsWith(":&"))
                        {
                            referencedLabels.Add((operands[i][2..], (uint)operandBytes.Count));
                            for (int j = 0; j < 8; j++)
                            {
                                // Label location will be resolved later, pad with 0s for now
                                operandBytes.Add(0);
                            }
                        }
                        else
                        {
                            operandBytes.AddRange(ParseLiteral(operands[i], false));
                        }
                        break;
                    case OperandType.Address:
                        if (operands[i][1] is >= '0' and <= '9')
                        {
                            // Literal address
                            operandBytes.AddRange(ParseLiteral(operands[i][1..], false));
                        }
                        else
                        {
                            // Label address
                            referencedLabels.Add((operands[i][1..], (uint)operandBytes.Count));
                            for (int j = 0; j < 8; j++)
                            {
                                // Label location will be resolved later, pad with 0s for now
                                operandBytes.Add(0);
                            }
                        }
                        break;
                    case OperandType.Pointer:
                        // Convert register name to associated byte value
                        operandBytes.Add((byte)Enum.Parse<Register>(operands[i][1..].ToLowerInvariant()));
                        break;
                    default: break;
                }
            }
            if (!Data.Mnemonics.TryGetValue((mnemonic.ToUpperInvariant(), operandTypes), out Opcode opcode))
            {
                throw new OpcodeException(string.Format(Strings.Assembler_Error_Invalid_Mnemonic_Combo, mnemonic, string.Join(Strings.Generic_CommaSeparate, operandTypes)));
            }

            uint opcodeSize = 1;
            operandBytes.Insert(0, opcode.InstructionCode);
            // Base instruction set only needs to be referenced by instruction code,
            // all others need to be in full form (0xFF, {ExtensionSet}, {InstructionCode})
            if (opcode.ExtensionSet != 0x00)
            {
                opcodeSize = 3;
                operandBytes.Insert(0, opcode.ExtensionSet);
                operandBytes.Insert(0, Opcode.FullyQualifiedMarker);
            }
            if (Data.ExtensionSetFeatureFlags.TryGetValue(opcode.ExtensionSet, out AAPFeatures newFlag))
            {
                usedExtensions |= newFlag;
            }

            // Add length of opcode to all label address offsets
            for (int i = 0; i < referencedLabels.Count; i++)
            {
                referencedLabels[i] = (referencedLabels[i].LabelName, referencedLabels[i].AddressOffset + opcodeSize);
            }

            return (operandBytes.ToArray(), referencedLabels);
        }

        /// <summary>
        /// Assemble a single line of AssEmbly to bytecode.
        /// </summary>
        /// <returns>The assembled bytes, along with a list of label names and the offset the addresses of the labels need to be inserted into.</returns>
        /// <exception cref="OperandException">Thrown when a mnemonic is given an invalid number or type of operands.</exception>
        /// <exception cref="OpcodeException">Thrown when a particular combination of mnemonic and operand types is not recognised.</exception>
        public static (byte[], List<(string LabelName, ulong AddressOffset)>) AssembleStatement(string mnemonic, string[] operands)
        {
            return AssembleStatement(mnemonic, operands, out _);
        }

        /// <summary>
        /// Remove surrounding whitespace and comments from a string.
        /// </summary>
        /// <param name="line">The raw AssEmbly source line.</param>
        public static string CleanLine(string line)
        {
            int indexOfSemicolon = line.IndexOf(';');
            if (indexOfSemicolon != -1)
            {
                line = line[..indexOfSemicolon];
            }
            return line.Trim();
        }

        /// <summary>
        /// Split a line of AssEmbly into its individual components.
        /// </summary>
        /// <param name="line">The raw AssEmbly source line.</param>
        /// <returns>
        /// An array of separated line components. May be empty if there is nothing on the line to assemble.
        /// If not empty, the first item will be the mnemonic.
        /// </returns>
        /// <exception cref="SyntaxError">The given line contains invalid formatting.</exception>
        public static string[] ParseLine(string line)
        {
            List<string> elements = new();

            StringBuilder sb = new();
            int trailingWhitespace = -1;
            int stringEnd = -1;
            // Macro definitions have different syntax rules
            // - quotes don't create strings and operands can contain whitespace
            bool isMacro = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == ';')
                {
                    // Comment found, ignore rest of line
                    break;
                }
                if (char.IsWhiteSpace(c) && !isMacro)
                {
                    if (sb.Length != 0 && elements.Count == 0)
                    {
                        // End of mnemonic found, add line so far as first item
                        string mnemonic = sb.ToString();
                        elements.Add(mnemonic);
                        sb = new StringBuilder();
                        isMacro = mnemonic.Equals("%MACRO", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (sb.Length != 0 && elements.Count > 0)
                    {
                        // We've encountered whitespace whilst inside an operand,
                        // no more non-whitespace characters may now follow until the next comma
                        trailingWhitespace = i;
                    }
                    continue;
                }
                if (c == ',')
                {
                    if (elements.Count == 0)
                    {
                        throw new SyntaxError(
                            string.Format(Strings.Assembler_Error_Mnemonic_Operand_Space, line, new string(' ', i)));
                    }
                    if (sb.Length == 0)
                    {
                        throw new SyntaxError(string.Format(Strings.Assembler_Error_Empty_Operand, line, new string(' ', i)));
                    }
                    // The replacement portion of macros can contain commas without it being considered a different operand
                    if (!isMacro || elements.Count < 2)
                    {
                        // End of operand found, add stored characters as an element and move on
                        elements.Add(sb.ToString());
                        sb = new StringBuilder();
                        trailingWhitespace = -1;
                        stringEnd = -1;
                        continue;
                    }
                }
                if (trailingWhitespace != -1 && !isMacro)
                {
                    throw new SyntaxError(
                        string.Format(Strings.Assembler_Error_Operand_Whitespace, line, new string(' ', trailingWhitespace)));
                }
                if (stringEnd != -1 && !isMacro)
                {
                    throw new SyntaxError(
                        string.Format(Strings.Assembler_Error_Quoted_Literal_Followed, line, new string(' ', i)));
                }
                if (c is '"' or '\'' && !isMacro)
                {
                    if (sb.Length != 0)
                    {
                        throw new SyntaxError(
                            string.Format(Strings.Assembler_Error_Quoted_Literal_Following, line, new string(' ', i)));
                    }
                    if (line.Length < 2)
                    {
                        throw new SyntaxError(Strings.Assembler_Error_Quoted_Literal_Line_Length_One);
                    }
                    _ = sb.Append(PreParseStringLiteral(line, ref i));
                    stringEnd = i;
                    continue;
                }
                _ = sb.Append(c);
            }
            // Add any remaining characters
            if (sb.Length > 0)
            {
                elements.Add(sb.ToString());
            }
            return elements.ToArray();
        }

        /// <summary>
        /// Process a string or character literal as a part of an entire line of AssEmbly source.
        /// Resolves escape sequences and ensures that the string ends with an unescaped quote mark.
        /// </summary>
        /// <param name="line">The entire source line with the string contained.</param>
        /// <param name="startIndex">
        /// The index in the line to the opening quote.
        /// It will be incremented automatically by this method to the index of the closing quote.
        /// </param>
        /// <returns>
        /// For double-quoted (") string literals, the processed string literal, including opening and closing quotes.
        /// For single-quoted (') character literals, the 32-bit numeric value corresponding to the
        /// UTF-8 representation as a base-10 string of the single contained character.
        /// </returns>
        /// <remarks>
        /// The string returned by this method is not fully ready to be inserted into a binary stream.
        /// Use <see cref="ParseLiteral"/> with the returned string to get an array of bytes for the final executable.
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">The given start index is outside the range of the given line.</exception>
        /// <exception cref="ArgumentException">The given line is invalid.</exception>
        /// <exception cref="SyntaxError">The string in the line is invalid.</exception>
        public static string PreParseStringLiteral(string line, ref int startIndex)
        {
            if (startIndex < 0 || startIndex >= line.Length)
            {
                throw new IndexOutOfRangeException(Strings.Assembler_Error_String_Bad_StartIndex);
            }
            if (line.Length < 2)
            {
                throw new ArgumentException(Strings.Assembler_Error_String_Too_Short);
            }
            bool singleCharacterLiteral = false;
            bool containsHighSurrogate = false;  // Only used for character literals
            if (line[startIndex] != '"')
            {
                if (line[startIndex] != '\'')
                {
                    throw new ArgumentException(Strings.Assembler_Error_String_Bad_First_Char);
                }
                singleCharacterLiteral = true;
            }
            StringBuilder sb = singleCharacterLiteral ? new() : new("\"");
            int i = startIndex;
            while (true)
            {
                if (++i >= line.Length)
                {
                    throw new SyntaxError(
                        string.Format(Strings.Assembler_Error_Quoted_Literal_EndOfLine, line, new string(' ', i - 1)));
                }
                // If the character literal contains a high surrogate, we need to allow 2 UTF-16 chars to get the full pair.
                // This will result in a single final represented character to convert to UTF-8.
                if (singleCharacterLiteral && sb.Length > (containsHighSurrogate ? 2 : 1))
                {
                    throw new SyntaxError(
                        string.Format(Strings.Assembler_Error_Character_Literal_Too_Long, line, new string(' ', i - 1)));
                }
                char c = line[i];
                if (char.IsHighSurrogate(c))
                {
                    containsHighSurrogate = true;
                }
                if (c == '\\')
                {
                    if (++i >= line.Length)
                    {
                        throw new SyntaxError(
                            string.Format(Strings.Assembler_Error_Quoted_Literal_EndOfLine, line, new string(' ', i - 1)));
                    }
                    char escape = line[i];
                    switch (escape)
                    {
                        // Escapes that keep the same character
                        case '\'':
                        case '"':
                        case '\\':
                            break;
                        // Escapes that map to another character
                        case '0':
                            escape = '\0';
                            break;
                        case 'a':
                            escape = '\a';
                            break;
                        case 'b':
                            escape = '\b';
                            break;
                        case 'f':
                            escape = '\f';
                            break;
                        case 'n':
                            escape = '\n';
                            break;
                        case 'r':
                            escape = '\r';
                            break;
                        case 't':
                            escape = '\t';
                            break;
                        case 'v':
                            escape = '\v';
                            break;
                        case 'u':
                            if (i + 4 >= line.Length)
                            {
                                throw new SyntaxError(string.Format(Strings.Assembler_Error_Unicode_Escape_EndOfLine, line, new string(' ', i)));
                            }
                            string rawCodePoint = line[(i + 1)..(i + 5)];
                            try
                            {
                                escape = (char)Convert.ToUInt16(rawCodePoint, 16);
                            }
                            catch (FormatException)
                            {
                                throw new SyntaxError(
                                    string.Format(Strings.Assembler_Error_Unicode_Escape_4_Digits, line, new string(' ', i)));
                            }
                            i += 4;
                            break;
                        case 'U':
                            if (i + 8 >= line.Length)
                            {
                                throw new SyntaxError(string.Format(Strings.Assembler_Error_Unicode_Escape_EndOfLine, line, new string(' ', i)));
                            }
                            rawCodePoint = line[(i + 1)..(i + 9)];
                            try
                            {
                                string encodedChar = char.ConvertFromUtf32(Convert.ToInt32(rawCodePoint, 16));
                                if (char.IsHighSurrogate(encodedChar[0]))
                                {
                                    containsHighSurrogate = true;
                                }
                                _ = sb.Append(encodedChar);
                            }
                            catch
                            {
                                throw new SyntaxError(
                                    string.Format(Strings.Assembler_Error_Unicode_Escape_8_Digits, line, new string(' ', i)));
                            }
                            i += 8;
                            continue;
                        default:
                            throw new SyntaxError(
                                string.Format(Strings.Assembler_Error_Invalid_Escape_Sequence, escape, line, new string(' ', i)));
                    }
                    _ = sb.Append(escape);
                    continue;
                }
                if ((singleCharacterLiteral && c == '\'') || (!singleCharacterLiteral && c == '"'))
                {
                    break;
                }
                _ = sb.Append(c);
            }
            startIndex = i;
            if (singleCharacterLiteral)
            {
                if (sb.Length == 0)
                {
                    throw new SyntaxError(string.Format(Strings.Assembler_Error_Character_Literal_Empty, line, new string(' ', i)));
                }
                byte[] characterBytes = new byte[4];
                _ = Encoding.UTF8.GetBytes(sb.ToString(), characterBytes);
                return BinaryPrimitives.ReadUInt32LittleEndian(characterBytes).ToString();
            }

            _ = sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Determine the type for a single operand.
        /// </summary>
        /// <param name="operand">A single operand with no comments or surrounding whitespace.</param>
        /// <param name="allowAddressLiteral">Whether the use of the literal address syntax (e.g :1024) is valid in this context.</param>
        /// <remarks>Operands will also be validated here.</remarks>
        /// <exception cref="SyntaxError">Thrown when an operand is badly formed.</exception>
        public static OperandType DetermineOperandType(string operand, bool allowAddressLiteral = true)
        {
            switch (operand[0])
            {
                case ':':
                {
                    if (operand.Length < 2)
                    {
                        throw new SyntaxError(Strings.Assembler_Error_Label_Empty_Name);
                    }
                    int offset;
                    if (operand[1] == '&')
                    {
                        offset = 2;
                        allowAddressLiteral = false;
                    }
                    else
                    {
                        offset = 1;
                    }
                    if (operand.Length <= offset)
                    {
                        throw new SyntaxError(Strings.Assembler_Error_Label_Empty_Name);
                    }
                    // Validating address literals with the same rules as labels has the intended
                    // side effect of disallowing negative and floating point literals
                    Match invalidMatch = allowAddressLiteral
                        ? Regex.Match(operand[offset..], "[^A-Za-z0-9_]")
                        : Regex.Match(operand[offset..], "^[0-9]|[^A-Za-z0-9_]");
                    if (!invalidMatch.Success && operand[offset] is >= '0' and <= '9')
                    {
                        // Literal address reference - validate as a numeric literal
                        ValidateNumericLiteral(operand[offset..]);
                    }
                    // Operand is a label reference - will assemble down to address
                    return invalidMatch.Success
                        ? throw new SyntaxError(string.Format(Strings.Assembler_Error_Label_Invalid_Character, operand, new string(' ', invalidMatch.Index + offset)))
                        : operand[1] == '&' ? OperandType.Literal : OperandType.Address;
                }
                case (>= '0' and <= '9') or '-' or '.':
                {
                    ValidateNumericLiteral(operand);
                    return OperandType.Literal;
                }
                case '"':
                    return OperandType.Literal;
                default:
                {
                    int offset = operand[0] == '*' ? 1 : 0;
                    return Enum.TryParse<Register>(operand[offset..].ToLowerInvariant(), out _)
                        ? operand[0] == '*' ? OperandType.Pointer : OperandType.Register
                        : throw new SyntaxError(
                            string.Format(Strings.Assembler_Error_Operand_Invalid, operand));
                }
            }
        }

        /// <summary>
        /// Parse an operand of literal type to its representation as bytes. 
        /// </summary>
        /// <remarks>
        /// Integer size constraints will be validated here, all other validation should be done as a part of <see cref="DetermineOperandType"/>.
        /// Strings are also converted to UTF-8 bytes by this method,
        /// though only strings that have already been pre-parsed and validated by <see cref="PreParseStringLiteral"/> should be passed.
        /// </remarks>
        /// <returns>The bytes representing the literal to be added to a program.</returns>
        /// <param name="parsedNumber">
        /// The value of the numeric literal parsed by the method,
        /// or the number of <see cref="char"/> contained in the given literal string if the literal was a string.
        /// </param>
        /// <exception cref="SyntaxError">Thrown when a string literal is given in an invalid context or an invalid format.</exception>
        /// <exception cref="OperandException">Thrown when the literal is too large for a single <see cref="ulong"/>.</exception>
        /// <exception cref="FormatException">Thrown when there are invalid characters in a numeric literal or the numeric literal is in an invalid format.</exception>
        public static byte[] ParseLiteral(string operand, bool allowString, out ulong parsedNumber)
        {
            if (operand[0] == '"')
            {
                if (!allowString)
                {
                    throw new SyntaxError(Strings.Assembler_Error_String_Not_Allowed);
                }
                if (operand[^1] != '"')
                {
                    throw new SyntaxError(Strings.Assembler_Error_String_Followed_Internal);
                }
                string str = operand[1..^1];
                parsedNumber = (uint)str.Length;
                return Encoding.UTF8.GetBytes(str);
            }
            operand = operand.ToLowerInvariant().Replace("_", "");
            try
            {
                // Hex (0x), Binary (0b), and Decimal literals are all supported
                parsedNumber = operand.StartsWith("0x")
                    ? Convert.ToUInt64(operand[2..], 16)
                : operand.StartsWith("0b")
                    ? Convert.ToUInt64(operand[2..], 2)
                : operand.Contains('.')
                    ? BitConverter.DoubleToUInt64Bits(Convert.ToDouble(operand))
                : operand.StartsWith('-')
                    ? (ulong)Convert.ToInt64(operand)
                    : Convert.ToUInt64(operand);
            }
            catch (OverflowException)
            {
                throw new OperandException(operand.StartsWith('-')
                    ? string.Format(Strings.Assembler_Error_Literal_Too_Small, long.MinValue, operand)
                    : string.Format(Strings.Assembler_Error_Literal_Too_Large, ulong.MaxValue, operand));
            }
            byte[] result = new byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(result, parsedNumber);
            return result;
        }

        /// <summary>
        /// Parse an operand of literal type to its representation as bytes. 
        /// </summary>
        /// <remarks>Strings and integer size constraints will be validated here, all other validation should be done as a part of <see cref="DetermineOperandType"/></remarks>
        /// <returns>The bytes representing the literal to be added to a program.</returns>
        /// <exception cref="SyntaxError">Thrown when there are invalid characters in the literal or the literal is in an invalid format.</exception>
        /// <exception cref="OperandException">Thrown when the literal is too large for a single <see cref="ulong"/>.</exception>
        public static byte[] ParseLiteral(string operand, bool allowString)
        {
            return ParseLiteral(operand, allowString, out _);
        }

        private void ResolveLabelReferences()
        {
            foreach ((string labelLink, (string labelTarget, string? filePath, int line)) in labelLinks)
            {
                if (!labels.TryGetValue(labelTarget, out ulong targetOffset))
                {
                    LabelNameException exc = new(
                        string.Format(Strings.Assembler_Error_Label_Not_Exists, labelTarget), line, filePath ?? "");
                    exc.ConsoleMessage = string.Format(Strings.Assembler_Error_On_Line, line, filePath ?? Strings.Generic_Base_File, exc.Message);
                    throw exc;
                }
                labels[labelLink] = targetOffset;
            }

            foreach ((string labelName, ulong labelAddress) in labels)
            {
                if (labelName.Equals("ENTRY", StringComparison.OrdinalIgnoreCase))
                {
                    entryPoint = labelAddress;
                }
                // Store address mapped to label name for debug file
                if (!addressLabelNames.TryGetValue(labelAddress, out List<string>? labelNameList))
                {
                    labelNameList = new List<string>();
                    addressLabelNames[labelAddress] = labelNameList;
                }
                labelNameList.Add(labelName);
            }

            Span<byte> programSpan = CollectionsMarshal.AsSpan(program);
            foreach ((string labelName, ulong insertOffset, string? filePath, int line) in labelReferences)
            {
                if (!labels.TryGetValue(labelName, out ulong targetOffset))
                {
                    LabelNameException exc = new(
                        string.Format(Strings.Assembler_Error_Label_Not_Exists, labelName), line, filePath ?? "");
                    exc.ConsoleMessage = string.Format(Strings.Assembler_Error_On_Line, line, filePath ?? Strings.Generic_Base_File, exc.Message);
                    throw exc;
                }
                // Write the now known address of the label to where it is required within the program
                BinaryPrimitives.WriteUInt64LittleEndian(programSpan[(int)insertOffset..], targetOffset);
            }
        }

        private void IncrementCurrentLine()
        {
            bool insideMacro = false;
            if (macroStack.TryPeek(out MacroStackFrame? currentMacro))
            {
                insideMacro = true;
                // Currently inside the usage of a multi-line macro
                currentMacro.RemainingLines--;
                while (currentMacro.RemainingLines < 0)
                {
                    // Reached the end of a macro.
                    // Pop from the macro stack until we reach an import that hasn't ended yet,
                    // or the end of all current macros.
                    _ = macroStack.Pop();
                    if (macroStack.TryPeek(out currentMacro))
                    {
                        currentMacro.RemainingLines--;
                    }
                    else
                    {
                        insideMacro = false;
                        break;
                    }
                }
            }

            // If we're inside an expanded macro the line number from the original file isn't changing
            if (!insideMacro)
            {
                if (importStack.TryPeek(out currentImport))
                {
                    // Currently inside an imported file
                    currentImport.CurrentLine++;
                    while (currentImport.CurrentLine > currentImport.TotalLines)
                    {
                        // Reached the end of an imported file.
                        // Pop from the import stack until we reach an import that hasn't ended yet,
                        // or the base file.
                        _ = importStack.Pop();
                        if (importStack.TryPeek(out currentImport))
                        {
                            currentImport.CurrentLine++;
                        }
                        else
                        {
                            baseFileLine++;
                            break;
                        }
                    }
                }
                else
                {
                    baseFileLine++;
                }
            }
        }

        /// <summary>
        /// Determine if a given statement is a known state-modifying assembler directive and process it if it is.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the directive, including the % prefix</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><see langword="true"/> - Directive was recognised and processed without error</item>
        /// <item><see langword="false"/> - Directive was not recognised and assembly of the statement should continue</item>
        /// </list>
        /// </returns>
        private bool ProcessStateDirective(string mnemonic, string[] operands, string rawLine, List<string> dynamicLines, ref int currentLineIndex)
        {
            switch (mnemonic.ToUpperInvariant())
            {
                // Import contents of another file
                case "%IMP":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_IMP_Operand_Count, operands.Length));
                    }
                    OperandType operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_IMP_Operand_Type, operandType));
                    }
                    if (operands[0][0] != '"')
                    {
                        throw new OperandException(Strings.Assembler_Error_IMP_Operand_String);
                    }
                    byte[] parsedBytes = ParseLiteral(operands[0], true);
                    string importPath = Encoding.UTF8.GetString(parsedBytes);
                    string resolvedPath = Path.GetFullPath(importPath);
                    if (!File.Exists(resolvedPath))
                    {
                        throw new ImportException(string.Format(Strings.Assembler_Error_IMP_File_Not_Exists, resolvedPath));
                    }
                    if (importStack.Any(x => string.Equals(x.ImportPath, resolvedPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ImportException(string.Format(Strings.Assembler_Error_Circular_Import, resolvedPath));
                    }
                    string[] linesToImport = File.ReadAllLines(resolvedPath);
                    // Insert the contents of the imported file so they are assembled next
                    dynamicLines.InsertRange(currentLineIndex + 1, linesToImport);
                    resolvedImports.Add((importPath, resolvedPath, (uint)program.Count));

                    warnings.AddRange(warningGenerator.NextInstruction(
                        Array.Empty<byte>(), mnemonic, operands,
                        currentImport?.CurrentLine ?? baseFileLine,
                        currentImport?.ImportPath ?? string.Empty, lineIsLabelled, lineIsEntry, rawLine, importStack));

                    importStack.Push(new ImportStackFrame(resolvedPath, 0, linesToImport.Length));
                    visitedFiles++;
                    return true;
                // Define macro
                case "%MACRO":
                    if (operands.Length == 2)
                    {
                        // Single-line macro
                        singleLineMacros[operands[0]] = operands[1];
                        singleLineMacroNames.Add(operands[0]);
                        singleLineMacroNames = singleLineMacroNames.OrderByDescending(n => n.Length).ToList();
                    }
                    else if (operands.Length == 1)
                    {
                        // Multi-line macro (must be terminated with %ENDMACRO)
                        int lineIndexAtStart = currentLineIndex;
                        int baseFileLineAtStart = baseFileLine;
                        Stack<ImportStackFrame> importStackAtStart = new(importStack.Select(f => new ImportStackFrame(f.ImportPath, f.CurrentLine, f.TotalLines)));
                        List<string> replacement = new();
                        // Add each line before the next encountered %ENDMACRO directive to the replacement text
                        while (true)
                        {
                            IncrementCurrentLine();
                            string line = dynamicLines[++currentLineIndex];
                            if (line.TrimStart().StartsWith("%ENDMACRO", StringComparison.OrdinalIgnoreCase))
                            {
                                // Parse the line to check it wasn't given with any operands
                                string[] parsedLine = ParseLine(line);
                                if (parsedLine.Length != 1)
                                {
                                    throw new OperandException(string.Format(Strings.Assembler_Error_ENDMACRO_Operand_Count, parsedLine.Length - 1));
                                }
                                break;
                            }
                            if (currentLineIndex >= dynamicLines.Count - 1)
                            {
                                // Rollback the state of the import stack to when macro definition started,
                                // so that error message shows that line instead of the end of the file
                                baseFileLine = baseFileLineAtStart;
                                currentLineIndex = lineIndexAtStart;
                                importStack.Clear();
                                foreach (ImportStackFrame frame in importStackAtStart.Reverse())
                                {
                                    importStack.Push(frame);
                                }
                                throw new MissingEndDirectiveException(Strings.Assembler_Error_ENDMACRO_Missing);
                            }
                            replacement.Add(line);
                        }
                        multiLineMacros[operands[0]] = replacement.ToArray();
                        multiLineMacroNames.Add(operands[0]);
                        multiLineMacroNames = multiLineMacroNames.OrderByDescending(n => n.Length).ToList();
                    }
                    else
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_MACRO_Operand_Count, operands.Length));
                    }
                    return true;
                // Define label address manually
                case "%LABEL_OVERRIDE":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_LABEL_OVERRIDE_Operand_Count, operands.Length));
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_LABEL_OVERRIDE_Operand_Type, operandType));
                    }
                    List<string> labelsToEdit = labels
                        .Where(kv => kv.Value == (ulong)program.Count && !overriddenLabels.Contains(kv.Key))
                        .Select(kv => kv.Key).ToList();
                    if (operands[0][0] == ':')
                    {
                        // Label reference used as %LABEL_OVERRIDE operand
                        foreach (string labelName in labelsToEdit)
                        {
                            // It's possible we don't know the address of the label yet, so store it as a "link" to resolve later
                            string linkedName = operands[0][2..];
                            if (labelName == linkedName)
                            {
                                throw new LabelNameException(string.Format(Strings.Assembler_Error_LABEL_OVERRIDE_Label_Reference_Also_Target, labelName));
                            }
                            // If the target label is already a link, store link to the actual target instead of chaining links
                            while (labelLinks.TryGetValue(linkedName, out (string Target, string? FilePath, int Line) checkName))
                            {
                                linkedName = checkName.Target;
                            }
                            labelLinks[labelName] = (linkedName, currentImport?.ImportPath, currentImport?.CurrentLine ?? baseFileLine);
                        }
                    }
                    else
                    {
                        _ = ParseLiteral(operands[0], true, out ulong parsedNumber);
                        foreach (string labelName in labelsToEdit)
                        {
                            // Overwrite the old label address
                            labels[labelName] = parsedNumber;
                        }
                    }

                    warnings.AddRange(warningGenerator.NextInstruction(
                        Array.Empty<byte>(), mnemonic, operands,
                        currentImport?.CurrentLine ?? baseFileLine,
                        currentImport?.ImportPath ?? string.Empty, lineIsLabelled, lineIsEntry, rawLine, importStack));

                    lineIsLabelled = false;
                    lineIsEntry = false;
                    overriddenLabels.UnionWith(labelsToEdit);
                    return true;
                // Toggle warnings
                case "%ANALYZER":
                    if (operands.Length != 3)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_ANALYZER_Operand_Count, operands.Length));
                    }
                    WarningSeverity severity = operands[0].ToUpperInvariant() switch
                    {
                        "ERROR" => WarningSeverity.NonFatalError,
                        "WARNING" => WarningSeverity.Warning,
                        "SUGGESTION" => WarningSeverity.Suggestion,
                        _ => throw new OperandException(Strings.Assembler_Error_ANALYZER_Operand_First)
                    };
                    if (!int.TryParse(operands[1], out int code))
                    {
                        throw new OperandException(Strings.Assembler_Error_ANALYZER_Operand_Second);
                    }
                    _ = operands[2].ToUpperInvariant() switch
                    {
                        // Disable
                        "0" => DisableAssemblerWarning(severity, code),
                        // Enable
                        "1" => EnableAssemblerWarning(severity, code),
                        // Restore
                        "R" => ResetAssemblerWarning(severity, code),
                        _ => throw new OperandException(
                            Strings.Assembler_Error_ANALYZER_Operand_Third)
                    };
                    return true;
                // Manually emit assembler warning
                case "%MESSAGE":
                    if (operands.Length is < 1 or > 2)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_MESSAGE_Operand_Count, operands.Length));
                    }
                    severity = operands[0].ToUpperInvariant() switch
                    {
                        "ERROR" => WarningSeverity.NonFatalError,
                        "WARNING" => WarningSeverity.Warning,
                        "SUGGESTION" => WarningSeverity.Suggestion,
                        _ => throw new OperandException(Strings.Assembler_Error_MESSAGE_Operand_First)
                    };
                    string? message = null;
                    if (operands.Length == 2)
                    {
                        operandType = DetermineOperandType(operands[1]);
                        if (operandType != OperandType.Literal)
                        {
                            throw new OperandException(string.Format(Strings.Assembler_Error_MESSAGE_Operand_Second_Type, operandType));
                        }
                        if (operands[1][0] != '"')
                        {
                            throw new OperandException(Strings.Assembler_Error_MESSAGE_Operand_Second_String);
                        }
                        parsedBytes = ParseLiteral(operands[1], true);
                        message = Encoding.UTF8.GetString(parsedBytes);
                    }
                    warnings.Add(new Warning(
                        severity, 0000, currentImport?.ImportPath ?? string.Empty, currentImport?.CurrentLine ?? baseFileLine,
                        mnemonic, operands, rawLine, message));
                    return true;
                // Print assembler state
                case "%DEBUG":
                    if (operands.Length != 0)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_DEBUG_Operand_Count, operands.Length));
                    }
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Header,
                        currentImport?.CurrentLine ?? baseFileLine,
                        currentImport is null ? Strings.Generic_Base_File : currentImport.ImportPath, program.Count);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Label_Header, labels.Count);
                    foreach ((string labelName, ulong address) in labels)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Label_Line, labelName, address);
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_LabelRef_Header, labelReferences.Count);
                    foreach ((string labelName, ulong insertOffset, string? filePath, int lineNum) in labelReferences)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_LabelRef_Line, labelName, insertOffset, filePath ?? Strings.Generic_Base_File, lineNum);
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Single_Line_Macro_Header, singleLineMacros.Count);
                    foreach ((string macro, string replacement) in singleLineMacros)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Single_Line_Macro_Line, macro, replacement);
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Multi_Line_Macro_Header, multiLineMacros.Count);
                    foreach ((string macro, string[] replacement) in multiLineMacros)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Multi_Line_Macro_Line, macro, replacement.Length >= 1 ? replacement[0] : "");
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Macro_Stack_Header);
                    foreach (MacroStackFrame macroFrame in macroStack)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Macro_Stack_Line, macroFrame.MacroName, macroFrame.RemainingLines);
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Import_Stack_Header);
                    foreach (ImportStackFrame importFrame in importStack)
                    {
                        Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Import_Stack_Line, importFrame.ImportPath, importFrame.CurrentLine, importFrame.TotalLines);
                    }
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Import_Stack_Base, baseFileLine);
                    Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Current_Extensions, usedExtensions);
                    Console.ResetColor();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a given statement is a known data insertion assembler directive and process it if it is.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the directive, including the % prefix</param>
        /// <param name="referencedLabels">A list to add any new label references to. Will be modified in-place</param>
        /// <param name="newBytes">The bytes to insert into the program. Will be null if the function returns false</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><see langword="true"/> - Directive was recognised and processed without error</item>
        /// <item><see langword="false"/> - Directive was not recognised and assembly of the statement should continue</item>
        /// </list>
        /// </returns>
        private static bool ProcessDataDirective(string mnemonic, string[] operands, List<(string, ulong)> referencedLabels, [MaybeNullWhen(false)] out byte[] newBytes)
        {
            switch (mnemonic.ToUpperInvariant())
            {
                // Single byte/string insertion
                case "%DAT":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_DAT_Operand_Count, operands.Length));
                    }
                    OperandType operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_DAT_Operand_Type, operandType));
                    }
                    if (operands[0][0] == ':')
                    {
                        throw new OperandException(Strings.Assembler_Error_DAT_Operand_Label_Reference);
                    }
                    byte[] parsedBytes = ParseLiteral(operands[0], true);
                    if (operands[0][0] != '"' && parsedBytes[1..].Any(b => b != 0))
                    {
                        throw new OperandException(
                            string.Format(Strings.Assembler_Error_DAT_Operand_Too_Large, operands[0]));
                    }
                    newBytes = operands[0][0] != '"' ? parsedBytes[..1] : parsedBytes;
                    return true;
                // 0-padding
                case "%PAD":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_PAD_Operand_Count, operands.Length));
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_PAD_Operand_Type, operandType));
                    }
                    if (operands[0][0] == ':')
                    {
                        throw new OperandException(Strings.Assembler_Error_PAD_Operand_Label_Reference);
                    }
                    _ = ParseLiteral(operands[0], false, out ulong parsedNumber);
                    // Generate an array of 0-bytes with the specified length
                    newBytes = Enumerable.Repeat((byte)0, (int)parsedNumber).ToArray();
                    return true;
                // 64-bit/floating point number insertion
                case "%NUM":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_NUM_Operand_Count, operands.Length));
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_NUM_Operand_Type, operandType));
                    }
                    if (operands[0][0] == ':')
                    {
                        // Label reference used as %NUM operand
                        referencedLabels.Add((operands[0][2..], 0));
                        // Label location will be resolved later, pad with 0s for now
                        newBytes = Enumerable.Repeat((byte)0, 8).ToArray();
                        return true;
                    }
                    newBytes = ParseLiteral(operands[0], false);
                    return true;
                // Raw file insertion
                case "%IBF":
                    if (operands.Length != 1)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_IBF_Operand_Count, operands.Length));
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != OperandType.Literal)
                    {
                        throw new OperandException(string.Format(Strings.Assembler_Error_IBF_Operand_Type, operandType));
                    }
                    if (operands[0][0] != '"')
                    {
                        throw new OperandException(Strings.Assembler_Error_IBF_Operand_String);
                    }
                    parsedBytes = ParseLiteral(operands[0], true);
                    string importPath = Encoding.UTF8.GetString(parsedBytes);
                    string resolvedPath = Path.GetFullPath(importPath);
                    if (!File.Exists(resolvedPath))
                    {
                        throw new ImportException(string.Format(Strings.Assembler_Error_IBF_File_Not_Exists, resolvedPath));
                    }
                    newBytes = File.ReadAllBytes(resolvedPath);
                    return true;
                default:
                    newBytes = null;
                    return false;
            }
        }

        private static void ValidateNumericLiteral(string operand)
        {
            operand = operand.ToLowerInvariant();
            Match invalidMatch = operand.StartsWith("0x") ? Regex.Match(operand, "[^0-9a-f_](?<!^0[xX])") : operand.StartsWith("0b")
                ? Regex.Match(operand, "[^0-1_](?<!^0[bB])")
                : Regex.Match(operand, @"[^0-9_\.](?<!^-)");
            if (invalidMatch.Success)
            {
                throw new SyntaxError(string.Format(Strings.Assembler_Error_Literal_Invalid_Character, operand, new string(' ', invalidMatch.Index)));
            }
            // Edge-case syntax errors not detected by invalid character regular expressions
            if ((operand[0] == '.' && operand.Length == 1) || operand == "-.")
            {
                throw new SyntaxError(Strings.Assembler_Error_Literal_Floating_Point_Decimal_Only);
            }
            if (operand[0] == '-' && operand.Length == 1)
            {
                throw new SyntaxError(Strings.Assembler_Error_Literal_Negative_Dash_Only);
            }
            if (operand.IndexOf('.') != operand.LastIndexOf('.'))
            {
                throw new SyntaxError(string.Format(Strings.Assembler_Error_Literal_Too_Many_Points, operand, new string(' ', operand.LastIndexOf('.'))));
            }
            if (operand is "0x" or "0b")
            {
                throw new SyntaxError(Strings.Assembler_Error_Literal_Base_Prefix_Only);
            }
        }
    }
}
