using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class CustomPolygon : Drawable<CustomPolygon>, IDeepCloneable
{
    public CustomPolygon() { }

    public CustomPolygon(Color fill)
    {
        Fill = fill;
    }

    public CustomPolygon(IEnumerable<Vector2> points)
    {
        Points = points.ToList();
    }

    public CustomPolygon(IEnumerable<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public CustomPolygon(List<Vector2> points)
    {
        Points = points;
    }

    public CustomPolygon(List<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public List<Vector2> Points { get; set; } = [];
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Points = Points.AsValueEnumerable().ToList();
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawCustomPolygon(transform, this);
    }
}

public static class CustomPolygonExtensions
{
    extension(Graphics graphics)
    {
        public void FillCustomPolygon(IEnumerable<Vector2> points, Color? color = null, Camera? camera = null)
        {
            graphics.FillCustomPolygonSpan(points.AsSpan(), color, camera);
        }

        public unsafe void FillCustomPolygonSpan(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (
                colorValue == Color.Transparent
                || points.Length < 3
                || (graphics.Culling() && !graphics.IsPolygonInBoundsSpan(points, camera))
            )
                return;
            graphics.BeginDrawing(camera);
            fixed (Vector2* pointsBuffer = points)
            {
                Raylib.DrawTriangleFan((System.Numerics.Vector2*)pointsBuffer, points.Length, colorValue.RColor);
            }

            graphics.EndDrawing();
        }

        public void StrokeCustomPolygon(
            IEnumerable<Vector2> points,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            graphics.StrokeCustomPolygonSpan(points.AsSpan(), color, strokeWidth, camera);
        }

        public void StrokeCustomPolygonSpan(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultStroke.Or(Color.White);
            var strokeWidthValue = strokeWidth ?? Drawing.DefaultStrokeWidth.Or(1);
            if (
                colorValue == Color.Transparent
                || strokeWidthValue <= 0
                || points.Length < 3
                || (graphics.Culling() && !graphics.IsPolygonInBoundsSpan(points, camera, strokeWidthValue * 0.5f))
            )
                return;
            graphics.BeginDrawing(camera);
            for (var i = 0; i < points.Length; i++)
            {
                var start = points[i];
                var end = points[(i + 1) % points.Length];
                Raylib.DrawLineEx(start, end, strokeWidthValue, colorValue.RColor);
                Raylib.DrawCircleV(start, strokeWidthValue * 0.5f, colorValue.RColor);
            }

            graphics.EndDrawing();
        }

        public void DrawCustomPolygon(CustomPolygon polygon)
        {
            graphics.DrawCustomPolygon(new Transform(), polygon);
        }

        public void DrawCustomPolygon(Transform transform, CustomPolygon polygon)
        {
            polygon.OnBeginDrawing?.Invoke(transform, polygon, graphics);
            transform += polygon.Transform;
            var camera = polygon.Camera.Get();
            var position = transform.Position;
            var scale = transform.Scale;
            var fill = polygon.Fill;
            var stroke = polygon.Stroke;
            var strokeWidth = polygon.StrokeWidth;
            var order = polygon.DrawOrder;
            graphics.PushMatrix();
            graphics.Pivot(transform, false);
            PooledArray<Vector2>? pooledArray = null;
            try
            {
                Span<Vector2> span;
                if (polygon.Points.Count > 128)
                {
                    pooledArray = polygon.Points.AsValueEnumerable().ToArrayPool();
                    span = pooledArray.Value.Span;
                }
                else
                {
                    var points = new Vector2[polygon.Points.Count];
                    var i = 0;
                    foreach (var point in polygon.Points)
                        points[i++] = point;
                    span = points;
                }

                Coordinates.Scale(span, scale, position);
                if (order == DrawOrder.StrokeThenFill)
                {
                    graphics.StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
                    graphics.FillCustomPolygonSpan(span, fill, camera);
                }
                else
                {
                    graphics.FillCustomPolygonSpan(span, fill, camera);
                    graphics.StrokeCustomPolygonSpan(span, stroke, strokeWidth, camera);
                }

                graphics.PopMatrix();
            }
            finally
            {
                pooledArray?.Dispose();
            }

            polygon.OnEndDrawing?.Invoke(transform, polygon, graphics);
        }
    }
}
