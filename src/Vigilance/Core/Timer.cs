namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteRepeatCount = -1;
    private int _repeatCounter;

    public Timer(
        TimeSpan delay,
        int repeatCount = InfiniteRepeatCount,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Elapsed = delay;
        Delay = delay;
        RepeatCount = repeatCount;
    }

    public bool Finished => Elapsed <= TimeSpan.Zero;

    public TimeSpan Elapsed { get; set; }
    public TimeSpan Delay { get; set; }
    public bool Paused { get; set; }
    public int RepeatCount { get; set; }

    public event Action? OnComplete;
    public event Action? OnRepeat;

    public void Update(TimeSpan step)
    {
        if (Paused || (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount))
            return;
        Elapsed -= step;
        if (!Finished)
            return;
        Elapsed = Delay;
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
