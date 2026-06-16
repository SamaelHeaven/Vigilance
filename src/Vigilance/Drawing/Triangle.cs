using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class Triangle : Drawable<Triangle>, IFullCloneable
{
    public Triangle() { }

    public Triangle(Color fill)
    {
        Fill = fill;
    }

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3, Color fill)
        : this(p1, p2, p3)
    {
        Fill = fill;
    }

    public Vector2 P1 { get; set; } = Vector2.Zero;
    public Vector2 P2 { get; set; } = Vector2.Zero;
    public Vector2 P3 { get; set; } = Vector2.Zero;
    public Color Fill { get; set; } = Drawing.DefaultFill;
    public Color Stroke { get; set; } = Drawing.DefaultStroke;
    public float StrokeWidth { get; set; } = Drawing.DefaultStrokeWidth;
    public DrawOrder DrawOrder { get; set; } = Drawing.DefaultOrder;

    public PointEnumerable Points => new(this);

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform), nameof(Points)), true);
    }

    protected override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawTriangle(transform, this);
    }

    public readonly struct PointEnumerable : IStructEnumerable<PointEnumerator, Vector2>, IReadOnlyCollection<Vector2>
    {
        private readonly Triangle _triangle;

        internal PointEnumerable(Triangle triangle)
        {
            _triangle = triangle;
        }

        public PointEnumerator GetEnumerator()
        {
            return new PointEnumerator(_triangle);
        }

        public ValueEnumerable<StructEnumerator<PointEnumerator, Vector2>, Vector2> AsValueEnumerable()
        {
            return new StructEnumerator<PointEnumerator, Vector2>(GetEnumerator());
        }

        public int Count => 3;
    }

    public struct PointEnumerator : IStructEnumerator<Vector2>
    {
        private readonly Triangle _triangle;
        private int _index;

        internal PointEnumerator(Triangle triangle)
        {
            _triangle = triangle;
            Reset();
        }

        public bool MoveNext()
        {
            var newIndex = _index + 1;
            if (newIndex >= 3)
                return false;
            _index = newIndex;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public readonly Vector2 Current
        {
            get
            {
                return _index switch
                {
                    0 => _triangle.P1,
                    1 => _triangle.P2,
                    2 => _triangle.P3,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
        }

        public void Dispose() { }
    }
}

public static class TriangleExtensions
{
    extension(Graphics graphics)
    {
        public void FillTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color? color = null, Camera? camera = null)
        {
            ReadOnlySpan<Vector2> span = stackalloc Vector2[] { v1, v2, v3 };
            graphics.FillCustomPolygonSpan(span, color, camera);
        }

        public void StrokeTriangle(
            Vector2 v1,
            Vector2 v2,
            Vector2 v3,
            Color? color = null,
            float? strokeWidth = null,
            Camera? camera = null
        )
        {
            ReadOnlySpan<Vector2> span = stackalloc Vector2[] { v1, v2, v3 };
            graphics.StrokeCustomPolygonSpan(span, color, strokeWidth, camera);
        }

        public void DrawTriangle(Triangle triangle)
        {
            graphics.DrawTriangle(new Transform(), triangle);
        }

        public void DrawTriangle(Transform transform, Triangle triangle)
        {
            triangle.OnBeginDrawing?.Invoke(transform, triangle, graphics);
            transform += triangle.Transform;
            var camera = triangle.Camera.Get();
            var position = transform.Position;
            var scale = transform.Scale;
            var fill = triangle.Fill;
            var stroke = triangle.Stroke;
            var strokeWidth = triangle.StrokeWidth;
            var order = triangle.DrawOrder;
            graphics.PushMatrix();
            graphics.Pivot(transform, false);
            Span<Vector2> span = stackalloc Vector2[3];
            var i = 0;
            foreach (var point in triangle.Points)
                span[i++] = point;
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
            triangle.OnEndDrawing?.Invoke(transform, triangle, graphics);
        }
    }
}
