using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class CircleGradient : Drawable<CircleGradient>
{
    public Color InnerFill { get; set; } = Drawing.DefaultFill;
    public Color OuterFill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public Color Fill
    {
        get => InnerFill.Blend(OuterFill);
        set
        {
            InnerFill = value;
            OuterFill = value;
        }
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform), nameof(Fill)), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawCircleGradient(transform, this);
    }
}

public static class CircleGradientExtensions
{
    extension(Graphics graphics)
    {
        public void FillCircleGradient(
            Vector2 center,
            float radius,
            Color? innerColor = null,
            Color? outerColor = null,
            Camera? camera = null
        )
        {
            var innerColorValue = innerColor ?? Drawing.DefaultFill.Or(Color.White);
            var outerColorValue = outerColor ?? Drawing.DefaultFill.Or(Color.White);
            if (
                (innerColorValue == Color.Transparent && outerColorValue == Color.Transparent)
                || (graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            )
                return;
            graphics.BeginDrawing(camera);
            Raylib.DrawCircleGradient(center, radius, innerColorValue.RColor, outerColorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawCircleGradient(CircleGradient circle)
        {
            graphics.DrawCircleGradient(new Transform(), circle);
        }

        public void DrawCircleGradient(Transform transform, CircleGradient circle)
        {
            circle.OnBeginDrawing?.Invoke(transform, circle, graphics);
            transform += circle.Transform;
            var camera = circle.Camera.Get();
            var innerFill = circle.InnerFill;
            var outerFill = circle.OuterFill;
            var stroke = circle.Stroke;
            var strokeWidth = circle.StrokeWidth;
            var segments = circle.Segments;
            var order = circle.DrawOrder;
            var position = transform.Position;
            var scale = transform.Scale;
            var radius = scale.Abs().Min() * 0.5f;
            graphics.PushMatrix();
            graphics.Pivot(transform, false);
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeCircle(position, radius, stroke, strokeWidth, 0, 360, segments, camera);
                graphics.FillCircleGradient(position, radius, innerFill, outerFill, camera);
            }
            else
            {
                graphics.FillCircleGradient(position, radius, innerFill, outerFill, camera);
                graphics.StrokeCircle(position, radius, stroke, strokeWidth, 0, 360, segments, camera);
            }

            graphics.PopMatrix();
            circle.OnEndDrawing?.Invoke(transform, circle, graphics);
        }
    }
}
