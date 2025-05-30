using System.Diagnostics;
using Raylib_cs;

namespace Vigilance.Core;

public sealed class Time
{
    public const float FixedDeltaSeconds = 1 / 60f;
    private static Time? _time;
    private readonly TimeSpan _launch;
    private readonly Stopwatch _stopwatch;
    private TimeSpan _delta;
    private TimeSpan _last;
    private float _scale;

    private Time()
    {
        Game.EnsureRunning();
        _stopwatch = Stopwatch.StartNew();
        _launch = GetTicks(_stopwatch);
        _delta = TimeSpan.Zero;
        _last = TimeSpan.Zero;
        _scale = 1;
    }

    public static TimeSpan FixedDelta { get; } = TimeSpan.FromSeconds(FixedDeltaSeconds);

    public static float DeltaSeconds
    {
        get
        {
            var time = GetTime();
            return (float)time._delta.TotalSeconds * time._scale;
        }
    }

    public static TimeSpan Delta
    {
        get
        {
            var time = GetTime();
            return time._delta * time._scale;
        }
    }

    public static float Scale
    {
        get => GetTime()._scale;
        set => GetTime()._scale = MathF.Max(0, value);
    }

    public static float UnscaledDeltaSeconds => (float)GetTime()._delta.TotalSeconds;

    public static TimeSpan UnscaledDelta => GetTime()._delta;

    public static float CurrentFps
    {
        get
        {
            var time = GetTime();
            var delta = (float)time._delta.TotalSeconds;
            return delta <= 0 ? 0 : 1 / delta;
        }
    }

    public static TimeSpan Elapsed
    {
        get
        {
            var time = GetTime();
            return GetTicks(time._stopwatch) - time._launch;
        }
    }

    private static TimeSpan GetTicks(Stopwatch stopwatch)
    {
        var elapsedTicks = stopwatch.ElapsedTicks;
        var frequency = Stopwatch.Frequency;
        var seconds = (double)elapsedTicks / frequency;
        return TimeSpan.FromSeconds(seconds);
    }

    internal static void Update()
    {
        var time = GetTime();
        var elapsed = Elapsed;
        time._delta = elapsed - time._last;
        time._last = elapsed;
        var fpsTarget = Game.FpsTarget;
        var target = fpsTarget < 1 ? 0 : 1.0 / fpsTarget;
        var wait = target - (elapsed - time._last).TotalSeconds;
        if (wait > 0 && wait <= target)
            Raylib.WaitTime(wait);
    }

    internal static void Restart()
    {
        var time = GetTime();
        time._delta = TimeSpan.Zero;
        time._last = TimeSpan.Zero;
    }

    private static Time GetTime()
    {
        return _time ??= new Time();
    }
}
