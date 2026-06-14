using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class SpriteAnimation : IArrayView<SpriteAnimationFrame>
{
    public const int InfiniteCycleCount = -1;
    private readonly SpriteAnimationFrame[] _frames;
    private TimeSpan _elapsed;
    private int _index;
    private int? _nextIndex;
    private int _startIndex;

    public SpriteAnimation(
        IEnumerable<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        _frames = frames.AsValueEnumerable().ToArray();
        if (_frames.Length == 0)
            throw new ArgumentException($"{nameof(SpriteAnimation)} must have at least one frame.");
        _nextIndex = null;
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Delay = delay;
        CycleCount = cycleCount;
        Index = startIndex;
        StartIndex = startIndex;
    }

    public TimeSpan Delay { get; set; }
    public bool IsPaused { get; set; }
    public bool DidRepeat { get; set; }
    public int CycleCount { get; set; }
    public int CurrentCycle { get; private set; }

    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }

    public SpriteAnimationFrame Frame => _frames[_index];
    public bool IsCompleted => CycleCount > InfiniteCycleCount && CurrentCycle >= CycleCount;
    public int FrameCount => _frames.Length;

    public int Index
    {
        get => _index;
        set
        {
            _index = value.Clamp(0, _frames.Length - 1);
            _elapsed = TimeSpan.Zero;
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

    public ArrayEnumerator<SpriteAnimationFrame> GetEnumerator()
    {
        return _frames;
    }

    public ValueEnumerable<FromArray<SpriteAnimationFrame>, SpriteAnimationFrame> AsValueEnumerable()
    {
        return _frames.AsValueEnumerable();
    }

    public void Update(TimeSpan? step = null)
    {
        DidRepeat = false;
        if (IsPaused || _frames.Length <= 1 || IsCompleted)
            return;
        _elapsed += step ?? Time.Delta;
        var frameDelay = Delay + _frames[_index].Delay;
        if (_elapsed < frameDelay)
            return;
        _elapsed -= frameDelay;
        _index = _nextIndex ?? (_index + 1) % _frames.Length;
        if (_nextIndex.HasValue)
            _nextIndex = null;
        if (_index != _startIndex)
            return;
        DidRepeat = true;
        CurrentCycle++;
        if (IsCompleted)
            OnComplete?.Invoke();
        else
            OnRepeat?.Invoke();
    }

    public void UpdateSprite(Sprite sprite)
    {
        Frame.UpdateSprite(sprite);
    }

    public void Reset()
    {
        _index = 0;
        _elapsed = TimeSpan.Zero;
        CurrentCycle = 0;
    }
}
