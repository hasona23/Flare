namespace Flare;

public static class Logger
{
    public static void LogInfo(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] :: {msg}");
        Console.ResetColor();
    }

    public static void LogError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] :: {msg}");
        Console.ResetColor();
    }

    public static void LogWarning(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] :: {msg}");
        Console.ResetColor();
    }
}