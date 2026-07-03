using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed class Tween
{
    public const int InfiniteCycleCount = -1;

    public Tween()
        : this(TimeSpan.Zero) { }

    public Tween(
        TimeSpan duration,
        TimeSpan elapsed = default,
        int cycleCount = InfiniteCycleCount,
        bool alternateDirection = false,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Elapsed = elapsed;
        Duration = duration;
        CycleCount = cycleCount;
        AlternateDirection = alternateDirection;
    }

    public TimeSpan Elapsed { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsPaused { get; set; }
    public int CycleCount { get; set; }
    public bool AlternateDirection { get; set; }
    public bool IsReversed { get; set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }
    public bool DidTick { get; private set; }
    public int CurrentCycle { get; private set; }

    public bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;

    public float Progress =>
        Duration == TimeSpan.Zero ? 1f : ((float)(Elapsed.TotalSeconds / Duration.TotalSeconds)).Clamp(0f, 1f);

    public float Value(Func<float, float>? ease = null)
    {
        var progress = IsReversed ? 1f - Progress : Progress;
        return ease?.Invoke(progress) ?? Ease.Linear(progress);
    }

    public T Interpolate<T>(T start, T end, Func<T, T, float, T> interpolate, Func<float, float>? ease = null)
    {
        return interpolate.Invoke(start, end, Value(ease));
    }

    public float Interpolate(float start, float end, Func<float, float>? ease = null)
    {
        return Interpolate(start, end, float.Lerp, ease);
    }

    public Vector2 Interpolate(Vector2 start, Vector2 end, Func<float, float>? ease = null)
    {
        return Interpolate(start, end, Vector2.Lerp, ease);
    }

    public Transform Interpolate(in Transform start, in Transform end, Func<float, float>? ease = null)
    {
        return Interpolate(start, end, Transform.Lerp, ease);
    }

    public Color Interpolate(Color start, Color end, Func<float, float>? ease = null)
    {
        return Interpolate(start, end, Color.Lerp, ease);
    }

    public void Update(TimeSpan? step = null)
    {
        DidTick = false;
        if (IsPaused || IsCompleted)
            return;
        Elapsed += step ?? Time.Delta;
        if (Elapsed < Duration)
            return;
        DidTick = true;
        CurrentCycle++;
        if (IsCompleted)
        {
            OnComplete?.Invoke();
            return;
        }

        Elapsed -= Duration;
        if (AlternateDirection)
            IsReversed = !IsReversed;
        OnRepeat?.Invoke();
    }

    public void Reset()
    {
        Elapsed = TimeSpan.Zero;
        CurrentCycle = 0;
        DidTick = false;
    }
}
