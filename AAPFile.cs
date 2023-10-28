using System.Buffers.Binary;

namespace AssEmbly
{
    [Flags]
    public enum AAPFeatures : ulong
    {
        None = 0,
        V1CallStack = 0b1,
        ExtensionSigned = 0b10,
        ExtensionFloat = 0b100,
        ExtensionExtendedBase = 0b1000,

        All = 0b1111,
        Incompatible = ~All
    }

    public class AAPFile
    {
        public const int HeaderSize = 36;
        // "AssEmbly"
        public static readonly byte[] MagicBytes = { 65, 115, 115, 69, 109, 98, 108, 121 };

        // Header
        public Version LanguageVersion { get; }
        public AAPFeatures Features { get; }
        public ulong EntryPoint { get; }

        public byte[] Program { get; }

        public AAPFile(Version languageVersion, AAPFeatures features, ulong entryPoint, byte[] program)
        {
            LanguageVersion = languageVersion;
            Features = features;
            EntryPoint = entryPoint;
            Program = program;
        }

        public AAPFile(byte[] executable)
        {
            if (executable.Length < HeaderSize)
            {
                throw new AAPFormatException("There are not enough bytes in the given array to be a valid AAP file");
            }
            if (!executable[..8].SequenceEqual(MagicBytes))
            {
                throw new AAPFormatException("Given bytes do not start with the correct header");
            }
            Span<byte> byteSpan = executable.AsSpan();

            int major = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[8..]);
            int minor = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[12..]);
            int build = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[16..]);
            LanguageVersion = new Version(major, minor, build);

            Features = (AAPFeatures)BinaryPrimitives.ReadUInt64LittleEndian(byteSpan[20..]);
            EntryPoint = BinaryPrimitives.ReadUInt64LittleEndian(byteSpan[28..]);

            Program = executable[36..];
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[HeaderSize + Program.Length];
            Span<byte> byteSpan = bytes.AsSpan();

            MagicBytes.CopyTo(bytes, 0);

            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[8..], LanguageVersion.Major);
            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[12..], LanguageVersion.Minor);
            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[16..], LanguageVersion.Build);

            BinaryPrimitives.WriteUInt64LittleEndian(byteSpan[20..], (ulong)Features);
            BinaryPrimitives.WriteUInt64LittleEndian(byteSpan[28..], EntryPoint);

            Program.CopyTo(bytes, HeaderSize);

            return bytes;
        }
    }
}
