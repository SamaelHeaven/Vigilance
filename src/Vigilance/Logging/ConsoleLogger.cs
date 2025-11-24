namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        Console.ResetColor();
        if (level is > LogLevel.All and < LogLevel.None)
        {
            var color = level.ConsoleColor;
            if (color.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = color.Value;
            }

            Console.Write($"{Ansi.Style.Bold} ");
            Console.Write(level.ToUpperString());
            Console.Write($" {Ansi.Reset}");
            Console.ResetColor();
        }

        Console.Write(" ");
        Console.WriteLine(message);
        Console.Out.Flush();
    }
}
