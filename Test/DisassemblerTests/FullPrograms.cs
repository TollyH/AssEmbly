namespace AssEmbly.Test.DisassemblerTests
{
    [TestClass]
    public class FullPrograms
    {
        [TestMethod]
        public void StringsPadsNoFullBase()
        {
            string program = Disassembler.DisassembleProgram(File.ReadAllBytes("KitchenSink.bin"),
                true, true, false);

            Assert.AreEqual(File.ReadAllText("KitchenSink.Disassembled.asm"), program,
                "The disassembly process produced unexpected output");

            Assembler asm = new("");
            asm.AssembleLines(program.Split('\n'));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "Reassembling the disassembled program produced unexpected program bytes");
        }

        [TestMethod]
        public void NoStringsPadsNoFullBase()
        {
            string program = Disassembler.DisassembleProgram(File.ReadAllBytes("KitchenSink.bin"),
                false, true, false);

            Assert.AreEqual(File.ReadAllText("KitchenSink.Disassembled.NoStrings.asm"), program,
                "The disassembly process produced unexpected output");

            Assembler asm = new("");
            asm.AssembleLines(program.Split('\n'));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "Reassembling the disassembled program produced unexpected program bytes");
        }

        [TestMethod]
        public void StringsNoPadsNoFullBase()
        {
            string program = Disassembler.DisassembleProgram(File.ReadAllBytes("KitchenSink.bin"),
                true, false, false);

            Assert.AreEqual(File.ReadAllText("KitchenSink.Disassembled.NoPads.asm"), program,
                "The disassembly process produced unexpected output");

            Assembler asm = new("");
            asm.AssembleLines(program.Split('\n'));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "Reassembling the disassembled program produced unexpected program bytes");
        }

        [TestMethod]
        public void StringsPadsFullBase()
        {
            string program = Disassembler.DisassembleProgram(File.ReadAllBytes("KitchenSink.bin"),
                true, true, true);

            Assert.AreEqual(File.ReadAllText("KitchenSink.Disassembled.FullBase.asm"), program,
                "The disassembly process produced unexpected output");
        }
    }
}
