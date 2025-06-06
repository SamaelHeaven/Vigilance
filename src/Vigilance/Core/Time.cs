using Raylib_cs;

namespace Vigilance.Core;

public sealed class Time
{
    public const float FixedDeltaSeconds = 1 / 60f;
    private const int FpsHistorySize = 200;
    private static Time? _time;
    private readonly Queue<float> _fpsHistory;
    private TimeSpan _delta;
    private TimeSpan _last;
    private float _scale;

    private Time()
    {
        Game.EnsureRunning();
        _delta = TimeSpan.Zero;
        _last = TimeSpan.FromSeconds(Raylib.GetTime());
        _fpsHistory = new Queue<float>(FpsHistorySize);
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

    public static float AverageFps
    {
        get
        {
            var time = GetTime();
            return time._fpsHistory.Count == 0 ? 0 : time._fpsHistory.Average();
        }
    }

    public static TimeSpan Elapsed
    {
        get
        {
            GetTime();
            return TimeSpan.FromSeconds(Raylib.GetTime());
        }
    }

    internal static void Update()
    {
        var time = GetTime();
        var fpsTarget = Game.FpsTarget;
        var target = fpsTarget < 1 ? 0 : 1.0 / fpsTarget;
        var wait = target - (Elapsed - time._last).TotalSeconds;
        if (wait > 0 && wait <= target)
            Raylib.WaitTime(wait);
        var elapsed = Elapsed;
        time._delta = elapsed - time._last;
        time._last = elapsed;
        while (time._fpsHistory.Count >= FpsHistorySize)
            time._fpsHistory.Dequeue();
        time._fpsHistory.Enqueue(CurrentFps);
    }

    internal static void Restart()
    {
        var time = GetTime();
        time._delta = TimeSpan.Zero;
        time._last = Elapsed;
        time._fpsHistory.Clear();
    }

    private static Time GetTime()
    {
        return _time ??= new Time();
    }
}
