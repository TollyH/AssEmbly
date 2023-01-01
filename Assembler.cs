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
        /// <summary>
        /// Assemble multiple AssEmbly lines at once into executable bytecode.
        /// </summary>
        /// <param name="lines">Each line of the program to assemble. Newline characters should not be included.</param>
        /// <returns>The fully assembled bytecode containing instructions and data.</returns>
        public static byte[] AssembleLines(string[] lines)
        {
            List<string> dynamicLines = lines.ToList();
            List<byte> program = new();
            Dictionary<string, ulong> labels = new();
            List<(string, ulong)> labelReferences = new();
            Dictionary<string, string> macros = new();
            for (int l = 0; l < dynamicLines.Count; l++)
            {
                string line = StripComments(dynamicLines[l]);
                foreach ((string macro, string replacement) in macros)
                {
                    line = line.Replace(macro, replacement);
                }
                if (line == "")
                {
                    continue;
                }
                try
                {
                    if (line.StartsWith(':'))
                    {
                        // Will throw an error if label is not valid
                        _ = DetermineOperandType(line);
                        if (line[1] == '&')
                        {
                            throw new FormatException($"Cannot convert a label definition to a literal address. Are you sure you meant to include the '&'?\n    {line}\n     ^");
                        }
                        if (labels.ContainsKey(line[1..]))
                        {
                            throw new FormatException($"Label \"{line[1..]}\" has already been defined. Label names must be unique.");
                        }
                        labels[line[1..]] = (uint)program.Count;
                        continue;
                    }
                    Match unclosedQuote = Regex.Match(line, @"(?<=^(?>(?:[^""]*""){2})*[^""]*)""(?=[^""]*?$)");
                    if (unclosedQuote.Success)
                    {
                        throw new FormatException($"Statement contains an unclosed quote mark:\n    {line}\n    {new string(' ', unclosedQuote.Index)}^");
                    }
                    string[] split = line.Split(' ', 2);
                    string mnemonic = split[0];
                    // Regex splits on commas surrounded by any amount of whitespace (not including newlines), unless wrapped in unescaped quotes.
                    string[] operands = split.Length == 2 ? Regex.Split(
                        split[1].Trim(), @"[^\S\r\n]*,[^\S\r\n]*(?=(?:[^""\\]*(?:\\.|""([^""\\]*\\.)*[^""\\]*""))*[^""]*$)") : Array.Empty<string>();
                    // Check for line-modifying assembler directives
                    switch (mnemonic.ToUpperInvariant())
                    {
                        // Import contents of another file
                        case "IMP":
                            if (operands.Length != 1)
                            {
                                throw new FormatException($"The IMP mnemonic requires a single operand. {operands.Length} were given.");
                            }
                            Data.OperandType operandType = DetermineOperandType(operands[0]);
                            if (operandType != Data.OperandType.Literal)
                            {
                                throw new FormatException($"The operand to the IMP mnemonic must be a literal. An operand of type {operandType} was provided.");
                            }
                            if (operands[0][0] != '"')
                            {
                                throw new FormatException("The literal operand to the IMP mnemonic must be a string.");
                            }
                            byte[] parsedBytes = ParseLiteral(operands[0], true);
                            string filepath = Encoding.UTF8.GetString(parsedBytes);
                            if (!File.Exists(filepath))
                            {
                                throw new FileNotFoundException($"The file \"{filepath}\" given to the IMP mnemonic could not be found.");
                            }
                            dynamicLines.InsertRange(l + 1, File.ReadAllLines(filepath));
                            continue;
                        // Define macro
                        case "MAC":
                            if (operands.Length != 2)
                            {
                                throw new FormatException($"The MAC mnemonic requires two operands. {operands.Length} were given.");
                            }
                            macros[operands[0]] = operands[1];
                            continue;
                        default:
                            break;
                    }
                    (byte[] newBytes, List<(string, ulong)> newLabels) = AssembleStatement(mnemonic, operands);
                    foreach ((string label, ulong relativeOffset) in newLabels)
                    {
                        labelReferences.Add((label, relativeOffset + (uint)program.Count));
                    }
                    program.AddRange(newBytes);
                }
                catch (Exception e)
                {
                    e.Data["UserMessage"] = $"Error on line {l + 1}\n    \"{line}\"\n{e.Message}";
                    throw;
                }
            }

            byte[] programBytes = program.ToArray();

            foreach ((string labelName, ulong insertOffset) in labelReferences)
            {
                if (!labels.TryGetValue(labelName, out ulong targetOffset))
                {
                    FormatException e = new();
                    e.Data["UserMessage"] = $"A label with the name {labelName} does not exist, but a reference was made to it. " +
                        $"Have you missed a definition?";
                    throw e;
                }
                BinaryPrimitives.WriteUInt64LittleEndian(programBytes.AsSpan()[(int)insertOffset..((int)insertOffset + 8)], targetOffset);
            }

            return programBytes;
        }

        /// <summary>
        /// Assemble a single line of AssEmbly to bytecode.
        /// </summary>
        /// <returns>The assembled bytes, along with a list of label names and the offset the addresses of the labels need to be inserted into.</returns>
        public static (byte[], List<(string, ulong)>) AssembleStatement(string mnemonic, string[] operands)
        {
            Data.OperandType[] operandTypes = new Data.OperandType[operands.Length];
            List<byte> operandBytes = new();
            List<(string, ulong)> labels = new();
            // Check for byte-inserting assembler directives
            switch (mnemonic.ToUpperInvariant())
            {
                // Single byte insertion
                case "DAT":
                    if (operands.Length != 1)
                    {
                        throw new FormatException($"The DAT mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    Data.OperandType operandType = DetermineOperandType(operands[0]);
                    if (operandType != Data.OperandType.Literal)
                    {
                        throw new FormatException($"The operand to the DAT mnemonic must be a literal. An operand of type {operandType} was provided.");
                    }
                    byte[] parsedBytes = ParseLiteral(operands[0], true);
                    return operands[0][0] != '"' && parsedBytes[1..].Any(b => b != 0)
                        ? throw new FormatException($"Numeric literal too large for DAT. 255 is the maximum value:\n    {operands[0]}")
                        : (operands[0][0] != '"' ? parsedBytes[0..1] : parsedBytes, new List<(string, ulong)>());
                // 0-padding
                case "PAD":
                    if (operands.Length != 1)
                    {
                        throw new FormatException($"The PAD mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    operandType = DetermineOperandType(operands[0]);
                    return operandType == Data.OperandType.Literal
                        ? (Enumerable.Repeat((byte)0, (int)BinaryPrimitives.ReadUInt64LittleEndian(
                            ParseLiteral(operands[0], false))).ToArray(), new List<(string, ulong)>())
                        : throw new FormatException($"The operand to the PAD mnemonic must be a literal. " +
                            $"An operand of type {operandType} was provided.");
                // 64-bit number insertion
                case "NUM":
                    if (operands.Length != 1)
                    {
                        throw new FormatException($"The NUM mnemonic requires a single operand. {operands.Length} were given.");
                    }
                    operandType = DetermineOperandType(operands[0]);
                    if (operandType != Data.OperandType.Literal)
                    {
                        throw new FormatException($"The operand to the NUM mnemonic must be a literal. An operand of type {operandType} was provided.");
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
                        operandBytes.Add((byte)Enum.Parse<Data.Register>(operands[i][1..].ToLowerInvariant()));
                        break;
                    default: break;
                }
            }
            if (!Data.Mnemonics.TryGetValue((mnemonic.ToUpperInvariant(), operandTypes), out byte opcode))
            {
                throw new FormatException($"Unrecognised mnemonic and operand combination:\n    {mnemonic} {string.Join(", ", operandTypes)}" +
                    $"\nConsult the language reference for a list of all valid mnemonic/operand combinations.");
            }
            operandBytes.Insert(0, opcode);
            return (operandBytes.ToArray(), labels);
        }

        /// <summary>
        /// Strip comments and surrounding whitespace from a line.
        /// </summary>
        public static string StripComments(string statement)
        {
            // Regex splits on semicolons, unless wrapped in unescaped quotes.
            return Regex.Split(statement, @";(?=(?:[^""\\]*(?:\\.|""([^""\\]*\\.)*[^""\\]*""))*[^""]*$)")[0].Trim();
        }

        /// <summary>
        /// Determine the type for a single operand.
        /// </summary>
        /// <param name="operand">A single operand with no comments or whitespace.</param>
        /// <remarks>Operands will also be validated here.</remarks>
        /// <exception cref="FormatException">Operand was not valid for the type it appeared to be, or type could not be determined.</exception>
        public static Data.OperandType DetermineOperandType(string operand)
        {
            if (operand[0] == ':')
            {
                int offset = operand[1] == '&' ? 2 : 1;
                Match invalidMatch = Regex.Match(operand[offset..], @"^[0-9]|[^A-Za-z0-9_]");
                // Operand is a label reference - will assemble down to address
                return invalidMatch.Success
                    ? throw new FormatException($"Invalid character in label:\n    {operand}\n    {new string(' ', invalidMatch.Index + offset)}^" +
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
                    ? throw new FormatException($"Invalid character in numeric literal:\n    {operand}\n    {new string(' ', invalidMatch.Index)}^" +
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
                    : throw new FormatException($"Type of operand \"{operand}\" could not be determined. Did you forget a colon before a label name or misspell a register name?");
            }
        }

        /// <summary>
        /// Parse a operand of literal type to its representation as bytes. 
        /// </summary>
        /// <remarks>Strings and integer size constraints will be validated here, all other validation should be done as a part of <see cref="DetermineOperandType"/></remarks>
        /// <returns>The bytes representing the literal to be added to a program.</returns>
        /// <exception cref="FormatException">String literal is invalid, or numeric literal was too large for UInt64.</exception>
        public static byte[] ParseLiteral(string operand, bool allowString)
        {
            if (operand[0] == '"')
            {
                if (!allowString)
                {
                    throw new FormatException("A string literal is not a valid operand in this context.");
                }
                if (Regex.Matches(operand, @"(?<!\\)""").Count > 2)
                {
                    throw new FormatException("An operand can only contain a single string literal. Did you forget to escape a quote mark?");
                }
                if (operand[^1] != '"')
                {
                    throw new FormatException("String literal contains characters after closing quote mark.");
                }
                string str = operand.Trim('"').Replace("\\\"", "\"");
                return Encoding.UTF8.GetBytes(str);
            }
            operand = operand.ToLowerInvariant().Replace("_", "");
            ulong number;
            try
            {
                number = operand.StartsWith("0x")
                    ? Convert.ToUInt64(operand[2..], 16)
                    : operand.StartsWith("0b")
                        ? Convert.ToUInt64(operand[2..], 2)
                        : Convert.ToUInt64(operand);
            }
            catch (OverflowException)
            {
                throw new FormatException($"Numeric literal too large. 18,446,744,073,709,551,615 is the maximum value:\n    {operand}");
            }
            byte[] result = new byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(result, number);
            return result;
        }
    }
}
