namespace Vigilance.Drawing;

[ValueWrapper<Drawable<ValueTriangle>>("Drawable")]
public partial struct ValueTriangle : IDrawable
{
    public ValueTriangle(Color fill)
        : this()
    {
        Fill = fill;
    }

    public ValueTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
        : this()
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }

    public ValueTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color fill)
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

    public readonly PointEnumerable Points => new(this);

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform), nameof(Points)]), true);
    }

    public readonly void Draw(Transform transform, Graphics graphics)
    {
        graphics.DrawTriangle(transform, this);
    }

    public readonly struct PointEnumerable : IStructEnumerable<PointEnumerator, Vector2>, IReadOnlyCollection<Vector2>
    {
        private readonly ValueTriangle _triangle;

        internal PointEnumerable(ValueTriangle triangle)
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
        private readonly ValueTriangle _triangle;
        private int _index;

        internal PointEnumerator(ValueTriangle triangle)
        {
            _triangle = triangle;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < 3)
            {
                Current = _index switch
                {
                    0 => _triangle.P1,
                    1 => _triangle.P2,
                    2 => _triangle.P3,
                    _ => throw new IndexOutOfRangeException(),
                };
                _index++;
                return true;
            }

            Current = default!;
            _index = -1;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public Vector2 Current { get; private set; }

        public void Dispose() { }
    }
}

[ValueWrapper<ValueTriangle>]
public sealed partial class Triangle : IDrawable, IFullCloneable
{
    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude([nameof(Transform), nameof(Points)]), true);
    }
}

public static class TriangleExtensions
{
    extension(Graphics graphics)
    {
        public void FillTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color? color = null, Camera? camera = null)
        {
            graphics.FillCustomPolygon([v1, v2, v3], color, camera);
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
            graphics.StrokeCustomPolygon([v1, v2, v3], color, strokeWidth, camera);
        }

        public void DrawTriangle(in ValueTriangle triangle)
        {
            graphics.DrawTriangle(new Transform(), triangle);
        }

        public void DrawTriangle(Transform transform, in ValueTriangle triangle)
        {
            using var _ = Drawable<ValueTriangle>.EnterDrawing(ref transform, triangle.Drawable, triangle, graphics);
            var camera = triangle.Camera.Get();
            var position = transform.Position;
            var scale = transform.Scale;
            var fill = triangle.Fill;
            var stroke = triangle.Stroke;
            var strokeWidth = triangle.StrokeWidth;
            var order = triangle.DrawOrder;
            graphics.Pivot(transform, false);
            Span<Vector2> span = stackalloc Vector2[3];
            var i = 0;
            foreach (var point in triangle.Points)
                span[i++] = point;
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
    }
}
