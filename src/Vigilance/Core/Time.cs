using System.Diagnostics;
using Raylib_cs;

namespace Vigilance.Core;

public sealed class Time
{
    public const float FixedDelta = 1 / 60f;
    private static Time? _time;
    private readonly TimeSpan _launchTime;
    private readonly Stopwatch _stopwatch;
    private float _averageFps;
    private float _delta;
    private ulong _frameCount;
    private TimeSpan _startTime;

    private Time()
    {
        Game.EnsureRunning();
        _stopwatch = Stopwatch.StartNew();
        _launchTime = GetTicks(_stopwatch);
        _startTime = _launchTime;
    }

    public static float Delta => GetTime()._delta;

    public static float AverageFps
    {
        get
        {
            var time = GetTime();
            return time._frameCount == 0 ? 0 : time._averageFps / time._frameCount;
        }
    }

    public static float CurrentFps
    {
        get
        {
            var time = GetTime();
            var delta = time._delta;
            return delta <= 0 ? 0 : 1 / delta;
        }
    }

    public static TimeSpan SinceStart => Ticks - GetTime()._startTime;
    public static TimeSpan SinceLaunch => Ticks - GetTime()._launchTime;
    private static TimeSpan Ticks => GetTicks(GetTime()._stopwatch);

    private static TimeSpan GetTicks(Stopwatch stopwatch)
    {
        var elapsedTicks = stopwatch.ElapsedTicks;
        var frequency = Stopwatch.Frequency;
        var nanoseconds = (double)elapsedTicks / frequency * 1_000_000_000;
        return TimeSpan.FromTicks((long)nanoseconds);
    }

    internal static void Update()
    {
        var time = GetTime();
        time._frameCount++;
        time._delta = Raylib.GetFrameTime();
        time._averageFps += time._delta <= 0 ? 0 : 1 / time._delta;
    }

    internal static void Restart()
    {
        var time = GetTime();
        time._startTime = Ticks;
        time._delta = 0;
        time._frameCount = 0;
        time._averageFps = 0;
    }

    private static Time GetTime()
    {
        return _time ??= new Time();
    }
}
