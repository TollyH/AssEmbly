namespace AssEmbly
{
    public class MnemonicComparer : EqualityComparer<(string Mnemonic, OperandType[] OperandTypes)>
    {
        public override bool Equals((string Mnemonic, OperandType[] OperandTypes) first, (string Mnemonic, OperandType[] OperandTypes) second)
        {
            return first.Mnemonic == second.Mnemonic && first.OperandTypes.SequenceEqual(second.OperandTypes);
        }

        public override int GetHashCode((string Mnemonic, OperandType[] OperandTypes) obj)
        {
            return obj.Mnemonic.GetHashCode();
        }
    }
}
