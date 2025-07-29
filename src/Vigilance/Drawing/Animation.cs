using System.Collections;
using System.Collections.Immutable;
using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class Animation : IEnumerable<AnimationFrame>
{
    public const int InfiniteRepeatCount = -1;
    private readonly IImmutableList<AnimationFrame> _frames;
    private TimeSpan _elapsed;
    private int _index;
    private int? _nextIndex;
    private int _repeatCounter;
    private int _startIndex;

    public Animation(
        IReadOnlyCollection<AnimationFrame> frames,
        TimeSpan delay,
        int repeatCount = InfiniteRepeatCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        if (frames.Count == 0)
            throw new ArgumentException("Animation must have at least one frame.");
        _frames = frames.ToImmutableList();
        _nextIndex = null;
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Delay = delay;
        RepeatCount = repeatCount;
        Index = startIndex;
        StartIndex = startIndex;
    }

    public TimeSpan Delay { get; set; }
    public bool Paused { get; set; }
    public int RepeatCount { get; set; }

    public AnimationFrame Frame => _frames[_index];

    public int Index
    {
        get => _index;
        set
        {
            _index = int.Clamp(value, 0, _frames.Count - 1);
            _elapsed = TimeSpan.Zero;
        }
    }

    public int StartIndex
    {
        get => _startIndex;
        set => _startIndex = int.Clamp(value, 0, _frames.Count - 1);
    }

    public int? NextIndex
    {
        get => _nextIndex;
        set => _nextIndex = value.HasValue ? int.Clamp(value.Value, 0, _frames.Count - 1) : null;
    }

    public int FrameCount => _frames.Count;

    public IEnumerator<AnimationFrame> GetEnumerator()
    {
        return _frames.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public event Action? OnComplete;
    public event Action? OnRepeat;

    public void Update()
    {
        Update(Time.Delta);
    }

    public void Update(TimeSpan step)
    {
        if (Paused || _frames.Count <= 1 || (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount))
            return;
        _elapsed += step;
        var frameDelay = Delay + _frames[_index].Delay;
        if (_elapsed < frameDelay)
            return;
        _elapsed -= frameDelay;
        _index = _nextIndex ?? (_index + 1) % _frames.Count;
        if (_nextIndex.HasValue)
            _nextIndex = null;
        if (_index != _startIndex)
            return;
        _repeatCounter++;
        OnRepeat?.Invoke();
        if (RepeatCount > InfiniteRepeatCount && _repeatCounter >= RepeatCount)
            OnComplete?.Invoke();
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
            sprite.Source = frame.Source.Value;
        if (frame.Tint.HasValue)
            sprite.Tint = frame.Tint.Value;
        if (frame.Interpolation.HasValue)
            sprite.Interpolation = frame.Interpolation.Value;
    }

    public void Reset()
    {
        _index = 0;
        _elapsed = TimeSpan.Zero;
        _repeatCounter = 0;
    }
}
