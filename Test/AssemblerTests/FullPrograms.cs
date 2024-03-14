namespace AssEmbly.Test.AssemblerTests
{
    [TestClass]
    public class FullPrograms
    {
        [TestMethod]
        public void KitchenSink()
        {
            Assembler asm = new();
            asm.AssembleLines(File.ReadAllLines("KitchenSink.asm"));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "The assembly process produced unexpected program bytes");
            Assert.AreEqual(0, result.Warnings.Length,
                "The assembly process returned unexpected warnings");
        }

        [TestMethod]
        public void ExampleProgramsNoErrors()
        {
            Environment.CurrentDirectory = "Example Programs";
            foreach (string asmFile in Directory.EnumerateFiles(".", "*.asm", SearchOption.AllDirectories))
            {
                if (asmFile.EndsWith(".ext.asm", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip files only intended to be imported
                    continue;
                }
                Assembler asm = new();
                asm.SetAssemblerVariable("RUNNING_UNIT_TESTS", 1);
                asm.AssembleLines(File.ReadAllLines(asmFile));
                AssemblyResult result = asm.GetAssemblyResult(true);

                Assert.AreNotEqual(0, result.Program.Length, "Example program \"{0}\" should not be empty", asmFile);
                Assert.AreEqual(0, result.Warnings.Length, "Example program \"{0}\" should not return any warnings", asmFile);
            }
        }
    }
}
