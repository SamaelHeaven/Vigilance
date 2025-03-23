namespace Vigilance.Math;

public struct Transform
{
    public Vector2 Position = Vector2.Zero;
    public Vector2 Scale = Vector2.One;
    public float Rotation = 0;
    public Vector2 PivotPoint = Vector2.Zero;

    public Transform() { }

    public override string ToString()
    {
        return $"{{\n  Position: {Position}\n  Scale: {Scale}\n  Rotation: {Rotation}\n  PivotPoint: {PivotPoint}\n}}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Transform t
            && Position.Equals(t.Position)
            && Scale.Equals(t.Scale)
            && Rotation.Equals(t.Rotation)
            && PivotPoint.Equals(t.PivotPoint);
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
