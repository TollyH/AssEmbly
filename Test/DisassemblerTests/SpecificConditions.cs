namespace AssEmbly.Test.DisassemblerTests
{
    [TestClass]
    public class SpecificConditions
    {
        private static readonly DisassemblerOptions disassemblerOptions = new()
        {
            DetectStrings = false
        };

        [TestMethod]
        public void EmptyProgram()
        {
            string result = Disassembler.DisassembleProgram(Array.Empty<byte>(), disassemblerOptions);
            Assert.AreEqual(string.Empty, result, "Providing empty program did not produce empty result");

            (string line, ulong additionalOffset, List<ulong> references, bool datFallback) = Disassembler.DisassembleInstruction(
                Span<byte>.Empty, disassemblerOptions, false);
            Assert.AreEqual(string.Empty, line, "Providing empty instruction did not produce empty line content");
            Assert.AreEqual(0UL, additionalOffset, "Providing empty instruction did not produce 0 additional offset");
            Assert.AreEqual(0, references.Count, "Providing empty instruction produced unexpected address references");
            Assert.IsFalse(datFallback, "Providing empty instruction produced unexpected %DAT fallback");
        }

        [TestMethod]
        public void MissingOperands()
        {
            string result = Disassembler.DisassembleProgram(new byte[] { 0x10 }, disassemblerOptions);
            Assert.AreEqual("%DAT 16", result, "Providing opcode without operands did not produce correct program");

            (string line, ulong additionalOffset, List<ulong> references, bool datFallback) = Disassembler.DisassembleInstruction(
                new byte[] { 0x12 }, disassemblerOptions, false);
            Assert.AreEqual("%DAT 18", line, "Providing opcode without operands did not produce correct line content");
            Assert.AreEqual(1UL, additionalOffset, "Providing opcode without operands did not produce correct additional offset");
            Assert.AreEqual(0, references.Count, "Providing opcode without operands produced unexpected address references");
            Assert.IsTrue(datFallback, "Providing opcode without operands did not produce %DAT fallback");
        }

        [TestMethod]
        public void TruncatedLiteralOperand()
        {
            string result = Disassembler.DisassembleProgram(
                new byte[] { 0x11, 0x06, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, disassemblerOptions);
            Assert.AreEqual("""
                %DAT 17
                %DAT 6
                %DAT 17
                %DAT 34
                %DAT 51
                %DAT 68
                %DAT 85
                %DAT 102
                %DAT 119
                """, result, "Providing opcode with truncated literal operand did not produce correct program");

            (string line, ulong additionalOffset, List<ulong> references, bool datFallback) = Disassembler.DisassembleInstruction(
                new byte[] { 0x11, 0x06, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, disassemblerOptions, false);
            Assert.AreEqual("%DAT 17", line, "Providing opcode with truncated literal operand did not produce correct line content");
            Assert.AreEqual(1UL, additionalOffset, "Providing opcode with truncated literal operand did not produce correct additional offset");
            Assert.AreEqual(0, references.Count, "Providing opcode with truncated literal operand produced unexpected address references");
            Assert.IsTrue(datFallback, "Providing opcode with truncated literal operand did not produce %DAT fallback");
        }

        [TestMethod]
        public void TruncatedAddressOperand()
        {
            string result = Disassembler.DisassembleProgram(
                new byte[] { 0x12, 0x06, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, disassemblerOptions);
            Assert.AreEqual("""
                %DAT 18
                %DAT 6
                %DAT 17
                %DAT 34
                %DAT 51
                %DAT 68
                %DAT 85
                %DAT 102
                %DAT 119
                """, result, "Providing opcode with truncated literal operand did not produce correct program");

            (string line, ulong additionalOffset, List<ulong> references, bool datFallback) = Disassembler.DisassembleInstruction(
                new byte[] { 0x12, 0x06, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, disassemblerOptions, false);
            Assert.AreEqual("%DAT 18", line, "Providing opcode with truncated literal operand did not produce correct line content");
            Assert.AreEqual(1UL, additionalOffset, "Providing opcode with truncated literal operand did not produce correct additional offset");
            Assert.AreEqual(0, references.Count, "Providing opcode with truncated literal operand produced unexpected address references");
            Assert.IsTrue(datFallback, "Providing opcode with truncated literal operand did not produce %DAT fallback");
        }

        [TestMethod]
        public void InvalidPointers()
        {
            string result = Disassembler.DisassembleProgram(
                new byte[] { 0x13, 0x06, 0x47, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, disassemblerOptions);
            Assert.AreEqual("""
                %DAT 19
                %DAT 6
                %DAT 71
                %DAT 34
                %DAT 51
                %DAT 68
                %DAT 85
                %DAT 102
                %DAT 119
                """, result, "Providing opcode with truncated pointer operand did not produce correct program");

            result = Disassembler.DisassembleProgram(
                new byte[] { 0x13, 0x06, 0xC7, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 }, disassemblerOptions);
            Assert.AreEqual("""
                %DAT 19
                JNE :0x88776655443322C7  ; Address does not align to a disassembled instruction
                """, result, "Providing opcode with truncated pointer operand did not produce correct program");
        }
    }
}
