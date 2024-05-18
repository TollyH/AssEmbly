namespace AssEmbly.Test.AssemblerTests
{
    [TestClass]
    public class InvalidPrograms
    {
        [TestMethod]
        public void BadLabelSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { ":LA BEL" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { ":LA-BEL" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { ":&LABEL" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { ":LABEL[22]" }));
            _ = Assert.ThrowsException<LabelNameException>(() => new Assembler("").AssembleLines(new[] { ":LABEL", ":LABEL" }));

            Assembler asm = new("");
            asm.AssembleLines(new[] { "MVQ rg0, :NOT_EXISTS" });
            // Non-existent label references should only throw exception when finalizing
            _ = asm.GetAssemblyResult(false);
            _ = Assert.ThrowsException<LabelNameException>(() => asm.GetAssemblyResult(true));

            Assembler asm2 = new("");
            asm2.AssembleLines(new[] { ":NEW_LABEL_LINK", "%LABEL_OVERRIDE :&NOT_EXISTS" });
            // Non-existent label references should only throw exception when finalizing
            _ = asm2.GetAssemblyResult(false);
            _ = Assert.ThrowsException<LabelNameException>(() => asm2.GetAssemblyResult(true));
        }

        [TestMethod]
        public void MissingEndingTags()
        {
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%IF NDEF, not_exists" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "ASMX_CLF", "%IF DEF, not_exists" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%PAD 42", "%WHILE GT, @!ASSEMBLER_VERSION_MAJOR, 0" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%DAT 12", "%MACRO macro" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%NUM 420", "%REPEAT 92" }));
        }

        [TestMethod]
        public void BadEndingTags()
        {
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%IF NDEF, not_exists", "%ENDWHILE" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "ASMX_CLF", "%IF DEF, not_exists", "%ENDREPEAT" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%PAD 42", "%WHILE GT, @!ASSEMBLER_VERSION_MAJOR, 0", "%ENDIF" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%DAT 12", "%MACRO macro", "%ELSE_IF NDEF, not_exists" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%DAT 12", "%WHILE GT, @!ASSEMBLER_VERSION_MAJOR, 0", "%ELSE_IF NDEF, not_exists" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%DAT 12", "%WHILE GT, @!ASSEMBLER_VERSION_MAJOR, 0", "%ELSE" }));
            _ = Assert.ThrowsException<EndingDirectiveException>(() => new Assembler("").AssembleLines(new[] { "%NUM 420", "%REPEAT 92", "%ENDMACRO" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%IF NDEF, not_exists", "%ENDIF rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, 45", "%MACRO macro", "%ENDMACRO rg0" }));
        }

        [TestMethod]
        public void BadSeparationSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ, rg0, 45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0 45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ, rg0 45" }));
        }

        [TestMethod]
        public void BadOperandSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg 0, 45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0,, 45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0,  , 45" }));
        }

        [TestMethod]
        public void BadStringSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, \'h" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, \'\'" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, \"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0\"\", 45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"\"bab" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "\"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, \'hi\'" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\u45" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\u4567" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\U4567" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\x" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\u45\"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\u45fg\"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\U0000000G\"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DAT \"bab\\UFFFFFFFF\"" }));
        }

        [TestMethod]
        public void BadMacroSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%MACRO test, $1", "test(CFL" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%MACRO test, $", "test(CFL)" }));
            _ = Assert.ThrowsException<MacroExpansionException>(() => new Assembler("").AssembleLines(new[] { "%MACRO test, $1!", "test" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "!>", "!>" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "<!" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%MACRO test", "$1", "%ENDMACRO", "test(CFL))" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%MACRO test", "$", "%ENDMACRO", "test(CFL)" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%MACRO test", "$", "%ENDMACRO", "test" }));
            _ = Assert.ThrowsException<MacroExpansionException>(() => new Assembler("").AssembleLines(new[] { "%MACRO test", "test", "%ENDMACRO", "test" }));
        }

        [TestMethod]
        public void BadVariableSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DEFINE bad-name, 5" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "%DEFINE !bad_name, 5" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, @" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, @!" }));
            _ = Assert.ThrowsException<VariableNameException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, @not_exists" }));
            _ = Assert.ThrowsException<VariableNameException>(() => new Assembler("").AssembleLines(new[] { "MVQ rg0, @!not_exists" }));
        }

        [TestMethod]
        public void BadDisplacementSyntax()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :12[]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :LABEL[]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :&LABEL[]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, *rg0[]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :12[21" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :LABEL[21" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :&LABEL[21" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, *rg0[21" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :12[21]]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :LABEL[21]]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :&LABEL[21]]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, *rg0[21]]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :12 [21]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :LABEL [21]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, :&LABEL [21]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0, *rg0 [21]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "JMP rg0,[21]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "[21]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "[]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "[" }));

            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "a[]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => new Assembler("").AssembleLines(new[] { "a[" }));
        }

        [TestMethod]
        public void BadConditionalOperands()
        {
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF BAD, 4, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE BAD, 4, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF DEF, 4, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE DEF, 4, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF GT, 4" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE GT, 4" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF GT, rg0, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE GT, rg0, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%IF GT, :&LABEL, 5" }));
            _ = Assert.ThrowsException<OperandException>(() => new Assembler("").AssembleLines(new[] { "%WHILE GT, :&LABEL, 5" }));
        }

        [TestMethod]
        public void InfiniteLoops()
        {
            _ = Assert.ThrowsException<MacroExpansionException>(() => new Assembler("").AssembleLines(new[] { "%MACRO test, test", "test" }));
            _ = Assert.ThrowsException<WhileLimitExceededException>(() => new Assembler("").AssembleLines(new[] { "%WHILE NDEF, not_exists", "%ENDWHILE" }));
        }
    }
}
