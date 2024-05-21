using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssEmbly
{
    public readonly struct Opcode(byte extensionSet, byte instructionCode) : IEquatable<Opcode>
    {
        public const byte FullyQualifiedMarker = 0xFF;

        public readonly byte ExtensionSet = extensionSet;
        public readonly byte InstructionCode = instructionCode;

        /// <summary>
        /// Parse an opcode from an array of bytes at a given offset.
        /// </summary>
        /// <param name="bytes">The array of bytes to parse the opcode from.</param>
        /// <param name="offset">The index in <paramref name="bytes"/> to start parsing from.</param>
        /// <returns>An opcode based on the parsed value from <paramref name="bytes"/> at <paramref name="offset"/>.</returns>
        /// <remarks><paramref name="offset"/> will be incremented to the index of the end of the opcode by this method.</remarks>
        public static Opcode ParseBytes(Span<byte> bytes, ref ulong offset)
        {
            return bytes[(int)offset] == FullyQualifiedMarker
                ? new Opcode(bytes[(int)++offset], bytes[(int)++offset])
                : new Opcode(0x00, bytes[(int)offset]);
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
        public long Length => End - Start;

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
            return Start < range.End && range.Start < End;
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

    [Serializable]
    public readonly struct FilePosition(int line, string file) : IEquatable<FilePosition>
    {
        public readonly int Line = line;
        public readonly string File = file;

        public bool Equals(FilePosition other)
        {
            return Line == other.Line && string.Equals(File, other.File, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is FilePosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(Line);
            hashCode.Add(File, StringComparer.OrdinalIgnoreCase);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(FilePosition left, FilePosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FilePosition left, FilePosition right)
        {
            return !left.Equals(right);
        }
    }

#if DISPLACEMENT
    public readonly struct Pointer : IEquatable<Pointer>
    {
        // All modes
        public readonly DisplacementMode Mode;
        public readonly Register PointerRegister;
        public readonly PointerReadSize ReadSize;

        // Constant + ConstantAndRegister only
        public readonly long DisplacementConstant;

        // Register + ConstantAndRegister only
        public readonly Register OtherRegister;
        public readonly bool SubtractOtherRegister;
        public readonly DisplacementMultiplier OtherRegisterMultiplier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(Register pointerRegister, PointerReadSize readSize)
        {
            Mode = DisplacementMode.NoDisplacement;
            PointerRegister = pointerRegister;
            ReadSize = readSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(Register pointerRegister, PointerReadSize readSize, long displacementConstant)
        {
            Mode = DisplacementMode.Constant;
            PointerRegister = pointerRegister;
            ReadSize = readSize;

            DisplacementConstant = displacementConstant;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(Register pointerRegister, PointerReadSize readSize,
            Register otherRegister, bool subtract, DisplacementMultiplier otherRegisterMultiplier)
        {
            Mode = DisplacementMode.Register;
            PointerRegister = pointerRegister;
            ReadSize = readSize;

            OtherRegister = otherRegister;
            SubtractOtherRegister = subtract;
            OtherRegisterMultiplier = otherRegisterMultiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(Register pointerRegister, PointerReadSize readSize,
            Register otherRegister, bool subtract, DisplacementMultiplier otherRegisterMultiplier,
            long displacementConstant)
        {
            Mode = DisplacementMode.ConstantAndRegister;
            PointerRegister = pointerRegister;
            ReadSize = readSize;

            DisplacementConstant = displacementConstant;

            OtherRegister = otherRegister;
            SubtractOtherRegister = subtract;
            OtherRegisterMultiplier = otherRegisterMultiplier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(Span<byte> encodedBytes, out ulong byteCount)
        {
            Mode = GetDisplacementMode(encodedBytes[0]);
            ReadSize = (PointerReadSize)((encodedBytes[0] >> 4) & 0b11);
            PointerRegister = (Register)(encodedBytes[0] & 0b1111);

            byteCount = Mode.GetByteCount();

            switch (Mode)
            {
                case DisplacementMode.NoDisplacement:
                default:
                    return;
                case DisplacementMode.Constant:
                    DisplacementConstant = BinaryPrimitives.ReadInt64LittleEndian(encodedBytes[1..]);
                    return;
                case DisplacementMode.Register:
                    OtherRegisterMultiplier = (DisplacementMultiplier)((encodedBytes[1] >> 4) & 0b111);
                    OtherRegister = (Register)(encodedBytes[1] & 0b1111);
                    SubtractOtherRegister = (encodedBytes[1] & 0b10000000) != 0;
                    return;
                case DisplacementMode.ConstantAndRegister:
                    DisplacementConstant = BinaryPrimitives.ReadInt64LittleEndian(encodedBytes[1..]);
                    OtherRegisterMultiplier = (DisplacementMultiplier)((encodedBytes[9] >> 4) & 0b111);
                    OtherRegister = (Register)(encodedBytes[9] & 0b1111);
                    SubtractOtherRegister = (encodedBytes[9] & 0b10000000) != 0;
                    return;
            }
        }

        /// <summary>
        /// Get the byte representation of this pointer to store in an AssEmbly program.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[Mode.GetByteCount()];
            Span<byte> byteSpan = bytes.AsSpan();

            bytes[0] = (byte)(((byte)Mode << 6) | ((byte)ReadSize << 4) | (byte)PointerRegister);
            switch (Mode)
            {
                case DisplacementMode.NoDisplacement:
                default:
                    break;
                case DisplacementMode.Constant:
                    BinaryPrimitives.WriteInt64LittleEndian(byteSpan[1..], DisplacementConstant);
                    break;
                case DisplacementMode.Register:
                    bytes[1] = (byte)(((byte)OtherRegisterMultiplier << 4) | (byte)OtherRegister);
                    if (SubtractOtherRegister)
                    {
                        bytes[1] |= 0b10000000;
                    }
                    break;
                case DisplacementMode.ConstantAndRegister:
                    BinaryPrimitives.WriteInt64LittleEndian(byteSpan[1..], DisplacementConstant);
                    bytes[9] = (byte)(((byte)OtherRegisterMultiplier << 4) | (byte)OtherRegister);
                    if (SubtractOtherRegister)
                    {
                        bytes[9] |= 0b10000000;
                    }
                    break;
            }

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DisplacementMode GetDisplacementMode(byte firstByte)
        {
            return (DisplacementMode)(firstByte >> 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Pointer other)
        {
            if (Mode != other.Mode)
            {
                return false;
            }
            return Mode switch
            {
                DisplacementMode.NoDisplacement => PointerRegister == other.PointerRegister
                    && ReadSize == other.ReadSize,
                DisplacementMode.Constant => PointerRegister == other.PointerRegister
                    && ReadSize == other.ReadSize
                    && DisplacementConstant == other.DisplacementConstant,
                DisplacementMode.Register => PointerRegister == other.PointerRegister
                    && ReadSize == other.ReadSize
                    && OtherRegister == other.OtherRegister
                    && SubtractOtherRegister == other.SubtractOtherRegister
                    && OtherRegisterMultiplier == other.OtherRegisterMultiplier,
                DisplacementMode.ConstantAndRegister => PointerRegister == other.PointerRegister
                    && ReadSize == other.ReadSize
                    && DisplacementConstant == other.DisplacementConstant
                    && OtherRegister == other.OtherRegister
                    && SubtractOtherRegister == other.SubtractOtherRegister
                    && OtherRegisterMultiplier == other.OtherRegisterMultiplier,
                _ => false
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is Pointer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Mode switch
            {
                DisplacementMode.NoDisplacement => HashCode.Combine(Mode, PointerRegister),
                DisplacementMode.Constant => HashCode.Combine(Mode, PointerRegister, DisplacementConstant),
                DisplacementMode.Register => HashCode.Combine(Mode, PointerRegister,
                    OtherRegister, SubtractOtherRegister, OtherRegisterMultiplier),
                DisplacementMode.ConstantAndRegister => HashCode.Combine(Mode, PointerRegister,
                    DisplacementConstant, OtherRegister, SubtractOtherRegister, OtherRegisterMultiplier),
                _ => Mode.GetHashCode()
            };
        }

        public static bool operator ==(Pointer left, Pointer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pointer left, Pointer right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            StringBuilder sb = new();

            // Read size ('Q' is the default so can be omitted)
            if (ReadSize != PointerReadSize.QuadWord)
            {
                _ = sb.Append(ReadSize switch
                {
                    PointerReadSize.DoubleWord => 'D',
                    PointerReadSize.Word => 'W',
                    PointerReadSize.Byte => 'B',
                    _ => '?'  // Invalid value - won't assemble
                });
            }

            // Main register
            _ = sb.Append('*').Append(PointerRegister);

            // Displacement component
            switch (Mode)
            {
                case DisplacementMode.NoDisplacement:
                    break;
                case DisplacementMode.Constant:
                    _ = sb.Append('[')
                        .Append(DisplacementConstant)
                        .Append(']');
                    break;
                case DisplacementMode.Register:
                    _ = sb.Append('[');
                    if (SubtractOtherRegister)
                    {
                        _ = sb.Append('-');
                    }
                    _ = sb.Append(OtherRegister);
                    if (OtherRegisterMultiplier != DisplacementMultiplier.x1)
                    {
                        _ = sb.Append(" * ")
                            .Append(OtherRegisterMultiplier.GetMultiplierString());
                    }
                    _ = sb.Append(']');
                    break;
                case DisplacementMode.ConstantAndRegister:
                    _ = sb.Append('[');
                    if (SubtractOtherRegister)
                    {
                        _ = sb.Append('-');
                    }
                    _ = sb.Append(OtherRegister);
                    if (OtherRegisterMultiplier != DisplacementMultiplier.x1)
                    {
                        _ = sb.Append(" * ").Append(OtherRegisterMultiplier.GetMultiplierString());
                    }
                    _ = sb.Append(DisplacementConstant >= 0 ? " + " : " - ")
                        .Append(Math.Abs(DisplacementConstant))
                        .Append(']');
                    break;
                default:
                    _ = sb.Append("[?]");  // Invalid value - won't assemble
                    break;
            }

            return sb.ToString();
        }
    }

    public readonly struct AddressReference : IEquatable<AddressReference>
    {
        public readonly AddressReferenceType ReferenceType;

        // LabelAddress + LabelLiteral
        public readonly string LabelName;

        // LiteralAddress only
        public readonly ulong Address;

        // All reference types
        public readonly bool Displaced;
        public readonly long DisplacementConstant;

        public AddressReference(string labelName, bool literal)
        {
            ReferenceType = literal
                ? AddressReferenceType.LabelLiteral
                : AddressReferenceType.LabelAddress;

            LabelName = labelName;

            Displaced = false;
        }

        public AddressReference(string labelName, bool literal, long displacementConstant)
        {
            ReferenceType = literal
                ? AddressReferenceType.LabelLiteral
                : AddressReferenceType.LabelAddress;

            LabelName = labelName;

            Displaced = true;
            DisplacementConstant = displacementConstant;
        }

        public AddressReference(ulong address)
        {
            ReferenceType = AddressReferenceType.LiteralAddress;

            LabelName = "";
            Address = address;

            Displaced = false;
        }

        public AddressReference(ulong address, long displacementConstant)
        {
            ReferenceType = AddressReferenceType.LiteralAddress;

            LabelName = "";
            Address = address;

            Displaced = true;
            DisplacementConstant = displacementConstant;
        }

        public bool Equals(AddressReference other)
        {
            if (ReferenceType != other.ReferenceType)
            {
                return false;
            }
            if (Displaced != other.Displaced || (Displaced && DisplacementConstant != other.DisplacementConstant))
            {
                return false;
            }
            return ReferenceType switch
            {
                AddressReferenceType.LabelAddress
                    or AddressReferenceType.LabelLiteral => LabelName == other.LabelName,
                AddressReferenceType.LiteralAddress => Address == other.Address,
                _ => false
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is AddressReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(ReferenceType);
            hash.Add(Displaced);
            if (Displaced)
            {
                hash.Add(DisplacementConstant);
            }
            switch (ReferenceType)
            {
                case AddressReferenceType.LabelAddress:
                case AddressReferenceType.LabelLiteral:
                    hash.Add(LabelName);
                    break;
                case AddressReferenceType.LiteralAddress:
                    hash.Add(Address);
                    break;
            }
            return hash.ToHashCode();
        }

        public static bool operator ==(AddressReference left, AddressReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AddressReference left, AddressReference right)
        {
            return !left.Equals(right);
        }
    }
#endif
}
