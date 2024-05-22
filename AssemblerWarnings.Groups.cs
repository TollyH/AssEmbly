namespace AssEmbly
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

            { new Opcode(0x00, 0x10), new[] { 0 } },  // ADD reg, reg
            { new Opcode(0x00, 0x11), new[] { 0 } },  // ADD reg, lit
            { new Opcode(0x00, 0x12), new[] { 0 } },  // ADD reg, adr
            { new Opcode(0x00, 0x13), new[] { 0 } },  // ADD reg, ptr
            { new Opcode(0x00, 0x14), new[] { 0 } },  // ICR reg

            { new Opcode(0x00, 0x20), new[] { 0 } },  // SUB reg, reg
            { new Opcode(0x00, 0x21), new[] { 0 } },  // SUB reg, lit
            { new Opcode(0x00, 0x22), new[] { 0 } },  // SUB reg, adr
            { new Opcode(0x00, 0x23), new[] { 0 } },  // SUB reg, ptr
            { new Opcode(0x00, 0x24), new[] { 0 } },  // DCR reg

            { new Opcode(0x00, 0x30), new[] { 0 } },  // MUL reg, reg
            { new Opcode(0x00, 0x31), new[] { 0 } },  // MUL reg, lit
            { new Opcode(0x00, 0x32), new[] { 0 } },  // MUL reg, adr
            { new Opcode(0x00, 0x33), new[] { 0 } },  // MUL reg, ptr

            { new Opcode(0x00, 0x40), new[] { 0 } },  // DIV reg, reg
            { new Opcode(0x00, 0x41), new[] { 0 } },  // DIV reg, lit
            { new Opcode(0x00, 0x42), new[] { 0 } },  // DIV reg, adr
            { new Opcode(0x00, 0x43), new[] { 0 } },  // DIV reg, ptr
            { new Opcode(0x00, 0x44), new[] { 0, 1 } },  // DVR reg, reg, reg
            { new Opcode(0x00, 0x45), new[] { 0, 1 } },  // DVR reg, reg, lit
            { new Opcode(0x00, 0x46), new[] { 0, 1 } },  // DVR reg, reg, adr
            { new Opcode(0x00, 0x47), new[] { 0, 1 } },  // DVR reg, reg, ptr
            { new Opcode(0x00, 0x48), new[] { 0 } },  // REM reg, reg
            { new Opcode(0x00, 0x49), new[] { 0 } },  // REM reg, lit
            { new Opcode(0x00, 0x4A), new[] { 0 } },  // REM reg, adr
            { new Opcode(0x00, 0x4B), new[] { 0 } },  // REM reg, ptr

            { new Opcode(0x00, 0x50), new[] { 0 } },  // SHL reg, reg
            { new Opcode(0x00, 0x51), new[] { 0 } },  // SHL reg, lit
            { new Opcode(0x00, 0x52), new[] { 0 } },  // SHL reg, adr
            { new Opcode(0x00, 0x53), new[] { 0 } },  // SHL reg, ptr
            { new Opcode(0x00, 0x54), new[] { 0 } },  // SHR reg, reg
            { new Opcode(0x00, 0x55), new[] { 0 } },  // SHR reg, lit
            { new Opcode(0x00, 0x56), new[] { 0 } },  // SHR reg, adr
            { new Opcode(0x00, 0x57), new[] { 0 } },  // SHR reg, ptr

            { new Opcode(0x00, 0x60), new[] { 0 } },  // AND reg, reg
            { new Opcode(0x00, 0x61), new[] { 0 } },  // AND reg, lit
            { new Opcode(0x00, 0x62), new[] { 0 } },  // AND reg, adr
            { new Opcode(0x00, 0x63), new[] { 0 } },  // AND reg, ptr
            { new Opcode(0x00, 0x64), new[] { 0 } },  // ORR reg, reg
            { new Opcode(0x00, 0x65), new[] { 0 } },  // ORR reg, lit
            { new Opcode(0x00, 0x66), new[] { 0 } },  // ORR reg, adr
            { new Opcode(0x00, 0x67), new[] { 0 } },  // ORR reg, ptr
            { new Opcode(0x00, 0x68), new[] { 0 } },  // XOR reg, reg
            { new Opcode(0x00, 0x69), new[] { 0 } },  // XOR reg, lit
            { new Opcode(0x00, 0x6A), new[] { 0 } },  // XOR reg, adr
            { new Opcode(0x00, 0x6B), new[] { 0 } },  // XOR reg, ptr
            { new Opcode(0x00, 0x6C), new[] { 0 } },  // NOT reg
            { new Opcode(0x00, 0x6D), new[] { 0 } },  // RNG reg

            { new Opcode(0x00, 0x80), new[] { 0 } },  // MVB reg, reg
            { new Opcode(0x00, 0x81), new[] { 0 } },  // MVB reg, lit
            { new Opcode(0x00, 0x82), new[] { 0 } },  // MVB reg, adr
            { new Opcode(0x00, 0x83), new[] { 0 } },  // MVB reg, ptr
            { new Opcode(0x00, 0x84), new[] { 0 } },  // MVB adr, reg
            { new Opcode(0x00, 0x85), new[] { 0 } },  // MVB adr, lit
            { new Opcode(0x00, 0x86), new[] { 0 } },  // MVB ptr, reg
            { new Opcode(0x00, 0x87), new[] { 0 } },  // MVB ptr, lit
            { new Opcode(0x00, 0x88), new[] { 0 } },  // MVW reg, reg
            { new Opcode(0x00, 0x89), new[] { 0 } },  // MVW reg, lit
            { new Opcode(0x00, 0x8A), new[] { 0 } },  // MVW reg, adr
            { new Opcode(0x00, 0x8B), new[] { 0 } },  // MVW reg, ptr
            { new Opcode(0x00, 0x8C), new[] { 0 } },  // MVW adr, reg
            { new Opcode(0x00, 0x8D), new[] { 0 } },  // MVW adr, lit
            { new Opcode(0x00, 0x8E), new[] { 0 } },  // MVW ptr, reg
            { new Opcode(0x00, 0x8F), new[] { 0 } },  // MVW ptr, lit

            { new Opcode(0x00, 0x90), new[] { 0 } },  // MVD reg, reg
            { new Opcode(0x00, 0x91), new[] { 0 } },  // MVD reg, lit
            { new Opcode(0x00, 0x92), new[] { 0 } },  // MVD reg, adr
            { new Opcode(0x00, 0x93), new[] { 0 } },  // MVD reg, ptr
            { new Opcode(0x00, 0x94), new[] { 0 } },  // MVD adr, reg
            { new Opcode(0x00, 0x95), new[] { 0 } },  // MVD adr, lit
            { new Opcode(0x00, 0x96), new[] { 0 } },  // MVD ptr, reg
            { new Opcode(0x00, 0x97), new[] { 0 } },  // MVD ptr, lit
            { new Opcode(0x00, 0x98), new[] { 0 } },  // MVQ reg, reg
            { new Opcode(0x00, 0x99), new[] { 0 } },  // MVQ reg, lit
            { new Opcode(0x00, 0x9A), new[] { 0 } },  // MVQ reg, adr
            { new Opcode(0x00, 0x9B), new[] { 0 } },  // MVQ reg, ptr
            { new Opcode(0x00, 0x9C), new[] { 0 } },  // MVQ adr, reg
            { new Opcode(0x00, 0x9D), new[] { 0 } },  // MVQ adr, lit
            { new Opcode(0x00, 0x9E), new[] { 0 } },  // MVQ ptr, reg
            { new Opcode(0x00, 0x9F), new[] { 0 } },  // MVQ ptr, lit

            { new Opcode(0x00, 0xA4), new[] { 0 } },  // POP reg

            { new Opcode(0x00, 0xE5), new[] { 0 } },  // FEX reg, adr
            { new Opcode(0x00, 0xE6), new[] { 0 } },  // FEX reg, ptr
            { new Opcode(0x00, 0xE7), new[] { 0 } },  // FSZ reg, adr
            { new Opcode(0x00, 0xE8), new[] { 0 } },  // FSZ reg, ptr

            { new Opcode(0x00, 0xF0), new[] { 0 } },  // RCC reg
            { new Opcode(0x00, 0xF1), new[] { 0 } },  // RFC reg

            // Signed extension set

            { new Opcode(0x01, 0x10), new[] { 0 } },  // SIGN_DIV reg, reg
            { new Opcode(0x01, 0x11), new[] { 0 } },  // SIGN_DIV reg, lit
            { new Opcode(0x01, 0x12), new[] { 0 } },  // SIGN_DIV reg, adr
            { new Opcode(0x01, 0x13), new[] { 0 } },  // SIGN_DIV reg, ptr
            { new Opcode(0x01, 0x14), new[] { 0, 1 } },  // SIGN_DVR reg, reg, reg
            { new Opcode(0x01, 0x15), new[] { 0, 1 } },  // SIGN_DVR reg, reg, lit
            { new Opcode(0x01, 0x16), new[] { 0, 1 } },  // SIGN_DVR reg, reg, adr
            { new Opcode(0x01, 0x17), new[] { 0, 1 } },  // SIGN_DVR reg, reg, ptr
            { new Opcode(0x01, 0x18), new[] { 0 } },  // SIGN_REM reg, reg
            { new Opcode(0x01, 0x19), new[] { 0 } },  // SIGN_REM reg, lit
            { new Opcode(0x01, 0x1A), new[] { 0 } },  // SIGN_REM reg, adr
            { new Opcode(0x01, 0x1B), new[] { 0 } },  // SIGN_REM reg, ptr

            { new Opcode(0x01, 0x20), new[] { 0 } },  // SIGN_SHR reg, reg
            { new Opcode(0x01, 0x21), new[] { 0 } },  // SIGN_SHR reg, lit
            { new Opcode(0x01, 0x22), new[] { 0 } },  // SIGN_SHR reg, adr
            { new Opcode(0x01, 0x23), new[] { 0 } },  // SIGN_SHR reg, ptr

            { new Opcode(0x01, 0x30), new[] { 0 } },  // SIGN_MVB reg, reg
            { new Opcode(0x01, 0x31), new[] { 0 } },  // SIGN_MVB reg, lit
            { new Opcode(0x01, 0x32), new[] { 0 } },  // SIGN_MVB reg, adr
            { new Opcode(0x01, 0x33), new[] { 0 } },  // SIGN_MVB reg, ptr
            { new Opcode(0x01, 0x34), new[] { 0 } },  // SIGN_MVW reg, reg
            { new Opcode(0x01, 0x35), new[] { 0 } },  // SIGN_MVW reg, lit
            { new Opcode(0x01, 0x36), new[] { 0 } },  // SIGN_MVW reg, adr
            { new Opcode(0x01, 0x37), new[] { 0 } },  // SIGN_MVW reg, ptr

            { new Opcode(0x01, 0x40), new[] { 0 } },  // SIGN_MVD reg, reg
            { new Opcode(0x01, 0x41), new[] { 0 } },  // SIGN_MVD reg, lit
            { new Opcode(0x01, 0x42), new[] { 0 } },  // SIGN_MVD reg, adr
            { new Opcode(0x01, 0x43), new[] { 0 } },  // SIGN_MVD reg, ptr

            { new Opcode(0x01, 0x70), new[] { 0 } },  // SIGN_EXB reg
            { new Opcode(0x01, 0x71), new[] { 0 } },  // SIGN_EXW reg
            { new Opcode(0x01, 0x72), new[] { 0 } },  // SIGN_EXD reg

            { new Opcode(0x01, 0x80), new[] { 0 } },  // SIGN_NEG reg

            // Floating point extension set

            { new Opcode(0x02, 0x00), new[] { 0 } },  // FLPT_ADD reg, reg
            { new Opcode(0x02, 0x01), new[] { 0 } },  // FLPT_ADD reg, lit
            { new Opcode(0x02, 0x02), new[] { 0 } },  // FLPT_ADD reg, adr
            { new Opcode(0x02, 0x03), new[] { 0 } },  // FLPT_ADD reg, ptr

            { new Opcode(0x02, 0x10), new[] { 0 } },  // FLPT_SUB reg, reg
            { new Opcode(0x02, 0x11), new[] { 0 } },  // FLPT_SUB reg, lit
            { new Opcode(0x02, 0x12), new[] { 0 } },  // FLPT_SUB reg, adr
            { new Opcode(0x02, 0x13), new[] { 0 } },  // FLPT_SUB reg, ptr

            { new Opcode(0x02, 0x20), new[] { 0 } },  // FLPT_MUL reg, reg
            { new Opcode(0x02, 0x21), new[] { 0 } },  // FLPT_MUL reg, lit
            { new Opcode(0x02, 0x22), new[] { 0 } },  // FLPT_MUL reg, adr
            { new Opcode(0x02, 0x23), new[] { 0 } },  // FLPT_MUL reg, ptr

            { new Opcode(0x02, 0x30), new[] { 0 } },  // FLPT_DIV reg, reg
            { new Opcode(0x02, 0x31), new[] { 0 } },  // FLPT_DIV reg, lit
            { new Opcode(0x02, 0x32), new[] { 0 } },  // FLPT_DIV reg, adr
            { new Opcode(0x02, 0x33), new[] { 0 } },  // FLPT_DIV reg, ptr
            { new Opcode(0x02, 0x34), new[] { 0, 1 } },  // FLPT_DVR reg, reg, reg
            { new Opcode(0x02, 0x35), new[] { 0, 1 } },  // FLPT_DVR reg, reg, lit
            { new Opcode(0x02, 0x36), new[] { 0, 1 } },  // FLPT_DVR reg, reg, adr
            { new Opcode(0x02, 0x37), new[] { 0, 1 } },  // FLPT_DVR reg, reg, ptr
            { new Opcode(0x02, 0x38), new[] { 0 } },  // FLPT_REM reg, reg
            { new Opcode(0x02, 0x39), new[] { 0 } },  // FLPT_REM reg, lit
            { new Opcode(0x02, 0x3A), new[] { 0 } },  // FLPT_REM reg, adr
            { new Opcode(0x02, 0x3B), new[] { 0 } },  // FLPT_REM reg, ptr

            { new Opcode(0x02, 0x40), new[] { 0 } },  // FLPT_SIN reg
            { new Opcode(0x02, 0x41), new[] { 0 } },  // FLPT_ASN reg
            { new Opcode(0x02, 0x42), new[] { 0 } },  // FLPT_COS reg
            { new Opcode(0x02, 0x43), new[] { 0 } },  // FLPT_ACS reg
            { new Opcode(0x02, 0x44), new[] { 0 } },  // FLPT_TAN reg
            { new Opcode(0x02, 0x45), new[] { 0 } },  // FLPT_ATN reg
            { new Opcode(0x02, 0x46), new[] { 0 } },  // FLPT_PTN reg, reg
            { new Opcode(0x02, 0x47), new[] { 0 } },  // FLPT_PTN reg, lit
            { new Opcode(0x02, 0x48), new[] { 0 } },  // FLPT_PTN reg, adr
            { new Opcode(0x02, 0x49), new[] { 0 } },  // FLPT_PTN reg, ptr

            { new Opcode(0x02, 0x50), new[] { 0 } },  // FLPT_POW reg, reg
            { new Opcode(0x02, 0x51), new[] { 0 } },  // FLPT_POW reg, lit
            { new Opcode(0x02, 0x52), new[] { 0 } },  // FLPT_POW reg, adr
            { new Opcode(0x02, 0x53), new[] { 0 } },  // FLPT_POW reg, ptr

            { new Opcode(0x02, 0x60), new[] { 0 } },  // FLPT_LOG reg, reg
            { new Opcode(0x02, 0x61), new[] { 0 } },  // FLPT_LOG reg, lit
            { new Opcode(0x02, 0x62), new[] { 0 } },  // FLPT_LOG reg, adr
            { new Opcode(0x02, 0x63), new[] { 0 } },  // FLPT_LOG reg, ptr

            { new Opcode(0x02, 0x90), new[] { 0 } },  // FLPT_EXH reg
            { new Opcode(0x02, 0x91), new[] { 0 } },  // FLPT_EXS reg
            { new Opcode(0x02, 0x92), new[] { 0 } },  // FLPT_SHS reg
            { new Opcode(0x02, 0x93), new[] { 0 } },  // FLPT_SHH reg

            { new Opcode(0x02, 0xA0), new[] { 0 } },  // FLPT_NEG reg

            { new Opcode(0x02, 0xB0), new[] { 0 } },  // FLPT_UTF reg
            { new Opcode(0x02, 0xB1), new[] { 0 } },  // FLPT_STF reg

            { new Opcode(0x02, 0xC0), new[] { 0 } },  // FLPT_FTS reg
            { new Opcode(0x02, 0xC1), new[] { 0 } },  // FLPT_FCS reg
            { new Opcode(0x02, 0xC2), new[] { 0 } },  // FLPT_FFS reg
            { new Opcode(0x02, 0xC3), new[] { 0 } },  // FLPT_FNS reg

            // Extended base set

            { new Opcode(0x03, 0x00), new[] { 0 } },  // EXTD_BSW reg

            // External assembly extension set

            { new Opcode(0x04, 0x20), new[] { 0 } },  // ASMX_AEX reg, adr
            { new Opcode(0x04, 0x21), new[] { 0 } },  // ASMX_AEX reg, ptr
            { new Opcode(0x04, 0x22), new[] { 0 } },  // ASMX_FEX reg, adr
            { new Opcode(0x04, 0x23), new[] { 0 } },  // ASMX_FEX reg, ptr

            // Memory allocation extension set

            { new Opcode(0x05, 0x00), new[] { 0 } },  // HEAP_ALC reg, reg
            { new Opcode(0x05, 0x01), new[] { 0 } },  // HEAP_ALC reg, lit
            { new Opcode(0x05, 0x02), new[] { 0 } },  // HEAP_ALC reg, adr
            { new Opcode(0x05, 0x03), new[] { 0 } },  // HEAP_ALC reg, ptr

            { new Opcode(0x05, 0x04), new[] { 0 } },  // HEAP_TRY reg, reg
            { new Opcode(0x05, 0x05), new[] { 0 } },  // HEAP_TRY reg, lit
            { new Opcode(0x05, 0x06), new[] { 0 } },  // HEAP_TRY reg, adr
            { new Opcode(0x05, 0x07), new[] { 0 } },  // HEAP_TRY reg, ptr

            { new Opcode(0x05, 0x10), new[] { 0 } },  // HEAP_REA reg, reg
            { new Opcode(0x05, 0x11), new[] { 0 } },  // HEAP_REA reg, lit
            { new Opcode(0x05, 0x12), new[] { 0 } },  // HEAP_REA reg, adr
            { new Opcode(0x05, 0x13), new[] { 0 } },  // HEAP_REA reg, ptr

            { new Opcode(0x05, 0x14), new[] { 0 } },  // HEAP_TRE reg, reg
            { new Opcode(0x05, 0x15), new[] { 0 } },  // HEAP_TRE reg, lit
            { new Opcode(0x05, 0x16), new[] { 0 } },  // HEAP_TRE reg, adr
            { new Opcode(0x05, 0x17), new[] { 0 } },  // HEAP_TRE reg, ptr

            // File system extension set

            { new Opcode(0x06, 0x02), new[] { 0 } },  // FSYS_GWD adr
            { new Opcode(0x06, 0x03), new[] { 0 } },  // FSYS_GWD ptr

            { new Opcode(0x06, 0x30), new[] { 0 } },  // FSYS_DEX reg, adr
            { new Opcode(0x06, 0x31), new[] { 0 } },  // FSYS_DEX reg, ptr

            { new Opcode(0x06, 0x60), new[] { 0 } },  // FSYS_GNF adr
            { new Opcode(0x06, 0x61), new[] { 0 } },  // FSYS_GNF ptr
            { new Opcode(0x06, 0x62), new[] { 0 } },  // FSYS_GND adr
            { new Opcode(0x06, 0x63), new[] { 0 } },  // FSYS_GND ptr

            { new Opcode(0x06, 0x70), new[] { 0 } },  // FSYS_GCT reg, adr
            { new Opcode(0x06, 0x71), new[] { 0 } },  // FSYS_GCT reg, ptr
            { new Opcode(0x06, 0x72), new[] { 0 } },  // FSYS_GMT reg, adr
            { new Opcode(0x06, 0x73), new[] { 0 } },  // FSYS_GMT reg, ptr
            { new Opcode(0x06, 0x74), new[] { 0 } },  // FSYS_GAT reg, adr
            { new Opcode(0x06, 0x75), new[] { 0 } },  // FSYS_GAT reg, ptr

            // Terminal extension set

            { new Opcode(0x07, 0x30), new[] { 0 } },  // TERM_GCY reg
            { new Opcode(0x07, 0x31), new[] { 0 } },  // TERM_GCX reg
            { new Opcode(0x07, 0x32), new[] { 0 } },  // TERM_GSY reg
            { new Opcode(0x07, 0x33), new[] { 0 } },  // TERM_GSX reg
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

            new Opcode(0x06, 0x02),  // FSYS_GWD adr
            new Opcode(0x06, 0x60),  // FSYS_GNF adr
            new Opcode(0x06, 0x62),  // FSYS_GND adr
        };

        /// <summary>
        /// Every opcode that reads a value from memory as data, mapped to the byte offsets of the operands that are the memory addresses (not including the opcode).
        /// </summary>
        internal static readonly Dictionary<Opcode, int[]> readValueFromMemory = new()
        {
            { new Opcode(0x00, 0x12), new[] { 1 } },  // ADD reg, adr
            { new Opcode(0x00, 0x22), new[] { 1 } },  // SUB reg, adr
            { new Opcode(0x00, 0x32), new[] { 1 } },  // MUL reg, adr
            { new Opcode(0x00, 0x42), new[] { 1 } },  // DIV reg, adr
            { new Opcode(0x00, 0x46), new[] { 2 } },  // DVR reg, reg, adr
            { new Opcode(0x00, 0x4A), new[] { 1 } },  // REM reg, adr
            { new Opcode(0x00, 0x52), new[] { 1 } },  // SHL reg, adr
            { new Opcode(0x00, 0x56), new[] { 1 } },  // SHR reg, adr
            { new Opcode(0x00, 0x62), new[] { 1 } },  // AND reg, adr
            { new Opcode(0x00, 0x66), new[] { 1 } },  // ORR reg, adr
            { new Opcode(0x00, 0x6A), new[] { 1 } },  // XOR reg, adr
            { new Opcode(0x00, 0x72), new[] { 1 } },  // TST reg, adr
            { new Opcode(0x00, 0x76), new[] { 1 } },  // CMP reg, adr
            { new Opcode(0x00, 0x82), new[] { 1 } },  // MVB reg, adr
            { new Opcode(0x00, 0x8A), new[] { 1 } },  // MVW reg, adr
            { new Opcode(0x00, 0x92), new[] { 1 } },  // MVD reg, adr
            { new Opcode(0x00, 0x9A), new[] { 1 } },  // MVQ reg, adr
            { new Opcode(0x00, 0xA2), new[] { 0 } },  // PSH adr
            { new Opcode(0x00, 0xB4), new[] { 1 } },  // CAL adr, adr
            { new Opcode(0x00, 0xB8), new[] { 1 } },  // CAL ptr, adr
            { new Opcode(0x00, 0xBD), new[] { 0 } },  // RET adr
            { new Opcode(0x00, 0xC2), new[] { 0 } },  // WCN adr
            { new Opcode(0x00, 0xC6), new[] { 0 } },  // WCB adr
            { new Opcode(0x00, 0xCA), new[] { 0 } },  // WCX adr
            { new Opcode(0x00, 0xCE), new[] { 0 } },  // WCC adr
            { new Opcode(0x00, 0xD2), new[] { 0 } },  // WFN adr
            { new Opcode(0x00, 0xD6), new[] { 0 } },  // WFB adr
            { new Opcode(0x00, 0xDA), new[] { 0 } },  // WFX adr
            { new Opcode(0x00, 0xDE), new[] { 0 } },  // WFC adr
            { new Opcode(0x00, 0xE0), new[] { 0 } },  // OFL adr
            { new Opcode(0x00, 0xE3), new[] { 0 } },  // DFL adr
            { new Opcode(0x00, 0xE5), new[] { 1 } },  // FEX reg, adr
            { new Opcode(0x00, 0xE7), new[] { 1 } },  // FSZ reg, adr

            { new Opcode(0x01, 0x12), new[] { 1 } },  // SIGN_DIV reg, adr
            { new Opcode(0x01, 0x16), new[] { 2 } },  // SIGN_DVR reg, reg, adr
            { new Opcode(0x01, 0x1A), new[] { 1 } },  // SIGN_REM reg, adr
            { new Opcode(0x01, 0x22), new[] { 1 } },  // SIGN_SHL reg, adr
            { new Opcode(0x01, 0x32), new[] { 1 } },  // SIGN_MVB reg, adr
            { new Opcode(0x01, 0x36), new[] { 1 } },  // SIGN_MVW reg, adr
            { new Opcode(0x01, 0x42), new[] { 1 } },  // SIGN_MVD reg, adr
            { new Opcode(0x01, 0x52), new[] { 0 } },  // SIGN_WCN adr
            { new Opcode(0x01, 0x56), new[] { 0 } },  // SIGN_WCB adr
            { new Opcode(0x01, 0x62), new[] { 0 } },  // SIGN_WFN adr
            { new Opcode(0x01, 0x66), new[] { 0 } },  // SIGN_WFB adr

            { new Opcode(0x02, 0x02), new[] { 1 } },  // FLPT_ADD reg, adr
            { new Opcode(0x02, 0x12), new[] { 1 } },  // FLPT_SUB reg, adr
            { new Opcode(0x02, 0x22), new[] { 1 } },  // FLPT_MUL reg, adr
            { new Opcode(0x02, 0x32), new[] { 1 } },  // FLPT_DIV reg, adr
            { new Opcode(0x02, 0x36), new[] { 2 } },  // FLPT_DVR reg, reg, adr
            { new Opcode(0x02, 0x3A), new[] { 1 } },  // FLPT_REM reg, adr
            { new Opcode(0x02, 0x48), new[] { 1 } },  // FLPT_PTN reg, adr
            { new Opcode(0x02, 0x52), new[] { 1 } },  // FLPT_POW reg, adr
            { new Opcode(0x02, 0x62), new[] { 1 } },  // FLPT_LOG reg, adr
            { new Opcode(0x02, 0x72), new[] { 0 } },  // FLPT_WCN adr
            { new Opcode(0x02, 0x82), new[] { 0 } },  // FLPT_WFN adr
            { new Opcode(0x02, 0xD2), new[] { 1 } },  // FLPT_CMP reg, adr

            { new Opcode(0x04, 0x20), new[] { 1 } },  // ASMX_AEX reg, adr
            { new Opcode(0x04, 0x22), new[] { 1 } },  // ASMX_FEX reg, adr

            { new Opcode(0x05, 0x02), new[] { 1 } },  // HEAP_ALC reg, adr
            { new Opcode(0x05, 0x06), new[] { 1 } },  // HEAP_TRY reg, adr
            { new Opcode(0x05, 0x12), new[] { 1 } },  // HEAP_REA reg, adr
            { new Opcode(0x05, 0x16), new[] { 1 } },  // HEAP_TRE reg, adr

            { new Opcode(0x06, 0x00), new[] { 0 } },  // FSYS_CWD adr
            { new Opcode(0x06, 0x10), new[] { 0 } },  // FSYS_CDR adr
            { new Opcode(0x06, 0x20), new[] { 0 } },  // FSYS_DDR adr
            { new Opcode(0x06, 0x22), new[] { 0 } },  // FSYS_DDE adr
            { new Opcode(0x06, 0x30), new[] { 1 } },  // FSYS_DEX reg, adr
            { new Opcode(0x06, 0x40), new[] { 0, 1 } },  // FSYS_CPY adr, adr
            { new Opcode(0x06, 0x41), new[] { 0 } },  // FSYS_CPY adr, ptr
            { new Opcode(0x06, 0x42), new[] { 1 } },  // FSYS_CPY ptr, adr
            { new Opcode(0x06, 0x44), new[] { 0, 1 } },  // FSYS_MOV adr, adr
            { new Opcode(0x06, 0x45), new[] { 0 } },  // FSYS_MOV adr, ptr
            { new Opcode(0x06, 0x46), new[] { 1 } },  // FSYS_MOV ptr, adr
            { new Opcode(0x06, 0x51), new[] { 0 } },  // FSYS_BDL adr
            { new Opcode(0x06, 0x70), new[] { 1 } },  // FSYS_GCT reg, adr
            { new Opcode(0x06, 0x72), new[] { 1 } },  // FSYS_GMT reg, adr
            { new Opcode(0x06, 0x74), new[] { 1 } },  // FSYS_GAT reg, adr
            { new Opcode(0x06, 0x80), new[] { 0 } },  // FSYS_SCT adr, reg
            { new Opcode(0x06, 0x82), new[] { 0 } },  // FSYS_SCT adr, lit
            { new Opcode(0x06, 0x84), new[] { 0 } },  // FSYS_SMT adr, reg
            { new Opcode(0x06, 0x86), new[] { 0 } },  // FSYS_SMT adr, lit
            { new Opcode(0x06, 0x88), new[] { 0 } },  // FSYS_SAT adr, reg
            { new Opcode(0x06, 0x8A), new[] { 0 } },  // FSYS_SAT adr, lit

            { new Opcode(0x07, 0x22), new[] { 0 } },  // TERM_SCY adr
            { new Opcode(0x07, 0x26), new[] { 0 } },  // TERM_SCX adr
            { new Opcode(0x07, 0x52), new[] { 0 } },  // TERM_SFC adr
            { new Opcode(0x07, 0x56), new[] { 0 } },  // TERM_SBC adr
        };

        /// <summary>
        /// All instructions that only use the address of a pointer, not the value at that address, thereby making the read size irrelevant.
        /// Includes instructions that write to the pointer address or that read a string from the address.
        /// Mapped to the indices of the pointer operands (not including the opcode).
        /// </summary>
        internal static readonly Dictionary<Opcode, int[]> pointerForAddress = new()
        {
            { new Opcode(0x00, 0x03), new[] { 0 } },
            { new Opcode(0x00, 0x05), new[] { 0 } },
            { new Opcode(0x00, 0x07), new[] { 0 } },
            { new Opcode(0x00, 0x09), new[] { 0 } },
            { new Opcode(0x00, 0x0B), new[] { 0 } },
            { new Opcode(0x00, 0x0D), new[] { 0 } },
            { new Opcode(0x00, 0x0F), new[] { 0 } },
            { new Opcode(0x00, 0xB1), new[] { 0 } },
            { new Opcode(0x00, 0xB6), new[] { 0 } },
            { new Opcode(0x00, 0xB7), new[] { 0 } },
            { new Opcode(0x00, 0xB8), new[] { 0 } },
            { new Opcode(0x00, 0xB9), new[] { 0 } },

            { new Opcode(0x01, 0x01), new[] { 0 } },
            { new Opcode(0x01, 0x03), new[] { 0 } },
            { new Opcode(0x01, 0x05), new[] { 0 } },
            { new Opcode(0x01, 0x07), new[] { 0 } },
            { new Opcode(0x01, 0x09), new[] { 0 } },
            { new Opcode(0x01, 0x0B), new[] { 0 } },
            { new Opcode(0x01, 0x0D), new[] { 0 } },
            { new Opcode(0x01, 0x0F), new[] { 0 } },

            { new Opcode(0x03, 0x30), new[] { 1 } },
            { new Opcode(0x03, 0x31), new[] { 1 } },
            { new Opcode(0x03, 0x32), new[] { 0, 1 } },

            { new Opcode(0x00, 0xE1), new[] { 0 } },
            { new Opcode(0x00, 0xE4), new[] { 0 } },
            { new Opcode(0x00, 0xE6), new[] { 1 } },
            { new Opcode(0x00, 0xE8), new[] { 1 } },

            { new Opcode(0x04, 0x01), new[] { 0 } },
            { new Opcode(0x04, 0x03), new[] { 0 } },
            { new Opcode(0x04, 0x21), new[] { 1 } },
            { new Opcode(0x04, 0x23), new[] { 1 } },

            { new Opcode(0x06, 0x01), new[] { 0 } },
            { new Opcode(0x06, 0x11), new[] { 0 } },
            { new Opcode(0x06, 0x21), new[] { 0 } },
            { new Opcode(0x06, 0x23), new[] { 0 } },
            { new Opcode(0x06, 0x31), new[] { 1 } },
            { new Opcode(0x06, 0x41), new[] { 1 } },
            { new Opcode(0x06, 0x45), new[] { 1 } },
            { new Opcode(0x06, 0x42), new[] { 1 } },
            { new Opcode(0x06, 0x46), new[] { 1 } },
            { new Opcode(0x06, 0x43), new[] { 0, 1 } },
            { new Opcode(0x06, 0x47), new[] { 0, 1 } },
            { new Opcode(0x06, 0x52), new[] { 0 } },
            { new Opcode(0x06, 0x71), new[] { 1 } },
            { new Opcode(0x06, 0x73), new[] { 1 } },
            { new Opcode(0x06, 0x75), new[] { 1 } },
            { new Opcode(0x06, 0x81), new[] { 0 } },
            { new Opcode(0x06, 0x83), new[] { 0 } },
            { new Opcode(0x06, 0x85), new[] { 0 } },
            { new Opcode(0x06, 0x87), new[] { 0 } },
            { new Opcode(0x06, 0x89), new[] { 0 } },
            { new Opcode(0x06, 0x8B), new[] { 0 } },

            { new Opcode(0x00, 0x86), new[] { 0 } },
            { new Opcode(0x00, 0x87), new[] { 0 } },
            { new Opcode(0x00, 0x8E), new[] { 0 } },
            { new Opcode(0x00, 0x8F), new[] { 0 } },
            { new Opcode(0x00, 0x96), new[] { 0 } },
            { new Opcode(0x00, 0x97), new[] { 0 } },
            { new Opcode(0x00, 0x9E), new[] { 0 } },
            { new Opcode(0x00, 0x9F), new[] { 0 } },

            { new Opcode(0x06, 0x03), new[] { 0 } },
            { new Opcode(0x06, 0x61), new[] { 0 } },
            { new Opcode(0x06, 0x63), new[] { 0 } },
        };
        /// <summary>
        /// All instructions that use the address in a pointer as a literal value instead of as an address.
        /// The pointer is always the second operand.
        /// </summary>
        internal static readonly HashSet<Opcode> pointerAddressAsLiteral = new()
        {
            new Opcode(0x03, 0x30), new Opcode(0x03, 0x31), new Opcode(0x03, 0x32),
        };

        /// <summary>
        /// Directives that result in data (non-code bytes) being inserted into the assembly.
        /// </summary>
        internal static readonly HashSet<string> dataInsertionDirectives = new(StringComparer.OrdinalIgnoreCase)
        {
            "%DAT", "%PAD", "%NUM", "%IBF",
            "DAT", "PAD", "NUM", "IBF"
        };
        /// <summary>
        /// Directives that take a literal name of an assembler variable as an operand, without the '@' prefix.
        /// Mapped to the 0-based index of the operand that is an unprefixed variable name.
        /// </summary>
        internal static readonly Dictionary<string, int> takesLiteralVariableName = new(StringComparer.OrdinalIgnoreCase)
        {
            { "%DEFINE", 0 },
            { "%UNDEFINE", 0 },
            { "%VAROP", 1 },
            { "%IF", 1 },
            { "%ELSE_IF", 1 },
        };
        /// <summary>
        /// All directives that take a literal operand for operating on assembler variables,
        /// mapped to the 0-based indexes of the operand that is the literal.
        /// </summary>
        internal static readonly Dictionary<string, int[]> assemblerVariableLiteral = new(StringComparer.OrdinalIgnoreCase)
        {
            { "%DEFINE", new[] { 1 } },
            { "%VAROP", new[] { 2 } },
            { "%IF", new[] { 1, 2 } },
            { "%ELSE_IF", new[] { 1, 2 } },
        };
        /// <summary>
        /// %VAROP/%IF directive operations (the first operand) that do not work as expected when given a negative literal as the third operand.
        /// </summary>
        internal static readonly HashSet<string> noNegativeVarop = new(StringComparer.OrdinalIgnoreCase) { "DIV", "REM", "SHL", "SHR", "GT", "GTE", "LT", "LTE" };
        /// <summary>
        /// Every opcode that results in the location of execution being moved to an address in memory.
        /// As of current, the address to jump to is always the first operand to these opcodes.
        /// </summary>
        internal static readonly HashSet<Opcode> jumpCallToAddressOpcodes = new()
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

            { new Opcode(0x02, 0x31), 1 },
            { new Opcode(0x02, 0x35), 2 },
            { new Opcode(0x02, 0x39), 1 },
        };
        /// <summary>
        /// All opcodes that allocate a literal value of bytes in memory. The literal value is always the second operand.
        /// </summary>
        internal static readonly HashSet<Opcode> allocationOfLiteral = new()
        {
            new Opcode(0x05, 0x01),
            new Opcode(0x05, 0x05),
            new Opcode(0x05, 0x11),
            new Opcode(0x05, 0x15),
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

        /// <summary>
        /// All opcodes that can operate as intended when given signed integer literals as an operand
        /// </summary>
        internal static readonly HashSet<Opcode> signedLiteralAccepting = new()
        {
            new Opcode(0x00, 0x11),  // ADD reg, lit
            new Opcode(0x00, 0x21),  // SUB reg, lit
            new Opcode(0x00, 0x31),  // MUL reg, lit
            new Opcode(0x00, 0x61),  // AND reg, lit
            new Opcode(0x00, 0x65),  // ORR reg, lit
            new Opcode(0x00, 0x69),  // XOR reg, lit
            new Opcode(0x00, 0x71),  // TST reg, lit
            new Opcode(0x00, 0x75),  // CMP reg, lit
            new Opcode(0x00, 0x81),  // MVB reg, lit
            new Opcode(0x00, 0x85),  // MVB adr, lit
            new Opcode(0x00, 0x87),  // MVB ptr, lit
            new Opcode(0x00, 0x89),  // MVW reg, lit
            new Opcode(0x00, 0x8D),  // MVW adr, lit
            new Opcode(0x00, 0x8F),  // MVW ptr, lit
            new Opcode(0x00, 0x91),  // MVD reg, lit
            new Opcode(0x00, 0x95),  // MVD adr, lit
            new Opcode(0x00, 0x97),  // MVD ptr, lit
            new Opcode(0x00, 0x99),  // MVQ reg, lit
            new Opcode(0x00, 0x9D),  // MVQ adr, lit
            new Opcode(0x00, 0x9F),  // MVQ ptr, lit
            new Opcode(0x00, 0xA1),  // PSH lit
            new Opcode(0x00, 0xB3),  // CAL adr, lit
            new Opcode(0x00, 0xB7),  // CAL ptr, lit
            new Opcode(0x00, 0xBC),  // RET lit
            new Opcode(0x00, 0xC9),  // WCX lit
            new Opcode(0x00, 0xD9),  // WFX lit

            new Opcode(0x01, 0x11),  // SIGN_DIV reg, lit
            new Opcode(0x01, 0x15),  // SIGN_DVR reg, lit
            new Opcode(0x01, 0x19),  // SIGN_REM reg, lit
            new Opcode(0x01, 0x31),  // SIGN_MVB reg, lit
            new Opcode(0x01, 0x35),  // SIGN_MVW reg, lit
            new Opcode(0x01, 0x41),  // SIGN_MVD reg, lit
            new Opcode(0x01, 0x51),  // SIGN_WCN lit
            new Opcode(0x01, 0x55),  // SIGN_WCB lit
            new Opcode(0x01, 0x61),  // SIGN_WFN lit
            new Opcode(0x01, 0x65),  // SIGN_WFB lit

            new Opcode(0x04, 0x32),  // ASMX_CAL lit

            new Opcode(0x06, 0x82),  // FSYS_SCT adr, lit
            new Opcode(0x06, 0x83),  // FSYS_SCT ptr, lit
            new Opcode(0x06, 0x86),  // FSYS_SMT adr, lit
            new Opcode(0x06, 0x87),  // FSYS_SMT ptr, lit
            new Opcode(0x06, 0x8A),  // FSYS_SAT adr, lit
            new Opcode(0x06, 0x8B),  // FSYS_SAT ptr, lit
        };
        /// <summary>
        /// All opcodes that can only operate as intended when given literals within the range of a signed 64-bit integer as an operand
        /// </summary>
        internal static readonly HashSet<Opcode> signedLiteralOnly = new()
        {
            new Opcode(0x01, 0x11),  // SIGN_DIV reg, lit
            new Opcode(0x01, 0x15),  // SIGN_DVR reg, lit
            new Opcode(0x01, 0x19),  // SIGN_REM reg, lit
            new Opcode(0x01, 0x31),  // SIGN_MVB reg, lit
            new Opcode(0x01, 0x35),  // SIGN_MVW reg, lit
            new Opcode(0x01, 0x41),  // SIGN_MVD reg, lit
            new Opcode(0x01, 0x51),  // SIGN_WCN lit
            new Opcode(0x01, 0x55),  // SIGN_WCB lit
            new Opcode(0x01, 0x61),  // SIGN_WFN lit
            new Opcode(0x01, 0x65),  // SIGN_WFB lit

            new Opcode(0x06, 0x82),  // FSYS_SCT adr, lit
            new Opcode(0x06, 0x83),  // FSYS_SCT ptr, lit
            new Opcode(0x06, 0x86),  // FSYS_SMT adr, lit
            new Opcode(0x06, 0x87),  // FSYS_SMT ptr, lit
            new Opcode(0x06, 0x8A),  // FSYS_SAT adr, lit
            new Opcode(0x06, 0x8B),  // FSYS_SAT ptr, lit
        };
        /// <summary>
        /// All opcodes that can operate as intended when given floating point literals as an operand
        /// </summary>
        internal static readonly HashSet<Opcode> floatLiteralAccepting = new()
        {
            new Opcode(0x00, 0x99),  // MVQ reg, lit
            new Opcode(0x00, 0x9D),  // MVQ adr, lit
            new Opcode(0x00, 0x9F),  // MVQ ptr, lit
            new Opcode(0x00, 0xA1),  // PSH lit
            new Opcode(0x00, 0xB3),  // CAL adr, lit
            new Opcode(0x00, 0xB7),  // CAL ptr, lit
            new Opcode(0x00, 0xBC),  // RET lit

            new Opcode(0x02, 0x01),  // FLPT_ADD reg, lit
            new Opcode(0x02, 0x11),  // FLPT_SUB reg, lit
            new Opcode(0x02, 0x21),  // FLPT_MUL reg, lit
            new Opcode(0x02, 0x31),  // FLPT_DIV reg, lit
            new Opcode(0x02, 0x35),  // FLPT_DVR reg, lit
            new Opcode(0x02, 0x39),  // FLPT_REM reg, lit
            new Opcode(0x02, 0x47),  // FLPT_PTN reg, lit
            new Opcode(0x02, 0x51),  // FLPT_POW reg, lit
            new Opcode(0x02, 0x61),  // FLPT_LOG reg, lit
            new Opcode(0x02, 0x71),  // FLPT_WCN reg, lit
            new Opcode(0x02, 0x81),  // FLPT_WFN reg, lit
            new Opcode(0x02, 0xD1),  // FLPT_CMP reg, lit

            new Opcode(0x04, 0x32),  // ASMX_CAL lit
        };
        /// <summary>
        /// All opcodes that can only operate as intended when given floating point literals as an operand
        /// </summary>
        internal static readonly HashSet<Opcode> floatLiteralOnly = new()
        {
            new Opcode(0x02, 0x01),  // FLPT_ADD reg, lit
            new Opcode(0x02, 0x11),  // FLPT_SUB reg, lit
            new Opcode(0x02, 0x21),  // FLPT_MUL reg, lit
            new Opcode(0x02, 0x31),  // FLPT_DIV reg, lit
            new Opcode(0x02, 0x35),  // FLPT_DVR reg, lit
            new Opcode(0x02, 0x39),  // FLPT_REM reg, lit
            new Opcode(0x02, 0x47),  // FLPT_PTN reg, lit
            new Opcode(0x02, 0x51),  // FLPT_POW reg, lit
            new Opcode(0x02, 0x61),  // FLPT_LOG reg, lit
            new Opcode(0x02, 0x71),  // FLPT_WCN reg, lit
            new Opcode(0x02, 0x81),  // FLPT_WFN reg, lit
            new Opcode(0x02, 0xD1),  // FLPT_CMP reg, lit
        };
        /// <summary>
        /// All opcodes that read a pointer as a floating point value. The pointer is always the second operand
        /// </summary>
        internal static readonly HashSet<Opcode> floatPointerRead = new()
        {
            new Opcode(0x02, 0x03),  // FLPT_ADD reg, ptr
            new Opcode(0x02, 0x13),  // FLPT_SUB reg, ptr
            new Opcode(0x02, 0x23),  // FLPT_MUL reg, ptr
            new Opcode(0x02, 0x33),  // FLPT_DIV reg, ptr
            new Opcode(0x02, 0x37),  // FLPT_DVR reg, ptr
            new Opcode(0x02, 0x3B),  // FLPT_REM reg, ptr
            new Opcode(0x02, 0x49),  // FLPT_PTN reg, ptr
            new Opcode(0x02, 0x53),  // FLPT_POW reg, ptr
            new Opcode(0x02, 0x63),  // FLPT_LOG reg, ptr
            new Opcode(0x02, 0x73),  // FLPT_WCN reg, ptr
            new Opcode(0x02, 0x83),  // FLPT_WFN reg, ptr
            new Opcode(0x02, 0xD3),  // FLPT_CMP reg, ptr
        };

        /// <summary>
        /// All opcodes that require a valid terminal colour literal
        /// </summary>
        internal static readonly HashSet<Opcode> terminalColorInstructions = new()
        {
            new Opcode(0x07, 0x51),
            new Opcode(0x07, 0x55)
        };
    }
}
