using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(withDisabled: WithDisabled.Yes)
{
    public override void Update()
    {
        foreach (var animation in Components<Animation>())
            animation.Update();

        foreach (var (animation, sprite) in Components<Animation, Sprite>())
            animation.UpdateSprite(sprite);

        foreach (var controller in Components<AnimationController>())
            controller.Animation.Update();

        foreach (var (controller, sprite) in Components<AnimationController, Sprite>())
            controller.Animation.UpdateSprite(sprite);
    }
}
