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
                (Animation animation) =>
                {
                    animation.Update(step);
                }
            );

            scene.Each(
                (Animation animation, Sprite sprite) =>
                {
                    animation.UpdateSprite(sprite);
                }
            );

            scene.Each(
                (AnimationController controller) =>
                {
                    controller.Animation.Update(step);
                }
            );

            scene.Each(
                (AnimationController controller, Sprite sprite) =>
                {
                    controller.Animation.UpdateSprite(sprite);
                }
            );
        });
    }
}
