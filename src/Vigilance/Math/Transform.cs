namespace Vigilance.Math;

public record struct Transform
{
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

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;

    public static Transform operator +(Transform a, in Transform b)
    {
        a.Position += b.Position;
        a.Scale *= b.Scale;
        a.Rotation += b.Rotation;
        a.PivotPoint += b.PivotPoint;
        return a;
    }

    public static Transform operator -(Transform a, in Transform b)
    {
        a.Position -= b.Position;
        a.Scale /= b.Scale;
        a.Rotation -= b.Rotation;
        a.PivotPoint -= b.PivotPoint;
        return a;
    }
}
