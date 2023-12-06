using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class AssEmblyInterop
{
    // Load the Win32 API message box function to avoid additional .NET dependencies like WinForms or WPF
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    private static string messageBoxTitle = "Title";
    private static string messageBoxContent = "Content";
    private static uint messageBoxFlags = 0;

    // Method is private, so cannot be accessed from AssEmbly
    private static string GetStringFromMemory(byte[] memory, ulong startAddress)
    {
        ulong length = 0;
        while (memory[startAddress + length] != 0)
        {
            length++;
        }
        return Encoding.UTF8.GetString(memory, (int)startAddress, (int)length);
    }

    public static void SetMessageBoxTitle(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        if (passedValue is null)
        {
            throw new ArgumentException("This method requires the address of a null-terminated string to set the message box title to");
        }
        messageBoxTitle = GetStringFromMemory(memory, passedValue.Value);
    }

    public static void SetMessageBoxContent(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        if (passedValue is null)
        {
            throw new ArgumentException("This method requires the address of a null-terminated string to set the message box content to");
        }
        messageBoxContent = GetStringFromMemory(memory, passedValue.Value);
    }

    public static void SetMessageBoxFlags(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        if (passedValue is null)
        {
            throw new ArgumentException("This method requires a value representing the desired type of Win32 message box");
        }
        if (passedValue.Value > uint.MaxValue)
        {
            throw new ArgumentException("The passed value is too large for a UInt32");
        }
        messageBoxFlags = (uint)passedValue.Value;
    }

    public static void ShowMessageBox(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        // 0x4 = rrv
        registers[0x4] = (uint)MessageBoxW(IntPtr.Zero, messageBoxContent, messageBoxTitle, messageBoxFlags);
    }
}
