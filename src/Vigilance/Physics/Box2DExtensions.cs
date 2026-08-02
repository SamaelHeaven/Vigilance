using System.Runtime.CompilerServices;
using Box2D.NET;

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
            var normals = Unsafe.As<B2FixedArray8<B2Vec2>, InlineArray8<Vector2>>(ref Unsafe.AsRef(in polygon.normals));
            return new PolygonShape
            {
                Vertices = new InlineList<InlineArray8<Vector2>, Vector2>(vertices, polygon.count),
                Normals = new InlineList<InlineArray8<Vector2>, Vector2>(normals, polygon.count),
                Centroid = World.MetersToPixels(new Vector2(polygon.centroid)),
                Radius = World.MetersToPixels(polygon.radius),
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
                Mask = (ShapeFilterCategory)filter.maskBits,
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

    extension(B2HexColor hexColor)
    {
        public Color ToColor()
        {
            var value = (uint)hexColor;
            return new Color((byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff), (byte)(value & 0xff));
        }
    }

    extension(in B2Transform transform)
    {
        public Vector2 Transform(B2Vec2 vertex)
        {
            var x = transform.q.c * vertex.X - transform.q.s * vertex.Y + transform.p.X;
            var y = transform.q.s * vertex.X + transform.q.c * vertex.Y + transform.p.Y;
            return World.MetersToPixels(new Vector2(x, y));
        }
    }

    extension(in B2UserData userData)
    {
        public static B2UserData FromPrimitive(in Primitive primitive)
        {
            return primitive.Type switch
            {
                PrimitiveType.Long => new B2UserData(primitive.Long),
                PrimitiveType.ULong => new B2UserData(primitive.ULong),
                PrimitiveType.Double => new B2UserData(primitive.Double),
                PrimitiveType.Object => new B2UserData(primitive.Object),
                PrimitiveType.Bool => new B2UserData(primitive.Bool ? 1 : 0),
                PrimitiveType.SByte => new B2UserData(primitive.SByte),
                PrimitiveType.Byte => new B2UserData(primitive.Byte),
                PrimitiveType.Short => new B2UserData(primitive.Short),
                PrimitiveType.UShort => new B2UserData(primitive.UShort),
                PrimitiveType.Int => new B2UserData(primitive.Int),
                PrimitiveType.UInt => new B2UserData(primitive.UInt),
                PrimitiveType.Float => new B2UserData(primitive.Float),
                PrimitiveType.IntPtr => new B2UserData(primitive.IntPtr),
                PrimitiveType.UIntPtr => new B2UserData(primitive.UIntPtr),
                _ => B2UserData.Empty,
            };
        }

        public Primitive ToPrimitive()
        {
            var type = (PrimitiveType)userData.type;
            return type switch
            {
                PrimitiveType.Long => userData.iValue,
                PrimitiveType.ULong => userData.ulValue,
                PrimitiveType.Double => userData.dValue,
                PrimitiveType.Object => Primitive.From(userData.oValue),
                PrimitiveType.Bool => userData.iValue != 0,
                PrimitiveType.SByte => (sbyte)userData.iValue,
                PrimitiveType.Byte => (byte)userData.iValue,
                PrimitiveType.Short => (short)userData.iValue,
                PrimitiveType.UShort => (ushort)userData.iValue,
                PrimitiveType.Int => (int)userData.iValue,
                PrimitiveType.UInt => (uint)userData.iValue,
                PrimitiveType.Float => (float)userData.dValue,
                PrimitiveType.IntPtr => (nint)userData.iValue,
                PrimitiveType.UIntPtr => (nuint)userData.ulValue,
                _ => new Primitive { Type = type },
            };
        }
    }
}
