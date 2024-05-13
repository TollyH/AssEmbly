#if ASSEMBLER_WARNINGS && !ASSEMBLER
#error Compiling ASSEMBLER_WARNINGS requires that ASSEMBLER is also compiled.
#endif

#if DEBUGGER && (!CLI || !PROCESSOR || !ASSEMBLER || !DISASSEMBLER)
#error Compiling DEBUGGER requires that CLI, PROCESSOR, ASSEMBLER, and DISASSEMBLER are also compiled.
#endif
