namespace AssEmbly.Test
{
    public class ProcessorTests
    {
        [TestClass]
        public class FullOpcodeTest
        {
            [TestMethod]
            public void HLT()
            {
                // Execute should return true if HLT instruction was run
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0 });
                Assert.IsTrue(testProcessor.Execute(false), "HLT did not cause Execute to return true");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1 });
                Assert.IsFalse(testProcessor.Execute(false), "Execute returned false when HLT was not run");

                // Repeat execution should stop when HLT instruction is run
                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1, 0, 1 });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "HLT did not end program execution");
            }

            [TestMethod]
            public void NOP()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1 });
                _ = testProcessor.Execute(false);
                Processor defaultProcessor = new(2046);
                defaultProcessor.Memory[0] = 1;

                Assert.IsTrue(testProcessor.Memory.SequenceEqual(defaultProcessor.Memory), "NOP instruction affected process memory");
                // Set default rpo to expected value for test rpo
                defaultProcessor.Registers[(int)Register.rpo] = 1;
                Assert.IsTrue(testProcessor.Registers.SequenceEqual(defaultProcessor.Registers), "NOP instruction affected registers");
            }

            [TestMethod]
            public void JMP_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 2, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JMP did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JMP_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg0] = 0x0102030405060708UL;
                testProcessor.LoadProgram(new byte[] { 3, (int)Register.rg0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JMP did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JEQ_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 4, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JEQ did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 4, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JEQ updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JEQ_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 5, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JEQ did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 5, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JEQ updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JNE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 6, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JNE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 6, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JNE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JNE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg2] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 7, (int)Register.rg2 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JNE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg2] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 7, (int)Register.rg2 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JNE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 8, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLT did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 8, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JLT updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg3] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 9, (int)Register.rg3 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLT did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg3] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 9, (int)Register.rg3 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JLT updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JLE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JLE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JLE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JGT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JGT updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JGT updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JGT did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JGT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JGT updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JGT updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JGT did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JGE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JGE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "JGE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JGE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg6] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0F, (int)Register.rg6 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "JGE did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg6] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0F, (int)Register.rg6 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "JGE updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void ADD_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.Registers[(int)Register.rg8] = 0x0807060504030201;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x0807060504030201UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "ADD updated the second operand");
            }

            [TestMethod]
            public void ADD_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2, 0x0807060504030201);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x0807060504030201UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "ADD updated the second operand");
            }

            [TestMethod]
            public void ADD_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x0807060504030201);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0807060504030201UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "ADD updated the second operand");
            }

            [TestMethod]
            public void ADD_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x0807060504030201);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "ADD updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0807060504030201UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "ADD updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "ADD updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "ADD updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "ADD updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "ADD updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "ADD did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "ADD did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "ADD updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "ADD updated the second operand");
            }

            [TestMethod]
            public void ICR_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "ICR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0102030405060709UL, testProcessor.Registers[(int)Register.rg7], "ICR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "ICR did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "ICR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "ICR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "ICR did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "ICR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "ICR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "ICR did not correctly set status flags");
            }

            [TestMethod]
            public void SUB_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.Registers[(int)Register.rg8] = 0x0706050403020100;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x0706050403020100UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "SUB updated the second operand");
            }

            [TestMethod]
            public void SUB_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x0706050403020100UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "SUB updated the second operand");
            }

            [TestMethod]
            public void SUB_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "SUB updated the second operand");
            }

            [TestMethod]
            public void SUB_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "SUB updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "SUB updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "SUB updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "SUB updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "SUB updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "SUB updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "SUB did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "SUB did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "SUB updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "SUB updated the second operand");
            }

            [TestMethod]
            public void DCR_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "DCR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0102030405060707UL, testProcessor.Registers[(int)Register.rg7], "DCR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DCR did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "DCR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFFFFFFFUL, testProcessor.Registers[(int)Register.rg9], "DCR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "DCR did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "DCR updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "DCR did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "DCR did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 1;
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "DCR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DCR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DCR did not correctly set status flags");
            }

            [TestMethod]
            public void MUL_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "MUL updated the second operand");
            }

            [TestMethod]
            public void MUL_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "MUL updated the second operand");
            }

            [TestMethod]
            public void MUL_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "MUL updated the second operand");
            }

            [TestMethod]
            public void MUL_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "MUL updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "MUL updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "MUL did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "MUL did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "MUL updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "MUL updated the second operand");
            }

            [TestMethod]
            public void DIV_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DIV by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DIV updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DIV updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "DIV updated the second operand");
            }

            [TestMethod]
            public void DIV_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 987654321UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), ulong.MaxValue, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DIV by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DIV updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DIV updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "DIV updated the second operand");
            }

            [TestMethod]
            public void DIV_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DIV by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DIV updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DIV updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "DIV updated the second operand");
            }

            [TestMethod]
            public void DIV_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DIV updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DIV updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DIV updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DIV did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DIV did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DIV updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "DIV updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DIV by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DIV updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DIV updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DIV updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DIV updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "DIV updated the second operand");
            }

            [TestMethod]
            public void DVR_Register_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DVR by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DVR updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "DVR updated the second operand");
            }

            [TestMethod]
            public void DVR_Register_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 3456789UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 987654321UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 1UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), ulong.MaxValue, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DVR by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DVR updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 0UL, "DVR updated the second operand");
            }

            [TestMethod]
            public void DVR_Register_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DVR by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DVR updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "DVR updated the second operand");
            }

            [TestMethod]
            public void DVR_Register_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DVR updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DVR updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DVR updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "DVR did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "DVR did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "DVR did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DVR updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "DVR updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "DVR by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "DVR updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "DVR updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "DVR updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "DVR updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "DVR updated the second operand");
            }

            [TestMethod]
            public void REM_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "REM by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "REM updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "REM updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "REM updated the second operand");
            }

            [TestMethod]
            public void REM_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 987654321UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), ulong.MaxValue, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "REM by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "REM updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "REM updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "REM updated the second operand");
            }

            [TestMethod]
            public void REM_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "REM by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "REM updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "REM updated status flags after exception");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "REM updated the second operand");
            }

            [TestMethod]
            public void REM_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "REM updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "REM updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "REM updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "REM did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "REM did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "REM updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "REM updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "REM by 0 didn't throw DivideByZeroException");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rpo], "REM updated the rpo register by an incorrect amount after exception");
                Assert.AreEqual(9876543210UL, testProcessor.Registers[(int)Register.rg7], "REM updated the result register after exception");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "REM updated status flags after exception");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "REM updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "REM updated the second operand");
            }

            [TestMethod]
            public void SHL_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHL_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHL_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHL_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHR_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHR_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHR_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SHR_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void AND_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void AND_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void AND_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void AND_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void ORR_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void ORR_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void ORR_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void ORR_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void XOR_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void XOR_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void XOR_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void XOR_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void NOT_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RNG_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void TST_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void TST_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void TST_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void TST_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CMP_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CMP_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CMP_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CMP_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVB_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVW_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVD_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void MVQ_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void PSH_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void PSH_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void PSH_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void PSH_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void POP_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Address_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Address_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Pointer_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CAL_Pointer_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RET()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RET_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RET_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RET_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RET_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCB_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCB_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCB_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCB_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCX_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCX_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCX_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCX_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCC_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCC_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCC_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WCC_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFB_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFB_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFB_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFB_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFX_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFX_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFX_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFX_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFC_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFC_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFC_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void WFC_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void OFL_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void OFL_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void CFL()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void DFL_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void DFL_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FEX_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FEX_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSZ_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSZ_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RCC_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void RFC_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JLT_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JLT_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JLE_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JLE_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JGT_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JGT_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JGE_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JGE_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JSI_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JSI_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JNS_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JNS_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JOV_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JOV_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JNO_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_JNO_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DIV_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DIV_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DIV_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DIV_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_REM_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_REM_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_REM_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_REM_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_SHR_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_SHR_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_SHR_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_SHR_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVB_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVB_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVB_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVB_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVW_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVW_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVW_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVW_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVD_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVD_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVD_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_MVD_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCB_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCB_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCB_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WCB_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFB_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFB_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFB_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_WFB_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_EXB_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_EXW_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_EXD_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void SIGN_NEG_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ADD_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ADD_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ADD_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ADD_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SUB_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SUB_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SUB_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SUB_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_MUL_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_MUL_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_MUL_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_MUL_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DIV_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DIV_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DIV_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DIV_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_REM_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_REM_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_REM_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_REM_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SIN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ASN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_COS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ACS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_TAN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_ATN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_PTN_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_PTN_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_PTN_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_PTN_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_POW_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_POW_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_POW_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_POW_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_LOG_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_LOG_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_LOG_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_LOG_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WCN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WCN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WCN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WCN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WFN_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WFN_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WFN_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_WFN_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_EXH_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_EXS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SHS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_SHH_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_NEG_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_UTF_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_STF_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_FTS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_FCS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_FFS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_FNS_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_CMP_Register_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_CMP_Register_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_CMP_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FLPT_CMP_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void EXTD_BSW_Register()
            {
                throw new NotImplementedException();
            }
        }
    }
}