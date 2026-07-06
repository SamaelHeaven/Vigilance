using System.Runtime.CompilerServices;

namespace Vigilance.Drawing;

public sealed class SpriteAnimationController : AnimationController<SpriteAnimation>
{
    [OverloadResolutionPriority(1)]
    public SpriteAnimationController(params ReadOnlySpan<(string, SpriteAnimation)> animations)
        : base(animations) { }

    [OverloadResolutionPriority(1)]
    public SpriteAnimationController(params ReadOnlySpan<KeyValuePair<string, SpriteAnimation>> animations)
        : base(animations) { }

    public SpriteAnimationController(IEnumerable<(string, SpriteAnimation)> animations)
        : base(animations) { }

    public SpriteAnimationController(IEnumerable<KeyValuePair<string, SpriteAnimation>> animations)
        : base(animations) { }
}
