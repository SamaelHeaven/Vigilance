using System.Runtime.CompilerServices;

namespace Vigilance.Math;

public record struct Transform
    : IWriteImmutableComponent,
        IRemoveImmutableComponent,
        ISkipAddEventComponent,
        ISkipRemoveEventComponent
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Transform Lerp(Transform start, Transform end, float t)
    {
        return new Transform(
            Vector2.Lerp(start.Position, end.Position, t),
            Vector2.Lerp(start.Scale, end.Scale, t),
            float.LerpAngle(start.Rotation, end.Rotation, t),
            Vector2.Lerp(start.PivotPoint, end.PivotPoint, t)
        );
    }

    // ReSharper disable once InconsistentNaming
    public Matrix3x2 ToMatrix3x2()
    {
        var matrix = Matrix3x2.Identity;
        if (!Precision.AreEqual(Rotation, 0))
        {
            if (!Precision.AreEqual(PivotPoint, Vector2.Zero))
                matrix = Matrix3x2.CreateTranslation(PivotPoint.X, PivotPoint.Y) * matrix;
            matrix = Matrix3x2.CreateRotation(Rotation.DegToRad()) * matrix;
            if (!Precision.AreEqual(PivotPoint, Vector2.Zero))
                matrix = Matrix3x2.CreateTranslation(-PivotPoint.X, -PivotPoint.Y) * matrix;
        }

        matrix = Matrix3x2.CreateTranslation(Position.X, Position.Y) * matrix;
        matrix = Matrix3x2.CreateScale(Scale.X.Abs(), Scale.Y.Abs()) * matrix;
        return matrix;
    }

    // ReSharper disable once InconsistentNaming
    public Matrix4x4 ToMatrix4x4()
    {
        return ToMatrix3x2().ToMatrix4x4();
    }
}
