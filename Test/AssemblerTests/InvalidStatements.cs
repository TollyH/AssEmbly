namespace AssEmbly.Test.AssemblerTests
{
    [TestClass]
    public class InvalidStatements
    {
        [TestMethod]
        public void BadOpcode()
        {
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("BAD", Array.Empty<string>()));
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("%BAD", Array.Empty<string>()));
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("456546FGDFG&&**\v\a\b\0   -++23432 ## ~", Array.Empty<string>()));
        }

        [TestMethod]
        public void WrongOperandCount()
        {
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("JMP", Array.Empty<string>()));
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("ADD", new[] { "rg0" }));
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "123", "999" }));
        }

        [TestMethod]
        public void WrongOperandTypes()
        {
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("ADD", new[] { "123", "rg0" }));
            _ = Assert.ThrowsException<OpcodeException>(() => Assembler.AssembleStatement("FEX", new[] { "rg0", "rg0" }));
        }

        [TestMethod]
        public void IndeterminableOperands()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "meaningless" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "meaningless" }));
        }

        [TestMethod]
        public void InvalidNumericLiterals()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "123bd" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0b11011013001" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0x99234987g" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "." }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "-." }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "-" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "3.5.6" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "--3" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "-5-6" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "543-" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "5-6" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0x5.6" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0X-5" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0b11011.11" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0b-100011101" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0_b110101101011" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "_0b110101101011" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "_123" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0_x123" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "\"Hello\"" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0x_" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0x" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "0b" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "_" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "18446744073709551616" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("ADD", new[] { "rg0", "-18446744073709551613" }));
        }

        [TestMethod]
        public void InvalidLabels()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":invalid-label" }));
        }

        [TestMethod]
        public void InvalidPointerDisplacements()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[-]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[--]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[-.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[-.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[-.]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[.]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[--3]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[_123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[_123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[_123]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[3.5.6]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[3.5.6]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[3.5.6]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[3-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[3-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[3-]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[3-4]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[3-4]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[3-4]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[3+rg0]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[3+rg0]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[3+rg0]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[3*rg0]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[3*rg0]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[3*rg0]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rxx]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rxx]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rxx]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0+]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0+]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0+]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0+rg1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0+rg1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0+rg1]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0-]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0--3]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*69]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*69]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*69]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*256]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*256]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*256]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*8*8]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*8*8]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*8*8]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*8*]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*8*]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*8*]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*8+]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*8+]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*8+]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*8-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*8-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*8-]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0*8--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0*8--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0*8--]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg0**8-5]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg0**8-6]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg0**8-7]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[:]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "D*rg0[:&]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[:LABEL]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[-:&LABEL]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[:&LABEL+1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[:&LABEL-1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[:12345]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "*rg0[rg2+:]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "D*rg0[rg4+:&]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4*8+:LABEL]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4-:&LABEL]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4+:&LABEL+1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4+:&LABEL-1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4*2+:&LABEL+1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "B*rg4[rg4*2+:&LABEL-1]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Q*rg8[rg4+:12345]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "X*rg0" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Y*rg4" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Z*rg8" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "QQ*rg0" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "QD*rg4" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "BW*rg8" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", "rg0[rg0*8]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Brg4[rg0*8]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", "Qrg8[rg0*8]" }));
        }

        [TestMethod]
        public void InvalidAddressDisplacements()
        {
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[-]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[-]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[--]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[--]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[-.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[-.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[-.]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[.]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[.]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[--3]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[--3]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[_123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[_123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[_123]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[:&LABEL+123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[:&LABEL+123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[:&LABEL+123]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[:&LABEL-123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[:&LABEL-123]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[:&LABEL-123]" }));

            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("JMP", new[] { "rg0", ":12[3.5.6]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":LABEL[3.5.6]" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("WCN", new[] { "rg0", ":&LABEL[3.5.6]" }));
        }

        [TestMethod]
        public void DAT_Directive()
        {
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", Array.Empty<string>()));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", new[] { "12", "34" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", new[] { "rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", new[] { "*rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", new[] { ":&LABEL" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%DAT", new[] { "256" }));
        }

        [TestMethod]
        public void NUM_Directive()
        {
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%NUM", Array.Empty<string>()));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%NUM", new[] { "12", "34" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%NUM", new[] { "rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%NUM", new[] { "*rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%NUM", new[] { "18446744073709551616" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("%NUM", new[] { "\"Hello\"" }));
        }

        [TestMethod]
        public void PAD_Directive()
        {
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", Array.Empty<string>()));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", new[] { "12", "34" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", new[] { "rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", new[] { "*rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", new[] { ":&LABEL" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%PAD", new[] { "18446744073709551616" }));
            _ = Assert.ThrowsException<SyntaxError>(() => Assembler.AssembleStatement("%PAD", new[] { "\"Hello\"" }));
        }

        [TestMethod]
        public void IBF_Directive()
        {
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", Array.Empty<string>()));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { "\"Hello\"", "\"Hello\"" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { "12" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { "rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { "*rg0" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { ":&LABEL" }));
            _ = Assert.ThrowsException<OperandException>(() => Assembler.AssembleStatement("%IBF", new[] { "18446744073709551616" }));
            _ = Assert.ThrowsException<ImportException>(() => Assembler.AssembleStatement("%IBF", new[] { "\"This file does not exist\"" }));
        }
    }
}
