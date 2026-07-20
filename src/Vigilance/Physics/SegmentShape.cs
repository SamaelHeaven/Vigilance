using Box2D.NET;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct SegmentShape : IShape
{
    public Vector2 Point1 { get; set; }
    public Vector2 Point2 { get; set; }

    internal readonly B2Segment B2Segment =>
        new() { point1 = World.PixelsToMeters(Point1).B2Vec2, point2 = World.PixelsToMeters(Point2).B2Vec2 };

    public readonly ShapeProxy MakeProxy()
    {
        return new ShapeProxy { Points = [Point1, Point2] };
    }

    public static SegmentShape Make(Vector2 point1, Vector2 point2)
    {
        return new SegmentShape { Point1 = point1, Point2 = point2 };
    }
}
