﻿using AssEmbly.Resources.Localization;

namespace AssEmbly
{
    internal static partial class Program
    {
        public static readonly ulong DefaultMemorySize = 2046;

        // Shared methods that are used by multiple commands
        public static AAPFile LoadAAPFile(string appPath, bool ignoreNewerVersion)
        {
            AAPFile file;
            try
            {
                file = new AAPFile(File.ReadAllBytes(appPath));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Error_Invalid_AAP);
                Console.ResetColor();
                Environment.Exit(1);
                return null;
            }
            if ((file.Features & AAPFeatures.Incompatible) != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Error_AAP_Feature_Incompatible);
                Console.ResetColor();
                Environment.Exit(1);
                return null;
            }
            if (!ignoreNewerVersion && file.LanguageVersion > (version ?? new Version()))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Strings.CLI_Warning_Newer_Build_Version,
                    file.LanguageVersion.Major, file.LanguageVersion.Minor, file.LanguageVersion.Build, version?.Major, version?.Minor, version?.Build);
                Console.ResetColor();
                if (file.LanguageVersion.Major > version?.Major)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Strings.CLI_Error_Newer_Major_Build_Version, file.LanguageVersion.Major, version.Major);
                    Console.ResetColor();
                    Environment.Exit(1);
                    return null;
                }
            }
            return file;
        }

        public static Processor LoadExecutableToProcessor(string appPath, ulong memSize,
            bool useV1Format, bool useV1CallStack, bool ignoreNewerVersion)
        {
            byte[] program;
            Processor processor;
            if (useV1Format)
            {
                program = File.ReadAllBytes(appPath);
                processor = new Processor(memSize, entryPoint: 0, useV1CallStack: true);
            }
            else
            {
                AAPFile file = LoadAAPFile(appPath, ignoreNewerVersion);
                processor = new Processor(memSize, entryPoint: file.EntryPoint,
                    useV1CallStack: useV1CallStack || file.Features.HasFlag(AAPFeatures.V1CallStack));
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
                Console.WriteLine(Strings.CLI_Error_Program_Load_Unexpected, e.GetType().Name, e.Message);
            }
        }

        [System.ComponentModel.Localizable(true)]
        public static bool CheckInputFileArg(string[] args, string missingMessage)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(missingMessage);
                Console.ResetColor();
                Environment.Exit(1);
                return false;
            }
            if (!File.Exists(args[1]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Strings.CLI_Error_File_Not_Exists);
                Console.ResetColor();
                Environment.Exit(1);
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

        public static ulong GetMemorySize(string[] args)
        {
            foreach (string a in args)
            {
                if (a.ToLowerInvariant().StartsWith("--mem-size="))
                {
                    string memSizeString = a.Split("=")[1];
                    if (!ulong.TryParse(memSizeString, out ulong memSize))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Strings.CLI_Error_Invalid_Memory_Size, memSizeString);
                        Console.ResetColor();
                        Environment.Exit(1);
                        return 0;
                    }
                    return memSize;
                }
            }
            return DefaultMemorySize;
        }

        public static void OnExecutionException(Exception e, Processor processor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (e is IndexOutOfRangeException or ArgumentOutOfRangeException or RuntimeException
                or DivideByZeroException or FileNotFoundException or DirectoryNotFoundException)
            {
                string message = e switch
                {
                    RuntimeException runtimeException => runtimeException.ConsoleMessage,
                    DivideByZeroException => Strings.CLI_Error_Runtime_Zero_Divide,
                    FileNotFoundException or DirectoryNotFoundException => e.Message,
                    _ => Strings.CLI_Error_Runtime_Invalid_Address
                };
                Console.WriteLine(Strings.CLI_Error_Runtime_Known, message);
            }
            else
            {
                Console.WriteLine(Strings.CLI_Error_Unexpected_With_Type, e.GetType().Name, e.Message);
            }
            PrintRegisterStates(processor);
            Console.ResetColor();
        }

        public static void ExecuteProcessor(Processor processor)
        {
            try
            {
                _ = processor.Execute(true);
            }
            catch (Exception e)
            {
                OnExecutionException(e, processor);
            }
        }

        public static void OnAssemblerException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (e is AssemblerException assemblerException)
            {
                Console.WriteLine(assemblerException.ConsoleMessage);
            }
            else
            {
                Console.WriteLine(Strings.CLI_Error_Unexpected_With_Type, e.GetType().Name, e.Message);
            }
            Console.ResetColor();
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
    }
}
