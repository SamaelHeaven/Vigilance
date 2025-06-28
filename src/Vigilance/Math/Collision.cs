using Vigilance.Core;

namespace Vigilance.Math;

public static class Collision
{
    public static bool CheckBoxes(Box box1, Box box2)
    {
        return box1.X < box2.X + box2.Width
            && box1.X + box1.Width > box2.X
            && box1.Y < box2.Y + box2.Height
            && box1.Y + box1.Height > box2.Y;
    }

    public static bool CheckBoxes(Box box1, Box box2, out Box intersection)
    {
        var result = CheckBoxes(box1, box2);
        intersection = new Box();
        if (!result)
            return result;
        var left = box1.X > box2.X ? box1.X : box2.X;
        var right1 = box1.X + box1.Width;
        var right2 = box2.X + box2.Width;
        var right = right1 < right2 ? right1 : right2;
        var top = box1.Y > box2.Y ? box1.Y : box2.Y;
        var bottom1 = box1.Y + box1.Height;
        var bottom2 = box2.Y + box2.Height;
        var bottom = bottom1 < bottom2 ? bottom1 : bottom2;
        if (!(left < right) || !(top < bottom))
            return result;
        intersection.X = left;
        intersection.Y = top;
        intersection.Width = right - left;
        intersection.Height = bottom - top;
        return result;
    }

    public static bool CheckCircles(Vector2 center1, float radius1, Vector2 center2, float radius2)
    {
        var dx = center2.X - center1.X;
        var dy = center2.Y - center1.Y;
        var distanceSquared = dx * dx + dy * dy;
        var radiusSum = radius1 + radius2;
        return distanceSquared <= radiusSum * radiusSum;
    }

    public static bool CheckCircleBox(Vector2 center, float radius, Box box)
    {
        var boxCenterX = box.X + box.Width / 2.0f;
        var boxCenterY = box.Y + box.Height / 2.0f;
        var dx = (center.X - boxCenterX).Abs();
        var dy = (center.Y - boxCenterY).Abs();
        if (dx > box.Width / 2.0f + radius)
            return false;
        if (dy > box.Height / 2.0f + radius)
            return false;
        if (dx <= box.Width / 2.0f)
            return true;
        if (dy <= box.Height / 2.0f)
            return true;
        var cornerDistanceSq =
            (dx - box.Width / 2.0f) * (dx - box.Width / 2.0f) + (dy - box.Height / 2.0f) * (dy - box.Height / 2.0f);
        return cornerDistanceSq <= radius * radius;
    }

    public static bool CheckCircleLine(Vector2 center, float radius, Vector2 start, Vector2 end)
    {
        var dx = start.X - end.X;
        var dy = start.Y - end.Y;
        if (dx.Abs() + dy.Abs() <= float.Epsilon)
            return CheckCircles(start, 0, center, radius);
        var lengthSq = dx * dx + dy * dy;
        var dotProduct =
            ((center.X - start.X) * (end.X - start.X) + (center.Y - start.Y) * (end.Y - start.Y)) / lengthSq;
        dotProduct = dotProduct switch
        {
            > 1.0f => 1.0f,
            < 0.0f => 0.0f,
            _ => dotProduct,
        };
        var dx2 = start.X - dotProduct * dx - center.X;
        var dy2 = start.Y - dotProduct * dy - center.Y;
        var distanceSq = dx2 * dx2 + dy2 * dy2;
        return distanceSq <= radius * radius;
    }

    public static bool CheckPointBox(Vector2 point, Box box)
    {
        return point.X >= box.X && point.X < box.X + box.Width && point.Y >= box.Y && point.Y < box.Y + box.Height;
    }

    public static bool CheckPointCircle(Vector2 point, Vector2 center, float radius)
    {
        var distanceSquared = (point.X - center.X) * (point.X - center.X) + (point.Y - center.Y) * (point.Y - center.Y);
        return distanceSquared <= radius * radius;
    }

    public static bool CheckPointTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var alpha =
            ((p2.Y - p3.Y) * (point.X - p3.X) + (p3.X - p2.X) * (point.Y - p3.Y))
            / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
        var beta =
            ((p3.Y - p1.Y) * (point.X - p3.X) + (p1.X - p3.X) * (point.Y - p3.Y))
            / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));
        var gamma = 1.0f - alpha - beta;
        return alpha > 0 && beta > 0 && gamma > 0;
    }

    public static bool CheckPointLine(Vector2 point, Vector2 start, Vector2 end, int threshold = 0)
    {
        var dxc = point.X - start.X;
        var dyc = point.Y - start.Y;
        var dxl = end.X - start.X;
        var dyl = end.Y - start.Y;
        var cross = dxc * dyl - dyc * dxl;
        if (!(cross.Abs() < threshold * MathF.Max(dxl.Abs(), dyl.Abs())))
            return false;
        if (dxl.Abs() >= dyl.Abs())
            return dxl > 0 ? start.X <= point.X && point.X <= end.X : end.X <= point.X && point.X <= start.X;
        return dyl > 0 ? start.Y <= point.Y && point.Y <= end.Y : end.Y <= point.Y && point.Y <= start.Y;
    }

    public static bool CheckPointPolygon(Vector2 point, IEnumerable<Vector2> polygon)
    {
        return CheckPointPolygonSpan(point, polygon.AsSpan());
    }

    public static bool CheckPointPolygonSpan(Vector2 point, ReadOnlySpan<Vector2> span)
    {
        var collision = false;
        if (span.Length <= 2)
            return collision;
        for (int i = 0, j = span.Length - 1; i < span.Length; j = i++)
            if (
                span[i].Y > point.Y != span[j].Y > point.Y
                && point.X < (span[j].X - span[i].X) * (point.Y - span[i].Y) / (span[j].Y - span[i].Y) + span[i].X
            )
                collision = !collision;
        return collision;
    }

    public static bool CheckPointQuad(Vector2 point, Quad quad)
    {
        return CheckPointPolygonSpan(point, quad);
    }

    public static bool CheckPolygons(IEnumerable<Vector2> polygon1, IEnumerable<Vector2> polygon2)
    {
        return CheckPolygonsSpan(polygon1.AsSpan(), polygon2.AsSpan());
    }

    public static bool CheckPolygonsSpan(ReadOnlySpan<Vector2> polygon1, ReadOnlySpan<Vector2> polygon2)
    {
        if (polygon1.Length < 3 || polygon2.Length < 3)
            return false;
        return !HasSeparatingAxis(polygon1, polygon2) && !HasSeparatingAxis(polygon2, polygon1);

        bool HasSeparatingAxis(ReadOnlySpan<Vector2> polygonA, ReadOnlySpan<Vector2> polygonB)
        {
            for (var i = 0; i < polygonA.Length; i++)
            {
                var j = (i + 1) % polygonA.Length;
                var edge = polygonA[j] - polygonA[i];
                var axis = new Vector2(-edge.Y, edge.X);
                ProjectPolygon(polygonA, axis, out var minA, out var maxA);
                ProjectPolygon(polygonB, axis, out var minB, out var maxB);
                if (maxA < minB || maxB < minA)
                    return true;
            }

            return false;
        }

        void ProjectPolygon(ReadOnlySpan<Vector2> polygon, Vector2 axis, out float min, out float max)
        {
            var dot = polygon[0].Dot(axis);
            min = max = dot;
            for (var i = 1; i < polygon.Length; i++)
            {
                dot = polygon[i].Dot(axis);
                if (dot < min)
                    min = dot;
                if (dot > max)
                    max = dot;
            }
        }
    }

    public static bool CheckQuads(Quad a, Quad b)
    {
        return CheckPolygonsSpan(a, b);
    }

    public static bool CheckLines(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
    {
        return CheckLines(start1, end1, start2, end2, out _);
    }

    public static bool CheckLines(
        Vector2 start1,
        Vector2 end1,
        Vector2 start2,
        Vector2 end2,
        out Vector2 collisionPoint
    )
    {
        collisionPoint = Vector2.Zero;
        var collision = false;
        var div = (end2.Y - start2.Y) * (end1.X - start1.X) - (end2.X - start2.X) * (end1.Y - start1.Y);
        if (!(div.Abs() >= float.Epsilon))
            return collision;
        collision = true;
        var xi =
            (
                (start2.X - end2.X) * (start1.X * end1.Y - start1.Y * end1.X)
                - (start1.X - end1.X) * (start2.X * end2.Y - start2.Y * end2.X)
            ) / div;
        var yi =
            (
                (start2.Y - end2.Y) * (start1.X * end1.Y - start1.Y * end1.X)
                - (start1.Y - end1.Y) * (start2.X * end2.Y - start2.Y * end2.X)
            ) / div;
        if (
            (
                (start1.X - end1.X).Abs() > float.Epsilon
                && (xi < MathF.Min(start1.X, end1.X) || xi > MathF.Max(start1.X, end1.X))
            )
            || (
                (start2.X - end2.X).Abs() > float.Epsilon
                && (xi < MathF.Min(start2.X, end2.X) || xi > MathF.Max(start2.X, end2.X))
            )
            || (
                (start1.Y - end1.Y).Abs() > float.Epsilon
                && (yi < MathF.Min(start1.Y, end1.Y) || yi > MathF.Max(start1.Y, end1.Y))
            )
            || (
                (start2.Y - end2.Y).Abs() > float.Epsilon
                && (yi < MathF.Min(start2.Y, end2.Y) || yi > MathF.Max(start2.Y, end2.Y))
            )
        )
            collision = false;
        if (collision)
            collisionPoint = new Vector2(xi, yi);
        return collision;
    }
}
