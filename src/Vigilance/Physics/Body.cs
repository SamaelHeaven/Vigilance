using System.Buffers;
using System.Runtime.CompilerServices;
using Box2D.NET;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Physics;

public readonly record struct Body : IInterpolated, ISkipSetEventIfEqualComponent
{
    private readonly B2BodyId _id;

    internal Body(B2BodyId id)
    {
        _id = id;
    }

    public World World => new(B2Bodies.b2Body_GetWorld(_id));

    public Scene Scene => World.Scene;

    public Entity Entity
    {
        get => new(B2Bodies.b2Body_GetUserData(_id).ulValue, Scene);
        set => B2Bodies.b2Body_SetUserData(_id, new B2UserData(value.Id));
    }

    public BodyType Type
    {
        get => (BodyType)B2Bodies.b2Body_GetType(_id);
        set => B2Bodies.b2Body_SetType(_id, (B2BodyType)value);
    }

    public (Vector2 Position, float Rotation) Transform
    {
        get
        {
            var transform = B2Bodies.b2Body_GetTransform(_id);
            return (World.MetersToPixels(new Vector2(transform.p)), transform.q.ToDegrees());
        }
        set
        {
            var transform = Transform;
            if (
                Precision.AreEqual(value.Position, transform.Position)
                && Precision.AreEqual(value.Rotation, transform.Rotation)
            )
                return;
            B2Bodies.b2Body_SetTransform(
                _id,
                World.PixelsToMeters(value.Position).B2Vec2,
                B2Rot.FromDegrees(value.Rotation)
            );
        }
    }

    public Vector2 Position
    {
        get => World.MetersToPixels(new Vector2(B2Bodies.b2Body_GetPosition(_id)));
        set
        {
            var position = Position;
            if (Precision.AreEqual(value, position))
                return;
            B2Bodies.b2Body_SetTransform(_id, World.PixelsToMeters(value).B2Vec2, B2Bodies.b2Body_GetRotation(_id));
        }
    }

    public float Rotation
    {
        get => B2Bodies.b2Body_GetRotation(_id).ToDegrees();
        set
        {
            var rotation = Rotation;
            if (Precision.AreEqual(value, rotation))
                return;
            B2Bodies.b2Body_SetTransform(_id, B2Bodies.b2Body_GetPosition(_id), B2Rot.FromDegrees(value));
        }
    }

    public Vector2 LinearVelocity
    {
        get => World.MetersToPixels(new Vector2(B2Bodies.b2Body_GetLinearVelocity(_id)));
        set => B2Bodies.b2Body_SetLinearVelocity(_id, World.PixelsToMeters(value).B2Vec2);
    }

    public float AngularVelocity
    {
        get => B2Bodies.b2Body_GetAngularVelocity(_id);
        set => B2Bodies.b2Body_SetAngularVelocity(_id, value);
    }

    public float LinearDamping
    {
        get => B2Bodies.b2Body_GetLinearDamping(_id);
        set => B2Bodies.b2Body_SetLinearDamping(_id, value);
    }

    public float AngularDamping
    {
        get => B2Bodies.b2Body_GetAngularDamping(_id);
        set => B2Bodies.b2Body_SetAngularDamping(_id, value);
    }

    public float GravityScale
    {
        get => B2Bodies.b2Body_GetGravityScale(_id);
        set => B2Bodies.b2Body_SetGravityScale(_id, value);
    }

    public float SleepThreshold
    {
        get => World.MetersToPixels(B2Bodies.b2Body_GetSleepThreshold(_id));
        set => B2Bodies.b2Body_SetSleepThreshold(_id, World.PixelsToMeters(value));
    }

    public bool IsSleepEnabled
    {
        get => B2Bodies.b2Body_IsSleepEnabled(_id);
        set => B2Bodies.b2Body_EnableSleep(_id, value);
    }

    public bool IsAwake
    {
        get => B2Bodies.b2Body_IsAwake(_id);
        set => B2Bodies.b2Body_SetAwake(_id, value);
    }

    public bool IsEnabled
    {
        get => B2Bodies.b2Body_IsEnabled(_id);
        set
        {
            if (value)
                B2Bodies.b2Body_Enable(_id);
            else
                B2Bodies.b2Body_Disable(_id);
        }
    }

    public bool IsBullet
    {
        get => B2Bodies.b2Body_IsBullet(_id);
        set => B2Bodies.b2Body_SetBullet(_id, value);
    }

    public bool LockLinearX
    {
        get => B2Bodies.b2Body_GetMotionLocks(_id).linearX;
        set
        {
            var locks = B2Bodies.b2Body_GetMotionLocks(_id);
            locks.linearX = value;
            B2Bodies.b2Body_SetMotionLocks(_id, locks);
        }
    }

    public bool LockLinearY
    {
        get => B2Bodies.b2Body_GetMotionLocks(_id).linearY;
        set
        {
            var locks = B2Bodies.b2Body_GetMotionLocks(_id);
            locks.linearY = value;
            B2Bodies.b2Body_SetMotionLocks(_id, locks);
        }
    }

    public bool LockAngularZ
    {
        get => B2Bodies.b2Body_GetMotionLocks(_id).angularZ;
        set
        {
            var locks = B2Bodies.b2Body_GetMotionLocks(_id);
            locks.angularZ = value;
            B2Bodies.b2Body_SetMotionLocks(_id, locks);
        }
    }

    public ShapeEnumerable Shapes => new(_id);

    public float Mass => B2Bodies.b2Body_GetMass(_id);

    public float RotationalInertia => B2Bodies.b2Body_GetRotationalInertia(_id);

    public Vector2 LocalCenterOfMass => World.MetersToPixels(new Vector2(B2Bodies.b2Body_GetLocalCenterOfMass(_id)));

    public Vector2 WorldCenterOfMass => World.MetersToPixels(new Vector2(B2Bodies.b2Body_GetWorldCenterOfMass(_id)));

    public void ApplyForce(Vector2 force, Vector2 point, bool wake = true)
    {
        B2Bodies.b2Body_ApplyForce(_id, World.PixelsToMeters(force).B2Vec2, World.PixelsToMeters(point).B2Vec2, wake);
    }

    public void ApplyForce(Vector2 force, bool wake = true)
    {
        B2Bodies.b2Body_ApplyForceToCenter(_id, World.PixelsToMeters(force).B2Vec2, wake);
    }

    public void ApplyTorque(float torque, bool wake = true)
    {
        B2Bodies.b2Body_ApplyTorque(_id, torque, wake);
    }

    public void ApplyLinearImpulse(Vector2 impulse, Vector2 point, bool wake = true)
    {
        B2Bodies.b2Body_ApplyLinearImpulse(
            _id,
            World.PixelsToMeters(impulse).B2Vec2,
            World.PixelsToMeters(point).B2Vec2,
            wake
        );
    }

    public void ApplyLinearImpulse(Vector2 impulse, bool wake = true)
    {
        B2Bodies.b2Body_ApplyLinearImpulseToCenter(_id, World.PixelsToMeters(impulse).B2Vec2, wake);
    }

    public void ApplyAngularImpulse(float impulse, bool wake = true)
    {
        B2Bodies.b2Body_ApplyAngularImpulse(_id, impulse, wake);
    }

    public Vector2 GetLocalPoint(Vector2 worldPoint)
    {
        return World.MetersToPixels(
            new Vector2(B2Bodies.b2Body_GetLocalPoint(_id, World.PixelsToMeters(worldPoint).B2Vec2))
        );
    }

    public Vector2 GetWorldPoint(Vector2 localPoint)
    {
        return World.MetersToPixels(
            new Vector2(B2Bodies.b2Body_GetWorldPoint(_id, World.PixelsToMeters(localPoint).B2Vec2))
        );
    }

    public Vector2 GetLocalVector(Vector2 worldVector)
    {
        return World.MetersToPixels(
            new Vector2(B2Bodies.b2Body_GetLocalVector(_id, World.PixelsToMeters(worldVector).B2Vec2))
        );
    }

    public Vector2 GetWorldVector(Vector2 localVector)
    {
        return World.MetersToPixels(
            new Vector2(B2Bodies.b2Body_GetWorldVector(_id, World.PixelsToMeters(localVector).B2Vec2))
        );
    }

    public void Destroy()
    {
        B2Bodies.b2DestroyBody(_id);
    }

    public Shape CreateShape(in ShapeDef def, in PolygonShape polygon)
    {
        var b2Polygon = polygon.B2Polygon;
        return new Shape(B2Shapes.b2CreatePolygonShape(_id, ToB2ShapeDef(def), b2Polygon));
    }

    public Shape CreateShape(in ShapeDef def, in CircleShape circleShape)
    {
        return new Shape(B2Shapes.b2CreateCircleShape(_id, ToB2ShapeDef(def), circleShape.B2Circle));
    }

    public Shape CreateShape(in ShapeDef def, in CapsuleShape capsuleShape)
    {
        return new Shape(B2Shapes.b2CreateCapsuleShape(_id, ToB2ShapeDef(def), capsuleShape.B2Capsule));
    }

    public Shape CreateShape(in ShapeDef def, in SegmentShape segmentShape)
    {
        return new Shape(B2Shapes.b2CreateSegmentShape(_id, ToB2ShapeDef(def), segmentShape.B2Segment));
    }

    private static B2ShapeDef ToB2ShapeDef(in ShapeDef def)
    {
        var b2Def = B2Types.b2DefaultShapeDef();
        b2Def.material.friction = def.Friction;
        b2Def.material.restitution = def.Restitution;
        b2Def.material.rollingResistance = def.RollingResistance;
        b2Def.material.tangentSpeed = World.PixelsToMeters(def.TangentSpeed);
        b2Def.density = def.Density;
        b2Def.isSensor = def.IsSensor;
        b2Def.filter = def.Filter.B2Filter;
        b2Def.userData = new B2UserData(def.Data);
        b2Def.enableContactEvents = def.EnableContactEvents;
        b2Def.enableSensorEvents = def.EnableSensorEvents;
        b2Def.enableHitEvents = def.EnableHitEvents;
        return b2Def;
    }

    public readonly struct ShapeEnumerable : IStructEnumerable<ShapeEnumerable.Enumerator, Shape>
    {
        private readonly B2BodyId _bodyId;

        internal ShapeEnumerable(B2BodyId bodyId)
        {
            _bodyId = bodyId;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_bodyId);
        }

        public ValueEnumerable<Enumerator, Shape> AsValueEnumerable()
        {
            return new ValueEnumerable<Enumerator, Shape>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<Enumerator, Shape>, Shape> IStructEnumerable<
            Enumerator,
            Shape
        >.AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, Shape>(GetEnumerator());
        }

        public struct Enumerator : IStructEnumerator<Shape>, IValueEnumerator<Shape>
        {
            private readonly B2BodyId _bodyId;
            private B2ShapeId[]? _shapes;
            private int _count;
            private int _index;

            internal Enumerator(B2BodyId bodyId)
            {
                _bodyId = bodyId;
                _shapes = null;
                _count = 0;
                _index = -1;
            }

            public Shape Current => new(_shapes![_index]);

            private void Initialize()
            {
                _count = B2Bodies.b2Body_GetShapeCount(_bodyId);
                _shapes = ArrayPool<B2ShapeId>.Shared.Rent(_count);
                B2Bodies.b2Body_GetShapes(_bodyId, _shapes, _count);
            }

            public bool MoveNext()
            {
                if (_shapes is null)
                    Initialize();
                return ++_index < _count;
            }

            public bool TryGetNext(out Shape current)
            {
                if (MoveNext())
                {
                    current = Current;
                    return true;
                }

                Unsafe.SkipInit(out current);
                return false;
            }

            public void Reset()
            {
                _index = -1;
            }

            public bool TryGetNonEnumeratedCount(out int count)
            {
                count = _shapes is null ? B2Bodies.b2Body_GetShapeCount(_bodyId) : _count;
                return true;
            }

            public bool TryGetSpan(out ReadOnlySpan<Shape> span)
            {
                span = default;
                return false;
            }

            public bool TryCopyTo(scoped Span<Shape> destination, Index offset)
            {
                return false;
            }

            public void Dispose()
            {
                if (_shapes is null)
                    return;
                ArrayPool<B2ShapeId>.Shared.Return(_shapes);
                _shapes = null;
            }
        }
    }
}
