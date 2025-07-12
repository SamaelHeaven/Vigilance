namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        lock (Console.Out)
        {
            Console.ResetColor();
            if (level is > LogLevel.All and < LogLevel.None)
            {
                var color = level.GetConsoleColor();
                Console.Write("[");
                if (color is not null)
                    Console.ForegroundColor = color.Value;
                Console.Write(level);
                Console.ResetColor();
                Console.Write("] ");
            }

            Console.WriteLine(message);
            Console.Out.Flush();
        }
    }
}
