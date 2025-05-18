using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : ISystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public void Configure(Scene scene)
    {
        scene.OnRender(Render);
    }

    public void Render(Entity entity)
    {
        if (entity.Has<Color>())
            Graphics.DrawRectangle(entity.WorldTransform, new Rectangle { Fill = entity.Get<Color>() });
        if (entity.Has<Rectangle>())
            Graphics.DrawRectangle(entity.WorldTransform, entity.Get<Rectangle>());
        if (entity.Has<Circle>())
            Graphics.DrawCircle(entity.WorldTransform, entity.Get<Circle>());
        if (entity.Has<Triangle>())
            Graphics.DrawTriangle(entity.WorldTransform, entity.Get<Triangle>());
        if (entity.Has<RegularPolygon>())
            Graphics.DrawRegularPolygon(entity.WorldTransform, entity.Get<RegularPolygon>());
        if (entity.Has<CustomPolygon>())
            Graphics.DrawCustomPolygon(entity.WorldTransform, entity.Get<CustomPolygon>());
        if (entity.Has<Ring>())
            Graphics.DrawRing(entity.WorldTransform, entity.Get<Ring>());
        if (entity.Has<Line>())
            Graphics.DrawLine(entity.WorldTransform, entity.Get<Line>());
        if (entity.Has<Text>())
            Graphics.DrawText(entity.WorldTransform, entity.Get<Text>());
        if (entity.Has<Texture>())
            Graphics.DrawSprite(entity.WorldTransform, new Sprite { Texture = entity.Get<Texture>() });
        if (entity.Has<Sprite>())
            Graphics.DrawSprite(entity.WorldTransform, entity.Get<Sprite>());
    }
}
