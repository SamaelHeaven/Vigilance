using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: true)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Configure()
    {
        Scene.OnAddOrSet<Sprite>(OnSetSprite);
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange<GraphicsSystem, Rectangle>(
            this,
            static (system, entity, rectangle) => system.Graphics.DrawRectangle(entity.WorldTransform, rectangle)
        );

        commands.AddRange<GraphicsSystem, RectangleGradient>(
            this,
            static (system, entity, rectangleGradient) =>
                system.Graphics.DrawRectangleGradient(entity.WorldTransform, rectangleGradient)
        );

        commands.AddRange<GraphicsSystem, Circle>(
            this,
            static (system, entity, circle) => system.Graphics.DrawCircle(entity.WorldTransform, circle)
        );

        commands.AddRange<GraphicsSystem, CircleGradient>(
            this,
            static (system, entity, circleGradient) =>
                system.Graphics.DrawCircleGradient(entity.WorldTransform, circleGradient)
        );

        commands.AddRange<GraphicsSystem, Triangle>(
            this,
            static (system, entity, triangle) => system.Graphics.DrawTriangle(entity.WorldTransform, triangle)
        );

        commands.AddRange<GraphicsSystem, RegularPolygon>(
            this,
            static (system, entity, regularPolygon) =>
                system.Graphics.DrawRegularPolygon(entity.WorldTransform, regularPolygon)
        );

        commands.AddRange<GraphicsSystem, CustomPolygon>(
            this,
            static (system, entity, customPolygon) =>
                system.Graphics.DrawCustomPolygon(entity.WorldTransform, customPolygon)
        );

        commands.AddRange<GraphicsSystem, Ring>(
            this,
            static (system, entity, ring) => system.Graphics.DrawRing(entity.WorldTransform, ring)
        );

        commands.AddRange<GraphicsSystem, Line>(
            this,
            static (system, entity, line) => system.Graphics.DrawLine(entity.WorldTransform, line)
        );

        commands.AddRange<GraphicsSystem, Text>(
            this,
            static (system, entity, text) => system.Graphics.DrawText(entity.WorldTransform, text)
        );

        commands.AddRange<GraphicsSystem, Sprite>(
            this,
            static (system, entity, sprite) => system.Graphics.DrawSprite(entity.WorldTransform, sprite)
        );

        commands.AddRange<GraphicsSystem, Grid>(
            this,
            static (system, entity, grid) => system.Graphics.DrawGrid(entity.WorldTransform, grid)
        );
    }

    private static void OnSetSprite(Entity entity, Sprite sprite)
    {
        if (entity.Scale == Vector2.One)
            entity.Scale = sprite.Texture.Size;
    }
}
