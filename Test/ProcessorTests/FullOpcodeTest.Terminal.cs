namespace AssEmbly.Test.ProcessorTests
{
    public static partial class FullOpcodeTest
    {
        [TestClass]
        public class TerminalExtensionSet
        {
            [TestMethod]
            public void TERM_AEE()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x10 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual((ulong)StatusFlags.AutoEcho, testProcessor.Registers[(int)Register.rsf], "Instruction did not set auto-echo flag");
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_AED()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x11 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(~(ulong)StatusFlags.AutoEcho, testProcessor.Registers[(int)Register.rsf], "Instruction did not unset auto-echo flag");
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_BEP()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x40 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SFC_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x50 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SFC_Literal()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x51 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SFC_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x52, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SFC_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x53 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SBC_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x54 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SBC_Literal()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x55 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SBC_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x56, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_SBC_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x57 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }

            [TestMethod]
            public void TERM_RSC()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x07, 0x58 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
            }
        }
    }
}
