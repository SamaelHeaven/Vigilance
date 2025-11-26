namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        if (!Console.IsOutputRedirected)
            Console.Write(Ansi.Reset);
        Console.ResetColor();
        if (level is > LogLevel.All and < LogLevel.None)
        {
            var color = level.GetConsoleColor();
            if (color.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = color.Value;
            }

            if (!Console.IsOutputRedirected)
                Console.Write($"{Ansi.Style.Bold} ");
            Console.Write(level.ToUpperString());
            Console.Write(Console.IsOutputRedirected ? ":" : $" {Ansi.Reset}");
            Console.ResetColor();
            Console.Write(" ");
        }

        Console.WriteLine(message);
    }
}
