using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

public class SpriteAnimation : Animation<SpriteAnimationFrame>
{
    public SpriteAnimation(
        IEnumerable<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
        : base(frames, delay, cycleCount, startIndex, repeatAction, completeAction) { }

    [OverloadResolutionPriority(1)]
    public SpriteAnimation(
        in ReadOnlySpan<SpriteAnimationFrame> frames,
        TimeSpan delay,
        int cycleCount = InfiniteCycleCount,
        int startIndex = 0,
        Action? repeatAction = null,
        Action? completeAction = null
    )
        : base(in frames, delay, cycleCount, startIndex, repeatAction, completeAction) { }
}
