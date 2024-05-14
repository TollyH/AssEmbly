using System.Buffers.Binary;
using System.IO.Compression;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    [Flags]
    public enum AAPFeatures : ulong
    {
        None = 0,
#if V1_CALL_STACK_COMPAT
        V1CallStack = 0b1,
#endif
#if EXTENSION_SET_SIGNED
        ExtensionSigned = 0b10,
#endif
#if EXTENSION_SET_FLOATING_POINT
        ExtensionFloat = 0b100,
#endif
#if EXTENSION_SET_EXTENDED_BASE
        ExtensionExtendedBase = 0b1000,
#endif
#if GZIP_COMPRESSION
        GZipCompressed = 0b10000,
#endif
#if EXTENSION_SET_EXTERNAL_ASM
        ExtensionExternalAssembly = 0b100000,
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
        ExtensionMemoryAllocation = 0b1000000,
#endif
#if EXTENSION_SET_FILE_SYSTEM
        ExtensionFileSystem = 0b10000000,
#endif
#if EXTENSION_SET_TERMINAL
        ExtensionTerminal = 0b100000000,
#endif
#if DISPLACEMENT
        PointerDisplacement = 0b1000000000,
#endif

        All = None
#if V1_CALL_STACK_COMPAT
            | V1CallStack
#endif
#if EXTENSION_SET_SIGNED
            | ExtensionSigned
#endif
#if EXTENSION_SET_FLOATING_POINT
            | ExtensionFloat
#endif
#if EXTENSION_SET_EXTENDED_BASE
            | ExtensionExtendedBase
#endif
#if GZIP_COMPRESSION
            | GZipCompressed
#endif
#if EXTENSION_SET_EXTERNAL_ASM
            | ExtensionExternalAssembly
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
            | ExtensionMemoryAllocation
#endif
#if EXTENSION_SET_FILE_SYSTEM
            | ExtensionFileSystem
#endif
#if EXTENSION_SET_TERMINAL
            | ExtensionTerminal
#endif
#if DISPLACEMENT
            | PointerDisplacement
#endif
        , Incompatible = ~All
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

#if GZIP_COMPRESSION
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
#endif
                Program = executable[36..];
#if GZIP_COMPRESSION
            }
#endif
        }

        public byte[] GetBytes()
        {
            byte[] programBytes;
#if GZIP_COMPRESSION
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
#endif
                programBytes = Program;
#if GZIP_COMPRESSION
            }
#endif

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
