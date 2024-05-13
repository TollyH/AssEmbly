using System.ComponentModel;
using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    internal static partial class Program
    {
        public static readonly ulong DefaultMemorySize = 8192;  // 8KB

        // Shared methods that are used by multiple commands
        public static AAPFile? LoadAAPFile(string appPath, bool ignoreNewerVersion)
        {
            AAPFile file;
            try
            {
                file = new AAPFile(File.ReadAllBytes(appPath));
            }
            catch
            {
                PrintFatalError(Strings_CommandLine.Error_Invalid_AAP);
                return null;
            }
            if ((file.Features & AAPFeatures.Incompatible) != 0)
            {
                PrintFatalError(Strings_CommandLine.Error_AAP_Feature_Incompatible);
                return null;
            }
            if (!ignoreNewerVersion && file.LanguageVersion > (version ?? new Version()))
            {
                PrintWarning(Strings_CommandLine.Warning_Newer_Build_Version,
                    file.LanguageVersion.Major, file.LanguageVersion.Minor, file.LanguageVersion.Build, version?.Major, version?.Minor, version?.Build);
                if (file.LanguageVersion.Major > version?.Major)
                {
                    PrintFatalError(Strings_CommandLine.Error_Newer_Major_Build_Version, file.LanguageVersion.Major, version.Major);
                    return null;
                }
            }
            return file;
        }

        public static bool CheckInputFileArg(string[] args, [Localizable(true)] string missingMessage)
        {
            if (args.Length < 2)
            {
                PrintFatalError(missingMessage);
                return false;
            }
            if (!File.Exists(args[1]))
            {
                PrintFatalError(Strings_CommandLine.Error_File_Not_Exists);
                return false;
            }
            return true;
        }

        public static (string ArgPath, string Filename) ResolveInputFilePath(string[] args)
        {
            string argPath = Path.GetFullPath(args[1]);
            string parent = Path.GetDirectoryName(argPath)!;
            string filename = Path.GetFileNameWithoutExtension(argPath);
            Environment.CurrentDirectory = Path.GetFullPath(parent);
            return (argPath, filename);
        }

        public static void PrintFatalError([Localizable(true)] string errorText,
            [Localizable(true)] params object?[]? formatParams)
        {
            PrintError(errorText, formatParams);
#if !DEBUG
            Environment.Exit(1);
#endif
        }

        public static void PrintError([Localizable(true)] string errorText,
            [Localizable(true)] params object?[]? formatParams)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorText, formatParams);
            Console.ResetColor();
        }

        public static void PrintWarning([Localizable(true)] string warningText,
            [Localizable(true)] params object?[]? formatParams)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warningText, formatParams);
            Console.ResetColor();
        }

#if PROCESSOR
        public static Processor? LoadExecutableToProcessor(string appPath, ulong memSize,
            bool useV1Format, bool useV1CallStack, bool ignoreNewerVersion, bool mapStack, bool autoEcho)
        {
            byte[] program;
            Processor processor;
#if V1_CALL_STACK_COMPAT
            if (useV1Format)
            {
                program = File.ReadAllBytes(appPath);
                processor = new Processor(memSize, entryPoint: 0, useV1CallStack: true, mapStack: mapStack, autoEcho: autoEcho);
            }
            else
#endif
            {
                AAPFile? file = LoadAAPFile(appPath, ignoreNewerVersion);
                if (file is null)
                {
                    return null;
                }
                processor = new Processor(memSize, entryPoint: file.EntryPoint,
#if V1_CALL_STACK_COMPAT
                    useV1CallStack: useV1CallStack || file.Features.HasFlag(AAPFeatures.V1CallStack),
#endif
                    mapStack: mapStack, autoEcho: autoEcho);
                program = file.Program;
            }
            LoadProgramIntoProcessor(processor, program);
            return processor;
        }

        public static void LoadProgramIntoProcessor(Processor processor, byte[] program)
        {
            try
            {
                processor.LoadProgram(program);
            }
            catch (Exception e)
            {
                PrintFatalError(Strings_CommandLine.Error_Program_Load_Unexpected, e.GetType().Name, e.Message);
#if DEBUG
                throw;
#endif
            }
        }

        public static ulong GetMemorySize(CommandLineArgs args)
        {
            if (args.TryGetKeyValueOption("mem-size", out string? memSizeString))
            {
                if (!ulong.TryParse(memSizeString, out ulong memSize))
                {
                    PrintFatalError(Strings_CommandLine.Error_Invalid_Memory_Size, memSizeString);
                    return 0;
                }
                return memSize;
            }
            return DefaultMemorySize;
        }

        public static void OnExecutionException(Exception e, Processor processor)
        {
            if (e is IndexOutOfRangeException or ArgumentOutOfRangeException or RuntimeException
                or DivideByZeroException or FileNotFoundException or DirectoryNotFoundException)
            {
                string message = e switch
                {
                    RuntimeException runtimeException => runtimeException.ConsoleMessage,
                    DivideByZeroException => Strings_CommandLine.Error_Runtime_Zero_Divide,
                    FileNotFoundException or DirectoryNotFoundException => e.Message,
                    _ => Strings_CommandLine.Error_Runtime_Invalid_Address
                };
                PrintError(Strings_CommandLine.Error_Runtime_Known, message);
            }
            else
            {
                PrintError(Strings_CommandLine.Error_Unexpected_With_Type, e.GetType().Name, e.Message);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            PrintRegisterStates(processor);
            Console.ResetColor();
        }

        public static void ExecuteProcessor(Processor processor)
        {
            try
            {
                _ = processor.Execute(true);

                if (processor.IsFileOpen)
                {
                    PrintWarning(Strings_CommandLine.Warning_Processor_Exit_File_Open);
                }
#if EXTENSION_SET_EXTERNAL_ASM
                if (processor.IsExternalOpen)
                {
                    PrintWarning(Strings_CommandLine.Warning_Processor_Exit_External_Open);
                }
#endif
#if EXTENSION_SET_HEAP_ALLOCATE
                if (processor.AnyRegionsMapped)
                {
                    PrintWarning(Strings_CommandLine.Warning_Processor_Exit_Region_Mapped, processor.MappedMemoryRanges.Count - 2);
                }
#endif
            }
            catch (Exception e)
            {
                OnExecutionException(e, processor);
#if DEBUG
                throw;
#else
                Environment.Exit(1);
                return;
#endif
            }
        }

        public static void PrintRegisterStates(Processor processor)
        {
            Console.WriteLine(Strings.Generic_Register_States_Header);
            foreach (int register in Enum.GetValues(typeof(Register)))
            {
                ulong value = processor.Registers[register];
                Console.Write(Strings.Generic_Indented_Key_Value, Enum.GetName((Register)register), value);
                if (value != 0)
                {
                    double floatingValue = BitConverter.UInt64BitsToDouble(value);
                    // Don't print extreme values as floating point
                    if (Math.Abs(floatingValue) is >= 0.0000000000000001 and <= ulong.MaxValue)
                    {
                        Console.Write(Strings.Generic_Register_Floating_Value, floatingValue);
                    }
                    if (value >= 10)
                    {
                        Console.Write(Strings.Generic_Register_Hex_Value, value);
                    }
                    if ((value & Processor.SignBit) != 0)
                    {
                        Console.Write(Strings.Generic_Register_Denary_Value, (long)value);
                    }
                    else
                    {
                        Console.Write(Strings.Generic_Register_Binary_Value, Convert.ToString((long)value, 2));
                    }
                    // >= ' ' and <= '~'
                    if (value is >= 32 and <= 126)
                    {
                        Console.Write(Strings.Generic_Register_Char_Value, (char)value);
                    }
                }
                Console.WriteLine();
            }
        }
#endif

#if ASSEMBLER
        public static int GetMacroLimit(CommandLineArgs args)
        {
            if (args.TryGetKeyValueOption("macro-limit", out string? macroLimitString))
            {
                if (!int.TryParse(macroLimitString, out int macroLimit))
                {
                    PrintFatalError(Strings_CommandLine.Error_Invalid_Macro_Limit, macroLimitString);
                    return -1;
                }
                return macroLimit;
            }
            return -1;
        }

        public static int GetWhileLimit(CommandLineArgs args)
        {
            if (args.TryGetKeyValueOption("while-limit", out string? whileLimitString))
            {
                if (!int.TryParse(whileLimitString, out int whileLimit))
                {
                    PrintFatalError(Strings_CommandLine.Error_Invalid_While_Limit, whileLimitString);
                    return -1;
                }
                return whileLimit;
            }
            return -1;
        }

        public static List<(string Name, ulong Value)> GetVariableDefinitions(CommandLineArgs args)
        {
            if (args.TryGetKeyValueOption("define", out string? definitionString))
            {
                List<(string Name, ulong Value)> result = new();
                foreach (string variableDefinition in definitionString.Split(','))
                {
                    string[] split = variableDefinition.Split(':', 2);
                    ulong value = 0;
                    if (split.Length == 2 && !ulong.TryParse(split[1], out value))
                    {
                        PrintFatalError(Strings_CommandLine.Error_Invalid_Variable_Value, split[1]);
                        return new List<(string Name, ulong Value)>();
                    }
                    string name = split[0];
                    if (name.Length == 0)
                    {
                        continue;
                    }
                    result.Add((name, value));
                }
                return result;
            }
            return new List<(string Name, ulong Value)>();
        }

        public static void OnAssemblerException(Exception e)
        {
            if (e is AssemblerException assemblerException)
            {
                PrintError(assemblerException.ConsoleMessage);
            }
            else
            {
                PrintError(Strings_CommandLine.Error_Unexpected_With_Type, e.GetType().Name, e.Message);
            }
        }
#endif
    }
}
