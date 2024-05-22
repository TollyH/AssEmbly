namespace AssEmbly.Test.ProcessorTests
{
    public static partial class FullOpcodeTest
    {
        [TestClass]
        public class ExtendedBaseSet
        {
            [TestMethod]
            public void EXTD_BSW_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x1122334455667788;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x00, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8877665544332211, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x123456789ABCDEF0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x00, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0xF0DEBC9A78563412, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_QPF_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x10, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)AAPFeatures.All, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_QPV_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x11, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(typeof(Processor).Assembly.GetName().Version?.Major, (int)testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_QPV_Register_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x12, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(typeof(Processor).Assembly.GetName().Version?.Major, (int)testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(typeof(Processor).Assembly.GetName().Version?.Minor, (int)testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_CSS_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x13, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(16UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_HLT_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x10203040;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x20, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203040, Environment.ExitCode, "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_HLT_Literal()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x21, 0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203040, Environment.ExitCode, "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_HLT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x22, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                testProcessor.WriteMemoryQWord(0x140, 0x10203040);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203040, Environment.ExitCode, "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_HLT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x03, 0x23, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(0x140, 0x10203040);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203040, Environment.ExitCode, "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_MPA_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x30, (int)Register.rg6,
                    (int)Register.rg7
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x140UL, testProcessor.Registers[(int)Register.rg6], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x30, (int)Register.rg6,
                    ((int)DisplacementMode.Constant << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203180UL, testProcessor.Registers[(int)Register.rg6], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x30, (int)Register.rg6,
                    ((int)DisplacementMode.Register << 6) | (int)Register.rg7,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x204061C0UL, testProcessor.Registers[(int)Register.rg6], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x30, (int)Register.rg6,
                    ((int)DisplacementMode.ConstantAndRegister << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x30609200UL, testProcessor.Registers[(int)Register.rg6], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_MPA_Address_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x31, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    (int)Register.rg7
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x140UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x31, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMode.Constant << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(20UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203180UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x31, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMode.Register << 6) | (int)Register.rg7,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x204061C0UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x31, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMode.ConstantAndRegister << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(21UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x30609200UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void EXTD_MPA_Pointer_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg6] = 0x140;
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x32, (int)Register.rg6,
                    (int)Register.rg7
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x140UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg6] = 0x140;
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x32, (int)Register.rg6,
                    ((int)DisplacementMode.Constant << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x10203180UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg6] = 0x140;
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x32, (int)Register.rg6,
                    ((int)DisplacementMode.Register << 6) | (int)Register.rg7,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x204061C0UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg6] = 0x140;
                testProcessor.Registers[(int)Register.rg7] = 0x140;
                testProcessor.Registers[(int)Register.rg8] = 0x10203040;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x03, 0x32, (int)Register.rg6,
                    ((int)DisplacementMode.ConstantAndRegister << 6) | (int)Register.rg7,
                    0x40, 0x30, 0x20, 0x10, 0x00, 0x00, 0x00, 0x00,
                    ((int)DisplacementMultiplier.x2 << 4) | (int)Register.rg8
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x30609200UL, testProcessor.ReadMemoryQWord(0x140), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }
        }
    }
}
