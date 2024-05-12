using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    internal static partial class Program
    {
        internal static readonly Version? version = typeof(Program).Assembly.GetName().Version;

        private static void Main(string[] args)
        {
            CommandLineArgs processedArgs = new(StringComparer.OrdinalIgnoreCase, EqualityComparer<char>.Default);
            processedArgs.AddArguments(args);

            args = processedArgs.GetPositionalArguments();

            if (processedArgs.IsOptionGiven('v', "version"))
            {
                Console.WriteLine(version?.ToString());
                Console.WriteLine(string.Join('|', Enum.GetValues<AAPFeatures>()
                    .Where(v => v != 0 && ((v & (v - 1)) == 0)).Select(Enum.GetName).Where(n => n != "All").Distinct()));
                return;
            }
            if (!processedArgs.IsOptionGiven('n', "no-header"))
            {
                // Write to stderr to prevent header being included in redirected stdout streams
                Console.Error.WriteLine($"AssEmbly {version?.Major}.{version?.Minor}.{version?.Build}-{(ulong)AAPFeatures.All:X}" +
                    $" {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} - CLR {Environment.Version}, {Environment.OSVersion}" +
                    $" {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
                Console.Error.WriteLine(Strings.Generic_Copyright_Header);
#if DEBUG
                Console.Error.WriteLine("(DEBUG BUILD)");
#endif
                Console.Error.WriteLine();
            }
            if (args.Length < 1)
            {
                PrintError(Strings.CLI_Error_Missing_Operation_Body);
                PrintFatalError(Strings.CLI_Error_Missing_Operation_Hint);
                return;
            }
            switch (args[0].ToLowerInvariant())
            {
                case "assemble":
                    AssembleSourceFile(processedArgs);
                    break;
                case "execute":
                    ExecuteProgram(processedArgs);
                    break;
                case "run":
                    AssembleAndExecute(processedArgs);
                    break;
                case "debug":
                    RunDebugger(processedArgs);
                    break;
                case "disassemble":
                    PerformDisassembly(processedArgs);
                    break;
                case "repl":
                    RunRepl(processedArgs);
                    break;
                case "lint":
                    PerformLintingAssembly(processedArgs);
                    break;
                case "help":
                    DisplayHelp(processedArgs);
                    break;
                case "license":
                    DisplayLicense(processedArgs);
                    break;
                default:
                    PrintFatalError(Strings.CLI_Error_Invalid_Operation, args[0]);
                    return;
            }
        }

        private static void AssembleSourceFile(CommandLineArgs args)
        {
            Stopwatch assemblyStopwatch = Stopwatch.StartNew();

            string[] positionalArgs = args.GetPositionalArguments();
            if (!CheckInputFileArg(positionalArgs, Strings.CLI_Error_Argument_Missing_Path_Assemble))
            {
                return;
            }

            (string sourcePath, string filename) = ResolveInputFilePath(positionalArgs);

            HashSet<int> disabledErrors = new();
            HashSet<int> disabledWarnings = new();
            HashSet<int> disabledSuggestions = new();
            if (args.TryGetKeyValueOption("--disabled-errors", out string? errorCodes))
            {
                foreach (string codeString in errorCodes.Split(','))
                {
                    if (!int.TryParse(codeString, out int errorCode))
                    {
                        PrintFatalError(Strings.CLI_Assemble_Error_Invalid_Error_Code, codeString);
                        return;
                    }
                    _ = disabledErrors.Add(errorCode);
                }
            }
            if (args.TryGetKeyValueOption("--disabled-warnings", out string? warningCodes))
            {
                foreach (string codeString in warningCodes.Split(','))
                {
                    if (!int.TryParse(codeString, out int errorCode))
                    {
                        PrintFatalError(Strings.CLI_Assemble_Error_Invalid_Warning_Code, codeString);
                        return;
                    }
                    _ = disabledWarnings.Add(errorCode);
                }
            }
            if (args.TryGetKeyValueOption("--disabled-suggestions", out string? suggestionsCodes))
            {
                foreach (string codeString in suggestionsCodes.Split(','))
                {
                    if (!int.TryParse(codeString, out int errorCode))
                    {
                        PrintFatalError(Strings.CLI_Assemble_Error_Invalid_Suggestion_Code, codeString);
                        return;
                    }
                    _ = disabledSuggestions.Add(errorCode);
                }
            }

            if (args.IsOptionGiven('E', "no-errors"))
            {
                disabledErrors = AssemblerWarnings.NonFatalErrorMessages.Keys.ToHashSet();
            }
            if (args.IsOptionGiven('W', "no-warnings"))
            {
                disabledWarnings = AssemblerWarnings.WarningMessages.Keys.ToHashSet();
            }
            if (args.IsOptionGiven('S', "no-suggestions"))
            {
                disabledSuggestions = AssemblerWarnings.SuggestionMessages.Keys.ToHashSet();
            }

#if V1_CALL_STACK_COMPAT
            bool useV1Format = false;
            bool useV1Stack = false;
            if (args.IsOptionGiven('1', "v1-format"))
            {
                useV1Format = true;
                useV1Stack = true;
            }
            if (args.IsMultiCharacterOptionGiven("v1-call-stack"))
            {
                useV1Stack = true;
            }
#endif
            int macroExpansionLimit = GetMacroLimit(args);
            int whileRepeatLimit = GetWhileLimit(args);

            AssemblyResult assemblyResult;
            int totalErrors = 0;
            int totalWarnings = 0;
            int totalSuggestions = 0;
            try
            {
                Assembler assembler = new(sourcePath,
#if V1_CALL_STACK_COMPAT
                    useV1Format, useV1Stack,
#else
                    false, false,
#endif
                    disabledErrors, disabledWarnings, disabledSuggestions);
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
                if (whileRepeatLimit >= 0)
                {
                    assembler.WhileRepeatLimit = whileRepeatLimit;
                }
                assembler.EnableObsoleteDirectives = args.IsMultiCharacterOptionGiven("allow-old-directives");
                assembler.EnableVariableExpansion = !args.IsMultiCharacterOptionGiven("disable-variables");
                assembler.EnableEscapeSequences = !args.IsMultiCharacterOptionGiven("disable-escapes");
                assembler.EnableFilePathMacros = !args.IsMultiCharacterOptionGiven("disable-file-macros");
                assembler.ForceFullOpcodes = args.IsOptionGiven('f', "full-base-opcodes");
                foreach ((string name, ulong value) in GetVariableDefinitions(args))
                {
                    assembler.SetAssemblerVariable(name, value);
                }
                assembler.AssembleLines(File.ReadAllLines(sourcePath));
                assemblyResult = assembler.GetAssemblyResult(true);
                // Sort warnings by severity, then file, then line
                Array.Sort(assemblyResult.Warnings, (a, b) =>
                {
                    if (a.Severity == b.Severity)
                    {
                        if (a.Position.File.Equals(b.Position.File, StringComparison.OrdinalIgnoreCase))
                        {
                            return a.Position.Line.CompareTo(b.Position.Line);
                        }
                        return string.Compare(b.Position.File, a.Position.File, StringComparison.OrdinalIgnoreCase);
                    }
                    // Sort severity in reverse as bottom will be most visible to user
                    return b.Severity.CompareTo(a.Severity);
                });
                foreach (Warning warning in assemblyResult.Warnings)
                {
                    Console.ForegroundColor = warning.Severity switch
                    {
                        WarningSeverity.NonFatalError => ConsoleColor.Red,
                        WarningSeverity.Warning => ConsoleColor.DarkYellow,
                        WarningSeverity.Suggestion => ConsoleColor.Cyan,
                        _ => Console.ForegroundColor
                    };
                    string messageStart = warning.Severity switch
                    {
                        WarningSeverity.NonFatalError => Strings.Generic_Severity_Error,
                        WarningSeverity.Warning => Strings.Generic_Severity_Warning,
                        WarningSeverity.Suggestion => Strings.Generic_Severity_Suggestion,
                        _ => Strings.Generic_Unknown
                    };
                    switch (warning.Severity)
                    {
                        case WarningSeverity.FatalError:
                        case WarningSeverity.NonFatalError:
                            totalErrors++;
                            break;
                        case WarningSeverity.Warning:
                            totalWarnings++;
                            break;
                        case WarningSeverity.Suggestion:
                            totalSuggestions++;
                            break;
                    }
                    string macroName = "";
                    if (warning.MacroName != "")
                    {
                        macroName = string.Format(Strings.CLI_Assemble_Error_Warning_Printout_InMacro, warning.MacroName);
                    }
                    Console.WriteLine(Strings.CLI_Assemble_Error_Warning_Printout,
                        messageStart, warning.Code, warning.Position.Line,
                        warning.Position.File,
                        warning.OriginalLine, warning.Message, macroName);
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                OnAssemblerException(e);

                Console.Write(Strings.CLI_Assemble_Result_Header_Start);
                PrintFatalError(Strings.CLI_Assemble_Result_Header_Failed);
#if DEBUG
                throw;
#else
                return;
#endif
            }

            string destination = positionalArgs.Length >= 3 ? positionalArgs[2] : filename + ".aap";
            long programSize = 0;
#if V1_CALL_STACK_COMPAT
            if (useV1Format)
            {
                File.WriteAllBytes(destination, assemblyResult.Program);
            }
            else
#endif
            {
                AAPFeatures features = assemblyResult.UsedExtensions;
#if V1_CALL_STACK_COMPAT
                if (useV1Stack)
                {
                    features |= AAPFeatures.V1CallStack;
                }
#endif
#if GZIP_COMPRESSION
                if (args.IsOptionGiven('c', "compress"))
                {
                    features |= AAPFeatures.GZipCompressed;
                }
#endif
                AAPFile executable = new(version ?? new Version(), features, assemblyResult.EntryPoint, assemblyResult.Program);
                byte[] bytes = executable.GetBytes();
                File.WriteAllBytes(destination, executable.GetBytes());
                programSize = bytes.LongLength - AAPFile.HeaderSize;
            }

            if (!args.IsOptionGiven('D', "no-debug-file"))
            {
                File.WriteAllText(destination + ".adi", assemblyResult.DebugInfo);
            }
            if (args.IsOptionGiven('e', "output-expanded"))
            {
                File.WriteAllLines(filename + ".exp.asm", assemblyResult.ExpandedSourceFile);
            }
            if (args.IsOptionGiven('s', "output-symbols"))
            {
                StringBuilder symbolFile = new StringBuilder()
                    .AppendLine("; AssEmbly Symbol File")
                    .AppendLine($"; Generated by AssEmbly v{version?.Major}.{version?.Minor}.{version?.Build}")
                    .AppendLine($"; {filename}.aap must be in the same folder as this file");
                foreach ((string label, ulong address) in assemblyResult.Labels.OrderBy(kv => kv.Value))
                {
                    if (label.StartsWith('_'))
                    {
                        continue;
                    }
                    symbolFile.AppendLine($":{label}")
#if V1_CALL_STACK_COMPAT
                        .AppendLine($"%LABEL_OVERRIDE :_SYM_FILE_@!CURRENT_ADDRESS[0x{address + (useV1Format ? 0UL : AAPFile.HeaderSize):x}]");
#else
                        .AppendLine($"%LABEL_OVERRIDE :_SYM_FILE_@!CURRENT_ADDRESS[0x{address:x}]");
#endif
                }
                symbolFile.AppendLine(":_SYM_FILE_@!CURRENT_ADDRESS")
                    .AppendLine($"%IBF \"#FOLDER_PATH/{filename}.aap\"");
                File.WriteAllText(filename + ".sym.asm", symbolFile.ToString());
            }

            assemblyStopwatch.Stop();

            Console.Write(Strings.CLI_Assemble_Result_Header_Start);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Strings.CLI_Assemble_Result_Header_Success);
            Console.ResetColor();
            if (args.IsOptionGiven('c', "compress"))
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success_Compressed, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
#if V1_CALL_STACK_COMPAT
                    useV1Format ? assemblyResult.Program.LongLength :
#endif
                    programSize,
                    (double)(
#if V1_CALL_STACK_COMPAT
                        useV1Format ? assemblyResult.Program.LongLength :
#endif
                        programSize) / assemblyResult.Program.LongLength,
#if V1_CALL_STACK_COMPAT
                    useV1Format ? assemblyResult.Program.LongLength :
#endif
                    programSize + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines.Length, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
#if V1_CALL_STACK_COMPAT
                    useV1Format ? assemblyResult.Program.LongLength :
#endif
                    assemblyResult.Program.LongLength + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines.Length, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
            }

            args.WarnUnconsumedOptions(3);
        }

        private static void ExecuteProgram(CommandLineArgs args)
        {
            string[] positionalArgs = args.GetPositionalArguments();
            if (!CheckInputFileArg(positionalArgs, Strings.CLI_Error_Argument_Missing_Path_Execute))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(positionalArgs);

            ulong memSize = GetMemorySize(args);

            Processor? processor = LoadExecutableToProcessor(appPath, memSize,
#if V1_CALL_STACK_COMPAT
                args.IsOptionGiven('1', "v1-format"),
                args.IsMultiCharacterOptionGiven("v1-call-stack"),
#else
                false, false,
#endif
                args.IsOptionGiven('i', "ignore-newer-version"),
                !args.IsOptionGiven('u', "unmapped-stack"),
                args.IsOptionGiven('a', "auto-echo"));

            if (processor is null)
            {
                return;
            }

            args.WarnUnconsumedOptions(2);

            ExecuteProcessor(processor);
        }

        private static void AssembleAndExecute(CommandLineArgs args)
        {
            string[] positionalArgs = args.GetPositionalArguments();
            if (!CheckInputFileArg(positionalArgs, Strings.CLI_Error_Argument_Missing_Path_AssembleAndExecute))
            {
                return;
            }

            (string sourcePath, _) = ResolveInputFilePath(positionalArgs);

            ulong memSize = GetMemorySize(args);

            AssemblyResult assemblyResult;
            try
            {
                int macroExpansionLimit = GetMacroLimit(args);
                int whileRepeatLimit = GetWhileLimit(args);

                Assembler assembler = new(sourcePath);
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
                if (whileRepeatLimit >= 0)
                {
                    assembler.WhileRepeatLimit = whileRepeatLimit;
                }
                assembler.EnableObsoleteDirectives = args.IsMultiCharacterOptionGiven("allow-old-directives");
                assembler.EnableVariableExpansion = !args.IsMultiCharacterOptionGiven("disable-variables");
                assembler.EnableEscapeSequences = !args.IsMultiCharacterOptionGiven("disable-escapes");
                assembler.EnableFilePathMacros = !args.IsMultiCharacterOptionGiven("disable-file-macros");
                foreach ((string name, ulong value) in GetVariableDefinitions(args))
                {
                    assembler.SetAssemblerVariable(name, value);
                }
                assembler.AssembleLines(File.ReadAllLines(sourcePath));
                assemblyResult = assembler.GetAssemblyResult(true);
            }
            catch (Exception e)
            {
                OnAssemblerException(e);
#if DEBUG
                throw;
#else
                Environment.Exit(1);
                return;
#endif
            }

            Processor processor = new(
                memSize, assemblyResult.EntryPoint,
#if V1_CALL_STACK_COMPAT
                useV1CallStack: args.IsMultiCharacterOptionGiven("v1-call-stack"),
#endif
                mapStack: !args.IsOptionGiven('u', "unmapped-stack"),
                autoEcho: args.IsOptionGiven('a', "auto-echo"));
            LoadProgramIntoProcessor(processor, assemblyResult.Program);

            args.WarnUnconsumedOptions(2);

            ExecuteProcessor(processor);
        }

        private static void RunDebugger(CommandLineArgs args)
        {
            string[] positionalArgs = args.GetPositionalArguments();
            if (!CheckInputFileArg(positionalArgs, Strings.CLI_Error_Argument_Missing_Path_Debugger))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(positionalArgs);

            ulong memSize = GetMemorySize(args);

            Processor? processor = LoadExecutableToProcessor(appPath, memSize,
#if V1_CALL_STACK_COMPAT
                args.IsOptionGiven('1', "v1-format"),
                args.IsMultiCharacterOptionGiven("v1-call-stack"),
#else
                false, false,
#endif
                args.IsOptionGiven('i', "ignore-newer-version"),
                !args.IsOptionGiven('u', "unmapped-stack"),
                args.IsOptionGiven('a', "auto-echo"));

            if (processor is null)
            {
                return;
            }

            Debugger debugger = new(false, processor);
            if (positionalArgs.Length >= 3)
            {
                string debugFilePath = positionalArgs[2];
                debugger.LoadDebugFile(debugFilePath);
            }

            args.WarnUnconsumedOptions(3);

            debugger.StartDebugger();
        }

        private static void PerformDisassembly(CommandLineArgs args)
        {
            string[] positionalArgs = args.GetPositionalArguments();
            if (!CheckInputFileArg(positionalArgs, Strings.CLI_Error_Argument_Missing_Path_Disassemble))
            {
                return;
            }

            (string sourcePath, string filename) = ResolveInputFilePath(positionalArgs);

            string disassembledProgram;
            byte[] program;
#if V1_CALL_STACK_COMPAT
            if (args.IsOptionGiven('1', "v1-format"))
            {
                program = File.ReadAllBytes(sourcePath);
            }
            else
#endif
            {
                AAPFile? file = LoadAAPFile(sourcePath,
                    args.IsOptionGiven('i', "ignore-newer-version"));
                if (file is null)
                {
                    return;
                }
                program = file.Program;
            }

            try
            {
                disassembledProgram = Disassembler.DisassembleProgram(
                    program, new DisassemblerOptions()
                    {
                        DetectStrings = !args.IsOptionGiven('S', "no-strings"),
                        DetectPads = !args.IsOptionGiven('P', "no-pads"),
                        DetectFloats = !args.IsOptionGiven('F', "no-floats"),
                        DetectSigned = !args.IsOptionGiven('G', "no-signed"),
                        AllowFullyQualifiedBaseOpcodes = args.IsOptionGiven('f', "allow-full-base-opcodes")
                    });
            }
            catch (Exception e)
            {
                PrintFatalError(Strings.CLI_Disassemble_Error_Unexpected, e.GetType().Name, e.Message);
#if DEBUG
                throw;
#else
                return;
#endif
            }
            string destination = positionalArgs.Length >= 3 ? positionalArgs[2] : filename + ".dis.asm";
            File.WriteAllText(destination, disassembledProgram);
            Console.WriteLine(Strings.CLI_Disassemble_Success, Path.GetFullPath(destination));

            args.WarnUnconsumedOptions(3);
        }

        private static void RunRepl(CommandLineArgs args)
        {
            ulong memSize = GetMemorySize(args);
            Debugger debugger = new(true, memorySize: memSize,
#if V1_CALL_STACK_COMPAT
                useV1CallStack: args.IsMultiCharacterOptionGiven("v1-call-stack"),
#endif
                mapStack: !args.IsOptionGiven('u', "unmapped-stack"),
                autoEcho: args.IsOptionGiven('a', "auto-echo"));
            // Some program needs to be loaded or the processor won't run
            debugger.DebuggingProcessor.LoadProgram(Array.Empty<byte>());

            args.WarnUnconsumedOptions(1);

            debugger.StartDebugger();
        }

        [Localizable(false)]
        private static void PerformLintingAssembly(CommandLineArgs args)
        {
            // This is an undocumented operation designed for IDE extensions to provide linting on source files.
            // As such, all output is JSON formatted and does not use console colours.
            string[] positionalArgs = args.GetPositionalArguments();
            if (positionalArgs.Length < 2)
            {
                Console.WriteLine("{\"error\":\"A path to the program to be disassembled is required.\"}");
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(positionalArgs[1]))
            {
                Console.WriteLine("{\"error\":\"The specified file does not exist.\"}");
                Environment.Exit(1);
                return;
            }

            (string sourcePath, _) = ResolveInputFilePath(positionalArgs);

            try
            {
                int macroExpansionLimit = GetMacroLimit(args);
                int whileRepeatLimit = GetWhileLimit(args);

                Assembler assembler = new(sourcePath);
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
                if (whileRepeatLimit >= 0)
                {
                    assembler.WhileRepeatLimit = whileRepeatLimit;
                }
                assembler.EnableObsoleteDirectives = args.IsMultiCharacterOptionGiven("allow-old-directives");
                assembler.EnableVariableExpansion = !args.IsMultiCharacterOptionGiven("disable-variables");
                assembler.EnableEscapeSequences = !args.IsMultiCharacterOptionGiven("disable-escapes");
                assembler.EnableFilePathMacros = !args.IsMultiCharacterOptionGiven("disable-file-macros");
                foreach ((string name, ulong value) in GetVariableDefinitions(args))
                {
                    assembler.SetAssemblerVariable(name, value);
                }
                assembler.AssembleLines(File.ReadAllLines(sourcePath));
                AssemblyResult assemblyResult = assembler.GetAssemblyResult(true);
                Console.WriteLine(JsonSerializer.Serialize(assemblyResult, new JsonSerializerOptions { IncludeFields = true }));
            }
            catch (Exception e)
            {
                if (e is AssemblerException assemblerException)
                {
                    Console.WriteLine(JsonSerializer.Serialize(new Warning[1] { assemblerException.WarningObject },
                        new JsonSerializerOptions { IncludeFields = true }));
                }
                else
                {
                    Console.WriteLine($"{{\"error\":\"An unexpected error occurred " +
                        $"({HttpUtility.JavaScriptStringEncode(e.GetType().Name)}): {HttpUtility.JavaScriptStringEncode(e.Message)}\"}}");
                }
            }
        }

        private static void DisplayHelp(CommandLineArgs args)
        {
            string[] positionalArgs = args.GetPositionalArguments();

            if (positionalArgs.Length <= 1)
            {
                Console.WriteLine(Strings.CLI_Help_Body, DefaultMemorySize,
                    Assembler.DefaultMacroExpansionLimit, Assembler.DefaultWhileRepeatLimit);
            }
            else
            {
                switch (positionalArgs[1].ToLowerInvariant())
                {
                    case "assemble":
                        PrintOperationHelp("assemble", Strings.CLI_Help_Description_Assemble, Strings.CLI_Help_Options_Assemble);
                        break;
                    case "execute":
                        PrintOperationHelp("execute", Strings.CLI_Help_Description_Execute, Strings.CLI_Help_Options_Execute);
                        break;
                    case "run":
                        PrintOperationHelp("run", Strings.CLI_Help_Description_Run, Strings.CLI_Help_Options_Run);
                        break;
                    case "debug":
                        PrintOperationHelp("debug", Strings.CLI_Help_Description_Debug, Strings.CLI_Help_Options_Debug);
                        break;
                    case "disassemble":
                        PrintOperationHelp("disassemble", Strings.CLI_Help_Description_Disassemble, Strings.CLI_Help_Options_Disassemble);
                        break;
                    case "repl":
                        PrintOperationHelp("repl", Strings.CLI_Help_Description_REPL, Strings.CLI_Help_Options_REPL);
                        break;
                    case "license":
                        PrintOperationHelp("license", Strings.CLI_Help_Description_License, Strings.CLI_Help_Options_License);
                        break;
                    case "help":
                        PrintOperationHelp("help", Strings.CLI_Help_Description_Help, Strings.CLI_Help_Options_Help);
                        break;
                    default:
                        PrintError(Strings.CLI_Error_Invalid_Operation, positionalArgs[1]);
                        break;
                }
            }

            args.WarnUnconsumedOptions(2);
        }

        private static void DisplayLicense(CommandLineArgs args)
        {
            try
            {
                Console.WriteLine(Strings.CLI_License_Header);
                Console.WriteLine();
                if (!Console.IsOutputRedirected)
                {
                    // Wait for user to press a key before printing actual license text
                    Console.Write(Strings.Generic_Press_Any_Key_To_Continue);
                    _ = Console.ReadKey(true);
                    Console.WriteLine();
                }
                Console.WriteLine();
                using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AssEmbly.LICENSE")
                    ?? throw new NullReferenceException("Resource stream with name 'LICENSE' was missing");
                using StreamReader resourceReader = new(resourceStream);
                Console.WriteLine(resourceReader.ReadToEnd(), DefaultMemorySize, Assembler.DefaultMacroExpansionLimit);

                args.WarnUnconsumedOptions(1);
            }
            catch
            {
                PrintFatalError(Strings.CLI_License_Error);
#if DEBUG
                throw;
#else
                return;
#endif
            }
        }

        private static void PrintOperationHelp(string operationName,
            [Localizable(true)] string operationDescription,
            [Localizable(true)] string options)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(operationName);

            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(' ' + operationDescription);

            Console.ResetColor();
            Console.WriteLine(options);
        }
    }
}