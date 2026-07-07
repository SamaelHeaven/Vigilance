using System.Runtime.CompilerServices;
using Box2D.NET;
using Vigilance.Math;

namespace Vigilance.Physics;

public static class Box2DExtensions
{
    extension(B2Rot rot)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ToDegrees()
        {
            return MathF.Atan2(rot.s, rot.c).RadToDeg();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static B2Rot FromDegrees(float degrees)
        {
            var radians = degrees.DegToRad();
            return new B2Rot(MathF.Cos(radians), MathF.Sin(radians));
        }
    }

    extension(in B2Polygon polygon)
    {
        public PolygonShape ToPolygon()
        {
            var vertices = Unsafe.As<B2FixedArray8<B2Vec2>, InlineArray8<Vector2>>(
                ref Unsafe.AsRef(in polygon.vertices)
            );
            for (var i = 0; i < polygon.count; i++)
                vertices[i] = World.MetersToPixels(vertices[i]);
            return new PolygonShape
            {
                Vertices = vertices,
                Normals = Unsafe.As<B2FixedArray8<B2Vec2>, InlineArray8<Vector2>>(ref Unsafe.AsRef(in polygon.normals)),
                Centroid = World.MetersToPixels(new Vector2(polygon.centroid)),
                Radius = World.MetersToPixels(polygon.radius),
                Count = polygon.count,
            };
        }
    }

    extension(in B2Filter filter)
    {
        public ShapeFilter ToFilter()
        {
            return new ShapeFilter
            {
                Category = (ShapeFilterCategory)filter.categoryBits,
                Mask = (ShapeFilterMask)filter.maskBits,
                GroupIndex = filter.groupIndex,
            };
        }
    }

    extension(in B2Circle circle)
    {
        public CircleShape ToCircle()
        {
            return new CircleShape
            {
                Center = World.MetersToPixels(new Vector2(circle.center)),
                Radius = World.MetersToPixels(circle.radius),
            };
        }
    }

    extension(in B2Capsule capsule)
    {
        public CapsuleShape ToCapsule()
        {
            return new CapsuleShape
            {
                Center1 = World.MetersToPixels(new Vector2(capsule.center1)),
                Center2 = World.MetersToPixels(new Vector2(capsule.center2)),
                Radius = World.MetersToPixels(capsule.radius),
            };
        }
    }

    extension(in B2Segment segment)
    {
        public SegmentShape ToSegment()
        {
            return new SegmentShape
            {
                Point1 = World.MetersToPixels(new Vector2(segment.point1)),
                Point2 = World.MetersToPixels(new Vector2(segment.point2)),
            };
        }
    }

    extension(in ShapeFilter? filter)
    {
        internal B2QueryFilter ToB2QueryFilter()
        {
            return filter is not { } f
                ? B2Types.b2DefaultQueryFilter()
                : new B2QueryFilter { categoryBits = (ulong)f.Category, maskBits = (ulong)f.Mask };
        }
    }
}
