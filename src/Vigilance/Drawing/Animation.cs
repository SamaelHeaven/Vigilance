using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class Animation : IListView<AnimationFrame>
{
    public const int InfiniteRepeatCount = -1;
    private readonly List<AnimationFrame> _frames;
    private TimeSpan _elapsed;
    private int _index;
    private int? _nextIndex;
    private int _startIndex;

    public Animation(
        IEnumerable<AnimationFrame> frames,
        TimeSpan delay,
        int repeatCount = InfiniteRepeatCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null,
        Func<TimeSpan>? timeStepFunc = null
    )
    {
        _frames = frames.AsValueEnumerable().ToList();
        if (_frames.Count == 0)
            throw new ArgumentException("Animation must have at least one frame.");
        _nextIndex = null;
        OnComplete = completeAction;
        OnRepeat = repeatAction;
        Delay = delay;
        RepeatCount = repeatCount;
        Index = startIndex;
        StartIndex = startIndex;
        TimeStepFunc = timeStepFunc ?? (() => Time.Delta);
    }

    public TimeSpan Delay { get; set; }
    public bool IsPaused { get; set; }
    public bool DidRepeat { get; set; }
    public int RepeatCount { get; set; }
    public int CurrentRepeat { get; private set; }
    public Func<TimeSpan> TimeStepFunc { get; set; }

    public Action? OnComplete { get; set; }
    public Action? OnRepeat { get; set; }

    public AnimationFrame Frame => _frames[_index];
    public bool IsCompleted => RepeatCount > InfiniteRepeatCount && CurrentRepeat >= RepeatCount;
    public int FrameCount => _frames.Count;

    public int Index
    {
        get => _index;
        set
        {
            _index = value.Clamp(0, _frames.Count - 1);
            _elapsed = TimeSpan.Zero;
        }
    }

    public int StartIndex
    {
        get => _startIndex;
        set => _startIndex = value.Clamp(0, _frames.Count - 1);
    }

    public int? NextIndex
    {
        get => _nextIndex;
        set => _nextIndex = value?.Clamp(0, _frames.Count - 1);
    }

    public List<AnimationFrame>.Enumerator GetEnumerator()
    {
        return _frames.GetEnumerator();
    }

    public ValueEnumerable<FromList<AnimationFrame>, AnimationFrame> AsValueEnumerable()
    {
        return _frames.AsValueEnumerable();
    }

    public void Update()
    {
        Update(TimeStepFunc.Invoke());
    }

    public void Update(TimeSpan step)
    {
        DidRepeat = false;
        if (IsPaused || _frames.Count <= 1 || IsCompleted)
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
        DidRepeat = true;
        CurrentRepeat++;
        OnRepeat?.Invoke();
        if (IsCompleted)
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
        CurrentRepeat = 0;
    }
}
