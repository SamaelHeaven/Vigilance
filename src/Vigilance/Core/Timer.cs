using Vigilance.Math;

namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteRepeatCount = -1;

    public Timer(
        TimeSpan duration,
        TimeSpan? initialTime = null,
        int repeatCount = InfiniteRepeatCount,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        TimeLeft = initialTime ?? duration;
        Duration = duration;
        RepeatCount = repeatCount;
    }

    public TimeSpan TimeLeft { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPaused { get; set; }
    public int RepeatCount { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }
    public bool DidTick { get; private set; }
    public int CurrentRepeat { get; private set; }

    public bool IsCompleted => RepeatCount > InfiniteRepeatCount && CurrentRepeat >= RepeatCount;

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
        CurrentRepeat++;
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
        CurrentRepeat = 0;
        DidTick = false;
    }
}
