using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnUpdate(() =>
        {
            var step = Time.Delta;
            scene.Each(
                (ref Animation animation) =>
                {
                    animation.Update(step);
                }
            );

            scene.Each(
                static (ref Animation animation, ref Sprite sprite) =>
                {
                    animation.UpdateSprite(sprite);
                }
            );

            scene.Each(
                (ref AnimationController controller) =>
                {
                    controller.Animation.Update(step);
                }
            );

            scene.Each(
                static (ref AnimationController controller, ref Sprite sprite) =>
                {
                    controller.Animation.UpdateSprite(sprite);
                }
            );
        });
    }
}
