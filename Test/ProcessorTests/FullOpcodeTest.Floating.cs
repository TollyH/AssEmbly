using System.Text;

namespace AssEmbly.Test.ProcessorTests
{
    public partial class FullOpcodeTest
    {
        [TestClass]
        public class FloatingPointExtensionSet
        {
            [TestMethod]
            public void FLPT_ADD_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(12345.678);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x00, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(99999.999), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-42.1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x00, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 - 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-12.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x00, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 - 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x00, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-500.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x00, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ADD_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(12345.678);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x01, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(87654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(99999.999), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x01, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 - 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x01, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 - 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x01, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-500.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x01, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ADD_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(12345.678);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x02, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(87654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(99999.999), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x02, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 - 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x02, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 - 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x02, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x02, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ADD_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(12345.678);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x03, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(87654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(99999.999), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x03, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 - 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x03, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 - 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x03, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x03, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SUB_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(76543.210);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321 - 76543.210), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-42.1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 + 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-12.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 + 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x10, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x10, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SUB_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x11, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321 - 76543.210), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 + 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 + 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x11, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x11, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SUB_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321 - 76543.210), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 + 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 + 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x12, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x12, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SUB_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x13, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321 - 76543.210), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 + 42.1), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Carry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5 + 12.3), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x13, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x13, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_MUL_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456 * 654.321), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(654.321), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-42.1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 * (-42.1)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(12345.6789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-12345.6789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x20, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-0.0), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Zero | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x20, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_MUL_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x21, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456 * 654.321), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(654.321), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 * (-42.1)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(12345.6789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-12345.6789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x21, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-0.0), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Zero | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x21, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_MUL_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456 * 654.321), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(654.321), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 * (-42.1)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(12345.6789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-12345.6789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x22, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-0.0), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Zero | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x22, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_MUL_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(654.321));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456 * 654.321), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(654.321), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7 * (-42.1)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(12345.6789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.ZeroAndCarry, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-12345.6789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x23, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-0.0), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Zero | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x23, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DIV_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(2468.12);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-3.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x30, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DIV_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x31, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DIV_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x32, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DIV_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x33, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(2468.12);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-3.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x34, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg7, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(5, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg7, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(5, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg7, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(5, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg7, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(5, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg7, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(5, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rpo, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x35, (int)Register.rg9, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rpo, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x36, (int)Register.rg9, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_DVR_Register_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 / 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123456.789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) / (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NegativeInfinity), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x37, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_REM_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(2468.12);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-3.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x38, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_REM_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x39, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_REM_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_REM_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(2468.12));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % 2468.12), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(2468.12), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(123456.789));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-1.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456.789 % (-1.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-3.5));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((-123456.789) % (-3.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-3.5), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123456.789);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(double.NaN), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x3B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SIN_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x40, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Sin(1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x40, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x40, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Sin(-1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x40, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ASN_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.987654);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x41, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Asin(0.987654)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x41, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.987654);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x41, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Asin(-0.987654)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x41, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_COS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x42, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Cos(1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-2.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x42, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Cos(-2.5)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x42, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ACS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.987654);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x43, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Acos(0.987654)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x43, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x43, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_TAN_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x44, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Tan(1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x44, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x44, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Tan(-1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x44, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_ATN_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.987654);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x45, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan(0.987654)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x45, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.987654);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x45, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan(-0.987654)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x45, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_PTN_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.98765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x46, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x46, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.567);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x46, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(-0.765, 0.567)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.567), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x46, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_PTN_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x47, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.98765));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x47, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x47, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(-0.765, 0.567)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.567), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x47, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_PTN_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x48, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.98765));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x48, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x48, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.567));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(-0.765, 0.567)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x48, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_PTN_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.98765));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x49, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x49, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.567));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x49, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Atan2(-0.765, 0.567)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.567), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x49, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_POW_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(0.98765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(3.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x50, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(-0.765, 3.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x50, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_POW_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x51, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(0.98765));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x51, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x51, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(3.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(-0.765, 3.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x51, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_POW_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.98765));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x52, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(3.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(-0.765, 3.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x52, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_POW_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(0.98765));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x53, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(1.23456, 0.98765)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(0.98765), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x53, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-0.765);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(3.0));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x53, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Pow(-0.765, 3.0)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x53, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_LOG_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.345);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x60, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(2.345, 1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.23456), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(5.6);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x60, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(5.6), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.123);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(3.21);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x60, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(0.123, 3.21)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.21), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x60, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_LOG_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x61, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(1.23456));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(2.345, 1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.23456), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x61, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(5.6));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(5.6), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.123);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x61, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(3.21));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(0.123, 3.21)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.21), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x61, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_LOG_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x62, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.23456));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(2.345, 1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.23456), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x62, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(5.6));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(5.6), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.123);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x62, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(3.21));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(0.123, 3.21)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.21), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x62, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_LOG_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.345);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(1.23456));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x63, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(2.345, 1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.FileEnd | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.23456), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(5.6));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x63, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(5.6), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0.123);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(3.21));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x63, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(Math.Log(0.123, 3.21)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(3.21), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x63, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_WCN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x70, (int)Register.rg7 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456), testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123.456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x70, (int)Register.rg7 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123.456), testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }
            }

            [TestMethod]
            public void FLPT_WCN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x71 });
                testProcessor.WriteMemoryQWord(3, BitConverter.DoubleToUInt64Bits(123.456));
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x71 });
                testProcessor.WriteMemoryQWord(3, BitConverter.DoubleToUInt64Bits(-123.456));
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void FLPT_WCN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x72, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x72, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(-123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void FLPT_WCN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x73, (int)Register.rg7 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x73, (int)Register.rg7 });
                using (StringWriter consoleOutput = new())
                {
                    Console.SetOut(consoleOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(-123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", consoleOutput.ToString(), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }
            }

            [TestMethod]
            public void FLPT_WFN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(123.456);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x80, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123.456), testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-123.456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x80, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-123.456), testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x80, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void FLPT_WFN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x81 });
                testProcessor.WriteMemoryQWord(3, BitConverter.DoubleToUInt64Bits(123.456));
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x81 });
                testProcessor.WriteMemoryQWord(3, BitConverter.DoubleToUInt64Bits(-123.456));
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x81, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void FLPT_WFN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x82, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x82, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(-123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x82, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void FLPT_WFN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x83, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x83, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, BitConverter.DoubleToUInt64Bits(-123.456));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-123.456", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x83, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void FLPT_EXH_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.HalfToUInt16Bits((Half)1.2345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x90, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((double)(Half)1.2345), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.HalfToUInt16Bits((Half)0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x90, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.HalfToUInt16Bits((Half)(-1.23456));
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x90, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits((double)(Half)(-1.23456)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x90, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_EXS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.SingleToUInt32Bits(1.2345f);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x91, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.2345f), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.SingleToUInt32Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x91, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.SingleToUInt32Bits(-1.23456f);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x91, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.23456f), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x91, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SHS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.2345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x92, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.SingleToUInt32Bits((float)1.2345d), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x92, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x92, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.SingleToUInt32Bits((float)-1.23456d), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x92, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_SHH_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.2345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x93, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.HalfToUInt16Bits((Half)1.2345d), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x93, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x93, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.HalfToUInt16Bits((Half)(-1.23456d)), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0x93, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_NEG_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-1.2345);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xA0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(1.2345), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xA0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-0.0), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Zero | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(1.23456);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xA0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1.23456), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xA0, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_UTF_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 123456;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(ulong.MaxValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB0, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_STF_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 123456;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(123456), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xB1, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_FTS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-5), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-5), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC0, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_FCS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-5), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-5), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC1, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_FFS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-6), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-6), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC2, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_FNS_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(5.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(2.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-6), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(-5.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-5), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xC3, (int)Register.rpo });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void FLPT_CMP_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(76543.210);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD0, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-42.1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD0, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(-12.3);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD0, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD0, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD0, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void FLPT_CMP_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD1, (int)Register.rg7 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD1, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD1, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD1, (int)Register.rg9 });
                testProcessor.WriteMemoryQWord(4, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD1, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void FLPT_CMP_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD2, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD2, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD2, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD2, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD2, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
            }

            [TestMethod]
            public void FLPT_CMP_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = BitConverter.DoubleToUInt64Bits(87654.321);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD3, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(76543.210));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(87654.321), testProcessor.Registers[(int)Register.rg7], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(76543.210), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(56.7);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD3, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-42.1));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(56.7), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-42.1), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(-97.5);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD3, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(-12.3));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-97.5), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)(StatusFlags.Carry | StatusFlags.Sign), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(-12.3), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg9] = BitConverter.DoubleToUInt64Bits(500.0);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD3, (int)Register.rg9, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, BitConverter.DoubleToUInt64Bits(500.0));
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.Registers[(int)Register.rg9], "Instruction updated the first operand");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(BitConverter.DoubleToUInt64Bits(500.0), testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x02, 0xD3, (int)Register.rpo, (int)Register.rg8 });
                // Instruction isn't a writing instruction, so shouldn't throw an Exception when given rpo as a parameter
                _ = testProcessor.Execute(false);
            }
        }
    }
}
