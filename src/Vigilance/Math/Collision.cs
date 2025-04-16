using Raylib_cs;

namespace Vigilance.Math;

public static class Collision
{
    public static bool CheckBoxes(Box box1, Box box2)
    {
        return Raylib.CheckCollisionRecs(
            new Rectangle(box1.Position, box1.Size),
            new Rectangle(box2.Position, box2.Size)
        );
    }

    public static bool CheckBoxes(Box box1, Box box2, out Box intersection)
    {
        var rec1 = new Rectangle(box1.Position, box1.Size);
        var rec2 = new Rectangle(box2.Position, box2.Size);
        var result = Raylib.CheckCollisionRecs(rec1, rec2);
        intersection = new Box();
        if (!result)
            return result;
        var rec = Raylib.GetCollisionRec(rec1, rec2);
        intersection = new Box(rec.Position, rec.Size);
        return result;
    }

    public static bool CheckCircles(Vector2 center1, float radius1, Vector2 center2, float radius2)
    {
        return Raylib.CheckCollisionCircles(center1, radius1, center2, radius2);
    }

    public static bool CheckCircleBox(Vector2 center, float radius, Box box)
    {
        return Raylib.CheckCollisionCircleRec(center, radius, new Rectangle(box.Position, box.Size));
    }

    public static bool CheckCircleLine(Vector2 center, float radius, Vector2 start, Vector2 end)
    {
        return Raylib.CheckCollisionCircleLine(center, radius, start, end);
    }

    public static bool CheckPointBox(Vector2 point, Box box)
    {
        return Raylib.CheckCollisionPointRec(point, new Rectangle(box.Position, box.Size));
    }

    public static bool CheckPointCircle(Vector2 point, Vector2 center, float radius)
    {
        return Raylib.CheckCollisionPointCircle(point, center, radius);
    }

    public static bool CheckPointTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return Raylib.CheckCollisionPointTriangle(point, p1, p2, p3);
    }

    public static bool CheckPointLine(Vector2 point, Vector2 start, Vector2 end, int threshold = 0)
    {
        return Raylib.CheckCollisionPointLine(point, start, end, threshold);
    }

    public static unsafe bool CheckPointPolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
    {
        fixed (Vector2* polygonBuffer = polygon as Vector2[] ?? polygon.ToArray())
        {
            return Raylib.CheckCollisionPointPoly(point, (System.Numerics.Vector2*)polygonBuffer, polygon.Count);
        }
    }

    public static bool CheckLines(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
    {
        return CheckLines(start1, end1, start2, end2, out _);
    }

    public static unsafe bool CheckLines(
        Vector2 start1,
        Vector2 end1,
        Vector2 start2,
        Vector2 end2,
        out Vector2 collisionPoint
    )
    {
        fixed (Vector2* collisionPointBuffer = &collisionPoint)
        {
            return Raylib.CheckCollisionLines(
                start1,
                end1,
                start2,
                end2,
                (System.Numerics.Vector2*)collisionPointBuffer
            );
        }
    }
}
