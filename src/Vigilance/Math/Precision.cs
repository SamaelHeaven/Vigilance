namespace Vigilance.Math;

public static class Precision
{
    public const float DefaultFloatEpsilon = 1e-7f;
    public const double DefaultDoubleEpsilon = 1e-15;

    public static bool AreEqual(float a, float b, float epsilon = DefaultFloatEpsilon)
    {
        return MathF.Abs(a - b) <= epsilon;
    }

    public static bool AreEqual(double a, double b, double epsilon = DefaultDoubleEpsilon)
    {
        return System.Math.Abs(a - b) <= epsilon;
    }

    public static bool AreEqual(Vector2 a, Vector2 b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.X, b.X, epsilon) && AreEqual(a.Y, b.Y, epsilon);
    }

    public static bool AreEqual(Transform a, Transform b, float epsilon = DefaultFloatEpsilon)
    {
        return AreEqual(a.Position, b.Position, epsilon)
            && AreEqual(a.Scale, b.Scale, epsilon)
            && AreEqual(a.Rotation, b.Rotation, epsilon)
            && AreEqual(a.PivotPoint, b.PivotPoint, epsilon);
    }
}
