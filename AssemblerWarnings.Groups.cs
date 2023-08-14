namespace AssEmbly
{
    public partial class AssemblerWarnings
    {
        /// <summary>
        /// A dictionary of opcodes to an array of the 0-based indices of all operands to that opcode that are written to.
        /// Only opcodes that result in at least one operand being written to are included.
        /// </summary>
        internal static readonly Dictionary<byte, int[]> writingInstructions = new()
        {
            { 0x10, new int[] { 0 } },  // ADD reg, reg
            { 0x11, new int[] { 0 } },  // ADD reg, lit
            { 0x12, new int[] { 0 } },  // ADD reg, adr
            { 0x13, new int[] { 0 } },  // ADD reg, ptr
            { 0x14, new int[] { 0 } },  // ICR reg

            { 0x20, new int[] { 0 } },  // SUB reg, reg
            { 0x21, new int[] { 0 } },  // SUB reg, lit
            { 0x22, new int[] { 0 } },  // SUB reg, adr
            { 0x23, new int[] { 0 } },  // SUB reg, ptr
            { 0x24, new int[] { 0 } },  // DCR reg

            { 0x30, new int[] { 0 } },  // MUL reg, reg
            { 0x31, new int[] { 0 } },  // MUL reg, lit
            { 0x32, new int[] { 0 } },  // MUL reg, adr
            { 0x33, new int[] { 0 } },  // MUL reg, ptr

            { 0x40, new int[] { 0 } },  // DIV reg, reg
            { 0x41, new int[] { 0 } },  // DIV reg, lit
            { 0x42, new int[] { 0 } },  // DIV reg, adr
            { 0x43, new int[] { 0 } },  // DIV reg, ptr
            { 0x44, new int[] { 0, 1 } },  // DVR reg, reg, reg
            { 0x45, new int[] { 0, 1 } },  // DVR reg, reg, lit
            { 0x46, new int[] { 0, 1 } },  // DVR reg, reg, adr
            { 0x47, new int[] { 0, 1 } },  // DVR reg, reg, ptr
            { 0x48, new int[] { 0 } },  // REM reg, reg
            { 0x49, new int[] { 0 } },  // REM reg, lit
            { 0x4A, new int[] { 0 } },  // REM reg, adr
            { 0x4B, new int[] { 0 } },  // REM reg, ptr

            { 0x50, new int[] { 0 } },  // SHL reg, reg
            { 0x51, new int[] { 0 } },  // SHL reg, lit
            { 0x52, new int[] { 0 } },  // SHL reg, adr
            { 0x53, new int[] { 0 } },  // SHL reg, ptr
            { 0x54, new int[] { 0 } },  // SHR reg, reg
            { 0x55, new int[] { 0 } },  // SHR reg, lit
            { 0x56, new int[] { 0 } },  // SHR reg, adr
            { 0x57, new int[] { 0 } },  // SHR reg, ptr

            { 0x60, new int[] { 0 } },  // AND reg, reg
            { 0x61, new int[] { 0 } },  // AND reg, lit
            { 0x62, new int[] { 0 } },  // AND reg, adr
            { 0x63, new int[] { 0 } },  // AND reg, ptr
            { 0x64, new int[] { 0 } },  // ORR reg, reg
            { 0x65, new int[] { 0 } },  // ORR reg, lit
            { 0x66, new int[] { 0 } },  // ORR reg, adr
            { 0x67, new int[] { 0 } },  // ORR reg, ptr
            { 0x68, new int[] { 0 } },  // XOR reg, reg
            { 0x69, new int[] { 0 } },  // XOR reg, lit
            { 0x6A, new int[] { 0 } },  // XOR reg, adr
            { 0x6B, new int[] { 0 } },  // XOR reg, ptr
            { 0x6C, new int[] { 0 } },  // NOT reg
            { 0x6D, new int[] { 0 } },  // RNG reg

            { 0x80, new int[] { 0 } },  // MVB reg, reg
            { 0x81, new int[] { 0 } },  // MVB reg, lit
            { 0x82, new int[] { 0 } },  // MVB reg, adr
            { 0x83, new int[] { 0 } },  // MVB reg, ptr
            { 0x84, new int[] { 0 } },  // MVB adr, reg
            { 0x85, new int[] { 0 } },  // MVB adr, lit
            { 0x86, new int[] { 0 } },  // MVB ptr, reg
            { 0x87, new int[] { 0 } },  // MVB ptr, lit
            { 0x88, new int[] { 0 } },  // MVW reg, reg
            { 0x89, new int[] { 0 } },  // MVW reg, lit
            { 0x8A, new int[] { 0 } },  // MVW reg, adr
            { 0x8B, new int[] { 0 } },  // MVW reg, ptr
            { 0x8C, new int[] { 0 } },  // MVW adr, reg
            { 0x8D, new int[] { 0 } },  // MVW adr, lit
            { 0x8E, new int[] { 0 } },  // MVW ptr, reg
            { 0x8F, new int[] { 0 } },  // MVW ptr, lit

            { 0x90, new int[] { 0 } },  // MVD reg, reg
            { 0x91, new int[] { 0 } },  // MVD reg, lit
            { 0x92, new int[] { 0 } },  // MVD reg, adr
            { 0x93, new int[] { 0 } },  // MVD reg, ptr
            { 0x94, new int[] { 0 } },  // MVD adr, reg
            { 0x95, new int[] { 0 } },  // MVD adr, lit
            { 0x96, new int[] { 0 } },  // MVD ptr, reg
            { 0x97, new int[] { 0 } },  // MVD ptr, lit
            { 0x98, new int[] { 0 } },  // MVQ reg, reg
            { 0x99, new int[] { 0 } },  // MVQ reg, lit
            { 0x9A, new int[] { 0 } },  // MVQ reg, adr
            { 0x9B, new int[] { 0 } },  // MVQ reg, ptr
            { 0x9C, new int[] { 0 } },  // MVQ adr, reg
            { 0x9D, new int[] { 0 } },  // MVQ adr, lit
            { 0x9E, new int[] { 0 } },  // MVQ ptr, reg
            { 0x9F, new int[] { 0 } },  // MVQ ptr, lit

            { 0xA4, new int[] { 0 } },  // POP reg

            { 0xE5, new int[] { 0 } },  // FEX reg, adr
            { 0xE6, new int[] { 0 } },  // FEX reg, ptr
            { 0xE7, new int[] { 0 } },  // FSZ reg, adr
            { 0xE8, new int[] { 0 } },  // FSZ reg, ptr

            { 0xF0, new int[] { 0 } },  // RCC reg
            { 0xF1, new int[] { 0 } },  // RFC reg
        };

        /// <summary>
        /// Directives that result in data (non-code bytes) being inserted into the assembly.
        /// </summary>
        internal static readonly HashSet<string> dataInsertionDirectives = new() { "DAT", "PAD", "NUM" };
        /// <summary>
        /// Every opcode that results in the location of execution being moved to the address of a label.
        /// As of current, the address to jump to is always the first operand to these opcodes.
        /// </summary>
        internal static readonly HashSet<byte> jumpCallToLabelOpcodes = new()
        {
            0x02, 0x04, 0x06, 0x08, 0x0A, 0x0C, 0x0E,  // Jumps
            0xB0, 0xB2, 0xB3, 0xB4, 0xB5  // Calls
        };
        /// <summary>
        /// Any instruction that can be used to prevent execution flowing onwards 100% of the time.
        /// i.e. Unconditional jump, return, halt
        /// </summary>
        internal static readonly HashSet<byte> terminators = new() { 0x00, 0x02, 0x03, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE };

        /// <summary>
        /// All opcodes that move a literal value into a register. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<byte> moveRegLit = new() { 0x81, 0x89, 0x91, 0x99 };

        /// <summary>
        /// All opcodes that move a literal value. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<byte> moveLiteral = new() { 0x81, 0x85, 0x87, 0x89, 0x8D, 0x8F, 0x91, 0x95, 0x97, 0x99, 0x9D, 0x9F };
        /// <summary>
        /// All opcodes that shift the bits of a register by a literal value. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<byte> shiftByLiteral = new() { 0x51, 0x55 };
        /// <summary>
        /// All opcodes that perform a division by a literal value. The mapped value is the index of the literal in the operands.
        /// </summary>
        internal static readonly Dictionary<byte, int> divisionByLiteral = new()
        {
            { 0x41, 1 }, { 0x45, 2 }, { 0x49, 1 },
        };

        /// <summary>
        /// The number of bits moved by each movement opcode.
        /// </summary>
        internal static readonly Dictionary<byte, int> moveBitCounts = new()
        {
            // MVB
            { 0x80, 8 },
            { 0x81, 8 },
            { 0x82, 8 },
            { 0x83, 8 },
            { 0x84, 8 },
            { 0x85, 8 },
            { 0x86, 8 },
            { 0x87, 8 },
            // MVW
            { 0x88, 16 },
            { 0x89, 16 },
            { 0x8A, 16 },
            { 0x8B, 16 },
            { 0x8C, 16 },
            { 0x8D, 16 },
            { 0x8E, 16 },
            { 0x8F, 16 },
            // MVD
            { 0x90, 32 },
            { 0x91, 32 },
            { 0x92, 32 },
            { 0x93, 32 },
            { 0x94, 32 },
            { 0x95, 32 },
            { 0x96, 32 },
            { 0x97, 32 },
            // MVQ
            { 0x98, 64 },
            { 0x99, 64 },
            { 0x9A, 64 },
            { 0x9B, 64 },
            { 0x9C, 64 },
            { 0x9D, 64 },
            { 0x9E, 64 },
            { 0x9F, 64 },
        };
    }
}
