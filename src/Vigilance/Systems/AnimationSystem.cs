using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem(bool withDisabled = true) : GameSystem
{
    public bool WithDisabled { get; set; } = withDisabled;

    public override void Update()
    {
        var step = Time.Delta;
        foreach (var animation in Scene.Components<Animation>().WithDisabled(WithDisabled))
            animation.Update(step);
        foreach (var (animation, sprite) in Scene.Components<Animation, Sprite>().WithDisabled(WithDisabled))
            animation.UpdateSprite(sprite);
        foreach (var controller in Scene.Components<AnimationController>().WithDisabled(WithDisabled))
            controller.Animation.Update(step);
        foreach (var (controller, sprite) in Scene.Components<AnimationController, Sprite>().WithDisabled(WithDisabled))
            controller.Animation.UpdateSprite(sprite);
    }
}
