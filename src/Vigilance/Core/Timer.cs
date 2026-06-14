using Vigilance.Math;

namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteCycleCount = -1;

    public Timer(
        TimeSpan duration,
        TimeSpan? initialTime = null,
        int cycleCount = InfiniteCycleCount,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        TimeLeft = initialTime ?? duration;
        Duration = duration;
        CycleCount = cycleCount;
    }

    public TimeSpan TimeLeft { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPaused { get; set; }
    public int CycleCount { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }
    public bool DidTick { get; private set; }
    public int CurrentCycle { get; private set; }

    public bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;

    public float Progress =>
        Duration == TimeSpan.Zero ? 1f : (1f - (float)(TimeLeft.TotalSeconds / Duration.TotalSeconds)).Clamp(0f, 1f);

    public bool Update(TimeSpan? step = null)
    {
        DidTick = false;
        if (IsPaused || IsCompleted)
            return false;
        TimeLeft -= step ?? Time.Delta;
        if (TimeLeft > TimeSpan.Zero)
            return false;
        DidTick = true;
        CurrentCycle++;
        if (IsCompleted)
        {
            OnComplete?.Invoke();
            return true;
        }

        TimeLeft += Duration;
        OnRepeat?.Invoke();
        return true;
    }

    public void Reset()
    {
        TimeLeft = Duration;
        CurrentCycle = 0;
        DidTick = false;
    }
}
