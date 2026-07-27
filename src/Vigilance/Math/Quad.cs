using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Math;

public struct Quad : ISpanView<Vector2>
{
    public const int Length = 4;
    private Points _points;

    public Quad(Vector2 topLeft, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight)
    {
        TopLeft = topLeft;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
        TopRight = topRight;
    }

    public Quad(in Transform transform)
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

    public Quad(in Box box)
    {
        TopLeft = box.Position;
        BottomLeft = box.Position + Vector2.Down * box.Height;
        BottomRight = box.Position + box.Size;
        TopRight = box.Position + Vector2.Right * box.Width;
    }

    public Vector2 TopLeft
    {
        readonly get => _points[0];
        set => _points[0] = value;
    }

    public Vector2 BottomLeft
    {
        readonly get => _points[1];
        set => _points[1] = value;
    }

    public Vector2 BottomRight
    {
        readonly get => _points[2];
        set => _points[2] = value;
    }

    public Vector2 TopRight
    {
        readonly get => _points[3];
        set => _points[3] = value;
    }

    public readonly ReadOnlySpan<Vector2> AsSpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(in _points[0], Length);
    }

    public ValueEnumerator<FromSpan<Vector2>, Vector2> GetEnumerator()
    {
        return new ValueEnumerator<FromSpan<Vector2>, Vector2>(AsValueEnumerable().Enumerator);
    }

    public readonly ValueEnumerable<FromSpan<Vector2>, Vector2> AsValueEnumerable()
    {
        return AsSpan().AsValueEnumerable();
    }

    public static implicit operator (Vector2 TopLeft, Vector2 BottomLeft, Vector2 BottomRight, Vector2 TopRight)(
        in Quad quad
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

    public static implicit operator ReadOnlySpan<Vector2>(in Quad quad)
    {
        return quad.AsSpan();
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

    public readonly Quad Transform(in Matrix3x2 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public readonly Quad Transform(in Matrix4x4 matrix)
    {
        return new Quad(
            TopLeft.Transform(matrix),
            BottomLeft.Transform(matrix),
            BottomRight.Transform(matrix),
            TopRight.Transform(matrix)
        );
    }

    public readonly Quad Transform(in Quaternion quaternion)
    {
        return new Quad(
            TopLeft.Transform(quaternion),
            BottomLeft.Transform(quaternion),
            BottomRight.Transform(quaternion),
            TopRight.Transform(quaternion)
        );
    }

    public static Quad operator +(in Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft + b, a.BottomLeft + b, a.BottomRight + b, a.TopRight + b);
    }

    public static Quad operator +(in Quad a, in Quad b)
    {
        return new Quad(
            a.TopLeft + b.TopLeft,
            a.BottomLeft + b.BottomLeft,
            a.BottomRight + b.BottomRight,
            a.TopRight + b.TopRight
        );
    }

    public static Quad operator -(in Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft - b, a.BottomLeft - b, a.BottomRight - b, a.TopRight - b);
    }

    public static Quad operator -(in Quad a, in Quad b)
    {
        return new Quad(
            a.TopLeft - b.TopLeft,
            a.BottomLeft - b.BottomLeft,
            a.BottomRight - b.BottomRight,
            a.TopRight - b.TopRight
        );
    }

    public static Quad operator *(in Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft * b, a.BottomLeft * b, a.BottomRight * b, a.TopRight * b);
    }

    public static Quad operator *(in Quad a, in Quad b)
    {
        return new Quad(
            a.TopLeft * b.TopLeft,
            a.BottomLeft * b.BottomLeft,
            a.BottomRight * b.BottomRight,
            a.TopRight * b.TopRight
        );
    }

    public static Quad operator /(in Quad a, Vector2 b)
    {
        return new Quad(a.TopLeft / b, a.BottomLeft / b, a.BottomRight / b, a.TopRight / b);
    }

    public static Quad operator /(in Quad a, in Quad b)
    {
        return new Quad(
            a.TopLeft / b.TopLeft,
            a.BottomLeft / b.BottomLeft,
            a.BottomRight / b.BottomRight,
            a.TopRight / b.TopRight
        );
    }

    public static bool operator ==(in Quad left, in Quad right)
    {
        return left.Equals(in right);
    }

    public static bool operator !=(in Quad left, in Quad right)
    {
        return !left.Equals(in right);
    }

    public readonly bool Equals(in Quad other)
    {
        return TopLeft == other.TopLeft
            && BottomLeft == other.BottomLeft
            && BottomRight == other.BottomRight
            && TopRight == other.TopRight;
    }

    public readonly bool Equals(Quad other)
    {
        return Equals(in other);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Quad other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(TopLeft, BottomLeft, BottomRight, TopRight);
    }

    public override readonly string ToString()
    {
        return ObjectPrinter.Print(this);
    }

    [InlineArray(Length)]
    private struct Points
    {
        private Vector2 _element0;
    }
}
