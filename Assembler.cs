using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    /// <summary>
    /// Assembles text based AssEmbly programs to compiled AssEmbly bytes.
    /// </summary>
    public static class Assembler
    {
        public class ImportStackFrame
        {
            public string ImportPath { get; }
            public int CurrentLine { get; set; }
            public int TotalLines { get; }

            public ImportStackFrame(string importPath, int currentLine, int totalLines)
            {
                ImportPath = importPath;
                CurrentLine = currentLine;
                TotalLines = totalLines;
            }
        }

        /// <summary>
        /// Assemble multiple AssEmbly lines at once into executable bytecode.
        /// </summary>
        /// <param name="lines">Each line of the program to assemble. Newline characters should not be included.</param>
        /// <param name="usingV1Format">
        /// Whether or not a v1 executable will be generated from this assembly.
        /// Used only for warning generation. Does not affect the resulting bytes.
        /// </param>
        /// <param name="disabledNonFatalErrors">A set of non-fatal error codes to disable.</param>
        /// <param name="disabledWarnings">A set of warning codes to disable.</param>
        /// <param name="disabledSuggestions">A set of suggestion codes to disable.</param>
        /// <param name="debugInfo">A string to store generated debug information file text in.</param>
        /// <param name="warnings">A list to store any generated warnings in.</param>
        /// <param name="entryPoint">A ulong to store the entry point of the program in.</param>
        /// <param name="usedExtensions">The feature flags for all extension sets used in the program.</param>
        /// <returns>The fully assembled bytecode containing instructions and data.</returns>
        /// <exception cref="SyntaxError">Thrown when there is an error with how a line of AssEmbly has been written.</exception>
        /// <exception cref="LabelNameException">
        /// Thrown when the same label name is defined multiple times, or when a reference is made to a non-existent label.
        /// </exception>
        /// <exception cref="OperandException">Thrown when a mnemonic is given an invalid number or type of operands.</exception>
        /// <exception cref="ImportException">
        /// Thrown when an error occurs whilst attempting to import the contents of another AssEmbly file.
        /// </exception>
        /// <exception cref="OpcodeException">Thrown when a particular combination of mnemonic and operand types is not recognised.</exception>
        public static byte[] AssembleLines(IEnumerable<string> lines, bool usingV1Format,
            IReadOnlySet<int> disabledNonFatalErrors, IReadOnlySet<int> disabledWarnings, IReadOnlySet<int> disabledSuggestions,
            out string debugInfo, out List<Warning> warnings, out ulong entryPoint, out AAPFeatures usedExtensions)
        {
            // The lines to assemble may change during assembly, for example importing a file
            // will extend the list of lines to assemble as and when the import is reached.
            List<string> dynamicLines = lines.ToList();
            // Final compiled byte list
            List<byte> program = new();
            // Map of label names to final memory addresses
            Dictionary<string, ulong> labels = new();
            // List of references to labels by name along with the address to insert the relevant address in to.
            // Also has the line and path to the file (if imported) that the reference was assembled from for use in error messages.
            List<(string LabelName, ulong Address, string? FilePath, int Line)> labelReferences = new();
            // string -> replacement
            Dictionary<string, string> macros = new();
            usedExtensions = AAPFeatures.None;

            // Used for debug files
            List<(ulong Address, string Line)> assembledLines = new();
            Dictionary<ulong, List<string>> addressLabelNames = new();
            List<(string LocalPath, string FullPath, ulong Address)> resolvedImports = new();

            // For detecting circular imports and tracking imported line numbers
            Stack<ImportStackFrame> importStack = new();
            int baseFileLine = 0;

            bool lineIsLabelled = false;
            bool lineIsEntry = false;
            AssemblerWarnings warningGenerator = new(usingV1Format);
            warningGenerator.DisabledNonFatalErrors.UnionWith(disabledNonFatalErrors);
            warningGenerator.DisabledWarnings.UnionWith(disabledWarnings);
            warningGenerator.DisabledSuggestions.UnionWith(disabledSuggestions);
            warnings = new List<Warning>();

            entryPoint = 0;

            for (int l = 0; l < dynamicLines.Count; l++)
            {
                if (importStack.TryPeek(out ImportStackFrame? currentImport))
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
                string rawLine = dynamicLines[l];
                foreach ((string macro, string replacement) in macros)
                {
                    rawLine = rawLine.Replace(macro, replacement);
                }
                try
                {
                    string[] line = ParseLine(rawLine);
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    string mnemonic = line[0];
                    // Lines starting with ':' are label definitions
                    if (mnemonic.StartsWith(':'))
                    {
                        // Will throw an error if label is not valid
                        _ = DetermineOperandType(mnemonic);
                        if (mnemonic[1] == '&')
                        {
                            throw new SyntaxError(string.Format(Strings.Assembler_Error_Invalid_Literal_Label, rawLine));
                        }
                        string labelName = mnemonic[1..];
                        if (labels.ContainsKey(labelName))
                        {
                            throw new LabelNameException(string.Format(Strings.Assembler_Error_Label_Already_Defined, labelName));
                        }
                        if (labelName.ToUpperInvariant() == "ENTRY")
                        {
                            entryPoint = (uint)program.Count;
                            lineIsEntry = true;
                        }
                        labels[labelName] = (uint)program.Count;

                        if (!addressLabelNames.ContainsKey((uint)program.Count))
                        {
                            addressLabelNames[(uint)program.Count] = new List<string>();
                        }
                        addressLabelNames[(uint)program.Count].Add(labelName);

                        lineIsLabelled = true;
                        continue;
                    }
                    string[] operands = line[1..];
                    // Check for line-modifying/state altering assembler directives
                    switch (mnemonic.ToUpperInvariant())
                    {
                        // Import contents of another file
                        case "IMP":
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
                            string filepath = Path.GetFullPath(Encoding.UTF8.GetString(parsedBytes));
                            if (!File.Exists(filepath))
                            {
                                throw new ImportException(string.Format(Strings.Assembler_Error_IMP_File_Not_Exists, filepath));
                            }
                            if (importStack.Any(x => string.Equals(x.ImportPath, filepath, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                throw new ImportException(string.Format(Strings.Assembler_Error_Circular_Import, filepath));
                            }
                            string[] linesToImport = File.ReadAllLines(filepath);
                            // Insert the contents of the imported file so they are assembled next
                            dynamicLines.InsertRange(l + 1, linesToImport);
                            resolvedImports.Add((filepath, filepath, (uint)program.Count));

                            warnings.AddRange(warningGenerator.NextInstruction(
                                Array.Empty<byte>(), mnemonic, operands,
                                currentImport?.CurrentLine ?? baseFileLine,
                                currentImport?.ImportPath ?? string.Empty, lineIsLabelled, lineIsEntry, rawLine, importStack));
                            lineIsLabelled = false;
                            lineIsEntry = false;

                            importStack.Push(new ImportStackFrame(filepath, 0, linesToImport.Length));
                            continue;
                        // Define macro
                        case "MAC":
                            if (operands.Length != 2)
                            {
                                throw new OperandException(string.Format(Strings.Assembler_Error_MAC_Operand_Count, operands.Length));
                            }
                            macros[operands[0]] = operands[1];
                            lineIsLabelled = false;
                            lineIsEntry = false;
                            continue;
                        // Toggle warnings
                        case "ANALYZER":
                            if (operands.Length != 3)
                            {
                                throw new OperandException(string.Format(Strings.Assembler_Error_ANALYZER_Operand_Count, operands.Length));
                            }
                            HashSet<int> disabledSet = operands[0].ToUpperInvariant() switch
                            {
                                "ERROR" => warningGenerator.DisabledNonFatalErrors,
                                "WARNING" => warningGenerator.DisabledWarnings,
                                "SUGGESTION" => warningGenerator.DisabledSuggestions,
                                _ => throw new OperandException(Strings.Assembler_Error_ANALYZER_Operand_First)
                            };
                            IReadOnlySet<int> initialSet = operands[0].ToUpperInvariant() switch
                            {
                                "ERROR" => disabledNonFatalErrors,
                                "WARNING" => disabledWarnings,
                                "SUGGESTION" => disabledSuggestions,
                                _ => throw new OperandException(Strings.Assembler_Error_ANALYZER_Operand_First)
                            };
                            if (!int.TryParse(operands[1], out int code))
                            {
                                throw new OperandException(Strings.Assembler_Error_ANALYZER_Operand_Second);
                            }
                            _ = operands[2].ToUpperInvariant() switch
                            {
                                // Disable
                                "0" => disabledSet.Add(code),
                                // Enable
                                "1" => disabledSet.Remove(code),
                                // Restore
                                "R" => initialSet.Contains(code) ? disabledSet.Add(code) : disabledSet.Remove(code),
                                _ => throw new OperandException(
                                    Strings.Assembler_Error_ANALYZER_Operand_Third)
                            };
                            continue;
                        default:
                            break;
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

            byte[] programBytes = program.ToArray();

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
                BinaryPrimitives.WriteUInt64LittleEndian(programBytes.AsSpan()[(int)insertOffset..], targetOffset);
            }
            warnings.AddRange(warningGenerator.Finalize(programBytes));

            debugInfo = DebugInfo.GenerateDebugInfoFile((uint)program.Count, assembledLines,
                // Convert dictionary to sorted list
                addressLabelNames.Select(x => (x.Key, x.Value)).OrderBy(x => x.Key).ToList(),
                resolvedImports);

            return programBytes;
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
            List<(string LabelName, ulong AddressOffset)> labels = new();
            usedExtensions = AAPFeatures.None;
            // Check for byte-inserting assembler directives
            switch (mnemonic.ToUpperInvariant())
            {
                // Single byte/string insertion
                case "DAT":
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
                    return (operands[0][0] != '"' ? parsedBytes[..1] : parsedBytes, new List<(string, ulong)>());
                // 0-padding
                case "PAD":
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
                    return (Enumerable.Repeat((byte)0, (int)parsedNumber).ToArray(), new List<(string, ulong)>());
                // 64-bit/floating point number insertion
                case "NUM":
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
                        throw new OperandException(Strings.Assembler_Error_NUM_Operand_Label_Reference);
                    }
                    parsedBytes = ParseLiteral(operands[0], false);
                    return (parsedBytes, new List<(string, ulong)>());
                default:
                    break;
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
                            labels.Add((operands[i][2..], (uint)operandBytes.Count));
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
                        labels.Add((operands[i][1..], (uint)operandBytes.Count));
                        for (int j = 0; j < 8; j++)
                        {
                            // Label location will be resolved later, pad with 0s for now
                            operandBytes.Add(0);
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
                operandBytes.Insert(0, 0xFF);
            }
            if (Data.ExtensionSetFeatureFlags.TryGetValue(opcode.ExtensionSet, out AAPFeatures newFlag))
            {
                usedExtensions |= newFlag;
            }

            // Add length of opcode to all label address offsets
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i] = (labels[i].LabelName, labels[i].AddressOffset + opcodeSize);
            }

            return (operandBytes.ToArray(), labels);
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
                        isMacro = mnemonic.ToUpperInvariant() == "MAC";
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
        /// For double quoted (") string literals, the processed string literal, including opening and closing quotes.
        /// For single quoted (') character literals, the 32-bit numeric value corresponding to the
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
                throw new IndexOutOfRangeException("String start index is outside the range of the given line.");
            }
            if (line.Length < 2)
            {
                throw new ArgumentException("Given line is less than two characters long, which is invalid.");
            }
            bool singleCharacterLiteral = false;
            bool containsHighSurrogate = false;  // Only used for character literals
            if (line[startIndex] != '"')
            {
                if (line[startIndex] != '\'')
                {
                    throw new ArgumentException("Given string start index does not point to a quote mark.");
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
                                _ = sb.Append(char.ConvertFromUtf32(Convert.ToInt32(rawCodePoint, 16)));
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
        /// <param name="operand">A single operand with no comments or whitespace.</param>
        /// <remarks>Operands will also be validated here.</remarks>
        /// <exception cref="SyntaxError">Thrown when an operand is badly formed.</exception>
        public static OperandType DetermineOperandType(string operand)
        {
            switch (operand[0])
            {
                case ':':
                {
                    int offset = operand[1] == '&' ? 2 : 1;
                    Match invalidMatch = Regex.Match(operand[offset..], "^[0-9]|[^A-Za-z0-9_]");
                    // Operand is a label reference - will assemble down to address
                    return invalidMatch.Success
                        ? throw new SyntaxError(string.Format(Strings.Assembler_Error_Label_Invalid_Character, operand, new string(' ', invalidMatch.Index + offset)))
                        : operand[1] == '&' ? OperandType.Literal : OperandType.Address;
                }
                case (>= '0' and <= '9') or '-' or '.':
                {
                    operand = operand.ToLowerInvariant();
                    Match invalidMatch = operand.StartsWith("0x") ? Regex.Match(operand, "[^0-9a-f_](?<!^0[xX])") : operand.StartsWith("0b")
                        ? Regex.Match(operand, "[^0-1_](?<!^0[bB])")
                        : Regex.Match(operand, @"[^0-9_\.](?<!^-)");
                    if (invalidMatch.Success)
                    {
                        throw new SyntaxError(string.Format(Strings.Assembler_Error_Numeric_Invalid_Character, operand, new string(' ', invalidMatch.Index)));
                    }
                    if (operand[0] == '.' && operand.Length == 1)
                    {
                        throw new SyntaxError(Strings.Assembler_Error_Floating_Point_Decimal_Only);
                    }
                    if (operand.IndexOf('.') != operand.LastIndexOf('.'))
                    {
                        throw new SyntaxError(string.Format(Strings.Assembler_Error_Numeric_Too_Many_Points, operand, new string(' ', operand.LastIndexOf('.'))));
                    }
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
        /// Parse a operand of literal type to its representation as bytes. 
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
                    ? $"Numeric literal too small. {long.MinValue:N0} is the minimum value:\n    {operand}"
                    : $"Numeric literal too large. {ulong.MaxValue:N0} is the maximum value:\n    {operand}");
            }
            byte[] result = new byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(result, parsedNumber);
            return result;
        }

        /// <summary>
        /// Parse a operand of literal type to its representation as bytes. 
        /// </summary>
        /// <remarks>Strings and integer size constraints will be validated here, all other validation should be done as a part of <see cref="DetermineOperandType"/></remarks>
        /// <returns>The bytes representing the literal to be added to a program.</returns>
        /// <exception cref="SyntaxError">Thrown when there are invalid characters in the literal or the literal is in an invalid format.</exception>
        /// <exception cref="OperandException">Thrown when the literal is too large for a single <see cref="ulong"/>.</exception>
        public static byte[] ParseLiteral(string operand, bool allowString)
        {
            return ParseLiteral(operand, allowString, out _);
        }
    }
}
