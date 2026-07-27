using System.Runtime.CompilerServices;
using Box2D.NET;

namespace Vigilance.Physics;

public record struct ShapeProxy
{
    public InlineList<InlineArray8<Vector2>, Vector2> Points { get; set; }
    public float Radius { get; set; }

    internal readonly B2ShapeProxy B2ShapeProxy
    {
        get
        {
            var points = Points;
            for (var i = 0; i < points.Count; i++)
                points[i] = World.PixelsToMeters(points[i]);
            return new B2ShapeProxy
            {
                points = Unsafe.As<InlineList<InlineArray8<Vector2>, Vector2>, B2FixedArray8<B2Vec2>>(ref points),
                count = Points.Count,
                radius = World.PixelsToMeters(Radius),
            };
        }
    }
}
