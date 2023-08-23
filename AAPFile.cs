namespace AssEmbly
{
    [Flags]
    public enum AAPFeatures
    {
        V1CallStack = 0b1,
        ExtensionSigned = 0b10,
        ExtensionFloat = 0b100,
    }

    public class AAPFile
    {
        // "AssEmbly"
        public static readonly byte[] MagicBytes = { 65, 115, 115, 69, 109, 98, 108, 121 };

        // Header
        public Version LanguageVersion { get; set; }
        public AAPFeatures Features { get; set; }
        public ulong EntryPoint { get; set; }

        public byte[] Program { get; set; }

        public AAPFile(Version languageVersion, AAPFeatures features, ulong entryPoint, byte[] program)
        {
            LanguageVersion = languageVersion;
            Features = features;
            EntryPoint = entryPoint;
            Program = program;
        }
    }
}
