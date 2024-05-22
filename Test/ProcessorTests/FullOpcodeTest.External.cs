using System.Runtime.Loader;

namespace AssEmbly.Test.ProcessorTests
{
    public static partial class FullOpcodeTest
    {
        [TestClass]
        public class ExternalAssemblyExtensionSet
        {
            [TestMethod]
            public void ASMX_LDA_Address()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not create load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not open assembly");
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test-invalid.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading invalid assembly");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test-missing.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading non-existent assembly");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0, 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0, 0 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(true),
                        "Instruction did not throw an exception when loading assembly with one already open");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x00, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test-empty.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading assembly with missing AssEmblyInterop type");
                }
            }

            [TestMethod]
            public void ASMX_LDA_Pointer()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x01, (int)Register.rg7 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not create load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not open assembly");
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x01, (int)Register.rg7 });
                    "test-invalid.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading invalid assembly");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x01, (int)Register.rg7 });
                    "test-missing.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading non-existent assembly");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x01, (int)Register.rg7, 0xFF, 0x04, 0x01, (int)Register.rg7, 0 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(true),
                        "Instruction did not throw an exception when loading assembly with one already open");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x01, (int)Register.rg7 });
                    "test-empty.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<InvalidAssemblyException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading assembly with missing AssEmblyInterop type");
                }
            }

            [TestMethod]
            public void ASMX_LDF_Address()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not open function");
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0
                    });
                    "InvalidMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<InvalidFunctionException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading invalid function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0
                    });
                    "TestMethod3\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<InvalidFunctionException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading non-existent function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0, 0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0, 0
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(true),
                        "Instruction did not throw an exception when loading function with one already loaded");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x02, 0xD2, 4, 0, 0, 0, 0, 0, 0
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_LDF_Pointer()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rg8] = 1234;
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x03, (int)Register.rg8
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not open function");
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rg8] = 1234;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x03, (int)Register.rg8
                    });
                    "InvalidMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<InvalidFunctionException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading invalid function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rg8] = 1234;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x03, (int)Register.rg8
                    });
                    "TestMethod3\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<InvalidFunctionException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when loading non-existent function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rg8] = 1234;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x03, (int)Register.rg8, 0xFF, 0x04, 0x03, (int)Register.rg8, 0
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(true),
                        "Instruction did not throw an exception when loading function with one already loaded");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg8] = 1234;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x03, (int)Register.rg8
                    });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 1234);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CLA()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x10
                    });
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not remove load context");
                    Assert.IsNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not remove open assembly");
                    Assert.IsNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not remove open function");
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x10
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CLF()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x11
                    });
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction did not remove open function");
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x11
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }
            }

            [TestMethod]
            public void ASMX_AEX_Register_Address()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x20, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x20, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test-invalid.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x20, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "test-missing.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void ASMX_AEX_Register_Pointer()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x21, (int)Register.rg0, (int)Register.rg7 });
                    "test.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x21, (int)Register.rg0, (int)Register.rg7 });
                    "test-invalid.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x21, (int)Register.rg0, (int)Register.rg7 });
                    "test-missing.dll\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }
            }

            [TestMethod]
            public void ASMX_FEX_Register_Address()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x22, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x22, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "InvalidMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x22, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "TestMethod2\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(12UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x22, (int)Register.rg0, 0x28, 2, 0, 0, 0, 0, 0, 0 });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_FEX_Register_Pointer()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x23, (int)Register.rg0, (int)Register.rg7 });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(1UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x23, (int)Register.rg0, (int)Register.rg7 });
                    "InvalidMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop"));
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x23, (int)Register.rg0, (int)Register.rg7 });
                    "TestMethod2\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(5UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg0], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.Registers[(int)Register.rg7] = 552;
                    testProcessor.LoadProgram(new byte[] { 0xFF, 0x04, 0x23, (int)Register.rg0, (int)Register.rg7 });
                    "TestMethod\0"u8.ToArray().CopyTo(testProcessor.Memory, 552);
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CAL()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x30
                    });
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(3UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0xABUL, testProcessor.Memory[1234], "Instruction did not produce correct result");
                    Assert.AreEqual(0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0xCDUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x30
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x30
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CAL_Register()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg3] = 0x123456789ABCDEF0;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x31, (int)Register.rg3
                    });
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0xABUL, testProcessor.Memory[1234], "Instruction did not produce correct result");
                    Assert.AreEqual(0x123456789ABCDEF0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0xCDUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x31, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x31, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CAL_Literal()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x32, 0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12
                    });
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0xABUL, testProcessor.Memory[1234], "Instruction did not produce correct result");
                    Assert.AreEqual(0x123456789ABCDEF0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0xCDUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x32, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x32, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CAL_Address()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x33, 0x28, 2, 0, 0, 0, 0, 0, 0
                    });
                    testProcessor.WriteMemoryQWord(552, 0x123456789ABCDEF0);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(11UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0xABUL, testProcessor.Memory[1234], "Instruction did not produce correct result");
                    Assert.AreEqual(0x123456789ABCDEF0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0xCDUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x33, 0x28, 2, 0, 0, 0, 0, 0, 0
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x33, 0x28, 2, 0, 0, 0, 0, 0, 0
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }

            [TestMethod]
            public void ASMX_CAL_Pointer()
            {
                // "using" is used here so that the open assembly is closed without having to use closing instructions
                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly?.GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Static, Processor.ExternalMethodParamTypes));
                    testProcessor.Registers[(int)Register.rsf] = ulong.MaxValue;
                    testProcessor.Registers[(int)Register.rg3] = 552;
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x34, (int)Register.rg3
                    });
                    testProcessor.WriteMemoryQWord(552, 0x123456789ABCDEF0);
                    // Test that no exception is thrown
                    _ = testProcessor.Execute(false);
                    Assert.AreEqual(4UL, testProcessor.Registers[(int)Register.rpo], "Instruction updated the rpo register by an incorrect amount");
                    Assert.AreEqual(0xABUL, testProcessor.Memory[1234], "Instruction did not produce correct result");
                    Assert.AreEqual(0x123456789ABCDEF0UL, testProcessor.Registers[(int)Register.rg8], "Instruction did not produce correct result");
                    Assert.AreEqual(0xCDUL, testProcessor.Registers[(int)Register.rg9], "Instruction did not produce correct result");
                    Assert.AreEqual(ulong.MaxValue, testProcessor.Registers[(int)Register.rsf], "Instruction updated the status flags");
                    Assert.IsNotNull(typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed load context");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open assembly");
                    Assert.IsNotNull(typeof(Processor).GetField("openExtFunction", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(testProcessor), "Instruction removed open function");
                }

                using (Processor testProcessor = new(2046))
                {
                    AssemblyLoadContext loadContext = new("TestLoadContext", true);
                    typeof(Processor).GetField("extLoadContext", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(testProcessor, loadContext);
                    Type? loadedAssembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath("test.dll")).GetType("AssEmblyInterop");
                    typeof(Processor).GetField("openExtAssembly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(
                        testProcessor, loadedAssembly);
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x34, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading a function");
                }

                using (Processor testProcessor = new(2046))
                {
                    testProcessor.LoadProgram(new byte[]
                    {
                        0xFF, 0x04, 0x34, (int)Register.rg3
                    });
                    _ = Assert.ThrowsException<ExternalOperationException>(() => testProcessor.Execute(false),
                        "Instruction did not throw an exception when used without loading an assembly");
                }
            }
        }
    }
}
