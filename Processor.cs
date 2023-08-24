using System.Buffers.Binary;
using System.Text;

namespace AssEmbly
{
    /// <summary>
    /// Executes compiled AssEmbly programs.
    /// </summary>
    public class Processor
    {
        public readonly byte[] Memory;
        public readonly ulong[] Registers;
        public readonly bool UseV1CallStack;

        private readonly ulong stackCallSize;

        public bool ProgramLoaded { get; private set; }

        private FileStream? openFile;
        private BinaryReader? fileRead;
        private BinaryWriter? fileWrite;
        private long openFileSize = 0;

        private readonly Random rng = new();

        // Because C#'s console methods work with potentially multi-byte characters at a time,
        // but AssEmbly works with single bytes, we need a queue to store bytes that have been read
        // from stdin but are yet to be processed by an AssEmbly read instruction.
        private readonly Queue<byte> stdinByteQueue = new();

        public Processor(ulong memorySize, ulong entryPoint = 0, bool useV1CallStack = false)
        {
            Memory = new byte[memorySize];
            Registers = new ulong[Enum.GetNames(typeof(Register)).Length];
            Registers[(int)Register.rpo] = entryPoint;
            Registers[(int)Register.rso] = memorySize;
            Registers[(int)Register.rsb] = memorySize;
            ProgramLoaded = false;
            // AssEmbly stores strings as UTF-8, so console must be set to UTF-8 to render bytes correctly
            Console.OutputEncoding = Encoding.UTF8;
            UseV1CallStack = useV1CallStack;
            stackCallSize = useV1CallStack ? 24UL : 16UL;
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
                throw new InvalidOperationException("A program is already loaded in this processor.");
            }
            if (programData.LongLength > Memory.LongLength)
            {
                throw new InvalidOperationException($"Program too large to fit in allocated memory. {Memory.LongLength} bytes available, {programData.LongLength} given.");
            }
            Array.Copy(programData, Memory, programData.LongLength);
            ProgramLoaded = true;
        }

        /// <summary>
        /// Execute either a single instruction or execute until a halt instruction is reached,
        /// depending on the value of <paramref name="runUntilHalt"/>.
        /// </summary>
        /// <returns><see langword="true"/> if execution should stop (HLT reached) - otherwise <see langword="false"/></returns>
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
        public bool Execute(bool runUntilHalt)
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException("A program has not been loaded in this processor.");
            }
            if (Registers[(int)Register.rpo] >= (ulong)Memory.LongLength)
            {
                throw new InvalidOperationException("The processor has reached the end of accessible memory.");
            }
            bool halt = false;
            do
            {
                byte opcode = Memory[Registers[(int)Register.rpo]];
                // Upper 4-bytes (general category of instruction)
                byte opcodeHigh = (byte)((0xF0 & opcode) >> 4);
                // Lower 4-bytes (specific operation and operand types)
                byte opcodeLow = (byte)(0x0F & opcode);
                Registers[(int)Register.rpo]++;
                switch (opcodeHigh)
                {
                    case 0x0:  // Control / Jump
                        switch (opcodeLow)
                        {
                            case 0x0:  // HLT (Halt)
                                halt = true;
                                break;
                            case 0x1:  // NOP
                                break;
                            case 0x2:  // JMP adr (Unconditional Jump)
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x3:  // JMP ptr (Unconditional Jump)
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0x4:  // JEQ adr (Jump If Equal To - Zero Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0x5:  // JEQ ptr (Jump If Equal To - Zero Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            case 0x6:  // JNE adr (Jump If Not Equal To - Zero Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0x7:  // JNE ptr (Jump If Not Equal To - Zero Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Zero) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            case 0x8:  // JLT adr (Jump If Less Than - Carry Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0x9:  // JLT ptr (Jump If Less Than - Carry Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            case 0xA:  // JLE adr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0xB:  // JLE ptr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) != 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            case 0xC:  // JGT adr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0xD:  // JGT ptr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.ZeroAndCarry) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            case 0xE:  // JGE adr (Jump If Greater Than or Equal To - Carry Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 8;
                                }
                                break;
                            case 0xF:  // JGE ptr (Jump If Greater Than or Equal To - Carry Flag Unset)
                                if ((Registers[(int)Register.rsf] & (ulong)StatusFlags.Carry) == 0)
                                {
                                    Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                }
                                else
                                {
                                    Registers[(int)Register.rpo] += 1;
                                }
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised control low opcode");
                        }
                        break;
                    case 0x1:  // Addition
                        Register targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        ulong initial = Registers[(int)targetRegister];
                        switch (opcodeLow)
                        {
                            case 0x0:  // ADD reg, reg
                                Registers[(int)targetRegister] += ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // ADD reg, lit
                                Registers[(int)targetRegister] += ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // ADD reg, adr
                                Registers[(int)targetRegister] += ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // ADD reg, ptr
                                Registers[(int)targetRegister] += ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // ICR reg
                                Registers[(int)targetRegister]++;
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised addition low opcode");
                        }
                        if (Registers[(int)targetRegister] < initial)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        }
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x2:  // Subtraction
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        initial = Registers[(int)targetRegister];
                        switch (opcodeLow)
                        {
                            case 0x0:  // SUB reg, reg
                                Registers[(int)targetRegister] -= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // SUB reg, lit
                                Registers[(int)targetRegister] -= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // SUB reg, adr
                                Registers[(int)targetRegister] -= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // SUB reg, ptr
                                Registers[(int)targetRegister] -= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // DCR reg
                                Registers[(int)targetRegister]--;
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised subtraction low opcode");
                        }
                        if (Registers[(int)targetRegister] > initial)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        }
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x3:  // Multiplication
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        initial = Registers[(int)targetRegister];
                        switch (opcodeLow)
                        {
                            case 0x0:  // MUL reg, reg
                                Registers[(int)targetRegister] *= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // MUL reg, lit
                                Registers[(int)targetRegister] *= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // MUL reg, adr
                                Registers[(int)targetRegister] *= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // MUL reg, ptr
                                Registers[(int)targetRegister] *= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised multiplication low opcode");
                        }
                        if (Registers[(int)targetRegister] < initial)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        }
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x4:  // Division
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        // Only used to store remainder in DVR, set to an unused default otherwise
                        Register secondTarget = Register.rpo;
                        if (opcodeLow is >= 0x4 and <= 0x7 &&  // DVR
                            (secondTarget = ReadMemoryRegisterType(Registers[(int)Register.rpo] + 1)) == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {secondTarget}");
                        }
                        switch (opcodeLow)
                        {
                            case 0x0:  // DIV reg, reg
                                Registers[(int)targetRegister] /= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // DIV reg, lit
                                Registers[(int)targetRegister] /= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // DIV reg, adr
                                Registers[(int)targetRegister] /= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // DIV reg, ptr
                                Registers[(int)targetRegister] /= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // DVR reg, reg, reg
                                ulong dividend = Registers[(int)targetRegister];
                                ulong divisor = ReadMemoryRegister(Registers[(int)Register.rpo] + 2);
                                ulong div = dividend / divisor;
                                ulong rem = dividend % divisor;
                                Registers[(int)targetRegister] = div;
                                Registers[(int)secondTarget] = rem;
                                Registers[(int)Register.rpo] += 3;
                                break;
                            case 0x5:  // DVR reg, reg, lit
                                dividend = Registers[(int)targetRegister];
                                divisor = ReadMemoryQWord(Registers[(int)Register.rpo] + 2);
                                div = dividend / divisor;
                                rem = dividend % divisor;
                                Registers[(int)targetRegister] = div;
                                Registers[(int)secondTarget] = rem;
                                Registers[(int)Register.rpo] += 10;
                                break;
                            case 0x6:  // DVR reg, reg, adr
                                dividend = Registers[(int)targetRegister];
                                divisor = ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 2);
                                div = dividend / divisor;
                                rem = dividend % divisor;
                                Registers[(int)targetRegister] = div;
                                Registers[(int)secondTarget] = rem;
                                Registers[(int)Register.rpo] += 10;
                                break;
                            case 0x7:  // DVR reg, reg, ptr
                                dividend = Registers[(int)targetRegister];
                                divisor = ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 2);
                                div = dividend / divisor;
                                rem = dividend % divisor;
                                Registers[(int)targetRegister] = div;
                                Registers[(int)secondTarget] = rem;
                                Registers[(int)Register.rpo] += 3;
                                break;
                            case 0x8:  // REM reg, reg
                                Registers[(int)targetRegister] %= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x9:  // REM reg, lit
                                Registers[(int)targetRegister] %= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xA:  // REM reg, adr
                                Registers[(int)targetRegister] %= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xB:  // REM reg, ptr
                                Registers[(int)targetRegister] %= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised division low opcode");
                        }
                        Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x5:  // Shifting
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        initial = Registers[(int)targetRegister];
                        int amount;
                        switch (opcodeLow)
                        {
                            case 0x0:  // SHL reg, reg
                                amount = (int)ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] <<= amount;
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // SHL reg, lit
                                amount = (int)ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] <<= amount;
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // SHL reg, adr
                                amount = (int)ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] <<= amount;
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // SHL reg, ptr
                                amount = (int)ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] <<= amount;
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // SHR reg, reg
                                amount = (int)ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] >>= amount;
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x5:  // SHR reg, lit
                                amount = (int)ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] >>= amount;
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x6:  // SHR reg, adr
                                amount = (int)ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] >>= amount;
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x7:  // SHR reg, ptr
                                amount = (int)ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)targetRegister] >>= amount;
                                Registers[(int)Register.rpo] += 2;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised shifting low opcode");
                        }
                        // As registers are only 64 bits, shifting by 64 bits or more will always result in 0
                        if (amount >= 64)
                        {
                            Registers[(int)targetRegister] = 0;
                        }
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
                        if (amount != 0 && initial != 0 && (amount >= 64 || opcodeLow <= 0x3
                            ? (initial >> (64 - amount)) != 0
                            : (initial << (64 - amount)) != 0))
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        }
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x6:  // Bitwise
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        switch (opcodeLow)
                        {
                            case 0x0:  // AND reg, reg
                                Registers[(int)targetRegister] &= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // AND reg, lit
                                Registers[(int)targetRegister] &= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // AND reg, adr
                                Registers[(int)targetRegister] &= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // AND reg, ptr
                                Registers[(int)targetRegister] &= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // ORR reg, reg
                                Registers[(int)targetRegister] |= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x5:  // ORR reg, lit
                                Registers[(int)targetRegister] |= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x6:  // ORR reg, adr
                                Registers[(int)targetRegister] |= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x7:  // ORR reg, ptr
                                Registers[(int)targetRegister] |= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x8:  // XOR reg, reg
                                Registers[(int)targetRegister] ^= ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x9:  // XOR reg, lit
                                Registers[(int)targetRegister] ^= ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xA:  // XOR reg, adr
                                Registers[(int)targetRegister] ^= ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xB:  // XOR reg, ptr
                                Registers[(int)targetRegister] ^= ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0xC:  // NOT reg
                                Registers[(int)targetRegister] = ~Registers[(int)targetRegister];
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0xD:  // RNG reg
                                byte[] randomBuffer = new byte[8];
                                rng.NextBytes(randomBuffer);
                                Registers[(int)targetRegister] = BinaryPrimitives.ReadUInt64LittleEndian(randomBuffer);
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised bitwise low opcode");
                        }
                        Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        if (Registers[(int)targetRegister] == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x7:  // Test
                        targetRegister = ReadMemoryRegisterType(Registers[(int)Register.rpo]);
                        if (targetRegister == Register.rpo)
                        {
                            throw new ReadOnlyRegisterException($"Cannot write to read-only register {targetRegister}");
                        }
                        ulong newValue;
                        switch (opcodeLow)
                        {
                            case 0x0:  // TST reg, reg
                                newValue = Registers[(int)targetRegister] & ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // TST reg, lit
                                newValue = Registers[(int)targetRegister] & ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // TST reg, adr
                                newValue = Registers[(int)targetRegister] & ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // TST reg, ptr
                                newValue = Registers[(int)targetRegister] & ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // CMP reg, reg
                                newValue = Registers[(int)targetRegister] - ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x5:  // CMP reg, lit
                                newValue = Registers[(int)targetRegister] - ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x6:  // CMP reg, adr
                                newValue = Registers[(int)targetRegister] - ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x7:  // CMP reg, ptr
                                newValue = Registers[(int)targetRegister] - ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised comparison low opcode");
                        }
                        if (opcodeLow >= 0x4 && newValue > Registers[(int)targetRegister])
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Carry;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Carry;
                        }
                        if (newValue == 0)
                        {
                            Registers[(int)Register.rsf] |= (ulong)StatusFlags.Zero;
                        }
                        else
                        {
                            Registers[(int)Register.rsf] &= ~(ulong)StatusFlags.Zero;
                        }
                        break;
                    case 0x8:  // Small Move
                        switch (opcodeLow)
                        {
                            case 0x0:  // MVB reg, reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // MVB reg, lit
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // MVB reg, adr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryPointedByte(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // MVB reg, ptr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // MVB adr, reg
                                WriteMemoryPointedByte(Registers[(int)Register.rpo], (byte)(0xFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x5:  // MVB adr, lit
                                WriteMemoryPointedByte(Registers[(int)Register.rpo], (byte)(0xFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 16;
                                break;
                            case 0x6:  // MVB ptr, reg
                                WriteMemoryRegisterPointedByte(Registers[(int)Register.rpo], (byte)(0xFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x7:  // MVB ptr, lit
                                WriteMemoryRegisterPointedByte(Registers[(int)Register.rpo], (byte)(0xFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x8:  // MVW reg, reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x9:  // MVW reg, lit
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xA:  // MVW reg, adr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryPointedWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xB:  // MVW reg, ptr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryRegisterPointedWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0xC:  // MVW adr, reg
                                WriteMemoryPointedWord(Registers[(int)Register.rpo], (ushort)(0xFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xD:  // MVW adr, lit
                                WriteMemoryPointedWord(Registers[(int)Register.rpo], (ushort)(0xFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 16;
                                break;
                            case 0xE:  // MVW ptr, reg
                                WriteMemoryRegisterPointedWord(Registers[(int)Register.rpo], (ushort)(0xFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0xF:  // MVW ptr, lit
                                WriteMemoryRegisterPointedWord(Registers[(int)Register.rpo], (ushort)(0xFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised small move low opcode");
                        }
                        break;
                    case 0x9:  // Large Move
                        switch (opcodeLow)
                        {
                            case 0x0:  // MVD reg, reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFFFFFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x1:  // MVD reg, lit
                                WriteMemoryRegister(Registers[(int)Register.rpo], 0xFFFFFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x2:  // MVD reg, adr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryPointedDWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x3:  // MVD reg, ptr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryRegisterPointedDWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x4:  // MVD adr, reg
                                WriteMemoryPointedDWord(Registers[(int)Register.rpo], (uint)(0xFFFFFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x5:  // MVD adr, lit
                                WriteMemoryPointedDWord(Registers[(int)Register.rpo], (uint)(0xFFFFFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 8)));
                                Registers[(int)Register.rpo] += 16;
                                break;
                            case 0x6:  // MVD ptr, reg
                                WriteMemoryRegisterPointedDWord(Registers[(int)Register.rpo], (uint)(0xFFFFFFFF & ReadMemoryRegister(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x7:  // MVD ptr, lit
                                WriteMemoryRegisterPointedDWord(Registers[(int)Register.rpo], (uint)(0xFFFFFFFF & ReadMemoryQWord(Registers[(int)Register.rpo] + 1)));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x8:  // MVQ reg, reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryRegister(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x9:  // MVQ reg, lit
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xA:  // MVQ reg, adr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xB:  // MVQ reg, ptr
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0xC:  // MVQ adr, reg
                                WriteMemoryPointedQWord(Registers[(int)Register.rpo], ReadMemoryRegister(Registers[(int)Register.rpo] + 8));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0xD:  // MVQ adr, lit
                                WriteMemoryPointedQWord(Registers[(int)Register.rpo], ReadMemoryQWord(Registers[(int)Register.rpo] + 8));
                                Registers[(int)Register.rpo] += 16;
                                break;
                            case 0xE:  // MVQ ptr, reg
                                WriteMemoryRegisterPointedQWord(Registers[(int)Register.rpo], ReadMemoryRegister(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0xF:  // MVQ ptr, lit
                                WriteMemoryRegisterPointedQWord(Registers[(int)Register.rpo], ReadMemoryQWord(Registers[(int)Register.rpo] + 1));
                                Registers[(int)Register.rpo] += 9;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised large move low opcode");
                        }
                        break;
                    case 0xA:  // Stack
                        switch (opcodeLow)
                        {
                            case 0x0:  // PSH reg
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryRegister(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rso] -= 8;
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x1:  // PSH lit
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rso] -= 8;
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x2:  // PSH adr
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryPointedQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rso] -= 8;
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x3:  // PSH ptr
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rso] -= 8;
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x4:  // POP reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], ReadMemoryQWord(Registers[(int)Register.rso]));
                                Registers[(int)Register.rso] += 8;
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised stack low opcode");
                        }
                        break;
                    case 0xB:  // Subroutines
                        switch (opcodeLow)
                        {
                            case 0x0:  // CAL adr
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 8);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x1:  // CAL ptr
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 1);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0x2:  // CAL adr, reg
                                Registers[(int)Register.rfp] = ReadMemoryRegister(Registers[(int)Register.rpo] + 8);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 9);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x3:  // CAL adr, lit
                                Registers[(int)Register.rfp] = ReadMemoryQWord(Registers[(int)Register.rpo] + 8);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 16);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x4:  // CAL adr, adr
                                Registers[(int)Register.rfp] = ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 8);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 16);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x5:  // CAL adr, ptr
                                Registers[(int)Register.rfp] = ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 8);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 9);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                break;
                            case 0x6:  // CAL ptr, reg
                                Registers[(int)Register.rfp] = ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 2);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0x7:  // CAL ptr, lit
                                Registers[(int)Register.rfp] = ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 9);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0x8:  // CAL ptr, adr
                                Registers[(int)Register.rfp] = ReadMemoryPointedQWord(Registers[(int)Register.rpo] + 1);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 9);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0x9:  // CAL ptr, ptr
                                Registers[(int)Register.rfp] = ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo] + 1);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 8, Registers[(int)Register.rpo] + 2);
                                WriteMemoryQWord(Registers[(int)Register.rso] - 16, Registers[(int)Register.rsb]);
                                if (UseV1CallStack)
                                {
                                    WriteMemoryQWord(Registers[(int)Register.rso] - 24, Registers[(int)Register.rso]);
                                }
                                Registers[(int)Register.rso] -= stackCallSize;
                                Registers[(int)Register.rsb] = Registers[(int)Register.rso];
                                Registers[(int)Register.rpo] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                break;
                            case 0xA:  // RET
                                Registers[(int)Register.rso] += stackCallSize;
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                break;
                            case 0xB:  // RET reg
                                Registers[(int)Register.rrv] = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                Registers[(int)Register.rso] += stackCallSize;
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                break;
                            case 0xC:  // RET lit
                                Registers[(int)Register.rrv] = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                Registers[(int)Register.rso] += stackCallSize;
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                break;
                            case 0xD:  // RET adr
                                Registers[(int)Register.rrv] = ReadMemoryPointedQWord(Registers[(int)Register.rpo]);
                                Registers[(int)Register.rso] += stackCallSize;
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                break;
                            case 0xE:  // RET ptr
                                Registers[(int)Register.rrv] = ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo]);
                                Registers[(int)Register.rso] += stackCallSize;
                                Registers[(int)Register.rpo] = ReadMemoryQWord(Registers[(int)Register.rso] - 8);
                                Registers[(int)Register.rsb] = ReadMemoryQWord(Registers[(int)Register.rso] - 16);
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised subroutine low opcode");
                        }
                        break;
                    case 0xC:  // Console Write
                        switch (opcodeLow)
                        {
                            case 0x0:  // WCN reg
                                Console.Write(ReadMemoryRegister(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x1:  // WCN lit
                                Console.Write(ReadMemoryQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x2:  // WCN adr
                                Console.Write(ReadMemoryPointedQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x3:  // WCN ptr
                                Console.Write(ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x4:  // WCB reg
                                Console.Write(0xFF & ReadMemoryRegister(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x5:  // WCB lit
                                Console.Write(Memory[Registers[(int)Register.rpo]]);
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x6:  // WCB adr
                                Console.Write(ReadMemoryPointedByte(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x7:  // WCB ptr
                                Console.Write(ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x8:  // WCX reg
                                Console.Write(string.Format("{0:X}", 0xFF & ReadMemoryRegister(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x9:  // WCX lit
                                Console.Write(string.Format("{0:X}", Memory[Registers[(int)Register.rpo]]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xA:  // WCX adr
                                Console.Write(string.Format("{0:X}", ReadMemoryPointedByte(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xB:  // WCX ptr
                                Console.Write(string.Format("{0:X}", ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo]++;
                                break;
                            // Following instructions write raw bytes to stdout to prevent C# converting our UTF-8 bytes to UTF-16.
                            case 0xC:  // WCC reg
                                {
                                    using Stream stdout = Console.OpenStandardOutput();
                                    stdout.WriteByte((byte)(0xFF & ReadMemoryRegister(Registers[(int)Register.rpo])));
                                    Registers[(int)Register.rpo]++;
                                    break;
                                }
                            case 0xD:  // WCC lit
                                {
                                    using Stream stdout = Console.OpenStandardOutput();
                                    stdout.WriteByte(Memory[Registers[(int)Register.rpo]]);
                                    Registers[(int)Register.rpo] += 8;
                                    break;
                                }
                            case 0xE:  // WCC adr
                                {
                                    using Stream stdout = Console.OpenStandardOutput();
                                    stdout.WriteByte(ReadMemoryPointedByte(Registers[(int)Register.rpo]));
                                    Registers[(int)Register.rpo] += 8;
                                    break;
                                }
                            case 0xF:  // WCC ptr
                                {
                                    using Stream stdout = Console.OpenStandardOutput();
                                    stdout.WriteByte(ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo]));
                                    Registers[(int)Register.rpo]++;
                                    break;
                                }
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised console write low opcode");
                        }
                        break;
                    case 0xD:  // File Write
                        if (openFile is null)
                        {
                            throw new FileOperationException("Cannot perform file operations if no file is open. Run OFL (0xE0) first");
                        }
                        switch (opcodeLow)
                        {
                            case 0x0:  // WFN reg
                                foreach (char digit in ReadMemoryRegister(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x1:  // WFN lit
                                foreach (char digit in ReadMemoryQWord(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x2:  // WFN adr
                                foreach (char digit in ReadMemoryPointedQWord(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x3:  // WFN ptr
                                foreach (char digit in ReadMemoryRegisterPointedQWord(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x4:  // WFB reg
                                foreach (char digit in (0xFF & ReadMemoryRegister(Registers[(int)Register.rpo])).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x5:  // WFB lit
                                foreach (char digit in Memory[Registers[(int)Register.rpo]].ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x6:  // WFB adr
                                foreach (char digit in ReadMemoryPointedByte(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0x7:  // WFB ptr
                                foreach (char digit in ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo]).ToString())
                                {
                                    fileWrite!.Write(digit);
                                }
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x8:  // WFX reg
                                fileWrite!.Write(string.Format("{0:X}", 0xFF & ReadMemoryRegister(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0x9:  // WFX lit
                                fileWrite!.Write(string.Format("{0:X}", Memory[Registers[(int)Register.rpo]]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xA:  // WFX adr
                                fileWrite!.Write(string.Format("{0:X}", ReadMemoryPointedByte(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xB:  // WFX ptr
                                fileWrite!.Write(string.Format("{0:X}", ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0xC:  // WFC reg
                                fileWrite!.Write((byte)(0xFF & ReadMemoryRegister(Registers[(int)Register.rpo])));
                                Registers[(int)Register.rpo]++;
                                break;
                            case 0xD:  // WFC lit
                                fileWrite!.Write(Memory[Registers[(int)Register.rpo]]);
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xE:  // WFC adr
                                fileWrite!.Write(ReadMemoryPointedByte(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo] += 8;
                                break;
                            case 0xF:  // WFC ptr
                                fileWrite!.Write(ReadMemoryRegisterPointedByte(Registers[(int)Register.rpo]));
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised file write low opcode");
                        }
                        break;
                    case 0xE:  // File Operations
                        switch (opcodeLow)
                        {
                            case 0x0:  // OFL adr
                                if (openFile is not null)
                                {
                                    throw new FileOperationException("Cannot execute open file instruction if a file is already open");
                                }
                                ulong startIndex = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                ulong count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                string filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                Registers[(int)Register.rpo] += 8;
                                openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                openFileSize = openFile.Length;
                                fileWrite = new BinaryWriter(openFile);
                                fileRead = new BinaryReader(openFile);
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
                                    throw new FileOperationException("Cannot execute open file instruction if a file is already open");
                                }
                                startIndex = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                Registers[(int)Register.rpo]++;
                                openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                openFileSize = openFile.Length;
                                fileWrite = new BinaryWriter(openFile);
                                fileRead = new BinaryReader(openFile);
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
                                    throw new FileOperationException("Cannot execute close file instruction if a file is not open");
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
                                startIndex = ReadMemoryQWord(Registers[(int)Register.rpo]);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                Registers[(int)Register.rpo] += 8;
                                File.Delete(filepath);
                                break;
                            case 0x4:  // DFL ptr
                                startIndex = ReadMemoryRegister(Registers[(int)Register.rpo]);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                Registers[(int)Register.rpo]++;
                                File.Delete(filepath);
                                break;
                            case 0x5:  // FEX reg, adr
                                startIndex = ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                WriteMemoryRegister(Registers[(int)Register.rpo], File.Exists(filepath) ? 1UL : 0UL);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x6:  // FEX reg, ptr
                                startIndex = ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                WriteMemoryRegister(Registers[(int)Register.rpo], File.Exists(filepath) ? 1UL : 0UL);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            case 0x7:  // FSZ reg, adr
                                startIndex = ReadMemoryQWord(Registers[(int)Register.rpo] + 1);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                WriteMemoryRegister(Registers[(int)Register.rpo], (ulong)new FileInfo(filepath).Length);
                                Registers[(int)Register.rpo] += 9;
                                break;
                            case 0x8:  // FSZ reg, ptr
                                startIndex = ReadMemoryRegister(Registers[(int)Register.rpo] + 1);
                                count = 0;
                                while (Memory[startIndex + count] != 0x0)
                                {
                                    count++;
                                }
                                filepath = Encoding.UTF8.GetString(Memory, (int)startIndex, (int)count);
                                WriteMemoryRegister(Registers[(int)Register.rpo], (ulong)new FileInfo(filepath).Length);
                                Registers[(int)Register.rpo] += 2;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised file operation low opcode");
                        }
                        break;
                    case 0xF:  // Reading
                        switch (opcodeLow)
                        {
                            case 0x0:  // RCC reg
                                {
                                    if (!stdinByteQueue.TryDequeue(out byte currentByte))
                                    {
                                        // There are no remaining queued UTF-8 bytes, so get the next character from the console.
                                        string inputCharacter = "";
                                        ConsoleKeyInfo pressedKey;
                                        // C# gives us a single part of a UTF-16 surrogate pair if something outside the BMP is typed (i.e. an emoji).
                                        // If we detect the start of a surrogate pair, get another character as well to have the full character
                                        // to be converted to UTF-8.
                                        do
                                        {
                                            pressedKey = Console.ReadKey(true);
                                            // By default pressing enter will get \r, we want \n
                                            inputCharacter += pressedKey.Key == ConsoleKey.Enter
                                                ? '\n' : pressedKey.KeyChar;
                                        } while (char.IsHighSurrogate(pressedKey.KeyChar));
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
                                    // Echo byte to console
                                    using Stream stdout = Console.OpenStandardOutput();
                                    stdout.WriteByte(currentByte);

                                    WriteMemoryRegister(Registers[(int)Register.rpo], currentByte);
                                    Registers[(int)Register.rpo]++;
                                    break;
                                }
                            case 0x1:  // RFC reg
                                WriteMemoryRegister(Registers[(int)Register.rpo], fileRead!.ReadByte());
                                if (fileRead.BaseStream.Position >= openFileSize)
                                {
                                    Registers[(int)Register.rsf] |= (ulong)StatusFlags.FileEnd;
                                }
                                Registers[(int)Register.rpo]++;
                                break;
                            default:
                                throw new InvalidOpcodeException($"{opcodeLow:X} is not a recognised reading low opcode");
                        }
                        break;
                    default:
                        throw new InvalidOpcodeException($"{opcodeHigh:X} is not a recognised high opcode");
                }
            } while (runUntilHalt && !halt);
            return halt;
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned, integer) from the given memory offset.
        /// </summary>
        public ushort ReadMemoryWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(Memory.AsSpan()[(int)offset..]);
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the given memory offset.
        /// </summary>
        public uint ReadMemoryDWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(Memory.AsSpan()[(int)offset..]);
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the given memory offset.
        /// </summary>
        public ulong ReadMemoryQWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(Memory.AsSpan()[(int)offset..]);
        }

        /// <summary>
        /// Read the stored register type at the given memory offset.
        /// </summary>
        public Register ReadMemoryRegisterType(ulong offset)
        {
            return (Register)Memory[offset];
        }

        /// <summary>
        /// Read the value of the stored register type at the given memory offset.
        /// </summary>
        public ulong ReadMemoryRegister(ulong offset)
        {
            return Registers[(int)ReadMemoryRegisterType(offset)];
        }

        /// <summary>
        /// Read a byte from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public byte ReadMemoryRegisterPointedByte(ulong offset)
        {
            return Memory[Registers[(int)ReadMemoryRegisterType(offset)]];
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public ushort ReadMemoryRegisterPointedWord(ulong offset)
        {
            return ReadMemoryWord(Registers[(int)ReadMemoryRegisterType(offset)]);
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public uint ReadMemoryRegisterPointedDWord(ulong offset)
        {
            return ReadMemoryDWord(Registers[(int)ReadMemoryRegisterType(offset)]);
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public ulong ReadMemoryRegisterPointedQWord(ulong offset)
        {
            return ReadMemoryQWord(Registers[(int)ReadMemoryRegisterType(offset)]);
        }

        /// <summary>
        /// Read a byte from the memory address stored at the given memory offset.
        /// </summary>
        public byte ReadMemoryPointedByte(ulong offset)
        {
            return Memory[ReadMemoryQWord(offset)];
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public ushort ReadMemoryPointedWord(ulong offset)
        {
            return ReadMemoryWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public uint ReadMemoryPointedDWord(ulong offset)
        {
            return ReadMemoryDWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public ulong ReadMemoryPointedQWord(ulong offset)
        {
            return ReadMemoryQWord(ReadMemoryQWord(offset));
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void WriteMemoryWord(ulong offset, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void WriteMemoryDWord(ulong offset, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void WriteMemoryQWord(ulong offset, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(Memory.AsSpan()[(int)offset..], value);
        }

        /// <summary>
        /// Modify the value of the stored register type at the given memory offset.
        /// </summary>
        public void WriteMemoryRegister(ulong offset, ulong value)
        {
            Register registerType = ReadMemoryRegisterType(offset);
            if (registerType == Register.rpo)
            {
                throw new ReadOnlyRegisterException($"Cannot write to read-only register {registerType}");
            }
            Registers[(int)registerType] = value;
        }

        /// <summary>
        /// Write a byte to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void WriteMemoryRegisterPointedByte(ulong offset, byte value)
        {
            Memory[Registers[(int)ReadMemoryRegisterType(offset)]] = value;
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void WriteMemoryRegisterPointedWord(ulong offset, ushort value)
        {
            WriteMemoryWord(Registers[(int)ReadMemoryRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void WriteMemoryRegisterPointedDWord(ulong offset, uint value)
        {
            WriteMemoryDWord(Registers[(int)ReadMemoryRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void WriteMemoryRegisterPointedQWord(ulong offset, ulong value)
        {
            WriteMemoryQWord(Registers[(int)ReadMemoryRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a byte to the memory address stored at the given memory offset.
        /// </summary>
        public void WriteMemoryPointedByte(ulong offset, byte value)
        {
            Memory[ReadMemoryQWord(offset)] = value;
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void WriteMemoryPointedWord(ulong offset, ushort value)
        {
            WriteMemoryWord(ReadMemoryQWord(offset), value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void WriteMemoryPointedDWord(ulong offset, uint value)
        {
            WriteMemoryDWord(ReadMemoryQWord(offset), value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void WriteMemoryPointedQWord(ulong offset, ulong value)
        {
            WriteMemoryQWord(ReadMemoryQWord(offset), value);
        }
    }
}
