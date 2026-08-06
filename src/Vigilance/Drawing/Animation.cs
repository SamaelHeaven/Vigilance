using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

public struct ValueAnimation<TFrame> : IAnimation, IArrayView<TFrame>
    where TFrame : IAnimationFrame
{
    public const int InfiniteCycleCount = -1;
    private readonly TFrame[] _frames;
    private int _index;
    private int? _nextIndex;
    private int _startIndex;

    public ValueAnimation(
        IEnumerable<TFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : this(frames.ToArray(), delay, cycleCount, startIndex) { }

    [OverloadResolutionPriority(1)]
    public ValueAnimation(
        in ReadOnlySpan<TFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : this(frames.ToArray(), delay, cycleCount, startIndex) { }

    [OverloadResolutionPriority(2)]
    public ValueAnimation(TFrame[] frames, TimeSpan delay, int cycleCount = InfiniteCycleCount, int startIndex = 0)
    {
        if (frames.Length == 0)
            throw new ArgumentException($"{nameof(Animation<>)} must have at least one frame.");
        _frames = frames;
        _nextIndex = null;
        Delay = delay;
        CycleCount = cycleCount;
        Index = startIndex;
        StartIndex = startIndex;
    }

    public TimeSpan Elapsed { get; set; }
    public TimeSpan Delay { get; set; }
    public bool DidTick { get; set; }
    public int CycleCount { get; set; }
    public int CurrentCycle { get; private set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }

    public readonly ref TFrame Frame => ref _frames[_index];
    public readonly bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;
    public readonly int FrameCount => _frames.Length;

    public int Index
    {
        readonly get => _index;
        set
        {
            _index = value.Clamp(0, _frames.Length - 1);
            Elapsed = TimeSpan.Zero;
        }
    }

    public int StartIndex
    {
        readonly get => _startIndex;
        set => _startIndex = value.Clamp(0, _frames.Length - 1);
    }

    public int? NextIndex
    {
        readonly get => _nextIndex;
        set => _nextIndex = value?.Clamp(0, _frames.Length - 1);
    }

    public bool IsPaused { get; set; }

    public void Update(TimeSpan? step = null)
    {
        DidTick = false;
        if (IsPaused || _frames.Length <= 1 || IsCompleted)
            return;
        Elapsed += step ?? Time.Delta;
        var frameDelay = Delay + _frames[_index].Delay;
        if (Elapsed < frameDelay)
            return;
        Elapsed -= frameDelay;
        _index = _nextIndex ?? (_index + 1) % _frames.Length;
        if (_nextIndex.HasValue)
            _nextIndex = null;
        if (_index != _startIndex)
            return;
        DidTick = true;
        CurrentCycle++;
        if (IsCompleted)
            OnComplete?.SafeInvoke();
        else
            OnRepeat?.SafeInvoke();
    }

    public void Apply(Entity entity)
    {
        Frame.Apply(entity);
    }

    public void Reset()
    {
        _index = _startIndex;
        Elapsed = TimeSpan.Zero;
        CurrentCycle = 0;
    }

    public readonly ArrayEnumerator<TFrame> GetEnumerator()
    {
        return _frames;
    }

    public readonly ValueEnumerable<FromArray<TFrame>, TFrame> AsValueEnumerable()
    {
        return _frames.AsValueEnumerable();
    }

    public readonly Span<TFrame> AsSpan()
    {
        return _frames;
    }
}

[ValueWrapper(typeof(ValueAnimation<>))]
public sealed partial class Animation<TFrame> : IAnimation, IArrayView<TFrame>, IShallowCloneable
    where TFrame : IAnimationFrame;
