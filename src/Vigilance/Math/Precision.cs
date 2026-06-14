using System.Runtime.CompilerServices;

namespace Vigilance.Math;

public static class Precision
{
    public const float DefaultFloatEpsilon = 1e-7f;
    public const double DefaultDoubleEpsilon = 1e-15;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(float a, float b, float epsilon = DefaultFloatEpsilon)
    {
        if (float.IsNaN(a) && float.IsNaN(b))
            return true;
        return (a - b).Abs() <= epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(float? a, float? b, float epsilon = DefaultFloatEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(double a, double b, double epsilon = DefaultDoubleEpsilon)
    {
        return (a - b).Abs() <= epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(double? a, double? b, double epsilon = DefaultDoubleEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(Vector2 a, Vector2 b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.X, b.X, epsilon) && AreEqual(a.Y, b.Y, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Vector2? a, in Vector2? b, float epsilon = DefaultFloatEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Box a, in Box b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.Position, b.Position, epsilon) && AreEqual(a.Size, b.Size, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Box? a, in Box? b, float epsilon = DefaultFloatEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Transform a, in Transform b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.Position, b.Position, epsilon)
            && AreEqual(a.Scale, b.Scale, epsilon)
            && AreEqual(a.Rotation, b.Rotation, epsilon)
            && AreEqual(a.PivotPoint, b.PivotPoint, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Transform? a, in Transform? b, float epsilon = DefaultFloatEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Quad a, in Quad b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.TopLeft, b.TopLeft, epsilon)
            && AreEqual(a.BottomLeft, b.BottomLeft, epsilon)
            && AreEqual(a.BottomRight, b.BottomRight, epsilon)
            && AreEqual(a.TopRight, b.TopRight, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool AreEqual(in Quad? a, in Quad? b, float epsilon = DefaultFloatEpsilon)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return AreEqual(a.Value, b.Value, epsilon);
    }
}
