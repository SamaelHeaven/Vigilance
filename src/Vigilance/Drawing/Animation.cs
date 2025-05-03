namespace Vigilance.Drawing;

public struct Animation
{
    private readonly AnimationFrame[] _frames;
    private int _index;
    private int _startIndex;
    private int? _nextIndex;
    private TimeSpan _timer;
    private int _repeatCount;
    private Action? _completeAction;
    private Action? _repeatAction;
    public TimeSpan Delay { get; set; }
    public bool Paused { get; set; }
    public int RepeatCount { get; set; }
    public readonly ref readonly AnimationFrame Frame => ref _frames[_index];

    public int Index
    {
        readonly get => _index;
        set
        {
            _index = System.Math.Clamp(value, 0, _frames.Length - 1);
            _timer = TimeSpan.Zero;
        }
    }

    public int StartIndex
    {
        readonly get => _startIndex;
        set => _startIndex = System.Math.Clamp(value, 0, _frames.Length - 1);
    }

    public int? NextIndex
    {
        readonly get => _nextIndex;
        set => _nextIndex = value.HasValue ? System.Math.Clamp(value.Value, 0, _frames.Length - 1) : null;
    }

    public int FrameCount => _frames.Length;

    public const int InfiniteRepeatCount = -1;

    public Animation(
        AnimationFrame[] frames,
        TimeSpan delay,
        int repeatCount = InfiniteRepeatCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
    {
        if (frames.Length == 0)
            throw new ArgumentException("Animation must have at least one frame.");
        _frames = frames;
        _nextIndex = null;
        _completeAction = completeAction;
        _repeatAction = repeatAction;
        Delay = delay;
        RepeatCount = repeatCount;
        Index = startIndex;
        StartIndex = startIndex;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (Paused || _frames.Length <= 1 || (RepeatCount > InfiniteRepeatCount && _repeatCount >= RepeatCount))
            return;
        _timer += deltaTime;
        var frameDelay = Delay + _frames[_index].Delay;
        if (_timer < frameDelay)
            return;
        _timer -= frameDelay;
        _index = _nextIndex ?? (_index + 1) % _frames.Length;
        if (_nextIndex.HasValue)
            _nextIndex = null;
        if (_index != _startIndex)
            return;
        _repeatCount++;
        _repeatAction?.Invoke();
        if (RepeatCount <= InfiniteRepeatCount)
            return;
        _completeAction?.Invoke();
    }

    public void UpdateSprite(Sprite sprite)
    {
        UpdateSprite(ref sprite);
    }

    public void UpdateSprite(ref Sprite sprite)
    {
        ref readonly var frame = ref Frame;
        if (frame.Texture != null)
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
        _timer = TimeSpan.Zero;
        _repeatCount = 0;
    }

    public void OnComplete(Action action)
    {
        _completeAction += action;
    }

    public void OnRepeat(Action action)
    {
        _repeatAction += action;
    }
}
