using System.Text;

namespace AssEmbly.Test.ProcessorTests
{
    public partial class FullOpcodeTest
    {
        [TestClass]
        public class FileSystemExtensionSet
        {
            [TestMethod]
            public void FSYS_CWD_Address()
            {
                string startDirectory = Environment.CurrentDirectory;
                string targetDirectory = Path.Join(Environment.CurrentDirectory, "Example Programs");

                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                Assert.AreEqual(targetDirectory, Environment.CurrentDirectory, "Instruction did not set correct working directory");
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Environment.CurrentDirectory = startDirectory;

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                "Example Programs\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(targetDirectory, Environment.CurrentDirectory, "Instruction did not set correct working directory");
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Environment.CurrentDirectory = startDirectory;

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                "Not Exists\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception with non-existent directory");

                Environment.CurrentDirectory = startDirectory;
            }

            [TestMethod]
            public void FSYS_CWD_Pointer()
            {
                string startDirectory = Environment.CurrentDirectory;
                string targetDirectory = Path.Join(Environment.CurrentDirectory, "Example Programs");

                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x01, (int)Register.rg7 });
                Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                Assert.AreEqual(targetDirectory, Environment.CurrentDirectory, "Instruction did not set correct working directory");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Environment.CurrentDirectory = startDirectory;

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x01, (int)Register.rg7 });
                "Example Programs\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(targetDirectory, Environment.CurrentDirectory, "Instruction did not set correct working directory");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Environment.CurrentDirectory = startDirectory;

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x01, (int)Register.rg7 });
                "Not Exists\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = Assert.ThrowsException<DirectoryNotFoundException>(() => testProcessor.Execute(false),
                    "Instruction did not throw an exception with non-existent directory");

                Environment.CurrentDirectory = startDirectory;
            }

            [TestMethod]
            public void FSYS_GWD_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x02, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                string retrievedDirectory = testProcessor.ReadMemoryString(552);
                Assert.AreEqual(Environment.CurrentDirectory, retrievedDirectory, "Instruction did not get correct working directory");
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void FSYS_GWD_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x03, (int)Register.rg7 });
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                string retrievedDirectory = testProcessor.ReadMemoryString(552);
                Assert.AreEqual(Environment.CurrentDirectory, retrievedDirectory, "Instruction did not get correct working directory");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void FSYS_CDR_Address()
            {
                string targetDirectory = Path.Join(Environment.CurrentDirectory, "New Dir");

                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory);
                }

                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x10, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                Assert.IsTrue(Directory.Exists(targetDirectory), "Instruction did not create directory");
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Directory.Delete(targetDirectory);

                targetDirectory = Path.Join(Environment.CurrentDirectory, "New Dir", "Dir1", "Directory 2");

                testProcessor = new(2046);
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x10, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                "New Dir/Dir1/Directory 2\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.IsTrue(Directory.Exists(targetDirectory), "Instruction did not create directory");
                Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Directory.Delete("New Dir", true);
            }

            [TestMethod]
            public void FSYS_CDR_Pointer()
            {
                string targetDirectory = Path.Join(Environment.CurrentDirectory, "New Dir");

                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory);
                }

                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x11, (int)Register.rg7 });
                Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                // Test that no exception is thrown
                _ = testProcessor.Execute(false);
                Assert.IsTrue(Directory.Exists(targetDirectory), "Instruction did not create directory");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Directory.Delete(targetDirectory);

                targetDirectory = Path.Join(Environment.CurrentDirectory, "New Dir", "Dir1", "Directory 2");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rg7] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x11, (int)Register.rg7 });
                "New Dir/Dir1/Directory 2\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.IsTrue(Directory.Exists(targetDirectory), "Instruction did not create directory");
                Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                Directory.Delete("New Dir", true);
            }

            [TestMethod]
            public void FSYS_DDR_Address()
            {
                string parentDirectory = Path.Join(Environment.CurrentDirectory, "New Dir 1");
                string targetDirectory = Path.Join(parentDirectory, "New Dir 2");

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x20, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    Encoding.UTF8.GetBytes(parentDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.IsFalse(Directory.Exists(parentDirectory), "Instruction did not delete directory");
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (Directory.Exists(parentDirectory))
                    {
                        Directory.Delete(parentDirectory, true);
                    }
                }
            }

            [TestMethod]
            public void FSYS_DDR_Pointer()
            {
                string parentDirectory = Path.Join(Environment.CurrentDirectory, "New Dir 1");
                string targetDirectory = Path.Join(parentDirectory, "New Dir 2");

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg1] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x21, (int)Register.rg1 });
                    Encoding.UTF8.GetBytes(parentDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.IsFalse(Directory.Exists(parentDirectory), "Instruction did not delete directory");
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (Directory.Exists(parentDirectory))
                    {
                        Directory.Delete(parentDirectory, true);
                    }
                }
            }

            [TestMethod]
            public void FSYS_DDE_Address()
            {
                string parentDirectory = Path.Join(Environment.CurrentDirectory, "New Dir 1");
                string targetDirectory = Path.Join(parentDirectory, "New Dir 2");

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x22, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.IsFalse(Directory.Exists(targetDirectory), "Instruction did not delete directory");
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                    Directory.CreateDirectory(targetDirectory);

                    testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x22, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    Encoding.UTF8.GetBytes(parentDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<IOException>(() => testProcessor.Execute(false));
                }
                finally
                {
                    if (Directory.Exists(parentDirectory))
                    {
                        Directory.Delete(parentDirectory, true);
                    }
                }
            }

            [TestMethod]
            public void FSYS_DDE_Pointer()
            {
                string parentDirectory = Path.Join(Environment.CurrentDirectory, "New Dir 1");
                string targetDirectory = Path.Join(parentDirectory, "New Dir 2");

                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg1] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x23, (int)Register.rg1 });
                    Encoding.UTF8.GetBytes(targetDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = testProcessor.Execute(false);
                    Assert.IsFalse(Directory.Exists(targetDirectory), "Instruction did not delete directory");
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                    Directory.CreateDirectory(targetDirectory);

                    testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg1] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x23, (int)Register.rg1 });
                    Encoding.UTF8.GetBytes(parentDirectory + '\0').CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<IOException>(() => testProcessor.Execute(false));
                }
                finally
                {
                    if (Directory.Exists(parentDirectory))
                    {
                        Directory.Delete(parentDirectory, true);
                    }
                }
            }

            [TestMethod]
            public void FSYS_DEX_Register_Address()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x30, (int)Register.rg4, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                "Example Programs\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not return correct result");
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x30, (int)Register.rg4, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                "Example Programs/Not Exists/Not Here\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not return correct result");
                Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void FSYS_DEX_Register_Pointer()
            {
                Processor testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg5] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x31, (int)Register.rg4, (int)Register.rg5 });
                "Example Programs\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not return correct result");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");

                testProcessor = new(2046);
                testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                testProcessor.Registers[(int)Register.rg5] = 552;
                testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x31, (int)Register.rg4, (int)Register.rg5 });
                "Example Programs/Not Exists/Not Here\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                _ = testProcessor.Execute(false);
                Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg4], "Instruction did not return correct result");
                Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
            }

            [TestMethod]
            public void FSYS_CPY_Address_Address()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x40, 0x28, 2, 0, 0, 0, 0, 0, 0, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not copy file");
                    CollectionAssert.AreEqual(File.ReadAllBytes(sourceFile), File.ReadAllBytes(targetFile), "Copied file is not identical");
                    Assert.AreEqual(19UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_CPY_Address_Pointer()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 1024;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x41, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg5 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not copy file");
                    CollectionAssert.AreEqual(File.ReadAllBytes(sourceFile), File.ReadAllBytes(targetFile), "Copied file is not identical");
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_CPY_Pointer_Address()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x42, (int)Register.rg5, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not copy file");
                    CollectionAssert.AreEqual(File.ReadAllBytes(sourceFile), File.ReadAllBytes(targetFile), "Copied file is not identical");
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_CPY_Pointer_Pointer()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 552;
                    testProcessor.Registers[(int)Register.rg6] = 1024;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x43, (int)Register.rg5, (int)Register.rg6 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not copy file");
                    CollectionAssert.AreEqual(File.ReadAllBytes(sourceFile), File.ReadAllBytes(targetFile), "Copied file is not identical");
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_MOV_Address_Address()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x44, 0x28, 2, 0, 0, 0, 0, 0, 0, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not move file");
                    Assert.IsFalse(File.Exists(sourceFile), "Instruction did not move file");
                    Assert.AreEqual(19UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Move(targetFile, sourceFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_MOV_Address_Pointer()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 1024;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x45, 0x28, 2, 0, 0, 0, 0, 0, 0, (int)Register.rg5 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not move file");
                    Assert.IsFalse(File.Exists(sourceFile), "Instruction did not move file");
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Move(targetFile, sourceFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_MOV_Pointer_Address()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x46, (int)Register.rg5, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not move file");
                    Assert.IsFalse(File.Exists(sourceFile), "Instruction did not move file");
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Move(targetFile, sourceFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_MOV_Pointer_Pointer()
            {
                string sourceFile = Path.Join(Environment.CurrentDirectory, "copyfile");
                string targetFile = Path.Join(Environment.CurrentDirectory, "otherfile");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                }

                try
                {
                    Processor testProcessor = new(2046);
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg5] = 552;
                    testProcessor.Registers[(int)Register.rg6] = 1024;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x06, 0x47, (int)Register.rg5, (int)Register.rg6 });
                    Encoding.UTF8.GetBytes(targetFile + '\0').CopyTo(testProcessor.Memory, 552);
                    Encoding.UTF8.GetBytes(sourceFile + '\0').CopyTo(testProcessor.Memory, 1024);
                    _ = testProcessor.Execute(false);
                    Assert.IsTrue(File.Exists(targetFile), "Instruction did not move file");
                    Assert.IsFalse(File.Exists(sourceFile), "Instruction did not move file");
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
                finally
                {
                    if (File.Exists(targetFile))
                    {
                        File.Move(targetFile, sourceFile);
                    }
                }
            }

            [TestMethod]
            public void FSYS_BDL()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_BDL_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_BDL_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GNF_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GNF_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GND_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GND_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GCT_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GCT_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GMT_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GMT_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GAT_Register_Address()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_GAT_Register_Pointer()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SCT_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SCT_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SCT_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SCT_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SMT_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SMT_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SMT_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SMT_Pointer_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SAT_Address_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SAT_Pointer_Register()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SAT_Address_Literal()
            {
                throw new NotImplementedException();
            }

            [TestMethod]
            public void FSYS_SAT_Pointer_Literal()
            {
                throw new NotImplementedException();
            }
        }
    }
}
