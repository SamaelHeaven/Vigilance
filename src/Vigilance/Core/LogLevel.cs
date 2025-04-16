using Raylib_cs;

namespace Vigilance.Core;

public enum LogLevel
{
    All = TraceLogLevel.All,
    Debug = TraceLogLevel.Debug,
    Info = TraceLogLevel.Info,
    Warn = TraceLogLevel.Warning,
    Error = TraceLogLevel.Error,
    Fatal = TraceLogLevel.Fatal,
    None = TraceLogLevel.None,
}
