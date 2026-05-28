namespace Vigilance.Core;

public sealed class Timer
{
    public const int InfiniteRepeatCount = -1;

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

    public bool IsCompleted => RepeatCount > InfiniteRepeatCount && CurrentRepeat >= RepeatCount;

    public TimeSpan TimeLeft { get; set; }
    public TimeSpan Delay { get; set; }
    public bool IsPaused { get; set; }
    public bool DidRepeat { get; set; }
    public int RepeatCount { get; set; }
    public int CurrentRepeat { get; private set; }
    public Func<TimeSpan> TimeStepFunc { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }

    public bool Update()
    {
        return Update(TimeStepFunc.Invoke());
    }

    public bool Update(TimeSpan step)
    {
        DidRepeat = false;
        if (IsPaused || IsCompleted)
            return false;
        TimeLeft -= step;
        if (TimeLeft > TimeSpan.Zero)
            return false;
        DidRepeat = true;
        TimeLeft += Delay;
        CurrentRepeat++;
        OnRepeat?.Invoke();
        if (IsCompleted)
            OnComplete?.Invoke();
        return true;
    }

    public void Reset()
    {
        TimeLeft = Delay;
        CurrentRepeat = 0;
    }
}
