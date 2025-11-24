using Vigilance.Drawing;

namespace Vigilance.Logging;

public sealed class ConsoleLogger : ILogger
{
    public void Log(LogLevel level, string message)
    {
        if (level is > LogLevel.All and < LogLevel.None)
        {
            var background = level.Background;
            var foreground = level.Foreground;
            Console.Write(
                $"{Ansi.Reset}{(foreground == Color.Black ? Ansi.Style.Bold : "")}{(background.HasValue
                    ? Ansi.Background.Rgb(background.Value)
                    : "")}{(foreground.HasValue
                    ? Ansi.Foreground.Rgb(foreground.Value)
                    : "")} {level.ToUpperString()} {Ansi.Reset} "
            );
        }

        Console.WriteLine(message);
        Console.Out.Flush();
    }
}
