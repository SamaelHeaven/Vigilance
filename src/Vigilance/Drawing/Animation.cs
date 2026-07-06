using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public class Animation<TFrame> : IAnimation, IArrayView<TFrame>, IShallowCloneable
    where TFrame : IAnimationFrame
{
    public const int InfiniteCycleCount = -1;
    private readonly TFrame[] _frames;
    private int _index;
    private int? _nextIndex;
    private int _startIndex;

    public Animation(
        IEnumerable<TFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : this(frames.ToArray(), delay, cycleCount, startIndex) { }

    [OverloadResolutionPriority(1)]
    public Animation(
        in ReadOnlySpan<TFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : this(frames.ToArray(), delay, cycleCount, startIndex) { }

    [OverloadResolutionPriority(2)]
    private Animation(TFrame[] frames, TimeSpan delay, int cycleCount = InfiniteCycleCount, int startIndex = 0)
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
    public bool IsPaused { get; set; }
    public bool DidTick { get; set; }
    public int CycleCount { get; set; }
    public int CurrentCycle { get; private set; }
    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }

    public ref TFrame Frame => ref _frames[_index];
    public bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;
    public int FrameCount => _frames.Length;

    public int Index
    {
        get => _index;
        set
        {
            _index = value.Clamp(0, _frames.Length - 1);
            Elapsed = TimeSpan.Zero;
        }
    }

    public int StartIndex
    {
        get => _startIndex;
        set => _startIndex = value.Clamp(0, _frames.Length - 1);
    }

    public int? NextIndex
    {
        get => _nextIndex;
        set => _nextIndex = value?.Clamp(0, _frames.Length - 1);
    }

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
            OnComplete?.Invoke();
        else
            OnRepeat?.Invoke();
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

    public ArrayEnumerator<TFrame> GetEnumerator()
    {
        return _frames;
    }

    public ValueEnumerable<FromArray<TFrame>, TFrame> AsValueEnumerable()
    {
        return _frames.AsValueEnumerable();
    }

    public Span<TFrame> AsSpan()
    {
        return _frames;
    }
}
