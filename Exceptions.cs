using System.ComponentModel;

namespace AssEmbly
{
    // BASE CLASS

    /// <summary>
    /// Represents errors that occur in any part of AssEmbly.
    /// </summary>
    /// <remarks>
    /// This is a base class for more specific exception types, which should most often be used instead.
    /// </remarks>
    public class AssEmblyException : Exception
    {
        private string? _consoleMessage;
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string ConsoleMessage {
            get => _consoleMessage ?? Message;
            set => _consoleMessage = value;
        }

        public AssEmblyException() { }
        public AssEmblyException([Localizable(true)] string message) : base(message) { }
        public AssEmblyException([Localizable(true)] string message, Exception inner) : base(message, inner) { }

        public AssEmblyException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message)
        {
            ConsoleMessage = consoleMessage;
        }

        public AssEmblyException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, inner)
        {
            ConsoleMessage = consoleMessage;
        }
    }

#if ASSEMBLER
    /// <summary>
    /// Represents errors that occur during the assembly of an AssEmbly source file.
    /// </summary>
    public class AssemblerException : AssEmblyException
    {
#if ASSEMBLER_WARNINGS
        public Warning WarningObject { get; set; }
#else
        public FilePosition Position { get; set; }
#endif

        public AssemblerException([Localizable(true)] string message) : base(message) { }
        public AssemblerException([Localizable(true)] string message, int line, string file) : base(message)
        {
#if ASSEMBLER_WARNINGS
            WarningObject = new Warning(
                WarningSeverity.FatalError, 0000, new FilePosition(line, file),
                "", Array.Empty<string>(), "", "", message);
#else
            Position = new FilePosition(line, file);
#endif
        }
    }

    /// <summary>
    /// The exception that is thrown when a error is encountered with the written format of an instruction during assembly.
    /// </summary>
    public class SyntaxError : AssemblerException
    {
        public SyntaxError([Localizable(true)] string message) : base(message) { }
        public SyntaxError([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the operands given for an instruction during assembly are invalid.
    /// </summary>
    public class OperandException : AssemblerException
    {
        public OperandException([Localizable(true)] string message) : base(message) { }
        public OperandException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst importing an external AssEmbly source file.
    /// </summary>
    public class ImportException : AssemblerException
    {
        public ImportException([Localizable(true)] string message) : base(message) { }
        public ImportException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst attempting to retrieve the opcode for an instruction.
    /// </summary>
    public class OpcodeException : AssemblerException
    {
        public OpcodeException([Localizable(true)] string message) : base(message) { }
        public OpcodeException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid label name during assembly.
    /// </summary>
    public class LabelNameException : AssemblerException
    {
        public LabelNameException([Localizable(true)] string message) : base(message) { }
        public LabelNameException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid macro name during assembly.
    /// </summary>
    public class MacroNameException : AssemblerException
    {
        public MacroNameException([Localizable(true)] string message) : base(message) { }
        public MacroNameException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid assembler variable/constant name during assembly.
    /// </summary>
    public class VariableNameException : AssemblerException
    {
        public VariableNameException([Localizable(true)] string message) : base(message) { }
        public VariableNameException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the end of the file is reached before a required closing directive is encountered.
    /// </summary>
    public class EndingDirectiveException : AssemblerException
    {
        public EndingDirectiveException([Localizable(true)] string message) : base(message) { }
        public EndingDirectiveException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst attempting to expand the usage of a macro.
    /// </summary>
    public class MacroExpansionException : AssemblerException
    {
        public MacroExpansionException([Localizable(true)] string message) : base(message) { }
        public MacroExpansionException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the program requests that assembly stop.
    /// </summary>
    public class AssemblyStoppedException : AssemblerException
    {
        public AssemblyStoppedException([Localizable(true)] string message) : base(message) { }
        public AssemblyStoppedException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the program exceeds the limit of allowed while repeats.
    /// </summary>
    public class WhileLimitExceededException : AssemblerException
    {
        public WhileLimitExceededException([Localizable(true)] string message) : base(message) { }
        public WhileLimitExceededException([Localizable(true)] string message, int line, string file) : base(message, line, file) { }
    }
#endif

    /// <summary>
    /// Represents exceptions specific to the AssEmbly debugger.
    /// </summary>
    public class DebuggerException : AssEmblyException
    {
        public DebuggerException() { }
        public DebuggerException([Localizable(true)] string message) : base(message) { }
        public DebuggerException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public DebuggerException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public DebuggerException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a given debug file is in an invalid format.
    /// </summary>
    public class DebugFileException : DebuggerException
    {
        public DebugFileException() { }
        public DebugFileException([Localizable(true)] string message) : base(message) { }
        public DebugFileException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public DebugFileException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public DebugFileException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

#if PROCESSOR
    /// <summary>
    /// Represents errors that occur during the execution of an AssEmbly program.
    /// </summary>
    public class RuntimeException : AssEmblyException
    {
        public RuntimeException() { }
        public RuntimeException([Localizable(true)] string message) : base(message) { }
        public RuntimeException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public RuntimeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public RuntimeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an opcode is not recognised by the AssEmbly processor.
    /// </summary>
    public class InvalidOpcodeException : RuntimeException
    {
        public InvalidOpcodeException() { }
        public InvalidOpcodeException([Localizable(true)] string message) : base(message) { }
        public InvalidOpcodeException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public InvalidOpcodeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public InvalidOpcodeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction attempts to write to a read-only register.
    /// </summary>
    public class ReadOnlyRegisterException : RuntimeException
    {
        public ReadOnlyRegisterException() { }
        public ReadOnlyRegisterException([Localizable(true)] string message) : base(message) { }
        public ReadOnlyRegisterException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public ReadOnlyRegisterException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public ReadOnlyRegisterException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction attempts to perform a file related operation that is
    /// invalid given the current state of the program's execution.
    /// </summary>
    public class FileOperationException : RuntimeException
    {
        public FileOperationException() { }
        public FileOperationException([Localizable(true)] string message) : base(message) { }
        public FileOperationException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public FileOperationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public FileOperationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

#if EXTENSION_SET_EXTERNAL_ASM
    /// <summary>
    /// The exception that is thrown when an external assembly is invalid or could not be found.
    /// </summary>
    public class InvalidAssemblyException : RuntimeException
    {
        public InvalidAssemblyException() { }
        public InvalidAssemblyException([Localizable(true)] string message) : base(message) { }
        public InvalidAssemblyException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public InvalidAssemblyException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public InvalidAssemblyException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an external function is invalid or could not be found.
    /// </summary>
    public class InvalidFunctionException : RuntimeException
    {
        public InvalidFunctionException() { }
        public InvalidFunctionException([Localizable(true)] string message) : base(message) { }
        public InvalidFunctionException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public InvalidFunctionException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public InvalidFunctionException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction is used without an external assembly or function loaded when one is required.
    /// Also used for if an exception occurs inside an external method.
    /// </summary>
    public class ExternalOperationException : RuntimeException
    {
        public ExternalOperationException() { }
        public ExternalOperationException([Localizable(true)] string message) : base(message) { }
        public ExternalOperationException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public ExternalOperationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public ExternalOperationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
#endif

#if EXTENSION_SET_HEAP_ALLOCATE
    /// <summary>
    /// The exception that is thrown when there is not enough free memory remaining to perform the requested allocation.
    /// </summary>
    public class MemoryAllocationException : RuntimeException
    {
        public MemoryAllocationException() { }
        public MemoryAllocationException([Localizable(true)] string message) : base(message) { }
        public MemoryAllocationException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public MemoryAllocationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public MemoryAllocationException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when the stack grows to collide with already allocated heap memory.
    /// </summary>
    public class StackSizeException : RuntimeException
    {
        public StackSizeException() { }
        public StackSizeException([Localizable(true)] string message) : base(message) { }
        public StackSizeException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public StackSizeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public StackSizeException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a given address does not correspond to the start of a block of memory.
    /// </summary>
    public class InvalidMemoryBlockException : RuntimeException
    {
        public InvalidMemoryBlockException() { }
        public InvalidMemoryBlockException([Localizable(true)] string message) : base(message) { }
        public InvalidMemoryBlockException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public InvalidMemoryBlockException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public InvalidMemoryBlockException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
#endif
#endif

    // AAP FORMAT EXCEPTIONS
    /// <summary>
    /// Represents errors that occur during loading or saving an AAP file.
    /// </summary>
    public class AAPFormatException : AssEmblyException
    {
        public AAPFormatException() { }
        public AAPFormatException([Localizable(true)] string message) : base(message) { }
        public AAPFormatException([Localizable(true)] string message, Exception inner) : base(message, inner) { }
        public AAPFormatException([Localizable(true)] string message, [Localizable(true)] string consoleMessage) : base(message, consoleMessage) { }
        public AAPFormatException([Localizable(true)] string message, [Localizable(true)] string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
}
