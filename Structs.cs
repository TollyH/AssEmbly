namespace AssEmbly
{
    public readonly struct Opcode : IEquatable<Opcode>
    {
        public readonly byte ExtensionSet;
        public readonly byte InstructionCode;

        public Opcode(byte extensionSet, byte instructionCode)
        {
            ExtensionSet = extensionSet;
            InstructionCode = instructionCode;
        }

        /// <summary>
        /// Parse an opcode from an array of bytes at a given offset.
        /// </summary>
        /// <param name="bytes">The array of bytes to parse the opcode from.</param>
        /// <param name="offset">The index in <paramref name="bytes"/> to start parsing from.</param>
        /// <returns>An opcode based on the parsed value from <paramref name="bytes"/> at <paramref name="offset"/>.</returns>
        /// <remarks><paramref name="offset"/> will be incremented to the index of the end of the opcode by this method.</remarks>
        public static Opcode ParseBytes(Span<byte> bytes, ref ulong offset)
        {
            if (bytes[(int)offset] == 0xFF)
            {
                return new Opcode(bytes[(int)++offset], bytes[(int)++offset]);
            }
            return new Opcode(0x00, bytes[(int)offset]);
        }

        public override bool Equals(object? obj)
        {
            return obj is Opcode opcode && Equals(opcode);
        }

        public bool Equals(Opcode other)
        {
            return ExtensionSet == other.ExtensionSet &&
                InstructionCode == other.InstructionCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExtensionSet, InstructionCode);
        }

        public static bool operator ==(Opcode left, Opcode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Opcode left, Opcode right)
        {
            return !(left == right);
        }
    }

    public readonly struct Range : IEnumerable<long>, IEquatable<Range>
    {
        /// <summary>
        /// Inclusive start point of the range
        /// </summary>
        public readonly long Start;
        /// <summary>
        /// Exclusive end point of the range
        /// </summary>
        public readonly long End;

        public long LastIndex => End - 1;

        /// <param name="start">Inclusive start point of the range</param>
        /// <param name="end">Exclusive end point of the range</param>
        public Range(long start, long end)
        {
            if (start > end)
            {
                throw new ArgumentException("The start parameter must be less than or equal to end parameter.");
            }
            Start = start;
            End = end;
        }

        public bool Contains(long value)
        {
            return value >= Start && value < End;
        }

        public bool Contains(Range range)
        {
            return range.Start >= Start && range.End <= End;
        }

        public bool Overlaps(Range range)
        {
            return Start <= range.End && range.Start <= End;
        }

        public IEnumerator<long> GetEnumerator()
        {
            for (long i = Start; i < End; i++)
            {
                yield return i;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Range other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object? obj)
        {
            return obj is Range other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(Range left, Range right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Range left, Range right)
        {
            return !left.Equals(right);
        }
    }
}
