﻿using System.Diagnostics;

namespace AssEmbly
{
    internal static partial class Program
    {
        public static readonly ulong DefaultMemorySize = 2046;

        // Shared methods that are used by multiple commands
        public static Processor? LoadExecutableToProcessor(string appPath, ulong memSize, bool useV1Format, bool useV1CallStack)
        {
            byte[] program;
            Processor processor;
            if (useV1Format)
            {
                program = File.ReadAllBytes(appPath);
                processor = new(memSize, entryPoint: 0, useV1CallStack: true);
            }
            else
            {
                AAPFile file;
                try
                {
                    file = new(File.ReadAllBytes(appPath));
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The given executable file is invalid. " +
                        "Make sure you're not attempting to execute the source file instead of the executable. " +
                        "To run an executable built in AssEmbly v1.x.x, use the --v1-format parameter.");
                    Console.ResetColor();
                    Environment.Exit(1);
                    return null;
                }
                program = file.Program;
                if ((file.Features & AAPFeatures.Incompatible) != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("This program uses features incompatible with the current version of AssEmbly.");
                    Console.ResetColor();
                    Environment.Exit(1);
                    return null;
                }
                if (file.LanguageVersion > (version ?? new Version()))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Warning: This program was assembled for a newer version of AssEmbly. It was built for version " +
                        $"{file.LanguageVersion.Major}.{file.LanguageVersion.Minor}.{file.LanguageVersion.Build} " +
                        $"- you have version {version?.Major}.{version?.Minor}.{version?.Build}.");
                    Console.ResetColor();
                }
                processor = new(memSize, entryPoint: file.EntryPoint,
                    useV1CallStack: useV1CallStack || file.Features.HasFlag(AAPFeatures.V1CallStack));
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
                Console.WriteLine($"An unexpected error occurred while loading your program:\r\n    {e.GetType().Name}: {e.Message}");
            }
        }

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
                Console.WriteLine("The specified file does not exist.");
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
                        Console.WriteLine($"{memSizeString} is not a valid number of bytes for memory size.");
                        Console.ResetColor();
                        Environment.Exit(1);
                        return memSize;
                    }
                }
            }
            return DefaultMemorySize;
        }

        public static void OnExecutionException(Exception e, Processor processor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (e is IndexOutOfRangeException or ArgumentOutOfRangeException or RuntimeException or DivideByZeroException)
            {
                string message = e is RuntimeException runtimeException
                    ? runtimeException.ConsoleMessage
                    : e is DivideByZeroException
                        ? "An instruction attempted to divide by zero."
                        : "An instruction tried to access an invalid memory address.";
                Console.WriteLine($"\n\nAn error occurred executing your program:\n    {message}\nRegister states:");
                if (processor is not null)
                {
                    foreach (int register in Enum.GetValues(typeof(Register)))
                    {
                        ulong value = processor.Registers[register];
                        Console.WriteLine($"    {Enum.GetName((Register)register)}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                    }
                }
            }
            else
            {
                Console.WriteLine($"An unexpected error occurred:\r\n    {e.GetType().Name}: {e.Message}");
            }
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
                Console.WriteLine($"An unexpected error occurred:\r\n    {e.GetType().Name}: {e.Message}");
            }
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}