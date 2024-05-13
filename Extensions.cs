namespace AssEmbly
{
    public static class Extensions
    {
        /// <summary>
        /// Convert raw text to its equivalent form as an AssEmbly string literal.
        /// </summary>
        /// <remarks>Neither the input nor the output to this function have surrounding quote marks.</remarks>
        public static string EscapeCharacters(this string unescaped)
        {
            return unescaped.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("@", "\\@");
        }

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
            return new Stack<T>(stack.Select(i => (T)i.Clone()).Reverse());
        }
    }
}
