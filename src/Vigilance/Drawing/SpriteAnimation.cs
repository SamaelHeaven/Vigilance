using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

public class SpriteAnimation : Animation<SpriteAnimationFrame>
{
    public SpriteAnimation(
        IEnumerable<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(frames, delay, cycleCount, startIndex) { }

    [OverloadResolutionPriority(1)]
    public SpriteAnimation(
        in ReadOnlySpan<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0
    )
        : base(in frames, delay, cycleCount, startIndex) { }
}
