using System.Diagnostics;
using Raylib_cs;

namespace Vigilance.Core;

public static class Time
{
    private const int FpsHistorySize = 200;
    private static ValueQueue<float> _fpsHistory;
    private static TimeSpan _delta;
    private static TimeSpan _last;
    private static float _scale;
    private static readonly Stopwatch _stopwatch;
    private static TimeConfig _config = new();

    static Time()
    {
        Game.ThrowIfNotRunning();
        _stopwatch = Stopwatch.StartNew();
        _fpsHistory = new ValueQueue<float>(FpsHistorySize);
        _delta = TimeSpan.Zero;
        _last = Elapsed;
        _scale = 1;
        FixedDeltaSeconds = _config.FixedDeltaSeconds;
        MaxDeltaSeconds = _config.MaxDeltaSeconds;
        FixedDelta = TimeSpan.FromSeconds(_config.FixedDeltaSeconds);
    }

    public static TimeSpan FixedAccumulator { get; internal set; } = TimeSpan.Zero;

    public static float FixedAccumulatorSeconds => (float)FixedAccumulator.TotalSeconds;

    public static float FixedDeltaSeconds { get; private set; }

    public static TimeSpan FixedDelta { get; private set; }

    public static float MaxDeltaSeconds { get; private set; }

    public static float DeltaSeconds => (float)_delta.TotalSeconds * _scale;

    public static TimeSpan Delta => _delta * _scale;

    public static float Scale
    {
        get => _scale;
        set => _scale = value.Max(0);
    }

    public static float UnscaledDeltaSeconds => (float)_delta.TotalSeconds;

    public static TimeSpan UnscaledDelta => _delta;

    public static float CurrentFps
    {
        get
        {
            var delta = (float)_delta.TotalSeconds;
            return delta <= 0 ? 0 : 1 / delta;
        }
    }

    public static float AverageFps { get; private set; }

    public static TimeSpan Elapsed => _stopwatch.Elapsed;

    public static void Sleep(TimeSpan duration)
    {
        Sleep(duration.TotalSeconds);
    }

    public static void Sleep(double seconds)
    {
        Raylib.WaitTime(seconds);
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<TimeConfig>() ?? _config;
        MaxDeltaSeconds = _config.MaxDeltaSeconds;
        FixedDeltaSeconds = _config.FixedDeltaSeconds;
        FixedDelta = TimeSpan.FromSeconds(_config.FixedDeltaSeconds);
    }

    internal static void Update()
    {
        var fpsTarget =
            Display.FpsTarget < 1 && Display.Vsync && Display.Minimized ? Display.RefreshRate : Display.FpsTarget;
        var target = fpsTarget < 1 ? 0 : 1.0 / fpsTarget;
        var wait = target - (Elapsed - _last).TotalSeconds;
        if (wait > 0 && wait <= target)
            Sleep(wait);
        var elapsed = Elapsed;
        _delta = elapsed - _last;
        _last = elapsed;
        if (_delta.TotalSeconds > MaxDeltaSeconds)
            _delta = TimeSpan.FromSeconds(MaxDeltaSeconds);
        while (_fpsHistory.Count >= FpsHistorySize)
            _fpsHistory.Dequeue();
        _fpsHistory.Enqueue(CurrentFps);
        AverageFps = _fpsHistory.AsValueEnumerable().Average();
    }

    internal static void Restart()
    {
        FixedAccumulator = TimeSpan.Zero;
        _delta = TimeSpan.Zero;
        _last = Elapsed;
        _fpsHistory.Clear();
    }
}

public sealed class TimeConfig
{
    public float MaxDeltaSeconds { get; set; } = 1 / 4f;
    public float FixedDeltaSeconds { get; set; } = 1 / 60f;
}

public static class TimeConfigExtensions
{
    public static ConfigBuilder Time(this ConfigBuilder configs, Action<TimeConfig> config)
    {
        return configs.Add(config);
    }
}
