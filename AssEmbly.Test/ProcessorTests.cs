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
                Assert.IsTrue(testProcessor.Execute(false), "Instruction did not cause Execute to return true");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1 });
                Assert.IsFalse(testProcessor.Execute(false), "Execute returned false when HLT was not run");

                // Repeat execution should stop when HLT instruction is run
                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1, 0, 1 });
                _ = testProcessor.Execute(true);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not end program execution");
            }

            [TestMethod]
            public void NOP()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 1 });
                _ = testProcessor.Execute(false);
                Processor defaultProcessor = new(2046);
                defaultProcessor.Memory[0] = 1;

                Assert.IsTrue(testProcessor.Memory.SequenceEqual(defaultProcessor.Memory), "Instruction instruction affected process memory");
                // Set default rpo to expected value for test rpo
                defaultProcessor.Registers[(int)Register.rpo] = 1;
                Assert.IsTrue(testProcessor.Registers.SequenceEqual(defaultProcessor.Registers), "Instruction instruction affected registers");
            }

            [TestMethod]
            public void JMP_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 2, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JMP_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg0] = 0x0102030405060708UL;
                testProcessor.LoadProgram(new byte[] { 3, (int)Register.rg0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JEQ_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 4, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 4, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JEQ_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 5, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 5, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JNE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 6, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 6, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JNE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg2] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 7, (int)Register.rg2 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg2] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 7, (int)Register.rg2 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 8, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 8, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg3] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 9, (int)Register.rg3 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg3] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 9, (int)Register.rg3 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.LoadProgram(new byte[] { 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JLE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg4] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0B, (int)Register.rg4 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JGT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.LoadProgram(new byte[] { 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JGT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg5] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0D, (int)Register.rg5 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void JGE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.LoadProgram(new byte[] { 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void JGE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg6] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0F, (int)Register.rg6 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Carry;
                testProcessor.Registers[(int)Register.rg6] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0x0F, (int)Register.rg6 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x0807060504030201UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x0807060504030201UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0807060504030201UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0909090909090909UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0807060504030201UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0102030405060709UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x14, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0101010101010101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0102030405060707UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFFFFFFFUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 1;
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x24, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(34141125200427UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(3), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rpo, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg9, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rpo, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg9, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x47, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
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
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 3456789UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 987654321UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHL_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101;
                testProcessor.Registers[(int)Register.rg8] = 6;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101000000UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 63;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 65;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.Registers[(int)Register.rg8] = 57;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 64;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHL_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 6, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101000000UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x3F, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x41, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x39, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x40, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHL_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101000000UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHL_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101000000UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHR_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.Registers[(int)Register.rg8] = 6;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 63;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 65;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 57;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 64;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHR_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 6, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x3F, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x41, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x39, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x40, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHR_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SHR_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 6UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 63UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 65UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 57UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 64UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void AND_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 0b11000101;
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10000100UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void AND_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg7, 0b11000101, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10000100UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void AND_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10000100UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void AND_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10000100UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void ORR_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 0b11000101;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11101101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void ORR_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg7, 0b11000101, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11101101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void ORR_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11101101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void ORR_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11101101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void XOR_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 0b11000101;
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b01101001UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void XOR_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg7, 0b11000101, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b01101001UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void XOR_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b01101001UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void XOR_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b01101001UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void NOT_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x6C, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-173), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x6C, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x6C, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234566UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 255;
                testProcessor.LoadProgram(new byte[] { 0x6C, (int)Register.rg9 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-256), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x6C, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void RNG_Register()
            {
                Processor testProcessor = new(2046);
                // Overwrite the processor's random number generator to one with a known seed, so we can test expected values
                typeof(Processor).GetField("rng", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, new Random(123456789));
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.LoadProgram(new byte[] { 0x6D, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2249533549807962317UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                // Overwrite the processor's random number generator to a dummy one that will always generate 0's
                typeof(Processor).GetField("rng", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, new AllZeroRandom());
                testProcessor.LoadProgram(new byte[] { 0x6D, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                // Overwrite the processor's random number generator to one with a known seed, so we can test expected values
                typeof(Processor).GetField("rng", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, new Random(-2147373325));
                testProcessor.LoadProgram(new byte[] { 0x6D, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9498905931710079094UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x6D, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void TST_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 0b11000101;
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101100UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void TST_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg7, 0b11000101, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101100UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void TST_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101100UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void TST_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b10101100;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b11000101);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101100UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0b11000101UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-87654), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 255UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void CMP_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.Registers[(int)Register.rg8] = 0x0706050403020100;
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0807060504030201UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void CMP_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0807060504030201UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(2), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void CMP_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0807060504030201UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void CMP_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0x0807060504030201;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x0706050403020100);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0807060504030201UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x0706050403020100UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 1234567UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-42), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), unchecked((ulong)-1234567), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 552UL, "Instruction updated the second operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), 0x8000000000000000UL, "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
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

        private class AllZeroRandom : Random
        {
            public override long NextInt64(long minValue, long maxValue)
            {
                return 0;
            }
        }
    }
}