namespace AssEmbly
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!args.Contains("--no-header"))
            {
                Console.WriteLine("AssEmbly - A mock assembly language running on .NET");
                Console.WriteLine("Copyright © 2022  Ptolemy Hill");
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
                #region Assemble
                case "assemble":
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
                    Environment.CurrentDirectory = Path.GetDirectoryName(args[1])!;
                    string filename = string.Join('.', args[1].Split('.')[..^1]);
                    byte[] program;
                    try
                    {
                        program = Assembler.AssembleLines(File.ReadAllLines(args[1]));
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Data["UserMessage"]);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return;
                    }
                    string destination = args.Length >= 3 ? args[2] : filename + ".aap";
                    File.WriteAllBytes(destination, program);
                    Console.WriteLine($"Program assembled into {program.LongLength} bytes successfully. It can be found at: \"{destination}\"");
                    break;
                #endregion
                #region Execute
                case "execute":
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
                    Environment.CurrentDirectory = Path.GetDirectoryName(args[1])!;
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
                        Console.WriteLine($"\n\nAn error occurred executing your program:\n    {e.Message}\nRegister states:");
                        foreach ((Data.Register register, ulong value) in processor.Registers)
                        {
                            Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                        }
                        Console.ResetColor();
                    }
                    break;
                #endregion
                #region Run
                case "run":
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
                    Environment.CurrentDirectory = Path.GetDirectoryName(args[1])!;
                    memSize = 2046;
                    foreach (string a in args)
                    {
                        if (a.ToLowerInvariant().StartsWith("--mem-size="))
                        {
                            memSize = ulong.Parse(a.Split("=")[1]);
                        }
                    }
                    processor = new(memSize);
                    try
                    {
                        program = Assembler.AssembleLines(File.ReadAllLines(args[1]));
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
                        Console.WriteLine($"\n\nAn error occurred running your program:\n    {e.Message}\nRegister states:");
                        foreach ((Data.Register register, ulong value) in processor.Registers)
                        {
                            Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                        }
                        Console.ResetColor();
                    }
                    break;
                #endregion
                #region Debug
                case "debug":
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
                    Environment.CurrentDirectory = Path.GetDirectoryName(args[1])!;
                    memSize = 2046;
                    foreach (string a in args)
                    {
                        if (a.ToLowerInvariant().StartsWith("--mem-size="))
                        {
                            memSize = ulong.Parse(a.Split("=")[1]);
                        }
                    }
                    processor = new(memSize);
                    try
                    {
                        processor.LoadProgram(File.ReadAllBytes(args[1]));
                        while (true)
                        {
                            void DisplayDebugInfo()
                            {
                                Console.Write($"\n\nAbout to execute instruction:\n    ");
                                Console.WriteLine(Disassembler.DisassembleInstruction(
                                    processor.Memory[(int)processor.Registers[Data.Register.rpo]..]).Item1);
                                Console.WriteLine();
                                Console.WriteLine("Register states:");
                                foreach ((Data.Register register, ulong value) in processor.Registers)
                                {
                                    Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                                }
                            }
                            DisplayDebugInfo();
                            bool endLoop = false;
                            while (!endLoop)
                            {
                                Console.Write("\nPress ENTER to continue, or type a command: ");
                                string[] command = Console.ReadLine()!.Trim().ToLower().Split(' ');
                                switch (command[0])
                                {
                                    case "":
                                        break;
                                    case "refresh":
                                        DisplayDebugInfo();
                                        continue;
                                    #region Debug Read
                                    case "read":
                                        if (command.Length == 3)
                                        {
                                            ulong bytesToRead = command[1] == "byte" ? 1 : command[1] == "word"
                                                ? 2 : command[1] == "dword" ? 4 : command[1] == "qword" ? 8 : 0U;
                                            if (bytesToRead == 0)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"\"{command[1]}\" is not a valid size specifier. Run 'help' for more info.");
                                                Console.ResetColor();
                                                continue;
                                            }
                                            if (!ulong.TryParse(command[2], out ulong address))
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"\"{command[2]}\" is not a valid memory address. Run 'help' for more info.");
                                                Console.ResetColor();
                                                continue;
                                            }
                                            if (address + bytesToRead > (ulong)processor.Memory.LongLength || address < 0)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"\"{command[2]}\" is outside the range of allowed memory addresses.");
                                                Console.ResetColor();
                                                continue;
                                            }
                                            ulong value = 0;
                                            for (ulong i = address; i < address + bytesToRead; i++)
                                            {
                                                value += (ulong)processor.Memory[i] << (byte)((i - address) * 8);
                                            }
                                            Console.WriteLine($"Value at {address}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command requires 2 arguments. Run 'help' for more info.");
                                            Console.ResetColor();
                                        }
                                        continue;
                                    #endregion
                                    #region Debug Write
                                    case "write":
                                        if (command.Length == 4)
                                        {
                                            if (command[1] == "mem")
                                            {
                                                if (!ulong.TryParse(command[2], out ulong address))
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"\"{command[2]}\" is not a valid memory address. Run 'help' for more info.");
                                                    Console.ResetColor();
                                                    continue;
                                                }
                                                if (address >= (ulong)processor.Memory.LongLength || address < 0)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"\"{command[2]}\" is outside the range of allowed memory addresses.");
                                                    Console.ResetColor();
                                                    continue;
                                                }
                                                if (!byte.TryParse(command[3], out byte value))
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"\"{command[3]}\" is not a valid byte value for memory.");
                                                    Console.ResetColor();
                                                    continue;
                                                }
                                                processor.Memory[address] = value;
                                                Console.WriteLine($"Successfully set value of address {address} to {value}");
                                            }
                                            else if (command[1] == "reg")
                                            {
                                                if (!Enum.TryParse(command[2], out Data.Register register))
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"\"{command[2]}\" is not a valid register. Run 'help' for more info.");
                                                    Console.ResetColor();
                                                    continue;
                                                }
                                                if (!ulong.TryParse(command[3], out ulong value))
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine($"\"{command[3]}\" is not a valid value for register.");
                                                    Console.ResetColor();
                                                    continue;
                                                }
                                                processor.Registers[register] = value;
                                                Console.WriteLine($"Successfully set value of register {register} to {value}");
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"\"{command[1]}\" is not a valid location specifier. Run 'help' for more info.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command requires 3 arguments. Run 'help' for more info.");
                                            Console.ResetColor();
                                        }
                                        continue;
                                    #endregion
                                    #region Debug Map
                                    case "map":
                                        ulong limit = ulong.MaxValue;
                                        if (command.Length == 2)
                                        {
                                            if (!ulong.TryParse(command[1], out limit))
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine($"\"{command[1]}\" is not a valid limit. Run 'help' for more info.");
                                                Console.ResetColor();
                                                continue;
                                            }
                                        }
                                        else if (command.Length != 1)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command requires 0 or 1 arguments. Run 'help' for more info.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        Console.Write("Offset (Hex)     │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F\n" +
                                            "─────────────────┼────────────────────────────────────────────────");
                                        ulong min = (ulong)processor.Memory.LongLength < limit ? (ulong)processor.Memory.LongLength : limit;
                                        for (ulong i = 0; i < min; i++)
                                        {
                                            if (i % 16 == 0)
                                            {
                                                Console.Write($"\n{i:X16} │");
                                            }
                                            if (i == processor.Registers[Data.Register.rso])
                                            {
                                                Console.ForegroundColor = ConsoleColor.Green;
                                            }
                                            if (i == processor.Registers[Data.Register.rsb])
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                            }
                                            if (i == processor.Registers[Data.Register.rpo])
                                            {
                                                Console.ForegroundColor = ConsoleColor.Blue;
                                            }
                                            Console.Write($" {processor.Memory[i]:X2}");
                                            Console.ResetColor();
                                        }
                                        Console.WriteLine();
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.Write("rpo");
                                        Console.ResetColor();
                                        Console.Write(", ");
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write("rsb");
                                        Console.ResetColor();
                                        Console.Write(", ");
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("rso");
                                        Console.ResetColor();
                                        Console.WriteLine();
                                        continue;
                                    #endregion
                                    #region Debug Stack
                                    case "stack":
                                        if (command.Length != 1)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command does not take any arguments. Run 'help' for more info.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        if (processor.Registers[Data.Register.rso] >= (ulong)processor.Memory.LongLength)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                                            Console.WriteLine("The stack is currently empty.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        if (processor.Registers[Data.Register.rso] > processor.Registers[Data.Register.rsb])
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                                            Console.WriteLine("The stack pointer is currently greater than the stack base - stack visualisation not available in this state.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        ulong currentStackOffset = processor.Registers[Data.Register.rso];
                                        ulong currentStackBase = processor.Registers[Data.Register.rsb];
                                        for (ulong i = currentStackOffset; i < currentStackBase; i += 8)
                                        {
                                            if (i == currentStackOffset)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("Current stack frame (likely local variables)");
                                                Console.ResetColor();
                                                Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                                            }
                                            Console.Write($"│ {i:X16} │ {processor.MemReadQWord(i):X16}              │ rsb - {currentStackBase - i,-4} │");
                                            if (i == currentStackOffset)
                                            {
                                                Console.WriteLine(" <- rsp");
                                            }
                                            else
                                            {
                                                Console.WriteLine();
                                            }
                                            if (i + 8 >= currentStackBase)
                                            {
                                                Console.WriteLine("└──────────────────┴───────────────────────────────┴────────────┘");
                                            }
                                        }
                                        if (currentStackBase + 16 < (ulong)processor.Memory.LongLength)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.WriteLine("Return information");
                                            Console.ResetColor();
                                            Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                                            Console.WriteLine($"│ {currentStackBase:X16} │ Reset rso to {processor.MemReadQWord(currentStackBase):X16} | rsb + 0    | <- rsb");
                                            Console.WriteLine($"│ {currentStackBase + 8:X16} │ Reset rsb to {processor.MemReadQWord(currentStackBase + 8):X16} | rsb + 8    |");
                                            Console.WriteLine($"│ {currentStackBase + 16:X16} │ Reset rpo to {processor.MemReadQWord(currentStackBase + 16):X16} | rsb + 16   |");
                                            Console.WriteLine("└──────────────────┴───────────────────────────────┴────────────┘");

                                            ulong parentStackBase = processor.MemReadQWord(currentStackBase + 8);
                                            for (ulong i = currentStackBase + 24; i < parentStackBase; i += 8)
                                            {
                                                if (i == currentStackBase + 24)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("Parent stack frame (possibly parameters to this subroutine)");
                                                    Console.ResetColor();
                                                    Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                                                }
                                                Console.WriteLine($"│ {i:X16} │ {processor.MemReadQWord(i):X16}              │ rsb + {i - currentStackBase,-4} │");
                                                if (i + 8 >= parentStackBase)
                                                {
                                                    Console.WriteLine("└──────────────────┴───────────────────────────────┴────────────┘");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                                            Console.WriteLine("Bottom of the stack reached - most likely the program is not currently in a subroutine.");
                                            Console.ResetColor();
                                        }
                                        continue;
                                    #endregion
                                    #region Debug Dec to Hex
                                    case "dec2hex":
                                        if (command.Length != 2)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command requires 1 argument. Run 'help' for more info.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        if (!ulong.TryParse(command[1], out ulong decValue))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"\"{command[1]}\" is not a valid value to convert.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        Console.WriteLine($"{decValue} in hexadecimal is {decValue:X}");
                                        continue;
                                    #endregion
                                    #region Debug Hex to Dec
                                    case "hex2dec":
                                        if (command.Length != 2)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("This command requires 1 argument. Run 'help' for more info.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        try
                                        {
                                            ulong convertedValue = Convert.ToUInt64(command[1], 16);
                                            Console.WriteLine($"{command[1].ToUpperInvariant()} in decimal is {convertedValue}");
                                        }
                                        catch
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"\"{command[1]}\" is not a valid value to convert.");
                                            Console.ResetColor();
                                            continue;
                                        }
                                        continue;
                                    #endregion
                                    #region Debug Help
                                    case "help":
                                        Console.WriteLine("\nread <byte|word|dword|qword> <address> - Read data at a memory address");
                                        Console.WriteLine("write <mem|reg> <address|register-name> <value> - Modify the value of a memory address or register");
                                        Console.WriteLine("map [limit] - Display all (optionally limited amount) of memory in a grid of bytes");
                                        Console.WriteLine("stack - Visualise the state of the stack");
                                        Console.WriteLine("dec2hex <dec-number> - Convert a decimal number to hexadecimal");
                                        Console.WriteLine("hex2dec <hex-number> - Convert a hexadecimal number to decimal");
                                        Console.WriteLine("refresh - Display the instruction to be executed and register states again");
                                        continue;
                                    #endregion
                                    default:
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\"{command[0]}\" is not a recognised command. Run 'help' for more info.");
                                        Console.ResetColor();
                                        continue;
                                }
                                endLoop = true;
                            }
                            if (processor.Step())
                            {
                                break;
                            }
                        }
                        Console.WriteLine("\n\nHalt instruction reached. Execution finished.");
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n\nAn error occurred executing your program:\n    {e.Message}\nRegister states:");
                        foreach ((Data.Register register, ulong value) in processor.Registers)
                        {
                            Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                        }
                        Console.ResetColor();
                    }
                    break;
                #endregion
                #region Disassemble
                case "disassemble":
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
                    Environment.CurrentDirectory = Path.GetDirectoryName(args[1])!;
                    filename = string.Join('.', args[1].Split('.')[..^1]);
                    string disassembledProgram;
                    try
                    {
                        disassembledProgram = Disassembler.DisassembleProgram(File.ReadAllBytes(args[1]), !args.Contains("--no-strings"));
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Data["UserMessage"]);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return;
                    }
                    destination = args.Length >= 3 && args[2] != "--no-strings" ? args[2] : filename + ".dis.asm";
                    File.WriteAllText(destination, disassembledProgram);
                    Console.WriteLine($"Program disassembled successfully. It can be found at: \"{destination}\"");
                    break;
                #endregion
                #region Help
                case "help":
                    Console.WriteLine("Usage: 'AssEmbly <operation> <required-parameters (if any)> [optional-parameters]'");
                    Console.WriteLine("Any command can take the '--no-header' optional parameter to disable the copyright printout.");
                    Console.WriteLine();
                    Console.WriteLine("Operations:");
                    Console.WriteLine("assemble - Take a program written in AssEmbly and assemble it down to executable bytecode");
                    Console.WriteLine("    Usage: 'AssEmbly assemble <file-path> [destination-path]'");
                    Console.WriteLine();
                    Console.WriteLine("execute - Execute an already assembled bytecode file");
                    Console.WriteLine("    Usage: 'AssEmbly execute <file-path> [--mem-size=2046]'");
                    Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
                    Console.WriteLine("    Memory size will be 2046 bytes if paramter is not given.");
                    Console.WriteLine();
                    Console.WriteLine("run - Assemble then execute a program written is AssEmbly. Assembled bytes will be discarded after execution.");
                    Console.WriteLine("    Usage: 'AssEmbly run <file-path> [--mem-size=2046]'");
                    Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
                    Console.WriteLine("    Memory size will be 2046 bytes if paramter is not given.");
                    Console.WriteLine();
                    Console.WriteLine("debug - Step through an assembled bytecode file, pausing before each instruction begins execution.");
                    Console.WriteLine("    Usage: 'AssEmbly debug <file-path> [--mem-size=2046]'");
                    Console.WriteLine("    --mem-size=2046 - Sets the total size of memory available to the program in bytes.");
                    Console.WriteLine("    Memory size will be 2046 bytes if paramter is not given.");
                    Console.WriteLine();
                    Console.WriteLine("disassemble - Generate an AssEmbly program listing from already assembled bytecode.");
                    Console.WriteLine("    Usage: 'AssEmbly disassemble <file-path> [destination-path] [--no-strings]'");
                    Console.WriteLine("    --no-strings - Don't attempt to locate and decode strings; keep them as raw bytes");
                    Console.WriteLine();
                    break;
                #endregion
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{args[0]}\" is not a valid operation.");
                    Console.ResetColor();
                    Environment.Exit(1);
                    return;
            }
        }
    }
}