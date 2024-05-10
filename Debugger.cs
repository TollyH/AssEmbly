using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public class Debugger
    {
        public Processor DebuggingProcessor { get; }
        public DebugInfo.DebugInfoFile? LoadedDebugInfoFile { get; set; }

        public bool InReplMode { get; }

        public bool StepInstructions { get; set; } = true;
        public bool RunToReturn { get; set; }
        public ulong? StepOverStackBase { get; set; }

        public List<(Register Register, ulong Value)> Breakpoints { get; } = new();

#if V1_CALL_STACK_COMPAT
        public bool UseV1CallStack => DebuggingProcessor.UseV1CallStack;
        private ulong stackCallSize => UseV1CallStack ? 24UL : 16UL;
#else
        private const ulong stackCallSize = 16;
#endif

        private Register[] registerPushOrder =>
#if V1_CALL_STACK_COMPAT
            UseV1CallStack ? new Register[3] { Register.rso, Register.rsb, Register.rpo } :
#endif
            new Register[2] { Register.rsb, Register.rpo };

        private readonly ulong[] replPreviousRegisters = new ulong[Enum.GetNames(typeof(Register)).Length];
        private readonly Dictionary<string, ulong> replLabels = new() { { "START", 0 } };
        private ulong nextFreeRpoAddress;

        public Debugger(bool inReplMode, ulong entryPoint = 0,
            bool useV1CallStack = false, bool mapStack = true, bool autoEcho = false)
        {
            InReplMode = inReplMode;
            DebuggingProcessor = new Processor(Program.DefaultMemorySize, entryPoint, useV1CallStack, mapStack, autoEcho);
            DebuggingProcessor.Registers.CopyTo(replPreviousRegisters, 0);
        }

        public Debugger(bool inReplMode, ulong memorySize, ulong entryPoint = 0,
            bool useV1CallStack = false, bool mapStack = true, bool autoEcho = false)
        {
            InReplMode = inReplMode;
            DebuggingProcessor = new Processor(memorySize, entryPoint, useV1CallStack, mapStack, autoEcho);
            DebuggingProcessor.Registers.CopyTo(replPreviousRegisters, 0);
        }

        public Debugger(bool inReplMode, Processor processorToDebug)
        {
            InReplMode = inReplMode;
            DebuggingProcessor = processorToDebug;
            DebuggingProcessor.Registers.CopyTo(replPreviousRegisters, 0);
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
                Console.WriteLine(Strings.Debugger_Warning_Debug_Info_File, exc.GetType().Name, exc.Message);
                Console.ResetColor();
#if DEBUG
                throw;
#endif
            }
        }

        public void DisplayDebugInfo()
        {
            if (!InReplMode)
            {
                ulong currentAddress = DebuggingProcessor.Registers[(int)Register.rpo];
                // Disassemble line on-the-fly, unless a provided debugging file provides the original text for the line
                string lineDisassembly = LoadedDebugInfoFile is null
                    || !LoadedDebugInfoFile.Value.AssembledInstructions.TryGetValue(currentAddress, out string? inst)
                        ? Disassembler.DisassembleInstruction(
                            DebuggingProcessor.Memory.AsSpan()[(int)currentAddress..], true, false).Line
                        : inst;

                Console.WriteLine();
                Console.WriteLine();
                if (LoadedDebugInfoFile is not null)
                {
                    if (LoadedDebugInfoFile.Value.AddressLabels.TryGetValue(currentAddress, out string[]? labels))
                    {
                        Console.Write(Strings.Debugger_Execution_Preface_Labels);
                        Console.WriteLine(string.Join("\n    ", labels));
                        Console.WriteLine();
                    }
                    if (LoadedDebugInfoFile.Value.ImportLocations.TryGetValue(currentAddress, out string? importName))
                    {
                        Console.Write(Strings.Debugger_Execution_Preface_Imports);
                        Console.WriteLine(importName);
                        Console.WriteLine();
                    }
                    if (LoadedDebugInfoFile.Value.FileLineMap.TryGetValue(currentAddress, out FilePosition position))
                    {
                        Console.WriteLine(Strings.Debugger_Execution_Position, position.File, position.Line);
                    }
                }
                Console.Write(Strings.Debugger_Execution_Preface_Header);
                Console.WriteLine(lineDisassembly);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine();
            }

            Program.PrintRegisterStates(DebuggingProcessor);

            StatusFlags statusFlags = (StatusFlags)DebuggingProcessor.Registers[(int)Register.rsf];
            Console.Write(Strings.Generic_Flags_Header);
            foreach (StatusFlags flag in Enum.GetValues(typeof(StatusFlags)))
            {
                // Ignore combined flags (e.g. SignAndOverflow)
                if (BitOperations.PopCount((uint)flag) == 1 && (statusFlags & flag) != 0)
                {
                    Console.Write(Strings.Generic_Single_Indent, Enum.GetName(flag));
                }
            }
            Console.WriteLine();
        }

        public void DisplayReplInfo()
        {
            bool foundChange = false;
            foreach (int register in Enum.GetValues(typeof(Register)))
            {
                ulong value = DebuggingProcessor.Registers[register];
                ulong previousValue = replPreviousRegisters[register];
                if (value != previousValue)
                {
                    if (!foundChange)
                    {
                        foundChange = true;
                        Console.WriteLine(Strings.REPL_Changed_Registers_Header);
                    }
                    Console.WriteLine(Strings.REPL_Changed_Registers_Line, Enum.GetName((Register)register), previousValue, value, previousValue, value);
                }
            }

            StatusFlags statusFlags = (StatusFlags)DebuggingProcessor.Registers[(int)Register.rsf];
            StatusFlags previousFlags = (StatusFlags)replPreviousRegisters[(int)Register.rsf];
            foundChange = false;
            foreach (StatusFlags flag in Enum.GetValues(typeof(StatusFlags)))
            {
                // Ignore combined flags (e.g. SignAndOverflow)
                if (BitOperations.PopCount((uint)flag) == 1 && (statusFlags & flag) != (previousFlags & flag))
                {
                    if (!foundChange)
                    {
                        foundChange = true;
                        Console.Write(Strings.REPL_Changed_Flags_Header);
                    }
                    Console.Write(Strings.Generic_Single_Indented_Key_Value,
                        (statusFlags & flag) == 0 ? Strings.REPL_Changed_Flags_Unset : Strings.REPL_Changed_Flags_Set, Enum.GetName(flag));
                }
            }
            Console.WriteLine();

            Console.WriteLine(Strings.REPL_Remaining_Memory, (uint)DebuggingProcessor.Memory.Length - DebuggingProcessor.Registers[(int)Register.rpo]);
        }

        public static void DisplayReplHeader()
        {
            Console.WriteLine(Strings.REPL_Header);
        }

        public void StartDebugger()
        {
            if (InReplMode)
            {
                // AssEmbly expects UTF-8 encoding for special characters, make sure REPL input complies
                Console.InputEncoding = Encoding.UTF8;
                DisplayReplHeader();
            }
            while (true)
            {
                try
                {
                    bool breakForDebug;
                    if (InReplMode)
                    {
                        // If we're acting as a REPL, only ask user for an instruction if there isn't one already to execute.
                        breakForDebug = DebuggingProcessor.Memory[DebuggingProcessor.Registers[(int)Register.rpo]] == 0;
                    }
                    else
                    {
                        // Only pause for debugging instruction if not running to break, not in a deeper subroutine than we were if stepping over,
                        // and aren't waiting for a return instruction
                        breakForDebug = StepInstructions && (StepOverStackBase is null || DebuggingProcessor.Registers[(int)Register.rsb] >= StepOverStackBase)
                            // Is the next instruction a return instruction?
                            && (!RunToReturn || DebuggingProcessor.Memory[DebuggingProcessor.Registers[(int)Register.rpo]] is 0xBA or 0xBB or 0xBC or 0xBD or 0xBE);
                    }

                    foreach ((Register register, ulong value) in Breakpoints)
                    {
                        if (DebuggingProcessor.Registers[(int)register] == value)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(Strings.Debugger_Breakpoint_Hit, register, value);
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
                        if (InReplMode)
                        {
                            DisplayReplInfo();
                            DebuggingProcessor.Registers.CopyTo(replPreviousRegisters, 0);
                        }
                        else
                        {
                            DisplayDebugInfo();
                        }
                    }
                    bool endCommandEntryLoop = false;
                    while (!endCommandEntryLoop && breakForDebug)
                    {
                        Console.Write(InReplMode ? Strings.REPL_Command_Prompt : Strings.Debugger_Command_Prompt);
                        string userInput = Console.ReadLine()!;
                        if (InReplMode)
                        {
                            if (ProcessReplInput(userInput))
                            {
                                endCommandEntryLoop = true;
                            }
                        }
                        else
                        {
                            string[] command = userInput.Trim().Split(' ');
                            switch (command[0].ToLower())
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
                                    StepOverStackBase = DebuggingProcessor.Registers[(int)Register.rsb];
                                    break;
                                case "return":
                                    endCommandEntryLoop = true;
                                    StepOverStackBase = DebuggingProcessor.Registers[(int)Register.rsb];
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
#if EXTENSION_SET_HEAP_ALLOCATE
                                case "heap":
                                    CommandHeapStats();
                                    break;
#endif
                                case "help":
                                    CommandDebugHelp();
                                    break;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(Strings.Debugger_Error_Unrecognised_Command, command[0]);
                                    Console.ResetColor();
                                    break;
                            }
                        }
                    }
                    if (DebuggingProcessor.Execute(false) && !InReplMode)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(Strings.Debugger_Warning_HLT_Reached);
                        Console.ResetColor();
                        Console.Write(Strings.Debugger_Any_Key_Continue);
                        _ = Console.ReadKey(true);
                        Console.WriteLine();
                        StepInstructions = true;
                    }
                }
                catch (Exception e)
                {
                    Program.OnExecutionException(e, DebuggingProcessor);
                    if (InReplMode)
                    {
                        // Move past all existing instruction data if an error is encountered
                        DebuggingProcessor.Registers[(int)Register.rpo] = nextFreeRpoAddress;
                    }
                    else
                    {
#if DEBUG
                        throw;
#else
                        Environment.Exit(1);
                        return;
#endif
                    }
                }
            }
        }

        private void CommandReadMemory(IReadOnlyList<string> command)
        {
            if (command.Count == 3)
            {
                ulong bytesToRead = command[1] == "byte" ? 1 : command[1] == "word"
                    ? 2 : command[1] == "dword" ? 4 : command[1] == "qword" ? 8 : 0U;
                if (bytesToRead == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Size, command[1]);
                    Console.ResetColor();
                    return;
                }
                if (!ulong.TryParse(command[2], out ulong address))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Address, command[2]);
                    Console.ResetColor();
                    return;
                }
                if (address + bytesToRead > (ulong)DebuggingProcessor.Memory.LongLength)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_OutOfRange_Address, command[2]);
                    Console.ResetColor();
                    return;
                }
                ulong value = 0;
                for (ulong i = address; i < address + bytesToRead; i++)
                {
                    value += (ulong)DebuggingProcessor.Memory[i] << (byte)((i - address) * 8);
                }
                Console.WriteLine(Strings.Debugger_Memory_Value, address, value, value, Convert.ToString((long)value, 2));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_2);
                Console.ResetColor();
            }
        }

        private void CommandWriteMemReg(IReadOnlyList<string> command)
        {
            if (command.Count == 4)
            {
                switch (command[1])
                {
                    case "mem":
                    {
                        if (!ulong.TryParse(command[2], out ulong address))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Strings.Debugger_Error_Invalid_Address, command[2]);
                            Console.ResetColor();
                            return;
                        }
                        if (address >= (ulong)DebuggingProcessor.Memory.LongLength)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Strings.Debugger_Error_OutOfRange_Address, command[2]);
                            Console.ResetColor();
                            return;
                        }
                        if (!byte.TryParse(command[3], out byte value))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Strings.Debugger_Error_Invalid_Byte_Value, command[3]);
                            Console.ResetColor();
                            return;
                        }
                        DebuggingProcessor.Memory[address] = value;
                        Console.WriteLine(Strings.Debugger_Success_Address_Value_Set, address, value);
                        break;
                    }
                    case "reg":
                    {
                        if (!Enum.TryParse(command[2], out Register register))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Strings.Debugger_Error_Invalid_Register, command[2]);
                            Console.ResetColor();
                            return;
                        }
                        if (!ulong.TryParse(command[3], out ulong value))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(Strings.Debugger_Error_Invalid_Register_Value, command[3]);
                            Console.ResetColor();
                            return;
                        }
                        DebuggingProcessor.Registers[(int)register] = value;
                        Console.WriteLine(Strings.Debugger_Success_Register_Value_Set, Enum.GetName(register), value);
                        break;
                    }
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.Debugger_Error_Invalid_Location, command[1]);
                        Console.ResetColor();
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_3);
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
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Offset, command[1]);
                    Console.ResetColor();
                    return;
                }
            }
            if (command.Length == 3)
            {
                if (!ulong.TryParse(command[2], out limit))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Limit, command[1]);
                    Console.ResetColor();
                    return;
                }
            }
            if (command.Length is not 1 and > 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_0to2);
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(Strings.Generic_Register_rpo);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Strings.Generic_Register_rsb);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Strings.Generic_Register_rso);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(Strings.Generic_Unmapped);
            Console.ResetColor();
            Console.WriteLine();
            Console.Write(Strings.Debugger_MemoryMap_Header);
            ulong start = offset - (offset % 16);  // Ensure offset is a multiple of 16
            ulong end = (ulong)DebuggingProcessor.Memory.LongLength < (limit + start) ? (ulong)DebuggingProcessor.Memory.LongLength : (limit + start);
            bool writtenZeroFill = false;
            for (ulong rowStartAdr = start; rowStartAdr < end; rowStartAdr += 16)
            {
                byte[] nextBytes = DebuggingProcessor.Memory[(int)rowStartAdr..((int)rowStartAdr + 16)];

                // Fill rows that are all 0 with a single asterisk
                if (nextBytes.All(b => b == 0))
                {
                    if (!writtenZeroFill)
                    {
                        Console.WriteLine('*');
                        writtenZeroFill = true;
                    }
                    continue;
                }
                writtenZeroFill = false;

                Console.Write(Strings.Debugger_MemoryMap_FirstCol, rowStartAdr);
                for (ulong i = rowStartAdr; i < rowStartAdr + 16; i++)
                {
                    string valueStr = string.Format(Strings.Debugger_MemoryMap_Cell, DebuggingProcessor.Memory[i]);
#if EXTENSION_SET_HEAP_ALLOCATE
                    // Being unmapped is the lowest priority colour, and should be completely replaced by any register colours
                    if (DebuggingProcessor.MappedMemoryRanges.All(mappedRange => !mappedRange.Contains((long)i)))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
#endif
                    // Write both characters separately so that if multiple registers point to the same address, the cell becomes multi-coloured
                    if (i == DebuggingProcessor.Registers[(int)Register.rso])
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    if (i == DebuggingProcessor.Registers[(int)Register.rsb])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    if (i == DebuggingProcessor.Registers[(int)Register.rpo])
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    Console.Write(valueStr[0]);
                    // Reversed colour priority for second character
                    if (i == DebuggingProcessor.Registers[(int)Register.rpo])
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    if (i == DebuggingProcessor.Registers[(int)Register.rsb])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    if (i == DebuggingProcessor.Registers[(int)Register.rso])
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(valueStr[1]);
                    Console.ResetColor();
                    Console.Write(' ');
                }
                Console.Write(Strings.Debugger_MemoryMap_VerticalSep);
                for (ulong i = rowStartAdr; i < rowStartAdr + 16; i++)
                {
                    char value = (char)DebuggingProcessor.Memory[i];
                    Console.Write(value is >= ' ' and <= '~' ? value : '.');
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(Strings.Generic_Register_rpo);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Strings.Generic_Register_rsb);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Strings.Generic_Register_rso);
            Console.ResetColor();
            Console.Write(Strings.Generic_CommaSeparate);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(Strings.Generic_Unmapped);
            Console.ResetColor();
            Console.WriteLine();
        }

        private void CommandFormatStack(string[] command)
        {
            ulong limit = uint.MaxValue;
            if (command.Length == 2)
            {
                if (!ulong.TryParse(command[1], out limit))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Limit, command[1]);
                    Console.ResetColor();
                    return;
                }
            }
            else if (command.Length != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_0to1);
                Console.ResetColor();
                return;
            }

            if (DebuggingProcessor.Registers[(int)Register.rso] >= (ulong)DebuggingProcessor.Memory.LongLength)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Strings.Debugger_Warning_Stack_Empty);
                Console.ResetColor();
                return;
            }
            if (DebuggingProcessor.Registers[(int)Register.rso] > DebuggingProcessor.Registers[(int)Register.rsb])
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Strings.Debugger_Warning_rso_GT_rsb);
                Console.ResetColor();
                return;
            }

            ulong currentStackOffset = DebuggingProcessor.Registers[(int)Register.rso];
            ulong currentStackBase = DebuggingProcessor.Registers[(int)Register.rsb];
            if (currentStackBase - currentStackOffset > limit)
            {
                currentStackOffset = currentStackBase - (limit / 8 * 8);  // Ensure limit is a multiple of 8
            }
            for (ulong i = currentStackOffset; i < currentStackBase; i += 8)
            {
                if (i == currentStackOffset)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Strings.Debugger_Stack_CurrentFrame_Header);
                    Console.ResetColor();
                    Console.WriteLine(Strings.Debugger_Stack_Box_Top);
                }
                Console.Write(Strings.Debugger_Stack_CurrentFrame_Row, i, DebuggingProcessor.ReadMemoryQWord(i), currentStackBase - i);
                if (i == DebuggingProcessor.Registers[(int)Register.rso])
                {
                    Console.WriteLine(Strings.Debugger_Stack_Pointer_rso);
                }
                else
                {
                    Console.WriteLine();
                }
                if (i + 8 >= currentStackBase)
                {
                    Console.WriteLine(Strings.Debugger_Stack_Box_Bottom);
                }
            }
            if (currentStackBase + stackCallSize <= (ulong)DebuggingProcessor.Memory.LongLength)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(Strings.Debugger_Stack_ReturnInfo_Header);
                Console.ResetColor();
                Console.WriteLine(Strings.Debugger_Stack_Box_Top);
                Console.WriteLine(Strings.Debugger_Stack_ReturnInfo_First, currentStackBase, registerPushOrder[0], DebuggingProcessor.ReadMemoryQWord(currentStackBase));
                Console.WriteLine(Strings.Debugger_Stack_ReturnInfo_Second, currentStackBase + 8, registerPushOrder[1], DebuggingProcessor.ReadMemoryQWord(currentStackBase + 8));
#if V1_CALL_STACK_COMPAT
                if (UseV1CallStack)
                {
                    Console.WriteLine(Strings.Debugger_Stack_ReturnInfo_Third, currentStackBase + 16, registerPushOrder[2], DebuggingProcessor.ReadMemoryQWord(currentStackBase + 16));
                }
#endif
                Console.WriteLine(Strings.Debugger_Stack_Box_Bottom);

                ulong parentStackBase = DebuggingProcessor.ReadMemoryQWord(currentStackBase
#if V1_CALL_STACK_COMPAT
                + (UseV1CallStack ? 8UL : 0UL)
#endif
                );

                if (currentStackBase + stackCallSize >= parentStackBase)
                {
                    // Parent stack is empty
                    return;
                }
                if (parentStackBase - (currentStackBase + stackCallSize + 7) > limit)
                {
                    parentStackBase = currentStackBase + stackCallSize + limit - 7;
                }
                for (ulong i = currentStackBase + stackCallSize; i < parentStackBase; i += 8)
                {
                    if (i == currentStackBase + stackCallSize)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.Debugger_Stack_ParentFrame_Header);
                        Console.ResetColor();
                        Console.WriteLine(Strings.Debugger_Stack_Box_Top);
                    }
                    Console.WriteLine(Strings.Debugger_Stack_ParentFrame_Row, i, DebuggingProcessor.ReadMemoryQWord(i), i - currentStackBase);
                    if (i + 8 >= parentStackBase)
                    {
                        Console.WriteLine(Strings.Debugger_Stack_Box_Bottom);
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Strings.Debugger_Warning_Stack_Bottom);
                Console.ResetColor();
            }
        }

        private static void CommandDecimalToHexadecimal(string[] command)
        {
            if (command.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_1);
                Console.ResetColor();
                return;
            }
            if (!ulong.TryParse(command[1], out ulong decValue))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Invalid_Convert_Value, command[1]);
                Console.ResetColor();
                return;
            }
            Console.WriteLine(Strings.Debugger_Value_In_Hex, decValue, decValue);
        }

        private static void CommandHexadecimalToDecimal(string[] command)
        {
            if (command.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_1);
                Console.ResetColor();
                return;
            }
            try
            {
                ulong convertedValue = Convert.ToUInt64(command[1], 16);
                Console.WriteLine(Strings.Debugger_Value_In_Decimal, command[1], convertedValue);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Invalid_Convert_Value, command[1]);
                Console.ResetColor();
            }
        }

        private void CommandBreakpointManage(string[] command)
        {
            if (command.Length == 1)
            {
                Console.WriteLine(Strings.Debugger_Breakpoints_Header);
                foreach ((Register register, ulong value) in Breakpoints)
                {
                    Console.WriteLine(Strings.Generic_Key_Value, register, value);
                }
            }
            else if (command.Length == 4)
            {
                if (!Enum.TryParse(command[2], out Register register))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Register, command[2]);
                    Console.ResetColor();
                    return;
                }
                if (!ulong.TryParse(command[3], out ulong value))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.Debugger_Error_Invalid_Break_Value, command[3]);
                    Console.ResetColor();
                    return;
                }
                switch (command[1].ToLower())
                {
                    case "add":
                        if (!Breakpoints.Contains((register, value)))
                        {
                            Breakpoints.Add((register, value));
                            Console.WriteLine(Strings.Debugger_Success_Breakpoint, register, value);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(Strings.Debugger_Warning_Breakpoint_Exists, register, value);
                            Console.ResetColor();
                        }
                        break;
                    case "remove":
                        if (Breakpoints.RemoveAll(x => x.Register == register && x.Value == value) == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(Strings.Debugger_Warning_Breakpoint_No_Matching);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine(Strings.Debugger_Success_Breakpoint_Remove, register, value);
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.Debugger_Error_Invalid_Breakpoint_Action, command[1]);
                        Console.ResetColor();
                        break;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.Debugger_Error_Args_Required_Breakpoint);
                Console.ResetColor();
            }
        }

#if EXTENSION_SET_HEAP_ALLOCATE
        private void CommandHeapStats()
        {
            long memorySize = DebuggingProcessor.Memory.LongLength;
            IReadOnlyList<Range> mappedRanges = DebuggingProcessor.MappedMemoryRanges;
            long programSize = mappedRanges[0].Length;
            long stackSize = mappedRanges[^1].Length;
            long freeMemory =
                memorySize - programSize - stackSize - mappedRanges.Skip(1).SkipLast(1).Sum(r => r.Length);

            List<Range> freeBlocks = new();
            long largestFree = -1;
            for (int i = 0; i < mappedRanges.Count - 1; i++)
            {
                if (mappedRanges[i].End != mappedRanges[i + 1].Start)
                {
                    Range newRange = new(mappedRanges[i].End, mappedRanges[i + 1].Start);
                    freeBlocks.Add(newRange);
                    if (newRange.Length > largestFree)
                    {
                        largestFree = newRange.Length;
                    }
                }
            }

            Console.WriteLine(Strings.Debugger_Heap_Stats_Main,
                memorySize,
                freeMemory,
                freeBlocks.Count,
                largestFree,
                100d - (double)largestFree / freeMemory * 100d,
                mappedRanges.Count - 2,
                mappedRanges.Skip(1).SkipLast(1).Sum(m => m.Length),
                stackSize,
                DebuggingProcessor.MapStack ? "" : Strings.Debugger_Heap_Unmapped,
                programSize);

            // Generate visual map of stack allocation
            Console.WriteLine();
            int padding = Console.WindowWidth / 12;
            int numberOfBars = Console.WindowWidth - padding;
            double bytesPerBar = (double)memorySize / numberOfBars;
            Console.WriteLine(Strings.Debugger_Heap_Map_Header);
            // Colour and block size key
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('\u2588');
            Console.ResetColor();
            Console.Write(Strings.Debugger_Heap_Map_Fully_Unmapped);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('\u2588');
            Console.ResetColor();
            Console.Write(Strings.Debugger_Heap_Map_Fully_Mapped);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write('\u2588');
            Console.ResetColor();
            Console.Write(Strings.Debugger_Heap_Map_Partially_Mapped, bytesPerBar);
            Console.WriteLine('\n');
            for (int i = 0; i < numberOfBars; i++)
            {
                Range representingRange = new((long)Math.Floor(i * bytesPerBar),
                    Math.Min((long)Math.Ceiling((i + 1) * bytesPerBar), memorySize));
                bool anyMapped = mappedRanges.Any(r => r.Overlaps(representingRange));
                bool anyUnmapped = freeBlocks.Any(r => r.Overlaps(representingRange));
                if (anyMapped && anyUnmapped)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (anyMapped)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.Write('\u2588');
                Console.ResetColor();
            }
            Console.WriteLine();
        }
#endif

        private static void CommandDebugHelp()
        {
            Console.WriteLine(Strings.Debugger_Help_Body);
        }

        private bool ProcessReplInput(string userInput)
        {
            try
            {
                string[] instruction = Assembler.ParseLine(userInput);
                if (instruction.Length == 0)
                {
                    DisplayDebugInfo();
                    return false;
                }
                string mnemonic = instruction[0];

                if (mnemonic[0] == ':')
                {
                    // Will throw an error if label is not valid
                    OperandType operandType = Assembler.DetermineOperandType(mnemonic, false);
                    if (operandType != OperandType.Address)
                    {
                        throw new SyntaxError(Strings.REPL_Error_Label_Ampersand);
                    }
                    string labelName = mnemonic[1..];
                    replLabels[labelName] = DebuggingProcessor.Registers[(int)Register.rpo];
                    return false;
                }

                (byte[] newBytes, List<(string LabelName, ulong AddressOffset)> newLabels) =
                    Assembler.AssembleStatement(mnemonic, instruction[1..]);

                foreach ((string labelName, ulong addressOffset) in newLabels)
                {
                    if (!replLabels.TryGetValue(labelName, out ulong address))
                    {
                        throw new LabelNameException(string.Format(Strings.REPL_Error_Label_Not_Exists, labelName));
                    }
                    BinaryPrimitives.WriteUInt64LittleEndian(newBytes.AsSpan()[(int)addressOffset..], address);
                }

                newBytes.CopyTo(DebuggingProcessor.Memory, (int)DebuggingProcessor.Registers[(int)Register.rpo]);
                nextFreeRpoAddress = DebuggingProcessor.Registers[(int)Register.rpo] + (uint)newBytes.Length;
                if (userInput[0] == ' ')
                {
                    // Starting with a space signifies that the REPL shouldn't execute the input, only insert it
                    DebuggingProcessor.Registers[(int)Register.rpo] = nextFreeRpoAddress;
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Program.OnAssemblerException(e);
#if DEBUG
                throw;
#else
                return false;
#endif
            }
        }
    }
}
