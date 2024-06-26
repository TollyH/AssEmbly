using System.Text;

namespace AssEmbly.Test.ProcessorTests
{
    public static partial class FullOpcodeTest
    {
        [TestClass]
        public class BaseInstructionSet
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
                testProcessor.Registers[(int)Register.rg1] = 0x20UL;
                testProcessor.LoadProgram(new byte[] { 3, ((int)DisplacementMode.Register << 6) | (int)Register.rg0, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060728UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
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
                testProcessor.Registers[(int)Register.rg2] = 0x20UL;
                testProcessor.LoadProgram(new byte[] { 5, ((int)DisplacementMode.Register << 6) | (int)Register.rg1, (int)Register.rg2 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060728UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

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
                testProcessor.Registers[(int)Register.rg3] = 0x20UL;
                testProcessor.LoadProgram(new byte[] { 7, ((int)DisplacementMode.Register << 6) | (int)Register.rg2, (int)Register.rg3 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060728UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

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
                Assert.AreEqual(0x0807060504030201UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0x0807060504030201UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0x0807060504030201UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg7, ((int)PointerReadSize.DoubleWord << 4) | (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x0807060504030201);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x0102030409090909UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x0807060504030201UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.SignAndOverflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(14UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223372036853541241UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.ZeroAndCarry | StatusFlags.Overflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x7FFFFFFFFFED2979UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(98UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x800000000012D686UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x30, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x31, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x32, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 9223372036853541241;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(9223370512699098319UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-2352), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 142536475869;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x33, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x41, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x43, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x44, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(3), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(3), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(3), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x45, (int)Register.rg7, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(3), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x46, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x48, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x49, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.LoadProgram(new byte[] { 0x4A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = long.MaxValue + 1UL;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0x4B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(long.MaxValue + 1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 63;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 65;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.Registers[(int)Register.rg8] = 57;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 64;
                testProcessor.LoadProgram(new byte[] { 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x3F, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x41, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x39, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x51, (int)Register.rg7, 0x40, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b10000001;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(144115188075855872UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x53, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 63;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 65;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 57;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 64;
                testProcessor.LoadProgram(new byte[] { 0x54, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x3F, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x41, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x39, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x55, (int)Register.rg7, 0x40, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0x56, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x57, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x60, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x61, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x62, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1300200), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x63, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(80UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x64, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x65, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x66, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-22021), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x67, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x68, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x69, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x6A, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1278179UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x6B, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-81), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-87654);
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 255;
                testProcessor.LoadProgram(new byte[] { 0x70, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 0x9A, 0xA9, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x71, (int)Register.rg9, 255, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.LoadProgram(new byte[] { 0x72, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b11000101UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-1234567);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-87654));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-87654), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)-176);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x73, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 255);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-176), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(255UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 1234567;
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-42);
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = unchecked((ulong)-1234567);
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x74, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x75, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(2, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

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
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.LoadProgram(new byte[] { 0x76, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

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
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x0706050403020100UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 1234567);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)long.MinValue), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Overflow, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1234567UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 56;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-42));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(56UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-42), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = long.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, unchecked((ulong)-1234567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ulong)long.MaxValue, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.SignAndOverflow), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(unchecked((ulong)-1234567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = 0x8000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0x8000000000000000);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0x8000000000000000UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x77, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void MVB_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = byte.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x80, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x80, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x80, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010;
                testProcessor.LoadProgram(new byte[] { 0x80, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x80, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVB_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x81, (int)Register.rg7, 0xFF, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x81, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x81, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x81, (int)Register.rg7, 0b10101010, 0b1010, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x81, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVB_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x82, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, byte.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x82, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x82, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x82, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x82, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVB_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x83, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, byte.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x83, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x83, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x83, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x83, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVB_Address_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = byte.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x84, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x84, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x84, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010;
                testProcessor.LoadProgram(new byte[] { 0x84, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b11001101;
                testProcessor.LoadProgram(new byte[] { 0x84, 0xFD, 0x07, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                // Ensure that writing a byte at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((byte)0b11001101, testProcessor.Memory[2045], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((byte)0b11001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVB_Address_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x85, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x85, 0x28, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x85, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x85, 0x28, 2, 0, 0, 0, 0, 0, 0, 0b10101010, 0b1010, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x85, 0xFD, 0x07, 0, 0, 0, 0, 0, 0, 0b11001101, 0, 0, 0, 0, 0, 0, 0 });
                // Ensure that writing a byte at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((byte)0b11001101, testProcessor.Memory[2045], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((byte)0b11001101, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVB_Pointer_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = byte.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x86, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x86, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x86, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x86, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b11001101;
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0x86, (int)Register.rg7, (int)Register.rg8 });
                // Ensure that writing a byte at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((byte)0b11001101, testProcessor.Memory[2045], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((byte)0b11001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVB_Pointer_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x87, (int)Register.rg7, 0xFF, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x87, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x87, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x87, (int)Register.rg7, 0b10101010, 0b1010, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0x87, (int)Register.rg7, 0b11001101, 0, 0, 0, 0, 0, 0, 0 });
                // Ensure that writing a byte at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((byte)0b11001101, testProcessor.Memory[2045], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((byte)0b11001101, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVW_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ushort.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x88, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x88, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x88, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b10101010101010101010;
                testProcessor.LoadProgram(new byte[] { 0x88, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x88, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVW_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x89, (int)Register.rg7, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x89, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x89, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x89, (int)Register.rg7, 0b10101010, 0b10101010, 0b1010, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x89, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVW_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x8A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ushort.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x8A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b10101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVW_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ushort.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b10101010101010101010UL);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVW_Address_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ushort.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x8C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x8C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x8C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b10101010101010101010UL;
                testProcessor.LoadProgram(new byte[] { 0x8C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b1100110111001101;
                testProcessor.LoadProgram(new byte[] { 0x8C, 0xFC, 0x07, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                // Ensure that writing a word at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryWord(2044), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVW_Address_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x8D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0b10101010, 0b10101010, 0b1010, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x8D, 0xFC, 0x07, 0, 0, 0, 0, 0, 0, 0b11001101, 0b11001101, 0, 0, 0, 0, 0, 0 });
                // Ensure that writing a word at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryWord(2044), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVW_Pointer_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ushort.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8E, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8E, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8E, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b10101010101010101010UL;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8E, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b1100110111001101;
                testProcessor.Registers[(int)Register.rg7] = 2044;
                testProcessor.LoadProgram(new byte[] { 0x8E, (int)Register.rg7, (int)Register.rg8 });
                // Ensure that writing a word at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryWord(2044), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(2044UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVW_Pointer_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8F, (int)Register.rg7, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8F, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8F, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x8F, (int)Register.rg7, 0b10101010, 0b10101010, 0b1010, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2044;
                testProcessor.LoadProgram(new byte[] { 0x8F, (int)Register.rg7, 0b11001101, 0b11001101, 0, 0, 0, 0, 0, 0 });
                // Ensure that writing a word at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryWord(2044), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual((ushort)0b1100110111001101, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(2044UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVD_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = uint.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x90, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x90, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x90, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010101010101010101010101010;
                testProcessor.LoadProgram(new byte[] { 0x90, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x90, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVD_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x91, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x91, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x91, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x91, (int)Register.rg7, 0b10101010, 0b10101010, 0b10101010, 0b10101010, 0b1010, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x91, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVD_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x92, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, uint.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x92, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x92, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x92, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010101010101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x92, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVD_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x93, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, uint.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x93, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x93, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x93, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010101010101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x93, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVD_Address_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = uint.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x94, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x94, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x94, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010101010101010101010101010;
                testProcessor.LoadProgram(new byte[] { 0x94, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b11001101110011011100110111001101;
                testProcessor.LoadProgram(new byte[] { 0x94, 0xFA, 0x07, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                // Ensure that writing a dword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryDWord(2042), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b11001101110011011100110111001101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVD_Address_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x95, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x95, 0x28, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x95, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x95, 0x28, 2, 0, 0, 0, 0, 0, 0, 0b10101010, 0b10101010, 0b10101010, 0b10101010, 0b1010, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x95, 0xFA, 0x07, 0, 0, 0, 0, 0, 0, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0, 0, 0, 0 });
                // Ensure that writing a dword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryDWord(2042), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVD_Pointer_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = uint.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x96, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x96, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x96, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010101010101010101010101010UL;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x96, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b11001101110011011100110111001101;
                testProcessor.Registers[(int)Register.rg7] = 2042;
                testProcessor.LoadProgram(new byte[] { 0x96, (int)Register.rg7, (int)Register.rg8 });
                // Ensure that writing a dword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryDWord(2042), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(2042UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVD_Pointer_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x97, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x97, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x97, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x97, (int)Register.rg7, 0b10101010, 0b10101010, 0b10101010, 0b10101010, 0b1010, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b10101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2042;
                testProcessor.LoadProgram(new byte[] { 0x97, (int)Register.rg7, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0, 0, 0, 0 });
                // Ensure that writing a dword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryDWord(2042), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b11001101110011011100110111001101, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(2042UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVQ_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x98, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x98, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x98, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVQ_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x99, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x99, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x99, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVQ_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x9A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x9A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x9A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVQ_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9B, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void MVQ_Address_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x9C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0x9C, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b1100110111001101110011011100110111001101110011011100110111001101;
                testProcessor.LoadProgram(new byte[] { 0x9C, 0xF6, 0x07, 0, 0, 0, 0, 0, 0, (int)Register.rg8 });
                // Ensure that writing a qword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVQ_Address_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0x9D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x9D, 0x28, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0x9D, 0xF6, 0x07, 0, 0, 0, 0, 0, 0, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101 });
                // Ensure that writing a qword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(17UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(9), "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVQ_Pointer_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9E, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9E, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b1100110111001101110011011100110111001101110011011100110111001101;
                testProcessor.Registers[(int)Register.rg7] = 2038;
                testProcessor.LoadProgram(new byte[] { 0x9E, (int)Register.rg7, (int)Register.rg8 });
                // Ensure that writing a qword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void MVQ_Pointer_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9F, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9F, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0x9F, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2038;
                testProcessor.LoadProgram(new byte[] { 0x9F, (int)Register.rg7, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101, 0b11001101 });
                // Ensure that writing a qword at the end of memory doesn't cause out of range error
                _ = testProcessor.Execute(false);
                Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b1100110111001101110011011100110111001101110011011100110111001101, testProcessor.ReadMemoryQWord(2), "Instruction updated the second operand");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void PSH_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xA0, (int)Register.rg7, 0xA0, (int)Register.rg8 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg7], ulong.MaxValue, "Instruction updated the operand");

                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.WriteMemoryQWord(2030, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 0UL, "Instruction updated the operand");
            }

            [TestMethod]
            public void PSH_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xA1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(1), ulong.MaxValue, "Instruction updated the operand");

                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.WriteMemoryQWord(2030, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(18UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(10), 0UL, "Instruction updated the operand");
            }

            [TestMethod]
            public void PSH_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xA2, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xA2, 0x30, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.WriteMemoryQWord(560, 0);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the operand");

                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.WriteMemoryQWord(2030, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(18UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(560), 0UL, "Instruction updated the operand");
            }

            [TestMethod]
            public void PSH_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0xA3, (int)Register.rg7, 0xA3, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.WriteMemoryQWord(560, 0);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(2038), "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg7], 552UL, "Instruction updated the operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(552), ulong.MaxValue, "Instruction updated the operand");

                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg8] = 560;
                testProcessor.WriteMemoryQWord(2030, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(testProcessor.Registers[(int)Register.rg8], 560UL, "Instruction updated the operand");
                Assert.AreEqual(testProcessor.ReadMemoryQWord(560), 0UL, "Instruction updated the operand");
            }

            [TestMethod]
            public void POP_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xA4, (int)Register.rg7, 0xA4, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(2038, 0);
                testProcessor.WriteMemoryQWord(2030, ulong.MaxValue);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                testProcessor.Registers[(int)Register.rso] = 2030;
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2038UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void CAL_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(9UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(9UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void CAL_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB1, (int)Register.rg8 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB1, (int)Register.rg8 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the operand");
            }

            [TestMethod]
            public void CAL_Address_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB2, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg7 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB2, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg7 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void CAL_Address_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB3, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(17UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB3, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(17UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void CAL_Address_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB4, 0x28, 2, 0, 0, 0, 0, 0, 0, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(17UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB4, 0x28, 2, 0, 0, 0, 0, 0, 0, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(17UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void CAL_Address_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB5, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB5, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void CAL_Pointer_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                testProcessor.Registers[(int)Register.rg8] = 552;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB6, (int)Register.rg8, (int)Register.rg7 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(3UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB6, (int)Register.rg8, (int)Register.rg7 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(3UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void CAL_Pointer_Literal()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB7, (int)Register.rg8, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB7, (int)Register.rg8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
            }

            [TestMethod]
            public void CAL_Pointer_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB8, (int)Register.rg8, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB8, (int)Register.rg8, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(10UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
            }

            [TestMethod]
            public void CAL_Pointer_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.Registers[(int)Register.rg8] = 552;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xB9, (int)Register.rg8, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(3UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(2046UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xB9, (int)Register.rg8, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "rso register started with incorrect value");
                _ = testProcessor.Execute(false);
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2030UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(3UL, testProcessor.ReadMemoryQWord(2038), "Instruction did not correctly push values to stack");
                Assert.AreEqual(12345UL, testProcessor.ReadMemoryQWord(2030), "Instruction did not correctly push values to stack");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rfp], "Instruction did not correctly update rfp");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the first operand");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void RET()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xBA });
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rpo] = 26;
                testProcessor.LoadProgram(Array.Empty<byte>());
                // rpo isn't starting at 0, so manually load the program at the correct offset
                testProcessor.Memory[26] = 0xBA;
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void RET_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xBB, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xBB, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void RET_Literal()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xBC, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xBC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void RET_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xBD, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xBD, 225, 0, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void RET_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xBE, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, 1234567890);
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rso] = 2030;
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.Registers[(int)Register.rsb] = 12345;
                testProcessor.LoadProgram(new byte[] { 0xBE, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                testProcessor.WriteMemoryQWord(2030, 1234567890);
                testProcessor.WriteMemoryQWord(2038, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rsb], "Instruction updated the rsb register by an incorrect amount");
                Assert.AreEqual(2046UL, testProcessor.Registers[(int)Register.rso], "Instruction updated the rso register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rrv], "Instruction did not correctly update rrv");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
            }

            [TestMethod]
            public void WCN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC0, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xC0, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC1, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xC1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC2, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xC2, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC3, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xC3, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCB_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC4, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xC4, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCB_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC5, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xC5, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCB_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC6, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, stdoutOverride: consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xC6, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCB_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC7, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xC7, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCX_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC8, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xC8, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCX_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xC9, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xC9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCX_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCA, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xCA, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCX_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCB, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xCB, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void WCC_Register()
            {
                Processor testProcessor = new(2046);
                // WCC should only process 1 byte
                testProcessor.Registers[(int)Register.rg7] = (ulong)'e' + 0b100000000;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCC, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Registers[(int)Register.rg7] = 0xE3;
                testProcessor.Registers[(int)Register.rg8] = 0x83;
                testProcessor.Registers[(int)Register.rg9] = 0x88;
                testProcessor.LoadProgram(new byte[] { 0xCC, (int)Register.rg7, 0xCC, (int)Register.rg8, 0xCC, (int)Register.rg9, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(true, consoleOutput);
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCC_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCD, 0x65, 1, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.LoadProgram(new byte[] { 0xCD, 0xE3, 0, 0, 0, 0, 0, 0, 0, 0xCD, 0x83, 0, 0, 0, 0, 0, 0, 0, 0xCD, 0x88, 0, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(true, consoleOutput);
                    Assert.AreEqual(28UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCC_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCE, 225, 0, 0, 0, 0, 0, 0, 0 });
                // WCC should only process 1 byte
                testProcessor.WriteMemoryQWord(225, (ulong)'e' + 0b100000000);
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xCE, 0xFB, 7, 0, 0, 0, 0, 0, 0, 0xCE, 0xFC, 7, 0, 0, 0, 0, 0, 0, 0xCE, 0xFD, 7, 0, 0, 0, 0, 0, 0, 0 });
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Memory[2043] = 0xE3;
                testProcessor.Memory[2044] = 0x83;
                testProcessor.Memory[2045] = 0x88;
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(true, consoleOutput);
                    Assert.AreEqual(28UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WCC_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xCF, (int)Register.rg7 });
                // WCC should only process 1 byte
                testProcessor.WriteMemoryQWord(225, (ulong)'e' + 0b100000000);
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2043;
                testProcessor.Registers[(int)Register.rg8] = 2044;
                testProcessor.Registers[(int)Register.rg9] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xCF, (int)Register.rg7, 0xCF, (int)Register.rg8, 0xCF, (int)Register.rg9, 0 });
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Memory[2043] = 0xE3;
                testProcessor.Memory[2044] = 0x83;
                testProcessor.Memory[2045] = 0x88;
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(true, consoleOutput);
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void WFN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD0, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xD0, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD1, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD1, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD2, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD2, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD2, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD3, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xD3, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("18446744073709551615", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD3, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFB_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD4, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xD4, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD4, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFB_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD5, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD5, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD5, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFB_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD6, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD6, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD6, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFB_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD7, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("210", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xD7, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("255", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD7, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFX_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD8, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xD8, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD8, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFX_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xD9, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xD9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFX_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDA, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDA, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDA, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFX_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDB, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("D2", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xDB, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("FF", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDB, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFC_Register()
            {
                Processor testProcessor = new(2046);
                // WFC should only process 1 byte
                testProcessor.Registers[(int)Register.rg7] = (ulong)'e' + 0b100000000;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDC, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Registers[(int)Register.rg7] = 0xE3;
                testProcessor.Registers[(int)Register.rg8] = 0x83;
                testProcessor.Registers[(int)Register.rg9] = 0x88;
                testProcessor.LoadProgram(new byte[] { 0xDC, (int)Register.rg7, 0xDC, (int)Register.rg8, 0xDC, (int)Register.rg9, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDC, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFC_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDD, 0x65, 1, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.LoadProgram(new byte[] { 0xDD, 0xE3, 0, 0, 0, 0, 0, 0, 0, 0xDD, 0x83, 0, 0, 0, 0, 0, 0, 0, 0xDD, 0x88, 0, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(28UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDD, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFC_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDE, 225, 0, 0, 0, 0, 0, 0, 0 });
                // WFC should only process 1 byte
                testProcessor.WriteMemoryQWord(225, (ulong)'e' + 0b100000000);
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDE, 0xFB, 7, 0, 0, 0, 0, 0, 0, 0xDE, 0xFC, 7, 0, 0, 0, 0, 0, 0, 0xDE, 0xFD, 7, 0, 0, 0, 0, 0, 0, 0 });
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Memory[2043] = 0xE3;
                testProcessor.Memory[2044] = 0x83;
                testProcessor.Memory[2045] = 0x88;
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(28UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDE, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void WFC_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xDF, (int)Register.rg7 });
                // WFC should only process 1 byte
                testProcessor.WriteMemoryQWord(225, (ulong)'e' + 0b100000000);
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("e", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2043;
                testProcessor.Registers[(int)Register.rg8] = 2044;
                testProcessor.Registers[(int)Register.rg9] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xDF, (int)Register.rg7, 0xDF, (int)Register.rg8, 0xDF, (int)Register.rg9, 0 });
                // U+30C8 (Katakana Letter To) in UTF-8
                testProcessor.Memory[2043] = 0xE3;
                testProcessor.Memory[2044] = 0x83;
                testProcessor.Memory[2045] = 0x88;
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("\u30C8", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xDF, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void OFL_Address()
            {
                try
                {
                    // "using" is used here so that the open file stream is closed without having to use the CFL instruction
                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.LoadProgram(new byte[] { 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                        "OFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        // Make sure file doesn't exist already
                        File.Delete("OFL_Address.txt");
                        _ = testProcessor.Execute(false);
                        Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.IsTrue(File.Exists("OFL_Address.txt"), "Instruction did not create the file");
                        Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        File.WriteAllText("OFL_Address.txt", "This file is not empty");
                        testProcessor.LoadProgram(new byte[] { 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                        "OFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = testProcessor.Execute(false);
                        FileStream? openFile = typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor) as FileStream;
                        Assert.IsNotNull(openFile, "Instruction did not open a file");
                        Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.AreEqual("OFL_Address.txt", Path.GetFileName(openFile.Name), "Instruction did not open the correct file");
                        Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        _ = Directory.CreateDirectory("OFL_Address_DIR");
                        testProcessor.LoadProgram(new byte[] { 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                        "OFL_Address_DIR/OFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        // Make sure file doesn't exist already
                        File.Delete("OFL_Address_DIR/OFL_Address.txt");
                        _ = testProcessor.Execute(false);
                        Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.IsTrue(File.Exists("OFL_Address_DIR/OFL_Address.txt"), "Instruction did not create the file");
                        Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.LoadProgram(new byte[] { 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0, 0 });
                        "OFL_Address_DIR/OFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(true),
                            "Instruction did not throw an exception when opening a file with one already open");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.LoadProgram(new byte[] { 0xE0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                        "ThisDirDoesntExist/OFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                            "Instruction did not throw an exception when creating file in non-existent directory");
                    }
                }
                finally
                {
                    if (Directory.Exists("OFL_Address_DIR"))
                    {
                        File.Delete("OFL_Address_DIR/OFL_Address.txt");
                        Directory.Delete("OFL_Address_DIR");
                    }
                    File.Delete("OFL_Address.txt");
                }
            }

            [TestMethod]
            public void OFL_Pointer()
            {
                try
                {
                    // "using" is used here so that the open file stream is closed without having to use the CFL instruction
                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.Registers[(int)Register.rg7] = 552;
                        testProcessor.LoadProgram(new byte[] { 0xE1, (int)Register.rg7 });
                        "OFL_Pointer.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        // Make sure file doesn't exist already
                        File.Delete("OFL_Pointer.txt");
                        _ = testProcessor.Execute(false);
                        Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.IsTrue(File.Exists("OFL_Pointer.txt"), "Instruction did not create the file");
                        Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        File.WriteAllText("OFL_Pointer.txt", "This file is not empty");
                        testProcessor.Registers[(int)Register.rg7] = 552;
                        testProcessor.LoadProgram(new byte[] { 0xE1, (int)Register.rg7 });
                        "OFL_Pointer.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = testProcessor.Execute(false);
                        FileStream? openFile = typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor) as FileStream;
                        Assert.IsNotNull(openFile, "Instruction did not open a file");
                        Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.AreEqual("OFL_Pointer.txt", Path.GetFileName(openFile.Name), "Instruction did not open the correct file");
                        Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        _ = Directory.CreateDirectory("OFL_Pointer_DIR");
                        testProcessor.Registers[(int)Register.rg7] = 552;
                        testProcessor.LoadProgram(new byte[] { 0xE1, (int)Register.rg7 });
                        "OFL_Pointer_DIR/OFL_Pointer.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        // Make sure file doesn't exist already
                        File.Delete("OFL_Pointer_DIR/OFL_Pointer.txt");
                        _ = testProcessor.Execute(false);
                        Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                        Assert.IsTrue(File.Exists("OFL_Pointer_DIR/OFL_Pointer.txt"), "Instruction did not create the file");
                        Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.Registers[(int)Register.rg7] = 552;
                        testProcessor.LoadProgram(new byte[] { 0xE1, (int)Register.rg7, 0xE1, (int)Register.rg7, 0 });
                        "OFL_Pointer_DIR/OFL_Pointer.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(true),
                            "Instruction did not throw an exception when opening a file with one already open");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.Registers[(int)Register.rg7] = 552;
                        testProcessor.LoadProgram(new byte[] { 0xE1, (int)Register.rg7 });
                        "ThisDirDoesntExist/OFL_Pointer.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                        _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                            "Instruction did not throw an exception when creating file in non-existent directory");
                    }
                }
                finally
                {
                    if (Directory.Exists("OFL_Pointer_DIR"))
                    {
                        File.Delete("OFL_Pointer_DIR/OFL_Pointer.txt");
                        Directory.Delete("OFL_Pointer_DIR");
                    }
                    File.Delete("OFL_Pointer.txt");
                }
            }

            [TestMethod]
            public void CFL()
            {
                try
                {
                    using (Processor testProcessor = new(2046))
                    {
                        FileStream openFile = new("CFL.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        BinaryWriter fileWrite = new(openFile, Encoding.UTF8);
                        BinaryReader fileRead = new(openFile, Encoding.UTF8);
                        typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, openFile);
                        typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileWrite);
                        typeof(Processor).GetField("fileRead", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileRead);
                        testProcessor.LoadProgram(new byte[] { 0xE2 });
                        _ = testProcessor.Execute(false);
                        Assert.IsFalse(openFile.CanRead, "Instruction did not close the file stream");
                        Assert.IsFalse(openFile.CanWrite, "Instruction did not close the file stream");
                    }

                    using (Processor testProcessor = new(2046))
                    {
                        testProcessor.LoadProgram(new byte[] { 0xE2 });
                        _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                            "Instruction did not throw an exception when closing file without one open");
                    }
                }
                finally
                {
                    File.Delete("CFL.txt");
                }
            }

            [TestMethod]
            public void DFL_Address()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.CreateText("DFL_Address.txt").Close();
                    testProcessor.LoadProgram(new byte[] { 0xE3, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.IsFalse(File.Exists("DFL_Address.txt"), "Instruction did not delete the file");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("DFL_Address_DIR");
                    File.CreateText("DFL_Address_DIR/DFL_Address.txt").Close();
                    testProcessor.LoadProgram(new byte[] { 0xE3, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "DFL_Address_DIR/DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(9UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.IsFalse(File.Exists("DFL_Address_DIR/DFL_Address.txt"), "Instruction did not delete the file");

                    testProcessor = new(2046);
                    testProcessor.LoadProgram(new byte[] { 0xE3, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "ThisDirDoesntExist/DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when deleting file in non-existent directory");
                }
                finally
                {
                    if (Directory.Exists("DFL_Address_DIR"))
                    {
                        File.Delete("DFL_Address_DIR/DFL_Address.txt");
                        Directory.Delete("DFL_Address_DIR");
                    }
                    File.Delete("DFL_Address.txt");
                }
            }

            [TestMethod]
            public void DFL_Pointer()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.CreateText("DFL_Address.txt").Close();
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE4, (int)Register.rg7 });
                    "DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.IsFalse(File.Exists("DFL_Address.txt"), "Instruction did not delete the file");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("DFL_Address_DIR");
                    File.CreateText("DFL_Address_DIR/DFL_Address.txt").Close();
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE4, (int)Register.rg7 });
                    "DFL_Address_DIR/DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.IsFalse(File.Exists("DFL_Address_DIR/DFL_Address.txt"), "Instruction did not delete the file");

                    testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE4, (int)Register.rg7 });
                    "ThisDirDoesntExist/DFL_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when deleting file in non-existent directory");
                }
                finally
                {
                    if (Directory.Exists("DFL_Address_DIR"))
                    {
                        File.Delete("DFL_Address_DIR/DFL_Address.txt");
                        Directory.Delete("DFL_Address_DIR");
                    }
                    File.Delete("DFL_Address.txt");
                }
            }

            [TestMethod]
            public void FEX_Register_Address()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.CreateText("FEX_Register_Address.txt").Close();
                    testProcessor.LoadProgram(new byte[] { 0xE5, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.Delete("FEX_Register_Address.txt");
                    testProcessor.LoadProgram(new byte[] { 0xE5, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("FEX_Register_Address_DIR");
                    File.CreateText("FEX_Register_Address_DIR/FEX_Register_Address.txt").Close();
                    testProcessor.LoadProgram(new byte[] { 0xE5, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address_DIR/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    testProcessor.LoadProgram(new byte[] { 0xE5, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "ThisDirDoesntExist/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                }
                finally
                {
                    if (Directory.Exists("FEX_Register_Address_DIR"))
                    {
                        File.Delete("FEX_Register_Address_DIR/FEX_Register_Address.txt");
                        Directory.Delete("FEX_Register_Address_DIR");
                    }
                    File.Delete("FEX_Register_Address.txt");
                }
            }

            [TestMethod]
            public void FEX_Register_Pointer()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.CreateText("FEX_Register_Address.txt").Close();
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE6, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.Delete("FEX_Register_Address.txt");
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE6, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("FEX_Register_Address_DIR");
                    File.CreateText("FEX_Register_Address_DIR/FEX_Register_Address.txt").Close();
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE6, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address_DIR/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE6, (int)Register.rg8, (int)Register.rg7 });
                    "ThisDirDoesntExist/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                }
                finally
                {
                    if (Directory.Exists("FEX_Register_Address_DIR"))
                    {
                        File.Delete("FEX_Register_Address_DIR/FEX_Register_Address.txt");
                        Directory.Delete("FEX_Register_Address_DIR");
                    }
                    File.Delete("FEX_Register_Address.txt");
                }
            }

            [TestMethod]
            public void FSZ_Register_Address()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.WriteAllText("FEX_Register_Address.txt", "abcdefg");
                    testProcessor.LoadProgram(new byte[] { 0xE7, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.WriteAllText("FEX_Register_Address.txt", "abcdefghijklmnop");
                    testProcessor.LoadProgram(new byte[] { 0xE7, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(16UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("FEX_Register_Address_DIR");
                    File.WriteAllText("FEX_Register_Address_DIR/FEX_Register_Address.txt", "abcdefghijklmnopqrstuvwxyz");
                    testProcessor.LoadProgram(new byte[] { 0xE7, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address_DIR/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(10UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.Delete("FEX_Register_Address.txt");
                    testProcessor.LoadProgram(new byte[] { 0xE7, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<FileNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when checking file size of non-existent file");

                    testProcessor = new(2046);
                    testProcessor.LoadProgram(new byte[] { 0xE7, (int)Register.rg8, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "ThisDirDoesntExist/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<FileNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when checking file size of non-existent file");
                }
                finally
                {
                    if (Directory.Exists("FEX_Register_Address_DIR"))
                    {
                        File.Delete("FEX_Register_Address_DIR/FEX_Register_Address.txt");
                        Directory.Delete("FEX_Register_Address_DIR");
                    }
                    File.Delete("FEX_Register_Address.txt");
                }
            }

            [TestMethod]
            public void FSZ_Register_Pointer()
            {
                try
                {
                    Processor testProcessor = new(2046);
                    File.WriteAllText("FEX_Register_Address.txt", "abcdefg");
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE8, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.WriteAllText("FEX_Register_Address.txt", "abcdefghijklmnop");
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE8, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(16UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    _ = Directory.CreateDirectory("FEX_Register_Address_DIR");
                    File.WriteAllText("FEX_Register_Address_DIR/FEX_Register_Address.txt", "abcdefghijklmnopqrstuvwxyz");
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE8, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address_DIR/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(26UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");

                    testProcessor = new(2046);
                    File.Delete("FEX_Register_Address.txt");
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE8, (int)Register.rg8, (int)Register.rg7 });
                    "FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<FileNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when checking file size of non-existent file");

                    testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xE8, (int)Register.rg8, (int)Register.rg7 });
                    "ThisDirDoesntExist/FEX_Register_Address.txt\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<FileNotFoundException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when checking file size of non-existent file");
                }
                finally
                {
                    if (Directory.Exists("FEX_Register_Address_DIR"))
                    {
                        File.Delete("FEX_Register_Address_DIR/FEX_Register_Address.txt");
                        Directory.Delete("FEX_Register_Address_DIR");
                    }
                    File.Delete("FEX_Register_Address.txt");
                }
            }

            [TestMethod]
            public void RCC_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xF0, (int)Register.rg7 });
                Queue<byte> characterQueue = new();
                characterQueue.Enqueue((byte)'e');
                typeof(Processor).GetField("stdinByteQueue", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, characterQueue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual('e', testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xF0, (int)Register.rg7, 0xF0, (int)Register.rg8, 0xF0, (int)Register.rg9, 0 });
                characterQueue = new();
                // U+30C8 (Katakana Letter To) in UTF-8
                characterQueue.Enqueue(0xE3);
                characterQueue.Enqueue(0x83);
                characterQueue.Enqueue(0x88);
                typeof(Processor).GetField("stdinByteQueue", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, characterQueue);
                _ = testProcessor.Execute(true);
                Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(227UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(131UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                Assert.AreEqual(136UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void RFC_Register()
            {
                using (MemoryStream fileStream = new(new[] { (byte)'e' }))
                {
                    Processor testProcessor = new(2046);
                    using BinaryReader fileInput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileRead", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileInput);
                    typeof(Processor).GetField("openFileSize", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, 1L);
                    testProcessor.LoadProgram(new byte[] { 0xF1, (int)Register.rg7 });
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual('e', testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                    Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                }

                // U+30C8 (Katakana Letter To) in UTF-8
                using (MemoryStream fileStream = new(new byte[] { 0xE3, 0x83, 0x88 }))
                {
                    Processor testProcessor = new(2046);
                    using BinaryReader fileInput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileRead", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileInput);
                    typeof(Processor).GetField("openFileSize", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, 3L);
                    testProcessor.LoadProgram(new byte[] { 0xF1, (int)Register.rg7, 0xF1, (int)Register.rg8, 0xF1, (int)Register.rg9, 0 });
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(7UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(227UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                    Assert.AreEqual(131UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(136UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                }

                // U+30C8 (Katakana Letter To) in UTF-8
                using (MemoryStream fileStream = new(new byte[] { 0xE3, 0x83, 0x88 }))
                {
                    Processor testProcessor = new(2046);
                    using BinaryReader fileInput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileRead", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileInput);
                    typeof(Processor).GetField("openFileSize", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, 3L);
                    // Read less characters than there are in stream to check that file end flag isn't set
                    testProcessor.LoadProgram(new byte[] { 0xF1, (int)Register.rg7, 0xF1, (int)Register.rg8, 0 });
                    _ = testProcessor.Execute(true);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(227UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                    Assert.AreEqual(131UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                }

                // U+30C8 (Katakana Letter To) in UTF-8
                using (MemoryStream fileStream = new(new byte[] { 0xE3, 0x83, 0x88 }))
                {
                    Processor testProcessor = new(2046);
                    using BinaryReader fileInput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileRead", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileInput);
                    typeof(Processor).GetField("openFileSize", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, 3L);
                    testProcessor.LoadProgram(new byte[] { 0xF1, (int)Register.rg7, 0xF1, (int)Register.rg8, 0xF1, (int)Register.rg9, 0xF1, (int)Register.rg9, 0 });
                    _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(true),
                        "Instruction did not throw an exception when reading past end of file");
                }

                Processor testFailProcessor = new(2046);
                testFailProcessor.LoadProgram(new byte[] { 0xF1, (int)Register.rg7, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testFailProcessor.Execute(true),
                    "Instruction did not throw an exception when reading file character without a file open");
            }
        }
    }

    public class AllZeroRandom : Random
    {
        public override long NextInt64(long minValue, long maxValue)
        {
            return 0;
        }
    }
}