using System.Runtime.CompilerServices;
using Box2D.NET;
using Vigilance.Core;
using Vigilance.Drawing;
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

    public void DebugDraw(Graphics graphics, DebugDrawFlags flags = DebugDrawFlags.Default, Camera? camera = null)
    {
        var draw = B2Types.b2DefaultDebugDraw();
        draw.context = new DebugDrawContext(graphics, camera);
        draw.DrawPolygonFcn = DrawPolygon;
        draw.DrawSolidPolygonFcn = DrawSolidPolygon;
        draw.DrawCircleFcn = DrawCircle;
        draw.DrawSolidCircleFcn = DrawSolidCircle;
        draw.DrawSolidCapsuleFcn = DrawSolidCapsule;
        draw.drawLineFcn = DrawSegment;
        draw.DrawTransformFcn = DrawTransform;
        draw.DrawPointFcn = DrawPoint;
        draw.DrawStringFcn = DrawString;
        draw.drawShapes = flags.HasFlag(DebugDrawFlags.Shapes);
        draw.drawJoints = flags.HasFlag(DebugDrawFlags.Joints);
        draw.drawJointExtras = flags.HasFlag(DebugDrawFlags.JointExtras);
        draw.drawBounds = flags.HasFlag(DebugDrawFlags.Bounds);
        draw.drawMass = flags.HasFlag(DebugDrawFlags.Mass);
        draw.drawBodyNames = flags.HasFlag(DebugDrawFlags.BodyNames);
        draw.drawContactPoints = flags.HasFlag(DebugDrawFlags.ContactPoints);
        draw.drawGraphColors = flags.HasFlag(DebugDrawFlags.GraphColors);
        draw.drawContactFeatures = flags.HasFlag(DebugDrawFlags.ContactFeatures);
        draw.drawContactNormals = flags.HasFlag(DebugDrawFlags.ContactNormals);
        draw.drawContactForces = flags.HasFlag(DebugDrawFlags.ContactForces);
        draw.drawFrictionForces = flags.HasFlag(DebugDrawFlags.FrictionForces);
        draw.drawIslands = flags.HasFlag(DebugDrawFlags.Islands);
        B2Worlds.b2World_Draw(_id, draw);
    }

    private static Color ToColor(B2HexColor hexColor)
    {
        var value = (uint)hexColor;
        return new Color((byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff), (byte)(value & 0xff));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Transform(in B2Transform transform, B2Vec2 vertex)
    {
        var x = transform.q.c * vertex.X - transform.q.s * vertex.Y + transform.p.X;
        var y = transform.q.s * vertex.X + transform.q.c * vertex.Y + transform.p.Y;
        return MetersToPixels(new Vector2(x, y));
    }

    private static Vector2[] TransformVertices(
        in B2Transform transform,
        in ReadOnlySpan<B2Vec2> vertices,
        int vertexCount
    )
    {
        var points = new Vector2[vertexCount];
        for (var i = 0; i < vertexCount; i++)
            points[i] = Transform(transform, vertices[i]);
        return points;
    }

    private static void DrawPolygon(ReadOnlySpan<B2Vec2> vertices, int vertexCount, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var points = new Vector2[vertexCount];
        for (var i = 0; i < vertexCount; i++)
            points[i] = MetersToPixels(new Vector2(vertices[i]));
        graphics.StrokeCustomPolygon(points, ToColor(color), camera: camera);
    }

    private static void DrawSolidPolygon(
        in B2Transform transform,
        ReadOnlySpan<B2Vec2> vertices,
        int vertexCount,
        float radius,
        B2HexColor color,
        object context
    )
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var points = TransformVertices(transform, vertices, vertexCount);
        var fill = ToColor(color);
        graphics.FillCustomPolygon(points, fill.Alpha(0.5f), camera);
        graphics.StrokeCustomPolygon(points, fill, camera: camera);
    }

    private static void DrawCircle(in B2Vec2 center, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.StrokeCircle(
            MetersToPixels(new Vector2(center)),
            MetersToPixels(radius),
            ToColor(color),
            camera: camera
        );
    }

    private static void DrawSolidCircle(in B2Transform transform, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var center = MetersToPixels(new Vector2(transform.p));
        var radiusPixels = MetersToPixels(radius);
        var fill = ToColor(color);
        graphics.FillCircle(center, radiusPixels, fill.Alpha(0.5f), camera: camera);
        graphics.StrokeCircle(center, radiusPixels, fill, camera: camera);
        var axis = Transform(transform, new B2Vec2(radius, 0f));
        graphics.DrawLine(center, axis, fill, camera: camera);
    }

    private static void DrawSolidCapsule(in B2Vec2 p1, in B2Vec2 p2, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var start = MetersToPixels(new Vector2(p1));
        var end = MetersToPixels(new Vector2(p2));
        var radiusPixels = MetersToPixels(radius);
        var fill = ToColor(color);
        var body = fill.Alpha(0.5f);
        var axis = end - start;
        var normal = new Vector2(-axis.Y, axis.X).Normalize() * radiusPixels;
        graphics.FillCustomPolygon([start + normal, end + normal, end - normal, start - normal], body, camera);
        graphics.FillCircle(start, radiusPixels, body, camera: camera);
        graphics.FillCircle(end, radiusPixels, body, camera: camera);
        graphics.DrawLine(start + normal, end + normal, fill, camera: camera);
        graphics.DrawLine(start - normal, end - normal, fill, camera: camera);
        graphics.StrokeCircle(start, radiusPixels, fill, camera: camera);
        graphics.StrokeCircle(end, radiusPixels, fill, camera: camera);
    }

    private static void DrawSegment(in B2Vec2 p1, in B2Vec2 p2, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.DrawLine(
            MetersToPixels(new Vector2(p1)),
            MetersToPixels(new Vector2(p2)),
            ToColor(color),
            camera: camera
        );
    }

    private static void DrawTransform(in B2Transform transform, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        const float axisScale = 0.4f;
        var origin = MetersToPixels(new Vector2(transform.p));
        graphics.DrawLine(origin, Transform(transform, new B2Vec2(axisScale, 0f)), Color.Red, camera: camera);
        graphics.DrawLine(origin, Transform(transform, new B2Vec2(0f, axisScale)), Color.Green, camera: camera);
    }

    private static void DrawPoint(in B2Vec2 p, float size, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.FillCircle(MetersToPixels(new Vector2(p)), size * 0.5f, ToColor(color), camera: camera);
    }

    private static void DrawString(in B2Vec2 p, string s, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.FillText(s, MetersToPixels(new Vector2(p)), ToColor(color), camera: camera);
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

    private record DebugDrawContext(Graphics Graphics, Camera? Camera);
}

internal sealed class WorldData
{
    public Action<Shape, Shape>? OnContactBegin;
    public Action<Shape, Shape>? OnContactEnd;
    public Action<ContactHit>? OnContactHit;
    public Func<Shape, Shape, bool>? OnFilter;
    public Scene? Scene = null;
}
