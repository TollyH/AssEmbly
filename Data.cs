namespace AssEmbly
{
    /// <summary>
    /// Stores <see langword="static"/> data about AssEmbly, such as register and operand types, and mnemonic mappings.
    /// </summary>
    public static class Data
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
            rsf,  // Status Flags (Zero Flag, Carry Flag, File End Flag, 61 remaining high bits undefined)
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
        public enum StatusFlags
        {
            Zero = 0b1,
            Carry = 0b10,
            FileEnd = 0b100,

            ZeroAndCarry = Zero | Carry,
        }

        /// <summary>
        /// A mapping of what byte a mnemonic with a particular set of operands should compile to.
        /// </summary>
        public static readonly Dictionary<(string Mnemonic, OperandType[] OperandTypes), byte> Mnemonics = new(new MnemonicComparer())
        {
            // Control
            // HLT (Halt)
            { ("HLT", Array.Empty<OperandType>()), 0x00 },
            // NOP (No Operation)
            { ("NOP", Array.Empty<OperandType>()), 0x01 },

            // Jump
            // JMP (Unconditional Jump)
            { ("JMP", new OperandType[1] { OperandType.Address }), 0x02 },
            { ("JMP", new OperandType[1] { OperandType.Pointer }), 0x03 },
            // JEQ (Jump If Equal To - Zero Flag Set)
            { ("JEQ", new OperandType[1] { OperandType.Address }), 0x04 },
            { ("JZO", new OperandType[1] { OperandType.Address }), 0x04 },
            { ("JEQ", new OperandType[1] { OperandType.Pointer }), 0x05 },
            { ("JZO", new OperandType[1] { OperandType.Pointer }), 0x05 },
            // JNE (Jump If Not Equal To - Zero Flag Unset)
            { ("JNE", new OperandType[1] { OperandType.Address }), 0x06 },
            { ("JNZ", new OperandType[1] { OperandType.Address }), 0x06 },
            { ("JNE", new OperandType[1] { OperandType.Pointer }), 0x07 },
            { ("JNZ", new OperandType[1] { OperandType.Pointer }), 0x07 },
            // JLT (Jump If Less Than - Carry Flag Set)
            { ("JLT", new OperandType[1] { OperandType.Address }), 0x08 },
            { ("JCA", new OperandType[1] { OperandType.Address }), 0x08 },
            { ("JLT", new OperandType[1] { OperandType.Pointer }), 0x09 },
            { ("JCA", new OperandType[1] { OperandType.Pointer }), 0x09 },
            // JLE (Jump If Less Than or Equal To - Carry Flag Set or Zero Flag Set)
            { ("JLE", new OperandType[1] { OperandType.Address }), 0x0A },
            { ("JLE", new OperandType[1] { OperandType.Pointer }), 0x0B },
            // JGT (Jump If Greater Than - Carry Flag Unset and Zero Flag Unset)
            { ("JGT", new OperandType[1] { OperandType.Address }), 0x0C },
            { ("JGT", new OperandType[1] { OperandType.Pointer }), 0x0D },
            // JGE (Jump If Greater Than or Equal To - Carry Flag Unset)
            { ("JGE", new OperandType[1] { OperandType.Address }), 0x0E },
            { ("JNC", new OperandType[1] { OperandType.Address }), 0x0E },
            { ("JGE", new OperandType[1] { OperandType.Pointer }), 0x0F },
            { ("JNC", new OperandType[1] { OperandType.Pointer }), 0x0F },


            // Math (All operations store result in the first register operand)
            // ADD (Addition)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x10 },  // (reg + reg)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x11 },  // (reg + lit)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x12 },  // (reg + adr)
            { ("ADD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x13 },  // (reg + ptr)

            // ICR (Increment)
            { ("ICR", new OperandType[1] { OperandType.Register }), 0x14 },

            // SUB (Subtraction)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x20 },  // (reg - reg)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x21 },  // (reg - lit)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x22 },  // (reg - adr)
            { ("SUB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x23 },  // (reg - ptr)

            // DCR (Decrement)
            { ("DCR", new OperandType[1] { OperandType.Register }), 0x24 },

            // MUL (Multiplication)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x30 },  // (reg * reg)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x31 },  // (reg * lit)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x32 },  // (reg * adr)
            { ("MUL", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x33 },  // (reg * ptr)

            // DIV (Integer Division)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x40 },  // (reg / reg)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x41 },  // (reg / lit)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x42 },  // (reg / adr)
            { ("DIV", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x43 },  // (reg / ptr)

            // DVR (Integer Division With Remainder [Remainder Stored in Second Register])
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Register }), 0x44 },  // (reg / reg)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Literal }), 0x45 },  // (reg / lit)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Address }), 0x46 },  // (reg / adr)
            { ("DVR", new OperandType[3] { OperandType.Register, OperandType.Register, OperandType.Pointer }), 0x47 },  // (reg / ptr)

            // REM (Remainder)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x48 },  // (reg % reg)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x49 },  // (reg % lit)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x4A },  // (reg % adr)
            { ("REM", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x4B },  // (reg % ptr)

            // SHL (Shift Left)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x50 },  // (reg << reg)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x51 },  // (reg << lit)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x52 },  // (reg << adr)
            { ("SHL", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x53 },  // (reg << ptr)

            // SHR (Shift Right)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x54 },  // (reg >> reg)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x55 },  // (reg >> lit)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x56 },  // (reg >> adr)
            { ("SHR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x57 },  // (reg >> ptr)

            // Bitwise (All operations store result in the first register operand)
            // AND
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x60 },  // (reg & reg)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x61 },  // (reg & lit)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x62 },  // (reg & adr)
            { ("AND", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x63 },  // (reg & ptr)

            // ORR
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x64 },  // (reg | reg)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x65 },  // (reg | lit)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x66 },  // (reg | adr)
            { ("ORR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x67 },  // (reg | ptr)

            // XOR
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x68 },  // (reg ^ reg)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x69 },  // (reg ^ lit)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x6A },  // (reg ^ adr)
            { ("XOR", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x6B },  // (reg ^ ptr)

            // NOT
            { ("NOT", new OperandType[1] { OperandType.Register }), 0x6C },

            // RNG
            { ("RNG", new OperandType[1] { OperandType.Register }), 0x6D },

            // Comparison (Results will be discarded, but flags will still be set - best used in conjunction with conditional jumps)
            // TST (Test [Bitwise And Then Discard Result])
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x70 },  // (reg & reg)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x71 },  // (reg & lit)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x72 },  // (reg & adr)
            { ("TST", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x73 },  // (reg & ptr)

            // CMP (Compare [Subtract Then Discard Result])
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x74 },  // (reg - reg)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x75 },  // (reg - lit)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x76 },  // (reg - adr)
            { ("CMP", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x77 },  // (reg - ptr)

            // Data
            // MVB (Move Byte [8 bits])
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x80 },  // (reg <- reg)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x81 },  // (reg <- lit)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x82 },  // (reg <- adr)
            { ("MVB", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x83 },  // (reg <- ptr)
            { ("MVB", new OperandType[2] { OperandType.Address, OperandType.Register }), 0x84 },  // (adr <- reg)
            { ("MVB", new OperandType[2] { OperandType.Address, OperandType.Literal }), 0x85 },  // (adr <- lit)
            { ("MVB", new OperandType[2] { OperandType.Pointer, OperandType.Register }), 0x86 },  // (ptr <- reg)
            { ("MVB", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), 0x87 },  // (ptr <- lit)

            // MVW (Move Word [16 bits - 2 bytes])
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x88 },  // (reg <- reg)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x89 },  // (reg <- lit)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x8A },  // (reg <- adr)
            { ("MVW", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x8B },  // (reg <- ptr)
            { ("MVW", new OperandType[2] { OperandType.Address, OperandType.Register }), 0x8C },  // (adr <- reg)
            { ("MVW", new OperandType[2] { OperandType.Address, OperandType.Literal }), 0x8D },  // (adr <- lit)
            { ("MVW", new OperandType[2] { OperandType.Pointer, OperandType.Register }), 0x8E },  // (ptr <- reg)
            { ("MVW", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), 0x8F },  // (ptr <- lit)

            // MVD (Move Double Word [32 bits - 4 bytes])
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x90 },  // (reg <- reg)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x91 },  // (reg <- lit)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x92 },  // (reg <- adr)
            { ("MVD", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x93 },  // (reg <- ptr)
            { ("MVD", new OperandType[2] { OperandType.Address, OperandType.Register }), 0x94 },  // (adr <- reg)
            { ("MVD", new OperandType[2] { OperandType.Address, OperandType.Literal }), 0x95 },  // (adr <- lit)
            { ("MVD", new OperandType[2] { OperandType.Pointer, OperandType.Register }), 0x96 },  // (ptr <- reg)
            { ("MVD", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), 0x97 },  // (ptr <- lit)

            // MVQ (Move Quad Word [64 bits - 8 bytes])
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Register }), 0x98 },  // (reg <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Literal }), 0x99 },  // (reg <- lit)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Address }), 0x9A },  // (reg <- adr)
            { ("MVQ", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0x9B },  // (reg <- ptr)
            { ("MVQ", new OperandType[2] { OperandType.Address, OperandType.Register }), 0x9C },  // (adr <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Address, OperandType.Literal }), 0x9D },  // (adr <- lit)
            { ("MVQ", new OperandType[2] { OperandType.Pointer, OperandType.Register }), 0x9E },  // (ptr <- reg)
            { ("MVQ", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), 0x9F },  // (ptr <- lit)

            // Stack and Subroutines [All Stack Operations are 64-bit]
            // PSH (Push Value to Stack)
            { ("PSH", new OperandType[1] { OperandType.Register }), 0xA0 },  // reg ->
            { ("PSH", new OperandType[1] { OperandType.Literal }), 0xA1 },  // lit ->
            { ("PSH", new OperandType[1] { OperandType.Address }), 0xA2 },  // adr ->
            { ("PSH", new OperandType[1] { OperandType.Pointer }), 0xA3 },  // ptr ->

            // POP (Pop Value from Stack)
            { ("POP", new OperandType[1] { OperandType.Register }), 0xA4 },  // reg <-

            // CAL (Call Subroutine at Address/Pointer, Pushing rpo and rsb to the stack)
            { ("CAL", new OperandType[1] { OperandType.Address }), 0xB0 },
            { ("CAL", new OperandType[1] { OperandType.Pointer }), 0xB1 },

            // CAL (Call Subroutine at Address/Pointer, Pushing rpo and rsb to the stack, storing second operand in rfp)
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Register }), 0xB2 },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Literal }), 0xB3 },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Address }), 0xB4 },
            { ("CAL", new OperandType[2] { OperandType.Address, OperandType.Pointer }), 0xB5 },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Register }), 0xB6 },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Literal }), 0xB7 },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Address }), 0xB8 },
            { ("CAL", new OperandType[2] { OperandType.Pointer, OperandType.Pointer }), 0xB9 },

            // RET (Return from Subroutine, Restoring rsb and rpo from the stack)
            { ("RET", Array.Empty<OperandType>()), 0xBA },

            // RET (Return from Subroutine, Restoring rsb and rpo from the stack, storing operand in rrv)
            { ("RET", new OperandType[1] { OperandType.Register }), 0xBB },
            { ("RET", new OperandType[1] { OperandType.Literal }), 0xBC },
            { ("RET", new OperandType[1] { OperandType.Address }), 0xBD },
            { ("RET", new OperandType[1] { OperandType.Pointer }), 0xBE },

            // Console Write
            // WCN (Write 64-bit Number to Console as Decimal)
            { ("WCN", new OperandType[1] { OperandType.Register }), 0xC0 },
            { ("WCN", new OperandType[1] { OperandType.Literal }), 0xC1 },
            { ("WCN", new OperandType[1] { OperandType.Address }), 0xC2 },
            { ("WCN", new OperandType[1] { OperandType.Pointer }), 0xC3 },

            // WCB (Write Byte to Console as Decimal)
            { ("WCB", new OperandType[1] { OperandType.Register }), 0xC4 },
            { ("WCB", new OperandType[1] { OperandType.Literal }), 0xC5 },
            { ("WCB", new OperandType[1] { OperandType.Address }), 0xC6 },
            { ("WCB", new OperandType[1] { OperandType.Pointer }), 0xC7 },

            // WCX (Write Byte to Console as Hex)
            { ("WCX", new OperandType[1] { OperandType.Register }), 0xC8 },
            { ("WCX", new OperandType[1] { OperandType.Literal }), 0xC9 },
            { ("WCX", new OperandType[1] { OperandType.Address }), 0xCA },
            { ("WCX", new OperandType[1] { OperandType.Pointer }), 0xCB },

            // WCC (Write Raw Byte to Console)
            { ("WCC", new OperandType[1] { OperandType.Register }), 0xCC },
            { ("WCC", new OperandType[1] { OperandType.Literal }), 0xCD },
            { ("WCC", new OperandType[1] { OperandType.Address }), 0xCE },
            { ("WCC", new OperandType[1] { OperandType.Pointer }), 0xCF },

            // File Write
            // WFN (Write 64-bit Number to Open File as Decimal)
            { ("WFN", new OperandType[1] { OperandType.Register }), 0xD0 },
            { ("WFN", new OperandType[1] { OperandType.Literal }), 0xD1 },
            { ("WFN", new OperandType[1] { OperandType.Address }), 0xD2 },
            { ("WFN", new OperandType[1] { OperandType.Pointer }), 0xD3 },

            // WFB (Write Byte to Open File as Decimal)
            { ("WFB", new OperandType[1] { OperandType.Register }), 0xD4 },
            { ("WFB", new OperandType[1] { OperandType.Literal }), 0xD5 },
            { ("WFB", new OperandType[1] { OperandType.Address }), 0xD6 },
            { ("WFB", new OperandType[1] { OperandType.Pointer }), 0xD7 },

            // WFX (Write Byte to Open File as Hex)
            { ("WFX", new OperandType[1] { OperandType.Register }), 0xD8 },
            { ("WFX", new OperandType[1] { OperandType.Literal }), 0xD9 },
            { ("WFX", new OperandType[1] { OperandType.Address }), 0xDA },
            { ("WFX", new OperandType[1] { OperandType.Pointer }), 0xDB },

            // WFC (Write Raw Byte to Open File)
            { ("WFC", new OperandType[1] { OperandType.Register }), 0xDC },
            { ("WFC", new OperandType[1] { OperandType.Literal }), 0xDD },
            { ("WFC", new OperandType[1] { OperandType.Address }), 0xDE },
            { ("WFC", new OperandType[1] { OperandType.Pointer }), 0xDF },

            // OFL (Open File at Path Specified by 0x00 Terminated String in Memory)
            { ("OFL", new OperandType[1] { OperandType.Address }), 0xE0 },
            { ("OFL", new OperandType[1] { OperandType.Pointer }), 0xE1 },

            // CFL (Close Currently Open File)
            { ("CFL", Array.Empty<OperandType>()), 0xE2 },

            // DFL (Delete File at Path Specified by 0x00 Terminated String in Memory)
            { ("DFL", new OperandType[1] { OperandType.Address }), 0xE3 },
            { ("DFL", new OperandType[1] { OperandType.Pointer }), 0xE4 },

            // FEX (Does File Exist at Path Specified by 0x00 Terminated String in Memory?)
            { ("FEX", new OperandType[2] { OperandType.Register, OperandType.Address }), 0xE5 },
            { ("FEX", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0xE6 },

            // FSZ (Get Size of File at Path Specified by 0x00 Terminated String in Memory)
            { ("FSZ", new OperandType[2] { OperandType.Register, OperandType.Address }), 0xE7 },
            { ("FSZ", new OperandType[2] { OperandType.Register, OperandType.Pointer }), 0xE8 },

            // Reading
            // RCC (Read Character from Console as a Byte)
            { ("RCC", new OperandType[1] { OperandType.Register }), 0xF0 },

            // RFC (Read Character from Open File as a Byte)
            { ("RFC", new OperandType[1] { OperandType.Register }), 0xF1 },
        };
    }
}
