namespace Vigilance.Math;

public struct Box(float x, float y, float width, float height)
{
    public float X = x;
    public float Y = y;
    public float Width = width;
    public float Height = height;

    public Box(Vector2 position, Vector2 size)
        : this(position.X, position.Y, size.X, size.Y) { }

    public static Box From(Transform transform)
    {
        var position = transform.Position;
        var size = transform.Scale.Abs();
        var rotation = transform.Rotation;
        var pivotPoint = transform.PivotPoint;
        var topLeft = position - size * 0.5f;
        if (transform.Rotation == 0f)
            return new Box(topLeft, size);
        var topRight = topLeft + Vector2.Right * size;
        var bottomLeft = topLeft + Vector2.Down * size;
        var bottomRight = topLeft + size;
        var rotationPoint = position + pivotPoint;
        var rotatedTopLeft = topLeft.Rotate(rotation, rotationPoint);
        var rotatedTopRight = topRight.Rotate(rotation, rotationPoint);
        var rotatedBottomLeft = bottomLeft.Rotate(rotation, rotationPoint);
        var rotatedBottomRight = bottomRight.Rotate(rotation, rotationPoint);
        var minX = MathF.Min(
            MathF.Min(MathF.Min(rotatedTopLeft.X, rotatedTopRight.X), rotatedBottomLeft.X),
            rotatedBottomRight.X
        );
        var maxX = MathF.Max(
            MathF.Max(MathF.Max(rotatedTopLeft.X, rotatedTopRight.X), rotatedBottomLeft.X),
            rotatedBottomRight.X
        );
        var minY = MathF.Min(
            MathF.Min(MathF.Min(rotatedTopLeft.Y, rotatedTopRight.Y), rotatedBottomLeft.Y),
            rotatedBottomRight.Y
        );
        var maxY = MathF.Max(
            MathF.Max(MathF.Max(rotatedTopLeft.Y, rotatedTopRight.Y), rotatedBottomLeft.Y),
            rotatedBottomRight.Y
        );
        return new Box(minX, minY, maxX - minX, maxY - minY);
    }

    public bool Intersects(Box bounds)
    {
        return X < bounds.X + bounds.Width
            && X + Width > bounds.X
            && Y < bounds.Y + bounds.Height
            && Y + Height > bounds.Y;
    }

    public bool Contains(Vector2 position)
    {
        return X < position.X && X + Width > position.X && Y < position.Y && Y + Height > position.Y;
    }

    public Vector2 Position => new(X, Y);

    public Vector2 Size => new(Width, Height);

    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

    public override bool Equals(object? obj)
    {
        if (obj is Box boundingBox)
            return X.Equals(boundingBox.X)
                && Y.Equals(boundingBox.Y)
                && Width.Equals(boundingBox.Width)
                && Height.Equals(boundingBox.Height);
        return base.Equals(obj);
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
        return $"{{ X: {X}, Y: {Y}, Width: {Width}, Height: {Height} }}";
    }
}
