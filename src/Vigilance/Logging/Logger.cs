using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Raylib_cs.BleedingEdge.Interop;
using Vigilance.Core;

namespace Vigilance.Logging;

public static unsafe class Logger
{
    private static readonly Lock LogLock = new();
    private static LoggingConfig _config = new();

    public static LogLevel LogLevel
    {
        get => _config.LogLevel;
        set
        {
            _config.LogLevel = value;
            Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        }
    }

    internal static void Initialize()
    {
        _config = Game.Configs.TryTake(out LoggingConfig config) ? config : new LoggingConfig();
        LogLevel = _config.LogLevel;
        var engine = Assemblies.Engine.GetName();
        var message = $"Initializing {engine.Name} {engine.Version}";
        try
        {
            if (Platform.Web.IsCurrent())
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

    public static void Debug(object? value)
    {
        Log(LogLevel.Debug, value);
    }

    public static void Info(object? value)
    {
        Log(LogLevel.Info, value);
    }

    public static void Warning(object? value)
    {
        Log(LogLevel.Warning, value);
    }

    public static void Error(object? value)
    {
        Log(LogLevel.Error, value);
    }

    public static void Fatal(object? value)
    {
        Log(LogLevel.Fatal, value);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedLog(TraceLogLevel logLevel, sbyte* format, nint args)
    {
        var message = NativeStringFormatter.Format((nint)format, args);
        Log((LogLevel)logLevel, message);
    }
}
