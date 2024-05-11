using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    public class CommandLineArgs
    {
        public bool PrintWarnings { get; set; } = true;

        public IEqualityComparer<char> CharArgComparer { get; }
        public IEqualityComparer<string> StringArgComparer { get; }

        private readonly List<string> positionalArgs = new();
        private readonly HashSet<char> singleCharacterOptions;
        private readonly HashSet<string> multiCharacterOptions;
        private readonly Dictionary<string, string> keyValueOptions;

        private readonly HashSet<char> consumedSingleCharacterOptions;
        private readonly HashSet<string> consumedMultiCharacterOptions;
        private readonly HashSet<string> consumedKeyValueOptions;

        private string[]? positionalArgsArrayCache = null;

        public CommandLineArgs(IEqualityComparer<string> stringArgComparer, IEqualityComparer<char> charArgComparer)
        {
            StringArgComparer = stringArgComparer;
            CharArgComparer = charArgComparer;

            singleCharacterOptions = new HashSet<char>(CharArgComparer);
            multiCharacterOptions = new HashSet<string>(StringArgComparer);
            keyValueOptions = new Dictionary<string, string>(StringArgComparer);

            consumedSingleCharacterOptions = new HashSet<char>(CharArgComparer);
            consumedMultiCharacterOptions = new HashSet<string>(StringArgComparer);
            consumedKeyValueOptions = new HashSet<string>(StringArgComparer);
        }

        /// <summary>
        /// Process a collection of arguments.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>
        ///     Arguments are processed in order, by default being added to a list of "positional arguments".
        ///     </item>
        ///     <item>
        ///     Arguments starting with a dash (-) are instead treated as options and are omitted from the positional argument list.
        ///     </item>
        ///     <item>
        ///     A single dash (-) denotes an option argument that is a single character long.
        ///     Multiple characters can come after a single dash and each will be processed as a separate argument.
        ///     </item>
        ///     <item>
        ///     Two dashes (--) denote an option argument with an arbitrary number of characters.
        ///     If one of these characters is an equals sign (=), the option is treated as a key/value pair, splitting on the first equals sign.
        ///     </item>
        ///     <item>
        ///     Two dashes (--) without any following characters will result in all subsequent arguments being treated as regular positional arguments,
        ///     even if they start with a dash.
        ///     </item>
        /// </list>
        /// </remarks>
        public void AddArguments(IEnumerable<string> args)
        {
            bool allArgsPositional = false;

            // New arguments are being added, so invalidate the old cache.
            positionalArgsArrayCache = null;

            foreach (string arg in args)
            {
                if (!allArgsPositional && arg.StartsWith('-') && arg.Length >= 2)
                {
                    if (arg[1] == '-')
                    {
                        if (arg.Length == 2)
                        {
                            // All arguments after a "--" argument should be treated as positional arguments,
                            // regardless of whether they start with a dash
                            allArgsPositional = true;
                            continue;
                        }
                        string argText = arg[2..];
                        int equalsIndex;
                        if ((equalsIndex = argText.IndexOf('=')) != -1)
                        {
                            string argKey = argText[..equalsIndex];
                            if (!keyValueOptions.TryAdd(argKey, argText[(equalsIndex + 1)..]) && PrintWarnings)
                            {
                                PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Exists_KeyValue, argKey));
                            }
                        }
                        else if (!multiCharacterOptions.Add(argText) && PrintWarnings)
                        {
                            PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Exists_MultiCharacter, argText));
                        }
                    }
                    else
                    {
                        foreach (char c in arg[1..])
                        {
                            if (!singleCharacterOptions.Add(c) && PrintWarnings)
                            {
                                PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Exists_SingleCharacter, c));
                            }
                        }
                    }
                }
                else
                {
                    positionalArgs.Add(arg);
                }
            }
        }

        /// <summary>
        /// Get all positional arguments in order. Does not include any option arguments.
        /// </summary>
        public string[] GetPositionalArguments()
        {
            return positionalArgsArrayCache ?? positionalArgs.ToArray();
        }

        public bool IsSingleCharacterOptionGiven(char argument)
        {
            _ = consumedSingleCharacterOptions.Add(argument);
            return singleCharacterOptions.Contains(argument);
        }

        public bool IsMultiCharacterOptionGiven(string argument)
        {
            _ = consumedMultiCharacterOptions.Add(argument);
            return multiCharacterOptions.Contains(argument);
        }

        /// <summary>
        /// Check if one or both of the given single-character and multi-character arguments are given.
        /// </summary>
        /// <remarks>This is intended for arguments that have both a single- and multi-character form.</remarks>
        public bool IsOptionGiven(char singleArgument, string multiArgument)
        {
            return IsSingleCharacterOptionGiven(singleArgument)
                || IsMultiCharacterOptionGiven(multiArgument);
        }

        public string GetKeyValueOption(string key)
        {
            _ = consumedKeyValueOptions.Add(key);
            return keyValueOptions[key];
        }

        public bool TryGetKeyValueOption(string key, [MaybeNullWhen(false)] out string value)
        {
            _ = consumedKeyValueOptions.Add(key);
            return keyValueOptions.TryGetValue(key, out value);
        }

        public string GetKeyValueOptionOrDefault(string key, string defaultValue)
        {
            _ = consumedKeyValueOptions.Add(key);
            return keyValueOptions.GetValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// Print a warning for each option argument that was given but hasn't been checked for.
        /// </summary>
        /// <remarks>This method ignores the value of <see cref="PrintWarnings"/>.</remarks>
        public void WarnUnconsumedOptions()
        {
            foreach (char singleArgument in singleCharacterOptions.Except(consumedSingleCharacterOptions))
            {
                PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Unconsumed_SingleCharacter, singleArgument));
            }
            foreach (string multiArgument in multiCharacterOptions.Except(consumedMultiCharacterOptions))
            {
                PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Unconsumed_MultiCharacter, multiArgument));
            }
            foreach (string key in keyValueOptions.Keys.Except(consumedKeyValueOptions))
            {
                PrintWarning(string.Format(Strings.CommandLineArgs_Warning_Unconsumed_KeyValue, key));
            }
        }

        /// <summary>
        /// Print a warning for each option argument that was given but hasn't been checked for,
        /// as well as any positional arguments beyond the given length.
        /// </summary>
        /// <remarks>This method ignores the value of <see cref="PrintWarnings"/>.</remarks>
        public void WarnUnconsumedOptions(int maxPositionalArgsLength)
        {
            WarnUnconsumedOptions();

            if (positionalArgs.Count > maxPositionalArgsLength)
            {
                int excess = positionalArgs.Count - maxPositionalArgsLength;
                PrintWarning(excess == 1
                    ? string.Format(Strings.CommandLineArgs_Warning_Unconsumed_Positional_Single, excess)
                    : string.Format(Strings.CommandLineArgs_Warning_Unconsumed_Positional_Multiple, excess));
            }
        }

        private static void PrintWarning([Localizable(true)] string warningText)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warningText);
            Console.ResetColor();
        }
    }
}
