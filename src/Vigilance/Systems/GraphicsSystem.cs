using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: Inclusion.Include)
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
            static (entity, self, rectangle) => self.Graphics.DrawRectangle(entity.WorldTransform, rectangle)
        );

        commands.AddRange<GraphicsSystem, RectangleGradient>(
            this,
            static (entity, self, rectangleGradient) =>
                self.Graphics.DrawRectangleGradient(entity.WorldTransform, rectangleGradient)
        );

        commands.AddRange<GraphicsSystem, Circle>(
            this,
            static (entity, self, circle) => self.Graphics.DrawCircle(entity.WorldTransform, circle)
        );

        commands.AddRange<GraphicsSystem, CircleGradient>(
            this,
            static (entity, self, circleGradient) =>
                self.Graphics.DrawCircleGradient(entity.WorldTransform, circleGradient)
        );

        commands.AddRange<GraphicsSystem, Triangle>(
            this,
            static (entity, self, triangle) => self.Graphics.DrawTriangle(entity.WorldTransform, triangle)
        );

        commands.AddRange<GraphicsSystem, RegularPolygon>(
            this,
            static (entity, self, regularPolygon) =>
                self.Graphics.DrawRegularPolygon(entity.WorldTransform, regularPolygon)
        );

        commands.AddRange<GraphicsSystem, CustomPolygon>(
            this,
            static (entity, self, customPolygon) =>
                self.Graphics.DrawCustomPolygon(entity.WorldTransform, customPolygon)
        );

        commands.AddRange<GraphicsSystem, Ring>(
            this,
            static (entity, self, ring) => self.Graphics.DrawRing(entity.WorldTransform, ring)
        );

        commands.AddRange<GraphicsSystem, Line>(
            this,
            static (entity, self, line) => self.Graphics.DrawLine(entity.WorldTransform, line)
        );

        commands.AddRange<GraphicsSystem, Text>(
            this,
            static (entity, self, text) => self.Graphics.DrawText(entity.WorldTransform, text)
        );

        commands.AddRange<GraphicsSystem, Sprite>(
            this,
            static (entity, self, sprite) => self.Graphics.DrawSprite(entity.WorldTransform, sprite)
        );

        commands.AddRange<GraphicsSystem, Grid>(
            this,
            static (entity, self, grid) => self.Graphics.DrawGrid(entity.WorldTransform, grid)
        );
    }

    private static void OnSetSprite(Entity entity, Sprite sprite)
    {
        if (entity.Scale == Vector2.One)
            entity.Scale = sprite.Texture.Size;
    }
}
