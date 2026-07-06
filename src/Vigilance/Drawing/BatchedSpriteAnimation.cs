using System.Runtime.CompilerServices;

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

    [OverloadResolutionPriority(1)]
    public BatchedSpriteAnimation(
        in ReadOnlySpan<BatchedSpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(in frames, delay, cycleCount, startIndex) { }
}
