using System.Text;

namespace AssEmbly.Test.ProcessorTests
{
    public static partial class FullOpcodeTest
    {
        [TestClass]
        public class SignedExtensionSet
        {
            [TestMethod]
            public void SIGN_JLT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x00, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JLT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x01, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JLE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x02, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JLE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x03, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JGT_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x04, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JGT_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x05, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JGE_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x06, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JGE_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x07, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JSI_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x08, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JSI_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x09, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JNS_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0A, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JNS_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0B, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JOV_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0C, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JOV_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0D, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");
            }

            [TestMethod]
            public void SIGN_JNO_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0E, 8, 7, 6, 5, 4, 3, 2, 1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_JNO_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Sign;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Overflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.Zero;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)(StatusFlags.SignAndOverflow | StatusFlags.Zero);
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.SignAndOverflow;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register when it shouldn't have");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = 0;
                testProcessor.Registers[(int)Register.rg1] = 0x0102030405060708;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x0F, (int)Register.rg1 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0x0102030405060708UL, testProcessor.Registers[(int)Register.rpo], "Instruction did not update rpo register or updated it incorrectly");
            }

            [TestMethod]
            public void SIGN_DIV_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x10, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DIV_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rg7, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x11, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DIV_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x12, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DIV_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x13, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x14, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg7, (int)Register.rg9, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg7, (int)Register.rg9, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg7, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg7, (int)Register.rg9, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(5), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg7, (int)Register.rg9, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rpo, (int)Register.rg9, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x15, (int)Register.rg9, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(13UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg7, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rpo, (int)Register.rg9, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x16, (int)Register.rg9, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_DVR_Register_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, ulong.MaxValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ulong.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg7, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rpo, (int)Register.rg9, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x17, (int)Register.rg9, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_REM_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 3456789;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-6);
                testProcessor.Registers[(int)Register.rg8] = 5;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x18, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_REM_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rg7, 0x15, 0xBF, 0x34, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rg7, 0xB1, 0x68, 0xDE, 0x3A, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rg7, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-6);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rg7, 5, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(5UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x19, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_REM_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 3456789);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 987654321);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-6);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 5);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(5UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1A, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_REM_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 9876543;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 3456789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(2962965UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(3456789UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 987654321);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(987654321UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-6);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 5);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(5UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 9876543210;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 0);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rg7, (int)Register.rg8 });
                _ = Assert.ThrowsException<DivideByZeroException>(() => testProcessor.Execute(false), "Division by 0 didn't throw DivideByZeroException");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.WriteMemoryQWord(552, 1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x1B, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_SHR_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.Registers[(int)Register.rg8] = 6;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(6UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 63;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 65;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 57;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111111000000, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 64;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x20, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_SHR_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 6, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 0x3F, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 0x41, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 0x39, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111111000000, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rg7, 0x40, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x21, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_SHR_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111111000000, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x22, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_SHR_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set the file end flag to make sure the instruction doesn't affect it
                testProcessor.Registers[(int)Register.rsf] = (ulong)StatusFlags.FileEnd;
                testProcessor.Registers[(int)Register.rg7] = 0b101000101000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 6);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b101000101UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.FileEnd, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(6UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)long.MinValue);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 63);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(63UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 65);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(65UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0b1000000100000000000000000000000000000000000000000000000000000000;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 57);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111111000000, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)(StatusFlags.Sign | StatusFlags.Carry), testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(57UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 17997888522041065068;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(17997888522041065068UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 64);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(64UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x23, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVB_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = byte.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = long.MaxValue - 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x30, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111110101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x30, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVB_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x31, (int)Register.rg7, 0xFF, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x31, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x31, (int)Register.rg7, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x31, (int)Register.rg7, 0b10101010, 0b1010, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111110101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x31, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVB_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, byte.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x32, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111110101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x32, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVB_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x33, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, byte.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(byte.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x33, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x33, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x33, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111111111110101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x33, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVW_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = ushort.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x34, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x34, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = long.MaxValue - 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x34, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b10101010101010101010;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x34, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x34, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVW_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x35, (int)Register.rg7, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x35, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x35, (int)Register.rg7, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x35, (int)Register.rg7, 0b10101010, 0b10101010, 0b1010, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x35, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVW_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x36, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, ushort.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x36, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x36, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x36, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b10101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x36, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVW_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x37, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, ushort.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(ushort.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x37, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x37, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x37, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b10101010101010101010UL);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111111111111111111111010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b10101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x37, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVD_Register_Register()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = uint.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = long.MaxValue - 1;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 0b101010101010101010101010101010101010;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x40, (int)Register.rg7, (int)Register.rg8 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111110101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x40, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVD_Register_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x41, (int)Register.rg7, 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x41, (int)Register.rg7, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x41, (int)Register.rg7, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x41, (int)Register.rg7, 0b10101010, 0b10101010, 0b10101010, 0b10101010, 0b1010, 0, 0, 0 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111110101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(4), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x41, (int)Register.rpo, 1, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVD_Register_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, uint.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x42, (int)Register.rg7, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010101010101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111110101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x42, (int)Register.rpo, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_MVD_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x43, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, uint.MaxValue);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(uint.MaxValue, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x43, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x43, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, long.MaxValue - 1);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue - 1, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(long.MaxValue - 1UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x43, (int)Register.rg7, (int)Register.rg8 });
                testProcessor.WriteMemoryQWord(552, 0b101010101010101010101010101010101010);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0b1111111111111111111111111111111110101010101010101010101010101010UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                Assert.AreEqual(552UL, testProcessor.Registers[(int)Register.rg8], "Instruction updated the second operand");
                Assert.AreEqual(0b101010101010101010101010101010101010UL, testProcessor.ReadMemoryQWord(552), "Instruction updated the second operand");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg8] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x43, (int)Register.rpo, (int)Register.rg8 });
                _ = Assert.ThrowsException<ReadOnlyRegisterException>(() => testProcessor.Execute(false), "Instruction with rpo as destination didn't throw ReadOnlyRegisterException");
            }

            [TestMethod]
            public void SIGN_WCN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x50, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x50, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void SIGN_WCN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x51, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x51, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void SIGN_WCN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x52, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x52, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void SIGN_WCN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x53, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x53, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void SIGN_WCB_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x54, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x54, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void SIGN_WCB_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x55, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x55, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream consoleOutput = new())
                {
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void SIGN_WCB_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x56, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x56, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void SIGN_WCB_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x57, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x57, (int)Register.rg7 });
                using (MemoryStream consoleOutput = new())
                {
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false, consoleOutput);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(consoleOutput.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }
            }

            [TestMethod]
            public void SIGN_WFN_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x60, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x60, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x60, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFN_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x61, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x61, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x61, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFN_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x62, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x62, 225, 0, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x62, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFN_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x63, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, 1234567890);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("1234567890", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 225;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x63, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.WriteMemoryQWord(225, unchecked((ulong)-1));
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(225UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x63, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFB_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 1234567890;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x64, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(1234567890UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-1);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x64, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(unchecked((ulong)-1), testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x64, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFB_Literal()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x65, 0xD2, 0x02, 0x96, 0x49, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x65, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x65, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFB_Address()
            {
                Processor testProcessor = new(2046);
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x66, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x66, 0xFD, 7, 0, 0, 0, 0, 0, 0 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x66, 0, 0, 0, 0, 0, 0, 0, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_WFB_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                // Set all status flags to ensure the instruction doesn't update them
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x67, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = 210;
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-46", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 2045;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x67, (int)Register.rg7 });
                using (MemoryStream fileStream = new())
                {
                    using BinaryWriter fileOutput = new(fileStream);
                    typeof(Processor).GetField("openFile", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileStream);
                    typeof(Processor).GetField("fileWrite", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, fileOutput);
                    testProcessor.Memory[2045] = unchecked((byte)-1);
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual("-1", Encoding.UTF8.GetString(fileStream.ToArray()), "Instruction printed an incorrect result to the console");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.AreEqual(2045UL, testProcessor.Registers[(int)Register.rg7], "Instruction updated the second operand");
                }

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x67, 0 });
                _ = Assert.ThrowsException<FileOperationException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception when writing to file without one open");
            }

            [TestMethod]
            public void SIGN_EXB_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((byte)-45);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x70, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-45), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((byte)sbyte.MinValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x70, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)sbyte.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 123456789;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x70, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(21UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x70, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
            }

            [TestMethod]
            public void SIGN_EXW_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ushort)-1234);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x71, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-1234), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ushort)short.MinValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x71, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)short.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 76966591;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x71, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(27327UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x71, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
            }

            [TestMethod]
            public void SIGN_EXD_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((uint)-123456789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x72, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-123456789), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((uint)int.MinValue);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x72, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)int.MinValue), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 4763509140480223935;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x72, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(983984831UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x72, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
            }

            [TestMethod]
            public void SIGN_NEG_Register()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = unchecked((ulong)-123456789);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x80, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(123456789UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 987654321;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x80, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(unchecked((ulong)-987654321), testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Sign, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = ulong.MaxValue - 123456;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x80, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(123457UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 0;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x01, 0x80, (int)Register.rg7 });
                _ = testProcessor.Execute(false);
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg7], "Instruction did not produce correct result");
                Assert.AreEqual((ulong)StatusFlags.Zero, testProcessor.Registers[(int)Register.rsf], "Instruction did not correctly set status flags");
            }
        }
    }
}
