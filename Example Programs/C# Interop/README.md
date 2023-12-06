# C# Interop Examples

Examples of the External Assembly Extension Set being used to call C# methods from AssEmbly code.

**The C# files must be compiled before AssEmbly can load them!**

To compile a C# file to a DLL without creating a new project, you can use the following command in a Visual Studio Developer Command Prompt: `csc /t:library <file_name>.cs`. This will generate a .NET Framework assembly with the name `<file_name>.dll`.
