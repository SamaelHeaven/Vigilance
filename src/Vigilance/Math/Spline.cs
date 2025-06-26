using Raylib_cs.BleedingEdge;

namespace Vigilance.Math;

public static class Spline
{
    public static Vector2 GetLinear(Vector2 start, Vector2 end, float t)
    {
        return Raylib.GetSplinePointLinear(start, end, t);
    }

    public static Vector2 GetBasis(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
    {
        return Raylib.GetSplinePointBasis(p1, p2, p3, p4, t);
    }

    public static Vector2 GetCatmullRom(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
    {
        return Raylib.GetSplinePointCatmullRom(p1, p2, p3, p4, t);
    }

    public static Vector2 GetBezierQuad(Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        return Raylib.GetSplinePointBezierQuad(p1, p2, p3, t);
    }

    public static Vector2 GetBezierCubic(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
    {
        return Raylib.GetSplinePointBezierCubic(p1, p2, p3, p4, t);
    }
}
