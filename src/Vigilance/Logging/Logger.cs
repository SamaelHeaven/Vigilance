using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Raylib_cs.BleedingEdge.Interop;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Logging;

public static unsafe partial class Logger
{
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private static readonly Lock LogLock = new();
    private static LoggingConfig _config = new();

    public static LogLevel LogLevel
    {
        get => _config.LogLevel;
        set
        {
            _config.LogLevel = value;
            if (Game.Running)
                Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        }
    }

    internal static void Initialize()
    {
        _config = Game.Config.TryTake(out LoggingConfig config) ? config : new LoggingConfig();
        LogLevel = _config.LogLevel;
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
            Log(LogLevel.Warning, "Failed to initialize custom logging");
            Log(message);
        }
    }

    public static void Log(object? value)
    {
        Log(value is Exception ? LogLevel.Error : LogLevel.Info, value);
    }

    public static void Log(params object?[] values)
    {
        Log(LogLevel.Info, values);
    }

    public static void Log(LogLevel level, object? value)
    {
        if (_config.LogLevel > level)
            return;
        lock (LogLock)
        {
            var message = value is Exception e
                ? $"{e.GetType()}: {e.Message}{(e.StackTrace is null ? "" : $"\n{e.StackTrace}")}"
                : value?.ToString() ?? "";
            if (_config.Logger is null)
            {
                if (level is > LogLevel.All and < LogLevel.None)
                    Console.Write($"{level.ToString().ToUpper()}: ");
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

    public static void Log(LogLevel level, params object?[] values)
    {
        if (_config.LogLevel > level)
            return;
        Log(level, values.AsValueEnumerable().JoinToString(", "));
    }

    public static void Debug(object? value)
    {
        Log(LogLevel.Debug, value);
    }

    public static void Debug(params object?[] values)
    {
        Log(LogLevel.Debug, values);
    }

    public static void Info(object? value)
    {
        Log(LogLevel.Info, value);
    }

    public static void Info(params object?[] values)
    {
        Log(LogLevel.Info, values);
    }

    public static void Warning(object? value)
    {
        Log(LogLevel.Warning, value);
    }

    public static void Warning(params object?[] values)
    {
        Log(LogLevel.Warning, values);
    }

    public static void Error(object? value)
    {
        Log(LogLevel.Error, value);
    }

    public static void Error(params object?[] values)
    {
        Log(LogLevel.Error, values);
    }

    public static void Fatal(object? value)
    {
        Log(LogLevel.Fatal, value);
    }

    public static void Fatal(params object?[] values)
    {
        Log(LogLevel.Fatal, values);
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
    private static partial void SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
}
