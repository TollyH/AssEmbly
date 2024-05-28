namespace AssEmbly
{
    public enum OperandType
    {
        Register,
        Literal,
        Address,
        Pointer
    }

    public enum Register
    {
        rpo,  // Program Offset
        rso,  // Stack Offset
        rsb,  // Stack Base
        rsf,  // Status Flags
        rrv,  // Return Value
        rfp,  // Fast Pass Parameter
        rg0,  // General 0
        rg1,  // General 1
        rg2,  // General 2
        rg3,  // General 3
        rg4,  // General 4
        rg5,  // General 5
        rg6,  // General 6
        rg7,  // General 7
        rg8,  // General 8
        rg9,  // General 9
    }

    [Flags]
    public enum StatusFlags : ulong
    {
        Zero = 0b1,
        Carry = 0b10,
        FileEnd = 0b100,
#if EXTENSION_SET_SIGNED
        Sign = 0b1000,
        Overflow = 0b10000,
#endif
#if EXTENSION_SET_TERMINAL
        AutoEcho = 0b100000,
#endif

        ZeroAndCarry = Zero | Carry,
#if EXTENSION_SET_SIGNED
        SignAndOverflow = Sign | Overflow,
#endif
    }

#if DISPLACEMENT
    public enum DisplacementMode : byte
    {
        NoDisplacement = 0b00,
        Constant = 0b01,
        Register = 0b10,
        ConstantAndRegister = 0b11
    }

    public enum DisplacementMultiplier : byte
    {
        x1 = 0b000,
        x2 = 0b001,
        x4 = 0b010,
        x8 = 0b011,
        x16 = 0b100,
        x32 = 0b101,
        x64 = 0b110,
        x128 = 0b111
    }

    public enum PointerReadSize : byte
    {
        QuadWord = 0b00,
        DoubleWord = 0b01,
        Word = 0b10,
        Byte = 0b11
    }

    public enum AddressReferenceType
    {
        LabelAddress,  // :XYZ
        LabelLiteral,  // :&XYZ
        LiteralAddress  // :123
    }
#endif

    /// <summary>
    /// Stores <see langword="static"/> data about AssEmbly, such as register and operand types, and mnemonic mappings.
    /// </summary>
    public static class Data
    {
        public static readonly Dictionary<byte, AAPFeatures> ExtensionSetFeatureFlags = new()
        {
#if EXTENSION_SET_SIGNED
            { 0x01, AAPFeatures.ExtensionSigned },
#endif
#if EXTENSION_SET_FLOATING_POINT
            { 0x02, AAPFeatures.ExtensionFloat },
#endif
#if EXTENSION_SET_EXTENDED_BASE
            { 0x03, AAPFeatures.ExtensionExtendedBase },
#endif
#if EXTENSION_SET_EXTERNAL_ASM
            { 0x04, AAPFeatures.ExtensionExternalAssembly },
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
            { 0x05, AAPFeatures.ExtensionMemoryAllocation },
#endif
#if EXTENSION_SET_FILE_SYSTEM
            { 0x06, AAPFeatures.ExtensionFileSystem },
#endif
#if EXTENSION_SET_TERMINAL
            { 0x07, AAPFeatures.ExtensionTerminal },
#endif
        };

        /// <summary>
        /// A mapping of what opcode a mnemonic with a particular set of operands should compile to.
        /// </summary>
        public static readonly Dictionary<(string Mnemonic, OperandType[] OperandTypes), Opcode> Mnemonics = new(new MnemonicComparer())
        {
            // BASE INSTRUCTION SET

            // Control
            // HLT (Halt)
            { ("HLT", Array.Empty<OperandType>()), new Opcode(0x00, 0x00) },
            // NOP (No Operation)
            { ("NOP", Array.Empty<OperandType>()), new Opcode(0x00, 0x01) },

            // Jump
            // JMP (Unconditional Jump)
            { ("JMP", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x02) },
            { ("JMP", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x03) },
            // JEQ (Jump If Equal To - Zero Flag Set)
            { ("JEQ", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x04) },
            { ("JZO", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x04) },
            { ("JEQ", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x05) },
            { ("JZO", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x05) },
            // JNE (Jump If Not Equal To - Zero Flag Unset)
            { ("JNE", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x06) },
            { ("JNZ", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x06) },
            { ("JNE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x07) },
            { ("JNZ", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x07) },
            // JLT (Jump If Less Than - Carry Flag Set)
            { ("JLT", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x08) },
            { ("JCA", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x08) },
            { ("JLT", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x09) },
            { ("JCA", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x09) },
            // JLE (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
            { ("JLE", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x0A) },
            { ("JLE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x0B) },
            // JGT (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
            { ("JGT", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x0C) },
            { ("JGT", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x0D) },
            // JGE (Jump If Greater Than or Equal To - Carry Flag Unset)
            { ("JGE", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x0E) },
            { ("JNC", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0x0E) },
            { ("JGE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x0F) },
            { ("JNC", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0x0F) },


            // Math (All operations store result in the first register operand)
            // ADD (Addition)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x10) },  // (reg + reg)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x11) },  // (reg + lit)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x12) },  // (reg + adr)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x13) },  // (reg + ptr)

            // ICR (Increment)
            { ("ICR", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0x14) },

            // SUB (Subtraction)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x20) },  // (reg - reg)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x21) },  // (reg - lit)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x22) },  // (reg - adr)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x23) },  // (reg - ptr)

            // DCR (Decrement)
            { ("DCR", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0x24) },

            // MUL (Multiplication)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x30) },  // (reg * reg)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x31) },  // (reg * lit)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x32) },  // (reg * adr)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x33) },  // (reg * ptr)

            // DIV (Integer Division)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x40) },  // (reg / reg)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x41) },  // (reg / lit)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x42) },  // (reg / adr)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x43) },  // (reg / ptr)

            // DVR (Integer Division With Remainder [Remainder Stored in Second Register])
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x44) },  // (reg / reg)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x45) },  // (reg / lit)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x46) },  // (reg / adr)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x47) },  // (reg / ptr)

            // REM (Remainder)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x48) },  // (reg % reg)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x49) },  // (reg % lit)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x4A) },  // (reg % adr)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x4B) },  // (reg % ptr)

            // SHL (Shift Left)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x50) },  // (reg << reg)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x51) },  // (reg << lit)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x52) },  // (reg << adr)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x53) },  // (reg << ptr)

            // SHR (Shift Right)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x54) },  // (reg >> reg)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x55) },  // (reg >> lit)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x56) },  // (reg >> adr)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x57) },  // (reg >> ptr)

            // Bitwise (All operations store result in the first register operand)
            // AND
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x60) },  // (reg & reg)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x61) },  // (reg & lit)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x62) },  // (reg & adr)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x63) },  // (reg & ptr)

            // ORR
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x64) },  // (reg | reg)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x65) },  // (reg | lit)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x66) },  // (reg | adr)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x67) },  // (reg | ptr)

            // XOR
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x68) },  // (reg ^ reg)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x69) },  // (reg ^ lit)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x6A) },  // (reg ^ adr)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x6B) },  // (reg ^ ptr)

            // NOT
            { ("NOT", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0x6C) },

            // RNG
            { ("RNG", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0x6D) },

            // Comparison (Results will be discarded, but flags will still be set - best used in conjunction with conditional jumps)
            // TST (Test [Bitwise And Then Discard Result])
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x70) },  // (reg & reg)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x71) },  // (reg & lit)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x72) },  // (reg & adr)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x73) },  // (reg & ptr)

            // CMP (Compare [Subtract Then Discard Result])
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x74) },  // (reg - reg)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x75) },  // (reg - lit)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x76) },  // (reg - adr)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x77) },  // (reg - ptr)

            // Data
            // MVB (Move Byte [8 bits])
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x80) },  // (reg <- reg)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x81) },  // (reg <- lit)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x82) },  // (reg <- adr)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x83) },  // (reg <- ptr)
            { ("MVB", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x00, 0x84) },  // (adr <- reg)
            { ("MVB", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x00, 0x85) },  // (adr <- lit)
            { ("MVB", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x00, 0x86) },  // (ptr <- reg)
            { ("MVB", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x00, 0x87) },  // (ptr <- lit)

            // MVW (Move Word [16 bits - 2 bytes])
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x88) },  // (reg <- reg)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x89) },  // (reg <- lit)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x8A) },  // (reg <- adr)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x8B) },  // (reg <- ptr)
            { ("MVW", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x00, 0x8C) },  // (adr <- reg)
            { ("MVW", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x00, 0x8D) },  // (adr <- lit)
            { ("MVW", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x00, 0x8E) },  // (ptr <- reg)
            { ("MVW", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x00, 0x8F) },  // (ptr <- lit)

            // MVD (Move Double Word [32 bits - 4 bytes])
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x90) },  // (reg <- reg)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x91) },  // (reg <- lit)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x92) },  // (reg <- adr)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x93) },  // (reg <- ptr)
            { ("MVD", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x00, 0x94) },  // (adr <- reg)
            { ("MVD", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x00, 0x95) },  // (adr <- lit)
            { ("MVD", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x00, 0x96) },  // (ptr <- reg)
            { ("MVD", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x00, 0x97) },  // (ptr <- lit)

            // MVQ (Move Quad Word [64 bits - 8 bytes])
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x00, 0x98) },  // (reg <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x00, 0x99) },  // (reg <- lit)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0x9A) },  // (reg <- adr)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0x9B) },  // (reg <- ptr)
            { ("MVQ", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x00, 0x9C) },  // (adr <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x00, 0x9D) },  // (adr <- lit)
            { ("MVQ", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x00, 0x9E) },  // (ptr <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x00, 0x9F) },  // (ptr <- lit)

            // Stack and Subroutines [All Stack Operations are 64-bit]
            // PSH (Push Value to Stack)
            { ("PSH", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xA0) },  // reg ->
            { ("PSH", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xA1) },  // lit ->
            { ("PSH", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xA2) },  // adr ->
            { ("PSH", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xA3) },  // ptr ->

            // POP (Pop Value from Stack)
            { ("POP", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xA4) },  // reg <-

            // CAL (Call Subroutine at Address/Pointer, Pushing rpo and rsb to the stack)
            { ("CAL", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xB0) },
            { ("CAL", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xB1) },

            // CAL (Call Subroutine at Address/Pointer, Pushing rpo and rsb to the stack, storing second operand in rfp)
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x00, 0xB2) },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x00, 0xB3) },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Address }), new Opcode(0x00, 0xB4) },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Pointer }), new Opcode(0x00, 0xB5) },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x00, 0xB6) },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x00, 0xB7) },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Address }), new Opcode(0x00, 0xB8) },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Pointer }), new Opcode(0x00, 0xB9) },

            // RET (Return from Subroutine, Restoring rsb and rpo from the stack)
            { ("RET", Array.Empty<OperandType>()), new Opcode(0x00, 0xBA) },

            // RET (Return from Subroutine, Restoring rsb and rpo from the stack, storing operand in rrv)
            { ("RET", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xBB) },
            { ("RET", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xBC) },
            { ("RET", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xBD) },
            { ("RET", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xBE) },

            // Console Write
            // WCN (Write 64-bit Number to Console as Decimal)
            { ("WCN", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xC0) },
            { ("WCN", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xC1) },
            { ("WCN", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xC2) },
            { ("WCN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xC3) },

            // WCB (Write Byte to Console as Decimal)
            { ("WCB", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xC4) },
            { ("WCB", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xC5) },
            { ("WCB", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xC6) },
            { ("WCB", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xC7) },

            // WCX (Write Byte to Console as Hex)
            { ("WCX", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xC8) },
            { ("WCX", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xC9) },
            { ("WCX", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xCA) },
            { ("WCX", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xCB) },

            // WCC (Write Raw Byte to Console)
            { ("WCC", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xCC) },
            { ("WCC", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xCD) },
            { ("WCC", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xCE) },
            { ("WCC", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xCF) },

            // File Write
            // WFN (Write 64-bit Number to Open File as Decimal)
            { ("WFN", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xD0) },
            { ("WFN", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xD1) },
            { ("WFN", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xD2) },
            { ("WFN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xD3) },

            // WFB (Write Byte to Open File as Decimal)
            { ("WFB", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xD4) },
            { ("WFB", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xD5) },
            { ("WFB", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xD6) },
            { ("WFB", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xD7) },

            // WFX (Write Byte to Open File as Hex)
            { ("WFX", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xD8) },
            { ("WFX", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xD9) },
            { ("WFX", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xDA) },
            { ("WFX", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xDB) },

            // WFC (Write Raw Byte to Open File)
            { ("WFC", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xDC) },
            { ("WFC", new OperandType[1] { OperandType.Literal }), new Opcode(0x00, 0xDD) },
            { ("WFC", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xDE) },
            { ("WFC", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xDF) },

            // OFL (Open File at Path Specified by 0x00 Terminated String in Memory)
            { ("OFL", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xE0) },
            { ("OFL", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xE1) },

            // CFL (Close Currently Open File)
            { ("CFL", Array.Empty<OperandType>()), new Opcode(0x00, 0xE2) },

            // DFL (Delete File at Path Specified by 0x00 Terminated String in Memory)
            { ("DFL", new OperandType[1] { OperandType.Address }), new Opcode(0x00, 0xE3) },
            { ("DFL", new OperandType[1] { OperandType.Pointer }), new Opcode(0x00, 0xE4) },

            // FEX (Does File Exist at Path Specified by 0x00 Terminated String in Memory?)
            { ("FEX", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0xE5) },
            { ("FEX", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0xE6) },

            // FSZ (Get Size of File at Path Specified by 0x00 Terminated String in Memory)
            { ("FSZ", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x00, 0xE7) },
            { ("FSZ", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x00, 0xE8) },

            // Reading
            // RCC (Read Character from Console as a Byte)
            { ("RCC", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xF0) },

            // RFC (Read Character from Open File as a Byte)
            { ("RFC", new OperandType[1] { OperandType.Register }), new Opcode(0x00, 0xF1) },

#if EXTENSION_SET_SIGNED
            // SIGNED EXTENSION SET

            // Signed Conditional Jumps
            // SIGN_JLT (Jump If Less Than - Sign Flag != Overflow Flag)
            { ("SIGN_JLT", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x00) },
            { ("SIGN_JLT", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x01) },
            // SIGN_JLE (Jump If Less Than or Equal To - Sign Flag != Overflow Flag or Zero Flag Set)
            { ("SIGN_JLE", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x02) },
            { ("SIGN_JLE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x03) },
            // SIGN_JGT (Jump If Greater Than - Sign Flag == Overflow Flag and Zero Flag Unset)
            { ("SIGN_JGT", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x04) },
            { ("SIGN_JGT", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x05) },
            // SIGN_JGE (Jump If Greater Than or Equal To - Sign Flag == Overflow Flag)
            { ("SIGN_JGE", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x06) },
            { ("SIGN_JGE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x07) },
            // SIGN_JSI (Jump If Sign Flag Set)
            { ("SIGN_JSI", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x08) },
            { ("SIGN_JSI", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x09) },
            // SIGN_JNS (Jump If Sign Flag Unset)
            { ("SIGN_JNS", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x0A) },
            { ("SIGN_JNS", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x0B) },
            // SIGN_JOV (Jump If Overflow Flag Set)
            { ("SIGN_JOV", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x0C) },
            { ("SIGN_JOV", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x0D) },
            // SIGN_JNO (Jump If Overflow Flag Unset)
            { ("SIGN_JNO", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x0E) },
            { ("SIGN_JNO", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x0F) },

            // Signed Math (All operations store result in the first register operand)
            // SIGN_DIV (Integer Division)
            { ("SIGN_DIV", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x10) },  // (reg / reg)
            { ("SIGN_DIV", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x11) },  // (reg / lit)
            { ("SIGN_DIV", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x12) },  // (reg / adr)
            { ("SIGN_DIV", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x13) },  // (reg / ptr)

            // SIGN_DVR (Integer Division With Remainder [Remainder Stored in Second Register])
            { ("SIGN_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x14) },  // (reg / reg)
            { ("SIGN_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x15) },  // (reg / lit)
            { ("SIGN_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x16) },  // (reg / adr)
            { ("SIGN_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x17) },  // (reg / ptr)

            // SIGN_REM (Remainder)
            { ("SIGN_REM", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x18) },  // (reg % reg)
            { ("SIGN_REM", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x19) },  // (reg % lit)
            { ("SIGN_REM", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x1A) },  // (reg % adr)
            { ("SIGN_REM", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x1B) },  // (reg % ptr)

            // SIGN_SHR (Shift Right)
            { ("SIGN_SHR", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x20) },  // (reg >> reg)
            { ("SIGN_SHR", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x21) },  // (reg >> lit)
            { ("SIGN_SHR", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x22) },  // (reg >> adr)
            { ("SIGN_SHR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x23) },  // (reg >> ptr)

            // Sign-Preserving Data Moves
            // SIGN_MVB (Move Byte [8 bits])
            { ("SIGN_MVB", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x30) },  // (reg <- reg)
            { ("SIGN_MVB", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x31) },  // (reg <- lit)
            { ("SIGN_MVB", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x32) },  // (reg <- adr)
            { ("SIGN_MVB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x33) },  // (reg <- ptr)

            // SIGN_MVW (Move Word [16 bits - 2 bytes])
            { ("SIGN_MVW", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x34) },  // (reg <- reg)
            { ("SIGN_MVW", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x35) },  // (reg <- lit)
            { ("SIGN_MVW", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x36) },  // (reg <- adr)
            { ("SIGN_MVW", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x37) },  // (reg <- ptr)

            // SIGN_MVD (Move Double Word [32 bits - 4 bytes])
            { ("SIGN_MVD", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x01, 0x40) },  // (reg <- reg)
            { ("SIGN_MVD", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x01, 0x41) },  // (reg <- lit)
            { ("SIGN_MVD", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x01, 0x42) },  // (reg <- adr)
            { ("SIGN_MVD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x01, 0x43) },  // (reg <- ptr)

            // Console Write
            // SIGN_WCN (Write 64-bit Number to Console as Decimal)
            { ("SIGN_WCN", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x50) },
            { ("SIGN_WCN", new OperandType[1] { OperandType.Literal }), new Opcode(0x01, 0x51) },
            { ("SIGN_WCN", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x52) },
            { ("SIGN_WCN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x53) },

            // SIGN_WCB (Write Byte to Console as Decimal)
            { ("SIGN_WCB", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x54) },
            { ("SIGN_WCB", new OperandType[1] { OperandType.Literal }), new Opcode(0x01, 0x55) },
            { ("SIGN_WCB", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x56) },
            { ("SIGN_WCB", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x57) },

            // File Write
            // SIGN_WFN (Write 64-bit Number to Open File as Decimal)
            { ("SIGN_WFN", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x60) },
            { ("SIGN_WFN", new OperandType[1] { OperandType.Literal }), new Opcode(0x01, 0x61) },
            { ("SIGN_WFN", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x62) },
            { ("SIGN_WFN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x63) },

            // SIGN_WFB (Write Byte to Open File as Decimal)
            { ("SIGN_WFB", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x64) },
            { ("SIGN_WFB", new OperandType[1] { OperandType.Literal }), new Opcode(0x01, 0x65) },
            { ("SIGN_WFB", new OperandType[1] { OperandType.Address }), new Opcode(0x01, 0x66) },
            { ("SIGN_WFB", new OperandType[1] { OperandType.Pointer }), new Opcode(0x01, 0x67) },

            // Signed Specific Operations
            // SIGN_EXB (Extend Signed Byte to Signed Quad Word)
            { ("SIGN_EXB", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x70) },
            // SIGN_EXW (Extend Signed Word to Signed Quad Word)
            { ("SIGN_EXW", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x71) },
            // SIGN_EXD (Extend Signed Double Word to Signed Quad Word)
            { ("SIGN_EXD", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x72) },

            // SIGN_NEG (Two's Complement Negation)
            { ("SIGN_NEG", new OperandType[1] { OperandType.Register }), new Opcode(0x01, 0x80) },
#endif

#if EXTENSION_SET_FLOATING_POINT
            // FLOATING POINT EXTENSION SET

            // Math (All operations store result in the first register operand)
            // FLPT_ADD (Addition)
            { ("FLPT_ADD", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x00) },  // (reg + reg)
            { ("FLPT_ADD", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x01) },  // (reg + lit)
            { ("FLPT_ADD", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x02) },  // (reg + adr)
            { ("FLPT_ADD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x03) },  // (reg + ptr)

            // FLPT_SUB (Subtraction)
            { ("FLPT_SUB", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x10) },  // (reg - reg)
            { ("FLPT_SUB", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x11) },  // (reg - lit)
            { ("FLPT_SUB", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x12) },  // (reg - adr)
            { ("FLPT_SUB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x13) },  // (reg - ptr)

            // FLPT_MUL (Multiplication)
            { ("FLPT_MUL", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x20) },  // (reg * reg)
            { ("FLPT_MUL", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x21) },  // (reg * lit)
            { ("FLPT_MUL", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x22) },  // (reg * adr)
            { ("FLPT_MUL", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x23) },  // (reg * ptr)

            // FLPT_DIV (Division)
            { ("FLPT_DIV", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x30) },  // (reg / reg)
            { ("FLPT_DIV", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x31) },  // (reg / lit)
            { ("FLPT_DIV", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x32) },  // (reg / adr)
            { ("FLPT_DIV", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x33) },  // (reg / ptr)

            // FLPT_DVR (Division With Remainder [Remainder Stored in Second Register])
            { ("FLPT_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x34) },  // (reg / reg)
            { ("FLPT_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x35) },  // (reg / lit)
            { ("FLPT_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x36) },  // (reg / adr)
            { ("FLPT_DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x37) },  // (reg / ptr)

            // FLPT_REM (Remainder)
            { ("FLPT_REM", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x38) },  // (reg % reg)
            { ("FLPT_REM", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x39) },  // (reg % lit)
            { ("FLPT_REM", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x3A) },  // (reg % adr)
            { ("FLPT_REM", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x3B) },  // (reg % ptr)

            // Floating point specific math
            // FLPT_SIN (Sine)
            { ("FLPT_SIN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x40) },
            // FLPT_ASN (Inverse sine)
            { ("FLPT_ASN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x41) },
            // FLPT_COS (Cosine)
            { ("FLPT_COS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x42) },
            // FLPT_ACS (Inverse Cosine)
            { ("FLPT_ACS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x43) },
            // FLPT_TAN (Tangent)
            { ("FLPT_TAN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x44) },
            // FLPT_ATN (Inverse tangent)
            { ("FLPT_ATN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x45) },
            // FLPT_PTN (2 argument inverse tangent)
            { ("FLPT_PTN", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x46) },
            { ("FLPT_PTN", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x47) },
            { ("FLPT_PTN", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x48) },
            { ("FLPT_PTN", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x49) },

            // FLPT_POW (Exponentiation)
            { ("FLPT_POW", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x50) },
            { ("FLPT_POW", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x51) },
            { ("FLPT_POW", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x52) },
            { ("FLPT_POW", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x53) },

            // FLPT_LOG (Logarithm)
            { ("FLPT_LOG", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0x60) },
            { ("FLPT_LOG", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0x61) },
            { ("FLPT_LOG", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0x62) },
            { ("FLPT_LOG", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0x63) },

            // Console Write
            // FLPT_WCN (Write Double Precision Floating Point Number to Console as Decimal)
            { ("FLPT_WCN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x70) },
            { ("FLPT_WCN", new OperandType[1] { OperandType.Literal }), new Opcode(0x02, 0x71) },
            { ("FLPT_WCN", new OperandType[1] { OperandType.Address }), new Opcode(0x02, 0x72) },
            { ("FLPT_WCN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x02, 0x73) },

            // File Write
            // FLPT_WFN (Write Double Precision Floating Point Number to Open File as Decimal)
            { ("FLPT_WFN", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x80) },
            { ("FLPT_WFN", new OperandType[1] { OperandType.Literal }), new Opcode(0x02, 0x81) },
            { ("FLPT_WFN", new OperandType[1] { OperandType.Address }), new Opcode(0x02, 0x82) },
            { ("FLPT_WFN", new OperandType[1] { OperandType.Pointer }), new Opcode(0x02, 0x83) },

            // Conversions
            // FLPT_EXH (Extend Half Precision Float to Double Precision Float)
            { ("FLPT_EXH", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x90) },
            // FLPT_EXS (Extend Single Precision Float to Double Precision Float)
            { ("FLPT_EXS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x91) },
            // FLPT_SHS (Shrink Double Precision Float to Single Precision Float)
            { ("FLPT_SHS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x92) },
            // FLPT_SHH (Shrink Double Precision Float to Half Precision Float)
            { ("FLPT_SHH", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0x93) },

            // FLPT_NEG (Floating Point Negation)
            { ("FLPT_NEG", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xA0) },

            // FLPT_UTF (Convert Unsigned Quad Word to Double Precision Float)
            { ("FLPT_UTF", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xB0) },
            // FLPT_STF (Convert Signed Quad Word to Double Precision Float)
            { ("FLPT_STF", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xB1) },

            // FLPT_FTS (Convert Double Precision Float to Signed Quad Word through Truncation)
            { ("FLPT_FTS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xC0) },
            // FLPT_FCS (Convert Double Precision Float to Signed Quad Word through Ceiling Rounding)
            { ("FLPT_FCS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xC1) },
            // FLPT_FFS (Convert Double Precision Float to Signed Quad Word through Floor Rounding)
            { ("FLPT_FFS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xC2) },
            // FLPT_FNS (Convert Double Precision Float to Signed Quad Word through Nearest Rounding)
            { ("FLPT_FNS", new OperandType[1] { OperandType.Register }), new Opcode(0x02, 0xC3) },

            // FLPT_CMP (Compare [Subtract Then Discard Result])
            { ("FLPT_CMP", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x02, 0xD0) },  // (reg - reg)
            { ("FLPT_CMP", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x02, 0xD1) },  // (reg - lit)
            { ("FLPT_CMP", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x02, 0xD2) },  // (reg - adr)
            { ("FLPT_CMP", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x02, 0xD3) },  // (reg - ptr)
#endif

#if EXTENSION_SET_EXTENDED_BASE
            // EXTENDED BASE SET

            // EXTD_BSW (Reverse Byte Order)
            { ("EXTD_BSW", new OperandType[1] { OperandType.Register }), new Opcode(0x03, 0x00) },

            // EXTD_QPF (Query Present Features)
            { ("EXTD_QPF", new OperandType[1] { OperandType.Register }), new Opcode(0x03, 0x10) },

            // EXTD_QPV (Query Present Version)
            { ("EXTD_QPV", new OperandType[1] { OperandType.Register }), new Opcode(0x03, 0x11) },
            { ("EXTD_QPV", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x03, 0x12) },

            // EXTD_CSS (Call Stack Size)
            { ("EXTD_CSS", new OperandType[1] { OperandType.Register }), new Opcode(0x03, 0x13) },

            // EXTD_HLT (Halt With Exit Code)
            { ("EXTD_HLT", new OperandType[1] { OperandType.Register }), new Opcode(0x03, 0x20) },
            { ("EXTD_HLT", new OperandType[1] { OperandType.Literal }), new Opcode(0x03, 0x21) },
            { ("EXTD_HLT", new OperandType[1] { OperandType.Address }), new Opcode(0x03, 0x22) },
            { ("EXTD_HLT", new OperandType[1] { OperandType.Pointer }), new Opcode(0x03, 0x23) },

            // EXTD_MPA (Move Pointer Address)
            { ("EXTD_MPA", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x03, 0x30) },
            { ("EXTD_MPA", new OperandType[2] { OperandType.Address, OperandType.Pointer }), new Opcode(0x03, 0x31) },
            { ("EXTD_MPA", new OperandType[2] { OperandType.Pointer, OperandType.Pointer }), new Opcode(0x03, 0x32) },
#endif

#if EXTENSION_SET_EXTERNAL_ASM
            // EXTERNAL ASSEMBLY EXTENSION SET

            // ASMX_LDA (Load Assembly)
            { ("ASMX_LDA", new OperandType[1] { OperandType.Address }), new Opcode(0x04, 0x00) },
            { ("ASMX_LDA", new OperandType[1] { OperandType.Pointer }), new Opcode(0x04, 0x01) },

            // ASMX_LDF (Load Function)
            { ("ASMX_LDF", new OperandType[1] { OperandType.Address }), new Opcode(0x04, 0x02) },
            { ("ASMX_LDF", new OperandType[1] { OperandType.Pointer }), new Opcode(0x04, 0x03) },

            // ASMX_CLA (Close Assembly)
            { ("ASMX_CLA", Array.Empty<OperandType>()), new Opcode(0x04, 0x10) },

            // ASMX_CLF (Close Function)
            { ("ASMX_CLF", Array.Empty<OperandType>()), new Opcode(0x04, 0x11) },

            // ASMX_AEX (Does Assembly Exist and Is It Valid?)
            { ("ASMX_AEX", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x04, 0x20) },
            { ("ASMX_AEX", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x04, 0x21) },

            // ASMX_FEX (Does Function Exist and Is It Valid?)
            { ("ASMX_FEX", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x04, 0x22) },
            { ("ASMX_FEX", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x04, 0x23) },

            // ASMX_CAL (Call Loaded External Function)
            { ("ASMX_CAL", Array.Empty<OperandType>()), new Opcode(0x04, 0x30) },
            { ("ASMX_CAL", new OperandType[1] { OperandType.Register }), new Opcode(0x04, 0x31) },
            { ("ASMX_CAL", new OperandType[1] { OperandType.Literal }), new Opcode(0x04, 0x32) },
            { ("ASMX_CAL", new OperandType[1] { OperandType.Address }), new Opcode(0x04, 0x33) },
            { ("ASMX_CAL", new OperandType[1] { OperandType.Pointer }), new Opcode(0x04, 0x34) },
#endif

#if EXTENSION_SET_HEAP_ALLOCATE
            // MEMORY ALLOCATION EXTENSION SET

            // HEAP_ALC (Allocate Heap Memory - Error if Fail)
            { ("HEAP_ALC", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x05, 0x00) },
            { ("HEAP_ALC", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x05, 0x01) },
            { ("HEAP_ALC", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x05, 0x02) },
            { ("HEAP_ALC", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x05, 0x03) },

            // HEAP_TRY (Allocate Heap Memory - Return -1 if Fail)
            { ("HEAP_TRY", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x05, 0x04) },
            { ("HEAP_TRY", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x05, 0x05) },
            { ("HEAP_TRY", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x05, 0x06) },
            { ("HEAP_TRY", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x05, 0x07) },

            // HEAP_REA (Re-allocate Heap Memory - Error if Fail)
            { ("HEAP_REA", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x05, 0x10) },
            { ("HEAP_REA", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x05, 0x11) },
            { ("HEAP_REA", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x05, 0x12) },
            { ("HEAP_REA", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x05, 0x13) },

            // HEAP_TRE (Re-allocate Heap Memory - Return -1 if Fail)
            { ("HEAP_TRE", new OperandType[2] { OperandType.Register, OperandType.Register }), new Opcode(0x05, 0x14) },
            { ("HEAP_TRE", new OperandType[2] { OperandType.Register, OperandType.Literal }), new Opcode(0x05, 0x15) },
            { ("HEAP_TRE", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x05, 0x16) },
            { ("HEAP_TRE", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x05, 0x17) },

            // HEAP_FRE (Free Allocated Heap Memory)
            { ("HEAP_FRE", new OperandType[1] { OperandType.Register }), new Opcode(0x05, 0x20) },
#endif

#if EXTENSION_SET_FILE_SYSTEM
            // FILE SYSTEM EXTENSION SET

            // FSYS_CWD (Change Working Directory to Path Specified by 0x00 Terminated String in Memory)
            { ("FSYS_CWD", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x00) },
            { ("FSYS_CWD", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x01) },

            // FSYS_GWD (Get Working Directory)
            { ("FSYS_GWD", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x02) },
            { ("FSYS_GWD", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x03) },

            // FSYS_CDR (Create Directory at Path Specified by 0x00 Terminated String in Memory)
            { ("FSYS_CDR", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x10) },
            { ("FSYS_CDR", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x11) },

            // FSYS_DDR (Delete Directory Recursively at Path Specified by 0x00 Terminated String in Memory)
            { ("FSYS_DDR", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x20) },
            { ("FSYS_DDR", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x21) },

            // FSYS_DDE (Delete Empty Directory at Path Specified by 0x00 Terminated String in Memory)
            { ("FSYS_DDE", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x22) },
            { ("FSYS_DDE", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x23) },

            // FSYS_DEX (Does Directory Exist at Path Specified by 0x00 Terminated String in Memory?)
            { ("FSYS_DEX", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x06, 0x30) },
            { ("FSYS_DEX", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x06, 0x31) },

            // FSYS_CPY (Copy Between Paths Specified by 0x00 Terminated Strings in Memory)
            { ("FSYS_CPY", new OperandType[2] { OperandType.Address, OperandType.Address }), new Opcode(0x06, 0x40) },
            { ("FSYS_CPY", new OperandType[2] { OperandType.Address, OperandType.Pointer }), new Opcode(0x06, 0x41) },
            { ("FSYS_CPY", new OperandType[2] { OperandType.Pointer, OperandType.Address }), new Opcode(0x06, 0x42) },
            { ("FSYS_CPY", new OperandType[2] { OperandType.Pointer, OperandType.Pointer }), new Opcode(0x06, 0x43) },

            // FSYS_MOV (Move Between Paths Specified by 0x00 Terminated Strings in Memory)
            { ("FSYS_MOV", new OperandType[2] { OperandType.Address, OperandType.Address }), new Opcode(0x06, 0x44) },
            { ("FSYS_MOV", new OperandType[2] { OperandType.Address, OperandType.Pointer }), new Opcode(0x06, 0x45) },
            { ("FSYS_MOV", new OperandType[2] { OperandType.Pointer, OperandType.Address }), new Opcode(0x06, 0x46) },
            { ("FSYS_MOV", new OperandType[2] { OperandType.Pointer, OperandType.Pointer }), new Opcode(0x06, 0x47) },

            // FSYS_BDL (Begin Directory Listing of Current Directory or Directory at Path Specified by 0x00 Terminated String in Memory)
            { ("FSYS_BDL", Array.Empty<OperandType>()), new Opcode(0x06, 0x50) },
            { ("FSYS_BDL", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x51) },
            { ("FSYS_BDL", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x52) },

            // FSYS_GNF (Get Next File in Currently Listed Directory)
            { ("FSYS_GNF", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x60) },
            { ("FSYS_GNF", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x61) },

            // FSYS_GND (Get Next Directory in Currently Listed Directory)
            { ("FSYS_GND", new OperandType[1] { OperandType.Address }), new Opcode(0x06, 0x62) },
            { ("FSYS_GND", new OperandType[1] { OperandType.Pointer }), new Opcode(0x06, 0x63) },

            // FSYS_GCT (Get Creation Time of File or Directory)
            { ("FSYS_GCT", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x06, 0x70) },
            { ("FSYS_GCT", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x06, 0x71) },

            // FSYS_GMT (Get Modification Time of File or Directory)
            { ("FSYS_GMT", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x06, 0x72) },
            { ("FSYS_GMT", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x06, 0x73) },

            // FSYS_GAT (Get Access Time of File or Directory)
            { ("FSYS_GAT", new OperandType[2] { OperandType.Register, OperandType.Address }), new Opcode(0x06, 0x74) },
            { ("FSYS_GAT", new OperandType[2] { OperandType.Register, OperandType.Pointer }), new Opcode(0x06, 0x75) },

            // FSYS_SCT (Set Creation Time of File or Directory)
            { ("FSYS_SCT", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x06, 0x80) },
            { ("FSYS_SCT", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x06, 0x81) },
            { ("FSYS_SCT", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x06, 0x82) },
            { ("FSYS_SCT", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x06, 0x83) },

            // FSYS_SMT (Set Modification Time of File or Directory)
            { ("FSYS_SMT", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x06, 0x84) },
            { ("FSYS_SMT", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x06, 0x85) },
            { ("FSYS_SMT", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x06, 0x86) },
            { ("FSYS_SMT", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x06, 0x87) },

            // FSYS_SAT (Set Access Time of File or Directory)
            { ("FSYS_SAT", new OperandType[2] { OperandType.Address, OperandType.Register }), new Opcode(0x06, 0x88) },
            { ("FSYS_SAT", new OperandType[2] { OperandType.Address, OperandType.Literal }), new Opcode(0x06, 0x89) },
            { ("FSYS_SAT", new OperandType[2] { OperandType.Pointer, OperandType.Register }), new Opcode(0x06, 0x8A) },
            { ("FSYS_SAT", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), new Opcode(0x06, 0x8B) },
#endif

#if EXTENSION_SET_TERMINAL
            // TERMINAL EXTENSION SET

            // TERM_CLS (Clear Screen)
            { ("TERM_CLS", Array.Empty<OperandType>()), new Opcode(0x07, 0x00) },

            // TERM_AEE (Auto Echo Enable)
            { ("TERM_AEE", Array.Empty<OperandType>()), new Opcode(0x07, 0x10) },

            // TERM_AED (Auto Echo Disable)
            { ("TERM_AED", Array.Empty<OperandType>()), new Opcode(0x07, 0x11) },

            // TERM_SCY (Set Vertical Cursor Position)
            { ("TERM_SCY", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x20) },
            { ("TERM_SCY", new OperandType[1] { OperandType.Literal }), new Opcode(0x07, 0x21) },
            { ("TERM_SCY", new OperandType[1] { OperandType.Address }), new Opcode(0x07, 0x22) },
            { ("TERM_SCY", new OperandType[1] { OperandType.Pointer }), new Opcode(0x07, 0x23) },

            // TERM_SCX (Set Horizontal Cursor Position)
            { ("TERM_SCX", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x24) },
            { ("TERM_SCX", new OperandType[1] { OperandType.Literal }), new Opcode(0x07, 0x25) },
            { ("TERM_SCX", new OperandType[1] { OperandType.Address }), new Opcode(0x07, 0x26) },
            { ("TERM_SCX", new OperandType[1] { OperandType.Pointer }), new Opcode(0x07, 0x27) },

            // TERM_GCY (Get Vertical Cursor Position)
            { ("TERM_GCY", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x30) },

            // TERM_GCX (Get Horizontal Cursor Position)
            { ("TERM_GCX", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x31) },

            // TERM_GSY (Get Terminal Buffer Height)
            { ("TERM_GSY", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x32) },

            // TERM_GSX (Get Terminal Buffer Width)
            { ("TERM_GSX", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x33) },

            // TERM_BEP (Beep)
            { ("TERM_BEP", Array.Empty<OperandType>()), new Opcode(0x07, 0x40) },

            // TERM_SFC (Set Foreground Color)
            { ("TERM_SFC", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x50) },
            { ("TERM_SFC", new OperandType[1] { OperandType.Literal }), new Opcode(0x07, 0x51) },
            { ("TERM_SFC", new OperandType[1] { OperandType.Address }), new Opcode(0x07, 0x52) },
            { ("TERM_SFC", new OperandType[1] { OperandType.Pointer }), new Opcode(0x07, 0x53) },

            // TERM_SBC (Set Background Color)
            { ("TERM_SBC", new OperandType[1] { OperandType.Register }), new Opcode(0x07, 0x54) },
            { ("TERM_SBC", new OperandType[1] { OperandType.Literal }), new Opcode(0x07, 0x55) },
            { ("TERM_SBC", new OperandType[1] { OperandType.Address }), new Opcode(0x07, 0x56) },
            { ("TERM_SBC", new OperandType[1] { OperandType.Pointer }), new Opcode(0x07, 0x57) },

            // TERM_RSC (Reset Color)
            { ("TERM_RSC", Array.Empty<OperandType>()), new Opcode(0x07, 0x58) },
#endif
        };

        /// <summary>
        /// A mapping of what mnemonic and operand types an opcode corresponds to.
        /// </summary>
        public static readonly Dictionary<Opcode, (string Mnemonic, OperandType[] OperandTypes)> MnemonicsReverse =
            Mnemonics.DistinctBy(kv => kv.Value).ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public class MnemonicComparer : EqualityComparer<(string Mnemonic, OperandType[] OperandTypes)>
    {
        public override bool Equals((string Mnemonic, OperandType[] OperandTypes) first, (string Mnemonic, OperandType[] OperandTypes) second)
        {
            return first.Mnemonic.Equals(second.Mnemonic, StringComparison.OrdinalIgnoreCase) && first.OperandTypes.SequenceEqual(second.OperandTypes);
        }

        public override int GetHashCode((string Mnemonic, OperandType[] OperandTypes) obj)
        {
            HashCode hashCode = new();
            hashCode.Add(obj.Mnemonic, StringComparer.OrdinalIgnoreCase);
            foreach (OperandType type in obj.OperandTypes)
            {
                hashCode.Add(type);
            }
            return hashCode.ToHashCode();
        }
    }
}
