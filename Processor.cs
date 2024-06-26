﻿using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    /// <summary>
    /// Executes compiled AssEmbly programs.
    /// </summary>
    public class Processor : IDisposable
    {
#if EXTENSION_SET_EXTERNAL_ASM
        public static readonly Type[] ExternalMethodParamTypes =
            new Type[3] { typeof(byte[]), typeof(ulong[]), typeof(ulong?) };
#endif

        public readonly byte[] Memory;
        public readonly ulong[] Registers;

#if V1_CALL_STACK_COMPAT
        public readonly bool UseV1CallStack;
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
        public readonly bool MapStack;
#endif
#if !EXTENSION_SET_TERMINAL
        public readonly bool AutoEcho;
#endif

#if V1_CALL_STACK_COMPAT
        private readonly ulong stackCallSize;
#else
        private const ulong stackCallSize = 16;
#endif

#if EXTENSION_SET_HEAP_ALLOCATE
        private readonly List<Range> _mappedMemoryRanges = new();
        public IReadOnlyList<Range> MappedMemoryRanges => _mappedMemoryRanges.AsReadOnly();
#endif

        public bool ProgramLoaded { get; private set; }

        public bool IsFileOpen => openFile is not null || fileRead is not null || fileWrite is not null;
#if EXTENSION_SET_EXTERNAL_ASM
        public bool IsExternalOpen => openExtAssembly is not null || openExtFunction is not null || extLoadContext is not null;
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
        public bool AnyRegionsMapped => _mappedMemoryRanges.Count > 2;
#endif

        public bool IsClean =>
        !IsFileOpen
#if EXTENSION_SET_EXTERNAL_ASM
        && !IsExternalOpen
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
        && !AnyRegionsMapped
#endif
        ;

        private Stream? openFile;
        private BinaryReader? fileRead;
        private BinaryWriter? fileWrite;
        private long openFileSize;

#if EXTENSION_SET_EXTERNAL_ASM
        private AssemblyLoadContext? extLoadContext;
        private Type? openExtAssembly;
        private MethodInfo? openExtFunction;
#endif

#if EXTENSION_SET_FILE_SYSTEM
        private IEnumerator<string> directoryListing = Enumerable.Empty<string>().GetEnumerator();
        private IEnumerator<string> fileListing = Enumerable.Empty<string>().GetEnumerator();
#endif

        private readonly Random rng = new();

        // Because C#'s console methods work with potentially multi-byte characters at a time,
        // but AssEmbly works with single bytes, we need a queue to store bytes that have been read
        // from stdin but are yet to be processed by an AssEmbly read instruction.
        private readonly Queue<byte> stdinByteQueue = new();

        public const ulong SignBit = unchecked((ulong)long.MinValue);

        public Processor(ulong memorySize, ulong entryPoint = 0,
            bool useV1CallStack = false, bool mapStack = true, bool autoEcho = false)
        {
            Memory = new byte[memorySize];
            Registers = new ulong[Enum.GetNames(typeof(Register)).Length];
            Registers[(int)Register.rpo] = entryPoint;
            Registers[(int)Register.rso] = memorySize;
            Registers[(int)Register.rsb] = memorySize;
#if EXTENSION_SET_HEAP_ALLOCATE
            _mappedMemoryRanges.Add(new Range((long)memorySize, (long)memorySize));
#endif
            ProgramLoaded = false;
#if CLI
            // AssEmbly stores strings as UTF-8, so console must be set to UTF-8 to render bytes correctly
            Console.OutputEncoding = Encoding.UTF8;
#endif
#if V1_CALL_STACK_COMPAT
            UseV1CallStack = useV1CallStack;
            stackCallSize = useV1CallStack ? 24UL : 16UL;
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
            MapStack = mapStack;
#endif
#if EXTENSION_SET_TERMINAL
            if (autoEcho)
            {
                Registers[(int)Register.rsf] |= (ulong)StatusFlags.AutoEcho;
            }
#else
            AutoEcho = autoEcho;
#endif
        }

        ~Processor()
        {
            Dispose();
        }

        public void Dispose()
        {
#if EXTENSION_SET_EXTERNAL_ASM
            extLoadContext?.Unload();
            extLoadContext = null;
            openExtAssembly = null;
            openExtFunction = null;
#endif

            openFile?.Dispose();
            openFile = null;
            fileRead?.Dispose();
            fileRead = null;
            fileWrite?.Dispose();
            fileWrite = null;

#if EXTENSION_SET_FILE_SYSTEM
            directoryListing.Dispose();
            fileListing.Dispose();
#endif

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads a provided compiled program and its data into this processor's memory to be executed.
        /// </summary>
        /// <param name="programData">The entire program, including any data, to load into memory.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a program has already been loaded into this processor,
        /// or the program is too large to be loaded given the amount of allocated memory.
        /// </exception>
        public void LoadProgram(byte[] programData)
        {
            if (ProgramLoaded)
            {
                throw new InvalidOperationException(Strings_Processor.Error_Already_Loaded);
            }
            if (programData.LongLength > Memory.LongLength)
            {
                throw new InvalidOperationException(string.Format(Strings_Processor.Error_Program_Too_Large, Memory.LongLength, programData.LongLength));
            }
            Array.Copy(programData, Memory, programData.LongLength);
#if EXTENSION_SET_HEAP_ALLOCATE
            _mappedMemoryRanges.Insert(0, new Range(0, programData.LongLength));
#endif
            ProgramLoaded = true;
        }

        /// <summary>
        /// Execute either a single instruction or execute until a halt instruction is reached,
        /// depending on the value of <paramref name="runUntilHalt"/>.
        /// </summary>
        /// <returns><see langword="true"/> if execution should stop (HLT reached) - otherwise <see langword="false"/></returns>
        /// <param name="waitForInputChar">
        /// Must not be <see langword="false"/> if console key presses are being used for the RCC instruction.
        /// If <see langword="true"/>, RCC becomes a no-op and does not increment rpo if no characters are currently available on the input stream.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a program hasn't been loaded into this processor, or the processor has reached the end of allocated memory.
        /// </exception>
        /// <exception cref="InvalidOpcodeException">
        /// Thrown if the processor encounters an opcode to execute that didn't match any known opcodes.
        /// </exception>
        /// <exception cref="ReadOnlyRegisterException">Thrown if an instruction attempts to write to a read-only register.</exception>
        /// <exception cref="FileOperationException">
        /// Thrown when a file operation is attempted but is not valid given the current state of the processor.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if an instruction tried to access an invalid memory address.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an instruction tried to access an invalid memory address.</exception>
        /// <exception cref="DivideByZeroException">Thrown if a division instruction is executed with a value of zero as the divisor.</exception>
        /// <exception cref="FileNotFoundException">Thrown if an operation that requires a file to exist could not find the target file.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if an operation that requires a directory to exist could not find the target directory.</exception>
        public bool Execute(bool runUntilHalt, Stream? stdoutOverride = null, Stream? stdinOverride = null, bool waitForInputChar = true)
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException(Strings_Processor.Error_No_Program);
            }
            if (Registers[(int)Register.rpo] >= (ulong)Memory.LongLength)
            {
                throw new InvalidOperationException("The processor has reached the end of accessible memory.");
            }
            if (!waitForInputChar && stdinOverride is null && !Console.IsInputRedirected)
            {
                throw new ArgumentException("waitForInputChar cannot be false when characters are being read from console key presses, not a stream.");
            }
            bool halt = false;
            using Stream stdout = stdoutOverride ?? Console.OpenStandardOutput();
            using Stream? stdin = stdinOverride ?? (Console.IsInputRedirected ? Console.OpenStandardInput() : null);
            do
            {
                byte extensionSet;
                byte opcode;
                bool fullOpcode = Memory[Registers[(int)Register.rpo]] == Opcode.FullyQualifiedMarker;
                if (fullOpcode)
                {
                    extensionSet = Memory[++Registers[(int)Register.rpo]];
                    opcode = Memory[++Registers[(int)Register.rpo]];
                }
                else
                {
                    extensionSet = 0x0;
                    opcode = Memory[Registers[(int)Register.rpo]];
                }
                // Upper 4-bytes (general category of instruction)
                byte opcodeHigh = (byte)(0xF0 & opcode);
                // Lower 4-bytes (specific operation and operand types)
                byte opcodeLow = (byte)(0x0F & opcode);
                ulong operandStart = ++Registers[(int)Register.rpo];

#if EXTENSION_SET_HEAP_ALLOCATE
                ulong oldSO = Registers[(int)Register.rso];
#endif

                // Local variables used to hold additional state information while executing instructions
                ulong initial;
                ulong mathend;
                ulong result;
                ulong remainder;
                ulong byteCount;
                int shiftAmount;
                string filepath;
                long signedInitial;
#if EXTENSION_SET_SIGNED
                ulong initialSign;
                ulong resultSign;
                long signedMathend;
                ulong signedShiftAllBits;
#endif
#if EXTENSION_SET_FLOATING_POINT
                double floatingInitial;
                double floatingMathend;
                double floatingResult;
#endif
#if EXTENSION_SET_FILE_SYSTEM
                string destination;
#endif

                switch (extensionSet)
                {
                    case 0x00:  // Base instruction set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Control / Jump
                                switch (opcodeLow)
                                {
                                    case 0x0:  // HLT (Halt)
                                        halt = true;
                                        break;
                                    case 0x1:  // NOP
                                        break;
                                    case 0x2:  // JMP adr (Unconditional Jump)
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x3:  // JMP ptr (Unconditional Jump)
                                        Registers[(int)Register.rpo] = ReadMemoryPointer(operandStart, out _);
                                        break;
                                    case 0x4:  // JEQ adr (Jump If Equal To - Zero Flag Set)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x5:  // JEQ ptr (Jump If Equal To - Zero Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x6:  // JNE adr (Jump If Not Equal To - Zero Flag Unset)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x7:  // JNE ptr (Jump If Not Equal To - Zero Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x8:  // JLT adr (Jump If Less Than - Carry Flag Set)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x9:  // JLT ptr (Jump If Less Than - Carry Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xA:  // JLE adr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xB:  // JLE ptr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xC:  // JGT adr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xD:  // JGT ptr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xE:  // JGE adr (Jump If Greater Than or Equal To - Carry Flag Unset)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xF:  // JGE ptr (Jump If Greater Than or Equal To - Carry Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Control, opcodeLow));
                                }
                                break;
                            case 0x10:  // Addition
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // ADD reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // ADD reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // ADD reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // ADD reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // ICR reg
                                        mathend = 1;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Addition, opcodeLow));
                                }
                                result = initial + mathend;
                                WriteMemoryRegister(operandStart, result);

                                if (result < initial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                initialSign = initial & SignBit;
                                resultSign = result & SignBit;
                                if (initialSign == (mathend & SignBit) && initialSign != resultSign)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Overflow;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;
                                }

                                if (resultSign != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x20:  // Subtraction
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SUB reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SUB reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SUB reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SUB reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // DCR reg
                                        mathend = 1;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Subtraction, opcodeLow));
                                }
                                result = initial - mathend;
                                WriteMemoryRegister(operandStart, result);

                                if (result > initial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                initialSign = initial & SignBit;
                                resultSign = result & SignBit;
                                if (initialSign != (mathend & SignBit) && initialSign != resultSign)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Overflow;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;
                                }

                                if (resultSign != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x30:  // Multiplication
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // MUL reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // MUL reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // MUL reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // MUL reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Multiplication, opcodeLow));
                                }
                                result = initial * mathend;
                                WriteMemoryRegister(operandStart, result);

                                if (result < initial && mathend != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }


#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
#endif
                                break;
                            case 0x40:  // Division
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // DIV reg, reg
                                        result = initial / ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // DIV reg, lit
                                        result = initial / ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // DIV reg, adr
                                        result = initial / ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // DIV reg, ptr
                                        result = initial / ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // DVR reg, reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 2);
                                        result = initial / mathend;
                                        remainder = initial % mathend;
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 3;
                                        break;
                                    case 0x5:  // DVR reg, reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 2);
                                        result = initial / mathend;
                                        remainder = initial % mathend;
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x6:  // DVR reg, reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 2);
                                        result = initial / mathend;
                                        remainder = initial % mathend;
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x7:  // DVR reg, reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 2, out byteCount);
                                        result = initial / mathend;
                                        remainder = initial % mathend;
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += byteCount + 2;
                                        break;
                                    case 0x8:  // REM reg, reg
                                        result = initial % ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // REM reg, lit
                                        result = initial % ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // REM reg, adr
                                        result = initial % ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // REM reg, ptr
                                        result = initial % ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Division, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x50:  // Shifting
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SHL reg, reg
                                        shiftAmount = (int)ReadMemoryRegister(operandStart + 1);
                                        result = initial << shiftAmount;
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SHL reg, lit
                                        shiftAmount = (int)ReadMemoryQWord(operandStart + 1);
                                        result = initial << shiftAmount;
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SHL reg, adr
                                        shiftAmount = (int)ReadMemoryPointedQWord(operandStart + 1);
                                        result = initial << shiftAmount;
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SHL reg, ptr
                                        shiftAmount = (int)ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        result = initial << shiftAmount;
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // SHR reg, reg
                                        shiftAmount = (int)ReadMemoryRegister(operandStart + 1);
                                        result = initial >> shiftAmount;
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // SHR reg, lit
                                        shiftAmount = (int)ReadMemoryQWord(operandStart + 1);
                                        result = initial >> shiftAmount;
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // SHR reg, adr
                                        shiftAmount = (int)ReadMemoryPointedQWord(operandStart + 1);
                                        result = initial >> shiftAmount;
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // SHR reg, ptr
                                        shiftAmount = (int)ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        result = initial >> shiftAmount;
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Shifting, opcodeLow));
                                }
                                // C# only counts the lower 6 bits of the amount to shift by, so values greater than or equal to 64 will not return 0 as
                                // wanted for AssEmbly.
                                if (shiftAmount >= 64)
                                {
                                    result = 0;
                                }
                                WriteMemoryRegister(operandStart, result);

                                // We will never overflow when shifting by 0 bits or if the initial value is 0.
                                // We will always overflow if shifting by 64 bits or more as long as the above isn't the case.
                                //
                                // Otherwise, if shifting left (opcodeLow <= 0x3), "(initial >> (64 - amount)) != 0" checks if there are any 1 bits
                                // in the portion of the number that will be cutoff during the left shift by cutting off the bits that will remain.
                                // 8-bit e.g: 0b11001001 << 3 |> (0b11001001 >> (8 - 3)), (0b11001001 >> 5) = 0b00000110, result != 0, therefore set carry.
                                //
                                // If shifting right, "(initial << (64 - amount)) != 0" checks if there are any 1 bits
                                // in the portion of the number that will be cutoff during the right shift by cutting off the bits that will remain.
                                // 8-bit e.g: 0b11001001 >> 3 |> (0b11001001 << (8 - 3)), (0b11001001 << 5) = 0b00100000, result != 0, therefore set carry.
                                if (shiftAmount != 0 && initial != 0 && (shiftAmount >= 64 || opcodeLow <= 0x3
                                    ? (initial >> (64 - shiftAmount)) != 0
                                    : (initial << (64 - shiftAmount)) != 0))
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }

#if EXTENSION_SET_SIGNED
                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;
#endif
                                break;
                            case 0x60:  // Bitwise
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // AND reg, reg
                                        result = initial & ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // AND reg, lit
                                        result = initial & ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // AND reg, adr
                                        result = initial & ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // AND reg, ptr
                                        result = initial & ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // ORR reg, reg
                                        result = initial | ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // ORR reg, lit
                                        result = initial | ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // ORR reg, adr
                                        result = initial | ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // ORR reg, ptr
                                        result = initial | ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x8:  // XOR reg, reg
                                        result = initial ^ ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // XOR reg, lit
                                        result = initial ^ ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // XOR reg, adr
                                        result = initial ^ ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // XOR reg, ptr
                                        result = initial ^ ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xC:  // NOT reg
                                        result = ~initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0xD:  // RNG reg
                                        result = (ulong)rng.NextInt64(long.MinValue, long.MaxValue);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Bitwise, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif
                                break;
                            case 0x70:  // Test
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TST reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // TST reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // TST reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // TST reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // CMP reg, reg
                                        mathend = ReadMemoryRegister(operandStart + 1);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // CMP reg, lit
                                        mathend = ReadMemoryQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // CMP reg, adr
                                        mathend = ReadMemoryPointedQWord(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // CMP reg, ptr
                                        mathend = ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Comparison, opcodeLow));
                                }

                                if (opcodeLow >= 0x4)
                                {
                                    result = initial - mathend;
                                    if (result > initial)
                                    {
                                        Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                    }
                                    else
                                    {
                                        Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                    }

#if EXTENSION_SET_SIGNED
                                    initialSign = initial & SignBit;
                                    resultSign = result & SignBit;
                                    if (initialSign != (mathend & SignBit) && initialSign != resultSign)
                                    {
                                        Registers[(int)Register.rsf] |= (ulong)StatusFlags.Overflow;
                                    }
                                    else
                                    {
                                        Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;
                                    }
#endif
                                }
                                else
                                {
                                    result = initial & mathend;
#if EXTENSION_SET_SIGNED
                                    resultSign = result & SignBit;
#endif
                                }

#if EXTENSION_SET_SIGNED
                                if (resultSign != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x80:  // Small Move
                                switch (opcodeLow)
                                {
                                    case 0x0:  // MVB reg, reg
                                        WriteMemoryRegister(operandStart, 0xFF & ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // MVB reg, lit
                                        WriteMemoryRegister(operandStart, 0xFF & ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // MVB reg, adr
                                        WriteMemoryRegister(operandStart, ReadMemoryPointedByte(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // MVB reg, ptr
                                        WriteMemoryRegister(operandStart, ReadMemoryRegisterPointedByte(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // MVB adr, reg
                                        WriteMemoryPointedByte(operandStart, (byte)(0xFF & ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x5:  // MVB adr, lit
                                        WriteMemoryPointedByte(operandStart, (byte)(0xFF & ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0x6:  // MVB ptr, reg
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Memory[result] = (byte)(0xFF & ReadMemoryRegister(operandStart + byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x7:  // MVB ptr, lit
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Memory[result] = (byte)(0xFF & ReadMemoryQWord(operandStart + byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x8:  // MVW reg, reg
                                        WriteMemoryRegister(operandStart, 0xFFFF & ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // MVW reg, lit
                                        WriteMemoryRegister(operandStart, 0xFFFF & ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // MVW reg, adr
                                        WriteMemoryRegister(operandStart, ReadMemoryPointedWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // MVW reg, ptr
                                        WriteMemoryRegister(operandStart, ReadMemoryRegisterPointedWord(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xC:  // MVW adr, reg
                                        WriteMemoryPointedWord(operandStart, (ushort)(0xFFFF & ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xD:  // MVW adr, lit
                                        WriteMemoryPointedWord(operandStart, (ushort)(0xFFFF & ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0xE:  // MVW ptr, reg
                                        WriteMemoryWord(ReadMemoryPointer(operandStart, out byteCount), (ushort)(0xFFFF & ReadMemoryRegister(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xF:  // MVW ptr, lit
                                        WriteMemoryWord(ReadMemoryPointer(operandStart, out byteCount), (ushort)(0xFFFF & ReadMemoryQWord(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_SmallMove, opcodeLow));
                                }
                                break;
                            case 0x90:  // Large Move
                                switch (opcodeLow)
                                {
                                    case 0x0:  // MVD reg, reg
                                        WriteMemoryRegister(operandStart, 0xFFFFFFFF & ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // MVD reg, lit
                                        WriteMemoryRegister(operandStart, 0xFFFFFFFF & ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // MVD reg, adr
                                        WriteMemoryRegister(operandStart, ReadMemoryPointedDWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // MVD reg, ptr
                                        WriteMemoryRegister(operandStart, ReadMemoryRegisterPointedDWord(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // MVD adr, reg
                                        WriteMemoryPointedDWord(operandStart, (uint)(0xFFFFFFFF & ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x5:  // MVD adr, lit
                                        WriteMemoryPointedDWord(operandStart, (uint)(0xFFFFFFFF & ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0x6:  // MVD ptr, reg
                                        WriteMemoryDWord(ReadMemoryPointer(operandStart, out byteCount), (uint)(0xFFFFFFFF & ReadMemoryRegister(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x7:  // MVD ptr, lit
                                        WriteMemoryDWord(ReadMemoryPointer(operandStart, out byteCount), (uint)(0xFFFFFFFF & ReadMemoryQWord(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x8:  // MVQ reg, reg
                                        WriteMemoryRegister(operandStart, ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // MVQ reg, lit
                                        WriteMemoryRegister(operandStart, ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // MVQ reg, adr
                                        WriteMemoryRegister(operandStart, ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // MVQ reg, ptr
                                        WriteMemoryRegister(operandStart, ReadMemoryRegisterPointedQWord(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xC:  // MVQ adr, reg
                                        WriteMemoryPointedQWord(operandStart, ReadMemoryRegister(operandStart + 8));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xD:  // MVQ adr, lit
                                        WriteMemoryPointedQWord(operandStart, ReadMemoryQWord(operandStart + 8));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0xE:  // MVQ ptr, reg
                                        WriteMemoryQWord(ReadMemoryPointer(operandStart, out byteCount), ReadMemoryRegister(operandStart + byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xF:  // MVQ ptr, lit
                                        WriteMemoryQWord(ReadMemoryPointer(operandStart, out byteCount), ReadMemoryQWord(operandStart + byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_LargeMove, opcodeLow));
                                }
                                break;
                            case 0xA0:  // Stack
                                switch (opcodeLow)
                                {
                                    case 0x0:  // PSH reg
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryRegister(operandStart));
                                        Registers[(int)Register.rso] -= 8;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // PSH lit
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryQWord(operandStart));
                                        Registers[(int)Register.rso] -= 8;
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // PSH adr
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryPointedQWord(operandStart));
                                        Registers[(int)Register.rso] -= 8;
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // PSH ptr
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryRegisterPointedNumber(operandStart, out byteCount));
                                        Registers[(int)Register.rso] -= 8;
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // POP reg
                                        WriteMemoryRegister(operandStart, ReadMemoryQWord(Registers[(int)Register.rso]));
                                        Registers[(int)Register.rso] += 8;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Stack, opcodeLow));
                                }
                                break;
                            case 0xB0:  // Subroutines
                                switch (opcodeLow)
                                {
                                    case 0x0:  // CAL adr
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x1:  // CAL ptr
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + byteCount);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = result;
                                        break;
                                    case 0x2:  // CAL adr, reg
                                        Registers[(int)Register.rfp] = ReadMemoryRegister(operandStart + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + 9);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x3:  // CAL adr, lit
                                        Registers[(int)Register.rfp] = ReadMemoryQWord(operandStart + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + 16);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x4:  // CAL adr, adr
                                        Registers[(int)Register.rfp] = ReadMemoryPointedQWord(operandStart + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + 16);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x5:  // CAL adr, ptr
                                        Registers[(int)Register.rfp] = ReadMemoryRegisterPointedNumber(operandStart + 8, out _);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + 9);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        break;
                                    case 0x6:  // CAL ptr, reg
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Registers[(int)Register.rfp] = ReadMemoryRegister(operandStart + byteCount);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + byteCount + 1);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = result;
                                        break;
                                    case 0x7:  // CAL ptr, lit
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Registers[(int)Register.rfp] = ReadMemoryQWord(operandStart + byteCount);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + byteCount + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = result;
                                        break;
                                    case 0x8:  // CAL ptr, adr
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Registers[(int)Register.rfp] = ReadMemoryPointedQWord(operandStart + byteCount);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + byteCount + 8);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = result;
                                        break;
                                    case 0x9:  // CAL ptr, ptr
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        Registers[(int)Register.rfp] = ReadMemoryRegisterPointedNumber(operandStart + byteCount, out initial);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 8, operandStart + byteCount + initial);
                                        WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
#if V1_CALL_STACK_COMPAT
                                        if (UseV1CallStack)
                                        {
                                            WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                        }
#endif
                                        Registers[(int)Register.rso] -= stackCallSize;
                                        Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                        Registers[(int)Register.rpo] = result;
                                        break;
                                    case 0xA:  // RET
                                        Registers[(int)Register.rso] += stackCallSize;
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                        Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                        break;
                                    case 0xB:  // RET reg
                                        Registers[(int)Register.rrv] = ReadMemoryRegister(operandStart);
                                        Registers[(int)Register.rso] += stackCallSize;
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                        Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                        break;
                                    case 0xC:  // RET lit
                                        Registers[(int)Register.rrv] = ReadMemoryQWord(operandStart);
                                        Registers[(int)Register.rso] += stackCallSize;
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                        Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                        break;
                                    case 0xD:  // RET adr
                                        Registers[(int)Register.rrv] = ReadMemoryPointedQWord(operandStart);
                                        Registers[(int)Register.rso] += stackCallSize;
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                        Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                        break;
                                    case 0xE:  // RET ptr
                                        Registers[(int)Register.rrv] = ReadMemoryRegisterPointedNumber(operandStart, out _);
                                        Registers[(int)Register.rso] += stackCallSize;
                                        Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                        Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Subroutine, opcodeLow));
                                }
                                break;
                            case 0xC0:  // Console Write
                                switch (opcodeLow)
                                {
                                    case 0x0:  // WCN reg
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryRegister(operandStart).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // WCN lit
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryQWord(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // WCN adr
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryPointedQWord(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // WCN ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryRegisterPointedNumber(operandStart, out byteCount).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // WCB reg
                                        stdout.Write(Encoding.UTF8.GetBytes((0xFF & ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // WCB lit
                                        stdout.Write(Encoding.UTF8.GetBytes(Memory[operandStart].ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // WCB adr
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryPointedByte(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // WCB ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(ReadMemoryRegisterPointedByte(operandStart, out byteCount).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x8:  // WCX reg
                                        stdout.Write(Encoding.UTF8.GetBytes(string.Format(Strings.Generic_Hex_Value, 0xFF & ReadMemoryRegister(operandStart))));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x9:  // WCX lit
                                        stdout.Write(Encoding.UTF8.GetBytes(string.Format(Strings.Generic_Hex_Value, Memory[operandStart])));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xA:  // WCX adr
                                        stdout.Write(Encoding.UTF8.GetBytes(string.Format(Strings.Generic_Hex_Value, ReadMemoryPointedByte(operandStart))));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xB:  // WCX ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(string.Format(Strings.Generic_Hex_Value, ReadMemoryRegisterPointedByte(operandStart, out byteCount))));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0xC:  // WCC reg
                                    {
                                        stdout.WriteByte((byte)(0xFF & ReadMemoryRegister(operandStart)));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    }
                                    case 0xD:  // WCC lit
                                    {
                                        stdout.WriteByte(Memory[operandStart]);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    }
                                    case 0xE:  // WCC adr
                                    {
                                        stdout.WriteByte(ReadMemoryPointedByte(operandStart));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    }
                                    case 0xF:  // WCC ptr
                                    {
                                        stdout.WriteByte(ReadMemoryRegisterPointedByte(operandStart, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    }
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_ConsoleWrite, opcodeLow));
                                }
                                break;
                            case 0xD0:  // File Write
                                if (openFile is null)
                                {
                                    throw new FileOperationException(Strings_Processor.Error_File_No_Open);
                                }
                                switch (opcodeLow)
                                {
                                    case 0x0:  // WFN reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryRegister(operandStart).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // WFN lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryQWord(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // WFN adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryPointedQWord(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // WFN ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryRegisterPointedNumber(operandStart, out byteCount).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // WFB reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes((0xFF & ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // WFB lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(Memory[operandStart].ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // WFB adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryPointedByte(operandStart).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // WFB ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(ReadMemoryRegisterPointedByte(operandStart, out byteCount).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x8:  // WFX reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(
                                            string.Format(Strings.Generic_Hex_Value, 0xFF & ReadMemoryRegister(operandStart))));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x9:  // WFX lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(string.Format(Strings.Generic_Hex_Value, Memory[operandStart])));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xA:  // WFX adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(
                                            string.Format(Strings.Generic_Hex_Value, ReadMemoryPointedByte(operandStart))));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xB:  // WFX ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(
                                            string.Format(Strings.Generic_Hex_Value, ReadMemoryRegisterPointedByte(operandStart, out byteCount))));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0xC:  // WFC reg
                                        fileWrite!.Write((byte)(0xFF & ReadMemoryRegister(operandStart)));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0xD:  // WFC lit
                                        fileWrite!.Write(Memory[operandStart]);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xE:  // WFC adr
                                        fileWrite!.Write(ReadMemoryPointedByte(operandStart));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0xF:  // WFC ptr
                                        fileWrite!.Write(ReadMemoryRegisterPointedByte(operandStart, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_FileWrite, opcodeLow));
                                }
                                break;
                            case 0xE0:  // File Operations
                                switch (opcodeLow)
                                {
                                    case 0x0:  // OFL adr
                                        if (openFile is not null)
                                        {
                                            throw new FileOperationException(Strings_Processor.Error_File_Already_Open);
                                        }
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                        openFileSize = openFile.Length;
                                        fileWrite = new BinaryWriter(openFile, Encoding.UTF8);
                                        fileRead = new BinaryReader(openFile, Encoding.UTF8);
                                        if (fileRead.BaseStream.Position >= openFileSize)
                                        {
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.FileEnd;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.FileEnd;
                                        }
                                        break;
                                    case 0x1:  // OFL ptr
                                        if (openFile is not null)
                                        {
                                            throw new FileOperationException(Strings_Processor.Error_File_Already_Open);
                                        }
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                        openFileSize = openFile.Length;
                                        fileWrite = new BinaryWriter(openFile, Encoding.UTF8);
                                        fileRead = new BinaryReader(openFile, Encoding.UTF8);
                                        if (fileRead.BaseStream.Position >= openFileSize)
                                        {
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.FileEnd;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.FileEnd;
                                        }
                                        break;
                                    case 0x2:  // CFL
                                        if (openFile is null)
                                        {
                                            throw new FileOperationException(Strings_Processor.Error_File_Close_None_Open);
                                        }
                                        fileWrite!.Close();
                                        fileWrite = null;
                                        fileRead!.Close();
                                        fileRead = null;
                                        openFile!.Close();
                                        openFile = null;
                                        openFileSize = 0;
                                        break;
                                    case 0x3:  // DFL adr
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        File.Delete(filepath);
                                        break;
                                    case 0x4:  // DFL ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        File.Delete(filepath);
                                        break;
                                    case 0x5:  // FEX reg, adr
                                        filepath = ReadMemoryPointedString(operandStart + 1);
                                        WriteMemoryRegister(operandStart, File.Exists(filepath) ? 1UL : 0UL);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // FEX reg, ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 1, out byteCount);
                                        WriteMemoryRegister(operandStart, File.Exists(filepath) ? 1UL : 0UL);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x7:  // FSZ reg, adr
                                        filepath = ReadMemoryPointedString(operandStart + 1);
                                        WriteMemoryRegister(operandStart, (ulong)new FileInfo(filepath).Length);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x8:  // FSZ reg, ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 1, out byteCount);
                                        WriteMemoryRegister(operandStart, (ulong)new FileInfo(filepath).Length);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_FileOperation, opcodeLow));
                                }
                                break;
                            case 0xF0:  // Reading
                                switch (opcodeLow)
                                {
                                    case 0x0:  // RCC reg
                                        {
                                            if (!stdinByteQueue.TryDequeue(out byte currentByte))
                                            {
                                                // There are no remaining queued UTF-8 bytes, so get the next character from the console.
                                                string inputCharacter = "";
                                                char pressedKeyChar;
                                                // C# gives us a single part of a UTF-16 surrogate pair if something outside the BMP is typed (i.e. an emoji).
                                                // If we detect the start of a surrogate pair, get another character as well to have the full character
                                                // to be converted to UTF-8.
                                                do
                                                {
                                                    if (stdinOverride is not null || Console.IsInputRedirected)
                                                    {
                                                        // The program input isn't connected to a terminal,
                                                        // so we can't wait for a pressed key - read the stdin stream directly instead
                                                        do
                                                        {
                                                            signedInitial = stdinOverride?.ReadByte() ?? stdin!.ReadByte();
                                                        } while (waitForInputChar && signedInitial == -1);
                                                        if (signedInitial == -1)
                                                        {
                                                            Registers[(int)Register.rpo] -= fullOpcode ? 3UL : 1UL;
                                                            return false;
                                                        }
                                                        pressedKeyChar = (char)signedInitial;
                                                        inputCharacter += pressedKeyChar;
                                                    }
                                                    else
                                                    {
                                                        ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                                                        pressedKeyChar = pressedKey.KeyChar;
                                                        // By default pressing enter will get \r, we want \n
                                                        inputCharacter += pressedKey.Key == ConsoleKey.Enter
                                                            ? '\n'
                                                            : pressedKeyChar;
                                                    }
                                                } while (char.IsHighSurrogate(pressedKeyChar));
                                                byte[] utf8Bytes = Encoding.UTF8.GetBytes(inputCharacter);
                                                currentByte = utf8Bytes[0];

                                                // Add remaining UTF-8 bytes to a queue to be retrieved by future RCC instructions
                                                if (utf8Bytes.Length > 1)
                                                {
                                                    for (int i = 1; i < utf8Bytes.Length; i++)
                                                    {
                                                        stdinByteQueue.Enqueue(utf8Bytes[i]);
                                                    }
                                                }
                                            }
#if EXTENSION_SET_TERMINAL
                                            if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.AutoEcho) != 0)
#else
                                            if (AutoEcho)
#endif
                                            {
                                                // Echo byte to console
                                                stdout.WriteByte(currentByte);
                                            }

                                            WriteMemoryRegister(operandStart, currentByte);
                                            Registers[(int)Register.rpo]++;
                                            break;
                                        }
                                    case 0x1:  // RFC reg
                                        if (fileRead is null)
                                        {
                                            throw new FileOperationException(Strings_Processor.Error_File_Read_No_Open);
                                        }
                                        if (fileRead.BaseStream.Position >= openFileSize)
                                        {
                                            throw new FileOperationException(Strings_Processor.Error_File_Read_End_Reached);
                                        }
                                        WriteMemoryRegister(operandStart, fileRead.ReadByte());
                                        if (fileRead.BaseStream.Position >= openFileSize)
                                        {
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.FileEnd;
                                        }
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Base_Reading, opcodeLow));
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_Base, opcodeHigh));
                        }
                        break;
#if EXTENSION_SET_SIGNED
                    case 0x01:  // Signed extension set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Jumps
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_JLT adr (Jump If Less Than - Sign Flag != Overflow Flag)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is (ulong)StatusFlags.Sign or (ulong)StatusFlags.Overflow)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x1:  // SIGN_JLT ptr (Jump If Less Than - Sign Flag != Overflow Flag)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is (ulong)StatusFlags.Sign or (ulong)StatusFlags.Overflow)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x2:  // SIGN_JLE adr (Jump If Less Than or Equal To - Sign Flag != Overflow Flag or Zero Flag Set)
                                        if (((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is (ulong)StatusFlags.Sign or (ulong)StatusFlags.Overflow)
                                            || (Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x3:  // SIGN_JLE ptr (Jump If Less Than or Equal To - Sign Flag != Overflow Flag or Zero Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if (((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is (ulong)StatusFlags.Sign or (ulong)StatusFlags.Overflow)
                                            || (Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x4:  // SIGN_JGT adr (Jump If Greater Than - Sign Flag == Overflow Flag and Zero Flag Unset)
                                        if (((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is not (ulong)StatusFlags.Sign and not (ulong)StatusFlags.Overflow)
                                            && (Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x5:  // SIGN_JGT ptr (Jump If Greater Than - Sign Flag == Overflow Flag and Zero Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if (((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is not (ulong)StatusFlags.Sign and not (ulong)StatusFlags.Overflow)
                                            && (Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x6:  // SIGN_JGE adr (Jump If Greater Than or Equal To - Sign Flag == Overflow Flag)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is not (ulong)StatusFlags.Sign and not (ulong)StatusFlags.Overflow)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x7:  // SIGN_JGE ptr (Jump If Greater Than or Equal To - Sign Flag == Overflow Flag)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.SignAndOverflow)
                                            is not (ulong)StatusFlags.Sign and not (ulong)StatusFlags.Overflow)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0x8:  // SIGN_JSI adr (Jump If Sign Flag Set)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Sign) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0x9:  // SIGN_JSI ptr (Jump If Sign Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Sign) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xA:  // SIGN_JNS adr (Jump If Sign Flag Unset)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Sign) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xB:  // SIGN_JNS ptr (Jump If Sign Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Sign) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xC:  // SIGN_JOV adr (Jump If Overflow Flag Set)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Overflow) != 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xD:  // SIGN_JOV ptr (Jump If Overflow Flag Set)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Overflow) != 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    case 0xE:  // SIGN_JNO adr (Jump If Overflow Flag Unset)
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Overflow) == 0)
                                        {
                                            Registers[(int)Register.rpo] = ReadMemoryQWord(operandStart);
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += 8;
                                        }
                                        break;
                                    case 0xF:  // SIGN_JNO ptr (Jump If Overflow Flag Unset)
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Overflow) == 0)
                                        {
                                            Registers[(int)Register.rpo] = result;
                                        }
                                        else
                                        {
                                            Registers[(int)Register.rpo] += byteCount;
                                        }
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_Jump, opcodeLow));
                                }
                                break;
                            case 0x10:  // Division
                                signedInitial = (long)ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_DIV reg, reg
                                        result = (ulong)(signedInitial / (long)ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SIGN_DIV reg, lit
                                        result = (ulong)(signedInitial / (long)ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SIGN_DIV reg, adr
                                        result = (ulong)(signedInitial / (long)ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SIGN_DIV reg, ptr
                                        result = (ulong)(signedInitial / (long)ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // SIGN_DVR reg, reg, reg
                                        signedMathend = (long)ReadMemoryRegister(operandStart + 2);
                                        result = (ulong)(signedInitial / signedMathend);
                                        remainder = (ulong)(signedInitial % signedMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 3;
                                        break;
                                    case 0x5:  // SIGN_DVR reg, reg, lit
                                        signedMathend = (long)ReadMemoryQWord(operandStart + 2);
                                        result = (ulong)(signedInitial / signedMathend);
                                        remainder = (ulong)(signedInitial % signedMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x6:  // SIGN_DVR reg, reg, adr
                                        signedMathend = (long)ReadMemoryPointedQWord(operandStart + 2);
                                        result = (ulong)(signedInitial / signedMathend);
                                        remainder = (ulong)(signedInitial % signedMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x7:  // SIGN_DVR reg, reg, ptr
                                        signedMathend = (long)ReadMemoryRegisterPointedNumber(operandStart + 2, out byteCount);
                                        result = (ulong)(signedInitial / signedMathend);
                                        remainder = (ulong)(signedInitial % signedMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += byteCount + 2;
                                        break;
                                    case 0x8:  // SIGN_REM reg, reg
                                        result = (ulong)(signedInitial % (long)ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // SIGN_REM reg, lit
                                        result = (ulong)(signedInitial % (long)ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // SIGN_REM reg, adr
                                        result = (ulong)(signedInitial % (long)ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // SIGN_REM reg, ptr
                                        result = (ulong)(signedInitial % (long)ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_Division, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x20:  // Shifting
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_SHR reg, reg
                                        shiftAmount = (int)ReadMemoryRegister(operandStart + 1);
                                        result = (ulong)((long)initial >> shiftAmount);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SIGN_SHR reg, lit
                                        shiftAmount = (int)ReadMemoryQWord(operandStart + 1);
                                        result = (ulong)((long)initial >> shiftAmount);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SIGN_SHR reg, adr
                                        shiftAmount = (int)ReadMemoryPointedQWord(operandStart + 1);
                                        result = (ulong)((long)initial >> shiftAmount);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SIGN_SHR reg, ptr
                                        shiftAmount = (int)ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount);
                                        result = (ulong)((long)initial >> shiftAmount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_Shifting, opcodeLow));
                                }
                                initialSign = initial & SignBit;
                                signedShiftAllBits = initialSign == 0 ? 0 : (~0UL);
                                // C# only counts the lower 6 bits of the amount to shift by, so values greater than or equal to 64 will not return 0/~0 as
                                // wanted for AssEmbly.
                                if (shiftAmount >= 64)
                                {
                                    result = signedShiftAllBits;
                                }
                                WriteMemoryRegister(operandStart, result);

                                // We will never overflow when shifting by 0 bits or if the initial value's bits are all equal to the sign bit.
                                // We will always overflow if shifting by 64 bits or more as long as the above isn't the case.
                                //
                                // Otherwise, "(initial << (64 - amount)) != (signedShiftAllBits << (64 - shiftAmount))" checks if there are any bits
                                // not equal to the sign bit in the portion of the number that will be cutoff during the right shift by
                                // cutting off the bits that will remain.
                                // 8-bit e.g: 0b11001001 >> 3 |> (0b11001001 << (8 - 3)), (0b11001001 << 5) = 0b00100000, result != 0, therefore set carry.
                                if (shiftAmount != 0 && initial != signedShiftAllBits
                                    && (shiftAmount >= 64 || (initial << (64 - shiftAmount)) != (signedShiftAllBits << (64 - shiftAmount))))
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;
                                break;
                            case 0x30:  // Small Sign-Preserving Move
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_MVB reg, reg
                                        WriteMemoryRegister(operandStart, (ulong)(sbyte)ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SIGN_MVB reg, lit
                                        WriteMemoryRegister(operandStart, (ulong)(sbyte)ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SIGN_MVB reg, adr
                                        WriteMemoryRegister(operandStart, (ulong)(sbyte)ReadMemoryPointedByte(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SIGN_MVB reg, ptr
                                        WriteMemoryRegister(operandStart, (ulong)(sbyte)ReadMemoryRegisterPointedByte(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // SIGN_MVW reg, reg
                                        WriteMemoryRegister(operandStart, (ulong)(short)ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // SIGN_MVW reg, lit
                                        WriteMemoryRegister(operandStart, (ulong)(short)ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // SIGN_MVW reg, adr
                                        WriteMemoryRegister(operandStart, (ulong)(short)ReadMemoryPointedWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // SIGN_MVW reg, ptr
                                        WriteMemoryRegister(operandStart, (ulong)(short)ReadMemoryRegisterPointedWord(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_SmallMove, opcodeLow));
                                }
                                break;
                            case 0x40:  // Large Sign-Preserving Move
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_MVD reg, reg
                                        WriteMemoryRegister(operandStart, (ulong)(int)ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // SIGN_MVD reg, lit
                                        WriteMemoryRegister(operandStart, (ulong)(int)ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // SIGN_MVD reg, adr
                                        WriteMemoryRegister(operandStart, (ulong)(int)ReadMemoryPointedDWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // SIGN_MVD reg, ptr
                                        WriteMemoryRegister(operandStart, (ulong)(int)ReadMemoryRegisterPointedDWord(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_LargeMove, opcodeLow));
                                }
                                break;
                            case 0x50:  // Console Write
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_WCN reg
                                        stdout.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // SIGN_WCN lit
                                        stdout.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryQWord(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // SIGN_WCN adr
                                        stdout.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryPointedQWord(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // SIGN_WCN ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryRegisterPointedNumber(operandStart, out byteCount)).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // SIGN_WCB reg
                                        stdout.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // SIGN_WCB lit
                                        stdout.Write(Encoding.UTF8.GetBytes(((sbyte)Memory[operandStart]).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // SIGN_WCB adr
                                        stdout.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryPointedByte(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // SIGN_WCB ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryRegisterPointedByte(operandStart, out byteCount)).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_ConsoleWrite, opcodeLow));
                                }
                                break;
                            case 0x60:  // File Write
                                if (openFile is null)
                                {
                                    throw new FileOperationException(Strings_Processor.Error_File_No_Open);
                                }
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_WFN reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // SIGN_WFN lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryQWord(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // SIGN_WFN adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryPointedQWord(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // SIGN_WFN ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((long)ReadMemoryRegisterPointedNumber(operandStart, out byteCount)).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // SIGN_WFB reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryRegister(operandStart)).ToString()));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // SIGN_WFB lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((sbyte)Memory[operandStart]).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // SIGN_WFB adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryPointedByte(operandStart)).ToString()));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // SIGN_WFB ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(((sbyte)ReadMemoryRegisterPointedByte(operandStart, out byteCount)).ToString()));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_FileWrite, opcodeLow));
                                }
                                break;
                            case 0x70:  // Extend
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_EXB reg
                                        result = (ulong)(sbyte)initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // SIGN_EXW reg
                                        result = (ulong)(short)initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x2:  // SIGN_EXD reg
                                        result = (ulong)(int)initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_Extend, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x80:  // Negate
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // SIGN_NEG reg
                                        result = (ulong)-(long)initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Signed_Negate, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_Signed, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_FLOATING_POINT
                    case 0x2:  // Floating point extension set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Addition
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_ADD reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_ADD reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_ADD reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_ADD reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Addition, opcodeLow));
                                }
                                floatingResult = floatingInitial + floatingMathend;
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                if (floatingResult < floatingInitial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x10:  // Subtraction
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_SUB reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_SUB reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_SUB reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_SUB reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Subtraction, opcodeLow));
                                }
                                floatingResult = floatingInitial - floatingMathend;
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                if (floatingResult > floatingInitial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x20:  // Multiplication
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_MUL reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_MUL reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_MUL reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_MUL reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Multiplication, opcodeLow));
                                }
                                floatingResult = floatingInitial * floatingMathend;
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                if (floatingResult < floatingInitial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x30:  // Division
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_DIV reg, reg
                                        floatingResult = floatingInitial / BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_DIV reg, lit
                                        floatingResult = floatingInitial / BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_DIV reg, adr
                                        floatingResult = floatingInitial / BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_DIV reg, ptr
                                        floatingResult = floatingInitial / BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // FLPT_DVR reg, reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 2));
                                        floatingResult = floatingInitial / floatingMathend;
                                        remainder = BitConverter.DoubleToUInt64Bits(floatingInitial % floatingMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 3;
                                        break;
                                    case 0x5:  // FLPT_DVR reg, reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 2));
                                        floatingResult = floatingInitial / floatingMathend;
                                        remainder = BitConverter.DoubleToUInt64Bits(floatingInitial % floatingMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x6:  // FLPT_DVR reg, reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 2));
                                        floatingResult = floatingInitial / floatingMathend;
                                        remainder = BitConverter.DoubleToUInt64Bits(floatingInitial % floatingMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += 10;
                                        break;
                                    case 0x7:  // FLPT_DVR reg, reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 2, out byteCount));
                                        floatingResult = floatingInitial / floatingMathend;
                                        remainder = BitConverter.DoubleToUInt64Bits(floatingInitial % floatingMathend);
                                        WriteMemoryRegister(operandStart + 1, remainder);
                                        Registers[(int)Register.rpo] += byteCount + 2;
                                        break;
                                    case 0x8:  // FLPT_REM reg, reg
                                        floatingResult = floatingInitial % BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x9:  // FLPT_REM reg, lit
                                        floatingResult = floatingInitial % BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xA:  // FLPT_REM reg, adr
                                        floatingResult = floatingInitial % BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0xB:  // FLPT_REM reg, ptr
                                        floatingResult = floatingInitial % BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Division, opcodeLow));
                                }
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x40:  // Trigonometric functions
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_SIN reg
                                        floatingResult = Math.Sin(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_ASN reg
                                        floatingResult = Math.Asin(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x2:  // FLPT_COS reg
                                        floatingResult = Math.Cos(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x3:  // FLPT_ACS reg
                                        floatingResult = Math.Acos(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x4:  // FLPT_TAN reg
                                        floatingResult = Math.Tan(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // FLPT_ATN reg
                                        floatingResult = Math.Atan(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x6:  // FLPT_PTN reg, reg
                                        floatingResult = Math.Atan2(floatingInitial, BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1)));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x7:  // FLPT_PTN reg, lit
                                        floatingResult = Math.Atan2(floatingInitial, BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x8:  // FLPT_PTN reg, adr
                                        floatingResult = Math.Atan2(floatingInitial, BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x9:  // FLPT_PTN reg, ptr
                                        floatingResult = Math.Atan2(floatingInitial, BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Trigonometry, opcodeLow));
                                }
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x50:  // Exponentiation
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_POW reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_POW reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_POW reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_POW reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Exponentiation, opcodeLow));
                                }
                                floatingResult = Math.Pow(floatingInitial, floatingMathend);
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                if (floatingResult < floatingInitial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x60:  // Logarithm
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_LOG reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_LOG reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_LOG reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_LOG reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Logarithm, opcodeLow));
                                }
                                floatingResult = Math.Log(floatingInitial, floatingMathend);
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                if (floatingResult > floatingInitial)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0x70:  // Console Write
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_WCN reg
                                        stdout.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_WCN lit
                                        stdout.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // FLPT_WCN adr
                                        stdout.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // FLPT_WCN ptr
                                        stdout.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart, out byteCount)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_ConsoleWrite, opcodeLow));
                                }
                                break;
                            case 0x80:  // File Write
                                if (openFile is null)
                                {
                                    throw new FileOperationException(Strings_Processor.Error_File_No_Open);
                                }
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_WFN reg
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_WFN lit
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // FLPT_WFN adr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // FLPT_WFN ptr
                                        fileWrite!.Write(Encoding.UTF8.GetBytes(BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart, out byteCount)).ToString(CultureInfo.InvariantCulture)));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_FileWrite, opcodeLow));
                                }
                                break;
                            case 0x90:  // Float Size Conversions
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_EXH reg
                                        result = BitConverter.DoubleToUInt64Bits((double)BitConverter.UInt16BitsToHalf((ushort)initial));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_EXS reg
                                        result = BitConverter.DoubleToUInt64Bits(BitConverter.UInt32BitsToSingle((uint)initial));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x2:  // FLPT_SHS reg
                                        result = BitConverter.SingleToUInt32Bits((float)BitConverter.UInt64BitsToDouble(initial));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x3:  // FLPT_SHH reg
                                        result = BitConverter.HalfToUInt16Bits((Half)BitConverter.UInt64BitsToDouble(initial));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_SizeConversion, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0xA0:  // Negate
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_NEG reg
                                        floatingResult = -floatingInitial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Negate, opcodeLow));
                                }
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0xB0:  // Integer to Floating Point Conversion
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_UTF reg
                                        floatingResult = initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_STF reg
                                        floatingResult = (long)initial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_IntToFloat, opcodeLow));
                                }
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0xC0:  // Floating Point to Integer Conversion
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    // "(ulong)(long)" cast is used as casting negative floating point values directly to unsigned integers
                                    // causes unexpected results on some platforms, so a signed integer must be used as an intermediary.
                                    case 0x0:  // FLPT_FTS reg
                                        result = (ulong)(long)floatingInitial;
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // FLPT_FCS reg
                                        result = (ulong)(long)Math.Ceiling(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x2:  // FLPT_FFS reg
                                        result = (ulong)(long)Math.Floor(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x3:  // FLPT_FNS reg
                                        result = (ulong)(long)Math.Round(floatingInitial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_FloatToInt, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;

#if EXTENSION_SET_SIGNED
                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (result == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            case 0xD0:  // Comparison
                                floatingInitial = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart));
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FLPT_CMP reg, reg
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegister(operandStart + 1));
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // FLPT_CMP reg, lit
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // FLPT_CMP reg, adr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryPointedQWord(operandStart + 1));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FLPT_CMP reg, ptr
                                        floatingMathend = BitConverter.UInt64BitsToDouble(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FloatingPoint_Comparison, opcodeLow));
                                }
                                floatingResult = floatingInitial - floatingMathend;

                                if (floatingInitial < floatingMathend)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                                }

#if EXTENSION_SET_SIGNED
                                result = BitConverter.DoubleToUInt64Bits(floatingResult);

                                Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Overflow;

                                if ((result & SignBit) != 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Sign;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Sign;
                                }
#endif

                                if (floatingResult == 0)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                }
                                else
                                {
                                    Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_FloatingPoint, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_EXTENDED_BASE
                    case 0x3:  // Extended base set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Byte swap
                                initial = ReadMemoryRegister(operandStart);
                                switch (opcodeLow)
                                {
                                    case 0x0:  // EXTD_BSW reg
                                        result = BinaryPrimitives.ReverseEndianness(initial);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Extended_ByteSwap, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, result);
                                break;
                            case 0x10:  // Processor Info Query
                                switch (opcodeLow)
                                {
                                    case 0x0:  // EXTD_QPF reg
                                        WriteMemoryRegister(operandStart, (ulong)AAPFeatures.All);
                                        Registers[(ulong)Register.rpo]++;
                                        break;
                                    case 0x1:  // EXTD_QPV reg
                                        WriteMemoryRegister(operandStart, (ulong)(typeof(Processor).Assembly.GetName().Version?.Major ?? 0));
                                        Registers[(ulong)Register.rpo]++;
                                        break;
                                    case 0x2:  // EXTD_QPV reg, reg
                                        WriteMemoryRegister(operandStart, (ulong)(typeof(Processor).Assembly.GetName().Version?.Major ?? 0));
                                        WriteMemoryRegister(operandStart + 1, (ulong)(typeof(Processor).Assembly.GetName().Version?.Minor ?? 0));
                                        Registers[(ulong)Register.rpo] += 2;
                                        break;
                                    case 0x3:  // EXTD_CSS reg
                                        WriteMemoryRegister(operandStart, stackCallSize);
                                        Registers[(ulong)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Extended_InfoQuery, opcodeLow));
                                }
                                break;
                            case 0x20:  // Halt with Exit Code
                                switch (opcodeLow)
                                {
                                    case 0x0:  // EXTD_HLT reg
                                        result = ReadMemoryRegister(operandStart);
                                        Registers[(ulong)Register.rpo]++;
                                        break;
                                    case 0x1:  // EXTD_HLT lit
                                        result = ReadMemoryQWord(operandStart);
                                        Registers[(ulong)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // EXTD_HLT adr
                                        result = ReadMemoryPointedQWord(operandStart);
                                        Registers[(ulong)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // EXTD_HLT ptr
                                        result = ReadMemoryRegisterPointedNumber(operandStart, out byteCount);
                                        Registers[(ulong)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Extended_Halt, opcodeLow));
                                }
                                Environment.ExitCode = (int)result;
                                halt = true;
                                break;
                            case 0x30:  // Move Pointer Address
                                switch (opcodeLow)
                                {
                                    case 0x0:  // EXTD_MPA reg, ptr
                                        WriteMemoryRegister(operandStart, ReadMemoryPointer(operandStart + 1, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x1:  // EXTD_MPA adr, ptr
                                        WriteMemoryPointedQWord(operandStart, ReadMemoryPointer(operandStart + 8, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x2:  // EXTD_MPA ptr, ptr
                                        result = ReadMemoryPointer(operandStart, out byteCount);
                                        WriteMemoryQWord(result, ReadMemoryPointer(operandStart + byteCount, out initial));
                                        Registers[(int)Register.rpo] += byteCount + initial;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Extended_MovePointerAddress, opcodeLow));
                                }
                                break;
                            case 0x40:  // Sleep
                                switch (opcodeLow)
                                {
                                    case 0x0:  // EXTD_SLP reg
                                        result = ReadMemoryRegister(operandStart);
                                        Registers[(ulong)Register.rpo]++;
                                        break;
                                    case 0x1:  // EXTD_SLP lit
                                        result = ReadMemoryQWord(operandStart);
                                        Registers[(ulong)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // EXTD_SLP adr
                                        result = ReadMemoryPointedQWord(operandStart);
                                        Registers[(ulong)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // EXTD_SLP ptr
                                        result = ReadMemoryRegisterPointedNumber(operandStart, out byteCount);
                                        Registers[(ulong)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Extended_Halt, opcodeLow));
                                }
                                Thread.Sleep(TimeSpan.FromMilliseconds(result));
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_Extended, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_EXTERNAL_ASM
                    case 0x4:  // External Assembly Extension Set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Load
                                switch (opcodeLow)
                                {
                                    case 0x0:  // ASMX_LDA adr
                                        if (extLoadContext is not null || openExtAssembly is not null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Already_Open);
                                        }
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        try
                                        {
                                            extLoadContext = new AssemblyLoadContext("AssEmblyExternal", true);
                                            extLoadContext.Resolving += ExtLoadContext_Resolving;
                                            openExtAssembly = extLoadContext.LoadFromAssemblyPath(Path.GetFullPath(filepath)).GetType("AssEmblyInterop");
                                        }
                                        catch (Exception e)
                                        {
                                            extLoadContext?.Unload();
                                            extLoadContext = null;
                                            throw e switch
                                            {
                                                BadImageFormatException => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Invalid),
                                                FileNotFoundException => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Not_Found),
                                                _ => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Unknown)
                                            };
                                        }
                                        if (openExtAssembly is null)
                                        {
                                            extLoadContext?.Unload();
                                            extLoadContext = null;
                                            throw new InvalidAssemblyException(Strings_Processor.Error_Assembly_No_Type);
                                        }
                                        break;
                                    case 0x1:  // ASMX_LDA ptr
                                        if (extLoadContext is not null || openExtAssembly is not null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Already_Open);
                                        }
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        try
                                        {
                                            extLoadContext = new AssemblyLoadContext("AssEmblyExternal", true);
                                            extLoadContext.Resolving += ExtLoadContext_Resolving;
                                            openExtAssembly = extLoadContext.LoadFromAssemblyPath(Path.GetFullPath(filepath)).GetType("AssEmblyInterop");
                                        }
                                        catch (Exception e)
                                        {
                                            extLoadContext?.Unload();
                                            extLoadContext = null;
                                            throw e switch
                                            {
                                                BadImageFormatException => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Invalid),
                                                FileNotFoundException => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Not_Found),
                                                _ => new InvalidAssemblyException(
                                                    Strings_Processor.Error_Assembly_Unknown)
                                            };
                                        }
                                        if (openExtAssembly is null)
                                        {
                                            extLoadContext?.Unload();
                                            extLoadContext = null;
                                            throw new InvalidAssemblyException(Strings_Processor.Error_Assembly_No_Type);
                                        }
                                        break;
                                    case 0x2:  // ASMX_LDF adr
                                        if (extLoadContext is null || openExtAssembly is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                        }
                                        if (openExtFunction is not null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Function_Already_Open);
                                        }
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        try
                                        {
                                            openExtFunction = openExtAssembly.GetMethod(filepath, BindingFlags.Public | BindingFlags.Static, ExternalMethodParamTypes);
                                        }
                                        catch
                                        {
                                            throw new InvalidFunctionException(Strings_Processor.Error_Function_Unknown);
                                        }
                                        if (openExtFunction is null)
                                        {
                                            throw new InvalidFunctionException(Strings_Processor.Error_Function_Invalid);
                                        }
                                        break;
                                    case 0x3:  // ASMX_LDF ptr
                                        if (extLoadContext is null || openExtAssembly is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                        }
                                        if (openExtFunction is not null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Function_Already_Open);
                                        }
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        try
                                        {
                                            openExtFunction = openExtAssembly.GetMethod(filepath, BindingFlags.Public | BindingFlags.Static, ExternalMethodParamTypes);
                                        }
                                        catch
                                        {
                                            throw new InvalidFunctionException(Strings_Processor.Error_Function_Unknown);
                                        }
                                        if (openExtFunction is null)
                                        {
                                            throw new InvalidFunctionException(Strings_Processor.Error_Function_Invalid);
                                        }
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_External_Load, opcodeLow));
                                }
                                break;
                            case 0x10:  // Close
                                switch (opcodeLow)
                                {
                                    case 0x0:  // ASMX_CLA
                                        if (extLoadContext is null && openExtAssembly is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                        }
                                        extLoadContext?.Unload();
                                        extLoadContext = null;
                                        openExtAssembly = null;
                                        openExtFunction = null;
                                        break;
                                    case 0x1:  // ASMX_CLF
                                        if (openExtFunction is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Function_Not_Open);
                                        }
                                        openExtFunction = null;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_External_Close, opcodeLow));
                                }
                                break;
                            case 0x20:  // Exists
                                switch (opcodeLow)
                                {
                                    case 0x0:  // ASMX_AEX reg, adr
                                        filepath = ReadMemoryPointedString(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        AssemblyLoadContext testLoadContext = new("AssEmblyExternalTest", true);
                                        try
                                        {
                                            _ = testLoadContext.LoadFromAssemblyPath(Path.GetFullPath(filepath)).GetType("AssEmblyInterop");
                                            WriteMemoryRegister(operandStart, 1);
                                        }
                                        catch
                                        {
                                            WriteMemoryRegister(operandStart, 0);
                                        }
                                        finally
                                        {
                                            testLoadContext.Unload();
                                        }
                                        break;
                                    case 0x1:  // ASMX_AEX reg, ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        testLoadContext = new AssemblyLoadContext("AssEmblyExternalTest", true);
                                        try
                                        {
                                            _ = testLoadContext.LoadFromAssemblyPath(Path.GetFullPath(filepath)).GetType("AssEmblyInterop");
                                            WriteMemoryRegister(operandStart, 1);
                                        }
                                        catch
                                        {
                                            WriteMemoryRegister(operandStart, 0);
                                        }
                                        finally
                                        {
                                            testLoadContext.Unload();
                                        }
                                        break;
                                    case 0x2:  // ASMX_FEX reg, adr
                                        if (extLoadContext is null || openExtAssembly is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                        }
                                        filepath = ReadMemoryPointedString(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        try
                                        {
                                            MethodInfo? method = openExtAssembly.GetMethod(filepath, BindingFlags.Public | BindingFlags.Static, ExternalMethodParamTypes);
                                            WriteMemoryRegister(operandStart, method is null ? 0UL : 1UL);
                                        }
                                        catch
                                        {
                                            WriteMemoryRegister(operandStart, 0);
                                        }
                                        break;
                                    case 0x3:  // ASMX_FEX reg, ptr
                                        if (extLoadContext is null || openExtAssembly is null)
                                        {
                                            throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                        }
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        try
                                        {
                                            MethodInfo? method = openExtAssembly.GetMethod(filepath, BindingFlags.Public | BindingFlags.Static, ExternalMethodParamTypes);
                                            WriteMemoryRegister(operandStart, method is null ? 0UL : 1UL);
                                        }
                                        catch
                                        {
                                            WriteMemoryRegister(operandStart, 0);
                                        }
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_External_Existence, opcodeLow));
                                }
                                break;
                            case 0x30:  // Call
                                if (extLoadContext is null || openExtAssembly is null)
                                {
                                    throw new ExternalOperationException(Strings_Processor.Error_Assembly_Not_Open);
                                }
                                if (openExtFunction is null)
                                {
                                    throw new ExternalOperationException(Strings_Processor.Error_Function_Not_Open);
                                }
                                try
                                {
                                    switch (opcodeLow)
                                    {
                                        case 0x0:  // ASMX_CAL
                                            // null is used as obj parameter as method is static
                                            _ = openExtFunction.Invoke(null, new object?[3] { Memory, Registers, null });
                                            break;
                                        case 0x1:  // ASMX_CAL reg
                                            // null is used as obj parameter as method is static
                                            _ = openExtFunction.Invoke(null, new object?[3] { Memory, Registers, ReadMemoryRegister(operandStart) });
                                            Registers[(int)Register.rpo]++;
                                            break;
                                        case 0x2:  // ASMX_CAL lit
                                            // null is used as obj parameter as method is static
                                            _ = openExtFunction.Invoke(null, new object?[3] { Memory, Registers, ReadMemoryQWord(operandStart) });
                                            Registers[(int)Register.rpo] += 8;
                                            break;
                                        case 0x3:  // ASMX_CAL adr
                                            // null is used as obj parameter as method is static
                                            _ = openExtFunction.Invoke(null, new object?[3] { Memory, Registers, ReadMemoryPointedQWord(operandStart) });
                                            Registers[(int)Register.rpo] += 8;
                                            break;
                                        case 0x4:  // ASMX_CAL ptr
                                            // null is used as obj parameter as method is static
                                            _ = openExtFunction.Invoke(null, new object?[3] { Memory, Registers, ReadMemoryRegisterPointedNumber(operandStart, out byteCount) });
                                            Registers[(int)Register.rpo] += byteCount;
                                            break;
                                        default:
                                            throw new InvalidOpcodeException(
                                                string.Format(Strings_Processor.Error_Opcode_Low_External_Call, opcodeLow));
                                    }
                                }
                                catch (TargetInvocationException exc)
                                {
                                    throw new ExternalOperationException(string.Format(
                                        Strings_Processor.Error_External_Method, exc.InnerException?.Source, exc.InnerException?.TargetSite?.Name,
                                        exc.InnerException?.GetType(), exc.InnerException?.Message));
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_External, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
                    case 0x5:  // Memory Allocation Extension Set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Allocation
                                switch (opcodeLow)
                                {
                                    case 0x0:  // HEAP_ALC reg, reg
                                        result = AllocateMemory(ReadMemoryRegister(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // HEAP_ALC reg, lit
                                        result = AllocateMemory(ReadMemoryQWord(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // HEAP_ALC reg, adr
                                        result = AllocateMemory(ReadMemoryPointedQWord(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // HEAP_ALC reg, ptr
                                        result = AllocateMemory(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // HEAP_TRY reg, reg
                                        result = AllocateMemory(ReadMemoryRegister(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // HEAP_TRY reg, lit
                                        result = AllocateMemory(ReadMemoryQWord(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // HEAP_TRY reg, adr
                                        result = AllocateMemory(ReadMemoryPointedQWord(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // HEAP_TRY reg, ptr
                                        result = AllocateMemory(ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_Allocation_Allocation, opcodeLow));
                                }
                                break;
                            case 0x10:  // Re-allocation
                                switch (opcodeLow)
                                {
                                    case 0x0:  // HEAP_REA reg, reg
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryRegister(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        if (result == ulong.MaxValue - 1)
                                        {
                                            throw new InvalidMemoryBlockException(Strings_Processor.Error_Invalid_Memory_Block);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x1:  // HEAP_REA reg, lit
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryQWord(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        if (result == ulong.MaxValue - 1)
                                        {
                                            throw new InvalidMemoryBlockException(Strings_Processor.Error_Invalid_Memory_Block);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x2:  // HEAP_REA reg, adr
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryPointedQWord(operandStart + 1));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        if (result == ulong.MaxValue - 1)
                                        {
                                            throw new InvalidMemoryBlockException(Strings_Processor.Error_Invalid_Memory_Block);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // HEAP_REA reg, ptr
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        if (result == ulong.MaxValue)
                                        {
                                            throw new MemoryAllocationException(Strings_Processor.Error_Memory_Allocation);
                                        }
                                        if (result == ulong.MaxValue - 1)
                                        {
                                            throw new InvalidMemoryBlockException(Strings_Processor.Error_Invalid_Memory_Block);
                                        }
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // HEAP_TRE reg, reg
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryRegister(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 2;
                                        break;
                                    case 0x5:  // HEAP_TRE reg, lit
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryQWord(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x6:  // HEAP_TRE reg, adr
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryPointedQWord(operandStart + 1));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x7:  // HEAP_TRE reg, ptr
                                        result = ReallocateMemory(ReadMemoryRegister(operandStart), ReadMemoryRegisterPointedNumber(operandStart + 1, out byteCount));
                                        WriteMemoryRegister(operandStart, result);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_Allocation_Reallocation, opcodeLow));
                                }
                                break;
                            case 0x20:  // Free
                                switch (opcodeLow)
                                {
                                    case 0x0:  // HEAP_FRE reg
                                        if (!FreeMemory(ReadMemoryRegister(operandStart)))
                                        {
                                            throw new InvalidMemoryBlockException(Strings_Processor.Error_Invalid_Memory_Block);
                                        }
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(
                                            string.Format(Strings_Processor.Error_Opcode_Low_Allocation_Free, opcodeLow));
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_Allocation, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_FILE_SYSTEM
                    case 0x6:  // File System Extension Set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Current Working Directory
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_CWD adr
                                        Environment.CurrentDirectory = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x1:  // FSYS_CWD ptr
                                        Environment.CurrentDirectory = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x2:  // FSYS_GWD adr
                                        WriteMemoryPointedString(operandStart, Environment.CurrentDirectory);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // FSYS_GWD ptr
                                        WriteMemoryRegisterPointedString(operandStart, Environment.CurrentDirectory, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_WorkingDir, opcodeLow));
                                }
                                break;
                            case 0x10:  // Create Directory
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_CDR adr
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x1:  // FSYS_CDR ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_CreateDir, opcodeLow));
                                }
                                _ = Directory.CreateDirectory(filepath);
                                break;
                            case 0x20:  // Delete Directory
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_DDR adr
                                    case 0x2:  // FSYS_DDE adr
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x1:  // FSYS_DDR ptr
                                    case 0x3:  // FSYS_DDE ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_DeleteDir, opcodeLow));
                                }
                                if (Directory.Exists(filepath))
                                {
                                    Directory.Delete(filepath, opcodeLow <= 0x1);
                                }
                                break;
                            case 0x30:  // Directory Existence
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_DEX reg, adr
                                        filepath = ReadMemoryPointedString(operandStart + 1);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x1:  // FSYS_DEX reg, ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 1, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_DirectoryExists, opcodeLow));
                                }
                                WriteMemoryRegister(operandStart, Directory.Exists(filepath) ? 1UL : 0UL);
                                break;
                            case 0x40:  // Copy/Move
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_CPY adr, adr
                                    case 0x4:  // FSYS_MOV adr, adr
                                        destination = ReadMemoryPointedString(operandStart);
                                        filepath = ReadMemoryPointedString(operandStart + 8);
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0x1:  // FSYS_CPY adr, ptr
                                    case 0x5:  // FSYS_MOV adr, ptr
                                        destination = ReadMemoryPointedString(operandStart);
                                        filepath = ReadMemoryRegisterPointedString(operandStart + 8, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x2:  // FSYS_CPY ptr, adr
                                    case 0x6:  // FSYS_MOV ptr, adr
                                        destination = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        filepath = ReadMemoryPointedString(operandStart + byteCount);
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x3:  // FSYS_CPY ptr, ptr
                                    case 0x7:  // FSYS_MOV ptr, ptr
                                        destination = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        filepath = ReadMemoryRegisterPointedString(operandStart + byteCount, out initial);
                                        Registers[(int)Register.rpo] += byteCount + initial;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_Move, opcodeLow));
                                }
                                if (opcodeLow <= 0x03)
                                {
                                    File.Copy(filepath, destination);
                                }
                                else
                                {
                                    File.Move(filepath, destination);
                                }
                                break;
                            case 0x50:  // Directory Listing Start
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_BDL
                                        filepath = Environment.CurrentDirectory;
                                        break;
                                    case 0x1:  // FSYS_BDL adr
                                        filepath = ReadMemoryPointedString(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // FSYS_BDL ptr
                                        filepath = ReadMemoryRegisterPointedString(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_BeginListing, opcodeLow));
                                }
                                directoryListing.Dispose();
                                directoryListing = Directory.EnumerateDirectories(filepath).GetEnumerator();
                                fileListing.Dispose();
                                fileListing = Directory.EnumerateFiles(filepath).GetEnumerator();
                                break;
                            case 0x60:  // Get Items from Directory Listing
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_GNF adr
                                        if (fileListing.MoveNext())
                                        {
                                            filepath = Path.GetFileName(fileListing.Current);
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                        }
                                        else
                                        {
                                            filepath = "";
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                        }
                                        WriteMemoryPointedString(operandStart, filepath);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x1:  // FSYS_GNF ptr
                                        if (fileListing.MoveNext())
                                        {
                                            filepath = Path.GetFileName(fileListing.Current);
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                        }
                                        else
                                        {
                                            filepath = "";
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                        }
                                        WriteMemoryRegisterPointedString(operandStart, filepath, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x2:  // FSYS_GND adr
                                        if (directoryListing.MoveNext())
                                        {
                                            filepath = Path.GetFileName(directoryListing.Current);
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                        }
                                        else
                                        {
                                            filepath = "";
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                        }
                                        WriteMemoryPointedString(operandStart, filepath);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // FSYS_GND ptr
                                        if (directoryListing.MoveNext())
                                        {
                                            filepath = Path.GetFileName(directoryListing.Current);
                                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                                        }
                                        else
                                        {
                                            filepath = "";
                                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                                        }
                                        WriteMemoryRegisterPointedString(operandStart, filepath, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_ListingGet, opcodeLow));
                                }
                                break;
                            case 0x70:  // Get File Times
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_GCT reg, adr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetCreationTimeUtc(ReadMemoryPointedString(operandStart + 1)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x1:  // FSYS_GCT reg, ptr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetCreationTimeUtc(ReadMemoryRegisterPointedString(operandStart + 1, out byteCount)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x2:  // FSYS_GMT reg, adr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetLastWriteTimeUtc(ReadMemoryPointedString(operandStart + 1)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x3:  // FSYS_GMT reg, ptr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetLastWriteTimeUtc(ReadMemoryRegisterPointedString(operandStart + 1, out byteCount)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x4:  // FSYS_GAT reg, adr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetLastAccessTimeUtc(ReadMemoryPointedString(operandStart + 1)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x5:  // FSYS_GAT reg, ptr
                                        WriteMemoryRegister(operandStart,
                                            (ulong)(long)(File.GetLastAccessTimeUtc(ReadMemoryRegisterPointedString(operandStart + 1, out byteCount)) - DateTime.UnixEpoch).TotalSeconds);
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_TimeGet, opcodeLow));
                                }
                                break;
                            case 0x80:  // Set File Times
                                switch (opcodeLow)
                                {
                                    case 0x0:  // FSYS_SCT adr, reg
                                        File.SetCreationTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x1:  // FSYS_SCT adr, lit
                                        File.SetCreationTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0x2:  // FSYS_SCT ptr, reg
                                        File.SetCreationTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x3:  // FSYS_SCT ptr, lit
                                        File.SetCreationTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x4:  // FSYS_SMT adr, reg
                                        File.SetLastWriteTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x5:  // FSYS_SMT adr, lit
                                        File.SetLastWriteTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0x6:  // FSYS_SMT ptr, reg
                                        File.SetLastWriteTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0x7:  // FSYS_SMT ptr, lit
                                        File.SetLastWriteTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    case 0x8:  // FSYS_SAT adr, reg
                                        File.SetLastAccessTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 9;
                                        break;
                                    case 0x9:  // FSYS_SAT adr, lit
                                        File.SetLastAccessTimeUtc(ReadMemoryPointedString(operandStart),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + 8)));
                                        Registers[(int)Register.rpo] += 16;
                                        break;
                                    case 0xA:  // FSYS_SAT ptr, reg
                                        File.SetLastAccessTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryRegister(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 1;
                                        break;
                                    case 0xB:  // FSYS_SAT ptr, lit
                                        File.SetLastAccessTimeUtc(ReadMemoryRegisterPointedString(operandStart, out byteCount),
                                            DateTime.UnixEpoch.AddSeconds((long)ReadMemoryQWord(operandStart + byteCount)));
                                        Registers[(int)Register.rpo] += byteCount + 8;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_FileSystem_TimeSet, opcodeLow));
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_FileSystem, opcodeHigh));
                        }
                        break;
#endif
#if EXTENSION_SET_TERMINAL
                    case 0x7:  // Terminal Extension Set
                        switch (opcodeHigh)
                        {
                            case 0x00:  // Clear
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_CLS
                                        Console.Clear();
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_Clear, opcodeLow));
                                }
                                break;
                            case 0x10:  // Auto echo
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_AEE
                                        Registers[(int)Register.rsf] |= (ulong)StatusFlags.AutoEcho;
                                        break;
                                    case 0x1:  // TERM_AED
                                        Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.AutoEcho;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_Clear, opcodeLow));
                                }
                                break;
                            case 0x20:  // Set Cursor Position
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_SCY reg
                                        Console.SetCursorPosition(Console.GetCursorPosition().Left, (int)ReadMemoryRegister(operandStart));
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // TERM_SCY lit
                                        Console.SetCursorPosition(Console.GetCursorPosition().Left, (int)ReadMemoryQWord(operandStart));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // TERM_SCY adr
                                        Console.SetCursorPosition(Console.GetCursorPosition().Left, (int)ReadMemoryPointedQWord(operandStart));
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // TERM_SCY ptr
                                        Console.SetCursorPosition(Console.GetCursorPosition().Left, (int)ReadMemoryRegisterPointedNumber(operandStart, out byteCount));
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // TERM_SCX reg
                                        Console.SetCursorPosition((int)ReadMemoryRegister(operandStart), Console.GetCursorPosition().Top);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // TERM_SCX lit
                                        Console.SetCursorPosition((int)ReadMemoryQWord(operandStart), Console.GetCursorPosition().Top);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // TERM_SCX adr
                                        Console.SetCursorPosition((int)ReadMemoryPointedQWord(operandStart), Console.GetCursorPosition().Top);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // TERM_SCX ptr
                                        Console.SetCursorPosition((int)ReadMemoryRegisterPointedNumber(operandStart, out byteCount), Console.GetCursorPosition().Top);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_SetPosition, opcodeLow));
                                }
                                break;
                            case 0x30:  // Get Terminal Info
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_GCY reg
                                        WriteMemoryRegister(operandStart, (ulong)Console.GetCursorPosition().Top);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // TERM_GCX reg
                                        WriteMemoryRegister(operandStart, (ulong)Console.GetCursorPosition().Left);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x2:  // TERM_GSY reg
                                        WriteMemoryRegister(operandStart, (ulong)Console.BufferHeight);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x3:  // TERM_GSX reg
                                        WriteMemoryRegister(operandStart, (ulong)Console.BufferWidth);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_GetPosition, opcodeLow));
                                }
                                break;
                            case 0x40:  // Beep
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_BEP
                                        Console.Beep();
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_Beep, opcodeLow));
                                }
                                break;
                            case 0x50:  // Set Colour
                                switch (opcodeLow)
                                {
                                    case 0x0:  // TERM_SFC reg
                                        Console.ForegroundColor = (ConsoleColor)ReadMemoryRegister(operandStart);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x1:  // TERM_SFC lit
                                        Console.ForegroundColor = (ConsoleColor)ReadMemoryQWord(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x2:  // TERM_SFC adr
                                        Console.ForegroundColor = (ConsoleColor)ReadMemoryPointedQWord(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x3:  // TERM_SFC ptr
                                        Console.ForegroundColor = (ConsoleColor)ReadMemoryRegisterPointedNumber(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x4:  // TERM_SBC reg
                                        Console.BackgroundColor = (ConsoleColor)ReadMemoryRegister(operandStart);
                                        Registers[(int)Register.rpo]++;
                                        break;
                                    case 0x5:  // TERM_SBC lit
                                        Console.BackgroundColor = (ConsoleColor)ReadMemoryQWord(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x6:  // TERM_SBC adr
                                        Console.BackgroundColor = (ConsoleColor)ReadMemoryPointedQWord(operandStart);
                                        Registers[(int)Register.rpo] += 8;
                                        break;
                                    case 0x7:  // TERM_SBC ptr
                                        Console.BackgroundColor = (ConsoleColor)ReadMemoryRegisterPointedNumber(operandStart, out byteCount);
                                        Registers[(int)Register.rpo] += byteCount;
                                        break;
                                    case 0x8:  // TERM_RSC
                                        Console.ResetColor();
                                        break;
                                    default:
                                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Low_Terminal_Colour, opcodeLow));
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_High_Terminal, opcodeHigh));
                        }
                        break;
#endif
                            default:
                        throw new InvalidOpcodeException(string.Format(Strings_Processor.Error_Opcode_Extension_Set, extensionSet));
                }
#if EXTENSION_SET_HEAP_ALLOCATE
                ulong newSO = Registers[(int)Register.rso];
                if (MapStack && newSO != oldSO)
                {
                    // Update the mapped memory occupied by the stack, and throw an error if the stack has collided with allocated memory
                    if (newSO > (ulong)Memory.LongLength)
                    {
                        throw new StackSizeException(Strings_Processor.Error_Stack_Out_Of_Range);
                    }
                    _mappedMemoryRanges[^1] = new Range((long)newSO, Memory.LongLength);
                    if (_mappedMemoryRanges.Count >= 2 && _mappedMemoryRanges[^2].Overlaps(_mappedMemoryRanges[^1]))
                    {
                        throw new StackSizeException(Strings_Processor.Error_Stack_Collide);
                    }
                }
#endif
            } while (runUntilHalt && !halt);
            return halt;
        }

#if EXTENSION_SET_EXTERNAL_ASM
        private static Assembly? ExtLoadContext_Resolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
        {
            try
            {
                if (File.Exists(Path.GetFullPath(assemblyName.Name + ".dll")))
                {
                    return loadContext.LoadFromAssemblyPath(Path.GetFullPath(assemblyName.Name + ".dll"));
                }
                return Assembly.Load(assemblyName);
            }
            catch
            {
                return null;
            }
        }
#endif

#if EXTENSION_SET_HEAP_ALLOCATE
        /// <summary>
        /// Allocate a block of memory with a given size into the processor memory map.
        /// </summary>
        /// <returns>The address of the first byte in the allocated block, or <see cref="ulong.MaxValue"/> if the allocation failed.</returns>
        /// <exception cref="InvalidOperationException">Thrown if called before a program is loaded into this processor</exception>
        public ulong AllocateMemory(ulong size)
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException(Strings_Processor.Error_No_Program);
            }
            if (size is 0 or > long.MaxValue)
            {
                return ulong.MaxValue;
            }
            for (int i = 0; i < _mappedMemoryRanges.Count - 1; i++)
            {
                long lastIndex = _mappedMemoryRanges[i].End;
                Range testRange = new(lastIndex, lastIndex + (long)size);
                if (!testRange.Overlaps(_mappedMemoryRanges[i + 1]))
                {
                    _mappedMemoryRanges.Insert(i + 1, testRange);
                    return (ulong)testRange.Start;
                }
            }
            return ulong.MaxValue;
        }

        /// <summary>
        /// Re-allocate the block of memory starting at the given address with a newly given size in the processor memory map.
        /// </summary>
        /// <returns>
        /// The address of the first byte in the re-allocated block (it may be unchanged),
        /// or <see cref="ulong.MaxValue"/> if the re-allocation failed because there is not enough free contiguous memory,
        /// or <see cref="ulong.MaxValue"/> - 1 if the re-allocation failed because the given address does not correspond to the start of an allocated memory block.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if called before a program is loaded into this processor</exception>
        public ulong ReallocateMemory(ulong address, ulong size)
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException(Strings_Processor.Error_No_Program);
            }
            if (size is 0 or > long.MaxValue)
            {
                return ulong.MaxValue;
            }
            // Find referenced block
            int blockIndex = -1;
            Range thisRange = new(0, 0);
            for (int i = 0; i < _mappedMemoryRanges.Count - 1; i++)
            {
                Range range = _mappedMemoryRanges[i];
                if (range.Start == (long)address)
                {
                    blockIndex = i;
                    thisRange = range;
                    break;
                }
            }
            if (blockIndex == -1 || thisRange.Start == 0)
            {
                // Allocated memory block doesn't exist
                return ulong.MaxValue - 1;
            }
            // If after resizing the block it still fits in its current position, there's no need to copy data
            Range resizedRange = new(thisRange.Start, thisRange.Start + (long)size);
            if (!resizedRange.Overlaps(_mappedMemoryRanges[blockIndex + 1]))
            {
                _mappedMemoryRanges[blockIndex] = resizedRange;
                return (ulong)thisRange.Start;
            }

            // Hold temporary copy of contained data while the block is being re-allocated
            byte[] bytesCopy = new byte[thisRange.Length];
            Array.Copy(Memory, thisRange.Start, bytesCopy, 0, bytesCopy.LongLength);

            _mappedMemoryRanges.RemoveAt(blockIndex);
            ulong newAddress = AllocateMemory(size);
            if (newAddress == ulong.MaxValue)
            {
                // Not enough free contiguous memory to re-allocate, keep the state of mapped memory before method was called.
                _mappedMemoryRanges.Insert(blockIndex, thisRange);
                return ulong.MaxValue;
            }
            // Copy data to new location
            bytesCopy.CopyTo(Memory, (long)newAddress);
            return newAddress;
        }

        /// <summary>
        /// Remove a block of memory with starting at the given address from the processor memory map.
        /// </summary>
        /// <returns>Whether or not the free operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if called before a program is loaded into this processor</exception>
        public bool FreeMemory(ulong address)
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException(Strings_Processor.Error_No_Program);
            }
            for (int i = 1; i < _mappedMemoryRanges.Count - 1; i++)
            {
                if (_mappedMemoryRanges[i].Start == (long)address)
                {
                    _mappedMemoryRanges.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned, integer) from the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadMemoryWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(Memory)[(int)offset..]);
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadMemoryDWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(Memory)[(int)offset..]);
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryQWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(Memory)[(int)offset..]);
        }

        /// <summary>
        /// Read the stored register type at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Register ReadMemoryRegisterType(ulong offset)
        {
            return (Register)Memory[offset];
        }

        /// <summary>
        /// Read the value of the stored register type at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryRegister(ulong offset)
        {
            return Registers[(int)ReadMemoryRegisterType(offset)];
        }

        /// <summary>
        /// Calculate the memory address of the stored pointer at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryPointer(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return GetPointerAddress(new Pointer(new ReadOnlySpan<byte>(Memory)[(int)offset..], out byteCount));
#else
            byteCount = 1;
            return Registers[(int)ReadMemoryRegisterType(offset)];
#endif
        }

        /// <summary>
        /// Read a byte from the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadMemoryRegisterPointedByte(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return Memory[ReadMemoryPointer(offset, out byteCount)];
#else
            byteCount = 1;
            return Memory[ReadMemoryRegister(offset)];
#endif
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadMemoryRegisterPointedWord(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return ReadMemoryWord(ReadMemoryPointer(offset, out byteCount));
#else
            byteCount = 1;
            return ReadMemoryWord(ReadMemoryRegister(offset));
#endif
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadMemoryRegisterPointedDWord(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return ReadMemoryDWord(ReadMemoryPointer(offset, out byteCount));
#else
            byteCount = 1;
            return ReadMemoryDWord(ReadMemoryRegister(offset));
#endif
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryRegisterPointedQWord(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return ReadMemoryQWord(ReadMemoryPointer(offset, out byteCount));
#else
            byteCount = 1;
            return ReadMemoryQWord(ReadMemoryRegister(offset));
#endif
        }

        /// <summary>
        /// Read a number from the memory address stored in a pointer stored at the given memory offset, using the read size stored in the pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryRegisterPointedNumber(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            Pointer ptr = new(new ReadOnlySpan<byte>(Memory)[(int)offset..], out byteCount);
            ulong address = GetPointerAddress(ptr);
            return ptr.ReadSize switch
            {
                PointerReadSize.QuadWord => ReadMemoryQWord(address),
                PointerReadSize.DoubleWord => ReadMemoryDWord(address),
                PointerReadSize.Word => ReadMemoryWord(address),
                PointerReadSize.Byte => Memory[address],
                _ => 0
            };
#else
            byteCount = 1;
            return ReadMemoryQWord(ReadMemoryRegister(offset));
#endif
        }

        /// <summary>
        /// Read a byte from the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadMemoryPointedByte(ulong offset)
        {
            return Memory[ReadMemoryQWord(offset)];
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadMemoryPointedWord(ulong offset)
        {
            return ReadMemoryWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadMemoryPointedDWord(ulong offset)
        {
            return ReadMemoryDWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadMemoryPointedQWord(ulong offset)
        {
            return ReadMemoryQWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Read a null-terminated UTF-8 encoded string from the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadMemoryString(ulong offset)
        {
            ulong length;
            for (length = 0; Memory[offset + length] != 0x0; length++) { }
            return Encoding.UTF8.GetString(Memory, (int)offset, (int)length);
        }

        /// <summary>
        /// Read a null-terminated UTF-8 encoded string from the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadMemoryRegisterPointedString(ulong offset, out ulong byteCount)
        {
#if DISPLACEMENT
            return ReadMemoryString(ReadMemoryPointer(offset, out byteCount));
#else
            byteCount = 1;
            return ReadMemoryString(ReadMemoryRegister(offset));
#endif
        }

        /// <summary>
        /// Read a null-terminated UTF-8 encoded string from the memory address stored at the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadMemoryPointedString(ulong offset)
        {
            return ReadMemoryString(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryWord(ulong offset, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryDWord(ulong offset, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryQWord(ulong offset, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Modify the value of the stored register type at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegister(ulong offset, ulong value)
        {
            Register registerType = ReadMemoryRegisterType(offset);
            if (registerType == Register.rpo)
            {
                throw new ReadOnlyRegisterException(string.Format(Strings_Processor.Error_Read_Only_Register, registerType));
            }
            Registers[(int)registerType] = value;
        }

        /// <summary>
        /// Write a byte to the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegisterPointedByte(ulong offset, byte value, out ulong byteCount)
        {
#if DISPLACEMENT
            Memory[ReadMemoryPointer(offset, out byteCount)] = value;
#else
            byteCount = 1;
            Memory[ReadMemoryRegister(offset)] = value;
#endif
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegisterPointedWord(ulong offset, ushort value, out ulong byteCount)
        {
#if DISPLACEMENT
            WriteMemoryWord(ReadMemoryPointer(offset, out byteCount), value);
#else
            byteCount = 1;
            WriteMemoryWord(ReadMemoryRegister(offset), value);
#endif
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegisterPointedDWord(ulong offset, uint value, out ulong byteCount)
        {
#if DISPLACEMENT
            WriteMemoryDWord(ReadMemoryPointer(offset, out byteCount), value);
#else
            byteCount = 1;
            WriteMemoryDWord(ReadMemoryRegister(offset), value);
#endif
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegisterPointedQWord(ulong offset, ulong value, out ulong byteCount)
        {
#if DISPLACEMENT
            WriteMemoryQWord(ReadMemoryPointer(offset, out byteCount), value);
#else
            byteCount = 1;
            WriteMemoryQWord(ReadMemoryRegister(offset), value);
#endif
        }

        /// <summary>
        /// Write a byte to the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryPointedByte(ulong offset, byte value)
        {
            Memory[ReadMemoryQWord(offset)] = value;
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryPointedWord(ulong offset, ushort value)
        {
            WriteMemoryWord(ReadMemoryQWord(offset), value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryPointedDWord(ulong offset, uint value)
        {
            WriteMemoryDWord(ReadMemoryQWord(offset), value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryPointedQWord(ulong offset, ulong value)
        {
            WriteMemoryQWord(ReadMemoryQWord(offset), value);
        }

        /// <summary>
        /// Write a null-terminated UTF-8 encoded string to the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryString(ulong offset, string value)
        {
            Encoding.UTF8.GetBytes(value, 0, value.Length, Memory, (int)offset);
            // Write null terminator
            Memory[offset + (ulong)value.Length] = 0x00;
        }

        /// <summary>
        /// Write a null-terminated UTF-8 encoded string to the memory address stored in a pointer stored at the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryRegisterPointedString(ulong offset, string value, out ulong byteCount)
        {
#if DISPLACEMENT
            WriteMemoryString(ReadMemoryPointer(offset, out byteCount), value);
#else
            byteCount = 1;
            WriteMemoryString(ReadMemoryRegister(offset), value);
#endif
        }

        /// <summary>
        /// Write a null-terminated UTF-8 encoded string to the memory address stored at the given memory offset.
        /// </summary>
        /// <remarks>A null terminator is a single 0x00 byte.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryPointedString(ulong offset, string value)
        {
            WriteMemoryString(ReadMemoryQWord(offset), value);
        }

#if DISPLACEMENT
        /// <summary>
        /// Calculate the address of a pointer given the current state of this processor's registers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetPointerAddress(Pointer ptr)
        {
            switch (ptr.Mode)
            {
                case DisplacementMode.NoDisplacement:
                    return Registers[(int)ptr.PointerRegister];
                case DisplacementMode.Constant:
                    return Registers[(int)ptr.PointerRegister] + (ulong)ptr.DisplacementConstant;
                case DisplacementMode.Register:
                    ulong otherRegisterValue = Registers[(int)ptr.OtherRegister];
                    if (ptr.SubtractOtherRegister)
                    {
                        otherRegisterValue = (ulong)-(long)otherRegisterValue;
                    }
                    return Registers[(int)ptr.PointerRegister] + (otherRegisterValue << (int)ptr.OtherRegisterMultiplier);
                case DisplacementMode.ConstantAndRegister:
                    otherRegisterValue = Registers[(int)ptr.OtherRegister];
                    if (ptr.SubtractOtherRegister)
                    {
                        otherRegisterValue = (ulong)-(long)otherRegisterValue;
                    }
                    return Registers[(int)ptr.PointerRegister] + (otherRegisterValue << (int)ptr.OtherRegisterMultiplier) + (ulong)ptr.DisplacementConstant;
                default:
                    return 0;
            }
        }
#endif
    }
}
