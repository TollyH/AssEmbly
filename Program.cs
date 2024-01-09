using System.Diagnostics;
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
            if (args.Contains("--version"))
            {
                Console.WriteLine(version?.ToString());
                return;
            }
            if (!args.Contains("--no-header"))
            {
                // Write to stderr to prevent header being included in redirected stdout streams
                Console.Error.WriteLine($"AssEmbly {version?.Major}.{version?.Minor}.{version?.Build} {(Environment.Is64BitProcess ? "64-bit" : "32-bit")}" +
                    $" - CLR {Environment.Version}, {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
                Console.Error.WriteLine("Copyright © 2022-2024  Ptolemy Hill");
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
            foreach (string a in args)
            {
                string lowerA = a.ToLowerInvariant();
                if (lowerA.StartsWith("--disable-error-"))
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
                else if (lowerA.StartsWith("--disable-warning-"))
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
                else if (lowerA.StartsWith("--disable-suggestion-"))
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
                else if (lowerA == "--no-errors")
                {
                    disabledErrors = AssemblerWarnings.NonFatalErrorMessages.Keys.ToHashSet();
                }
                else if (lowerA == "--no-warnings")
                {
                    disabledWarnings = AssemblerWarnings.WarningMessages.Keys.ToHashSet();
                }
                else if (lowerA == "--no-suggestions")
                {
                    disabledSuggestions = AssemblerWarnings.SuggestionMessages.Keys.ToHashSet();
                }
                else if (lowerA == "--v1-format")
                {
                    useV1Format = true;
                }
            }

            Assembler.AssemblyResult assemblyResult;
            int totalErrors = 0;
            int totalWarnings = 0;
            int totalSuggestions = 0;
            try
            {
                assemblyResult = Assembler.AssembleLines(File.ReadAllLines(sourcePath),
                    useV1Format, disabledErrors, disabledWarnings, disabledSuggestions);
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
                        default: break;
                    }
                    Console.WriteLine(Strings.CLI_Assemble_Error_Warning_Printout,
                        messageStart, warning.Code, warning.Line, warning.File == "" ? Strings.Generic_Base_File : warning.File,
                        warning.OriginalLine, warning.Message);
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

                Environment.Exit(1);
                return;
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
                if (args.Contains("--v1-call-stack"))
                {
                    features |= AAPFeatures.V1CallStack;
                }
                if (args.Contains("--compress"))
                {
                    features |= AAPFeatures.GZipCompressed;
                }
                AAPFile executable = new(version ?? new Version(), features, assemblyResult.EntryPoint, assemblyResult.Program);
                byte[] bytes = executable.GetBytes();
                File.WriteAllBytes(destination, executable.GetBytes());
                programSize = bytes.LongLength - AAPFile.HeaderSize;
            }

            if (!args.Contains("--no-debug-file"))
            {
                File.WriteAllText(destination + ".adi", assemblyResult.DebugInfo);
            }

            assemblyStopwatch.Stop();

            Console.Write(Strings.CLI_Assemble_Result_Header_Start);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Strings.CLI_Assemble_Result_Header_Success);
            Console.ResetColor();
            if (args.Contains("--compress"))
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success_Compressed, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
                    useV1Format ? assemblyResult.Program.LongLength : programSize,
                    (double)(useV1Format ? assemblyResult.Program.LongLength : programSize) / assemblyResult.Program.LongLength,
                    useV1Format ? assemblyResult.Program.LongLength : programSize + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                Console.WriteLine(Strings.CLI_Assemble_Result_Success, assemblyResult.Program.LongLength, Path.GetFullPath(destination),
                    useV1Format ? assemblyResult.Program.LongLength : assemblyResult.Program.LongLength + AAPFile.HeaderSize,
                    totalErrors, totalWarnings, totalSuggestions,
                    assemblyResult.AssembledLines, assemblyResult.AssembledFiles, assemblyStopwatch.Elapsed.TotalMilliseconds);
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
                args.Contains("--v1-format"), args.Contains("--v1-call-stack"), args.Contains("--ignore-newer-version"),
                !args.Contains("--unmapped-stack"));

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

            Assembler.AssemblyResult assemblyResult;
            try
            {
                assemblyResult = Assembler.AssembleLines(File.ReadAllLines(sourcePath), false,
                    // Ignore all warnings when using 'run' command
                    AssemblerWarnings.NonFatalErrorMessages.Keys.ToHashSet(),
                    AssemblerWarnings.WarningMessages.Keys.ToHashSet(),
                    AssemblerWarnings.SuggestionMessages.Keys.ToHashSet());
            }
            catch (Exception e)
            {
                OnAssemblerException(e);
                Environment.Exit(1);
                return;
            }

            Processor processor = new(
                memSize, assemblyResult.EntryPoint, useV1CallStack: args.Contains("--v1-call-stack"), mapStack: !args.Contains("--unmapped-stack"));
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
                args.Contains("--v1-format"), args.Contains("--v1-call-stack"), args.Contains("--ignore-newer-version"),
                !args.Contains("--unmapped-stack"));

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
            if (args.Contains("--v1-format"))
            {
                program = File.ReadAllBytes(sourcePath);
            }
            else
            {
                AAPFile file = LoadAAPFile(sourcePath, args.Contains("--ignore-newer-version"));
                program = file.Program;
            }

            try
            {
                disassembledProgram = Disassembler.DisassembleProgram(program, !args.Contains("--no-strings"), !args.Contains("--no-pads"));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Disassemble_Error_Unexpected, e.GetType().Name, e.Message);
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".dis.asm";
            File.WriteAllText(destination, disassembledProgram);
            Console.WriteLine(Strings.CLI_Disassemble_Success, Path.GetFullPath(destination));
        }

        private static void RunRepl(string[] args)
        {
            ulong memSize = GetMemorySize(args);
            Debugger debugger = new(true, memorySize: memSize, useV1CallStack: args.Contains("--v1-call-stack"),
                mapStack: !args.Contains("--unmapped-stack"));
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
                Assembler.AssemblyResult assemblyResult = Assembler.AssembleLines(File.ReadAllLines(sourcePath), false,
                    // Never ignore warnings when using 'lint' command
                    new HashSet<int>(), new HashSet<int>(), new HashSet<int>());
                Console.WriteLine(JsonSerializer.Serialize(assemblyResult.Warnings, new JsonSerializerOptions { IncludeFields = true }));
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
            Console.WriteLine(Strings.CLI_Help_Body, DefaultMemorySize);
        }
    }
}