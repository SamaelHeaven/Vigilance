using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

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
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.Transparent;
    public float StrokeWidth { get; set; } = 1;
    public CameraProvider Camera { get; set; } = Core.Camera.Scene;

    public PointEnumerable Points => new(this);

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }

    public readonly struct PointEnumerable : IValueEnumerable<PointEnumerator, Vector2>, IReadOnlyCollection<Vector2>
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

        public int Count => 3;
    }

    public struct PointEnumerator : IValueEnumerator<Vector2>
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
            if (_index >= 3)
                return false;
            _index++;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public Vector2 Current
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
