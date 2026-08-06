using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueCircle>), "Drawable")]
public partial struct ValueCircle : IDrawable
{
    public ValueCircle(Color fill)
        : this()
    {
        Fill = fill;
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawCircle(transform, this);
    }
}

[ValueWrapper(typeof(ValueCircle))]
public sealed partial class Circle : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }
}

public static class CircleExtensions
{
    extension(Graphics graphics)
    {
        public void FillCircle(
            float x,
            float y,
            float radius,
            Color? color = null,
            float startAngle = 0,
            float endAngle = 360,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.FillCircle(new Vector2(x, y), radius, color, startAngle, endAngle, segments, camera);
        }

        public void FillCircle(
            Vector2 center,
            float radius,
            Color? color = null,
            float startAngle = 0,
            float endAngle = 360,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (
                colorValue == Color.Transparent
                || graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera)
            )
                return;
            segments = Drawing.CalculateSegments(radius, startAngle, endAngle, segments);
            graphics.BeginDrawing(camera);
            Raylib.DrawCircleSector(center, radius, startAngle, endAngle, segments, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void StrokeCircle(
            float x,
            float y,
            float radius,
            Color? color = null,
            float? strokeWidth = null,
            float startAngle = 0,
            float endAngle = 360,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.StrokeCircle(
                new Vector2(x, y),
                radius,
                color,
                strokeWidth,
                startAngle,
                endAngle,
                segments,
                camera
            );
        }

        public void StrokeCircle(
            Vector2 center,
            float radius,
            Color? color = null,
            float? strokeWidth = null,
            float startAngle = 0,
            float endAngle = 360,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || strokeWidthValue <= 0
                || graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera)
            )
                return;
            segments = Drawing.CalculateSegments(radius, startAngle, endAngle, segments);
            graphics.BeginDrawing(camera);
            Raylib.DrawRing(
                center,
                radius,
                radius + strokeWidthValue,
                startAngle,
                endAngle,
                segments,
                colorValue.RColor
            );
            graphics.EndDrawing();
        }

        public void DrawCircle(in ValueCircle circle)
        {
            graphics.DrawCircle(new Transform(), circle);
        }

        public void DrawCircle(Transform transform, in ValueCircle circle)
        {
            using var _ = Drawable<ValueCircle>.EnterDrawing(ref transform, circle.Drawable, circle, graphics);
            var camera = circle.Camera.Get();
            var fill = circle.Fill;
            var stroke = circle.Stroke;
            var strokeWidth = circle.StrokeWidth;
            var startAngle = circle.StartAngle;
            var endAngle = circle.EndAngle;
            var segments = circle.Segments;
            var order = circle.DrawOrder;
            var position = transform.Position;
            var scale = transform.Scale;
            var radius = scale.Abs().Min() * 0.5f;
            graphics.Pivot(transform, false);
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeCircle(position, radius, stroke, strokeWidth, startAngle, endAngle, segments, camera);
                graphics.FillCircle(position, radius, fill, startAngle, endAngle, segments, camera);
            }
            else
            {
                graphics.FillCircle(position, radius, fill, startAngle, endAngle, segments, camera);
                graphics.StrokeCircle(position, radius, stroke, strokeWidth, startAngle, endAngle, segments, camera);
            }
        }
    }
}
