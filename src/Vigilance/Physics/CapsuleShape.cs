using Box2D.NET;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct CapsuleShape : IShape
{
    public Vector2 Center1 { get; set; }
    public Vector2 Center2 { get; set; }
    public float Radius { get; set; }

    internal readonly B2Capsule B2Capsule =>
        new()
        {
            center1 = World.PixelsToMeters(Center1).B2Vec2,
            center2 = World.PixelsToMeters(Center2).B2Vec2,
            radius = World.PixelsToMeters(Radius),
        };

    public readonly ShapeProxy MakeProxy()
    {
        return new ShapeProxy { Points = [Center1, Center2], Radius = Radius };
    }

    public static CapsuleShape Make(Vector2 center1, Vector2 center2, float radius)
    {
        return new CapsuleShape
        {
            Center1 = center1,
            Center2 = center2,
            Radius = radius,
        };
    }
}
