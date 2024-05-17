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

#if DISPLACEMENT
        public static string GetMultiplierString(this DisplacementMultiplier multiplier)
        {
            return multiplier switch
            {
                DisplacementMultiplier.x2 => "2",
                DisplacementMultiplier.x4 => "4",
                DisplacementMultiplier.x8 => "8",
                DisplacementMultiplier.x16 => "16",
                DisplacementMultiplier.x32 => "32",
                DisplacementMultiplier.x64 => "64",
                DisplacementMultiplier.x128 => "128",
                _ => "?"  // Invalid value - won't assemble
            };
        }

        public static ulong GetByteCount(this DisplacementMode mode)
        {
            return mode switch
            {
                DisplacementMode.NoDisplacement => 1,
                DisplacementMode.Constant => 9,
                DisplacementMode.Register => 2,
                DisplacementMode.ConstantAndRegister => 10,
                _ => 0
            };
        }
#endif
    }
}
