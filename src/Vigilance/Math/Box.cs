namespace Vigilance.Math;

public struct Box
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
        var minX = MathF.Min(MathF.Min(MathF.Min(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
        var maxX = MathF.Max(MathF.Max(MathF.Max(topLeft.X, topRight.X), bottomLeft.X), bottomRight.X);
        var minY = MathF.Min(MathF.Min(MathF.Min(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);
        var maxY = MathF.Max(MathF.Max(MathF.Max(topLeft.Y, topRight.Y), bottomLeft.Y), bottomRight.Y);
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

    public readonly Vector2 Position => new(X, Y);

    public readonly Vector2 Size => new(Width, Height);

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
