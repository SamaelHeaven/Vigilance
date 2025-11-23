namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        Console.ResetColor();
        Console.Write(" ");
        if (level is > LogLevel.All and < LogLevel.None)
        {
            var color = level.ConsoleColor;
            if (color.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = color.Value;
            }

            Console.Write($"\e[1m {level.ToUpperString()} \e[0m");
            Console.ResetColor();
        }

        foreach (var range in message.AsSpan().Split("\n"))
        {
            var line = message.AsSpan(range);
            Console.Write(" ");
            Console.WriteLine(line);
        }

        Console.Out.Flush();
    }
}
