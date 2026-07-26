using Vigilance.Math;

namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteCycleCount = -1;

    public Timer()
    {
        CycleCount = InfiniteCycleCount;
    }

    public Timer(TimeSpan duration, TimeSpan elapsed = default, int cycleCount = InfiniteCycleCount)
    {
        Elapsed = elapsed;
        Duration = duration;
        CycleCount = cycleCount;
    }

    public TimeSpan Elapsed { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPaused { get; set; }
    public int CycleCount { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }
    public bool DidTick { get; private set; }
    public int CurrentCycle { get; private set; }

    public bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;

    public float Progress =>
        Duration == TimeSpan.Zero ? 1f : ((float)(Elapsed.TotalSeconds / Duration.TotalSeconds)).Clamp(0f, 1f);

    public bool Update(in TimeSpan? step = null)
    {
        DidTick = false;
        if (IsPaused || IsCompleted)
            return false;
        Elapsed += step ?? Time.Delta;
        if (Elapsed < Duration)
            return false;
        DidTick = true;
        CurrentCycle++;
        if (IsCompleted)
        {
            OnComplete?.SafeInvoke();
            return true;
        }

        Elapsed -= Duration;
        OnRepeat?.SafeInvoke();
        return true;
    }

    public void Reset()
    {
        Elapsed = TimeSpan.Zero;
        CurrentCycle = 0;
        DidTick = false;
    }
}
