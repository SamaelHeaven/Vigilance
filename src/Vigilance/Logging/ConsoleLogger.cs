namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(string message, LogLevel level)
    {
        if (level is > LogLevel.All and < LogLevel.None)
        {
            Console.Write("[");
            var color = Console.ForegroundColor;
            Console.ForegroundColor = level.GetConsoleColor() ?? color;
            Console.Write(level);
            Console.ForegroundColor = color;
            Console.Write("] ");
        }

        Console.WriteLine(message);
    }
}
