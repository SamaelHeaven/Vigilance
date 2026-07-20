using System.ComponentModel;
using Box2D.NET;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Physics;

public readonly record struct Shape : IShape
{
    private readonly B2ShapeId _id;

    internal Shape(B2ShapeId id)
    {
        _id = id;
    }

    public World World => World.GetWorld(B2Shapes.b2Shape_GetWorld(_id))!;

    public Scene Scene => World.GetScene(B2Shapes.b2Shape_GetWorld(_id))!;

    public Body Body => new(B2Shapes.b2Shape_GetBody(_id));

    public Entity Entity => Body.Entity;

    public ShapeType Type => (ShapeType)B2Shapes.b2Shape_GetType(_id);

    public bool IsSensor => B2Shapes.b2Shape_IsSensor(_id);

    public object? Data
    {
        get => B2Shapes.b2Shape_GetUserData(_id).oValue;
        set => B2Shapes.b2Shape_SetUserData(_id, new B2UserData(value));
    }

    public ShapeFilter Filter
    {
        get => B2Shapes.b2Shape_GetFilter(_id).ToFilter();
        set => B2Shapes.b2Shape_SetFilter(_id, value.B2Filter);
    }

    public float Friction
    {
        get => B2Shapes.b2Shape_GetFriction(_id);
        set => B2Shapes.b2Shape_SetFriction(_id, value);
    }

    public float Restitution
    {
        get => B2Shapes.b2Shape_GetRestitution(_id);
        set => B2Shapes.b2Shape_SetRestitution(_id, value);
    }

    public float Density
    {
        get => B2Shapes.b2Shape_GetDensity(_id);
        set => B2Shapes.b2Shape_SetDensity(_id, value, true);
    }

    public float RollingResistance
    {
        get => B2Shapes.b2Shape_GetSurfaceMaterial(_id).rollingResistance;
        set
        {
            var material = B2Shapes.b2Shape_GetSurfaceMaterial(_id);
            material.rollingResistance = value;
            B2Shapes.b2Shape_SetSurfaceMaterial(_id, material);
        }
    }

    public float TangentSpeed
    {
        get => World.MetersToPixels(B2Shapes.b2Shape_GetSurfaceMaterial(_id).tangentSpeed);
        set
        {
            var material = B2Shapes.b2Shape_GetSurfaceMaterial(_id);
            material.tangentSpeed = World.PixelsToMeters(value);
            B2Shapes.b2Shape_SetSurfaceMaterial(_id, material);
        }
    }

    public CircleShape Circle
    {
        get => B2Shapes.b2Shape_GetCircle(_id).ToCircle();
        set => B2Shapes.b2Shape_SetCircle(_id, value.B2Circle);
    }

    public CapsuleShape Capsule
    {
        get => B2Shapes.b2Shape_GetCapsule(_id).ToCapsule();
        set => B2Shapes.b2Shape_SetCapsule(_id, value.B2Capsule);
    }

    public SegmentShape Segment
    {
        get => B2Shapes.b2Shape_GetSegment(_id).ToSegment();
        set => B2Shapes.b2Shape_SetSegment(_id, value.B2Segment);
    }

    public PolygonShape Polygon
    {
        get => B2Shapes.b2Shape_GetPolygon(_id).ToPolygon();
        set
        {
            var b2Polygon = value.B2Polygon;
            B2Shapes.b2Shape_SetPolygon(_id, ref b2Polygon);
        }
    }

    public ShapeProxy MakeProxy()
    {
        return Type switch
        {
            ShapeType.Circle => Circle.MakeProxy(),
            ShapeType.Capsule => Capsule.MakeProxy(),
            ShapeType.Segment => Segment.MakeProxy(),
            ShapeType.Polygon => Polygon.MakeProxy(),
            ShapeType.ChainSegment or _ => throw new InvalidEnumArgumentException(
                nameof(Type),
                (int)Type,
                typeof(ShapeType)
            ),
        };
    }

    public bool TestPoint(Vector2 point)
    {
        return B2Shapes.b2Shape_TestPoint(_id, World.PixelsToMeters(point).B2Vec2);
    }

    public Vector2 GetClosestPoint(Vector2 target)
    {
        return World.MetersToPixels(
            new Vector2(B2Shapes.b2Shape_GetClosestPoint(_id, World.PixelsToMeters(target).B2Vec2))
        );
    }

    public void Destroy()
    {
        B2Shapes.b2DestroyShape(_id, true);
    }
}
