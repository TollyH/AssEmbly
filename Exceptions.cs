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
        private string? _consoleMessage = null;
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string ConsoleMessage {
            get => _consoleMessage ?? Message;
            set => _consoleMessage = value;
        }

        public AssEmblyException() : base() { }
        public AssEmblyException(string message) : base(message) { }
        public AssEmblyException(string message, Exception inner) : base(message, inner) { }

        public AssEmblyException(string message, string consoleMessage) : base(message)
        {
            ConsoleMessage = consoleMessage;
        }

        public AssEmblyException(string message, string consoleMessage, Exception inner) : base(message, inner)
        {
            ConsoleMessage = consoleMessage;
        }
    }

    // ASSEMBLER EXCEPTIONS

    /// <summary>
    /// Represents errors that occur during the assembly of an AssEmbly source file.
    /// </summary>
    public class AssemblerException : AssEmblyException
    {
        public Warning WarningObject { get; set; }

        public AssemblerException(string message) : base(message) { }
        public AssemblerException(string message, int line, string file) : base(message)
        {
            WarningObject = new Warning(
                WarningSeverity.FatalError, 0000, file, line, "", Array.Empty<string>(), "", message);
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

    // DEBUGGER EXCEPTIONS

    /// <summary>
    /// Represents exceptions specific to the AssEmbly debugger.
    /// </summary>
    public class DebuggerException : AssEmblyException
    {
        public DebuggerException() : base() { }
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
        public DebugFileException() : base() { }
        public DebugFileException(string message) : base(message) { }
        public DebugFileException(string message, Exception inner) : base(message, inner) { }
        public DebugFileException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public DebugFileException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }

    // RUNTIME EXCEPTIONS

    /// <summary>
    /// Represents errors that occur during the execution of an AssEmbly program.
    /// </summary>
    public class RuntimeException : AssEmblyException
    {
        public RuntimeException() : base() { }
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
        public InvalidOpcodeException() : base() { }
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
        public ReadOnlyRegisterException() : base() { }
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
        public FileOperationException() : base() { }
        public FileOperationException(string message) : base(message) { }
        public FileOperationException(string message, Exception inner) : base(message, inner) { }
        public FileOperationException(string message, string consoleMessage) : base(message, consoleMessage) { }
        public FileOperationException(string message, string consoleMessage, Exception inner) : base(message, consoleMessage, inner) { }
    }
}
