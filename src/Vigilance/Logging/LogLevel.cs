using Raylib_cs;

namespace Vigilance.Logging;

public enum LogLevel
{
    All = TraceLogLevel.All,
    Trace = TraceLogLevel.Trace,
    Debug = TraceLogLevel.Debug,
    Info = TraceLogLevel.Info,
    Warning = TraceLogLevel.Warning,
    Error = TraceLogLevel.Error,
    Fatal = TraceLogLevel.Fatal,
    None = TraceLogLevel.None,
}

public static class LogLevelExtensions
{
    public static ConsoleColor? GetConsoleColor(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => null,
        };
    }
}
