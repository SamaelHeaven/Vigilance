using Box2D.NET;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct CircleShape : IShape
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }

    internal readonly B2Circle B2Circle =>
        new() { center = World.PixelsToMeters(Center).B2Vec2, radius = World.PixelsToMeters(Radius) };

    public readonly ShapeProxy MakeProxy()
    {
        return new ShapeProxy { Points = [Center], Radius = Radius };
    }

    public static implicit operator ShapeProxy(in CircleShape shape)
    {
        return shape.MakeProxy();
    }

    public static CircleShape Make(float radius)
    {
        return Make(Vector2.Zero, radius);
    }

    public static CircleShape Make(Vector2 center, float radius)
    {
        return new CircleShape { Center = center, Radius = radius };
    }
}
