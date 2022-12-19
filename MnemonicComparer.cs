namespace AssEmbly
{
    public class MnemonicComparer : EqualityComparer<(string, Data.OperandType[])>
    {
        public override bool Equals((string, Data.OperandType[]) first, (string, Data.OperandType[]) second)
        {
            return first.Item1 == second.Item1 && first.Item2.SequenceEqual(second.Item2);
        }

        public override int GetHashCode((string, Data.OperandType[]) obj)
        {
            return obj.Item1.GetHashCode();
        }
    }
}
