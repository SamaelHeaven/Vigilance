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
                    animation.UpdateSprite(ref sprite);
                }
            );

            scene.Each(
                static (ref AnimationController controller, ref Sprite sprite) =>
                {
                    controller.Animation.UpdateSprite(ref sprite);
                }
            );
        });
    }
}
