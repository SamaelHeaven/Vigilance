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
                graphics.DrawRectangle(entity.WorldTransform, entity.Get<Rectangle>());
            if (entity.Has<Circle>())
                graphics.DrawCircle(entity.WorldTransform, entity.Get<Circle>());
            if (entity.Has<Text>())
                graphics.DrawText(entity.WorldTransform, entity.Get<Text>());
        });
    }
}
