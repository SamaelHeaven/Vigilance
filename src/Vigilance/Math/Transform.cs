namespace Vigilance.Math;

public struct Transform : IEquatable<Transform>
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;

    public Transform() { }

    public Transform(Vector2 position)
    {
        Position = position;
    }

    public Transform(Vector2 position, Vector2 scale)
    {
        Position = position;
        Scale = scale;
    }

    public Transform(Vector2 position, Vector2 scale, float rotation)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }

    public Transform(Vector2 position, Vector2 scale, float rotation, Vector2 pivotPoint)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
        PivotPoint = pivotPoint;
    }

    public override bool Equals(object? obj)
    {
        return obj is Transform t && Equals(t);
    }

    public bool Equals(Transform other)
    {
        return Position.Equals(other.Position)
            && Scale.Equals(other.Scale)
            && Rotation.Equals(other.Rotation)
            && PivotPoint.Equals(other.PivotPoint);
    }

    public static bool operator ==(Transform a, Transform b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Transform a, Transform b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Scale, Rotation, PivotPoint);
    }

    public static Transform operator +(Transform a, Transform b)
    {
        a.Position += b.Position;
        a.Scale *= b.Scale;
        a.Rotation += b.Rotation;
        a.PivotPoint += b.PivotPoint;
        return a;
    }

    public static Transform operator -(Transform a, Transform b)
    {
        a.Position -= b.Position;
        a.Scale /= b.Scale;
        a.Rotation -= b.Rotation;
        a.PivotPoint -= b.PivotPoint;
        return a;
    }
}
