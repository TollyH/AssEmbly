using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    [Serializable]
    public readonly record struct AssemblyResult
    (
        byte[] Program,
        string DebugInfo,
        string[] ExpandedSourceFile,
        Warning[] Warnings,
        ulong EntryPoint,
        AAPFeatures UsedExtensions,
        FilePosition[] AssembledLines,
        int AssembledFiles
    );

    /// <summary>
    /// Assembles text based AssEmbly programs to compiled AssEmbly bytes.
    /// </summary>
    public partial class Assembler
    {
        public class ImportStackFrame(string importPath, int currentLine, int totalLines) : ICloneable
        {
            public string ImportPath { get; } = importPath;
            public int CurrentLine { get; set; } = currentLine;
            public int TotalLines { get; } = totalLines;
            public bool AnyAssembledLines { get; set; } = false;

            public object Clone()
            {
                return new ImportStackFrame(ImportPath, CurrentLine, TotalLines)
                {
                    AnyAssembledLines = AnyAssembledLines
                };
            }
        }

        public class MacroStackFrame(string macroName, int remainingLines) : ICloneable
        {
            public string MacroName { get; } = macroName;
            public int RemainingLines { get; set; } = remainingLines;

            public object Clone()
            {
                return new MacroStackFrame(MacroName, RemainingLines);
            }
        }

        public readonly struct AssemblyPosition(Stack<ImportStackFrame> importStack, Stack<MacroStackFrame> macroStack, List<string> lines,
            int currentLineIndex, int baseFileLine, int macroLineDepth)
        {
            public readonly Stack<ImportStackFrame> ImportStack = importStack;
            public readonly Stack<MacroStackFrame> MacroStack = macroStack;
            public readonly List<string> Lines = lines;
            public readonly int CurrentLineIndex = currentLineIndex;
            public readonly int BaseFileLine = baseFileLine;
            public readonly int MacroLineDepth = macroLineDepth;
        }

        public const int DefaultMacroExpansionLimit = 1024;
        public const int DefaultWhileRepeatLimit = 16384;

        public bool Finalized { get; private set; }

        public int MacroExpansionLimit { get; set; } = DefaultMacroExpansionLimit;
        // This limit is per program, not per loop
        public int WhileRepeatLimit { get; set; } = DefaultWhileRepeatLimit;

        // Lines that start with anything in this HashSet followed by a space when trimmed are not subject to single-line macro expansion
        private static readonly HashSet<string> automaticMacroExcludedMnemonics = new(StringComparer.OrdinalIgnoreCase) { "%DELMACRO", "%MACRO" };

        // The lines to assemble may change during assembly, for example importing a file
        // will extend the list of lines to assemble as and when the import is reached.
        private readonly List<string> dynamicLines = new();
        private int lineIndex = 0;
        // Final compiled byte list
        private readonly List<byte> program = new();
        // Map of label names to final memory addresses
        private readonly Dictionary<string, ulong> labels = new();
        // Map of label names that link to another label name, along with the file and line the link was made on
        private readonly Dictionary<string, (string Target, string? FilePath, int Line)> labelLinks = new();
        // List of references to labels by name along with the address to insert the relevant address in to.
        // Also has the line and path to the file (if imported) that the reference was assembled from for use in error messages.
        private readonly List<(string LabelName, ulong Address, AssemblyPosition Position)> labelReferences = new();
        private readonly HashSet<string> overriddenLabels = new();
        // string -> replacement. All single-line macros are expanded before multi-line macros.
        private readonly Dictionary<string, string> singleLineMacros = new();
        private readonly Dictionary<string, string[]> multiLineMacros = new();
        // Sorted from the longest name to the shortest name - should always match the keys of the above dictionaries
        private List<string> singleLineMacroNames = new();
        private List<string> multiLineMacroNames = new();
        // '@' prefix is not included in name
        private readonly Dictionary<string, ulong> assemblerVariables = new();
        // '@!' prefix is not included in name
        // Cannot be edited by program
        private readonly Dictionary<string, Func<ulong>> assemblerConstants;
        private AAPFeatures usedExtensions = AAPFeatures.None;

        // For detecting circular imports and tracking imported line numbers
        private readonly Stack<ImportStackFrame> importStack = new();
        private ImportStackFrame? currentImport = null;
        private int baseFileLine = 0;
        private FilePosition currentFilePosition = new();

        // When %REPEAT is used, the current position of the assembler is cloned and added to this stack
        private readonly Stack<(AssemblyPosition StartPosition, ulong RemainingIterations)> currentRepeatSections = new();

        // When %WHILE is used, the current position of the assembler is cloned and added to this stack
        private readonly Stack<AssemblyPosition> currentWhileLoops = new();

        // Used to keep track of multi-line macros
        private readonly Stack<MacroStackFrame> macroStack = new();
        private MacroStackFrame? currentMacro = null;
        // The number of lines assembled for all macros referenced by current file line
        private int macroLineDepth = 0;
        // The number of macro expansions that have occurred on the current line. Used to enforce the macro expansion limit.
        private int currentMacroExpansions = 0;

        private bool insideMacroSkipBlock = false;

        private int currentlyOpenIfBlocks = 0;
        // For generating an exception if an %ENDIF directive is missing
        private AssemblyPosition lastIfDefinedPosition;

        // Used in combination with SetPosition() to start assembly from a given position instead of the line after it.
        private bool skipNextLineIncrement = false;

        // Used to enforce the program-wide while repeat limit
        private int whileRepeats = 0;

        // Used for debug files
        private readonly List<(ulong Address, string Line)> assembledLines = new();
        private readonly Dictionary<ulong, List<string>> addressLabelNames = new();
        private readonly List<(string LocalPath, string FullPath, ulong Address)> resolvedImports = new();
        private readonly List<(ulong Address, FilePosition Position)> fileLineMap = new();

        private readonly AssemblerWarnings warningGenerator;
        private readonly List<Warning> warnings = new();

        private readonly HashSet<int> initialDisabledNonFatalErrors;
        private readonly HashSet<int> initialDisabledWarnings;
        private readonly HashSet<int> initialDisabledSuggestions;

        private bool lineIsLabelled = false;
        private bool lineIsEntry = false;
        private ulong entryPoint = 0;

        private readonly HashSet<FilePosition> processedLines = new();
        private readonly Dictionary<string, int> timesSeenFile = new();
        // Files that begin with an %ASM_ONCE directive (i.e. won't throw a circular import error)
        private readonly HashSet<string> completeAsmOnceFiles = new();

        /// <param name="usingV1Format">
        /// Whether or not a v1 executable will be generated from this assembly.
        /// </param>
        /// <param name="usingV1Stack">
        /// Whether or not this program expects to use the v1 callstack behaviour.
        /// </param>
        /// <param name="disabledNonFatalErrors">A set of non-fatal error codes to disable.</param>
        /// <param name="disabledWarnings">A set of warning codes to disable.</param>
        /// <param name="disabledSuggestions">A set of suggestion codes to disable.</param>
        public Assembler(bool usingV1Format, bool usingV1Stack,
            IEnumerable<int> disabledNonFatalErrors, IEnumerable<int> disabledWarnings, IEnumerable<int> disabledSuggestions)
        {
            warningGenerator = new AssemblerWarnings(usingV1Format);
            warningGenerator.DisabledNonFatalErrors.UnionWith(disabledNonFatalErrors);
            warningGenerator.DisabledWarnings.UnionWith(disabledWarnings);
            warningGenerator.DisabledSuggestions.UnionWith(disabledSuggestions);

            initialDisabledNonFatalErrors = warningGenerator.DisabledNonFatalErrors.ToHashSet();
            initialDisabledWarnings = warningGenerator.DisabledWarnings.ToHashSet();
            initialDisabledSuggestions = warningGenerator.DisabledSuggestions.ToHashSet();

            assemblerConstants = new Dictionary<string, Func<ulong>>()
            {
                { "ASSEMBLER_VERSION_MAJOR", () => (ulong)(Program.version?.Major ?? 0) },
                { "ASSEMBLER_VERSION_MINOR", () => (ulong)(Program.version?.Minor ?? 0) },
                { "ASSEMBLER_VERSION_PATCH", () => (ulong)(Program.version?.Build ?? 0) },
                { "V1_FORMAT", () => usingV1Format ? 1UL : 0UL },
                { "V1_CALL_STACK", () => usingV1Stack ? 1UL : 0UL },
                { "IMPORT_DEPTH", () => (ulong)importStack.Count },
                { "CURRENT_ADDRESS", () => (ulong)program.Count },
            };

            InitializeStateDirectives(out stateDirectives);
        }

        public Assembler() : this(false, false,
            Enumerable.Empty<int>(), Enumerable.Empty<int>(), Enumerable.Empty<int>()) { }

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
        /// Create or update an assembler variable with the given name to the specified value.
        /// </summary>
        /// <remarks>This method will perform validation the name of the assembler variable.</remarks>
        /// <exception cref="SyntaxError">Thrown if the given name is invalid.</exception>
        public void SetAssemblerVariable(string name, ulong value)
        {
            if (name.Length == 0)
            {
                throw new SyntaxError(Strings.Assembler_Error_Variable_Empty_Name);
            }

            Match invalidMatch = Regex.Match(name, "[^A-Za-z0-9_]");
            if (invalidMatch.Success)
            {
                throw new SyntaxError(
                    string.Format(Strings.Assembler_Error_Variable_Invalid_Character, name, new string(' ', invalidMatch.Index)));
            }

            assemblerVariables[name] = value;
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
                resolvedImports, fileLineMap);
            return new AssemblyResult(
                programBytes, debugInfo, dynamicLines.ToArray(), warnings.ToArray(),
                entryPoint, usedExtensions, processedLines.ToArray(), timesSeenFile.Count);
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

            dynamicLines.Clear();
            dynamicLines.AddRange(lines.ToList());

            importStack.Clear();
            baseFileLine = 1;
            currentFilePosition = new FilePosition(1, "");

            lineIndex = 0;
            do
            {
                _ = processedLines.Add(currentFilePosition);
                string rawLine = CleanLine(dynamicLines[lineIndex]);
                if (rawLine.Length == 0)
                {
                    continue;
                }
                try
                {
                    if (ProcessLineMacros(ref rawLine, dynamicLines, lineIndex))
                    {
                        continue;
                    }

                    string preVariableLine = rawLine;
                    rawLine = ProcessAssemblerVariables(rawLine);

                    string[] line = ParseLine(rawLine);
                    if (line.Length == 0)
                    {
                        continue;
                    }

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

                    FilePosition startPosition = currentFilePosition;
                    bool lineWasLabelled = lineIsLabelled;
                    bool lineWasEntry = lineIsEntry;
                    if (ProcessStateDirective(mnemonic, operands, preVariableLine))
                    {
                        // Don't run warning analyzers if directive has changed our position in the file
                        // - it will have already run them for the original line if required
                        if (currentFilePosition == startPosition)
                        {
                            warnings.AddRange(warningGenerator.NextInstruction(
                                Array.Empty<byte>(), mnemonic, operands, preVariableLine,
                                currentFilePosition, lineWasLabelled, lineWasEntry, rawLine, importStack,
                                currentMacro?.MacroName, macroLineDepth));
                        }

                        // Directive found and processed, move onto next statement
                        continue;
                    }

                    if (currentImport is not null)
                    {
                        currentImport.AnyAssembledLines = true;
                    }

                    (byte[] newBytes, List<(string LabelName, ulong AddressOffset)> newLabels) =
                        AssembleStatement(mnemonic, operands, out AAPFeatures newFeatures);

                    foreach ((string label, ulong relativeOffset) in newLabels)
                    {
                        labelReferences.Add((label, relativeOffset + (uint)program.Count, GetCurrentPosition()));
                    }

                    usedExtensions |= newFeatures;

                    assembledLines.Add(((uint)program.Count, preVariableLine));
                    fileLineMap.Add(((uint)program.Count, currentFilePosition));

                    program.AddRange(newBytes);

                    warnings.AddRange(warningGenerator.NextInstruction(
                        newBytes, mnemonic, operands, preVariableLine,
                        currentFilePosition, lineIsLabelled, lineIsEntry, rawLine, importStack,
                        currentMacro?.MacroName, macroLineDepth));

                    lineIsLabelled = false;
                    lineIsEntry = false;
                }
                catch (AssemblerException e)
                {
                    HandleAssemblerException(e);
                    throw;
                }
            } while (IncrementCurrentLine());

            // Check for missing %ENDREPEAT directives
            if (currentRepeatSections.Count > 0)
            {
                // Rollback the current position of the assembler,
                // so that the line that the repeat started on is shown in the error message
                SetCurrentPosition(currentRepeatSections.Pop().StartPosition, false);
                EndingDirectiveException e = new(Strings.Assembler_Error_ENDREPEAT_Missing);
                HandleAssemblerException(e);
                throw e;
            }

            // Check for missing %ENDWHILE directives
            if (currentWhileLoops.Count > 0)
            {
                // Rollback the current position of the assembler,
                // so that the line that the while loop started on is shown in the error message
                SetCurrentPosition(currentWhileLoops.Pop(), false);
                EndingDirectiveException e = new(Strings.Assembler_Error_ENDWHILE_Missing);
                HandleAssemblerException(e);
                throw e;
            }

            // Check for missing %ENDIF directives
            if (currentlyOpenIfBlocks > 0)
            {
                SetCurrentPosition(lastIfDefinedPosition, false);
                EndingDirectiveException e = new(Strings.Assembler_Error_ENDIF_Missing);
                HandleAssemblerException(e);
                throw e;
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
                        operandBytes.Add((byte)Enum.Parse<Register>(operands[i], true));
                        break;
                    case OperandType.Literal:
                        if (operands[i].StartsWith(":&", StringComparison.OrdinalIgnoreCase))
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
                        operandBytes.Add((byte)Enum.Parse<Register>(operands[i][1..], true));
                        break;
                }
            }
            if (!Data.Mnemonics.TryGetValue((mnemonic, operandTypes), out Opcode opcode))
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
            bool openDoubleQuote = false;
            bool openSingleQuote = false;
            bool openBackslash = false;
            int indexOfSemicolon = -1;
            // Find the first instance of an unquoted semicolon, removing it and every character after it if found.
            // We don't need to actually validate string/character literals yet, we just care about removing comments.
            for (int i = 0; i < line.Length && indexOfSemicolon == -1; i++)
            {
                switch (line[i])
                {
                    case '"' when !openSingleQuote && !openBackslash:
                        openDoubleQuote = !openDoubleQuote;
                        break;
                    case '\'' when !openDoubleQuote && !openBackslash:
                        openSingleQuote = !openSingleQuote;
                        break;
                    case '\\' when openDoubleQuote || openSingleQuote:
                        openBackslash = !openBackslash;
                        break;
                    case ';' when !openDoubleQuote && !openSingleQuote:
                        indexOfSemicolon = i;
                        break;
                    default:
                        openBackslash = false;
                        break;
                }
            }
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
                        isMacro = mnemonic.Equals("%MACRO", StringComparison.OrdinalIgnoreCase)
                            || mnemonic.Equals("%DELMACRO", StringComparison.OrdinalIgnoreCase);
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
                    return Enum.TryParse<Register>(operand[offset..], true, out _)
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
            operand = operand.Replace("_", "");
            try
            {
                // Hex (0x), Binary (0b), and Decimal literals are all supported
                parsedNumber = operand.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? Convert.ToUInt64(operand[2..], 16)
                : operand.StartsWith("0b", StringComparison.OrdinalIgnoreCase)
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

        /// <summary>
        /// Process a set of macro parameters as a part of an entire line of AssEmbly source.
        /// Accounts for backslash escapes and ensures that the parameters are surrounded with brackets.
        /// </summary>
        /// <param name="line">The entire source line with the parameters contained.</param>
        /// <param name="startIndex">
        /// The index in the line to the opening bracket.
        /// It will be incremented automatically by this method to the index of the closing bracket.
        /// </param>
        /// <returns>
        /// An array where each element represents a single parameter to the macro.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">The given start index is outside the range of the given line.</exception>
        /// <exception cref="ArgumentException">The given line is invalid.</exception>
        /// <exception cref="SyntaxError">The parameters in the line are invalid.</exception>
        public static string[] ParseMacroParameters(string line, ref int startIndex)
        {
            if (startIndex < 0 || startIndex >= line.Length)
            {
                throw new IndexOutOfRangeException(Strings.Assembler_Error_Macro_Params_Bad_StartIndex);
            }
            if (line[startIndex] != '(')
            {
                throw new ArgumentException(Strings.Assembler_Error_Macro_Params_Bad_First_Char);
            }

            List<string> parameters = new();
            StringBuilder currentParameter = new();
            bool openBackslash = false;
            int openBrackets = 0;
            int i = startIndex;
            bool paramsClosed = false;
            while (!paramsClosed)
            {
                if (++i >= line.Length)
                {
                    throw new SyntaxError(
                        string.Format(Strings.Assembler_Error_Macro_Params_EndOfLine, line, new string(' ', i - 1)));
                }

                char c = line[i];

                if (!openBackslash)
                {
                    if (c == '(')
                    {
                        openBrackets++;
                    }
                    else if (openBrackets > 0)
                    {
                        if (c == ')')
                        {
                            openBrackets--;
                        }
                    }
                    else
                    {
                        switch (c)
                        {
                            case ')':
                                paramsClosed = true;
                                continue;
                            case '\\':
                                openBackslash = true;
                                continue;
                            case ',':
                                parameters.Add(currentParameter.ToString());
                                _ = currentParameter.Clear();
                                continue;
                        }
                    }
                }

                _ = currentParameter.Append(c);
                openBackslash = !openBackslash && c == '\\';
            }
            startIndex = i;

            // Add the final parameter
            parameters.Add(currentParameter.ToString());
            return parameters.ToArray();
        }

        /// <summary>
        /// Replaces parameters in the form $x within a macro replacement string with their corresponding value in the given list of parameters.
        /// </summary>
        /// <returns>A string with all macro parameters replaced.</returns>
        /// <remarks>
        /// Missing parameters will be replaced with empty strings, unless the parameter index is followed by an ! in which case an error will be thrown.
        /// The $ sign can be escaped by doubling it up ($$).
        /// </remarks>
        /// <exception cref="MacroExpansionException">Thrown if a required parameter is missing.</exception>
        /// <exception cref="SyntaxError">Thrown if a $ is missing an index number.</exception>
        public static string InsertMacroParameters(string macroContent, IList<string> parameters)
        {
            StringBuilder formattedContent = new();
            bool parsingParameter = false;
            int parsedParameterIndex = 0;

            for (int i = 0; i < macroContent.Length; i++)
            {
                char c = macroContent[i];

                if (parsingParameter)
                {
                    if (char.IsAsciiDigit(c))
                    {
                        parsedParameterIndex *= 10;
                        // Convert digit value to integer
                        parsedParameterIndex += c - '0';
                    }
                    // Parameter indices are terminated as soon as a non-numeric character is encountered
                    else
                    {
                        parsingParameter = false;

                        if (parsedParameterIndex < parameters.Count)
                        {
                            _ = formattedContent.Append(parameters[parsedParameterIndex]);
                        }

                        // Parameter marked as required (! not included in final content)
                        if (c == '!')
                        {
                            if (parsedParameterIndex >= parameters.Count)
                            {
                                throw new MacroExpansionException(string.Format(Strings.Assembler_Error_Macro_Missing_Parameter, parsedParameterIndex));
                            }
                            parsedParameterIndex = 0;
                            // Don't add literal ! character to replacement text
                            continue;
                        }

                        parsedParameterIndex = 0;
                    }
                }

                // parsingParameter can be false here even if it was true before (i.e. if parameter just ended)
                if (!parsingParameter)
                {
                    if (c == '$')
                    {
                        if (i >= macroContent.Length - 1 || (!char.IsAsciiDigit(macroContent[i + 1]) && macroContent[i + 1] != '$'))
                        {
                            throw new SyntaxError(string.Format(Strings.Assembler_Error_Macro_Param_No_Number, macroContent, new string(' ', i)));
                        }

                        if (macroContent[i + 1] != '$')
                        {
                            parsingParameter = true;
                        }
                        else
                        {
                            // $$ escapes to a single $ without starting a parameter
                            i++;
                            _ = formattedContent.Append('$');
                        }
                    }
                    else
                    {
                        _ = formattedContent.Append(c);
                    }
                }
            }

            if (parsingParameter && parsedParameterIndex < parameters.Count)
            {
                // Process any parameter that may be at the end of the string
                // We can skip the required parameter check, as we know the parameter can't have ended in an '!'
                _ = formattedContent.Append(parameters[parsedParameterIndex]);
            }

            return formattedContent.ToString();
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
            foreach ((string labelName, ulong insertOffset, AssemblyPosition position) in labelReferences)
            {
                if (!labels.TryGetValue(labelName, out ulong targetOffset))
                {
                    // Rollback the current position of the assembler,
                    // so that the line that the label was defined on is shown in the error message
                    SetCurrentPosition(position, false);
                    LabelNameException exc = new(
                        string.Format(Strings.Assembler_Error_Label_Not_Exists, labelName));
                    HandleAssemblerException(exc);
                    throw exc;
                }
                // Write the now known address of the label to where it is required within the program
                BinaryPrimitives.WriteUInt64LittleEndian(programSpan[(int)insertOffset..], targetOffset);
            }
        }

        private bool IncrementCurrentLine()
        {
            if (skipNextLineIncrement)
            {
                skipNextLineIncrement = false;
                return lineIndex < dynamicLines.Count;
            }

            lineIndex++;
            currentMacroExpansions = 0;

            bool insideMacro = false;
            if (macroStack.TryPeek(out currentMacro))
            {
                insideMacro = true;
                macroLineDepth++;
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
                macroLineDepth = 0;
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

            currentFilePosition = new FilePosition(
                currentImport?.CurrentLine ?? baseFileLine,
                currentImport?.ImportPath ?? string.Empty);

            return lineIndex < dynamicLines.Count;
        }

        private AssemblyPosition GetCurrentPosition()
        {
            return new AssemblyPosition(importStack.NestedCopy(), macroStack.NestedCopy(), new List<string>(dynamicLines),
                lineIndex, baseFileLine, macroLineDepth);
        }

        /// <summary>
        /// Restore the current position of the assembler to a previous position captured by <see cref="GetCurrentPosition"/>.
        /// </summary>
        /// <param name="skipIncrement">
        /// If <see langword="false"/>, assembly will continue from the line <i>after</i> the one in the specified position.
        /// </param>
        private void SetCurrentPosition(AssemblyPosition position, bool skipIncrement)
        {
            if (skipIncrement)
            {
                skipNextLineIncrement = true;
            }

            importStack.SetContentTo(position.ImportStack.NestedCopy());
            macroStack.SetContentTo(position.MacroStack.NestedCopy());

            dynamicLines.Clear();
            dynamicLines.AddRange(position.Lines);

            lineIndex = position.CurrentLineIndex;
            baseFileLine = position.BaseFileLine;
            macroLineDepth = position.MacroLineDepth;

            _ = macroStack.TryPeek(out currentMacro);
            _ = importStack.TryPeek(out currentImport);

            currentFilePosition = new FilePosition(
                currentImport?.CurrentLine ?? baseFileLine,
                currentImport?.ImportPath ?? string.Empty);
        }

        private string ExpandSingleLineMacros(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                foreach (string macro in singleLineMacroNames)
                {
                    if (macro.Length > text.Length - i)
                    {
                        continue;
                    }

                    bool match = true;
                    for (int j = 0; j < macro.Length; j++)
                    {
                        if (text[i + j] != macro[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        if (++currentMacroExpansions > MacroExpansionLimit)
                        {
                            throw new MacroExpansionException(string.Format(Strings.Assembler_Error_Macro_Limit_Exceeded, MacroExpansionLimit));
                        }
                        string[] parameters;
                        int paramIndex = i + macro.Length;
                        if (text.Length > paramIndex && text[paramIndex] == '(')
                        {
                            // Recursively expand macros in parameters
                            parameters = ParseMacroParameters(text, ref paramIndex).Select(ExpandSingleLineMacros).ToArray();
                            // Don't include the closing bracket
                            paramIndex++;
                        }
                        else
                        {
                            parameters = Array.Empty<string>();
                        }
                        text = text[..i] + InsertMacroParameters(singleLineMacros[macro], parameters) + text[paramIndex..];
                        // If a replacement occured, go back to beginning of line and try every macro again
                        i = -1;
                        break;
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Expand any matching single-line macros on the current line,
        /// then insert the contents of a matching multi-line macro after the current line if an exact match exists.
        /// </summary>
        /// <param name="rawLine">The line of AssEmbly code without trailing whitespace or comments</param>
        /// <returns>
        /// <see langword="true"/> if assembly should now immediately move onto the next line, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// This method accounts for the '!' line prefix and handles the starting and stopping of macro disabling blocks.
        /// <paramref name="rawLine"/> will have the '!' removed by this method.
        /// </remarks>
        private bool ProcessLineMacros(ref string rawLine, List<string> dynamicLines, int lineIndex)
        {
            if (rawLine == "!>")
            {
                if (insideMacroSkipBlock)
                {
                    throw new SyntaxError(Strings.Assembler_Error_Macro_Disable_Block_Nested);
                }
                insideMacroSkipBlock = true;
                return true;
            }
            if (rawLine == "<!")
            {
                if (!insideMacroSkipBlock)
                {
                    throw new SyntaxError(Strings.Assembler_Error_Macro_Disable_Block_Missing_Start);
                }
                insideMacroSkipBlock = false;
                return true;
            }

            if (rawLine[0] == '!')
            {
                // Remove the '!' prefix and skip macro processing for this line
                rawLine = CleanLine(rawLine[1..]);
            }
            // Don't do macro replacement on the %DELMACRO and %MACRO directives to make it easier to un-define/redefine existing macros
            else if (!insideMacroSkipBlock && !automaticMacroExcludedMnemonics.Contains(rawLine.Split(' ')[0]))
            {
                rawLine = ExpandSingleLineMacros(rawLine);

                // Multi-line macro expansion
                foreach (string macro in multiLineMacroNames)
                {
                    if (macro.Length > rawLine.Length)
                    {
                        continue;
                    }
                    string[] parameters;
                    // A multi-line macro can be considered as having parameters when the name of the macro and a parameter list are the only things on a line
                    if (macro.Length < rawLine.Length && rawLine[macro.Length] == '(' && rawLine[^1] == ')' &&
                        rawLine[..macro.Length] == macro)
                    {
                        int paramIndex = macro.Length;
                        parameters = ParseMacroParameters(rawLine, ref paramIndex).Select(ExpandSingleLineMacros).ToArray();
                        if (paramIndex != rawLine.Length - 1)
                        {
                            throw new SyntaxError(string.Format(Strings.Assembler_Error_Macro_Params_Unescaped_Close, rawLine, new string(' ', paramIndex)));
                        }
                        // Remove now-parsed parameters from line
                        rawLine = rawLine[..macro.Length];
                    }
                    else
                    {
                        parameters = Array.Empty<string>();
                    }
                    // Multi-line macros need an exact match with the macro name
                    if (rawLine == macro)
                    {
                        if (macroStack.Any(m => m.MacroName == macro))
                        {
                            throw new MacroExpansionException(string.Format(Strings.Assembler_Error_Circular_Macro, macro));
                        }
                        string[] replacement = multiLineMacros[macro];
                        dynamicLines.InsertRange(lineIndex + 1, replacement.Select(r => InsertMacroParameters(r, parameters)));
                        macroStack.Push(new MacroStackFrame(macro, replacement.Length));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Replace any assembler variable/constant references in a string with their currently stored value.
        /// </summary>
        private string ProcessAssemblerVariables(string text)
        {
            StringBuilder currentName = new();
            int currentStartIndex = 0;
            bool openBackslash = false;
            bool parsingName = false;

            if (text.TrimStart().StartsWith("%MACRO ", StringComparison.OrdinalIgnoreCase))
            {
                // Assembler variables in macro definitions should not be replaced
                return text;
            }

            for (int i = 0; i <= text.Length; i++)
            {
                // Terminate the string with a null character so that variables at the end of the line are still replaced
                char c = i < text.Length ? text[i] : '\0';

                if (parsingName)
                {
                    if (c is (< 'a' or > 'z') and (< 'A' or > 'Z') and (< '0' or > '9') and not '_'
                        && (currentStartIndex != i - 1 || c != '!'))
                    {
                        // Character not valid for an assembler variable, so mark the end of the current name and replace it
                        parsingName = false;

                        string value = GetAssemblerVariableValue(currentName.ToString());
                        text = text[..currentStartIndex] + value + text[i..];
                        // Adjust current string index based on newly replaced text
                        i = currentStartIndex - 1;

                        _ = currentName.Clear();
                        continue;
                    }
                    _ = currentName.Append(c);
                    continue;
                }

                if (!openBackslash)
                {
                    switch (c)
                    {
                        case '@':
                            currentStartIndex = i;
                            parsingName = true;
                            continue;
                        case '\\':
                            openBackslash = true;
                            continue;
                    }
                }

                if (c == '@')
                {
                    // Remove the backslash that escaped this '@' sign from the string
                    text = text[..(i - 1)] + text[i..];
                    i--;
                }

                openBackslash = false;
            }

            return text;
        }

        /// <summary>
        /// Get the string representation of an assembler variable/constant's current value.
        /// </summary>
        /// <param name="name">
        /// The name of the variable/constant.
        /// Should not include the '@' prefix, but should include the '!' prefix for constants.
        /// </param>
        /// <remarks>This method performs validation on the name.</remarks>
        private string GetAssemblerVariableValue(string name)
        {
            if (name.Length >= 1 && name[0] == '!')
            {
                // Assembler constant
                if (name.Length == 1)
                {
                    throw new SyntaxError(Strings.Assembler_Error_Constant_Empty_Name);
                }

                name = name[1..];
                if (assemblerConstants.TryGetValue(name, out Func<ulong>? valueGetter))
                {
                    return valueGetter().ToString();
                }

                throw new VariableNameException(string.Format(Strings.Assembler_Error_Constant_Not_Exists, name));
            }

            // Assembler variable
            if (name.Length == 0)
            {
                throw new SyntaxError(Strings.Assembler_Error_Variable_Empty_Name);
            }

            if (assemblerVariables.TryGetValue(name, out ulong value))
            {
                return value.ToString();
            }

            throw new VariableNameException(string.Format(Strings.Assembler_Error_Variable_Not_Exists, name));
        }

        private void HandleAssemblerException(AssemblerException e)
        {
            string rawLine = dynamicLines[lineIndex];

            e.WarningObject = new Warning(
                WarningSeverity.FatalError, 0000, currentFilePosition, "", Array.Empty<string>(),
                rawLine, currentMacro?.MacroName, e.Message);

            if (currentImport is null)
            {
                e.ConsoleMessage = string.Format(Strings.Assembler_Error_Message_Base_File, baseFileLine, rawLine, e.Message);
            }
            else
            {
                string newMessage = string.Format(Strings.Assembler_Error_Message_Imported, currentImport.CurrentLine, currentImport.ImportPath, rawLine);
                _ = importStack.Pop();  // Remove already printed frame from stack
                while (importStack.TryPop(out ImportStackFrame? nestedImport))
                {
                    newMessage += string.Format(Strings.Assembler_Error_Message_Imported_Import, nestedImport.CurrentLine, nestedImport.ImportPath);
                }
                newMessage += string.Format(Strings.Assembler_Error_Message_Imported_Base, baseFileLine, e.Message);
                e.ConsoleMessage = newMessage;
            }

            if (currentMacro is not null)
            {
                e.ConsoleMessage += string.Format(
                    Strings.Assembler_Error_Message_Macro_Stack, string.Join(" -> ", macroStack.Select(m => m.MacroName)));
            }
        }

        /// <summary>
        /// Determine if a given statement is a known state-modifying assembler directive and process it if it is.
        /// </summary>
        /// <param name="mnemonic">The mnemonic for the directive, including the % prefix</param>
        /// <param name="preVariableLine">The contents of the current line before assembler variables are expanded</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><see langword="true"/> - Directive was recognised and processed without error</item>
        /// <item><see langword="false"/> - Directive was not recognised and assembly of the statement should continue</item>
        /// </list>
        /// </returns>
        private bool ProcessStateDirective(string mnemonic, string[] operands, string preVariableLine)
        {
            if (stateDirectives.TryGetValue(mnemonic, out StateDirective? directiveFunc))
            {
                directiveFunc(mnemonic, operands, preVariableLine);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Skip assembly to the next instance of any of the given closing directives.
        /// </summary>
        /// <param name="directives">The case-insensitive closing directives to find.</param>
        /// <param name="parsedMatchedLine">The parsed line that was stopped on.</param>
        /// <param name="markProcessed">Whether the skipped lines should be considered processed (for linting).</param>
        /// <param name="reopeningDirectives">
        /// If any of these are encountered, it will increase the number of closing directive matches required to stop.
        /// Useful for nested block directives.
        /// </param>
        /// <param name="nestedClosingDirectives">
        /// The directives that can fully close a block opened by <paramref name="reopeningDirectives"/>.
        /// </param>
        /// <param name="parseLines">
        /// Whether each skipped line should be validated and parsed.
        /// <b>Must</b> be <see langword="true"/> for <paramref name="reopeningDirectives"/> and <paramref name="nestedClosingDirectives"/> to work.
        /// Lines will be cleaned of comments and trailing whitespace, blank lines will be removed,
        /// and any currently defined macros will be substituted and applied to the returned strings.
        /// <paramref name="parsedMatchedLine"/> will group all operands into the same string at index 1 with the mnemonic at index 0 if this is <see langword="false"/>.
        /// </param>
        /// <returns>An array of all the lines that were skipped</returns>
        /// <exception cref="EndingDirectiveException">There were no instances of the given closing directives found</exception>
        private string[] GoToNextClosingDirective(IEnumerable<string> directives, out string[] parsedMatchedLine, bool markProcessed,
            IEnumerable<string> reopeningDirectives, IEnumerable<string> nestedClosingDirectives, bool parseLines)
        {
            parsedMatchedLine = Array.Empty<string>();

            HashSet<string> closingTags = new(StringComparer.OrdinalIgnoreCase);
            closingTags.UnionWith(directives);

            HashSet<string> nestedOpeningTags = new(StringComparer.OrdinalIgnoreCase);
            nestedOpeningTags.UnionWith(reopeningDirectives);

            HashSet<string> nestedClosingTags = new(StringComparer.OrdinalIgnoreCase);
            nestedClosingTags.UnionWith(nestedClosingDirectives);

            AssemblyPosition startPosition = GetCurrentPosition();
            List<string> lines = new();
            bool foundEndTag = false;
            int nestedOpens = 0;

            while (IncrementCurrentLine())
            {
                if (markProcessed)
                {
                    _ = processedLines.Add(currentFilePosition);
                }
                string line = dynamicLines[lineIndex];
                if (parseLines)
                {
                    line = CleanLine(line);
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    if (ProcessLineMacros(ref line, dynamicLines, lineIndex))
                    {
                        continue;
                    }
                    parsedMatchedLine = ParseLine(line);
                    if (nestedOpeningTags.Contains(parsedMatchedLine[0]))
                    {
                        nestedOpens++;
                        continue;
                    }
                    if (closingTags.Contains(parsedMatchedLine[0]))
                    {
                        if (nestedOpens == 0)
                        {
                            foundEndTag = true;
                            // Even if intermediate lines aren't being marked as processed, the closing tag still should be
                            _ = processedLines.Add(currentFilePosition);
                            break;
                        }
                        if (nestedClosingTags.Contains(parsedMatchedLine[0]))
                        {
                            nestedOpens--;
                        }
                    }
                }
                else
                {
                    parsedMatchedLine = CleanLine(line).Split(' ', 2);
                    if (parsedMatchedLine.Length >= 1 && closingTags.Contains(parsedMatchedLine[0]))
                    {
                        foundEndTag = true;
                        // Even if intermediate lines aren't being marked as processed, the closing tag still should be
                        _ = processedLines.Add(currentFilePosition);
                        break;
                    }
                }
                lines.Add(line);
            }

            if (!foundEndTag)
            {
                // Rollback the state of the import stack to when loop started,
                // so that error message shows that line instead of the end of the file
                SetCurrentPosition(startPosition, false);
                throw new EndingDirectiveException(Strings.Assembler_Error_Closing_Directive_Missing);
            }

            return lines.ToArray();
        }

        private string[] GoToNextClosingDirective(string directive, bool markProcessed)
        {
            string[] lines = GoToNextClosingDirective(
                new List<string>() { directive }, out string[] parsedMatchedLine, markProcessed,
                Enumerable.Empty<string>(), Enumerable.Empty<string>(), false);
            if (parsedMatchedLine.Length != 1)
            {
                throw new OperandException(
                    string.Format(Strings.Assembler_Error_Closing_Directive_Operand_Count, parsedMatchedLine.Length - 1, parsedMatchedLine[0]));
            }
            return lines;
        }

        /// <summary>
        /// Evaluates a conditional expression defined by the given operands in the form "[OPERATION], [VALUE], [COMPARISON]".
        /// The [COMPARISON] operand must not be given for the DEF and NDEF operations.
        /// </summary>
        /// <exception cref="OperandException">The operands given were invalid.</exception>
        private bool RunConditionalCheck(string mnemonic, string[] operands)
        {
            if (operands.Length < 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_Conditional_Operand_Count, mnemonic, operands.Length));
            }

            string operation = operands[0];
            bool isDefinedCheck = operation is "DEF" or "NDEF";

            if ((isDefinedCheck && operands.Length != 2)
                || (!isDefinedCheck && operands.Length != 3))
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_Conditional_Operand_Count, mnemonic, operands.Length));
            }

            ulong value = 0;
            ulong comparison = 0;
            if (operands.Length == 3)
            {
                OperandType operandTypeSecond = DetermineOperandType(operands[1]);
                OperandType operandTypeThird = DetermineOperandType(operands[2]);
                if (operandTypeSecond != OperandType.Literal || operandTypeThird != OperandType.Literal)
                {
                    throw new OperandException(string.Format(Strings.Assembler_Error_Conditional_Operand_Second_Third_Type, mnemonic));
                }
                if (operands[1][0] == ':' || operands[2][0] == ':')
                {
                    throw new OperandException(string.Format(Strings.Assembler_Error_Conditional_Operand_Second_Third_Label_Reference, mnemonic));
                }
                _ = ParseLiteral(operands[1], false, out value);
                _ = ParseLiteral(operands[2], false, out comparison);
            }

            return operation.ToUpperInvariant() switch
            {
                "DEF" => assemblerVariables.ContainsKey(operands[1]),
                "NDEF" => !assemblerVariables.ContainsKey(operands[1]),
                "EQ" => value == comparison,
                "NEQ" => value != comparison,
                "GT" => value > comparison,
                "GTE" => value >= comparison,
                "LT" => value < comparison,
                "LTE" => value <= comparison,
                _ => throw new OperandException(string.Format(Strings.Assembler_Error_Conditional_Operand_First, mnemonic, operation)),
            };
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
            if (dataDirectives.TryGetValue(mnemonic, out DataDirective? directiveFunc))
            {
                newBytes = directiveFunc(operands, referencedLabels);
                return true;
            }
            newBytes = null;
            return false;
        }

        private static void ValidateNumericLiteral(string operand)
        {
            Match invalidMatch = operand.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? Regex.Match(operand, "[^0-9A-Fa-f_](?<!^0[xX])")  // Hex
                : operand.StartsWith("0b", StringComparison.OrdinalIgnoreCase)
                    ? Regex.Match(operand, "[^0-1_](?<!^0[bB])")  // Bin
                    : Regex.Match(operand, @"[^0-9_\.](?<!^-)");  // Dec
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
            if (operand.Equals("0x_", StringComparison.OrdinalIgnoreCase) || operand.Equals("0b_", StringComparison.OrdinalIgnoreCase))
            {
                throw new SyntaxError(Strings.Assembler_Error_Literal_Underscore_Only);
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
