using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnFixedUpdate(() =>
        {
            var deltaTime = Time.FixedDelta;
            scene.Each(
                (ref Animation animation) =>
                {
                    animation.Update(deltaTime);
                }
            );

            scene.Each(
                (ref AnimationController controller) =>
                {
                    controller.Animation.Update(deltaTime);
                }
            );

            scene.Each(
                static (ref Animation animation, ref Sprite sprite) =>
                {
                    UpdateAnimationSprite(ref animation, ref sprite);
                }
            );

            scene.Each(
                static (ref AnimationController controller, ref Sprite sprite) =>
                {
                    UpdateAnimationSprite(ref controller.Animation, ref sprite);
                }
            );
        });
    }

    private static void UpdateAnimationSprite(ref Animation animation, ref Sprite sprite)
    {
        ref readonly var frame = ref animation.Frame;
        if (frame.Texture != null)
            sprite.Texture = frame.Texture;
        if (frame.FlipX.HasValue)
            sprite.FlipX = frame.FlipX.Value;
        if (frame.FlipY.HasValue)
            sprite.FlipY = frame.FlipY.Value;
        if (frame.Source.HasValue)
            sprite.Source = frame.Source.Value;
        if (frame.Tint.HasValue)
            sprite.Tint = frame.Tint.Value;
        if (frame.Interpolation.HasValue)
            sprite.Interpolation = frame.Interpolation.Value;
    }
}
