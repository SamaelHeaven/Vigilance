using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace Vigilance.Logging;

public static unsafe partial class Log
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private static readonly Lock _logLock = new();
    private static LoggingConfig _config = new();
    private static ILogger? _logger;

    public static LogLevel LogLevel
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            Raylib.SetTraceLogLevel((TraceLogLevel)field);
        }
    } = _config.LogLevel;

    public static LogLevel SetLogLevel(LogLevel level)
    {
        var previous = LogLevel;
        LogLevel = level;
        return previous;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<LoggingConfig>() ?? _config;
        LogLevel = _config.LogLevel;
        _logger = _config.Logger;
        Raylib.SetTraceLogLevel((TraceLogLevel)LogLevel);
        var engine = Assemblies.Engine.GetName();
        var message = $"Initializing {engine.Name} {engine.Version}";
        try
        {
            EnableAnsiSupport();
            if (Platform.Web.IsCurrent)
                goto ERROR;
            Raylib.SetTraceLogCallback(&UnmanagedLog);
            Raylib.TraceLog(TraceLogLevel.Info, message);
        }
        catch
        {
            goto ERROR;
        }

        return;

        ERROR:
        _logger = null;
        Raylib.SetTraceLogCallback(null);
        Warning("Failed to initialize custom logging");
        Info(message);
    }

    public static void Invoke<T>(in T value)
    {
        Invoke(value is Exception ? LogLevel.Error : LogLevel.Info, value);
    }

    public static void Invoke(InfoLogHandler handler)
    {
        Invoke(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Invoke<T>(LogLevel level, in T value)
    {
        if (LogLevel > level)
            return;
        lock (_logLock)
        {
            var message = value is Exception e ? e.DetailedString : value?.ToString() ?? "";
            if (_logger is null)
            {
                if (level is > LogLevel.All and < LogLevel.None)
                {
                    Console.Write(level.ToUpperString());
                    Console.Write(": ");
                }

                Console.WriteLine(message);
            }
            else
            {
                try
                {
                    _logger.Log(level, message);
                }
                catch (Exception ex)
                {
                    Console.Write("ERROR: ");
                    Console.WriteLine(ex.DetailedString);
                }
            }

            if (level == LogLevel.Fatal)
                Environment.Exit(1);
        }
    }

    public static void Invoke(LogLevel level, [InterpolatedStringHandlerArgument(nameof(level))] LogHandler handler)
    {
        Invoke(level, handler.GetFormattedText());
    }

    public static void Trace<T>(in T value)
    {
        Invoke(LogLevel.Trace, value);
    }

    public static void Trace(TraceLogHandler handler)
    {
        Invoke(LogLevel.Trace, handler.GetFormattedText());
    }

    public static void Debug<T>(in T value)
    {
        Invoke(LogLevel.Debug, value);
    }

    public static void Debug(DebugLogHandler handler)
    {
        Invoke(LogLevel.Debug, handler.GetFormattedText());
    }

    public static void Info<T>(in T value)
    {
        Invoke(LogLevel.Info, value);
    }

    public static void Info(InfoLogHandler handler)
    {
        Invoke(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Warning<T>(in T value)
    {
        Invoke(LogLevel.Warning, value);
    }

    public static void Warning(WarningLogHandler handler)
    {
        Invoke(LogLevel.Warning, handler.GetFormattedText());
    }

    public static void Error<T>(in T value)
    {
        Invoke(LogLevel.Error, value);
    }

    public static void Error(ErrorLogHandler handler)
    {
        Invoke(LogLevel.Error, handler.GetFormattedText());
    }

    public static void Fatal<T>(in T value)
    {
        Invoke(LogLevel.Fatal, value);
    }

    public static void Fatal(FatalLogHandler handler)
    {
        Invoke(LogLevel.Fatal, handler.GetFormattedText());
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedLog(int level, sbyte* format, sbyte* args)
    {
        var message = Raylib_cs.Logging.GetLogMessage((nint)format, (nint)args);
        Invoke((LogLevel)level, message);
    }

    private static void EnableAnsiSupport()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var handle = GetStdHandle(StdOutputHandle);
        if (GetConsoleMode(handle, out var mode))
            SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial void SetConsoleMode(nint hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
}
