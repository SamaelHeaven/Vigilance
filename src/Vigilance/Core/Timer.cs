namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteRepeatCount = -1;
    private int _repeatCounter;

    public Timer(
        TimeSpan delay,
        int repeatCount = InfiniteRepeatCount,
        Action? repeatAction = null,
        Action? completeAction = null,
        bool immediate = false
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Elapsed = delay;
        Delay = delay;
        RepeatCount = repeatCount;
        if (immediate)
            repeatAction?.Invoke();
    }

    public bool IsFinished => Elapsed <= TimeSpan.Zero;

    public TimeSpan Elapsed { get; set; }
    public TimeSpan Delay { get; set; }
    public bool IsPaused { get; set; }
    public int RepeatCount { get; set; }

    public event Action? OnComplete;
    public event Action? OnRepeat;

    public void Update()
    {
        Update(Time.Delta);
    }

    public void Update(TimeSpan step)
    {
        if (IsPaused || (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount))
            return;
        Elapsed -= step;
        if (!IsFinished)
            return;
        Elapsed += Delay;
        _repeatCounter++;
        OnRepeat?.Invoke();
        if (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount)
            OnComplete?.Invoke();
    }

    public void Reset()
    {
        Elapsed = Delay;
        _repeatCounter = 0;
    }
}
