using System.Numerics;
using System.Runtime.InteropServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Math;

[StructLayout(LayoutKind.Sequential)]
public record struct Quad : IStructEnumerable<Quad.PointEnumerator, Vector2>, IReadOnlyCollection<Vector2>
{
    public Vector2 TopLeft { get; set; }
    public Vector2 BottomLeft { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 TopRight { get; set; }

    public Quad(Vector2 topLeft, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight)
    {
        TopLeft = topLeft;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        TopRight = topRight;
    }

    public Quad(Transform transform)
    {
        var position = transform.Position;
        var size = transform.Scale.Abs();
        var rotation = transform.Rotation;
        var pivotPoint = transform.PivotPoint;
        var topLeft = position - size * 0.5f;
        var bottomLeft = topLeft + Vector2.Down * size;
        var bottomRight = topLeft + size;
        var topRight = topLeft + Vector2.Right * size;
        var rotationPoint = position + pivotPoint;
        TopLeft = topLeft.Rotate(rotation, rotationPoint);
        BottomLeft = bottomLeft.Rotate(rotation, rotationPoint);
        BottomRight = bottomRight.Rotate(rotation, rotationPoint);
        TopRight = topRight.Rotate(rotation, rotationPoint);
    }

    public Quad(Box box)
    {
        TopLeft = box.Position;
        BottomLeft = box.Position + Vector2.Down * box.Height;
        BottomRight = box.Position + box.Size;
        TopRight = box.Position + Vector2.Right * box.Width;
    }

    public static implicit operator (Vector2 TopLeft, Vector2 BottomLeft, Vector2 BottomRight, Vector2 TopRight)(
        Quad quad
    )
    {
        return (quad.TopLeft, quad.BottomLeft, quad.BottomRight, quad.TopRight);
    }

    public static implicit operator Quad(
        (Vector2 TopLeft, Vector2 BottomLeft, Vector2 BottomRight, Vector2 TopRight) quad
    )
    {
        return new Quad(quad.TopLeft, quad.BottomLeft, quad.BottomRight, quad.TopRight);
    }

    public static implicit operator Quad(Box box)
    {
        return new Quad(box);
    }

    public static unsafe implicit operator ReadOnlySpan<Vector2>(Quad quad)
    {
        return new ReadOnlySpan<Vector2>(&quad, quad.Count);
    }

    public readonly void Deconstruct(
        out Vector2 topLeft,
        out Vector2 bottomLeft,
        out Vector2 bottomRight,
        out Vector2 topRight
    )
    {
        topLeft = TopLeft;
        bottomLeft = BottomLeft;
        bottomRight = BottomRight;
        topRight = TopRight;
    }

    public readonly Quad Transform(Matrix3x2 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public readonly Quad Transform(Matrix4x4 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public readonly Quad Transform(Quaternion quaternion)
    {
        return new Quad(
            TopLeft.Transform(quaternion),
            BottomLeft.Transform(quaternion),
            BottomRight.Transform(quaternion),
            TopRight.Transform(quaternion)
        );
    }

    public static Quad operator +(Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft + b, a.BottomLeft + b, a.BottomRight + b, a.TopRight + b);
    }

    public static Quad operator +(Quad a, Quad b)
    {
        return new Quad(
            a.TopLeft + b.TopLeft,
            a.BottomLeft + b.BottomLeft,
            a.BottomRight + b.BottomRight,
            a.TopRight + b.TopRight
        );
    }

    public static Quad operator -(Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft - b, a.BottomLeft - b, a.BottomRight - b, a.TopRight - b);
    }

    public static Quad operator -(Quad a, Quad b)
    {
        return new Quad(
            a.TopLeft - b.TopLeft,
            a.BottomLeft - b.BottomLeft,
            a.BottomRight - b.BottomRight,
            a.TopRight - b.TopRight
        );
    }

    public static Quad operator *(Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft * b, a.BottomLeft * b, a.BottomRight * b, a.TopRight * b);
    }

    public static Quad operator *(Quad a, Quad b)
    {
        return new Quad(
            a.TopLeft * b.TopLeft,
            a.BottomLeft * b.BottomLeft,
            a.BottomRight * b.BottomRight,
            a.TopRight * b.TopRight
        );
    }

    public static Quad operator /(Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft / b, a.BottomLeft / b, a.BottomRight / b, a.TopRight / b);
    }

    public static Quad operator /(Quad a, Quad b)
    {
        return new Quad(
            a.TopLeft / b.TopLeft,
            a.BottomLeft / b.BottomLeft,
            a.BottomRight / b.BottomRight,
            a.TopRight / b.TopRight
        );
    }

    public readonly int Count => 4;

    public readonly PointEnumerator GetEnumerator()
    {
        return new PointEnumerator(this);
    }

    public ValueEnumerable<StructEnumerator<PointEnumerator, Vector2>, Vector2> AsValueEnumerable()
    {
        return new StructEnumerator<PointEnumerator, Vector2>(GetEnumerator());
    }

    public struct PointEnumerator : IStructEnumerator<Vector2>
    {
        private readonly Quad _quad;
        private int _index;

        internal PointEnumerator(Quad quad)
        {
            _quad = quad;
            Reset();
        }

        public bool MoveNext()
        {
            if (_index >= 4)
                return false;
            _index++;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public readonly Vector2 Current =>
            _index switch
            {
                0 => _quad.TopLeft,
                1 => _quad.BottomLeft,
                2 => _quad.BottomRight,
                3 => _quad.TopRight,
                _ => throw new IndexOutOfRangeException(),
            };

        public void Dispose() { }
    }
}
