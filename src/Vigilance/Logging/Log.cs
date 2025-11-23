using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Raylib_cs.BleedingEdge.Interop;
using Vigilance.Core;

namespace Vigilance.Logging;

public static unsafe partial class Log
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private static readonly Lock _logLock = new();
    private static LoggingConfig _config = new();

    static Log()
    {
        _config.Logger = null;
    }

    public static LogLevel LogLevel
    {
        get => _config.LogLevel;
        set
        {
            _config.LogLevel = value;
            if (Game.Running && value != LogLevel)
                Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        }
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<LoggingConfig>() ?? new LoggingConfig();
        Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        var engine = Assemblies.Engine.GetName();
        var message = $"Initializing {engine.Name} {engine.Version}";
        try
        {
            EnableAnsiSupport();
            if (Platform.Web.IsCurrent)
                throw new PlatformNotSupportedException();
            Raylib.SetTraceLogCallback(&UnmanagedLog);
            Raylib.TraceLog(TraceLogLevel.Info, message);
        }
        catch
        {
            _config.Logger = null;
            Raylib.SetTraceLogCallback(null);
            Warning("Failed to initialize custom logging");
            Info(message);
        }
    }

    public static void Invoke<T>(T value)
    {
        Invoke(value is Exception ? LogLevel.Error : LogLevel.Info, value);
    }

    public static void Invoke(InfoLogHandler handler)
    {
        Invoke(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Invoke<T>(LogLevel level, T value)
    {
        if (LogLevel > level)
            return;
        lock (_logLock)
        {
            var message = value is Exception e
                ? $"{e.GetType()}: {e.Message}{(e.StackTrace is null ? "" : $"\n{e.StackTrace}")}"
                : value?.ToString() ?? "";
            if (_config.Logger is null)
            {
                if (level is > LogLevel.All and < LogLevel.None)
                {
                    Console.Write(level.ToUpperString());
                    Console.Write(": ");
                }

                Console.WriteLine(message);
                Console.Out.Flush();
            }
            else
            {
                _config.Logger.Log(level, message);
            }

            if (level == LogLevel.Fatal)
                Environment.Exit(1);
        }
    }

    public static void Invoke(LogLevel level, [InterpolatedStringHandlerArgument(nameof(level))] LogHandler handler)
    {
        Invoke(level, handler.GetFormattedText());
    }

    public static void Debug<T>(T value)
    {
        Invoke(LogLevel.Debug, value);
    }

    public static void Debug(DebugLogHandler handler)
    {
        Invoke(LogLevel.Debug, handler.GetFormattedText());
    }

    public static void Info<T>(T value)
    {
        Invoke(LogLevel.Info, value);
    }

    public static void Info(InfoLogHandler handler)
    {
        Invoke(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Warning<T>(T value)
    {
        Invoke(LogLevel.Warning, value);
    }

    public static void Warning(WarningLogHandler handler)
    {
        Invoke(LogLevel.Warning, handler.GetFormattedText());
    }

    public static void Error<T>(T value)
    {
        Invoke(LogLevel.Error, value);
    }

    public static void Error(ErrorLogHandler handler)
    {
        Invoke(LogLevel.Error, handler.GetFormattedText());
    }

    public static void Fatal<T>(T value)
    {
        Invoke(LogLevel.Fatal, value);
    }

    public static void Fatal(FatalLogHandler handler)
    {
        Invoke(LogLevel.Fatal, handler.GetFormattedText());
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedLog(TraceLogLevel level, sbyte* format, nint args)
    {
        var message = NativeStringFormatter.Format((nint)format, args);
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
