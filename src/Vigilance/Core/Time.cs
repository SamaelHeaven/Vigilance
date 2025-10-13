using Raylib_cs.BleedingEdge;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public static class Time
{
    public const float FixedDeltaSeconds = 1 / 60f;
    private const int FpsHistorySize = 200;
    private static readonly Queue<float> _fpsHistory;
    private static TimeSpan _delta;
    private static TimeSpan _last;
    private static float _scale;

    static Time()
    {
        Game.EnsureRunning();
        _fpsHistory = new Queue<float>(FpsHistorySize);
        _delta = TimeSpan.Zero;
        _last = TimeSpan.FromSeconds(Raylib.GetTime());
        _scale = 1;
    }

    public static TimeSpan FixedDelta { get; } = TimeSpan.FromSeconds(FixedDeltaSeconds);

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

    public static float AverageFps => _fpsHistory.Count == 0 ? 0 : _fpsHistory.AsValueEnumerable().Average();

    public static TimeSpan Elapsed => TimeSpan.FromSeconds(Raylib.GetTime());

    public static void Sleep(TimeSpan duration)
    {
        Sleep(duration.TotalSeconds);
    }

    public static void Sleep(double seconds)
    {
        Raylib.WaitTime(seconds);
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
        while (_fpsHistory.Count >= FpsHistorySize)
            _fpsHistory.Dequeue();
        _fpsHistory.Enqueue(CurrentFps);
    }

    internal static void Restart()
    {
        _delta = TimeSpan.Zero;
        _last = Elapsed;
        _fpsHistory.Clear();
    }
}
