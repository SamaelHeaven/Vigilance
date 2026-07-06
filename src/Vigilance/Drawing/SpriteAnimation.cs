using ZLinq;

namespace Vigilance.Drawing;

public sealed class SpriteAnimation : Animation<SpriteAnimationFrame>
{
    public SpriteAnimation(
        IEnumerable<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames, delay, cycleCount, startIndex) { }

    public SpriteAnimation(
        SpriteAnimationFrame[] frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames, delay, cycleCount, startIndex) { }

    public SpriteAnimation(
        in TextureAtlasSpriteAnimationExtensions.SpriteAnimationFrameEnumerable frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames.AsValueEnumerable().ToArray(), delay, cycleCount, startIndex) { }
}
