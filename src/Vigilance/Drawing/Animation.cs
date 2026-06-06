using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class Animation : IArrayView<AnimationFrame>
{
    public const int InfiniteCycleCount = -1;
    private readonly AnimationFrame[] _frames;
    private TimeSpan _elapsed;
    private int _index;
    private int? _nextIndex;
    private int _startIndex;

    public Animation(
        IEnumerable<AnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        _frames = frames.AsValueEnumerable().ToArray();
        if (_frames.Length == 0)
            throw new ArgumentException("Animation must have at least one frame.");
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

    public AnimationFrame Frame => _frames[_index];
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

    public ArrayEnumerator<AnimationFrame> GetEnumerator()
    {
        return _frames;
    }

    public ValueEnumerable<FromArray<AnimationFrame>, AnimationFrame> AsValueEnumerable()
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
        var frame = Frame;
        if (frame.Texture is not null)
            sprite.Texture = frame.Texture;
        if (frame.FlipX.HasValue)
            sprite.FlipX = frame.FlipX.Value;
        if (frame.FlipY.HasValue)
            sprite.FlipY = frame.FlipY.Value;
        if (frame.Source.HasValue)
            sprite.Source = frame.Source;
        if (frame.Tint.HasValue)
            sprite.Tint = frame.Tint.Value;
        if (frame.NPatchInfo.HasValue)
            sprite.NPatchInfo = frame.NPatchInfo;
        if (frame.Interpolation.HasValue)
            sprite.Interpolation = frame.Interpolation.Value;
    }

    public void Reset()
    {
        _index = 0;
        _elapsed = TimeSpan.Zero;
        CurrentCycle = 0;
    }
}
