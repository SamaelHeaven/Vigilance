using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem : GameSystem
{
    public override void Update()
    {
        var step = Time.Delta;
        foreach (var animation in Scene.Components<Animation>())
            animation.Update(step);
        foreach (var (animation, sprite) in Scene.Components<Animation, Sprite>())
            animation.UpdateSprite(sprite);
        foreach (var controller in Scene.Components<AnimationController>())
            controller.Animation.Update(step);
        foreach (var (controller, sprite) in Scene.Components<AnimationController, Sprite>())
            controller.Animation.UpdateSprite(sprite);
    }
}
