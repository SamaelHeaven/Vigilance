namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        Console.ResetColor();
        Console.Write(" ");
        if (level is > LogLevel.All and < LogLevel.None)
        {
            var color = level.GetConsoleColor();
            if (color is not null)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = color.Value;
            }

            Console.Write(" ");
            Console.Write(level.ToString().ToUpper());
            Console.Write(" ");
            Console.ResetColor();
            Console.Write(" ");
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }
}
