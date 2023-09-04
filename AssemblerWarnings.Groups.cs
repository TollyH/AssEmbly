﻿namespace AssEmbly
{
    public partial class AssemblerWarnings
    {
        /// <summary>
        /// A dictionary of opcodes to an array of the 0-based indices of all operands to that opcode that are written to.
        /// Only opcodes that result in at least one operand being written to are included.
        /// </summary>
        internal static readonly Dictionary<Opcode, int[]> writingInstructions = new()
        {
            // Base instruction set

            { new Opcode(0x00, 0x10), new int[] { 0 } },  // ADD reg, reg
            { new Opcode(0x00, 0x11), new int[] { 0 } },  // ADD reg, lit
            { new Opcode(0x00, 0x12), new int[] { 0 } },  // ADD reg, adr
            { new Opcode(0x00, 0x13), new int[] { 0 } },  // ADD reg, ptr
            { new Opcode(0x00, 0x14), new int[] { 0 } },  // ICR reg

            { new Opcode(0x00, 0x20), new int[] { 0 } },  // SUB reg, reg
            { new Opcode(0x00, 0x21), new int[] { 0 } },  // SUB reg, lit
            { new Opcode(0x00, 0x22), new int[] { 0 } },  // SUB reg, adr
            { new Opcode(0x00, 0x23), new int[] { 0 } },  // SUB reg, ptr
            { new Opcode(0x00, 0x24), new int[] { 0 } },  // DCR reg

            { new Opcode(0x00, 0x30), new int[] { 0 } },  // MUL reg, reg
            { new Opcode(0x00, 0x31), new int[] { 0 } },  // MUL reg, lit
            { new Opcode(0x00, 0x32), new int[] { 0 } },  // MUL reg, adr
            { new Opcode(0x00, 0x33), new int[] { 0 } },  // MUL reg, ptr

            { new Opcode(0x00, 0x40), new int[] { 0 } },  // DIV reg, reg
            { new Opcode(0x00, 0x41), new int[] { 0 } },  // DIV reg, lit
            { new Opcode(0x00, 0x42), new int[] { 0 } },  // DIV reg, adr
            { new Opcode(0x00, 0x43), new int[] { 0 } },  // DIV reg, ptr
            { new Opcode(0x00, 0x44), new int[] { 0, 1 } },  // DVR reg, reg, reg
            { new Opcode(0x00, 0x45), new int[] { 0, 1 } },  // DVR reg, reg, lit
            { new Opcode(0x00, 0x46), new int[] { 0, 1 } },  // DVR reg, reg, adr
            { new Opcode(0x00, 0x47), new int[] { 0, 1 } },  // DVR reg, reg, ptr
            { new Opcode(0x00, 0x48), new int[] { 0 } },  // REM reg, reg
            { new Opcode(0x00, 0x49), new int[] { 0 } },  // REM reg, lit
            { new Opcode(0x00, 0x4A), new int[] { 0 } },  // REM reg, adr
            { new Opcode(0x00, 0x4B), new int[] { 0 } },  // REM reg, ptr

            { new Opcode(0x00, 0x50), new int[] { 0 } },  // SHL reg, reg
            { new Opcode(0x00, 0x51), new int[] { 0 } },  // SHL reg, lit
            { new Opcode(0x00, 0x52), new int[] { 0 } },  // SHL reg, adr
            { new Opcode(0x00, 0x53), new int[] { 0 } },  // SHL reg, ptr
            { new Opcode(0x00, 0x54), new int[] { 0 } },  // SHR reg, reg
            { new Opcode(0x00, 0x55), new int[] { 0 } },  // SHR reg, lit
            { new Opcode(0x00, 0x56), new int[] { 0 } },  // SHR reg, adr
            { new Opcode(0x00, 0x57), new int[] { 0 } },  // SHR reg, ptr

            { new Opcode(0x00, 0x60), new int[] { 0 } },  // AND reg, reg
            { new Opcode(0x00, 0x61), new int[] { 0 } },  // AND reg, lit
            { new Opcode(0x00, 0x62), new int[] { 0 } },  // AND reg, adr
            { new Opcode(0x00, 0x63), new int[] { 0 } },  // AND reg, ptr
            { new Opcode(0x00, 0x64), new int[] { 0 } },  // ORR reg, reg
            { new Opcode(0x00, 0x65), new int[] { 0 } },  // ORR reg, lit
            { new Opcode(0x00, 0x66), new int[] { 0 } },  // ORR reg, adr
            { new Opcode(0x00, 0x67), new int[] { 0 } },  // ORR reg, ptr
            { new Opcode(0x00, 0x68), new int[] { 0 } },  // XOR reg, reg
            { new Opcode(0x00, 0x69), new int[] { 0 } },  // XOR reg, lit
            { new Opcode(0x00, 0x6A), new int[] { 0 } },  // XOR reg, adr
            { new Opcode(0x00, 0x6B), new int[] { 0 } },  // XOR reg, ptr
            { new Opcode(0x00, 0x6C), new int[] { 0 } },  // NOT reg
            { new Opcode(0x00, 0x6D), new int[] { 0 } },  // RNG reg

            { new Opcode(0x00, 0x80), new int[] { 0 } },  // MVB reg, reg
            { new Opcode(0x00, 0x81), new int[] { 0 } },  // MVB reg, lit
            { new Opcode(0x00, 0x82), new int[] { 0 } },  // MVB reg, adr
            { new Opcode(0x00, 0x83), new int[] { 0 } },  // MVB reg, ptr
            { new Opcode(0x00, 0x84), new int[] { 0 } },  // MVB adr, reg
            { new Opcode(0x00, 0x85), new int[] { 0 } },  // MVB adr, lit
            { new Opcode(0x00, 0x86), new int[] { 0 } },  // MVB ptr, reg
            { new Opcode(0x00, 0x87), new int[] { 0 } },  // MVB ptr, lit
            { new Opcode(0x00, 0x88), new int[] { 0 } },  // MVW reg, reg
            { new Opcode(0x00, 0x89), new int[] { 0 } },  // MVW reg, lit
            { new Opcode(0x00, 0x8A), new int[] { 0 } },  // MVW reg, adr
            { new Opcode(0x00, 0x8B), new int[] { 0 } },  // MVW reg, ptr
            { new Opcode(0x00, 0x8C), new int[] { 0 } },  // MVW adr, reg
            { new Opcode(0x00, 0x8D), new int[] { 0 } },  // MVW adr, lit
            { new Opcode(0x00, 0x8E), new int[] { 0 } },  // MVW ptr, reg
            { new Opcode(0x00, 0x8F), new int[] { 0 } },  // MVW ptr, lit

            { new Opcode(0x00, 0x90), new int[] { 0 } },  // MVD reg, reg
            { new Opcode(0x00, 0x91), new int[] { 0 } },  // MVD reg, lit
            { new Opcode(0x00, 0x92), new int[] { 0 } },  // MVD reg, adr
            { new Opcode(0x00, 0x93), new int[] { 0 } },  // MVD reg, ptr
            { new Opcode(0x00, 0x94), new int[] { 0 } },  // MVD adr, reg
            { new Opcode(0x00, 0x95), new int[] { 0 } },  // MVD adr, lit
            { new Opcode(0x00, 0x96), new int[] { 0 } },  // MVD ptr, reg
            { new Opcode(0x00, 0x97), new int[] { 0 } },  // MVD ptr, lit
            { new Opcode(0x00, 0x98), new int[] { 0 } },  // MVQ reg, reg
            { new Opcode(0x00, 0x99), new int[] { 0 } },  // MVQ reg, lit
            { new Opcode(0x00, 0x9A), new int[] { 0 } },  // MVQ reg, adr
            { new Opcode(0x00, 0x9B), new int[] { 0 } },  // MVQ reg, ptr
            { new Opcode(0x00, 0x9C), new int[] { 0 } },  // MVQ adr, reg
            { new Opcode(0x00, 0x9D), new int[] { 0 } },  // MVQ adr, lit
            { new Opcode(0x00, 0x9E), new int[] { 0 } },  // MVQ ptr, reg
            { new Opcode(0x00, 0x9F), new int[] { 0 } },  // MVQ ptr, lit

            { new Opcode(0x00, 0xA4), new int[] { 0 } },  // POP reg

            { new Opcode(0x00, 0xE5), new int[] { 0 } },  // FEX reg, adr
            { new Opcode(0x00, 0xE6), new int[] { 0 } },  // FEX reg, ptr
            { new Opcode(0x00, 0xE7), new int[] { 0 } },  // FSZ reg, adr
            { new Opcode(0x00, 0xE8), new int[] { 0 } },  // FSZ reg, ptr

            { new Opcode(0x00, 0xF0), new int[] { 0 } },  // RCC reg
            { new Opcode(0x00, 0xF1), new int[] { 0 } },  // RFC reg

            // Signed extension set

            { new Opcode(0x01, 0x10), new int[] { 0 } },  // SIGN_DIV reg, reg
            { new Opcode(0x01, 0x11), new int[] { 0 } },  // SIGN_DIV reg, lit
            { new Opcode(0x01, 0x12), new int[] { 0 } },  // SIGN_DIV reg, adr
            { new Opcode(0x01, 0x13), new int[] { 0 } },  // SIGN_DIV reg, ptr
            { new Opcode(0x01, 0x14), new int[] { 0, 1 } },  // SIGN_DVR reg, reg, reg
            { new Opcode(0x01, 0x15), new int[] { 0, 1 } },  // SIGN_DVR reg, reg, lit
            { new Opcode(0x01, 0x16), new int[] { 0, 1 } },  // SIGN_DVR reg, reg, adr
            { new Opcode(0x01, 0x17), new int[] { 0, 1 } },  // SIGN_DVR reg, reg, ptr
            { new Opcode(0x01, 0x18), new int[] { 0 } },  // SIGN_REM reg, reg
            { new Opcode(0x01, 0x19), new int[] { 0 } },  // SIGN_REM reg, lit
            { new Opcode(0x01, 0x1A), new int[] { 0 } },  // SIGN_REM reg, adr
            { new Opcode(0x01, 0x1B), new int[] { 0 } },  // SIGN_REM reg, ptr

            { new Opcode(0x01, 0x20), new int[] { 0 } },  // SIGN_SHR reg, reg
            { new Opcode(0x01, 0x21), new int[] { 0 } },  // SIGN_SHR reg, lit
            { new Opcode(0x01, 0x22), new int[] { 0 } },  // SIGN_SHR reg, adr
            { new Opcode(0x01, 0x23), new int[] { 0 } },  // SIGN_SHR reg, ptr

            { new Opcode(0x01, 0x30), new int[] { 0 } },  // SIGN_MVB reg, reg
            { new Opcode(0x01, 0x31), new int[] { 0 } },  // SIGN_MVB reg, lit
            { new Opcode(0x01, 0x32), new int[] { 0 } },  // SIGN_MVB reg, adr
            { new Opcode(0x01, 0x33), new int[] { 0 } },  // SIGN_MVB reg, ptr
            { new Opcode(0x01, 0x34), new int[] { 0 } },  // SIGN_MVW reg, reg
            { new Opcode(0x01, 0x35), new int[] { 0 } },  // SIGN_MVW reg, lit
            { new Opcode(0x01, 0x36), new int[] { 0 } },  // SIGN_MVW reg, adr
            { new Opcode(0x01, 0x37), new int[] { 0 } },  // SIGN_MVW reg, ptr

            { new Opcode(0x01, 0x40), new int[] { 0 } },  // SIGN_MVD reg, reg
            { new Opcode(0x01, 0x41), new int[] { 0 } },  // SIGN_MVD reg, lit
            { new Opcode(0x01, 0x42), new int[] { 0 } },  // SIGN_MVD reg, adr
            { new Opcode(0x01, 0x43), new int[] { 0 } },  // SIGN_MVD reg, ptr

            { new Opcode(0x01, 0x70), new int[] { 0 } },  // SIGN_EXB reg
            { new Opcode(0x01, 0x71), new int[] { 0 } },  // SIGN_EXW reg
            { new Opcode(0x01, 0x72), new int[] { 0 } },  // SIGN_EXD reg

            { new Opcode(0x01, 0x80), new int[] { 0 } },  // SIGN_NEG reg
        };
        /// <summary>
        /// Every opcode that writes to a literal memory location.
        /// Currently, the address of the memory location is always the first operand.
        /// </summary>
        internal static readonly HashSet<Opcode> writeToMemory = new()
        {
            new Opcode(0x00, 0x84),  // MVB adr, reg
            new Opcode(0x00, 0x85),  // MVB adr, lit
            new Opcode(0x00, 0x8C),  // MVW adr, reg
            new Opcode(0x00, 0x8D),  // MVW adr, lit
            new Opcode(0x00, 0x94),  // MVB adr, reg
            new Opcode(0x00, 0x95),  // MVB adr, lit
            new Opcode(0x00, 0x9C),  // MVW adr, reg
            new Opcode(0x00, 0x9D),  // MVW adr, lit
        };

        /// <summary>
        /// Every opcode that reads a value from memory as data, mapped to the index of the operand that is the memory address.
        /// </summary>
        internal static readonly Dictionary<Opcode, int> readValueFromMemory = new()
        {
            { new Opcode(0x00, 0x12), 1 },  // ADD reg, adr
            { new Opcode(0x00, 0x22), 1 },  // SUB reg, adr
            { new Opcode(0x00, 0x32), 1 },  // MUL reg, adr
            { new Opcode(0x00, 0x42), 1 },  // DIV reg, adr
            { new Opcode(0x00, 0x46), 2 },  // DVR reg, reg, adr
            { new Opcode(0x00, 0x4A), 1 },  // REM reg, adr
            { new Opcode(0x00, 0x52), 1 },  // SHL reg, adr
            { new Opcode(0x00, 0x56), 1 },  // SHR reg, adr
            { new Opcode(0x00, 0x62), 1 },  // AND reg, adr
            { new Opcode(0x00, 0x66), 1 },  // ORR reg, adr
            { new Opcode(0x00, 0x6A), 1 },  // XOR reg, adr
            { new Opcode(0x00, 0x72), 1 },  // TST reg, adr
            { new Opcode(0x00, 0x76), 1 },  // CMP reg, adr
            { new Opcode(0x00, 0x82), 1 },  // MVB reg, adr
            { new Opcode(0x00, 0x8A), 1 },  // MVW reg, adr
            { new Opcode(0x00, 0x92), 1 },  // MVD reg, adr
            { new Opcode(0x00, 0x9A), 1 },  // MVQ reg, adr
            { new Opcode(0x00, 0xA2), 0 },  // PSH adr
            { new Opcode(0x00, 0xB4), 1 },  // CAL adr, adr
            { new Opcode(0x00, 0xB8), 1 },  // CAL ptr, adr
            { new Opcode(0x00, 0xBD), 0 },  // RET adr
            { new Opcode(0x00, 0xC2), 0 },  // WCN adr
            { new Opcode(0x00, 0xC6), 0 },  // WCB adr
            { new Opcode(0x00, 0xCA), 0 },  // WCX adr
            { new Opcode(0x00, 0xCE), 0 },  // WCC adr
            { new Opcode(0x00, 0xD2), 0 },  // WFN adr
            { new Opcode(0x00, 0xD6), 0 },  // WFB adr
            { new Opcode(0x00, 0xDA), 0 },  // WFX adr
            { new Opcode(0x00, 0xDE), 0 },  // WFC adr
            { new Opcode(0x00, 0xE0), 0 },  // OFL adr
            { new Opcode(0x00, 0xE3), 0 },  // DFL adr
            { new Opcode(0x00, 0xE5), 1 },  // FEX reg, adr
            { new Opcode(0x00, 0xE7), 1 },  // FSZ reg, adr

            { new Opcode(0x01, 0x12), 1 },  // SIGN_DIV reg, adr
            { new Opcode(0x01, 0x16), 2 },  // SIGN_DVR reg, reg, adr
            { new Opcode(0x01, 0x1A), 1 },  // SIGN_REM reg, adr
            { new Opcode(0x01, 0x22), 1 },  // SIGN_SHL reg, adr
            { new Opcode(0x01, 0x32), 1 },  // SIGN_MVB reg, adr
            { new Opcode(0x01, 0x36), 1 },  // SIGN_MVW reg, adr
            { new Opcode(0x01, 0x42), 1 },  // SIGN_MVD reg, adr
            { new Opcode(0x01, 0x52), 0 },  // SIGN_WCN adr
            { new Opcode(0x01, 0x56), 0 },  // SIGN_WCB adr
            { new Opcode(0x01, 0x62), 0 },  // SIGN_WFN adr
            { new Opcode(0x01, 0x66), 0 },  // SIGN_WFB adr
        };

        /// <summary>
        /// Directives that result in data (non-code bytes) being inserted into the assembly.
        /// </summary>
        internal static readonly HashSet<string> dataInsertionDirectives = new() { "DAT", "PAD", "NUM" };
        /// <summary>
        /// Every opcode that results in the location of execution being moved to the address of a label.
        /// As of current, the address to jump to is always the first operand to these opcodes.
        /// </summary>
        internal static readonly HashSet<Opcode> jumpCallToLabelOpcodes = new()
        {
            // Jumps
            new Opcode(0x00, 0x02),
            new Opcode(0x00, 0x04),
            new Opcode(0x00, 0x06),
            new Opcode(0x00, 0x08),
            new Opcode(0x00, 0x0A),
            new Opcode(0x00, 0x0C),
            new Opcode(0x00, 0x0E),

            new Opcode(0x01, 0x00),
            new Opcode(0x01, 0x02),
            new Opcode(0x01, 0x04),
            new Opcode(0x01, 0x06),
            new Opcode(0x01, 0x08),
            new Opcode(0x01, 0x0A),
            new Opcode(0x01, 0x0C),
            new Opcode(0x01, 0x0E),
            // Calls
            new Opcode(0x00, 0xB0),
            new Opcode(0x00, 0xB2),
            new Opcode(0x00, 0xB3),
            new Opcode(0x00, 0xB4),
            new Opcode(0x00, 0xB5),
        };
        /// <summary>
        /// Any instruction that can be used to prevent execution flowing onwards 100% of the time.
        /// i.e. Unconditional jump, return, halt
        /// </summary>
        internal static readonly HashSet<Opcode> terminators = new()
        {
            new Opcode(0x00, 0x00),
            new Opcode(0x00, 0x02),
            new Opcode(0x00, 0x03),
            new Opcode(0x00, 0xBA),
            new Opcode(0x00, 0xBB),
            new Opcode(0x00, 0xBC),
            new Opcode(0x00, 0xBD),
            new Opcode(0x00, 0xBE),
        };

        /// <summary>
        /// All opcodes that move a literal value into a register. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<Opcode> moveRegLit = new()
        {
            new Opcode(0x00, 0x81),
            new Opcode(0x00, 0x89),
            new Opcode(0x00, 0x91),
            new Opcode(0x00, 0x99),

            new Opcode(0x01, 0x31),
            new Opcode(0x01, 0x35),
            new Opcode(0x01, 0x41),
        };

        /// <summary>
        /// All opcodes that move a literal value. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<Opcode> moveLiteral = new()
        {
            new Opcode(0x00, 0x81),
            new Opcode(0x00, 0x85),
            new Opcode(0x00, 0x87),
            new Opcode(0x00, 0x89),
            new Opcode(0x00, 0x8D),
            new Opcode(0x00, 0x8F),
            new Opcode(0x00, 0x91),
            new Opcode(0x00, 0x95),
            new Opcode(0x00, 0x97),
            new Opcode(0x00, 0x99),
            new Opcode(0x00, 0x9D),
            new Opcode(0x00, 0x9F),

            new Opcode(0x01, 0x31),
            new Opcode(0x01, 0x35),
            new Opcode(0x01, 0x41),
        };
        /// <summary>
        /// All opcodes that shift the bits of a register by a literal value. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<Opcode> shiftByLiteral = new()
        {
            new Opcode(0x00, 0x51),
            new Opcode(0x00, 0x55),

            new Opcode(0x01, 0x21),
        };
        /// <summary>
        /// All opcodes that perform a division by a literal value. The mapped value is the index of the literal in the operands.
        /// </summary>
        internal static readonly Dictionary<Opcode, int> divisionByLiteral = new()
        {
            { new Opcode(0x00, 0x41), 1 },
            { new Opcode(0x00, 0x45), 2 },
            { new Opcode(0x00, 0x49), 1 },

            { new Opcode(0x01, 0x11), 1 },
            { new Opcode(0x01, 0x15), 2 },
            { new Opcode(0x01, 0x19), 1 },
        };

        /// <summary>
        /// The upper and lower numeric limits of each move instruction before bits begin to be truncated.
        /// </summary>
        internal static readonly Dictionary<Opcode, (ulong MaxValue, long MinValue)> moveLimits = new()
        {
            // MVB
            { new Opcode(0x00, 0x80), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x81), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x82), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x83), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x84), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x85), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x86), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x00, 0x87), (byte.MaxValue, sbyte.MinValue) },
            // MVW
            { new Opcode(0x00, 0x88), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x89), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8A), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8B), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8C), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8D), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8E), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x00, 0x8F), (ushort.MaxValue, short.MinValue) },
            // MVD
            { new Opcode(0x00, 0x90), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x91), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x92), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x93), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x94), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x95), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x96), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x00, 0x97), (uint.MaxValue, int.MinValue) },
            // MVQ
            { new Opcode(0x00, 0x98), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x99), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9A), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9B), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9C), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9D), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9E), (ulong.MaxValue, long.MinValue) },
            { new Opcode(0x00, 0x9F), (ulong.MaxValue, long.MinValue) },

            // SIGN_MVB
            { new Opcode(0x01, 0x30), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x01, 0x31), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x01, 0x32), (byte.MaxValue, sbyte.MinValue) },
            { new Opcode(0x01, 0x33), (byte.MaxValue, sbyte.MinValue) },
            // SIGN_MVW
            { new Opcode(0x01, 0x34), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x01, 0x35), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x01, 0x36), (ushort.MaxValue, short.MinValue) },
            { new Opcode(0x01, 0x37), (ushort.MaxValue, short.MinValue) },
            // SIGN_MVD
            { new Opcode(0x01, 0x40), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x01, 0x41), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x01, 0x42), (uint.MaxValue, int.MinValue) },
            { new Opcode(0x01, 0x43), (uint.MaxValue, int.MinValue) },
        };
    }
}
