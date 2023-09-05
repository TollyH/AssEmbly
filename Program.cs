using System.Text.Json;
using System.Web;

namespace AssEmbly
{
    internal static partial class Program
    {
        internal static Version? version = typeof(Program).Assembly.GetName().Version;

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
                Console.Error.WriteLine("Copyright © 2022-2023  Ptolemy Hill");
                Console.Error.WriteLine();
            }
            if (args.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An operation to perform is required. Run the 'help' operation for information on available operations.");
                Console.WriteLine("i.e. 'AssEmbly help'");
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
                    Console.WriteLine($"\"{args[0]}\" is not a valid operation.");
                    Console.ResetColor();
                    Environment.Exit(1);
                    return;
            }
        }

        private static void AssembleSourceFile(string[] args)
        {
            if (!CheckInputFileArg(args, "A path to the program listing to be assembled is required."))
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
                        Console.WriteLine($"{a[16..]} is not a valid error code to disable.");
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
                        Console.WriteLine($"{a[18..]} is not a valid warning code to disable.");
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
                        Console.WriteLine($"{a[21..]} is not a valid suggestion code to disable.");
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

            byte[] program;
            string debugInfo;
            ulong entryPoint;
            AAPFeatures extensionFeatures;
            try
            {
                program = Assembler.AssembleLines(File.ReadAllLines(sourcePath),
                    useV1Format,
                    disabledErrors, disabledWarnings, disabledSuggestions,
                    out debugInfo, out List<Warning> warnings, out entryPoint,
                    out extensionFeatures);
                foreach (Warning warning in warnings)
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
                        WarningSeverity.NonFatalError => "Error",
                        WarningSeverity.Warning => "Warning",
                        WarningSeverity.Suggestion => "Suggestion",
                        _ => "Unknown"
                    };
                    Console.WriteLine(
                        $"\n{messageStart} {warning.Code:D4} on line {warning.Line} in {(warning.File == "" ? "base file" : warning.File)}" +
                        $"\n    {warning.OriginalLine}" +
                        $"\n{warning.Message}");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                OnAssemblerException(e);
                Environment.Exit(1);
                return;
            }

            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".aap";
            if (useV1Format)
            {
                File.WriteAllBytes(destination, program);
            }
            else
            {
                AAPFeatures features = extensionFeatures;
                if (args.Contains("--v1-call-stack"))
                {
                    features |= AAPFeatures.V1CallStack;
                }
                AAPFile executable = new(version ?? new Version(), features, entryPoint, program);
                File.WriteAllBytes(destination, executable.GetBytes());
            }

            if (!args.Contains("--no-debug-file"))
            {
                File.WriteAllText(destination + ".adi", debugInfo);
            }

            Console.WriteLine($"Program assembled into {program.LongLength} bytes successfully. It can be found at: \"{Path.GetFullPath(destination)}\"");
        }

        private static void ExecuteProgram(string[] args)
        {
            if (!CheckInputFileArg(args, "A path to the assembled program to be executed is required."))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            Processor? processor = LoadExecutableToProcessor(appPath, memSize, args.Contains("--v1-format"), args.Contains("--v1-call-stack"));
            if (processor is null)
            {
                return;
            }

            ExecuteProcessor(processor);
        }

        private static void AssembleAndExecute(string[] args)
        {
            if (!CheckInputFileArg(args, "A path to the program listing to be executed is required."))
            {
                return;
            }

            (string sourcePath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            byte[] program;
            ulong entryPoint;
            try
            {
                program = Assembler.AssembleLines(File.ReadAllLines(sourcePath), false,
                    // Ignore all warnings when using 'run' command
                    AssemblerWarnings.NonFatalErrorMessages.Keys.ToHashSet(),
                    AssemblerWarnings.WarningMessages.Keys.ToHashSet(),
                    AssemblerWarnings.SuggestionMessages.Keys.ToHashSet(),
                    out _, out _, out entryPoint, out _);
            }
            catch (Exception e)
            {
                OnAssemblerException(e);
                Environment.Exit(1);
                return;
            }

            Processor processor = new(memSize, entryPoint, useV1CallStack: args.Contains("--v1-call-stack"));
            LoadProgramIntoProcessor(processor, program);
            ExecuteProcessor(processor);
        }

        private static void RunDebugger(string[] args)
        {
            if (!CheckInputFileArg(args, "A path to the assembled program to be debugged is required."))
            {
                return;
            }

            (string appPath, _) = ResolveInputFilePath(args);

            ulong memSize = GetMemorySize(args);

            Processor? processor = LoadExecutableToProcessor(appPath, memSize, args.Contains("--v1-format"), args.Contains("--v1-call-stack"));
            if (processor is null)
            {
                return;
            }

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
            if (!CheckInputFileArg(args, "A path to the program to be disassembled is required."))
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
                AAPFile? file = LoadAAPFile(sourcePath);
                if (file is null)
                {
                    return;
                }
                program = file.Program;
            }

            try
            {
                disassembledProgram = Disassembler.DisassembleProgram(program, !args.Contains("--no-strings"), !args.Contains("--no-pads"));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred during disassembly:\n    {e.GetType().Name}: {e.Message}");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".dis.asm";
            File.WriteAllText(destination, disassembledProgram);
            Console.WriteLine($"Program disassembled successfully. It can be found at: \"{Path.GetFullPath(destination)}\"");
        }

        private static void RunRepl(string[] args)
        {
            ulong memSize = GetMemorySize(args);
            Debugger debugger = new(true, memorySize: memSize, useV1CallStack: args.Contains("--v1-call-stack"));
            // Some program needs to be loaded or the processor won't run
            debugger.DebuggingProcessor.LoadProgram(Array.Empty<byte>());
            debugger.StartDebugger();
        }

        // Suppress warning when unused code trimming is used with JSON serialization
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("", "IL2026",
            Justification = "Only the Warning struct is serialized, which contains only primitive types.")]
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
                _ = Assembler.AssembleLines(File.ReadAllLines(sourcePath), false,
                    // Never ignore warnings when using 'lint' command
                    new HashSet<int>(), new HashSet<int>(), new HashSet<int>(),
                    out _, out List<Warning> warnings, out _, out _);
                Console.WriteLine(JsonSerializer.Serialize(warnings, new JsonSerializerOptions { IncludeFields = true }));
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
            Console.WriteLine("Usage: 'AssEmbly <operation> <required-parameters (if any)> [optional-parameters]'");
            Console.WriteLine("Any command can take the '--no-header' optional parameter to disable the copyright printout.");
            Console.WriteLine("Using the '--version' optional parameter will print just the current version of AssEmbly then exit, regardless of other parameters.");
            Console.WriteLine();
            Console.WriteLine("Operations:");
            Console.WriteLine("assemble - Take a source file written in AssEmbly and assemble it in to an executable file");
            Console.WriteLine("    Usage: 'AssEmbly assemble <file-path> [destination-path] [options]'");
            Console.WriteLine("    --no-debug-file - Do not generate a debug information file with the executable.");
            Console.WriteLine("    --no-errors|warnings|suggestions - Disable all messages with severity error, warning, or suggestion. Fatal errors cannot be disabled.");
            Console.WriteLine("    --disable-error|warning|suggestion-xxxx - Disable a specific message with severity error, warning, or suggestion; and code xxxx. Fatal errors cannot be disabled.");
            Console.WriteLine("    --v1-call-stack - Specify that the program expects to use the old call stack behaviour from AssEmbly v1.x.x. Does not affect the program bytecode, but will set a flag in the header of the executable.");
            Console.WriteLine("    --v1-format - Force the generated executable to be in the header-less format from v1.x.x.");
            Console.WriteLine();
            Console.WriteLine("execute - Execute an already assembled executable file");
            Console.WriteLine("    Usage: 'AssEmbly execute <file-path> [options]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    --v1-call-stack - Use the old call stack behaviour from AssEmbly v1.x.x which pushes 3 registers when calling instead of 2.");
            Console.WriteLine("    --v1-format - Specifies that the given executable uses the v1.x.x header-less format. Also enables --v1-call-stack");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine();
            Console.WriteLine("run - Assemble then execute a source file written in AssEmbly. The assembled program will be discarded after execution.");
            Console.WriteLine("    Usage: 'AssEmbly run <file-path> [options]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    --v1-call-stack - Use the old call stack behaviour from AssEmbly v1.x.x which pushes 3 registers when calling instead of 2.");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine();
            Console.WriteLine("debug - Step through an assembled executable file, pausing before each instruction begins execution.");
            Console.WriteLine("    Usage: 'AssEmbly debug <file-path> [debug-info-file-path] [options]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    --v1-call-stack - Use the old call stack behaviour from AssEmbly v1.x.x which pushes 3 registers when calling instead of 2.");
            Console.WriteLine("    --v1-format - Specifies that the given executable uses the v1.x.x header-less format. Also enables --v1-call-stack");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine("    Providing a debug info file will allow label names and original AssEmbly source lines to be made available.");
            Console.WriteLine();
            Console.WriteLine("disassemble - Generate an AssEmbly program listing from an already assembled executable.");
            Console.WriteLine("    Usage: 'AssEmbly disassemble <file-path> [destination-path] [options]'");
            Console.WriteLine("    --no-strings - Don't attempt to locate and decode strings; keep them as raw bytes");
            Console.WriteLine("    --no-pads - Don't attempt to locate uses of the PAD directive; keep them as chains of HLT");
            Console.WriteLine("    --v1-format - Specifies that the given executable uses the v1.x.x header-less format.");
            Console.WriteLine();
            Console.WriteLine("repl - Run an AssEmbly REPL environment that lets you interactively run AssEmbly instructions.");
            Console.WriteLine("    Usage: 'AssEmbly repl [options]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the REPL in bytes.");
            Console.WriteLine("    --v1-call-stack - Use the old call stack behaviour from AssEmbly v1.x.x which pushes 3 registers when calling instead of 2.");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine();
        }
    }
}