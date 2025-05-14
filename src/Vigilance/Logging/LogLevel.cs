using Raylib_cs;

namespace Vigilance.Logging;

public enum LogLevel
{
    All = TraceLogLevel.All,
    Debug = TraceLogLevel.Debug,
    Info = TraceLogLevel.Info,
    Warn = TraceLogLevel.Warning,
    Error = TraceLogLevel.Error,
    Fatal = TraceLogLevel.Fatal,
    None = TraceLogLevel.None
}

public static class LogLevelExtensions
{
    public static ConsoleColor GetConsoleColor(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => ConsoleColor.Gray
        };
    }
}
