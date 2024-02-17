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
    }
}
