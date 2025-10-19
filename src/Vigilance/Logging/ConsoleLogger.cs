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

            Console.Write("\e[1m ");
            Console.Write(level.ToUpperString());
            Console.Write(" \e[0m");
            Console.ResetColor();
            Console.Write(" ");
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }
}
