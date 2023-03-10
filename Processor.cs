using System.Buffers.Binary;

namespace AssEmbly
{
    /// <summary>
    /// Executes compiled AssEmbly programs.
    /// </summary>
    public class Processor
    {
        public byte[] Memory { get; private set; }
        public Dictionary<Data.Register, ulong> Registers { get; private set; }
        public bool ProgramLoaded { get; private set; }

        private FileStream? openFile;
        private StreamReader? fileRead;
        private StreamWriter? fileWrite;

        private Random rng = new();

        public Processor(ulong memorySize)
        {
            Memory = new byte[memorySize];
            Registers = new()
            {
                { Data.Register.rpo, 0 },
                { Data.Register.rso, memorySize },
                { Data.Register.rsb, memorySize },
                { Data.Register.rsf, 0 },
                { Data.Register.rrv, 0 },
                { Data.Register.rfp, 0 },
                { Data.Register.rg0, 0 },
                { Data.Register.rg1, 0 },
                { Data.Register.rg2, 0 },
                { Data.Register.rg3, 0 },
                { Data.Register.rg4, 0 },
                { Data.Register.rg5, 0 },
                { Data.Register.rg6, 0 },
                { Data.Register.rg7, 0 },
                { Data.Register.rg8, 0 },
                { Data.Register.rg9, 0 },
            };
            ProgramLoaded = false;
        }

        /// <summary>
        /// Loads a provided compiled program and its data into this processor's memory to be executed.
        /// </summary>
        /// <param name="programData">The entire program, including any data, to load into memory.</param>
        /// <exception cref="InvalidOperationException">Thrown in a program has already been loaded.</exception>
        /// <exception cref="OutOfMemoryException">Thrown if the program is too large to fit into memory.</exception>
        public void LoadProgram(byte[] programData)
        {
            if (ProgramLoaded)
            {
                throw new InvalidOperationException("A program is already loaded in this processor.");
            }
            if (programData.LongLength > Memory.LongLength)
            {
                throw new OutOfMemoryException($"Program too large to fit in allocated memory. {Memory.LongLength} bytes available, {programData.LongLength} given.");
            }
            Array.Copy(programData, Memory, programData.LongLength);
            ProgramLoaded = true;
        }

        /// <summary>
        /// Execute the program until a halt instruction is reached. If a halt instruction has already been reached, continue from after it.
        /// </summary>
        public void Execute()
        {
            while (!Step()) { }
        }

        /// <summary>
        /// Execute a single instruction.
        /// </summary>
        /// <returns><see langword="true"/> if execution should stop (HLT reached) - otherwise <see langword="false"/></returns>
        /// <exception cref="InvalidOperationException">Thrown if the instruction was invalid, or attempted to perform an invalid operation.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if the instruction tried to access an invalid memory address.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the instruction tried to access an invalid memory address.</exception>
        public bool Step()
        {
            if (!ProgramLoaded)
            {
                throw new InvalidOperationException("A program has not been loaded in this processor.");
            }
            if (Registers[Data.Register.rpo] >= (ulong)Memory.LongLength)
            {
                throw new InvalidOperationException("The processor has reached the end of accessible memory.");
            }
            bool halt = false;
            byte opcode = Memory[Registers[Data.Register.rpo]];
            byte opcodeHigh = (byte)((0xF0 & opcode) >> 4);
            byte opcodeLow = (byte)(0x0F & opcode);
            Registers[Data.Register.rpo]++;
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
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x3:  // JMP ptr (Unconditional Jump)
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0x4:  // JEQ adr (Jump If Equal To - Zero Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b1) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0x5:  // JEQ ptr (Jump If Equal To - Zero Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b1) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        case 0x6:  // JNE adr (Jump If Not Equal To - Zero Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b1) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0x7:  // JNE ptr (Jump If Not Equal To - Zero Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b1) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        case 0x8:  // JLT adr (Jump If Less Than - Carry Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b10) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0x9:  // JLT ptr (Jump If Less Than - Carry Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b10) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        case 0xA:  // JLE adr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b11) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0xB:  // JLE ptr (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
                            if ((Registers[Data.Register.rsf] & 0b11) != 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        case 0xC:  // JGT adr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b11) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0xD:  // JGT ptr (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b11) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        case 0xE:  // JGE adr (Jump If Greater Than or Equal To - Carry Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b10) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 8;
                            }
                            break;
                        case 0xF:  // JGE ptr (Jump If Greater Than or Equal To - Carry Flag Unset)
                            if ((Registers[Data.Register.rsf] & 0b10) == 0)
                            {
                                Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            }
                            else
                            {
                                Registers[Data.Register.rpo] += 1;
                            }
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised control low opcode");
                    }
                    break;
                case 0x1:  // Addition
                    Data.Register targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    ulong initial = Registers[targetRegister];
                    switch (opcodeLow)
                    {
                        case 0x0:  // ADD reg, reg
                            Registers[targetRegister] += MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // ADD reg, lit
                            Registers[targetRegister] += MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // ADD reg, adr
                            Registers[targetRegister] += MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // ADD reg, ptr
                            Registers[targetRegister] += MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // ICR reg
                            Registers[targetRegister]++;
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised addition low opcode");
                    }
                    if (Registers[targetRegister] < initial)
                    {
                        Registers[Data.Register.rsf] |= 0b10;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x2:  // Subtraction
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    initial = Registers[targetRegister];
                    switch (opcodeLow)
                    {
                        case 0x0:  // SUB reg, reg
                            Registers[targetRegister] -= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // SUB reg, lit
                            Registers[targetRegister] -= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // SUB reg, adr
                            Registers[targetRegister] -= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // SUB reg, ptr
                            Registers[targetRegister] -= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // DCR reg
                            Registers[targetRegister]--;
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised subtraction low opcode");
                    }
                    if (Registers[targetRegister] > initial)
                    {
                        Registers[Data.Register.rsf] |= 0b10;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x3:  // Multiplication
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    initial = Registers[targetRegister];
                    switch (opcodeLow)
                    {
                        case 0x0:  // MUL reg, reg
                            Registers[targetRegister] *= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // MUL reg, lit
                            Registers[targetRegister] *= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // MUL reg, adr
                            Registers[targetRegister] *= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // MUL reg, ptr
                            Registers[targetRegister] *= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised multiplication low opcode");
                    }
                    if (Registers[targetRegister] < initial)
                    {
                        Registers[Data.Register.rsf] |= 0b10;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x4:  // Division
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    switch (opcodeLow)
                    {
                        case 0x0:  // DIV reg, reg
                            Registers[targetRegister] /= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // DIV reg, lit
                            Registers[targetRegister] /= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // DIV reg, adr
                            Registers[targetRegister] /= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // DIV reg, ptr
                            Registers[targetRegister] /= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // DVR reg, reg, reg
                            ulong dividend = Registers[targetRegister];
                            ulong divisor = MemReadRegister(Registers[Data.Register.rpo] + 2);
                            ulong div = dividend / divisor;
                            ulong rem = dividend % divisor;
                            Registers[targetRegister] = div;
                            Registers[MemReadRegisterType(Registers[Data.Register.rpo] + 1)] = rem;
                            Registers[Data.Register.rpo] += 3;
                            break;
                        case 0x5:  // DVR reg, reg, lit
                            dividend = Registers[targetRegister];
                            divisor = MemReadQWord(Registers[Data.Register.rpo] + 2);
                            div = dividend / divisor;
                            rem = dividend % divisor;
                            Registers[targetRegister] = div;
                            Registers[MemReadRegisterType(Registers[Data.Register.rpo] + 1)] = rem;
                            Registers[Data.Register.rpo] += 10;
                            break;
                        case 0x6:  // DVR reg, reg, adr
                            dividend = Registers[targetRegister];
                            divisor = MemReadQWordPointer(Registers[Data.Register.rpo] + 2);
                            div = dividend / divisor;
                            rem = dividend % divisor;
                            Registers[targetRegister] = div;
                            Registers[MemReadRegisterType(Registers[Data.Register.rpo] + 1)] = rem;
                            Registers[Data.Register.rpo] += 10;
                            break;
                        case 0x7:  // DVR reg, reg, ptr
                            dividend = Registers[targetRegister];
                            divisor = MemReadRegisterQWord(Registers[Data.Register.rpo] + 2);
                            div = dividend / divisor;
                            rem = dividend % divisor;
                            Registers[targetRegister] = div;
                            Registers[MemReadRegisterType(Registers[Data.Register.rpo] + 1)] = rem;
                            Registers[Data.Register.rpo] += 3;
                            break;
                        case 0x8:  // REM reg, reg
                            Registers[targetRegister] %= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x9:  // REM reg, lit
                            Registers[targetRegister] %= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xA:  // REM reg, adr
                            Registers[targetRegister] %= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xB:  // REM reg, ptr
                            Registers[targetRegister] %= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised division low opcode");
                    }
                    if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x5:  // Shifting
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    initial = Registers[targetRegister];
                    int amount;
                    switch (opcodeLow)
                    {
                        case 0x0:  // SHL reg, reg
                            amount = (int)MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] <<= amount;
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // SHL reg, lit
                            amount = (int)MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] <<= amount;
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // SHL reg, adr
                            amount = (int)MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] <<= amount;
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // SHL reg, ptr
                            amount = (int)MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] <<= amount;
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // SHR reg, reg
                            amount = (int)MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] >>= amount;
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x5:  // SHR reg, lit
                            amount = (int)MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] >>= amount;
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x6:  // SHR reg, adr
                            amount = (int)MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] >>= amount;
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x7:  // SHR reg, ptr
                            amount = (int)MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[targetRegister] >>= amount;
                            Registers[Data.Register.rpo] += 2;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised shifting low opcode");
                    }
                    if (amount >= 64)
                    {
                        Registers[targetRegister] = 0;
                    }
                    if (opcodeLow <= 0x3 ? ((amount >= 64 && initial != 0) || (initial >> (64 - amount) << (64 - amount)) != 0) && amount != 0
                        : (initial & (uint)Math.Pow(2, amount) - 1) != 0)
                    {
                        Registers[Data.Register.rsf] |= 0b10;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x6:  // Bitwise
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    switch (opcodeLow)
                    {
                        case 0x0:  // AND reg, reg
                            Registers[targetRegister] &= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // AND reg, lit
                            Registers[targetRegister] &= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // AND reg, adr
                            Registers[targetRegister] &= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // AND reg, ptr
                            Registers[targetRegister] &= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // ORR reg, reg
                            Registers[targetRegister] |= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x5:  // ORR reg, lit
                            Registers[targetRegister] |= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x6:  // ORR reg, adr
                            Registers[targetRegister] |= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x7:  // ORR reg, ptr
                            Registers[targetRegister] |= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x8:  // XOR reg, reg
                            Registers[targetRegister] ^= MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x9:  // XOR reg, lit
                            Registers[targetRegister] ^= MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xA:  // XOR reg, adr
                            Registers[targetRegister] ^= MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xB:  // XOR reg, ptr
                            Registers[targetRegister] ^= MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0xC:  // NOT reg
                            Registers[targetRegister] = ~Registers[targetRegister];
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0xD:  // RNG reg
                            byte[] randomBuffer = new byte[8];
                            rng.NextBytes(randomBuffer);
                            Registers[targetRegister] = BinaryPrimitives.ReadUInt64LittleEndian(randomBuffer);
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised bitwise low opcode");
                    }
                    if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (Registers[targetRegister] == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x7:  // Test
                    targetRegister = MemReadRegisterType(Registers[Data.Register.rpo]);
                    if (!Data.WritableRegisters.Contains(targetRegister))
                    {
                        throw new InvalidOperationException($"Cannot write to read-only register {targetRegister}");
                    }
                    ulong newValue;
                    switch (opcodeLow)
                    {
                        case 0x0:  // TST reg, reg
                            newValue = Registers[targetRegister] & MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // TST reg, lit
                            newValue = Registers[targetRegister] & MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // TST reg, adr
                            newValue = Registers[targetRegister] & MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // TST reg, ptr
                            newValue = Registers[targetRegister] & MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // CMP reg, reg
                            newValue = Registers[targetRegister] - MemReadRegister(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x5:  // CMP reg, lit
                            newValue = Registers[targetRegister] - MemReadQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x6:  // CMP reg, adr
                            newValue = Registers[targetRegister] - MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x7:  // CMP reg, ptr
                            newValue = Registers[targetRegister] - MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised comparison low opcode");
                    }
                    if (opcodeLow >= 0x4 && newValue > Registers[targetRegister])
                    {
                        Registers[Data.Register.rsf] |= 0b10;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b10) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b10;
                    }
                    if (newValue == 0)
                    {
                        Registers[Data.Register.rsf] |= 0b1;
                    }
                    else if ((Registers[Data.Register.rsf] & 0b1) != 0)
                    {
                        Registers[Data.Register.rsf] ^= 0b1;
                    }
                    break;
                case 0x8:  // Small Move
                    switch (opcodeLow)
                    {
                        case 0x0:  // MVB reg, reg
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFF & MemReadRegister(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // MVB reg, lit
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFF & MemReadQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // MVB reg, adr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadBytePointer(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // MVB reg, ptr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadRegisterByte(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // MVB adr, reg
                            MemWriteBytePointer(Registers[Data.Register.rpo], (byte)(0xFF & MemReadRegister(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x5:  // MVB adr, lit
                            MemWriteBytePointer(Registers[Data.Register.rpo], (byte)(0xFF & MemReadQWord(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 16;
                            break;
                        case 0x6:  // MVB ptr, reg
                            MemWriteRegisterByte(Registers[Data.Register.rpo], (byte)(0xFF & MemReadRegister(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x7:  // MVB ptr, lit
                            MemWriteRegisterByte(Registers[Data.Register.rpo], (byte)(0xFF & MemReadQWord(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x8:  // MVW reg, reg
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFFFF & MemReadRegister(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x9:  // MVW reg, lit
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFFFF & MemReadQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xA:  // MVW reg, adr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadWordPointer(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xB:  // MVW reg, ptr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadRegisterWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0xC:  // MVW adr, reg
                            MemWriteWordPointer(Registers[Data.Register.rpo], (ushort)(0xFFFF & MemReadRegister(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xD:  // MVW adr, lit
                            MemWriteWordPointer(Registers[Data.Register.rpo], (ushort)(0xFFFF & MemReadQWord(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 16;
                            break;
                        case 0xE:  // MVW ptr, reg
                            MemWriteRegisterWord(Registers[Data.Register.rpo], (ushort)(0xFFFF & MemReadRegister(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0xF:  // MVW ptr, lit
                            MemWriteRegisterWord(Registers[Data.Register.rpo], (ushort)(0xFFFF & MemReadQWord(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised small move low opcode");
                    }
                    break;
                case 0x9:  // Large Move
                    switch (opcodeLow)
                    {
                        case 0x0:  // MVD reg, reg
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFFFFFFFF & MemReadRegister(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x1:  // MVD reg, lit
                            MemWriteRegister(Registers[Data.Register.rpo], 0xFFFFFFFF & MemReadQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x2:  // MVD reg, adr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadDWordPointer(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x3:  // MVD reg, ptr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadRegisterDWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x4:  // MVD adr, reg
                            MemWriteDWordPointer(Registers[Data.Register.rpo], (uint)(0xFFFFFFFF & MemReadRegister(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x5:  // MVD adr, lit
                            MemWriteDWordPointer(Registers[Data.Register.rpo], (uint)(0xFFFFFFFF & MemReadQWord(Registers[Data.Register.rpo] + 8)));
                            Registers[Data.Register.rpo] += 16;
                            break;
                        case 0x6:  // MVD ptr, reg
                            MemWriteRegisterDWord(Registers[Data.Register.rpo], (uint)(0xFFFFFFFF & MemReadRegister(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x7:  // MVD ptr, lit
                            MemWriteRegisterDWord(Registers[Data.Register.rpo], (uint)(0xFFFFFFFF & MemReadQWord(Registers[Data.Register.rpo] + 1)));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x8:  // MVQ reg, reg
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadRegister(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0x9:  // MVQ reg, lit
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xA:  // MVQ reg, adr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadQWordPointer(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xB:  // MVQ reg, ptr
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadRegisterQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0xC:  // MVQ adr, reg
                            MemWriteQWordPointer(Registers[Data.Register.rpo], MemReadRegister(Registers[Data.Register.rpo] + 8));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0xD:  // MVQ adr, lit
                            MemWriteQWordPointer(Registers[Data.Register.rpo], MemReadQWord(Registers[Data.Register.rpo] + 8));
                            Registers[Data.Register.rpo] += 16;
                            break;
                        case 0xE:  // MVQ ptr, reg
                            MemWriteRegisterQWord(Registers[Data.Register.rpo], MemReadRegister(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 2;
                            break;
                        case 0xF:  // MVQ ptr, lit
                            MemWriteRegisterQWord(Registers[Data.Register.rpo], MemReadQWord(Registers[Data.Register.rpo] + 1));
                            Registers[Data.Register.rpo] += 9;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised large move low opcode");
                    }
                    break;
                case 0xA:  // Stack
                    switch (opcodeLow)
                    {
                        case 0x0:  // PSH reg
                            Registers[Data.Register.rso] -= 8;
                            MemWriteQWord(Registers[Data.Register.rso], MemReadRegister(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x1:  // PSH lit
                            Registers[Data.Register.rso] -= 8;
                            MemWriteQWord(Registers[Data.Register.rso], MemReadQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x2:  // PSH adr
                            Registers[Data.Register.rso] -= 8;
                            MemWriteQWord(Registers[Data.Register.rso], MemReadQWordPointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x3:  // PSH ptr
                            Registers[Data.Register.rso] -= 8;
                            MemWriteQWord(Registers[Data.Register.rso], MemReadRegisterQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x4:  // POP reg
                            MemWriteRegister(Registers[Data.Register.rpo], MemReadQWord(Registers[Data.Register.rso]));
                            Registers[Data.Register.rso] += 8;
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised stack low opcode");
                    }
                    break;
                case 0xB:  // Subroutines
                    switch (opcodeLow)
                    {
                        case 0x0:  // CAL adr
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 8);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x1:  // CAL ptr
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 1);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0x2:  // CAL adr, reg
                            Registers[Data.Register.rfp] = MemReadRegister(Registers[Data.Register.rpo] + 8);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 9);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x3:  // CAL adr, lit
                            Registers[Data.Register.rfp] = MemReadQWord(Registers[Data.Register.rpo] + 8);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 16);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x4:  // CAL adr, adr
                            Registers[Data.Register.rfp] = MemReadQWordPointer(Registers[Data.Register.rpo] + 8);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 16);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x5:  // CAL adr, ptr
                            Registers[Data.Register.rfp] = MemReadRegisterQWord(Registers[Data.Register.rpo] + 8);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 9);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rpo]);
                            break;
                        case 0x6:  // CAL ptr, reg
                            Registers[Data.Register.rfp] = MemReadRegister(Registers[Data.Register.rpo] + 1);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 2);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0x7:  // CAL ptr, lit
                            Registers[Data.Register.rfp] = MemReadQWord(Registers[Data.Register.rpo] + 1);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 9);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0x8:  // CAL ptr, adr
                            Registers[Data.Register.rfp] = MemReadQWordPointer(Registers[Data.Register.rpo] + 1);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 9);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0x9:  // CAL ptr, ptr
                            Registers[Data.Register.rfp] = MemReadRegisterQWord(Registers[Data.Register.rpo] + 1);
                            MemWriteQWord(Registers[Data.Register.rso] - 8, Registers[Data.Register.rpo] + 2);
                            MemWriteQWord(Registers[Data.Register.rso] - 16, Registers[Data.Register.rsb]);
                            MemWriteQWord(Registers[Data.Register.rso] - 24, Registers[Data.Register.rso]);
                            Registers[Data.Register.rso] -= 24;
                            Registers[Data.Register.rsb] = Registers[Data.Register.rso];
                            Registers[Data.Register.rpo] = MemReadRegister(Registers[Data.Register.rpo]);
                            break;
                        case 0xA:  // RET
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rsb] + 16);
                            Registers[Data.Register.rso] = MemReadQWord(Registers[Data.Register.rsb]);
                            Registers[Data.Register.rsb] = MemReadQWord(Registers[Data.Register.rsb] + 8);
                            break;
                        case 0xB:  // RET reg
                            Registers[Data.Register.rrv] = MemReadRegister(Registers[Data.Register.rpo]);
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rsb] + 16);
                            Registers[Data.Register.rso] = MemReadQWord(Registers[Data.Register.rsb]);
                            Registers[Data.Register.rsb] = MemReadQWord(Registers[Data.Register.rsb] + 8);
                            break;
                        case 0xC:  // RET lit
                            Registers[Data.Register.rrv] = MemReadQWord(Registers[Data.Register.rpo]);
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rsb] + 16);
                            Registers[Data.Register.rso] = MemReadQWord(Registers[Data.Register.rsb]);
                            Registers[Data.Register.rsb] = MemReadQWord(Registers[Data.Register.rsb] + 8);
                            break;
                        case 0xD:  // RET adr
                            Registers[Data.Register.rrv] = MemReadQWordPointer(Registers[Data.Register.rpo]);
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rsb] + 16);
                            Registers[Data.Register.rso] = MemReadQWord(Registers[Data.Register.rsb]);
                            Registers[Data.Register.rsb] = MemReadQWord(Registers[Data.Register.rsb] + 8);
                            break;
                        case 0xE:  // RET ptr
                            Registers[Data.Register.rrv] = MemReadRegisterQWord(Registers[Data.Register.rpo]);
                            Registers[Data.Register.rpo] = MemReadQWord(Registers[Data.Register.rsb] + 16);
                            Registers[Data.Register.rso] = MemReadQWord(Registers[Data.Register.rsb]);
                            Registers[Data.Register.rsb] = MemReadQWord(Registers[Data.Register.rsb] + 8);
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised subroutine low opcode");
                    }
                    break;
                case 0xC:  // Console Write
                    switch (opcodeLow)
                    {
                        case 0x0:  // WCN reg
                            Console.Write(MemReadRegister(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x1:  // WCN lit
                            Console.Write(MemReadQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x2:  // WCN adr
                            Console.Write(MemReadQWordPointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x3:  // WCN ptr
                            Console.Write(MemReadRegisterQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x4:  // WCB reg
                            Console.Write(0xFF & MemReadRegister(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x5:  // WCB lit
                            Console.Write(Memory[Registers[Data.Register.rpo]]);
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x6:  // WCB adr
                            Console.Write(MemReadBytePointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x7:  // WCB ptr
                            Console.Write(MemReadRegisterByte(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x8:  // WCX reg
                            Console.Write(string.Format("{0:X}", 0xFF & MemReadRegister(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x9:  // WCX lit
                            Console.Write(string.Format("{0:X}", Memory[Registers[Data.Register.rpo]]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xA:  // WCX adr
                            Console.Write(string.Format("{0:X}", MemReadBytePointer(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xB:  // WCX ptr
                            Console.Write(string.Format("{0:X}", MemReadRegisterByte(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0xC:  // WCC reg
                            Console.Write((char)(0xFF & MemReadRegister(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0xD:  // WCC lit
                            Console.Write((char)Memory[Registers[Data.Register.rpo]]);
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xE:  // WCC adr
                            Console.Write((char)MemReadBytePointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xF:  // WCC ptr
                            Console.Write((char)MemReadRegisterByte(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised console write low opcode");
                    }
                    break;
                case 0xD:  // File Write
                    if (openFile is null)
                    {
                        throw new InvalidOperationException("Cannot perform file operations if no file is open. Run OFL (0xE0) first");
                    }
                    switch (opcodeLow)
                    {
                        case 0x0:  // WFN reg
                            fileWrite!.Write(MemReadRegister(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x1:  // WFN lit
                            fileWrite!.Write(MemReadQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x2:  // WFN adr
                            fileWrite!.Write(MemReadQWordPointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x3:  // WFN ptr
                            fileWrite!.Write(MemReadRegisterQWord(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x4:  // WFB reg
                            fileWrite!.Write(0xFF & MemReadRegister(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x5:  // WFB lit
                            fileWrite!.Write(Memory[Registers[Data.Register.rpo]]);
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x6:  // WFB adr
                            fileWrite!.Write(MemReadBytePointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0x7:  // WFB ptr
                            fileWrite!.Write(MemReadRegisterByte(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x8:  // WFX reg
                            fileWrite!.Write(string.Format("{0:X}", 0xFF & MemReadRegister(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x9:  // WFX lit
                            fileWrite!.Write(string.Format("{0:X}", Memory[Registers[Data.Register.rpo]]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xA:  // WFX adr
                            fileWrite!.Write(string.Format("{0:X}", MemReadBytePointer(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xB:  // WFX ptr
                            fileWrite!.Write(string.Format("{0:X}", MemReadRegisterByte(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0xC:  // WFC reg
                            fileWrite!.Write((char)(0xFF & MemReadRegister(Registers[Data.Register.rpo])));
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0xD:  // WFC lit
                            fileWrite!.Write((char)Memory[Registers[Data.Register.rpo]]);
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xE:  // WFC adr
                            fileWrite!.Write((char)MemReadBytePointer(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo] += 8;
                            break;
                        case 0xF:  // WFC ptr
                            fileWrite!.Write((char)MemReadRegisterByte(Registers[Data.Register.rpo]));
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised file write low opcode");
                    }
                    break;
                case 0xE:  // File Operations
                    switch (opcodeLow)
                    {
                        case 0x0:  // OFL adr
                            if (openFile is not null)
                            {
                                throw new InvalidOperationException("Cannot execute open file instruction if a file is already open");
                            }
                            string filepath = "";
                            for (ulong i = MemReadQWord(Registers[Data.Register.rpo]); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            Registers[Data.Register.rpo] += 8;
                            openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            fileWrite = new StreamWriter(openFile);
                            fileRead = new StreamReader(openFile);
                            if (fileRead.EndOfStream)
                            {
                                Registers[Data.Register.rsf] |= 0b100;
                            }
                            else if ((Registers[Data.Register.rsf] & 0b100) != 0)
                            {
                                Registers[Data.Register.rsf] ^= 0b100;
                            }
                            break;
                        case 0x1:  // OFL ptr
                            if (openFile is not null)
                            {
                                throw new InvalidOperationException("Cannot execute open file instruction if a file is already open");
                            }
                            filepath = "";
                            for (ulong i = MemReadRegister(Registers[Data.Register.rpo]); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            Registers[Data.Register.rpo] += 8;
                            openFile = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            fileWrite = new StreamWriter(openFile);
                            fileRead = new StreamReader(openFile);
                            if (fileRead.EndOfStream)
                            {
                                Registers[Data.Register.rsf] |= 0b100;
                            }
                            else if ((Registers[Data.Register.rsf] & 0b100) != 0)
                            {
                                Registers[Data.Register.rsf] ^= 0b100;
                            }
                            break;
                        case 0x2:  // CFL
                            if (openFile is null)
                            {
                                throw new InvalidOperationException("Cannot execute close file instruction if a file is not open");
                            }
                            fileWrite!.Close();
                            fileWrite = null;
                            fileRead!.Close();
                            fileRead = null;
                            openFile!.Close();
                            openFile = null;
                            break;
                        case 0x3:  // DFL adr
                            filepath = "";
                            for (ulong i = MemReadQWord(Registers[Data.Register.rpo]); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            Registers[Data.Register.rpo] += 8;
                            File.Delete(filepath);
                            break;
                        case 0x4:  // DFL ptr
                            filepath = "";
                            for (ulong i = MemReadRegister(Registers[Data.Register.rpo]); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            Registers[Data.Register.rpo] += 8;
                            File.Delete(filepath);
                            break;
                        case 0x5:  // FEX reg, adr
                            filepath = "";
                            for (ulong i = MemReadQWord(Registers[Data.Register.rpo] + 1); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            MemWriteRegister(Registers[Data.Register.rpo], File.Exists(filepath) ? 1UL : 0UL);
                            Registers[Data.Register.rpo] += 9;
                            break;
                        case 0x6:  // FEX reg, ptr
                            filepath = "";
                            for (ulong i = MemReadRegister(Registers[Data.Register.rpo] + 1); Memory[i] != 0x0; i++)
                            {
                                filepath += (char)Memory[i];
                            }
                            MemWriteRegister(Registers[Data.Register.rpo], File.Exists(filepath) ? 1UL : 0UL);
                            Registers[Data.Register.rpo] += 2;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised file operation low opcode");
                    }
                    break;
                case 0xF:  // Reading
                    switch (opcodeLow)
                    {
                        case 0x0:  // RCC reg
                            ConsoleKeyInfo pressedKey = new();
                            while (char.IsControl(pressedKey.KeyChar) && pressedKey.KeyChar != '\r')
                            {
                                pressedKey = Console.ReadKey(true);
                            }
                            char pressedChar = pressedKey.KeyChar == '\r' ? '\n' : pressedKey.KeyChar;
                            MemWriteRegister(Registers[Data.Register.rpo], pressedChar);
                            Console.Write(pressedChar);
                            Registers[Data.Register.rpo]++;
                            break;
                        case 0x1:  // RFC reg
                            MemWriteRegister(Registers[Data.Register.rpo], (char)fileRead!.Read());
                            if (fileRead.EndOfStream)
                            {
                                Registers[Data.Register.rsf] |= 0b100;
                            }
                            Registers[Data.Register.rpo]++;
                            break;
                        default:
                            throw new InvalidOperationException($"{opcodeLow:X} is not a recognised reading low opcode");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"{opcodeHigh:X} is not a recognised high opcode");
            }
            return halt;
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned, integer) from the given memory offset.
        /// </summary>
        public ushort MemReadWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 2)]);
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the given memory offset.
        /// </summary>
        public uint MemReadDWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 4)]);
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the given memory offset.
        /// </summary>
        public ulong MemReadQWord(ulong offset)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 8)]);
        }

        /// <summary>
        /// Read the stored register type at the given memory offset.
        /// </summary>
        public Data.Register MemReadRegisterType(ulong offset)
        {
            return (Data.Register)Memory[offset];
        }

        /// <summary>
        /// Read the value of the stored register type at the given memory offset.
        /// </summary>
        public ulong MemReadRegister(ulong offset)
        {
            return Registers[MemReadRegisterType(offset)];
        }

        /// <summary>
        /// Read a byte from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public byte MemReadRegisterByte(ulong offset)
        {
            return Memory[Registers[MemReadRegisterType(offset)]];
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public ushort MemReadRegisterWord(ulong offset)
        {
            return MemReadWord(Registers[MemReadRegisterType(offset)]);
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public uint MemReadRegisterDWord(ulong offset)
        {
            return MemReadDWord(Registers[MemReadRegisterType(offset)]);
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public ulong MemReadRegisterQWord(ulong offset)
        {
            return MemReadQWord(Registers[MemReadRegisterType(offset)]);
        }

        /// <summary>
        /// Read a byte from the memory address stored at the given memory offset.
        /// </summary>
        public byte MemReadBytePointer(ulong offset)
        {
            return Memory[MemReadQWord(offset)];
        }

        /// <summary>
        /// Read a word (16 bit, 2 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public ushort MemReadWordPointer(ulong offset)
        {
            return MemReadWord(MemReadQWord(offset));
        }

        /// <summary>
        /// Read a double word (32 bit, 4 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public uint MemReadDWordPointer(ulong offset)
        {
            return MemReadDWord(MemReadQWord(offset));
        }

        /// <summary>
        /// Read a quad word (64 bit, 8 byte, unsigned integer) from the memory address stored at the given memory offset.
        /// </summary>
        public ulong MemReadQWordPointer(ulong offset)
        {
            return MemReadQWord(MemReadQWord(offset));
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void MemWriteWord(ulong offset, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 2)], value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void MemWriteDWord(ulong offset, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 4)], value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the given memory offset.
        /// </summary>
        public void MemWriteQWord(ulong offset, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(Memory.AsSpan()[(int)offset..(int)(offset + 8)], value);
        }

        /// <summary>
        /// Modify the value of the stored register type at the given memory offset.
        /// </summary>
        public void MemWriteRegister(ulong offset, ulong value)
        {
            Data.Register registerType = MemReadRegisterType(offset);
            if (!Data.WritableRegisters.Contains(registerType))
            {
                throw new InvalidOperationException($"Cannot write to read-only register {registerType}");
            }
            Registers[registerType] = value;
        }

        /// <summary>
        /// Write a byte to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void MemWriteRegisterByte(ulong offset, byte value)
        {
            Memory[Registers[MemReadRegisterType(offset)]] = value;
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void MemWriteRegisterWord(ulong offset, ushort value)
        {
            MemWriteWord(Registers[MemReadRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void MemWriteRegisterDWord(ulong offset, uint value)
        {
            MemWriteDWord(Registers[MemReadRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored at the register type stored at the given memory offset.
        /// </summary>
        public void MemWriteRegisterQWord(ulong offset, ulong value)
        {
            MemWriteQWord(Registers[MemReadRegisterType(offset)], value);
        }

        /// <summary>
        /// Write a byte to the memory address stored at the given memory offset.
        /// </summary>
        public void MemWriteBytePointer(ulong offset, byte value)
        {
            Memory[MemReadQWord(offset)] = value;
        }

        /// <summary>
        /// Write a word (16 bit, 2 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void MemWriteWordPointer(ulong offset, ushort value)
        {
            MemWriteWord(MemReadQWord(offset), value);
        }

        /// <summary>
        /// Write a double word (32 bit, 4 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void MemWriteDWordPointer(ulong offset, uint value)
        {
            MemWriteDWord(MemReadQWord(offset), value);
        }

        /// <summary>
        /// Write a quad word (64 bit, 8 byte, unsigned integer) to the memory address stored at the given memory offset.
        /// </summary>
        public void MemWriteQWordPointer(ulong offset, ulong value)
        {
            MemWriteQWord(MemReadQWord(offset), value);
        }
    }
}
