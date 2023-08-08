﻿namespace AssEmbly
{
    public class Debugger
    {
        public Processor DebuggingProcessor { get; set; }
        public DebugInfo.DebugInfoFile? LoadedDebugInfoFile { get; set; }

        public bool StepInstructions { get; set; } = true;
        public bool RunToReturn { get; set; } = false;
        public ulong? StepOverStackBase { get; set; } = null;

        public List<(Data.Register Register, ulong Value)> Breakpoints { get; set; } = new();

        public Debugger()
        {
            DebuggingProcessor = new(2046);
        }

        public Debugger(ulong memorySize)
        {
            DebuggingProcessor = new(memorySize);
        }

        public Debugger(Processor processorToDebug)
        {
            DebuggingProcessor = processorToDebug;
        }

        public void LoadDebugFile(string debugFilePath)
        {
            try
            {
                string debugInfoText = File.ReadAllText(debugFilePath);
                LoadedDebugInfoFile = DebugInfo.ParseDebugInfoFile(debugInfoText);
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"An error occurred whilst loading the debug information file:\n\"{exc.GetType().Name}: {exc.Message}\".\n" +
                    $"Label names and original source lines will not be available.");
                Console.ResetColor();
            }
        }

        public void DisplayDebugInfo()
        {
            ulong currentAddress = DebuggingProcessor.Registers[(int)Data.Register.rpo];
            // Disassemble line on-the-fly, unless a provided debugging file provides the original text for the line
            string lineDisassembly = LoadedDebugInfoFile is null
                || !LoadedDebugInfoFile.Value.AssembledInstructions.TryGetValue(currentAddress, out string? inst)
                    ? Disassembler.DisassembleInstruction(DebuggingProcessor.Memory.AsSpan()[(int)currentAddress..]).Line
                    : inst;

            Console.Write($"\n\nAbout to execute instruction:\n    ");
            Console.WriteLine(lineDisassembly);
            Console.WriteLine();
            if (LoadedDebugInfoFile is not null)
            {
                if (LoadedDebugInfoFile.Value.AddressLabels.TryGetValue(currentAddress, out string[]? labels))
                {
                    Console.Write("This address is referenced by the following labels:\n    ");
                    Console.WriteLine(string.Join("\n    ", labels));
                    Console.WriteLine();
                }
                if (LoadedDebugInfoFile.Value.ImportLocations.TryGetValue(currentAddress, out string? importName))
                {
                    Console.Write("The following file was imported here:\n    ");
                    Console.WriteLine(importName);
                    Console.WriteLine();
                }
            }
            Console.WriteLine("Register states:");
            foreach (int register in Enum.GetValues(typeof(Data.Register)))
            {
                ulong value = DebuggingProcessor.Registers[register];
                Console.WriteLine($"    {Enum.GetName((Data.Register)register)}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
            }
        }

        public void StartDebugger()
        {
            try
            {
                while (true)
                {
                    // Only pause for debugging instruction if not running to break, not in a deeper subroutine than we were if stepping over,
                    // and aren't waiting for a return instruction
                    bool breakForDebug = StepInstructions && (StepOverStackBase is null || DebuggingProcessor.Registers[(int)Data.Register.rsb] >= StepOverStackBase)
                    // Is the next instruction a return instruction?
                        && (!RunToReturn || DebuggingProcessor.Memory[DebuggingProcessor.Registers[(int)Data.Register.rpo]] is 0xBA or 0xBB or 0xBC or 0xBD or 0xBE);

                    foreach ((Data.Register register, ulong value) in Breakpoints)
                    {
                        if (DebuggingProcessor.Registers[(int)register] == value)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write($"Breakpoint hit! {register} == {value}");
                            Console.ResetColor();
                            breakForDebug = true;
                            break;
                        }
                    }

                    if (breakForDebug)
                    {
                        StepInstructions = true;
                        RunToReturn = false;
                        StepOverStackBase = null;
                        DisplayDebugInfo();
                    }
                    bool endCommandEntryLoop = false;
                    while (!endCommandEntryLoop && breakForDebug)
                    {
                        Console.Write("\nPress ENTER to continue, or type a command ('help' for command list): ");
                        string[] command = Console.ReadLine()!.Trim().ToLower().Split(' ');
                        switch (command[0])
                        {
                            case "":
                                endCommandEntryLoop = true;
                                break;
                            case "refresh":
                                DisplayDebugInfo();
                                break;
                            case "run":
                                endCommandEntryLoop = true;
                                StepInstructions = false;
                                break;
                            case "over":
                                endCommandEntryLoop = true;
                                StepOverStackBase = DebuggingProcessor.Registers[(int)Data.Register.rsb];
                                break;
                            case "return":
                                endCommandEntryLoop = true;
                                StepOverStackBase = DebuggingProcessor.Registers[(int)Data.Register.rsb];
                                RunToReturn = true;
                                break;
                            case "read":
                                CommandReadMemory(command);
                                break;
                            case "write":
                                CommandWriteMemReg(command);
                                break;
                            case "map":
                                CommandMapMemory(command);
                                break;
                            case "stack":
                                CommandFormatStack(command);
                                break;
                            case "dec2hex":
                                CommandDecimalToHexadecimal(command);
                                break;
                            case "hex2dec":
                                CommandHexadecimalToDecimal(command);
                                break;
                            case "breakpoint":
                                CommandBreakpointManage(command);
                                break;
                            case "help":
                                CommandDebugHelp();
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\"{command[0]}\" is not a recognised command. Run 'help' for more info.");
                                Console.ResetColor();
                                break;
                        }
                    }
                    if (DebuggingProcessor.Step())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("\n\nHalt instruction reached. You should not continue unless this instruction was placed as a breakpoint.");
                        Console.ResetColor();
                        Console.Write("Press any key to continue, or CTRL+C to stop...");
                        _ = Console.ReadKey();
                        Console.WriteLine();
                        StepInstructions = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (e is IndexOutOfRangeException or ArgumentOutOfRangeException or RuntimeException or DivideByZeroException)
                {
                    string message = e is RuntimeException runtimeException
                        ? runtimeException.ConsoleMessage
                        : e is DivideByZeroException
                            ? "An instruction attempted to divide by zero."
                            : "An instruction tried to access an invalid memory address.";
                    Console.WriteLine($"\n\nAn error occurred executing your program:\n    {message}\nRegister states:");
                    foreach (int register in Enum.GetValues(typeof(Data.Register)))
                    {
                        ulong value = DebuggingProcessor.Registers[register];
                        Console.WriteLine($"    {Enum.GetName((Data.Register)register)}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                    }
                }
                else
                {
                    Console.WriteLine($"An unexpected error occurred:\r\n    {e.GetType().Name}: {e.Message}");
                }
                Console.ResetColor();
            }
        }

        private void CommandReadMemory(string[] command)
        {
            if (command.Length == 3)
            {
                ulong bytesToRead = command[1] == "byte" ? 1 : command[1] == "word"
                    ? 2 : command[1] == "dword" ? 4 : command[1] == "qword" ? 8 : 0U;
                if (bytesToRead == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[1]}\" is not a valid size specifier. Run 'help' for more info.");
                    Console.ResetColor();
                    return;
                }
                if (!ulong.TryParse(command[2], out ulong address))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[2]}\" is not a valid memory address. Run 'help' for more info.");
                    Console.ResetColor();
                    return;
                }
                if (address + bytesToRead > (ulong)DebuggingProcessor.Memory.LongLength || address < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[2]}\" is outside the range of allowed memory addresses.");
                    Console.ResetColor();
                    return;
                }
                ulong value = 0;
                for (ulong i = address; i < address + bytesToRead; i++)
                {
                    value += (ulong)DebuggingProcessor.Memory[i] << (byte)((i - address) * 8);
                }
                Console.WriteLine($"Value at {address}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command requires 2 arguments. Run 'help' for more info.");
                Console.ResetColor();
            }
        }

        private void CommandWriteMemReg(string[] command)
        {
            if (command.Length == 4)
            {
                if (command[1] == "mem")
                {
                    if (!ulong.TryParse(command[2], out ulong address))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[2]}\" is not a valid memory address. Run 'help' for more info.");
                        Console.ResetColor();
                        return;
                    }
                    if (address >= (ulong)DebuggingProcessor.Memory.LongLength || address < 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[2]}\" is outside the range of allowed memory addresses.");
                        Console.ResetColor();
                        return;
                    }
                    if (!byte.TryParse(command[3], out byte value))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[3]}\" is not a valid byte value for memory.");
                        Console.ResetColor();
                        return;
                    }
                    DebuggingProcessor.Memory[address] = value;
                    Console.WriteLine($"Successfully set value of address {address} to {value}");
                }
                else if (command[1] == "reg")
                {
                    if (!Enum.TryParse(command[2], out Data.Register register))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[2]}\" is not a valid register. Run 'help' for more info.");
                        Console.ResetColor();
                        return;
                    }
                    if (!ulong.TryParse(command[3], out ulong value))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[3]}\" is not a valid value for register.");
                        Console.ResetColor();
                        return;
                    }
                    DebuggingProcessor.Registers[(int)register] = value;
                    Console.WriteLine($"Successfully set value of register {Enum.GetName(register)} to {value}");
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
        }

        private void CommandMapMemory(string[] command)
        {
            ulong offset = 0;
            ulong limit = uint.MaxValue;
            if (command.Length >= 2)
            {
                if (!ulong.TryParse(command[1], out offset))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[1]}\" is not a valid offset. Run 'help' for more info.");
                    Console.ResetColor();
                    return;
                }
            }
            if (command.Length == 3)
            {
                if (!ulong.TryParse(command[2], out limit))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[1]}\" is not a valid limit. Run 'help' for more info.");
                    Console.ResetColor();
                    return;
                }
            }
            if (command.Length is not 1 and > 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command requires 0 to 2 arguments. Run 'help' for more info.");
                Console.ResetColor();
                return;
            }
            Console.Write("Offset (Hex)     │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F\n" +
                "─────────────────┼────────────────────────────────────────────────");
            ulong start = offset - (offset % 16);  // Ensure offset is a multiple of 16
            ulong end = (ulong)DebuggingProcessor.Memory.LongLength < (limit + start) ? (ulong)DebuggingProcessor.Memory.LongLength : (limit + start);
            for (ulong i = start; i < end; i++)
            {
                if (i % 16 == 0)
                {
                    Console.Write($"\n{i:X16} │");
                }
                if (i == DebuggingProcessor.Registers[(int)Data.Register.rso])
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (i == DebuggingProcessor.Registers[(int)Data.Register.rsb])
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                if (i == DebuggingProcessor.Registers[(int)Data.Register.rpo])
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                Console.Write($" {DebuggingProcessor.Memory[i]:X2}");
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
        }

        private void CommandFormatStack(string[] command)
        {
            if (command.Length != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command does not take any arguments. Run 'help' for more info.");
                Console.ResetColor();
                return;
            }
            if (DebuggingProcessor.Registers[(int)Data.Register.rso] >= (ulong)DebuggingProcessor.Memory.LongLength)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("The stack is currently empty.");
                Console.ResetColor();
                return;
            }
            if (DebuggingProcessor.Registers[(int)Data.Register.rso] > DebuggingProcessor.Registers[(int)Data.Register.rsb])
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("The stack pointer is currently greater than the stack base - stack visualisation not available in this state.");
                Console.ResetColor();
                return;
            }
            ulong currentStackOffset = DebuggingProcessor.Registers[(int)Data.Register.rso];
            ulong currentStackBase = DebuggingProcessor.Registers[(int)Data.Register.rsb];
            for (ulong i = currentStackOffset; i < currentStackBase; i += 8)
            {
                if (i == currentStackOffset)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Current stack frame (likely local variables)");
                    Console.ResetColor();
                    Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                }
                Console.Write($"│ {i:X16} │ {DebuggingProcessor.MemReadQWord(i):X16}              │ rsb - {currentStackBase - i,-4} │");
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
            if (currentStackBase + 16 < (ulong)DebuggingProcessor.Memory.LongLength)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Return information");
                Console.ResetColor();
                Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                Console.WriteLine($"│ {currentStackBase:X16} │ Reset rso to {DebuggingProcessor.MemReadQWord(currentStackBase):X16} | rsb + 0    | <- rsb");
                Console.WriteLine($"│ {currentStackBase + 8:X16} │ Reset rsb to {DebuggingProcessor.MemReadQWord(currentStackBase + 8):X16} | rsb + 8    |");
                Console.WriteLine($"│ {currentStackBase + 16:X16} │ Reset rpo to {DebuggingProcessor.MemReadQWord(currentStackBase + 16):X16} | rsb + 16   |");
                Console.WriteLine("└──────────────────┴───────────────────────────────┴────────────┘");

                ulong parentStackBase = DebuggingProcessor.MemReadQWord(currentStackBase + 8);
                for (ulong i = currentStackBase + 24; i < parentStackBase; i += 8)
                {
                    if (i == currentStackBase + 24)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Parent stack frame (possibly parameters to this subroutine)");
                        Console.ResetColor();
                        Console.WriteLine("┌──────────────────┬───────────────────────────────┬────────────┐");
                    }
                    Console.WriteLine($"│ {i:X16} │ {DebuggingProcessor.MemReadQWord(i):X16}              │ rsb + {i - currentStackBase,-4} │");
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
        }

        private static void CommandDecimalToHexadecimal(string[] command)
        {
            if (command.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command requires 1 argument. Run 'help' for more info.");
                Console.ResetColor();
                return;
            }
            if (!ulong.TryParse(command[1], out ulong decValue))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\"{command[1]}\" is not a valid value to convert.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine($"{decValue} in hexadecimal is {decValue:X}");
        }

        private static void CommandHexadecimalToDecimal(string[] command)
        {
            if (command.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command requires 1 argument. Run 'help' for more info.");
                Console.ResetColor();
                return;
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
                return;
            }
        }

        private void CommandBreakpointManage(string[] command)
        {
            if (command.Length == 1)
            {
                Console.WriteLine("Current breakpoints:");
                foreach ((Data.Register register, ulong value) in Breakpoints)
                {
                    Console.WriteLine($"{register}: {value}");
                }
            }
            else if (command.Length == 4)
            {
                string action = command[1].ToLower();
                if (!Enum.TryParse(command[2], out Data.Register register))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[2]}\" is not a valid register. Run 'help' for more info.");
                    Console.ResetColor();
                    return;
                }
                if (!ulong.TryParse(command[3], out ulong value))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{command[3]}\" is not a valid value to break on.");
                    Console.ResetColor();
                    return;
                }
                switch (action)
                {
                    case "add":
                        if (!Breakpoints.Contains((register, value)))
                        {
                            Breakpoints.Add((register, value));
                            Console.WriteLine($"Breakpoint added for {register} with value {value}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"There is already a breakpoint added for {register} with value {value}.");
                            Console.ResetColor();
                        }
                        break;
                    case "remove":
                        if (Breakpoints.RemoveAll(x => x.Register == register && x.Value == value) == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("There were no matching breakpoints to remove.");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"Breakpoint removed for {register} with value {value}");
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{command[1]}\" is not a valid breakpoint action. Run 'help' for more info.");
                        Console.ResetColor();
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This command requires either 3 arguments to add or remove a breakpoint, or none to list them. Run 'help' for more info.");
                Console.ResetColor();
                return;
            }
        }

        private static void CommandDebugHelp()
        {
            Console.WriteLine("\nread <byte|word|dword|qword> <address> - Read data at a memory address");
            Console.WriteLine("write <mem|reg> <address|register-name> <value> - Modify the value of a memory address or register");
            Console.WriteLine("map [start offset] [limit] - Display (optionally limited amount) of memory in a grid of bytes");
            Console.WriteLine("stack - Visualise the state of the stack");
            Console.WriteLine("breakpoint [<add|remove> <register> <value>] - Add or remove a breakpoint for when a register is equal to a value");
            Console.WriteLine("dec2hex <dec-number> - Convert a decimal number to hexadecimal");
            Console.WriteLine("hex2dec <hex-number> - Convert a hexadecimal number to decimal");
            Console.WriteLine("refresh - Display the instruction to be executed and register states again");
            Console.WriteLine("run - Run the program without debugging until the next HLT instruction");
            Console.WriteLine("over - Continue to the next instruction in the current subroutine");
            Console.WriteLine("return - Continue to the next return instruction in this subroutine or higher");
        }
    }
}
