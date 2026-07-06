using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

public sealed class BatchedSpriteAnimationController : AnimationController<BatchedSpriteAnimation>
{
    [OverloadResolutionPriority(1)]
    public BatchedSpriteAnimationController(params ReadOnlySpan<(string, BatchedSpriteAnimation)> animations)
        : base(animations) { }

    [OverloadResolutionPriority(1)]
    public BatchedSpriteAnimationController(
        params ReadOnlySpan<KeyValuePair<string, BatchedSpriteAnimation>> animations
    )
        : base(animations) { }

    public BatchedSpriteAnimationController(IEnumerable<(string, BatchedSpriteAnimation)> animations)
        : base(animations) { }

    public BatchedSpriteAnimationController(IEnumerable<KeyValuePair<string, BatchedSpriteAnimation>> animations)
        : base(animations) { }
}
