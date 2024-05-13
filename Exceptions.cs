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

        public AssemblerException(string message) : base(message) { }
        public AssemblerException(string message, int line, string file) : base(message)
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
        public SyntaxError(string message) : base(message) { }
        public SyntaxError(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the operands given for an instruction during assembly are invalid.
    /// </summary>
    public class OperandException : AssemblerException
    {
        public OperandException(string message) : base(message) { }
        public OperandException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst importing an external AssEmbly source file.
    /// </summary>
    public class ImportException : AssemblerException
    {
        public ImportException(string message) : base(message) { }
        public ImportException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst attempting to retrieve the opcode for an instruction.
    /// </summary>
    public class OpcodeException : AssemblerException
    {
        public OpcodeException(string message) : base(message) { }
        public OpcodeException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid label name during assembly.
    /// </summary>
    public class LabelNameException : AssemblerException
    {
        public LabelNameException(string message) : base(message) { }
        public LabelNameException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid macro name during assembly.
    /// </summary>
    public class MacroNameException : AssemblerException
    {
        public MacroNameException(string message) : base(message) { }
        public MacroNameException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered with an otherwise valid assembler variable/constant name during assembly.
    /// </summary>
    public class VariableNameException : AssemblerException
    {
        public VariableNameException(string message) : base(message) { }
        public VariableNameException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the end of the file is reached before a required closing directive is encountered.
    /// </summary>
    public class EndingDirectiveException : AssemblerException
    {
        public EndingDirectiveException(string message) : base(message) { }
        public EndingDirectiveException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when an error is encountered whilst attempting to expand the usage of a macro.
    /// </summary>
    public class MacroExpansionException : AssemblerException
    {
        public MacroExpansionException(string message) : base(message) { }
        public MacroExpansionException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the program requests that assembly stop.
    /// </summary>
    public class AssemblyStoppedException : AssemblerException
    {
        public AssemblyStoppedException(string message) : base(message) { }
        public AssemblyStoppedException(string message, int line, string file) : base(message, line, file) { }
    }

    /// <summary>
    /// The exception that is thrown when the program exceeds the limit of allowed while repeats.
    /// </summary>
    public class WhileLimitExceededException : AssemblerException
    {
        public WhileLimitExceededException(string message) : base(message) { }
        public WhileLimitExceededException(string message, int line, string file) : base(message, line, file) { }
    }
#endif

#if DEBUGGER
    /// <summary>
    /// Represents exceptions specific to the AssEmbly debugger.
    /// </summary>
    public class DebuggerException : AssEmblyException
    {
        public DebuggerException() { }
        public DebuggerException(string message) : base(message) { }
        public DebuggerException(string message, Exception inner) : base(message, inner) { }
        public DebuggerException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public DebuggerException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a given debug file is in an invalid format.
    /// </summary>
    public class DebugFileException : DebuggerException
    {
        public DebugFileException() { }
        public DebugFileException(string message) : base(message) { }
        public DebugFileException(string message, Exception inner) : base(message, inner) { }
        public DebugFileException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public DebugFileException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
#endif

#if PROCESSOR
    /// <summary>
    /// Represents errors that occur during the execution of an AssEmbly program.
    /// </summary>
    public class RuntimeException : AssEmblyException
    {
        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { }
        public RuntimeException(string message, Exception inner) : base(message, inner) { }
        public RuntimeException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public RuntimeException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an opcode is not recognised by the AssEmbly processor.
    /// </summary>
    public class InvalidOpcodeException : RuntimeException
    {
        public InvalidOpcodeException() { }
        public InvalidOpcodeException(string message) : base(message) { }
        public InvalidOpcodeException(string message, Exception inner) : base(message, inner) { }
        public InvalidOpcodeException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public InvalidOpcodeException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction attempts to write to a read-only register.
    /// </summary>
    public class ReadOnlyRegisterException : RuntimeException
    {
        public ReadOnlyRegisterException() { }
        public ReadOnlyRegisterException(string message) : base(message) { }
        public ReadOnlyRegisterException(string message, Exception inner) : base(message, inner) { }
        public ReadOnlyRegisterException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public ReadOnlyRegisterException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction attempts to perform a file related operation that is
    /// invalid given the current state of the program's execution.
    /// </summary>
    public class FileOperationException : RuntimeException
    {
        public FileOperationException() { }
        public FileOperationException(string message) : base(message) { }
        public FileOperationException(string message, Exception inner) : base(message, inner) { }
        public FileOperationException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public FileOperationException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

#if EXTENSION_SET_EXTERNAL_ASM
    /// <summary>
    /// The exception that is thrown when an external assembly is invalid or could not be found.
    /// </summary>
    public class InvalidAssemblyException : RuntimeException
    {
        public InvalidAssemblyException() { }
        public InvalidAssemblyException(string message) : base(message) { }
        public InvalidAssemblyException(string message, Exception inner) : base(message, inner) { }
        public InvalidAssemblyException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public InvalidAssemblyException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an external function is invalid or could not be found.
    /// </summary>
    public class InvalidFunctionException : RuntimeException
    {
        public InvalidFunctionException() { }
        public InvalidFunctionException(string message) : base(message) { }
        public InvalidFunctionException(string message, Exception inner) : base(message, inner) { }
        public InvalidFunctionException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public InvalidFunctionException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when an instruction is used without an external assembly or function loaded when one is required.
    /// Also used for if an exception occurs inside an external method.
    /// </summary>
    public class ExternalOperationException : RuntimeException
    {
        public ExternalOperationException() { }
        public ExternalOperationException(string message) : base(message) { }
        public ExternalOperationException(string message, Exception inner) : base(message, inner) { }
        public ExternalOperationException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public ExternalOperationException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
#endif

#if EXTENSION_SET_HEAP_ALLOCATE
    /// <summary>
    /// The exception that is thrown when there is not enough free memory remaining to perform the requested allocation.
    /// </summary>
    public class MemoryAllocationException : RuntimeException
    {
        public MemoryAllocationException() { }
        public MemoryAllocationException(string message) : base(message) { }
        public MemoryAllocationException(string message, Exception inner) : base(message, inner) { }
        public MemoryAllocationException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public MemoryAllocationException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when the stack grows to collide with already allocated heap memory.
    /// </summary>
    public class StackSizeException : RuntimeException
    {
        public StackSizeException() { }
        public StackSizeException(string message) : base(message) { }
        public StackSizeException(string message, Exception inner) : base(message, inner) { }
        public StackSizeException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public StackSizeException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when a given address does not correspond to the start of a block of memory.
    /// </summary>
    public class InvalidMemoryBlockException : RuntimeException
    {
        public InvalidMemoryBlockException() { }
        public InvalidMemoryBlockException(string message) : base(message) { }
        public InvalidMemoryBlockException(string message, Exception inner) : base(message, inner) { }
        public InvalidMemoryBlockException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public InvalidMemoryBlockException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
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
        public AAPFormatException(string message) : base(message) { }
        public AAPFormatException(string message, Exception inner) : base(message, inner) { }
        public AAPFormatException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public AAPFormatException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
}
