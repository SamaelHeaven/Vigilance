using ZLinq;

namespace Vigilance.Drawing;

public sealed class BatchedSpriteAnimation : Animation<BatchedSpriteAnimationFrame>
{
    public BatchedSpriteAnimation(
        IEnumerable<BatchedSpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames, delay, cycleCount, startIndex) { }

    public BatchedSpriteAnimation(
        BatchedSpriteAnimationFrame[] frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames, delay, cycleCount, startIndex) { }

    public BatchedSpriteAnimation(
        in TextureAtlasBatchedSpriteAnimationExtensions.BatchedSpriteAnimationFrameEnumerable frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames.AsValueEnumerable().ToArray(), delay, cycleCount, startIndex) { }
}
