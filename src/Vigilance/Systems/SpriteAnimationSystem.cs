using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class SpriteAnimationSystem() : GameSystem(queryWithDisabled: true)
{
    public override void Update()
    {
        foreach (var animation in Components<SpriteAnimation>())
            animation.Update();

        foreach (var (animation, sprite) in Components<SpriteAnimation, Sprite>())
            animation.UpdateSprite(sprite);

        foreach (var controller in Components<SpriteAnimationController>())
            controller.Animation.Update();

        foreach (var (controller, sprite) in Components<SpriteAnimationController, Sprite>())
            controller.Animation.UpdateSprite(sprite);
    }
}
