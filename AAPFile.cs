using System.Buffers.Binary;
using System.IO.Compression;
using AssEmbly.Resources.Localization;

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
        GZipCompressed = 0b10000,
        ExtensionExternalAssembly = 0b100000,
        ExtensionMemoryAllocation = 0b1000000,
        ExtensionFileSystem = 0b10000000,
        ExtensionTerminal = 0b100000000,

        All = 0b111111111,
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
                throw new AAPFormatException(Strings.AAP_Error_Invalid_Not_Enough_Bytes);
            }
            if (!executable[..8].SequenceEqual(MagicBytes))
            {
                throw new AAPFormatException(Strings.AAP_Error_Invalid_Bad_Header);
            }
            Span<byte> byteSpan = executable.AsSpan();

            int major = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[8..]);
            int minor = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[12..]);
            int build = BinaryPrimitives.ReadInt32LittleEndian(byteSpan[16..]);
            LanguageVersion = new Version(major, minor, build);

            Features = (AAPFeatures)BinaryPrimitives.ReadUInt64LittleEndian(byteSpan[20..]);
            EntryPoint = BinaryPrimitives.ReadUInt64LittleEndian(byteSpan[28..]);

            if (Features.HasFlag(AAPFeatures.GZipCompressed))
            {
                using MemoryStream compressedProgram = new(executable[36..]);
                using GZipStream decompressor = new(compressedProgram, CompressionMode.Decompress);
                using MemoryStream decompressedProgram = new();
                decompressor.CopyTo(decompressedProgram);
                Program = decompressedProgram.ToArray();
            }
            else
            {
                Program = executable[36..];
            }
        }

        public byte[] GetBytes()
        {
            byte[] programBytes;
            if (Features.HasFlag(AAPFeatures.GZipCompressed))
            {
                using MemoryStream compressedProgram = new();
                using (GZipStream compressor = new(compressedProgram, CompressionLevel.SmallestSize, true))
                {
                    compressor.Write(Program);
                }
                programBytes = compressedProgram.ToArray();
            }
            else
            {
                programBytes = Program;
            }

            byte[] bytes = new byte[HeaderSize + programBytes.Length];
            Span<byte> byteSpan = bytes.AsSpan();

            MagicBytes.CopyTo(bytes, 0);

            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[8..], LanguageVersion.Major);
            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[12..], LanguageVersion.Minor);
            BinaryPrimitives.WriteInt32LittleEndian(byteSpan[16..], LanguageVersion.Build);

            BinaryPrimitives.WriteUInt64LittleEndian(byteSpan[20..], (ulong)Features);
            BinaryPrimitives.WriteUInt64LittleEndian(byteSpan[28..], EntryPoint);

            programBytes.CopyTo(bytes, HeaderSize);

            return bytes;
        }
    }
}
