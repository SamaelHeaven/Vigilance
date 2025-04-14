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
            if (entity.Has<Triangle>())
                graphics.DrawTriangle(entity.WorldTransform, ref entity.Get<Triangle>());
            if (entity.Has<RegularPolygon>())
                graphics.DrawRegularPolygon(entity.WorldTransform, ref entity.Get<RegularPolygon>());
            if (entity.Has<CustomPolygon>())
                graphics.DrawCustomPolygon(entity.WorldTransform, ref entity.Get<CustomPolygon>());
            if (entity.Has<Ring>())
                graphics.DrawRing(entity.WorldTransform, ref entity.Get<Ring>());
            if (entity.Has<Line>())
                graphics.DrawLine(entity.WorldTransform, ref entity.Get<Line>());
            if (entity.Has<Text>())
                graphics.DrawText(entity.WorldTransform, ref entity.Get<Text>());
            if (entity.Has<Texture>())
                graphics.DrawSprite(entity.WorldTransform, new Sprite { Texture = entity.Get<Texture>() });
            if (entity.Has<Sprite>())
                graphics.DrawSprite(entity.WorldTransform, ref entity.Get<Sprite>());
        });
    }
}
