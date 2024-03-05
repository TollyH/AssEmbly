namespace AssEmbly.Test.AssemblerTests
{
    [TestClass]
    public class AssembleKitchenSink
    {
        [TestMethod]
        public void CorrectByteOutput()
        {
            Assembler asm = new();
            asm.AssembleLines(File.ReadAllLines("KitchenSink.asm"));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "The assembly process produced unexpected program bytes");
            Assert.AreEqual(0, result.Warnings.Length,
                "The assembly process returned unexpected warnings");
        }
    }
}
