using System.Numerics;
using System.Runtime.InteropServices;
using Vigilance.Core;

namespace Vigilance.Math;

[StructLayout(LayoutKind.Sequential)]
public struct Quad : IValueEnumerable<Quad.PointEnumerator, Vector2>, IReadOnlyList<Vector2>, IEquatable<Quad>
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

    public void Deconstruct(out Vector2 topLeft, out Vector2 bottomLeft, out Vector2 bottomRight, out Vector2 topRight)
    {
        topLeft = TopLeft;
        bottomLeft = BottomLeft;
        bottomRight = BottomRight;
        topRight = TopRight;
    }

    public Quad Transform(Matrix3x2 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public Quad Transform(Matrix4x4 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public Quad Transform(Quaternion quaternion)
    {
        return new Quad(
            TopLeft.Transform(quaternion),
            BottomLeft.Transform(quaternion),
            BottomRight.Transform(quaternion),
            TopRight.Transform(quaternion)
        );
    }

    public override bool Equals(object? obj)
    {
        return obj is Quad other && Equals(other);
    }

    public bool Equals(Quad other)
    {
        return TopLeft.Equals(other.TopLeft)
            && BottomLeft.Equals(other.BottomLeft)
            && BottomRight.Equals(other.BottomRight)
            && TopRight.Equals(other.TopRight);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TopLeft, BottomLeft, BottomRight, TopRight);
    }

    public static bool operator ==(Quad a, Quad b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Quad a, Quad b)
    {
        return !a.Equals(b);
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

    public override string ToString()
    {
        return $"[{TopLeft}, {BottomLeft}, {BottomRight}, {TopRight}]";
    }

    public int Count => 4;

    public Vector2 this[int index] =>
        index switch
        {
            0 => TopLeft,
            1 => BottomLeft,
            2 => BottomRight,
            3 => TopRight,
            _ => throw new IndexOutOfRangeException(),
        };

    public PointEnumerator GetEnumerator()
    {
        return new PointEnumerator(this);
    }

    public struct PointEnumerator : IValueEnumerator<PointEnumerator, Vector2>
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

        public Vector2 Current
        {
            get
            {
                return _index switch
                {
                    0 => _quad.TopLeft,
                    1 => _quad.BottomLeft,
                    2 => _quad.BottomRight,
                    3 => _quad.TopRight,
                    _ => default,
                };
            }
        }

        public void Dispose() { }
    }
}
