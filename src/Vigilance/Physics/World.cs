using System.Buffers;
using System.Runtime.CompilerServices;
using Box2D.NET;

namespace Vigilance.Physics;

public sealed class World
{
    public const float PixelsPerMeter = 50f;
    public const float PixelsToMeter = 1f / PixelsPerMeter;
    private static WorldConfig _config = new();
    internal readonly B2WorldId Id;
    private readonly TaskFactory? _taskFactory;
    private readonly int _workerCount;
    private bool _disposed;
    private Action<Shape, Shape>? _onContactBegin;
    private Action<Shape, Shape>? _onContactEnd;
    private Action<ContactHit>? _onContactHit;
    private Func<Shape, Shape, bool>? _onFilter;
    private Action<Shape, Shape>? _onSensorBegin;
    private Action<Shape, Shape>? _onSensorEnd;

    public World()
        : this(new WorldDef()) { }

    public World(in WorldDef def)
    {
        Scene = def.Scene!;
        Multithreaded = def.Multithreaded && Platform.Current.SupportsThreads;
        var worldRef = new WeakReference<World>(this);
        var b2Def = B2Types.b2DefaultWorldDef();
        b2Def.userData = new B2UserData(
            def.Scene is null || !def.Internal ? worldRef : new WeakReference<Scene>(def.Scene)
        );
        b2Def.gravity = PixelsToMeters(def.Gravity).B2Vec2;
        if (Multithreaded)
        {
            _taskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.None,
                TaskScheduler.Default
            );
            _workerCount = Environment.ProcessorCount;
            b2Def.workerCount = _workerCount;
            b2Def.enqueueTask = EnqueueTask;
            b2Def.finishTask = FinishTask;
        }

        Id = B2Worlds.b2CreateWorld(b2Def);
        B2Worlds.b2World_SetCustomFilterCallback(Id, FilterCallback, worldRef);
    }

    public static Vector2 DefaultGravity { get; set; } = _config.DefaultGravity;
    public static bool DefaultMultithreaded { get; set; } = _config.DefaultMultithreaded;

    public Scene Scene { get; private set; }

    public bool Multithreaded { get; }

    public Vector2 Gravity
    {
        get => MetersToPixels(new Vector2(B2Worlds.b2World_GetGravity(Id)));
        set => B2Worlds.b2World_SetGravity(Id, PixelsToMeters(value).B2Vec2);
    }

    ~World()
    {
        if (!_disposed)
            Game.RunLater(this, world => world.ReleaseUnmanagedResources());
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;
        _disposed = true;
        Scene = null!;
        _onFilter = null;
        _onContactBegin = null;
        _onContactEnd = null;
        _onContactHit = null;
        _onSensorBegin = null;
        _onSensorEnd = null;
        B2Worlds.b2DestroyWorld(Id);
    }

    internal static World? GetWorld(B2WorldId worldId)
    {
        var value = B2Worlds.b2World_GetUserData(worldId).oValue;
        if (value is WeakReference<Scene> sceneRef)
            return sceneRef.TryGetTarget(out var scene) ? scene.World : null;
        return Unsafe.As<object, WeakReference<World>>(ref value).TryGetTarget(out var world) ? world : null;
    }

    internal static Scene? GetScene(B2WorldId worldId)
    {
        var value = B2Worlds.b2World_GetUserData(worldId).oValue;
        if (value is WeakReference<Scene> sceneRef)
            return sceneRef.TryGetTarget(out var scene) ? scene : null;
        return Unsafe.As<object, WeakReference<World>>(ref value).TryGetTarget(out var world) ? world.Scene : null;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<WorldConfig>() ?? _config;
        DefaultGravity = _config.DefaultGravity;
        DefaultMultithreaded = _config.DefaultMultithreaded;
    }

    public void OnFilter(Func<Shape, Shape, bool> func)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onFilter += func;
    }

    public void OnContactBegin(Action<Shape, Shape> callback)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onContactBegin += callback;
    }

    public void OnContactEnd(Action<Shape, Shape> callback)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onContactEnd += callback;
    }

    public void OnContactHit(Action<ContactHit> callback)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onContactHit += callback;
    }

    public void OnSensorBegin(Action<Shape, Shape> callback)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onSensorBegin += callback;
    }

    public void OnSensorEnd(Action<Shape, Shape> callback)
    {
        ((Scene?)Scene)?.ThrowIfConfigured();
        _onSensorEnd += callback;
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
        B2Worlds.b2World_Step(Id, (float)(step ?? Time.FixedDelta).TotalSeconds, 4);
        // ReSharper disable once CanReplaceCastWithVariableType
        var scene = (Scene?)Scene;
        scene?.BeginDefer();
        try
        {
            DispatchContactEvents();
            DispatchSensorEvents();
        }
        finally
        {
            scene?.EndDefer();
        }
    }

    public void Overlap(Box box, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        Overlap(box.Position, box.Position + box.Size, callback, in filter);
    }

    public void Overlap(Box box, Func<Shape, bool> callback, in ShapeFilter? filter = null)
    {
        Overlap(box.Position, box.Position + box.Size, callback, in filter);
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
            Id,
            in b2Aabb,
            in b2Filter,
            (id, ctx) => ((Func<Shape, bool>)ctx!)(new Shape(id)),
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
            Id,
            in b2Aabb,
            in b2Filter,
            (id, ctx) =>
            {
                ((Action<Shape>)ctx!)(new Shape(id));
                return true;
            },
            callback
        );
    }

    public void Overlap(in ShapeProxy shapeProxy, Func<Shape, bool> callback, in ShapeFilter? filter = null)
    {
        var proxy = shapeProxy.B2ShapeProxy;
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            Id,
            ref proxy,
            in b2Filter,
            (id, ctx) => ((Func<Shape, bool>)ctx!).SafeInvoke(new Shape(id)),
            callback
        );
    }

    public void Overlap(in ShapeProxy shapeProxy, Action<Shape> callback, in ShapeFilter? filter = null)
    {
        var proxy = shapeProxy.B2ShapeProxy;
        var b2Filter = filter.ToB2QueryFilter();
        B2Worlds.b2World_OverlapShape(
            Id,
            ref proxy,
            in b2Filter,
            (id, ctx) =>
            {
                ((Action<Shape>)ctx!).SafeInvoke(new Shape(id));
                return true;
            },
            callback
        );
    }

    public RayCastHit CastRay(Vector2 origin, Vector2 translation)
    {
        var result = B2Worlds.b2World_CastRayClosest(
            Id,
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
        var bodyId = B2Bodies.b2CreateBody(Id, b2Def);
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
        B2Worlds.b2World_Draw(Id, draw);
    }

    private static unsafe void DrawPolygon(
        ReadOnlySpan<B2Vec2> vertices,
        int vertexCount,
        B2HexColor color,
        object context
    )
    {
        var (graphics, camera) = (DebugDrawContext)context;
        Vector2[]? pooledArray = null;
        try
        {
            Span<Vector2> span;
            if (vertexCount > 128)
            {
                pooledArray = ArrayPool<Vector2>.Shared.Rent(vertexCount);
                span = pooledArray.AsSpan(0, vertexCount);
            }
            else
            {
                var points = stackalloc Vector2[vertexCount];
                span = new Span<Vector2>(points, vertexCount);
            }

            for (var i = 0; i < vertexCount; i++)
                span[i] = MetersToPixels(new Vector2(vertices[i]));
            graphics.StrokeCustomPolygon(span, color.ToColor(), camera: camera);
        }
        finally
        {
            if (pooledArray != null)
                ArrayPool<Vector2>.Shared.Return(pooledArray);
        }
    }

    private static unsafe void DrawSolidPolygon(
        in B2Transform transform,
        ReadOnlySpan<B2Vec2> vertices,
        int vertexCount,
        float radius,
        B2HexColor color,
        object context
    )
    {
        var (graphics, camera) = (DebugDrawContext)context;
        Vector2[]? pooledArray = null;
        try
        {
            Span<Vector2> span;
            if (vertexCount > 128)
            {
                pooledArray = ArrayPool<Vector2>.Shared.Rent(vertexCount);
                span = pooledArray.AsSpan(0, vertexCount);
            }
            else
            {
                var points = stackalloc Vector2[vertexCount];
                span = new Span<Vector2>(points, vertexCount);
            }

            for (var i = 0; i < vertexCount; i++)
                span[i] = transform.Transform(vertices[i]);
            var fill = color.ToColor();
            graphics.FillCustomPolygon(span, fill.Alpha(0.5f), camera);
            graphics.StrokeCustomPolygon(span, fill, camera: camera);
        }
        finally
        {
            if (pooledArray != null)
                ArrayPool<Vector2>.Shared.Return(pooledArray);
        }
    }

    private static void DrawCircle(in B2Vec2 center, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.StrokeCircle(
            MetersToPixels(new Vector2(center)),
            MetersToPixels(radius),
            color.ToColor(),
            camera: camera
        );
    }

    private static void DrawSolidCircle(in B2Transform transform, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var center = MetersToPixels(new Vector2(transform.p));
        var radiusPixels = MetersToPixels(radius);
        var fill = color.ToColor();
        graphics.FillCircle(center, radiusPixels, fill.Alpha(0.5f), camera: camera);
        graphics.StrokeCircle(center, radiusPixels, fill, camera: camera);
        var axis = transform.Transform(new B2Vec2(radius, 0f));
        graphics.DrawLine(center, axis, fill, camera: camera);
    }

    private static void DrawSolidCapsule(in B2Vec2 p1, in B2Vec2 p2, float radius, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        var start = MetersToPixels(new Vector2(p1));
        var end = MetersToPixels(new Vector2(p2));
        var radiusPixels = MetersToPixels(radius);
        var fill = color.ToColor();
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
            color.ToColor(),
            camera: camera
        );
    }

    private static void DrawTransform(in B2Transform transform, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        const float axisScale = 0.4f;
        var origin = MetersToPixels(new Vector2(transform.p));
        graphics.DrawLine(origin, transform.Transform(new B2Vec2(axisScale, 0f)), Color.Red500, camera: camera);
        graphics.DrawLine(origin, transform.Transform(new B2Vec2(0f, axisScale)), Color.Green500, camera: camera);
    }

    private static void DrawPoint(in B2Vec2 p, float size, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.FillCircle(MetersToPixels(new Vector2(p)), size * 0.5f, color.ToColor(), camera: camera);
    }

    private static void DrawString(in B2Vec2 p, string s, B2HexColor color, object context)
    {
        var (graphics, camera) = (DebugDrawContext)context;
        graphics.FillText(s, MetersToPixels(new Vector2(p)), color.ToColor(), fontSize: 8, camera: camera);
    }

    private void DispatchContactEvents()
    {
        if (_onContactBegin is null && _onContactEnd is null && _onContactHit is null)
            return;
        var events = B2Worlds.b2World_GetContactEvents(Id);
        if (_onContactBegin is { } onBegin)
            for (var i = 0; i < events.beginCount; i++)
            {
                var e = events.beginEvents[i];
                onBegin.SafeInvoke(new Shape(e.shapeIdA), new Shape(e.shapeIdB));
            }

        if (_onContactEnd is { } onEnd)
            for (var i = 0; i < events.endCount; i++)
            {
                var e = events.endEvents[i];
                onEnd.SafeInvoke(new Shape(e.shapeIdA), new Shape(e.shapeIdB));
            }

        if (_onContactHit is not { } onHit)
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

    private void DispatchSensorEvents()
    {
        if (_onSensorBegin is null && _onSensorEnd is null)
            return;
        var events = B2Worlds.b2World_GetSensorEvents(Id);
        if (_onSensorBegin is { } onBegin)
            for (var i = 0; i < events.beginCount; i++)
            {
                var e = events.beginEvents[i];
                onBegin.SafeInvoke(new Shape(e.sensorShapeId), new Shape(e.visitorShapeId));
            }

        if (_onSensorEnd is not { } onEnd)
            return;
        {
            for (var i = 0; i < events.endCount; i++)
            {
                var e = events.endEvents[i];
                onEnd.SafeInvoke(new Shape(e.sensorShapeId), new Shape(e.visitorShapeId));
            }
        }
    }

    private static bool FilterCallback(B2ShapeId shapeIdA, B2ShapeId shapeIdB, object context)
    {
        var worldRef = (WeakReference<World>)context;
        if (!worldRef.TryGetTarget(out var world))
            return true;
        foreach (var func in Delegate.EnumerateInvocationList(world._onFilter))
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

    private Box2DTask EnqueueTask(
        b2TaskCallback task,
        int itemCount,
        int minRange,
        object taskContext,
        object userContext
    )
    {
        var chunkCount = System.Math.Clamp((itemCount + minRange - 1) / minRange, 1, _workerCount);
        var handle = new Box2DTask(chunkCount);
        var baseSize = itemCount / chunkCount;
        var remainder = itemCount % chunkCount;
        var start = 0;
        for (uint chunk = 0; chunk < chunkCount; chunk++)
        {
            var size = baseSize + (chunk < remainder ? 1 : 0);
            var startIndex = start;
            var endIndex = start + size;
            start = endIndex;
            var workerIndex = chunk;
            _taskFactory?.StartNew(() =>
            {
                try
                {
                    task.Invoke(startIndex, endIndex, workerIndex, taskContext);
                }
                finally
                {
                    handle.Completion.Signal();
                }
            });
        }

        return handle;
    }

    private static void FinishTask(object task, object userContext)
    {
        var handle = (Box2DTask)task;
        handle.Completion.Wait();
        handle.Completion.Dispose();
    }

    private sealed class Box2DTask
    {
        public readonly CountdownEvent Completion;

        public Box2DTask(int count)
        {
            Completion = new CountdownEvent(count);
        }
    }

    private record DebugDrawContext(Graphics Graphics, Camera? Camera);
}
