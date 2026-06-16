using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class Ring : Drawable<Ring>, IFullCloneable
{
    public Ring() { }

    public Ring(Color fill)
    {
        Fill = fill;
    }

    public float InnerRadius { get; set; } = 0;
    public float OuterRadius { get; set; } = 0;
    public float StartAngle { get; set; } = 0;
    public float EndAngle { get; set; } = 360;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public int Segments { get; set; } = 0;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawRing(transform, this);
    }
}

public static class RingExtensions
{
    extension(Graphics graphics)
    {
        public void FillRing(
            float x,
            float y,
            float innerRadius,
            float outerRadius,
            float startAngle,
            float endAngle,
            Color? color = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.FillRing(
                new Vector2(x, y),
                innerRadius,
                outerRadius,
                startAngle,
                endAngle,
                color,
                segments,
                camera
            );
        }

        public void FillRing(
            Vector2 center,
            float innerRadius,
            float outerRadius,
            float startAngle,
            float endAngle,
            Color? color = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            var radius = innerRadius.Max(outerRadius);
            if (
                colorValue == Color.Transparent
                || (graphics.Culling() && !graphics.IsBoxInBounds(center - radius, new Vector2(radius * 2), camera))
            )
                return;
            segments = Drawing.CalculateSegments(radius, startAngle, endAngle, segments);
            graphics.BeginDrawing(camera);
            Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, segments, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void StrokeRing(
            float x,
            float y,
            float innerRadius,
            float outerRadius,
            float startAngle,
            float endAngle,
            Color? color = null,
            float? strokeWidth = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            graphics.StrokeRing(
                new Vector2(x, y),
                innerRadius,
                outerRadius,
                startAngle,
                endAngle,
                color,
                strokeWidth,
                segments,
                camera
            );
        }

        public void StrokeRing(
            Vector2 center,
            float innerRadius,
            float outerRadius,
            float startAngle,
            float endAngle,
            Color? color = null,
            float? strokeWidth = null,
            int segments = 0,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            var maxRadius = innerRadius.Max(outerRadius);
            var minRadius = innerRadius.Min(outerRadius);
            var innerStrokeRadius = (minRadius - strokeWidthValue).Max(0);
            if (
                colorValue == Color.Transparent
                || strokeWidthValue <= 0
                || (
                    graphics.Culling()
                    && !graphics.IsBoxInBounds(center - maxRadius, new Vector2(maxRadius * 2), camera, strokeWidthValue)
                )
            )
                return;
            var startDirection = startAngle.Min(endAngle).DegToDirection();
            var endDirection = endAngle.Max(startAngle).DegToDirection();
            var startTangent = new Vector2(-startDirection.Y, startDirection.X);
            var endTangent = new Vector2(-endDirection.Y, endDirection.X);
            var startInner = center + startDirection * innerStrokeRadius;
            var endInner = center + endDirection * innerStrokeRadius;
            var startOuter = center + startDirection * (maxRadius + strokeWidthValue);
            var endOuter = center + endDirection * (maxRadius + strokeWidthValue);
            var startOffset = startTangent * (strokeWidthValue * 0.5f);
            var endOffset = endTangent * (strokeWidthValue * 0.5f);
            segments = Drawing.CalculateSegments(maxRadius, startAngle, endAngle, segments);
            graphics.BeginDrawing(camera);
            Raylib.DrawRing(
                center,
                maxRadius,
                maxRadius + strokeWidthValue,
                startAngle,
                endAngle,
                segments,
                colorValue.RColor
            );
            if (minRadius > 0)
                Raylib.DrawRing(
                    center,
                    innerStrokeRadius,
                    minRadius,
                    startAngle,
                    endAngle,
                    segments,
                    colorValue.RColor
                );
            Raylib.DrawLineEx(startInner - startOffset, startOuter - startOffset, strokeWidthValue, colorValue.RColor);
            Raylib.DrawLineEx(endInner + endOffset, endOuter + endOffset, strokeWidthValue, colorValue.RColor);
            graphics.EndDrawing();
        }

        public void DrawRing(Ring ring)
        {
            graphics.DrawRing(new Transform(), ring);
        }

        public void DrawRing(Transform transform, Ring ring)
        {
            ring.OnBeginDrawing?.Invoke(transform, ring, graphics);
            transform += ring.Transform;
            var camera = ring.Camera.Get();
            var startAngle = ring.StartAngle;
            var endAngle = ring.EndAngle;
            var fill = ring.Fill;
            var stroke = ring.Stroke;
            var strokeWidth = ring.StrokeWidth;
            var segments = ring.Segments;
            var order = ring.DrawOrder;
            var position = transform.Position;
            var scale = transform.Scale.Abs().Min();
            var innerRadius = ring.InnerRadius * scale;
            var outerRadius = ring.OuterRadius * scale;
            graphics.PushMatrix();
            graphics.Pivot(transform, false);
            if (order == DrawOrder.StrokeThenFill)
            {
                graphics.StrokeRing(
                    position,
                    innerRadius,
                    outerRadius,
                    startAngle,
                    endAngle,
                    stroke,
                    strokeWidth,
                    segments,
                    camera
                );
                graphics.FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, segments, camera);
            }
            else
            {
                graphics.FillRing(position, innerRadius, outerRadius, startAngle, endAngle, fill, segments, camera);
                graphics.StrokeRing(
                    position,
                    innerRadius,
                    outerRadius,
                    startAngle,
                    endAngle,
                    stroke,
                    strokeWidth,
                    segments,
                    camera
                );
            }

            graphics.PopMatrix();
            ring.OnEndDrawing?.Invoke(transform, ring, graphics);
        }
    }
}
