using System;

public static class AssEmblyInterop
{
    // Method is private, so cannot be accessed from AssEmbly
    private static string GetStringFromMemory(byte[] memory, ulong startAddress)
    {
        ulong length = 0;
        while (memory[startAddress + length] != 0)
        {
            length++;
        }
        return System.Text.Encoding.UTF8.GetString(memory, (int)startAddress, (int)length);
    }
    
    public static void PrintFormattedDateTime(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        if (passedValue is null)
        {
            throw new ArgumentException("This method requires the address of a null-terminated string to use as the DateTime format");
        }
        string format = GetStringFromMemory(memory, passedValue.Value);
        Console.Write(DateTime.Now.ToString(format, System.Globalization.CultureInfo.InvariantCulture));
    }
    
    public static void Sleep(byte[] memory, ulong[] registers, ulong? passedValue)
    {
        if (passedValue is null)
        {
            throw new ArgumentException("This method requires a number of milliseconds to sleep");
        }
        System.Threading.Thread.Sleep((int)passedValue);
    }
}
