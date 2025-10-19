using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Raylib_cs.BleedingEdge.Interop;
using Vigilance.Core;

namespace Vigilance.Logging;

public static unsafe partial class Logger
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private static readonly Lock _logLock = new();
    private static LoggingConfig _config = new();

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
        _config = Game.Config.TryTake(out LoggingConfig config) ? config : new LoggingConfig();
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

    public static void Log<T>(T value)
    {
        Log(value is Exception ? LogLevel.Error : LogLevel.Info, value);
    }

    public static void Log(InfoLogHandler handler)
    {
        Log(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Log<T>(LogLevel level, T value)
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

    public static void Log(LogLevel level, [InterpolatedStringHandlerArgument(nameof(level))] LogHandler handler)
    {
        Log(level, handler.GetFormattedText());
    }

    public static void Debug<T>(T value)
    {
        Log(LogLevel.Debug, value);
    }

    public static void Debug(DebugLogHandler handler)
    {
        Log(LogLevel.Debug, handler.GetFormattedText());
    }

    public static void Info<T>(T value)
    {
        Log(LogLevel.Info, value);
    }

    public static void Info(InfoLogHandler handler)
    {
        Log(LogLevel.Info, handler.GetFormattedText());
    }

    public static void Warning<T>(T value)
    {
        Log(LogLevel.Warning, value);
    }

    public static void Warning(WarningLogHandler handler)
    {
        Log(LogLevel.Warning, handler.GetFormattedText());
    }

    public static void Error<T>(T value)
    {
        Log(LogLevel.Error, value);
    }

    public static void Error(ErrorLogHandler handler)
    {
        Log(LogLevel.Error, handler.GetFormattedText());
    }

    public static void Fatal<T>(T value)
    {
        Log(LogLevel.Fatal, value);
    }

    public static void Fatal(FatalLogHandler handler)
    {
        Log(LogLevel.Fatal, handler.GetFormattedText());
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedLog(TraceLogLevel level, sbyte* format, nint args)
    {
        var message = NativeStringFormatter.Format((nint)format, args);
        Log((LogLevel)level, message);
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
