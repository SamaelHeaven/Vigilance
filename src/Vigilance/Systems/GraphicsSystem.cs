using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class GraphicsSystem : ISystem
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
            if (entity.Has<Triangle>())
                graphics.DrawTriangle(entity.WorldTransform, entity.Get<Triangle>());
            if (entity.Has<RegularPolygon>())
                graphics.DrawRegularPolygon(entity.WorldTransform, entity.Get<RegularPolygon>());
            if (entity.Has<CustomPolygon>())
                graphics.DrawCustomPolygon(entity.WorldTransform, entity.Get<CustomPolygon>());
            if (entity.Has<Ring>())
                graphics.DrawRing(entity.WorldTransform, entity.Get<Ring>());
            if (entity.Has<Line>())
                graphics.DrawLine(entity.WorldTransform, entity.Get<Line>());
            if (entity.Has<Text>())
                graphics.DrawText(entity.WorldTransform, entity.Get<Text>());
            if (entity.Has<Texture>())
                graphics.DrawSprite(entity.WorldTransform, new Sprite { Texture = entity.Get<Texture>() });
            if (entity.Has<Sprite>())
                graphics.DrawSprite(entity.WorldTransform, entity.Get<Sprite>());
        });
    }
}
