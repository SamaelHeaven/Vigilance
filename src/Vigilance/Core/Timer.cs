namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteRepeatCount = -1;
    private int _repeatCounter;

    public Timer(
        TimeSpan delay,
        TimeSpan? initialTime = null,
        int repeatCount = InfiniteRepeatCount,
        Action? repeatAction = null,
        Action? completeAction = null,
        Func<TimeSpan>? timeStepFunc = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        TimeLeft = initialTime ?? delay;
        Delay = delay;
        RepeatCount = repeatCount;
        TimeStepFunc = timeStepFunc ?? (() => Time.Delta);
    }

    public bool IsFinished => TimeLeft <= TimeSpan.Zero;

    public TimeSpan TimeLeft { get; set; }
    public TimeSpan Delay { get; set; }
    public bool IsPaused { get; set; }
    public int RepeatCount { get; set; }
    public Func<TimeSpan> TimeStepFunc { get; set; }

    public event Action? OnComplete;
    public event Action? OnRepeat;

    public void Update()
    {
        Update(TimeStepFunc.Invoke());
    }

    public void Update(TimeSpan step)
    {
        if (IsPaused || (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount))
            return;
        TimeLeft -= step;
        if (!IsFinished)
            return;
        TimeLeft += Delay;
        _repeatCounter++;
        OnRepeat?.Invoke();
        if (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount)
            OnComplete?.Invoke();
    }

    public void Reset()
    {
        TimeLeft = Delay;
        _repeatCounter = 0;
    }
}
