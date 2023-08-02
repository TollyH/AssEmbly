namespace AssEmbly
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Version? version = typeof(Program).Assembly.GetName().Version;

            if (args.Contains("--version"))
            {
                Console.WriteLine(version?.ToString());
                return;
            }
            if (!args.Contains("--no-header"))
            {
                Console.WriteLine($"AssEmbly {version?.Major}.{version?.Minor}.{version?.Build} - A mock assembly language running on .NET");
                Console.WriteLine("Copyright © 2022-2023  Ptolemy Hill");
                Console.WriteLine();
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
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A path to the program listing to be assembled is required.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            // Change working directory to the location of the specified file.
            string parent = Path.GetDirectoryName(args[1])!;
            if (parent.Trim() != "")
            {
                Environment.CurrentDirectory = Path.GetFullPath(parent);
            }
            string filename = string.Join('.', args[1].Split('.')[..^1]);
            byte[] program;
            string debugInfo;
            try
            {
                program = Assembler.AssembleLines(File.ReadAllLines(args[1]), out debugInfo);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Data["UserMessage"]);
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".aap";
            File.WriteAllBytes(destination, program);

            if (!args.Contains("--no-debug-file"))
            {
                File.WriteAllText(destination + ".adi", debugInfo);
            }

            Console.WriteLine($"Program assembled into {program.LongLength} bytes successfully. It can be found at: \"{destination}\"");
        }

        private static void ExecuteProgram(string[] args)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A path to the assembled program to be executed is required.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            // Change working directory to the location of the specified file.
            string parent = Path.GetDirectoryName(args[1])!;
            if (parent.Trim() != "")
            {
                Environment.CurrentDirectory = Path.GetFullPath(parent);
            }
            ulong memSize = 2046;
            foreach (string a in args)
            {
                if (a.ToLowerInvariant().StartsWith("--mem-size="))
                {
                    memSize = ulong.Parse(a.Split("=")[1]);
                }
            }
            Processor processor = new(memSize);
            try
            {
                processor.LoadProgram(File.ReadAllBytes(args[1]));
                processor.Execute();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string message = e.GetType() == typeof(IndexOutOfRangeException) || e.GetType() == typeof(ArgumentOutOfRangeException)
                    ? "An instruction tried to access an invalid memory address." : e.Message;
                Console.WriteLine($"\n\nAn error occurred executing your program:\n    {message}\nRegister states:");
                foreach (int register in Enum.GetValues(typeof(Data.Register)))
                {
                    ulong value = processor.Registers[register];
                    Console.WriteLine($"    {Enum.GetName((Data.Register)register)}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                }
                Console.ResetColor();
            }
        }

        private static void AssembleAndExecute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A path to the program listing to be executed is required.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            // Change working directory to the location of the specified file.
            string parent = Path.GetDirectoryName(args[1])!;
            if (parent.Trim() != "")
            {
                Environment.CurrentDirectory = Path.GetFullPath(parent);
            }
            ulong memSize = 2046;
            foreach (string a in args)
            {
                if (a.ToLowerInvariant().StartsWith("--mem-size="))
                {
                    memSize = ulong.Parse(a.Split("=")[1]);
                }
            }
            Processor processor = new(memSize);
            byte[] program;
            try
            {
                program = Assembler.AssembleLines(File.ReadAllLines(args[1]), out _);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Data["UserMessage"]);
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            try
            {
                processor.LoadProgram(program);
                processor.Execute();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string message = e.GetType() == typeof(IndexOutOfRangeException) || e.GetType() == typeof(ArgumentOutOfRangeException)
                    ? "An instruction tried to access an invalid memory address." : e.Message;
                Console.WriteLine($"\n\nAn error occurred executing your program:\n    {message}\nRegister states:");
                foreach (int register in Enum.GetValues(typeof(Data.Register)))
                {
                    ulong value = processor.Registers[register];
                    Console.WriteLine($"    {Enum.GetName((Data.Register)register)}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                }
                Console.ResetColor();
            }
        }

        private static void RunDebugger(string[] args)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A path to the assembled program to be debugged is required.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            // Change working directory to the location of the specified file.
            string parent = Path.GetDirectoryName(args[1])!;
            if (parent.Trim() != "")
            {
                Environment.CurrentDirectory = Path.GetFullPath(parent);
            }
            ulong memSize = 2046;
            foreach (string a in args)
            {
                if (a.ToLowerInvariant().StartsWith("--mem-size="))
                {
                    memSize = ulong.Parse(a.Split("=")[1]);
                }
            }
            Processor processor = new(memSize);
            try
            {
                processor.LoadProgram(File.ReadAllBytes(args[1]));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\nAn error occurred loading your program:\n    {e.Message}");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }

            Debugger debugger = new(processor);
            if (args.Length >= 3 && !args[2].StartsWith('-'))
            {
                string debugFilePath = args[2];
                debugger.LoadDebugFile(debugFilePath);
            }
            debugger.StartDebugger();
        }

        private static void PerformDisassembly(string[] args)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A path to the program to be disassembled is required.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The specified file does not exist.");
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            // Change working directory to the location of the specified file.
            string parent = Path.GetDirectoryName(args[1])!;
            if (parent.Trim() != "")
            {
                Environment.CurrentDirectory = Path.GetFullPath(parent);
            }
            string filename = string.Join('.', args[1].Split('.')[..^1]);
            string disassembledProgram;
            try
            {
                disassembledProgram = Disassembler.DisassembleProgram(File.ReadAllBytes(args[1]), !args.Contains("--no-strings"), !args.Contains("--no-pads"));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Data["UserMessage"]);
                Console.ResetColor();
                Environment.Exit(1);
                return;
            }
            string destination = args.Length >= 3 && !args[2].StartsWith('-') ? args[2] : filename + ".dis.asm";
            File.WriteAllText(destination, disassembledProgram);
            Console.WriteLine($"Program disassembled successfully. It can be found at: \"{destination}\"");
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Usage: 'AssEmbly <operation> <required-parameters (if any)> [optional-parameters]'");
            Console.WriteLine("Any command can take the '--no-header' optional parameter to disable the copyright printout.");
            Console.WriteLine("Using the '--version' optional parameter will print just the current version of AssEmbly then exit, regardless of other parameters.");
            Console.WriteLine();
            Console.WriteLine("Operations:");
            Console.WriteLine("assemble - Take a program written in AssEmbly and assemble it down to executable bytecode");
            Console.WriteLine("    Usage: 'AssEmbly assemble <file-path> [destination-path] [--no-debug-file]'");
            Console.WriteLine("    --no-debug-file - Do not generate a debug information file with the executable.");
            Console.WriteLine();
            Console.WriteLine("execute - Execute an already assembled bytecode file");
            Console.WriteLine("    Usage: 'AssEmbly execute <file-path> [--mem-size=2046]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine();
            Console.WriteLine("run - Assemble then execute a program written is AssEmbly. Assembled bytes will be discarded after execution.");
            Console.WriteLine("    Usage: 'AssEmbly run <file-path> [--mem-size=2046]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine();
            Console.WriteLine("debug - Step through an assembled bytecode file, pausing before each instruction begins execution.");
            Console.WriteLine("    Usage: 'AssEmbly debug <file-path> [debug-info-file-path] [--mem-size=2046]'");
            Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
            Console.WriteLine("    Memory size will be 2046 bytes if parameter is not given.");
            Console.WriteLine("    Providing a debug info file will allow label names and original AssEmbly source lines to be made available.");
            Console.WriteLine();
            Console.WriteLine("disassemble - Generate an AssEmbly program listing from already assembled bytecode.");
            Console.WriteLine("    Usage: 'AssEmbly disassemble <file-path> [destination-path] [--no-strings|--no-pads]'");
            Console.WriteLine("    --no-strings - Don't attempt to locate and decode strings; keep them as raw bytes");
            Console.WriteLine("    --no-pads - Don't attempt to locate uses of the PAD directive; keep them as chains of HLT");
            Console.WriteLine();
        }
    }
}