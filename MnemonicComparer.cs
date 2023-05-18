namespace AssEmbly
{
    public class MnemonicComparer : EqualityComparer<(string Mnemonic, Data.OperandType[] OperandTypes)>
    {
        public override bool Equals((string Mnemonic, Data.OperandType[] OperandTypes) first, (string Mnemonic, Data.OperandType[] OperandTypes) second)
        {
            return first.Mnemonic == second.Mnemonic && first.OperandTypes.SequenceEqual(second.OperandTypes);
        }

        public override int GetHashCode((string Mnemonic, Data.OperandType[] OperandTypes) obj)
        {
            return obj.Mnemonic.GetHashCode();
        }
    }
}
