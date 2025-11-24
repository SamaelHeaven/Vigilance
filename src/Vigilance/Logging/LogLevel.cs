using Raylib_cs.BleedingEdge;
using Color = Vigilance.Drawing.Color;

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
        public Color? Foreground
        {
            get
            {
                return level switch
                {
                    LogLevel.Debug => Color.White,
                    LogLevel.Info => Color.Black,
                    LogLevel.Warning => Color.Black,
                    LogLevel.Error => Color.Black,
                    LogLevel.Fatal => Color.White,
                    _ => new Color?(),
                };
            }
        }

        public Color? Background
        {
            get
            {
                return level switch
                {
                    LogLevel.Debug => Color.DarkBlue,
                    LogLevel.Info => Color.Lime,
                    LogLevel.Warning => Color.Yellow,
                    LogLevel.Error => Color.Red,
                    LogLevel.Fatal => Color.Maroon,
                    _ => new Color?(),
                };
            }
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
