using System.Numerics;

namespace Vigilance.Math;

public struct Box : IEquatable<Box>
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Box(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Box(Vector2 position, Vector2 size)
        : this(position.X, position.Y, size.X, size.Y) { }

    public Box(Transform transform)
        : this(new Quad(transform)) { }

    public Box(Quad quad)
    {
        var (topLeft, bottomLeft, bottomRight, topRight) = quad;
        var minX = topLeft.X.Min(topRight.X).Min(bottomLeft.X).Min(bottomRight.X);
        var maxX = topLeft.X.Max(topRight.X).Max(bottomLeft.X).Max(bottomRight.X);
        var minY = topLeft.Y.Min(topRight.Y).Min(bottomLeft.Y).Min(bottomRight.Y);
        var maxY = topLeft.Y.Max(topRight.Y).Max(bottomLeft.Y).Max(bottomRight.Y);
        X = minX;
        Y = minY;
        Width = maxX - minX;
        Height = maxY - minY;
    }

    public static implicit operator (float X, float Y, float Width, float Height)(Box box)
    {
        return (box.X, box.Y, box.Width, box.Height);
    }

    public static implicit operator Box((float X, float Y, float Width, float Height) box)
    {
        return new Box(box.X, box.Y, box.Width, box.Height);
    }

    public static implicit operator (Vector2 Position, Vector2 Size)(Box box)
    {
        return (box.Position, box.Size);
    }

    public static implicit operator Box((Vector2 Position, Vector2 Size) box)
    {
        return new Box(box.Position, box.Size);
    }

    public Quad Transform(Matrix3x2 matrix)
    {
        var topLeft = new Vector2(X, Y).Transform(matrix);
        var bottomLeft = new Vector2(X, Y + Height).Transform(matrix);
        var bottomRight = new Vector2(X + Width, Y + Height).Transform(matrix);
        var topRight = new Vector2(X + Width, Y).Transform(matrix);
        return new Quad(topLeft, bottomLeft, bottomRight, topRight);
    }

    public Quad Transform(Matrix4x4 matrix)
    {
        var topLeft = new Vector2(X, Y).Transform(matrix);
        var bottomLeft = new Vector2(X, Y + Height).Transform(matrix);
        var bottomRight = new Vector2(X + Width, Y + Height).Transform(matrix);
        var topRight = new Vector2(X + Width, Y).Transform(matrix);
        return new Quad(topLeft, bottomLeft, bottomRight, topRight);
    }

    public Quad Transform(Quaternion quaternion)
    {
        var topLeft = new Vector2(X, Y).Transform(quaternion);
        var bottomLeft = new Vector2(X, Y + Height).Transform(quaternion);
        var bottomRight = new Vector2(X + Width, Y + Height).Transform(quaternion);
        var topRight = new Vector2(X + Width, Y).Transform(quaternion);
        return new Quad(topLeft, bottomLeft, bottomRight, topRight);
    }

    public void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    public void Deconstruct(out Vector2 position, out Vector2 size)
    {
        position = new Vector2(X, Y);
        size = new Vector2(Width, Height);
    }

    public Vector2 Position
    {
        readonly get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public Vector2 Size
    {
        readonly get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public readonly Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

    public override bool Equals(object? obj)
    {
        return obj is Box other && Equals(other);
    }

    public bool Equals(Box other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public static bool operator ==(Box a, Box b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Box a, Box b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public override string ToString()
    {
        return $"[X={X}, Y={Y}, W={Width}, H={Height}]";
    }
}
