using System.Diagnostics;
using System.Reflection;
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
            if (args.Contains("--version", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine(version?.ToString());
                return;
            }
            if (!args.Contains("--no-header", StringComparer.OrdinalIgnoreCase))
            {
                // Write to stderr to prevent header being included in redirected stdout streams
                Console.Error.WriteLine($"AssEmbly {version?.Major}.{version?.Minor}.{version?.Build} {(Environment.Is64BitProcess ? "64-bit" : "32-bit")}" +
                    $" - CLR {Environment.Version}, {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
                Console.Error.WriteLine(Strings.Generic_Copyright_Header);
#if DEBUG
                Console.Error.WriteLine("(DEBUG BUILD)");
#endif
                Console.Error.WriteLine();
            }
            if (args.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Error_Missing_Operation_Body);
                Console.WriteLine(Strings.CLI_Error_Missing_Operation_Hint);
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            switch (args[0])
            {
                case "assemble":
                    AssembleSourceFile(args);
                    break;
                case "execute":
                    ExecuteProgram(args);
                    break;
                case "run":
                    AssembleAndExecute(args);
                    break;
                case "debug":
                    RunDebugger(args);
                    break;
                case "disassemble":
                    PerformDisassembly(args);
                    break;
                case "repl":
                    RunRepl(args);
                    break;
                case "lint":
                    PerformLintingAssembly(args);
                    break;
                case "help":
                    DisplayHelp();
                    break;
                case "license":
                    DisplayLicense();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.CLI_Error_Invalid_Operation, args[0]);
                    Console.ResetColor();
                    Environment.Exit(1);
                    return;
            }
        }

        private static void AssembleSourceFile(string[] args)
        {
            Stopwatch assemblyStopwatch = Stopwatch.StartNew();
            if (!CheckInputFileArg(args, Strings.CLI_Error_Argument_Missing_Path_Assemble))
            {
                return;
            }

            (string sourcePath, string filename) = ResolveInputFilePath(args);

            HashSet<int> disabledErrors = new();
            HashSet<int> disabledWarnings = new();
            HashSet<int> disabledSuggestions = new();
            bool useV1Format = false;
            bool useV1Stack = false;
            bool enableObsoleteDirectives = false;
            int macroExpansionLimit = GetMacroLimit(args);
            int whileRepeatLimit = GetWhileLimit(args);
            foreach (string a in args)
            {
                if (a.StartsWith("--disable-error-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(a[16..], out int errorCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.CLI_Assemble_Error_Invalid_Error_Code, a[16..]);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return;
                    }
                    _ = disabledErrors.Add(errorCode);
                }
                else if (a.StartsWith("--disable-warning-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(a[18..], out int errorCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.CLI_Assemble_Error_Invalid_Warning_Code, a[18..]);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return;
                    }
                    _ = disabledWarnings.Add(errorCode);
                }
                else if (a.StartsWith("--disable-suggestion-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(a[21..], out int errorCode))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.CLI_Assemble_Error_Invalid_Suggestion_Code, a[21..]);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return;
                    }
                    _ = disabledSuggestions.Add(errorCode);
                }
                else if (a.Equals("--no-errors", StringComparison.OrdinalIgnoreCase))
                {
                    disabledErrors = AssemblerWarnings.NonFatalErrorMessages.Keys.ToHashSet();
                }
                else if (a.Equals("--no-warnings", StringComparison.OrdinalIgnoreCase))
                {
                    disabledWarnings = AssemblerWarnings.WarningMessages.Keys.ToHashSet();
                }
                else if (a.Equals("--no-suggestions", StringComparison.OrdinalIgnoreCase))
                {
                    disabledSuggestions = AssemblerWarnings.SuggestionMessages.Keys.ToHashSet();
                }
                else if (a.Equals("--v1-format", StringComparison.OrdinalIgnoreCase))
                {
                    useV1Format = true;
                    useV1Stack = true;
                }
                else if (a.Equals("--v1-call-stack", StringComparison.OrdinalIgnoreCase))
                {
                    useV1Stack = true;
                }
                else if (a.Equals("--allow-old-directives", StringComparison.OrdinalIgnoreCase))
                {
                    enableObsoleteDirectives = true;
                }
            }

            AssemblyResult assemblyResult;
            int totalErrors = 0;
            int totalWarnings = 0;
            int totalSuggestions = 0;
            try
            {
                Assembler assembler = new(useV1Format, useV1Stack, disabledErrors, disabledWarnings, disabledSuggestions);
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
                if (whileRepeatLimit >= 0)
                {
                    assembler.WhileRepeatLimit = whileRepeatLimit;
                }
                assembler.EnableObsoleteDirectives = enableObsoleteDirectives;
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
                        warning.Position.File == "" ? Strings.Generic_Base_File : warning.Position.File,
                        warning.OriginalLine, warning.Message, macroName);
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                OnAssemblerException(e);

                Console.Write(Strings.CLI_Assemble_Result_Header_Start);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Assemble_Result_Header_Failed);
                Console.ResetColor();

#if DEBUG
                throw;
#else
                Environment.Exit(1);
                return;
#endif
            }

            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".aap";
            long programSize = 0;
            if (useV1Format)
            {
                File.WriteAllBytes(destination, assemblyResult.Program);
            }
            else
            {
                AAPFeatures features = assemblyResult.UsedExtensions;
                if (useV1Stack)
                {
                    features |= AAPFeatures.V1CallStack;
                }
                if (args.Contains("--compress", StringComparer.OrdinalIgnoreCase))
                {
                    features |= AAPFeatures.GZipCompressed;
                }
                AAPFile executable = new(version ?? new Version(), features, assemblyResult.EntryPoint, assemblyResult.Program);
                byte[] bytes = executable.GetBytes();
                File.WriteAllBytes(destination, executable.GetBytes());
                programSize = bytes.LongLength - AAPFile.HeaderSize;
            }

            if (!args.Contains("--no-debug-file", StringComparer.OrdinalIgnoreCase))
            {
                File.WriteAllText(destination + ".adi", assemblyResult.DebugInfo);
            }
            if (args.Contains("--output-expanded", StringComparer.OrdinalIgnoreCase))
            {
                File.WriteAllLines(filename + ".exp.asm", assemblyResult.ExpandedSourceFile);
            }

            assemblyStopwatch.Stop();

            Console.Write(Strings.CLI_Assemble_Result_Header_Start);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Strings.CLI_Assemble_Result_Header_Success);
            Console.ResetColor();
            if (args.Contains("--compress", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success_Compressed, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
                    useV1Format ? assemblyResult.Program.LongLength : programSize,
                    (double)(useV1Format ? assemblyResult.Program.LongLength : programSize) / assemblyResult.Program.LongLength,
                    useV1Format ? assemblyResult.Program.LongLength : programSize + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines.Length, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
                    useV1Format ? assemblyResult.Program.LongLength : assemblyResult.Program.LongLength + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines.Length, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private static void ExecuteProgram(string[] args)
        {
            if (!CheckInputFileArg(args, Strings.CLI_Error_Argument_Missing_Path_Execute))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            Processor processor = LoadExecutableToProcessor(appPath, memSize,
                args.Contains("--v1-format", StringComparer.OrdinalIgnoreCase),
                args.Contains("--v1-call-stack", StringComparer.OrdinalIgnoreCase),
                args.Contains("--ignore-newer-version", StringComparer.OrdinalIgnoreCase),
                !args.Contains("--unmapped-stack", StringComparer.OrdinalIgnoreCase),
                args.Contains("--auto-echo", StringComparer.OrdinalIgnoreCase));

            ExecuteProcessor(processor);
        }

        private static void AssembleAndExecute(string[] args)
        {
            if (!CheckInputFileArg(args, Strings.CLI_Error_Argument_Missing_Path_AssembleAndExecute))
            {
                return;
            }

            (string sourcePath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            AssemblyResult assemblyResult;
            try
            {
                int macroExpansionLimit = GetMacroLimit(args);
                int whileRepeatLimit = GetWhileLimit(args);

                Assembler assembler = new();
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
                if (whileRepeatLimit >= 0)
                {
                    assembler.WhileRepeatLimit = whileRepeatLimit;
                }
                assembler.EnableObsoleteDirectives = args.Contains(
                    "--allow-old-directives", StringComparer.OrdinalIgnoreCase);
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
                useV1CallStack: args.Contains("--v1-call-stack", StringComparer.OrdinalIgnoreCase),
                mapStack: !args.Contains("--unmapped-stack", StringComparer.OrdinalIgnoreCase),
                autoEcho: args.Contains("--auto-echo", StringComparer.OrdinalIgnoreCase));
            LoadProgramIntoProcessor(processor, assemblyResult.Program);
            ExecuteProcessor(processor);
        }

        private static void RunDebugger(string[] args)
        {
            if (!CheckInputFileArg(args, Strings.CLI_Error_Argument_Missing_Path_Debugger))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            Processor processor = LoadExecutableToProcessor(appPath, memSize,
                args.Contains("--v1-format", StringComparer.OrdinalIgnoreCase),
                args.Contains("--v1-call-stack", StringComparer.OrdinalIgnoreCase),
                args.Contains("--ignore-newer-version", StringComparer.OrdinalIgnoreCase),
                !args.Contains("--unmapped-stack", StringComparer.OrdinalIgnoreCase),
                args.Contains("--auto-echo", StringComparer.OrdinalIgnoreCase));

            Debugger debugger = new(false, processor);
            if (args.Length >= 3 && !args[2].StartsWith('-'))
            {
                string debugFilePath = args[2];
                debugger.LoadDebugFile(debugFilePath);
            }
            debugger.StartDebugger();
        }

        private static void PerformDisassembly(string[] args)
        {
            if (!CheckInputFileArg(args, Strings.CLI_Error_Argument_Missing_Path_Disassemble))
            {
                return;
            }

            (string sourcePath, string filename) = ResolveInputFilePath(args);

            string disassembledProgram;
            byte[] program;
            if (args.Contains("--v1-format", StringComparer.OrdinalIgnoreCase))
            {
                program = File.ReadAllBytes(sourcePath);
            }
            else
            {
                AAPFile file = LoadAAPFile(sourcePath,
                    args.Contains("--ignore-newer-version", StringComparer.OrdinalIgnoreCase));
                program = file.Program;
            }

            try
            {
                disassembledProgram = Disassembler.DisassembleProgram(
                    program, !args.Contains("--no-strings", StringComparer.OrdinalIgnoreCase),
                    !args.Contains("--no-pads", StringComparer.OrdinalIgnoreCase),
                    args.Contains("--allow-full-base-opcodes", StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Disassemble_Error_Unexpected, e.GetType().Name, e.Message);
                Console.ResetColor();
#if DEBUG
                throw;
#else
                Environment.Exit(1);
                return;
#endif
            }
            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".dis.asm";
            File.WriteAllText(destination, disassembledProgram);
            Console.WriteLine(Strings.CLI_Disassemble_Success, Path.GetFullPath(destination));
        }

        private static void RunRepl(string[] args)
        {
            ulong memSize = GetMemorySize(args);
            Debugger debugger = new(true, memorySize: memSize,
                useV1CallStack: args.Contains("--v1-call-stack", StringComparer.OrdinalIgnoreCase),
                mapStack: !args.Contains("--unmapped-stack", StringComparer.OrdinalIgnoreCase),
                autoEcho: args.Contains("--auto-echo", StringComparer.OrdinalIgnoreCase));
            // Some program needs to be loaded or the processor won't run
            debugger.DebuggingProcessor.LoadProgram(Array.Empty<byte>());
            debugger.StartDebugger();
        }

        [System.ComponentModel.Localizable(false)]
        private static void PerformLintingAssembly(string[] args)
        {
            // This is an undocumented operation designed for IDE extensions to provide linting on source files.
            // As such, all output is JSON formatted and does not use console colours.
            if (args.Length < 2)
            {
                Console.WriteLine("{\"error\":\"A path to the program to be disassembled is required.\"}");
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("{\"error\":\"The specified file does not exist.\"}");
                Environment.Exit(1);
                return;
            }

            (string sourcePath, _) = ResolveInputFilePath(args);

            try
            {
                int macroExpansionLimit = GetMacroLimit(args);

                Assembler assembler = new();
                if (macroExpansionLimit >= 0)
                {
                    assembler.MacroExpansionLimit = macroExpansionLimit;
                }
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

        private static void DisplayHelp()
        {
            Console.WriteLine(Strings.CLI_Help_Body, DefaultMemorySize,
                Assembler.DefaultMacroExpansionLimit, Assembler.DefaultWhileRepeatLimit);
        }

        private static void DisplayLicense()
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
                using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AssEmbly.LICENCE")
                    ?? throw new NullReferenceException("Resource stream with name 'LICENSE' was missing");
                using StreamReader resourceReader = new(resourceStream);
                Console.WriteLine(resourceReader.ReadToEnd(), DefaultMemorySize, Assembler.DefaultMacroExpansionLimit);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_License_Error);
                Console.ResetColor();
#if DEBUG
                throw;
#endif
            }
        }
    }
}