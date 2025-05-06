using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnFixedUpdate(() =>
        {
            var step = Time.FixedDelta;
            scene.Each(
                (ref Animation animation) =>
                {
                    animation.Update(step);
                }
            );

            scene.Each(
                (ref AnimationController controller) =>
                {
                    controller.Animation.Update(step);
                }
            );
        });

        scene.OnRenderStart(() =>
        {
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
