namespace AssEmbly
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!args.Contains("--no-header"))
            {
                Console.WriteLine("AssEmbly - A mock assembly language running on .NET");
                Console.WriteLine("Copyright © 2022  Ptolemy Hill");
                Console.WriteLine();
            }
            if (args.Length < 1)
            {
                Console.WriteLine("An operation to perform is required.");
                Environment.Exit(1);
                return;
            }
            switch (args[0])
            {
                case "assemble":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("A path to the program listing to be assembled is required.");
                        Environment.Exit(1);
                        return;
                    }
                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("The specified file does not exist.");
                        Environment.Exit(1);
                        return;
                    }
                    string filename = string.Join('.', args[1].Split('.')[..^1]);
                    byte[] program;
                    try
                    {
                        program = Assembler.AssembleLines(File.ReadAllLines(args[1]));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Data["UserMessage"]);
                        Environment.Exit(1);
                        return;
                    }
                    File.WriteAllBytes(filename + ".aap", program);
                    Console.WriteLine($"Program assembled into {program.LongLength} bytes successfully. It can be found at: \"{filename + ".aap"}\"");
                    break;
                case "execute":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("A path to the assembled program to be executed is required.");
                        Environment.Exit(1);
                        return;
                    }
                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("The specified file does not exist.");
                        Environment.Exit(1);
                        return;
                    }
                    Processor p = new(2046);
                    try
                    {
                        p.LoadProgram(File.ReadAllBytes(args[1]));
                        p.Execute();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n\nAn error occurred executing your program:\n    {e.Message}\nRegister states:");
                        foreach ((Data.Register register, ulong value) in p.Registers)
                        {
                            Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                        }
                    }
                    break;
                case "run":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("A path to the program listing to be executed is required.");
                        Environment.Exit(1);
                        return;
                    }
                    if (!File.Exists(args[1]))
                    {
                        Console.WriteLine("The specified file does not exist.");
                        Environment.Exit(1);
                        return;
                    }
                    p = new(2046);
                    try
                    {
                        program = Assembler.AssembleLines(File.ReadAllLines(args[1]));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Data["UserMessage"]);
                        Environment.Exit(1);
                        return;
                    }
                    try
                    {
                        p.LoadProgram(program);
                        p.Execute();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n\nAn error occurred running your program:\n    {e.Message}\nRegister states:");
                        foreach ((Data.Register register, ulong value) in p.Registers)
                        {
                            Console.WriteLine($"    {register}: {value} (0x{value:X}) (0b{Convert.ToString((long)value, 2)})");
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"\"{args[0]}\" is not a valid operation.");
                    Environment.Exit(1);
                    return;
            }
        }
    }
}