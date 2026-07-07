using System.Runtime.CompilerServices;
using Box2D.NET;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Physics;

public readonly record struct World : IDisposable
{
    public const float PixelsPerMeter = 50f;
    public const float PixelsToMeter = 1f / PixelsPerMeter;
    private static WorldConfig _config = new();
    private readonly B2WorldId _id;

    public World()
    {
        var def = B2Types.b2DefaultWorldDef();
        def.gravity = PixelsToMeters(DefaultGravity).B2Vec2;
        _id = B2Worlds.b2CreateWorld(def);
        var data = new WorldData();
        B2Worlds.b2World_SetUserData(_id, new B2UserData(data));
        B2Worlds.b2World_SetCustomFilterCallback(_id, FilterCallback, data);
    }

    public World(Scene scene)
        : this()
    {
        Data.Scene = scene;
    }

    internal World(B2WorldId id)
    {
        _id = id;
    }

    public static Vector2 DefaultGravity { get; set; } = _config.DefaultGravity;

    private WorldData Data => (WorldData)B2Worlds.b2World_GetUserData(_id).oValue;

    public Scene Scene => Data.Scene!;

    public Vector2 Gravity
    {
        get => MetersToPixels(new Vector2(B2Worlds.b2World_GetGravity(_id)));
        set => B2Worlds.b2World_SetGravity(_id, PixelsToMeters(value).B2Vec2);
    }

    public void Dispose()
    {
        B2Worlds.b2DestroyWorld(_id);
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<WorldConfig>() ?? _config;
        DefaultGravity = _config.DefaultGravity;
    }

    public void OnFilter(Func<Shape, Shape, bool> func)
    {
        var data = Data;
        data.Scene?.ThrowIfConfigured();
        data.OnFilter += func;
    }

    public void OnContactBegin(Action<Shape, Shape> callback)
    {
        var data = Data;
        data.Scene?.ThrowIfConfigured();
        data.OnContactBegin += callback;
    }

    public void OnContactEnd(Action<Shape, Shape> callback)
    {
        var data = Data;
        data.Scene?.ThrowIfConfigured();
        data.OnContactEnd += callback;
    }

    public void OnContactHit(Action<ContactHit> callback)
    {
        var data = Data;
        data.Scene?.ThrowIfConfigured();
        data.OnContactHit += callback;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 PixelsToMeters(Vector2 pixels)
    {
        return pixels * PixelsToMeter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 MetersToPixels(Vector2 meters)
    {
        return meters * PixelsPerMeter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PixelsToMeters(float pixels)
    {
        return pixels * PixelsToMeter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MetersToPixels(float meters)
    {
        return meters * PixelsPerMeter;
    }

    public void Update(in TimeSpan? step = null)
    {
        B2Worlds.b2World_Step(_id, (float)(step ?? Time.FixedDelta).TotalSeconds, 4);
        DispatchContactEvents();
    }

    public void Overlap(
        Vector2 lowerBound,
        Vector2 upperBound,
        Func<Shape, bool> callback,
        in ShapeFilter? filter = null
    )
    {
        var b2Aabb = new B2AABB
        {
            lowerBound = PixelsToMeters(lowerBound).B2Vec2,
            upperBound = PixelsToMeters(upperBound).B2Vec2,
        };
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapAABB(
            _id,
            in b2Aabb,
            in b2Filter,
            static (id, ctx) => ((Func<Shape, bool>)ctx!)(new Shape(id)),
            callback
        );
    }

    public void Overlap(Vector2 lowerBound, Vector2 upperBound, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        var b2Aabb = new B2AABB
        {
            lowerBound = PixelsToMeters(lowerBound).B2Vec2,
            upperBound = PixelsToMeters(upperBound).B2Vec2,
        };
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapAABB(
            _id,
            in b2Aabb,
            in b2Filter,
            static (id, ctx) =>
            {
                ((Action<Shape>)ctx!)(new Shape(id));
                return true;
            },
            callback
        );
    }

    public void Overlap(in CircleShape circle, Func<Shape, bool> callback, in ShapeFilter? filter = null)
    {
        var proxy = circle.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) => ((Func<Shape, bool>)ctx!)(new Shape(id)),
            callback
        );
    }

    public void Overlap(in CircleShape circle, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        var proxy = circle.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) =>
            {
                ((Action<Shape>)ctx!)(new Shape(id));
                return true;
            },
            callback
        );
    }

    public void Overlap(in CapsuleShape capsule, Func<Shape, bool> callback, in ShapeFilter? filter = null)
    {
        var proxy = capsule.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) => ((Func<Shape, bool>)ctx!)(new Shape(id)),
            callback
        );
    }

    public void Overlap(in CapsuleShape capsule, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        var proxy = capsule.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) =>
            {
                ((Action<Shape>)ctx!)(new Shape(id));
                return true;
            },
            callback
        );
    }

    public void Overlap(in PolygonShape polygon, Func<Shape, bool> callback, in ShapeFilter? filter = null)
    {
        var proxy = polygon.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) => ((Func<Shape, bool>)ctx!)(new Shape(id)),
            callback
        );
    }

    public void Overlap(in PolygonShape polygon, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        var proxy = polygon.MakeProxy();
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            _id,
            ref proxy,
            in b2Filter,
            (id, ctx) =>
            {
                ((Action<Shape>)ctx!)(new Shape(id));
                return true;
            },
            callback
        );
    }

    public RayCastHit CastRay(Vector2 origin, Vector2 translation)
    {
        var result = B2Worlds.b2World_CastRayClosest(
            _id,
            PixelsToMeters(origin).B2Vec2,
            PixelsToMeters(translation).B2Vec2,
            B2Types.b2DefaultQueryFilter()
        );
        return new RayCastHit
        {
            Hit = result.hit,
            Shape = new Shape(result.shapeId),
            Point = MetersToPixels(new Vector2(result.point)),
            Normal = new Vector2(result.normal),
            Fraction = result.fraction,
        };
    }

    public Body CreateBody(in BodyDef def)
    {
        var b2Def = B2Types.b2DefaultBodyDef();
        b2Def.type = (B2BodyType)def.Type;
        b2Def.position = PixelsToMeters(def.Position).B2Vec2;
        b2Def.rotation = B2Rot.FromDegrees(def.Rotation);
        b2Def.linearVelocity = PixelsToMeters(def.LinearVelocity).B2Vec2;
        b2Def.angularVelocity = def.AngularVelocity;
        b2Def.linearDamping = def.LinearDamping;
        b2Def.angularDamping = def.AngularDamping;
        b2Def.gravityScale = def.GravityScale;
        b2Def.sleepThreshold = PixelsToMeters(def.SleepThreshold);
        b2Def.motionLocks.linearX = def.LockLinearX;
        b2Def.motionLocks.linearY = def.LockLinearY;
        b2Def.motionLocks.angularZ = def.LockAngularZ;
        b2Def.enableSleep = def.EnableSleep;
        b2Def.isAwake = def.IsAwake;
        b2Def.isBullet = def.IsBullet;
        b2Def.isEnabled = def.IsEnabled;
        b2Def.allowFastRotation = def.AllowFastRotation;
        var bodyId = B2Bodies.b2CreateBody(_id, b2Def);
        return new Body(bodyId);
    }

    private void DispatchContactEvents()
    {
        var data = Data;
        if (data.OnContactBegin is null && data.OnContactEnd is null && data.OnContactHit is null)
            return;
        var events = B2Worlds.b2World_GetContactEvents(_id);
        if (data.OnContactBegin is { } onBegin)
            for (var i = 0; i < events.beginCount; i++)
            {
                var e = events.beginEvents[i];
                onBegin.SafeInvoke(new Shape(e.shapeIdA), new Shape(e.shapeIdB));
            }

        if (data.OnContactEnd is { } onEnd)
            for (var i = 0; i < events.endCount; i++)
            {
                var e = events.endEvents[i];
                onEnd.SafeInvoke(new Shape(e.shapeIdA), new Shape(e.shapeIdB));
            }

        if (data.OnContactHit is not { } onHit)
            return;
        {
            for (var i = 0; i < events.hitCount; i++)
            {
                var e = events.hitEvents[i];
                onHit.SafeInvoke(
                    new ContactHit
                    {
                        ShapeA = new Shape(e.shapeIdA),
                        ShapeB = new Shape(e.shapeIdB),
                        Point = MetersToPixels(new Vector2(e.point)),
                        Normal = new Vector2(e.normal),
                        ApproachSpeed = MetersToPixels(e.approachSpeed),
                    }
                );
            }
        }
    }

    private static bool FilterCallback(B2ShapeId shapeIdA, B2ShapeId shapeIdB, object context)
    {
        var data = (WorldData)context;
        foreach (var func in Delegate.EnumerateInvocationList(data.OnFilter))
            try
            {
                if (!func.Invoke(new Shape(shapeIdA), new Shape(shapeIdB)))
                    return false;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        return true;
    }
}

internal sealed class WorldData
{
    public Action<Shape, Shape>? OnContactBegin;
    public Action<Shape, Shape>? OnContactEnd;
    public Action<ContactHit>? OnContactHit;
    public Func<Shape, Shape, bool>? OnFilter;
    public Scene? Scene = null;
}
