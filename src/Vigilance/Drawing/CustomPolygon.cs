using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Raylib_cs;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[ValueWrapper(typeof(Drawable<ValueCustomPolygon>), "Drawable")]
public partial struct ValueCustomPolygon : IDrawable
{
    private ValueList<Vector2> _points;

    public ValueCustomPolygon()
    {
        Drawable = new Drawable<ValueCustomPolygon>();
        _points = [];
    }

    public ValueCustomPolygon(Color fill)
        : this()
    {
        Fill = fill;
    }

    public ValueCustomPolygon(IEnumerable<Vector2> points)
        : this()
    {
        _points = new ValueList<Vector2>(points);
    }

    public ValueCustomPolygon(IEnumerable<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    [OverloadResolutionPriority(1)]
    public ValueCustomPolygon(in ReadOnlySpan<Vector2> points)
        : this()
    {
        _points = points.AsValueEnumerable().ToValueList();
    }

    [OverloadResolutionPriority(1)]
    public ValueCustomPolygon(in ReadOnlySpan<Vector2> points, Color fill)
        : this(points)
    {
        Fill = fill;
    }

    public ValueListRef<Vector2> Points
    {
        [UnscopedRef]
        readonly get => Unsafe.AsRef(in _points).AsRef();
        set => _points = value.AsValueEnumerable().ToValueList();
    }

    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawCustomPolygon(transform, this);
    }
}

[ValueWrapper(typeof(ValueCustomPolygon))]
public sealed partial class CustomPolygon : IDrawable, IFullCloneable
{
    object IDeepCloneable.DeepClone()
    {
        var result = this.ShallowClone();
        result.Value.Points = Value.Points;
        return result;
    }

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform)]), true);
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
                || graphics.Culling() && !graphics.IsPolygonInBounds(points, camera)
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
                || graphics.Culling() && !graphics.IsPolygonInBounds(points, camera, strokeWidthValue * 0.5f)
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

        public void DrawCustomPolygon(in ValueCustomPolygon polygon)
        {
            graphics.DrawCustomPolygon(new Transform(), polygon);
        }

        public unsafe void DrawCustomPolygon(Transform transform, in ValueCustomPolygon polygon)
        {
            using var _ = Drawable<ValueCustomPolygon>.EnterDrawing(ref transform, polygon.Drawable, polygon, graphics);
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
                    var points = stackalloc Vector2[polygon.Points.Count];
                    var i = 0;
                    foreach (var point in polygon.Points)
                        points[i++] = point;
                    span = new Span<Vector2>(points, polygon.Points.Count);
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
