using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null, bool withDisabled = true) : GameSystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;
    public bool WithDisabled { get; set; } = withDisabled;

    public override void Configure()
    {
        Scene.OnAddOrSet<Sprite>(OnSetSprite);
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange(
            this,
            Scene.Entries<Rectangle>().WithDisabled(WithDisabled),
            static (entity, self, rectangle) => self.Graphics.DrawRectangle(entity.WorldTransform, rectangle)
        );

        commands.AddRange(
            this,
            Scene.Entries<RectangleGradient>().WithDisabled(WithDisabled),
            static (entity, self, rectangleGradient) =>
                self.Graphics.DrawRectangleGradient(entity.WorldTransform, rectangleGradient)
        );

        commands.AddRange(
            this,
            Scene.Entries<Circle>().WithDisabled(WithDisabled),
            static (entity, self, circle) => self.Graphics.DrawCircle(entity.WorldTransform, circle)
        );

        commands.AddRange(
            this,
            Scene.Entries<CircleGradient>().WithDisabled(WithDisabled),
            static (entity, self, circleGradient) =>
                self.Graphics.DrawCircleGradient(entity.WorldTransform, circleGradient)
        );

        commands.AddRange(
            this,
            Scene.Entries<Triangle>().WithDisabled(WithDisabled),
            static (entity, self, triangle) => self.Graphics.DrawTriangle(entity.WorldTransform, triangle)
        );

        commands.AddRange(
            this,
            Scene.Entries<RegularPolygon>().WithDisabled(WithDisabled),
            static (entity, self, regularPolygon) =>
                self.Graphics.DrawRegularPolygon(entity.WorldTransform, regularPolygon)
        );

        commands.AddRange(
            this,
            Scene.Entries<CustomPolygon>().WithDisabled(WithDisabled),
            static (entity, self, customPolygon) =>
                self.Graphics.DrawCustomPolygon(entity.WorldTransform, customPolygon)
        );

        commands.AddRange(
            this,
            Scene.Entries<Ring>().WithDisabled(WithDisabled),
            static (entity, self, ring) => self.Graphics.DrawRing(entity.WorldTransform, ring)
        );

        commands.AddRange(
            this,
            Scene.Entries<Line>().WithDisabled(WithDisabled),
            static (entity, self, line) => self.Graphics.DrawLine(entity.WorldTransform, line)
        );

        commands.AddRange(
            this,
            Scene.Entries<Text>().WithDisabled(WithDisabled),
            static (entity, self, text) => self.Graphics.DrawText(entity.WorldTransform, text)
        );

        commands.AddRange(
            this,
            Scene.Entries<Sprite>().WithDisabled(WithDisabled),
            static (entity, self, sprite) => self.Graphics.DrawSprite(entity.WorldTransform, sprite)
        );

        commands.AddRange(
            this,
            Scene.Entries<Grid>().WithDisabled(WithDisabled),
            static (entity, self, grid) => self.Graphics.DrawGrid(entity.WorldTransform, grid)
        );
    }

    private static void OnSetSprite(Entity entity, Sprite sprite)
    {
        if (entity.Scale == Vector2.One)
            entity.Scale = sprite.Texture.Size;
    }
}
