using Raylib_cs.BleedingEdge;

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
            LogLevel.Debug => ConsoleColor.DarkBlue,
            LogLevel.Info => ConsoleColor.DarkGreen,
            LogLevel.Warning => ConsoleColor.DarkYellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => null,
        };
    }
}
