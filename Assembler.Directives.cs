﻿using AssEmbly.Resources.Localization;
using System.Text;

namespace AssEmbly
{
    public partial class Assembler
    {
        private delegate void StateDirective(string mnemonic, string[] operands);
        private delegate byte[] DataDirective(string[] operands, List<(string, ulong)> referencedLabels);

        private readonly Dictionary<string, StateDirective> stateDirectives;

        private static readonly Dictionary<string, DataDirective> dataDirectives = new(StringComparer.OrdinalIgnoreCase)
        {
            { "%DAT", DataDirective_ByteAndStringInsertion },
            { "%PAD", DataDirective_ZeroPadding },
            { "%NUM", DataDirective_NumberInsertion },
            { "%IBF", DataDirective_RawFileInsertion },
        };

        // Used to keep the dictionary definition within this file out of the constructor whilst keeping stateDirectives readonly
        private void InitializeStateDirectives(out Dictionary<string, StateDirective> dictionary)
        {
            dictionary = new Dictionary<string, StateDirective>(StringComparer.OrdinalIgnoreCase)
            {
                { "%IMP", StateDirective_ImportSourceFile },
                { "%MACRO", StateDirective_DefineMacro },
                { "%DELMACRO", StateDirective_RemoveMacro },
                { "%LABEL_OVERRIDE", StateDirective_ManualLabelDefine },
                { "%ANALYZER", StateDirective_SetAnalyzerState },
                { "%MESSAGE", StateDirective_EmitAssemblerMessage },
                { "%STOP", StateDirective_StopAssembly },
                { "%REPEAT", StateDirective_RepeatSourceLines },
                { "%ENDREPEAT", StateDirective_EndLineRepeat },
                { "%ASM_ONCE", StateDirective_SingleAssemblyGuard },
                { "%DEFINE", StateDirective_DefineAssemblerVariable },
                { "%UNDEFINE", StateDirective_RemoveAssemblerVariable },
                { "%DEBUG", StateDirective_PrintAssemblerState },
                { "%ENDMACRO", StateDirective_DanglingClosingDirective },
            };
        }

        // STATE DIRECTIVES

        private void StateDirective_ImportSourceFile(string mnemonic, string[] operands)
        {
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
            if (importStack.Any(x => string.Equals(x.ImportPath, resolvedPath, StringComparison.OrdinalIgnoreCase))
                // If a file is entirely guarded by %ASM_ONCE, we don't need to error if it's already present on import stack.
                && !completeAsmOnceFiles.Any(s => string.Equals(s, resolvedPath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ImportException(string.Format(Strings.Assembler_Error_Circular_Import, resolvedPath));
            }
            string[] linesToImport = File.ReadAllLines(resolvedPath);
            // Insert the contents of the imported file so they are assembled next
            dynamicLines.InsertRange(lineIndex + 1, linesToImport);
            resolvedImports.Add((importPath, resolvedPath, (uint)program.Count));

            if (!timesSeenFile.TryAdd(resolvedPath, 1))
            {
                timesSeenFile[resolvedPath]++;
            }

            importStack.Push(new ImportStackFrame(resolvedPath, 0, linesToImport.Length));
        }

        private void StateDirective_DefineMacro(string mnemonic, string[] operands)
        {
            if (operands.Length is < 1 or > 2)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_MACRO_Operand_Count, operands.Length));
            }

            string newMacroName = operands[0];
            int index;
            if ((index = newMacroName.IndexOf('(')) != -1)
            {
                throw new SyntaxError(string.Format(Strings.Assembler_Error_Macro_Name_Brackets, newMacroName, new string(' ', index)));
            }
            if ((index = newMacroName.IndexOf(')')) != -1)
            {
                throw new SyntaxError(string.Format(Strings.Assembler_Error_Macro_Name_Brackets, newMacroName, new string(' ', index)));
            }

            if (operands.Length == 2)
            {
                // Single-line macro
                singleLineMacros[newMacroName] = operands[1];
                singleLineMacroNames.Add(newMacroName);
                singleLineMacroNames = singleLineMacroNames.OrderByDescending(n => n.Length).ToList();
            }
            else if (operands.Length == 1)
            {
                // Multi-line macro (must be terminated with %ENDMACRO)
                AssemblyPosition startPosition = GetCurrentPosition();
                List<string> replacement = new();
                bool foundEndTag = false;
                // Add each line before the next encountered %ENDMACRO directive to the replacement text
                while (IncrementCurrentLine())
                {
                    string line = dynamicLines[lineIndex];
                    if (line.TrimStart().StartsWith("%ENDMACRO", StringComparison.OrdinalIgnoreCase))
                    {
                        // Parse the line to check it wasn't given with any operands
                        string[] parsedLine = ParseLine(line);
                        if (parsedLine.Length != 1)
                        {
                            throw new OperandException(string.Format(Strings.Assembler_Error_ENDMACRO_Operand_Count, parsedLine.Length - 1));
                        }
                        foundEndTag = true;
                        break;
                    }
                    replacement.Add(line);
                }
                if (!foundEndTag)
                {
                    // Rollback the state of the import stack to when macro definition started,
                    // so that error message shows that line instead of the end of the file
                    SetCurrentPosition(startPosition);
                    throw new EndingDirectiveException(Strings.Assembler_Error_ENDMACRO_Missing);
                }
                multiLineMacros[newMacroName] = replacement.ToArray();
                multiLineMacroNames.Add(newMacroName);
                multiLineMacroNames = multiLineMacroNames.OrderByDescending(n => n.Length).ToList();
            }
        }

        private void StateDirective_RemoveMacro(string mnemonic, string[] operands)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(Strings.Assembler_Error_DELMACRO_Operand_Count);
            }
            bool removed = false;
            if (singleLineMacros.Remove(operands[0]))
            {
                _ = singleLineMacroNames.Remove(operands[0]);
                removed = true;
            }
            if (multiLineMacros.Remove(operands[0]))
            {
                _ = multiLineMacroNames.Remove(operands[0]);
                removed = true;
            }
            if (!removed)
            {
                throw new MacroNameException(string.Format(Strings.Assembler_Error_DELMACRO_Not_Exists, operands[0]));
            }
        }

        private void StateDirective_ManualLabelDefine(string mnemonic, string[] operands)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_LABEL_OVERRIDE_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[0]);
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

            lineIsLabelled = false;
            lineIsEntry = false;
            overriddenLabels.UnionWith(labelsToEdit);
        }

        private void StateDirective_SetAnalyzerState(string mnemonic, string[] operands)
        {
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
        }

        private void StateDirective_EmitAssemblerMessage(string mnemonic, string[] operands)
        {
            if (operands.Length is < 1 or > 2)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_MESSAGE_Operand_Count, operands.Length));
            }
            WarningSeverity severity = operands[0].ToUpperInvariant() switch
            {
                "ERROR" => WarningSeverity.NonFatalError,
                "WARNING" => WarningSeverity.Warning,
                "SUGGESTION" => WarningSeverity.Suggestion,
                _ => throw new OperandException(Strings.Assembler_Error_MESSAGE_Operand_First)
            };
            string? message = null;
            if (operands.Length == 2)
            {
                OperandType operandType = DetermineOperandType(operands[1]);
                if (operandType != OperandType.Literal)
                {
                    throw new OperandException(string.Format(Strings.Assembler_Error_MESSAGE_Operand_Second_Type, operandType));
                }
                if (operands[1][0] != '"')
                {
                    throw new OperandException(Strings.Assembler_Error_MESSAGE_Operand_Second_String);
                }
                byte[] parsedBytes = ParseLiteral(operands[1], true);
                message = Encoding.UTF8.GetString(parsedBytes);
            }
            warnings.Add(new Warning(
                severity, 0000, currentImport?.ImportPath ?? string.Empty, currentImport?.CurrentLine ?? baseFileLine,
                mnemonic, operands, dynamicLines[lineIndex], currentMacro?.MacroName, message));
        }

        private void StateDirective_StopAssembly(string mnemonic, string[] operands)
        {
            if (operands.Length > 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_STOP_Operand_Count, operands.Length));
            }
            string? message = null;
            if (operands.Length == 1)
            {
                OperandType operandType = DetermineOperandType(operands[0]);
                if (operandType != OperandType.Literal)
                {
                    throw new OperandException(string.Format(Strings.Assembler_Error_STOP_Operand_First_Type, operandType));
                }
                if (operands[0][0] != '"')
                {
                    throw new OperandException(Strings.Assembler_Error_STOP_Operand_First_String);
                }
                byte[] parsedBytes = ParseLiteral(operands[0], true);
                message = Encoding.UTF8.GetString(parsedBytes);
            }
            throw new AssemblyStoppedException(message ?? Strings.Assembler_Error_STOP);
        }

        private void StateDirective_RepeatSourceLines(string mnemonic, string[] operands)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_REPEAT_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[0]);
            if (operandType != OperandType.Literal)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_REPEAT_Operand_Type, operandType));
            }
            if (operands[0][0] == ':')
            {
                throw new OperandException(Strings.Assembler_Error_REPEAT_Operand_Label_Reference);
            }
            if (operands[0][0] == '-' || operands[0].Contains('.'))
            {
                throw new OperandException(Strings.Assembler_Error_REPEAT_Operand_Signed_Or_Floating);
            }
            _ = ParseLiteral(operands[0], false, out ulong repeatCount);
            if (repeatCount == 0)
            {
                throw new OperandException(Strings.Assembler_Error_REPEAT_Zero);
            }
            currentRepeatSections.Push((GetCurrentPosition(), repeatCount - 1));
        }

        private void StateDirective_EndLineRepeat(string mnemonic, string[] operands)
        {
            if (operands.Length != 0)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_ENDREPEAT_Operand_Count, operands.Length));
            }
            if (currentRepeatSections.Count == 0)
            {
                throw new EndingDirectiveException(string.Format(Strings.Assembler_Error_Opening_Directive_Missing, mnemonic));
            }
            (AssemblyPosition position, ulong iterationsRemaining) = currentRepeatSections.Pop();
            if (iterationsRemaining > 0)
            {
                currentRepeatSections.Push((position, iterationsRemaining - 1));
                // Assembler will increment line by 1 after this set, which is what we want to skip repeating the initial %REPEAT directive
                SetCurrentPosition(position);
            }
        }

        private void StateDirective_SingleAssemblyGuard(string mnemonic, string[] operands)
        {
            if (operands.Length != 0)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_ASM_ONCE_Operand_Count, operands.Length));
            }
            if (currentImport is not null)
            {
                if (!currentImport.AnyAssembledLines)
                {
                    _ = completeAsmOnceFiles.Add(currentImport.ImportPath);
                }
                if (timesSeenFile.TryGetValue(currentImport.ImportPath, out int times) && times > 1)
                {
                    while (currentImport?.CurrentLine < currentImport?.TotalLines)
                    {
                        _ = IncrementCurrentLine();
                    }
                }
            }
        }

        private void StateDirective_DefineAssemblerVariable(string mnemonic, string[] operands)
        {
            if (operands.Length != 2)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_DEFINE_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[1]);
            if (operandType != OperandType.Literal)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_DEFINE_Operand_Type, operandType));
            }

            string newVariableName = operands[0];
            _ = ParseLiteral(operands[1], false, out ulong variableValue);
            SetAssemblerVariable(newVariableName, variableValue);
        }

        private void StateDirective_RemoveAssemblerVariable(string mnemonic, string[] operands)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_UNDEFINE_Operand_Count, operands.Length));
            }
            if (!assemblerVariables.Remove(operands[0]))
            {
                throw new VariableNameException(string.Format(Strings.Assembler_Error_Variable_Not_Exists, operands[0]));
            }
        }

        private void StateDirective_PrintAssemblerState(string mnemonic, string[] operands)
        {
            if (operands.Length != 0)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_DEBUG_Operand_Count, operands.Length));
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Header,
                currentImport?.CurrentLine ?? baseFileLine,
                currentImport is null ? Strings.Generic_Base_File : currentImport.ImportPath, program.Count);
            if (macroLineDepth != 0)
            {
                Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Header_Macro_Lines, macroLineDepth);
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Label_Header, labels.Count);
            foreach ((string labelName, ulong address) in labels)
            {
                Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Label_Line, labelName, address);
            }
            Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Label_Link_Header, labelLinks.Count);
            foreach ((string labelName, (string target, string? filePath, int line)) in labelLinks)
            {
                Console.Error.WriteLine(
                    Strings.Assembler_Debug_Directive_Label_Link_Line, labelName, target, filePath ?? Strings.Generic_Base_File, line);
            }
            Console.Error.WriteLine(Strings.Assembler_Debug_Directive_LabelRef_Header, labelReferences.Count);
            foreach ((string labelName, ulong insertOffset, _) in labelReferences)
            {
                Console.Error.WriteLine(
                    Strings.Assembler_Debug_Directive_LabelRef_Line, labelName, insertOffset);
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
            Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Assembler_Variable_Header, assemblerVariables.Count);
            foreach ((string variable, ulong value) in assemblerVariables)
            {
                Console.Error.WriteLine(Strings.Assembler_Debug_Directive_Assembler_Variable_Line, variable, value);
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
        }

        private void StateDirective_DanglingClosingDirective(string mnemonic, string[] operands)
        {
            throw new EndingDirectiveException(string.Format(Strings.Assembler_Error_Opening_Directive_Missing, mnemonic));
        }

        // DATA DIRECTIVES

        private static byte[] DataDirective_ByteAndStringInsertion(string[] operands, List<(string, ulong)> referencedLabels)
        {
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
            return operands[0][0] != '"' ? parsedBytes[..1] : parsedBytes;
        }

        private static byte[] DataDirective_ZeroPadding(string[] operands, List<(string, ulong)> referencedLabels)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_PAD_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[0]);
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
            return Enumerable.Repeat((byte)0, (int)parsedNumber).ToArray();
        }

        private static byte[] DataDirective_NumberInsertion(string[] operands, List<(string, ulong)> referencedLabels)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_NUM_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[0]);
            if (operandType != OperandType.Literal)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_NUM_Operand_Type, operandType));
            }
            if (operands[0][0] == ':')
            {
                // Label reference used as %NUM operand
                referencedLabels.Add((operands[0][2..], 0));
                // Label location will be resolved later, pad with 0s for now
                return Enumerable.Repeat((byte)0, 8).ToArray();
            }
            return ParseLiteral(operands[0], false);
        }

        private static byte[] DataDirective_RawFileInsertion(string[] operands, List<(string, ulong)> referencedLabels)
        {
            if (operands.Length != 1)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_IBF_Operand_Count, operands.Length));
            }
            OperandType operandType = DetermineOperandType(operands[0]);
            if (operandType != OperandType.Literal)
            {
                throw new OperandException(string.Format(Strings.Assembler_Error_IBF_Operand_Type, operandType));
            }
            if (operands[0][0] != '"')
            {
                throw new OperandException(Strings.Assembler_Error_IBF_Operand_String);
            }
            byte[] parsedBytes = ParseLiteral(operands[0], true);
            string importPath = Encoding.UTF8.GetString(parsedBytes);
            string resolvedPath = Path.GetFullPath(importPath);
            if (!File.Exists(resolvedPath))
            {
                throw new ImportException(string.Format(Strings.Assembler_Error_IBF_File_Not_Exists, resolvedPath));
            }
            return File.ReadAllBytes(resolvedPath);
        }
    }
}
