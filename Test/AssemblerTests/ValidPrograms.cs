namespace AssEmbly.Test.AssemblerTests
{
    [TestClass]
    public class ValidPrograms
    {
        [TestMethod]
        public void KitchenSink()
        {
            Assembler asm = new("");
            asm.AssembleLines(File.ReadAllLines("KitchenSink.asm"));
            AssemblyResult result = asm.GetAssemblyResult(true);

            CollectionAssert.AreEqual(File.ReadAllBytes("KitchenSink.bin"), result.Program,
                "The assembly process produced unexpected program bytes");
            Assert.AreEqual(0, result.Warnings.Length,
                "The assembly process returned unexpected warnings");
            Assert.AreEqual(0UL, result.EntryPoint,
                "The assembly process returned unexpected entry point");
        }

        [TestMethod]
        public void ExampleProgramsNoErrors()
        {
            string startDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = "Example Programs";

            foreach (string asmFile in Directory.EnumerateFiles(".", "*.asm", SearchOption.AllDirectories))
            {
                if (asmFile.EndsWith(".ext.asm", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip files only intended to be imported
                    continue;
                }
                Assembler asm = new("");
                asm.SetAssemblerVariable("RUNNING_UNIT_TESTS", 1);
                asm.AssembleLines(File.ReadAllLines(asmFile));
                AssemblyResult result = asm.GetAssemblyResult(true);

                Assert.AreNotEqual(0, result.Program.Length, "Example program \"{0}\" should not be empty", asmFile);
                Assert.AreEqual(0, result.Warnings.Length, "Example program \"{0}\" should not return any warnings", asmFile);
            }

            Environment.CurrentDirectory = startDirectory;
        }

        [TestMethod]
        public void SetEntryPoint()
        {
            Assembler asm = new("");
            asm.AssembleLines(new[] { "%PAD 56", "    :ENTRY" });
            AssemblyResult result = asm.GetAssemblyResult(true);

            Assert.AreEqual(56UL, result.EntryPoint);
        }

        [TestMethod]
        public void STOP_Directive()
        {
            _ = Assert.ThrowsException<AssemblyStoppedException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%STOP", "BAD" }));

            AssemblyStoppedException exc = Assert.ThrowsException<AssemblyStoppedException>(() =>
                new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%STOP \"ABCDEFG\n\nmy message :)\"", "BAD" }));
            Assert.AreEqual("ABCDEFG\n\nmy message :)", exc.Message, "%STOP exception had incorrect message");
        }

        [TestMethod]
        public void MESSAGE_Directive()
        {
            Assembler asm = new("");
            asm.AssembleLines(new[] { "%MESSAGE warning", "%MESSAGE suggestion", "%MESSAGE error" });
            AssemblyResult result = asm.GetAssemblyResult(true);

            Assert.AreEqual(3, result.Warnings.Length);

            Assert.AreEqual(0, result.Warnings[0].Code);
            Assert.AreEqual(WarningSeverity.Warning, result.Warnings[0].Severity);

            Assert.AreEqual(0, result.Warnings[1].Code);
            Assert.AreEqual(WarningSeverity.Suggestion, result.Warnings[1].Severity);

            Assert.AreEqual(0, result.Warnings[2].Code);
            Assert.AreEqual(WarningSeverity.NonFatalError, result.Warnings[2].Severity);

            asm = new Assembler("");
            asm.AssembleLines(new[] { "%MESSAGE WarNing, \"test warning\"", "%MESSAGE suGGestIon, \"test suggestion\"", "%MESSAGE erroR, \"test error\"" });
            result = asm.GetAssemblyResult(true);

            Assert.AreEqual(3, result.Warnings.Length);

            Assert.AreEqual("test warning", result.Warnings[0].Message);
            Assert.AreEqual(0, result.Warnings[0].Code);
            Assert.AreEqual(WarningSeverity.Warning, result.Warnings[0].Severity);

            Assert.AreEqual("test suggestion", result.Warnings[1].Message);
            Assert.AreEqual(0, result.Warnings[1].Code);
            Assert.AreEqual(WarningSeverity.Suggestion, result.Warnings[1].Severity);

            Assert.AreEqual("test error", result.Warnings[2].Message);
            Assert.AreEqual(0, result.Warnings[2].Code);
            Assert.AreEqual(WarningSeverity.NonFatalError, result.Warnings[2].Severity);
        }

        [TestMethod]
        public void CompatibilityOptions()
        {
            Assembler asm = new("")
            {
                EnableObsoleteDirectives = true
            };
            asm.AssembleLines(new[] { "DAT 123", "PAD 4", "NUM 42", "MESSAGE warning" });
            AssemblyResult result = asm.GetAssemblyResult(true);
            Assert.AreEqual(1, result.Warnings.Length,
                "Obsolete MESSAGE directive did not produce a warning.");
            CollectionAssert.AreEqual(new byte[] { 123, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0 }, result.Program,
                "Obsolete directives did not produce correct program.");

            asm = new Assembler("")
            {
                EnableVariableExpansion = false
            };
            asm.AssembleLines(new[] { "%DAT \"Test @VARIABLE_TEST @ test @anotherTest@\"" });
            result = asm.GetAssemblyResult(true);
            CollectionAssert.AreEqual("Test @VARIABLE_TEST @ test @anotherTest@"u8.ToArray(), result.Program,
                "Variable was expanded when the feature was disabled.");

            asm = new Assembler("")
            {
                EnableEscapeSequences = false
            };
            asm.AssembleLines(new[] { "%DAT \"C:\\This\\is\\a\\raw\\file\\path \\\" \\\\ \\u \\U \\0 \\n\"" });
            result = asm.GetAssemblyResult(true);
            CollectionAssert.AreEqual(@"C:\This\is\a\raw\file\path "" \\ \u \U \0 \n"u8.ToArray(), result.Program,
                "Escape sequence was expanded when the feature was disabled.");
        }
    }
}
