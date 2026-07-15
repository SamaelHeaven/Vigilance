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

public sealed class SpriteAnimationController<TKey> : AnimationController<TKey, SpriteAnimation>
    where TKey : notnull
{
    [OverloadResolutionPriority(1)]
    public SpriteAnimationController(params ReadOnlySpan<(TKey, SpriteAnimation)> animations)
        : base(animations) { }

    [OverloadResolutionPriority(1)]
    public SpriteAnimationController(params ReadOnlySpan<KeyValuePair<TKey, SpriteAnimation>> animations)
        : base(animations) { }

    public SpriteAnimationController(IEnumerable<(TKey, SpriteAnimation)> animations)
        : base(animations) { }

    public SpriteAnimationController(IEnumerable<KeyValuePair<TKey, SpriteAnimation>> animations)
        : base(animations) { }
}
