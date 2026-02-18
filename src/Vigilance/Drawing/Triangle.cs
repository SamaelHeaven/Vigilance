using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

public sealed class Triangle : IFullCloneable
{
    public Triangle() { }

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
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public PointEnumerable Points => new(this);

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
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
