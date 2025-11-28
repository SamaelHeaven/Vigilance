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
    extension(LogLevel level)
    {
        public ConsoleColor? GetConsoleColor()
        {
            return level switch
            {
                LogLevel.Trace => ConsoleColor.White,
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Info => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => null,
            };
        }

        public string ToUpperString()
        {
            return level switch
            {
                LogLevel.All => "ALL",
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Fatal => "FATAL",
                LogLevel.None => "NONE",
                _ => "",
            };
        }
    }
}
