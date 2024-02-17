namespace AssEmbly
{
    public static class Extensions
    {
        public static void SetContentTo<T>(this Stack<T> target, Stack<T> source)
        {
            target.Clear();
            foreach (T frame in source.Reverse())
            {
                target.Push(frame);
            }
        }

        public static Stack<T> NestedCopy<T>(this Stack<T> stack) where T : ICloneable
        {
            return new Stack<T>(stack.Select(i => (T)i.Clone()));
        }
    }
}
