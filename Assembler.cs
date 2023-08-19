using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace AssEmbly
{
    /// <summary>
    /// Assembles text based AssEmbly programs to compiled AssEmbly bytes.
    /// </summary>
    public static class Assembler
    {
        public class ImportStackFrame
        {
            public string ImportPath { get; set; }
            public int CurrentLine { get; set; }
            public int TotalLines { get; set; }

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
        /// <param name="disabledNonFatalErrors">A set of non-fatal error codes to disable.</param>
        /// <param name="disabledWarnings">A set of warning codes to disable.</param>
        /// <param name="disabledSuggestions">A set of suggestion codes to disable.</param>
        /// <param name="debugInfo">A string to store generated debug information file text in.</param>
        /// <param name="warnings">A list to store any generated warnings in.</param>
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
        public static byte[] AssembleLines(string[] lines,
            IReadOnlySet<int> disabledNonFatalErrors, IReadOnlySet<int> disabledWarnings, IReadOnlySet<int> disabledSuggestions,
            out string debugInfo, out List<Warning> warnings)
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

            // Used for debug files
            List<(ulong Address, string Line)> assembledLines = new();
            Dictionary<ulong, List<string>> addressLabelNames = new();
            List<(string LocalPath, string FullPath, ulong Address)> resolvedImports = new();

            // For detecting circular imports and tracking imported line numbers
            Stack<ImportStackFrame> importStack = new();
            int baseFileLine = 0;

            bool lineIsLabelled = false;
            AssemblerWarnings warningGenerator = new();
            warningGenerator.DisabledNonFatalErrors.UnionWith(disabledNonFatalErrors);
            warningGenerator.DisabledWarnings.UnionWith(disabledWarnings);
            warningGenerator.DisabledSuggestions.UnionWith(disabledSuggestions);
            warnings = new List<Warning>();

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
                            throw new SyntaxError($"Cannot convert a label definition to a literal address. Are you sure you meant to include the '&'?\n    {rawLine}\n     ^");
                        }
                        string labelName = mnemonic[1..];
                        if (labels.ContainsKey(labelName))
                        {
                            throw new LabelNameException($"Label \"{labelName}\" has already been defined. Label names must be unique.");
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
                                throw new OperandException($"The IMP mnemonic requires a single operand. {operands.Length} were given.");
                            }
                            Data.OperandType operandType = DetermineOperandType(operands[0]);
                            if (operandType != Data.OperandType.Literal)
                            {
                                throw new OperandException($"The operand to the IMP mnemonic must be a literal. An operand of type {operandType} was provided.");
                            }
                            if (operands[0][0] != '"')
                            {
                                throw new OperandException("The literal operand to the IMP mnemonic must be a string.");
                            }
                            byte[] parsedBytes = ParseLiteral(operands[0], true);
                            string filepath = Path.GetFullPath(Encoding.UTF8.GetString(parsedBytes));
                            if (!File.Exists(filepath))
                            {
                                throw new ImportException($"The file \"{filepath}\" given to the IMP mnemonic could not be found.");
                            }
                            if (importStack.Any(x => x.ImportPath.ToLower() == filepath.ToLower()))
                            {
                                throw new ImportException($"Circular import detected: attempted import from \"{filepath}\" when it is already in import stack.");
                            }
                            string[] linesToImport = File.ReadAllLines(filepath);
                            // Insert the contents of the imported file so they are assembled next
                            dynamicLines.InsertRange(l + 1, linesToImport);
                            resolvedImports.Add((filepath, filepath, (uint)program.Count));

                            warnings.AddRange(warningGenerator.NextInstruction(
                                Array.Empty<byte>(), mnemonic, operands,
                                currentImport is null ? baseFileLine : currentImport.CurrentLine,
                                currentImport?.ImportPath ?? string.Empty, lineIsLabelled, rawLine, importStack));
                            lineIsLabelled = false;

                            importStack.Push(new ImportStackFrame(filepath, 0, linesToImport.Length));
                            continue;
                        // Define macro
                        case "MAC":
                            if (operands.Length != 2)
                            {
                                throw new OperandException($"The MAC mnemonic requires two operands. {operands.Length} were given.");
                            }
                            macros[operands[0]] = operands[1];
                            lineIsLabelled = false;
                            continue;
                        // Toggle warnings
                        case "ANALYZER":
                            if (operands.Length != 3)
                            {
                                throw new OperandException($"The ANALYZER directive requires 3 operands. {operands.Length} were given.");
                            }
                            HashSet<int> disabledSet = operands[0].ToUpperInvariant() switch
                            {
                                "ERROR" => warningGenerator.DisabledNonFatalErrors,
                                "WARNING" => warningGenerator.DisabledWarnings,
                                "SUGGESTION" => warningGenerator.DisabledSuggestions,
                                _ => throw new OperandException("The first operand to the ANALYZER directive must be one of 'error', 'warning' or 'suggestion'.")
                            };
                            IReadOnlySet<int> initialSet = operands[0].ToUpperInvariant() switch
                            {
                                "ERROR" => disabledNonFatalErrors,
                                "WARNING" => disabledWarnings,
                                "SUGGESTION" => disabledSuggestions,
                                _ => throw new OperandException("The first operand to the ANALYZER directive must be one of 'error', 'warning' or 'suggestion'.")
                            };
                            if (!int.TryParse(operands[1], out int code))
                            {
                                throw new OperandException("The second operand to the ANALYZER directive must be an integer.");
                            }
                            switch (operands[2].ToUpperInvariant())
                            {
                                // Disable
                                case "0":
                                    _ = disabledSet.Add(code);
                                    break;
                                // Enable
                                case "1":
                                    _ = disabledSet.Remove(code);
                                    break;
                                // Restore
                                case "R":
                                    if (initialSet.Contains(code))
                                    {
                                        _ = disabledSet.Add(code);
                                    }
                                    else
                                    {
                                        _ = disabledSet.Remove(code);
                                    }
                                    break;
                                default:
                                    throw new OperandException("The third operand to the ANALYZER directive must be one of '0', '1', or 'r'.");
                            }
                            continue;
                        default:
                            break;
                    }
                    (byte[] newBytes, List<(string LabelName, ulong AddressOffset)> newLabels) = AssembleStatement(mnemonic, operands);
                    foreach ((string label, ulong relativeOffset) in newLabels)
                    {
                        labelReferences.Add((label, relativeOffset + (uint)program.Count, currentImport?.ImportPath,
                            currentImport is null ? baseFileLine : currentImport.CurrentLine));
                    }
                    assembledLines.Add(((uint)program.Count, rawLine));
                    program.AddRange(newBytes);
                    warnings.AddRange(warningGenerator.NextInstruction(
                        newBytes, mnemonic, operands,
                        currentImport is null ? baseFileLine : currentImport.CurrentLine,
                        currentImport?.ImportPath ?? string.Empty, lineIsLabelled, rawLine, importStack));
                    lineIsLabelled = false;
                }
                catch (AssemblerException e)
                {
                    if (currentImport is null)
                    {
                        e.ConsoleMessage = $"Error on line {baseFileLine} in base file\n    \"{rawLine}\"\n{e.Message}";
                        e.WarningObject = new Warning(WarningSeverity.FatalError, 0000, "", baseFileLine, "", Array.Empty<string>(), e.Message);
                    }
                    else
                    {
                        e.WarningObject = new Warning(WarningSeverity.FatalError, 0000,
                            currentImport.ImportPath, currentImport.CurrentLine, "", Array.Empty<string>(), e.Message);
                        string newMessage = $"Error on line {currentImport.CurrentLine} in \"{currentImport.ImportPath}\"\n    \"{rawLine}\"";
                        _ = importStack.Pop();  // Remove already printed frame from stack
                        while (importStack.TryPop(out ImportStackFrame? nestedImport))
                        {
                            newMessage += $"\nImported on line {nestedImport.CurrentLine} of \"{nestedImport.ImportPath}\"";
                        }
                        newMessage += $"\nImported on line {baseFileLine} of base file\n\n{e.Message}";
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
                        $"A label with the name \"{labelName}\" does not exist, but a reference was made to it. " +
                        $"Have you missed a definition?", line, filePath ?? "");
                    exc.ConsoleMessage = $"Error on line {line} in {filePath ?? "base file"}\n\n{exc.Message}";
                    throw exc;
                }
                // Write the now known address of the label to where it is required within the program
                BinaryPrimitives.WriteUInt64LittleEndian(programBytes.AsSpan()[(int)insertOffset..((int)insertOffset + 8)], targetOffset);
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
        /// <returns>The assembled bytes, along with a list of label names and the offset the addresses of the labels need to be inserted into.</returns>
        /// <exception cref="OperandException">Thrown when a mnemonic is given an invalid number or type of operands.</exception>
        /// <exception cref="OpcodeException">Thrown when a particular combination of mnemonic and operand types is not recognised.</exception>
        public static (byte[], List<(string LabelName, ulong AddressOffset)>) AssembleStatement(string mnemonic, string[] operands)
        {
            Data.OperandType[] operandTypes = new Data.OperandType[operands.Length];
            List<byte> operandBytes = new();
            List<(string LabelName, ulong AddressOffset)> labels = new();
            // Check for byte-inserting assembler directives
            switch (mnemonic.ToUpperInvariant())
            {
                // Single byte insertion
                case "DAT":
                    if (operands.Length != 1)
                    {
                        throw new OperandException($"The DAT mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    Data.OperandType operandType = DetermineOperandType(operands[0]);
                    if (operandType != Data.OperandType.Literal)
                    {
                        throw new OperandException($"The operand to the DAT mnemonic must be a literal. An operand of type {operandType} was provided.");
                    }
                    byte[] parsedBytes = ParseLiteral(operands[0], true);
                    return operands[0][0] != '"' && parsedBytes[1..].Any(b => b != 0)
                        ? throw new OperandException($"Numeric literal too large for DAT. 255 is the maximum value:\n    {operands[0]}")
                        : (operands[0][0] != '"' ? parsedBytes[0..1] : parsedBytes, new List<(string, ulong)>());
                // 0-padding
                case "PAD":
                    if (operands.Length != 1)
                    {
                        throw new OperandException($"The PAD mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    operandType = DetermineOperandType(operands[0]);
                    return operandType == Data.OperandType.Literal
                        // Generate an array of 0-bytes with the specified length
                        ? (Enumerable.Repeat((byte)0, (int)BinaryPrimitives.ReadUInt64LittleEndian(
                            ParseLiteral(operands[0], false))).ToArray(), new List<(string, ulong)>())
                        : throw new OperandException($"The operand to the PAD mnemonic must be a literal. " +
                            $"An operand of type {operandType} was provided.");
                // 64-bit number insertion
                case "NUM":
                    if (operands.Length != 1)
                    {
                        throw new OperandException($"The NUM mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != Data.OperandType.Literal)
                    {
                        throw new OperandException($"The operand to the NUM mnemonic must be a literal. An operand of type {operandType} was provided.");
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
                    case Data.OperandType.Register:
                        operandBytes.Add((byte)Enum.Parse<Data.Register>(operands[i].ToLowerInvariant()));
                        break;
                    case Data.OperandType.Literal:
                        if (operands[i].StartsWith(":&"))
                        {
                            labels.Add((operands[i][2..], (uint)operandBytes.Count + 1));
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
                    case Data.OperandType.Address:
                        labels.Add((operands[i][1..], (uint)operandBytes.Count + 1));
                        for (int j = 0; j < 8; j++)
                        {
                            // Label location will be resolved later, pad with 0s for now
                            operandBytes.Add(0);
                        }
                        break;
                    case Data.OperandType.Pointer:
                        // Convert register name to associated byte value
                        operandBytes.Add((byte)Enum.Parse<Data.Register>(operands[i][1..].ToLowerInvariant()));
                        break;
                    default: break;
                }
            }
            if (!Data.Mnemonics.TryGetValue((mnemonic.ToUpperInvariant(), operandTypes), out byte opcode))
            {
                throw new OpcodeException($"Unrecognised mnemonic and operand combination:\n    {mnemonic} {string.Join(", ", operandTypes)}" +
                    $"\nConsult the language reference for a list of all valid mnemonic/operand combinations.");
            }
            operandBytes.Insert(0, opcode);
            return (operandBytes.ToArray(), labels);
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
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == ';')
                {
                    // Comment found, ignore rest of line
                    break;
                }
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length != 0 && elements.Count == 0)
                    {
                        // End of mnemonic found, add line so far as first item
                        elements.Add(sb.ToString());
                        sb = new StringBuilder();
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
                            $"Mnemonics should be separated from operands with spaces, not commas:\n    {line}\n    {new string(' ', i)}^");
                    }
                    if (sb.Length == 0)
                    {
                        throw new SyntaxError($"Operands cannot be empty:\n    {line}\n    {new string(' ', i)}^");
                    }
                    // End of operand found, add stored characters as an element and move on
                    elements.Add(sb.ToString());
                    sb = new StringBuilder();
                    trailingWhitespace = -1;
                    stringEnd = -1;
                    continue;
                }
                if (trailingWhitespace != -1)
                {
                    throw new SyntaxError(
                        $"Operands cannot contain whitespace. Did you forget a comma?\n    {line}\n    {new string(' ', trailingWhitespace)}^");
                }
                if (stringEnd != -1)
                {
                    throw new SyntaxError(
                        $"Non-whitespace characters found after string literal. Did you forget a comma?\n    {line}\n    {new string(' ', i)}^");
                }
                if (c == '"')
                {
                    if (sb.Length != 0)
                    {
                        throw new SyntaxError(
                            $"String literal defined after non-whitespace characters. Did you forget a comma?:\n    {line}\n    {new string(' ', i)}^");
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
        /// Process a string literal as a part of an entire line of AssEmbly source.
        /// Resolves escape sequences and ensures that the string ends with an unescaped quote mark.
        /// </summary>
        /// <param name="line">The entire source line with the string contained.</param>
        /// <param name="startIndex">
        /// The index in the line to the opening quote.
        /// It will be incremented automatically by this method to the index of the closing quote.
        /// </param>
        /// <returns>The processed string literal, including opening and closing quotes.</returns>
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
            if (line[startIndex] != '"')
            {
                throw new ArgumentException("Given string start index does not point to a quote mark.");
            }
            StringBuilder sb = new("\"");
            int i = startIndex;
            while (true)
            {
                if (++i >= line.Length)
                {
                    throw new SyntaxError(
                        $"End of line found while processing string literal.\n    {line}\n    {new string(' ', i - 1)}^");
                }
                char c = line[i];
                if (c == '\\')
                {
                    char escape = line[++i];
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
                                throw new SyntaxError($"End of line reached when processing unicode escape\n    {line}\n    {new string(' ', i)}^");
                            }
                            string rawCodePoint = line[(i + 1)..(i + 5)];
                            try
                            {
                                escape = (char)Convert.ToUInt16(rawCodePoint, 16);
                            }
                            catch (FormatException)
                            {
                                throw new SyntaxError(
                                    $"Unicode escape must be immediately followed a 4 digit unicode codepoint\n    {line}\n    {new string(' ', i)}^");
                            }
                            i += 4;
                            break;
                        default:
                            throw new SyntaxError($"Unrecognised escape character '{escape}'\n    {line}\n    {new string(' ', i)}^");
                    }
                    _ = sb.Append(escape);
                    continue;
                }
                if (c == '"')
                {
                    break;
                }
                _ = sb.Append(c);
            }
            startIndex = i;
            _ = sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Determine the type for a single operand.
        /// </summary>
        /// <param name="operand">A single operand with no comments or whitespace.</param>
        /// <remarks>Operands will also be validated here.</remarks>
        /// <exception cref="SyntaxError">Thrown when an operand is badly formed.</exception>
        public static Data.OperandType DetermineOperandType(string operand)
        {
            if (operand[0] == ':')
            {
                int offset = operand[1] == '&' ? 2 : 1;
                Match invalidMatch = Regex.Match(operand[offset..], @"^[0-9]|[^A-Za-z0-9_]");
                // Operand is a label reference - will assemble down to address
                return invalidMatch.Success
                    ? throw new SyntaxError($"Invalid character in label:\n    {operand}\n    {new string(' ', invalidMatch.Index + offset)}^" +
                        $"\nLabel names may not contain symbols other than underscores, and cannot start with a numeral.")
                    : operand[1] == '&' ? Data.OperandType.Literal : Data.OperandType.Address;
            }
            else if (int.TryParse(operand[0..1], out _))
            {
                operand = operand.ToLowerInvariant();
                Match invalidMatch;
                invalidMatch = operand.StartsWith("0x") ? Regex.Match(operand, "[^0-9a-f_](?<!^0[xX])") : operand.StartsWith("0b")
                    ? Regex.Match(operand, "[^0-1_](?<!^0[bB])")
                    : Regex.Match(operand, "[^0-9_]");
                return invalidMatch.Success
                    ? throw new SyntaxError($"Invalid character in numeric literal:\n    {operand}\n    {new string(' ', invalidMatch.Index)}^" +
                        $"\nDid you forget a '0x' prefix before a hexadecimal number or put a digit other than 1 or 0 in a binary number?")
                    : Data.OperandType.Literal;
            }
            else if (operand[0] == '"')
            {
                return Data.OperandType.Literal;
            }
            else
            {
                int offset = operand[0] == '*' ? 1 : 0;
                return Enum.TryParse<Data.Register>(operand[offset..].ToLowerInvariant(), out _)
                    ? operand[0] == '*' ? Data.OperandType.Pointer : Data.OperandType.Register
                    : throw new SyntaxError($"Type of operand \"{operand}\" could not be determined. Did you forget a colon before a label name or misspell a register name?");
            }
        }

        /// <summary>
        /// Parse a operand of literal type to its representation as bytes. 
        /// </summary>
        /// <remarks>Strings and integer size constraints will be validated here, all other validation should be done as a part of <see cref="DetermineOperandType"/></remarks>
        /// <returns>The bytes representing the literal to be added to a program.</returns>
        /// <exception cref="SyntaxError">Thrown when there are invalid characters in a string literal or the string literal is in an invalid format.</exception>
        /// <exception cref="OperandException">Thrown when the literal is too large for a single <see cref="ulong"/>.</exception>
        /// <exception cref="FormatException">Thrown when there are invalid characters in a numeric literal or the numeric literal is in an invalid format.</exception>
        public static byte[] ParseLiteral(string operand, bool allowString, out ulong parsedNumber)
        {
            parsedNumber = 0;
            if (operand[0] == '"')
            {
                if (!allowString)
                {
                    throw new SyntaxError("A string literal is not a valid operand in this context.");
                }
                if (Regex.Matches(operand, @"(?<!\\)""").Count > 2)
                {
                    throw new SyntaxError("An operand can only contain a single string literal. Did you forget to escape a quote mark?");
                }
                if (operand[^1] != '"')
                {
                    throw new SyntaxError("String literal contains characters after closing quote mark.");
                }
                string str = operand.Trim('"').Replace("\\\"", "\"");
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
                        : Convert.ToUInt64(operand);
            }
            catch (OverflowException)
            {
                throw new OperandException($"Numeric literal too large. 18,446,744,073,709,551,615 is the maximum value:\n    {operand}");
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
