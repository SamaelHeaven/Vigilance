using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed class Tween
{
    public const int InfiniteRepeatCount = -1;

    public Tween(
        TimeSpan duration,
        TimeSpan? initialTime = null,
        int repeatCount = InfiniteRepeatCount,
        bool alternateDirection = false,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        TimeLeft = initialTime ?? duration;
        Duration = duration;
        RepeatCount = repeatCount;
        AlternateDirection = alternateDirection;
    }

    public TimeSpan TimeLeft { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPaused { get; set; }
    public int RepeatCount { get; set; }
    public bool AlternateDirection { get; }
    public bool Reversed { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }
    public bool DidTick { get; private set; }
    public int CurrentRepeat { get; private set; }

    public bool IsCompleted => RepeatCount > InfiniteRepeatCount && CurrentRepeat >= RepeatCount;

    public float Progress =>
        Duration == TimeSpan.Zero ? 1f : (1f - (float)(TimeLeft.TotalSeconds / Duration.TotalSeconds)).Clamp(0f, 1f);

    public float Value(Func<float, float> ease)
    {
        var progress = Reversed ? 1f - Progress : Progress;
        return ease.Invoke(progress);
    }

    public T Interpolate<T>(Func<float, float> ease, T start, T end, Func<T, T, float, T> interpolate)
    {
        return interpolate.Invoke(start, end, Value(ease));
    }

    public float Interpolate(Func<float, float> ease, float start, float end)
    {
        return Interpolate(ease, start, end, float.Lerp);
    }

    public Vector2 Interpolate(Func<float, float> ease, Vector2 start, Vector2 end)
    {
        return Interpolate(ease, start, end, Vector2.Lerp);
    }

    public Color Interpolate(Func<float, float> ease, Color start, Color end)
    {
        return Interpolate(ease, start, end, Color.Lerp);
    }

    public void Update(TimeSpan? step = null)
    {
        DidTick = false;
        if (IsPaused || IsCompleted)
            return;
        TimeLeft -= step ?? Time.Delta;
        if (TimeLeft > TimeSpan.Zero)
            return;
        DidTick = true;
        TimeLeft += Duration;
        CurrentRepeat++;
        if (AlternateDirection)
            Reversed = !Reversed;
        OnRepeat?.Invoke();
        if (IsCompleted)
            OnComplete?.Invoke();
    }

    public void Reset()
    {
        TimeLeft = Duration;
        CurrentRepeat = 0;
        DidTick = false;
    }
}
