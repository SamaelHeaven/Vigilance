using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public struct GraphicsSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnRender(static entity =>
        {
            var graphics = Renderer.Graphics;
            if (entity.Has<Color>())
                graphics.DrawRectangle(entity.WorldTransform, new Rectangle { Fill = entity.Get<Color>() });
            if (entity.Has<Rectangle>())
                graphics.DrawRectangle(entity.WorldTransform, ref entity.Get<Rectangle>());
            if (entity.Has<Circle>())
                graphics.DrawCircle(entity.WorldTransform, ref entity.Get<Circle>());
            if (entity.Has<Text>())
                graphics.DrawText(entity.WorldTransform, ref entity.Get<Text>());
        });
    }
}
