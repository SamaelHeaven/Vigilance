using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class RegularPolygon : Drawable<RegularPolygon>
{
    public RegularPolygon() { }

    public RegularPolygon(Color fill)
    {
        Fill = fill;
    }

    public RegularPolygon(int sides)
    {
        Sides = sides;
    }

    public RegularPolygon(int sides, Color fill)
        : this(sides)
    {
        Fill = fill;
    }

    public int Sides { get; set; } = 0;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public override void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawRegularPolygon(transform, this);
    }
}

public static class RegularPolygonExtensions
{
    extension(Graphics graphics)
    {
        public void FillRegularPolygon(
            float x,
            float y,
            int sides,
            float radius,
            Color? color = null,
            Camera? camera = null
        )
        {
            graphics.FillRegularPolygon(new Vector2(x, y), sides, radius, color, camera);
        }

        public void FillRegularPolygon(
            Vector2 center,
            int sides,
            float radius,
            Color? color = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (
                color == Color.Transparent
                || sides < 3
                || (graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawPoly(center, sides, radius, 0, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void StrokeRegularPolygon(
            float x,
            float y,
            int sides,
            float radius,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            graphics.StrokeRegularPolygon(new Vector2(x, y), sides, radius, color, strokeWidth, camera);
        }

        public void StrokeRegularPolygon(
            Vector2 center,
            int sides,
            float radius,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || sides < 3
                || strokeWidthValue <= 0
                || (graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawPolyLinesEx(center, sides, radius, 0, radius.Min(strokeWidthValue), colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawRegularPolygon(RegularPolygon polygon)
        {
            graphics.DrawRegularPolygon(new Transform(), polygon);
        }

        public void DrawRegularPolygon(Transform transform, RegularPolygon polygon)
        {
            using var _ = Drawable.EnterDrawing(ref transform, polygon, graphics);
            var camera = polygon.Camera.Get();
            var sides = polygon.Sides;
            var fill = polygon.Fill;
            var stroke = polygon.Stroke;
            var strokeWidth = polygon.StrokeWidth;
            var order = polygon.DrawOrder;
            var position = transform.Position;
            var scale = transform.Scale;
            graphics.Pivot(transform, false);
            var radius = scale.Abs().Min() * 0.5f;
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
                graphics.FillRegularPolygon(position, sides, radius, fill, camera);
            }
            else
            {
                graphics.FillRegularPolygon(position, sides, radius, fill, camera);
                graphics.StrokeRegularPolygon(position, sides, radius, stroke, strokeWidth, camera);
            }
        }
    }
}
