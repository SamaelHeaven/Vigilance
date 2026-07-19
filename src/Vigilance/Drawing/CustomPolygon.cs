using System.Runtime.CompilerServices;
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
    private ValueList<Vector2> _points;

    public CustomPolygon()
    {
        _points = [];
    }

    public CustomPolygon(Color fill)
        : this()
    {
        Fill = fill;
    }

    public CustomPolygon(IEnumerable<Vector2> points)
    {
        _points = new ValueList<Vector2>(points);
    }

    public CustomPolygon(IEnumerable<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    [OverloadResolutionPriority(1)]
    public CustomPolygon(in ReadOnlySpan<Vector2> points)
    {
        _points = points.AsValueEnumerable().ToValueList();
    }

    [OverloadResolutionPriority(1)]
    public CustomPolygon(in ReadOnlySpan<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public ValueListRef<Vector2> Points => _points.AsRef();
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result._points = new ValueList<Vector2>(_points);
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public override void Draw(Transform transform, Graphics graphics)
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
            graphics.FillCustomPolygon(points.AsSpan(), color, camera);
        }

        [OverloadResolutionPriority(1)]
        public unsafe void FillCustomPolygon(
            in ReadOnlySpan<Vector2> points,
            Color? color = null,
            Camera? camera = null
        )
        {
            var colorValue = color ?? Drawing.DefaultFill.Or(Color.White);
            if (
                colorValue == Color.Transparent
                || points.Length < 3
                || (graphics.Culling() && !graphics.IsPolygonInBounds(points, camera))
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
            graphics.StrokeCustomPolygon(points.AsSpan(), color, strokeWidth, camera);
        }

        [OverloadResolutionPriority(1)]
        public void StrokeCustomPolygon(
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
                || (graphics.Culling() && !graphics.IsPolygonInBounds(points, camera, strokeWidthValue * 0.5f))
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
            using var _ = Drawable.EnterDrawing(ref transform, polygon, graphics);
            var camera = polygon.Camera.Get();
            var position = transform.Position;
            var scale = transform.Scale;
            var fill = polygon.Fill;
            var stroke = polygon.Stroke;
            var strokeWidth = polygon.StrokeWidth;
            var order = polygon.DrawOrder;
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
                    graphics.StrokeCustomPolygon(span, stroke, strokeWidth, camera);
                    graphics.FillCustomPolygon(span, fill, camera);
                }
                else
                {
                    graphics.FillCustomPolygon(span, fill, camera);
                    graphics.StrokeCustomPolygon(span, stroke, strokeWidth, camera);
                }
            }
            finally
            {
                pooledArray?.Dispose();
            }
        }
    }
}
