namespace AssEmbly.Test.ProcessorTests
{
    public partial class FullOpcodeTest
    {
        [TestClass]
        public class MemoryAllocationExtensionSet
        {
            [TestMethod]
            public void HEAP_ALC_Register_Register()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 16;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x00, (int)Register.rg2, (int)Register.rg3,
                    0xFF, 0x05, 0x00, (int)Register.rg4, (int)Register.rg3
                });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(16UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 1024;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x00, (int)Register.rg2, (int)Register.rg3
                });
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");
            }

            [TestMethod]
            public void HEAP_ALC_Register_Literal()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x01, (int)Register.rg2, 16, 0, 0, 0, 0, 0, 0, 0,
                    0xFF, 0x05, 0x01, (int)Register.rg4, 16, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(25UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(24UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(40UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x01, (int)Register.rg2, 0, 4, 0, 0, 0, 0, 0, 0
                });
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");
            }

            [TestMethod]
            public void HEAP_ALC_Register_Address()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x02, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0,
                    0xFF, 0x05, 0x02, (int)Register.rg4, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 16);
                _ = testProcessor.Execute(true);
                Assert.AreEqual(25UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(24UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(40UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x02, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");
            }

            [TestMethod]
            public void HEAP_ALC_Register_Pointer()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x03, (int)Register.rg2, (int)Register.rg3,
                    0xFF, 0x05, 0x03, (int)Register.rg4, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 16);
                _ = testProcessor.Execute(true);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x03, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");
            }

            [TestMethod]
            public void HEAP_TRY_Register_Register()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 16;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x04, (int)Register.rg2, (int)Register.rg3,
                    0xFF, 0x05, 0x04, (int)Register.rg4, (int)Register.rg3
                });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(16UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 1024;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x04, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(1024UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRY_Register_Literal()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x05, (int)Register.rg2, 16, 0, 0, 0, 0, 0, 0, 0,
                    0xFF, 0x05, 0x05, (int)Register.rg4, 16, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(25UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(24UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(40UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x05, (int)Register.rg2, 0, 4, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRY_Register_Address()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x06, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0,
                    0xFF, 0x05, 0x06, (int)Register.rg4, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 16);
                _ = testProcessor.Execute(true);
                Assert.AreEqual(25UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(24UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(40UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x06, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRY_Register_Pointer()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x07, (int)Register.rg2, (int)Register.rg3,
                    0xFF, 0x05, 0x07, (int)Register.rg4, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 16);
                _ = testProcessor.Execute(true);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x07, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_REA_Register_Register()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 8;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x10, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(8UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 1024;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x10, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.Registers[(int)Register.rg3] = 8;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x10, (int)Register.rg2, (int)Register.rg3
                });
                _ = Assert.ThrowsException<InvalidMemoryBlockException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when trying to re-allocate invalid/non-existent block");
            }

            [TestMethod]
            public void HEAP_REA_Register_Literal()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x11, (int)Register.rg2, 8, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x11, (int)Register.rg2, 0, 4, 0, 0, 0, 0, 0, 0
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x11, (int)Register.rg2, 8, 0, 0, 0, 0, 0, 0, 0
                });
                _ = Assert.ThrowsException<InvalidMemoryBlockException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when trying to re-allocate invalid/non-existent block");
            }

            [TestMethod]
            public void HEAP_REA_Register_Address()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x12, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x12, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x12, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 8);
                _ = Assert.ThrowsException<InvalidMemoryBlockException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when trying to re-allocate invalid/non-existent block");
            }

            [TestMethod]
            public void HEAP_REA_Register_Pointer()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x13, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(48UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x13, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = Assert.ThrowsException<MemoryAllocationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when allocating more memory than available");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x13, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 8);
                _ = Assert.ThrowsException<InvalidMemoryBlockException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when trying to re-allocate invalid/non-existent block");
            }

            [TestMethod]
            public void HEAP_TRE_Register_Register()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 8;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x14, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(8UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 1024;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x14, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(1024UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.Registers[(int)Register.rg3] = 8;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x14, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRE_Register_Literal()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x15, (int)Register.rg2, 8, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x15, (int)Register.rg2, 0, 4, 0, 0, 0, 0, 0, 0
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x15, (int)Register.rg2, 8, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRE_Register_Address()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x16, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x16, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x16, (int)Register.rg2, 48, 0, 0, 0, 0, 0, 0, 0
                });
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_TRE_Register_Pointer()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x17, (int)Register.rg2, (int)Register.rg3
                });
                _ = testProcessor.AllocateMemory(16);
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                _ = testProcessor.FreeMemory(testProcessor.AllocateMemory(16));
                ulong initial = testProcessor.Registers[(int)Register.rg2];
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(initial, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(8L, testProcessor.MappedMemoryRanges[2].Length, "Instruction did not produce correct result");
                Assert.AreEqual(48UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x17, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg2] = testProcessor.AllocateMemory(16);
                testProcessor.WriteMemoryQWord(48, 1024);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(48UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.Registers[(int)Register.rg2] = 0;
                testProcessor.Registers[(int)Register.rg3] = 48;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x17, (int)Register.rg2, (int)Register.rg3
                });
                testProcessor.WriteMemoryQWord(48, 8);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg2], "Instruction did not produce correct result");
                Assert.AreEqual(48UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void HEAP_FRE_Register()
            {
                Processor testProcessor = new(64);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x20, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg3] = testProcessor.AllocateMemory(16);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2, testProcessor.MappedMemoryRanges.Count, "Instruction did not produce correct result");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rg3], "Instruction updated the operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new Processor(64);
                testProcessor.LoadProgram(new byte[]
                {
                    0xFF, 0x05, 0x20, (int)Register.rg3
                });
                testProcessor.Registers[(int)Register.rg3] = testProcessor.AllocateMemory(16) + 6;
                _ = Assert.ThrowsException<InvalidMemoryBlockException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when trying to free invalid/non-existent block");
            }
        }
    }
}
