using System.Runtime.CompilerServices;
using Box2D.NET;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct CircleShape
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }

    internal readonly B2Circle B2Circle =>
        new() { center = World.PixelsToMeters(Center).B2Vec2, radius = World.PixelsToMeters(Radius) };

    public static CircleShape Make(float radius)
    {
        return Make(Vector2.Zero, radius);
    }

    public static CircleShape Make(Vector2 center, float radius)
    {
        return new CircleShape { Center = center, Radius = radius };
    }

    internal readonly B2ShapeProxy MakeProxy()
    {
        var proxy = new B2ShapeProxy { count = 1, radius = World.PixelsToMeters(Radius) };
        ref var pts = ref Unsafe.As<B2FixedArray8<B2Vec2>, InlineArray8<Vector2>>(ref proxy.points);
        pts[0] = World.PixelsToMeters(Center);
        return proxy;
    }
}
