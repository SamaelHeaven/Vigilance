namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(string message, LogLevel level)
    {
        Console.Write("[");
        var color = Console.ForegroundColor;
        Console.ForegroundColor = level.GetConsoleColor();
        Console.Write(level);
        Console.ForegroundColor = color;
        Console.WriteLine("] " + message);
    }
}
