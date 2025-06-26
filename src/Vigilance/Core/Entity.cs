#pragma warning disable CS9084

using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public unsafe struct Entity
{
    public static Entity Null { get; } = new(Flecs.NET.Core.Entity.Null(), null!);
    public Scene Scene { get; }

    private Flecs.NET.Core.Entity _entity;

    internal Entity(Flecs.NET.Core.Entity entity, Scene scene)
    {
        _entity = entity;
        Scene = scene;
    }

    public ulong Id => _entity.Id.Value;

    public string Name => _entity.Name();

    public bool Valid => _entity.IsValid();

    public Entity Parent => new(_entity.Parent(), null!);

    public Transform Transform
    {
        get =>
            new()
            {
                Position = Position,
                Scale = Scale,
                Rotation = Rotation,
                PivotPoint = PivotPoint,
            };
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public Vector2 Position
    {
        get => Has<Position>() ? Get<Position>().Value : Vector2.Zero;
        set
        {
            if (!Precision.AreEqual(Position, value))
                Set(new Position { Value = value }, false);
        }
    }

    public Vector2 Scale
    {
        get => Has<Scale>() ? Get<Scale>().Value : Vector2.One;
        set
        {
            if (!Precision.AreEqual(Scale, value))
                Set(new Scale { Value = value }, false);
        }
    }

    public float Rotation
    {
        get => Has<Rotation>() ? Get<Rotation>().Value : 0;
        set
        {
            if (!Precision.AreEqual(Rotation, value))
                Set(new Rotation { Value = value }, false);
        }
    }

    public Vector2 PivotPoint
    {
        get => Has<PivotPoint>() ? Get<PivotPoint>().Value : Vector2.Zero;
        set
        {
            if (!Precision.AreEqual(PivotPoint, value))
                Set(new PivotPoint { Value = value }, false);
        }
    }

    public int ZIndex
    {
        get => Has<ZIndex>() ? Get<ZIndex>().Value : 0;
        set
        {
            if (ZIndex != value)
                Set(new ZIndex { Value = value }, false);
        }
    }

    public Transform WorldTransform
    {
        get
        {
            var transform = Transform;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                transform += entity.Transform;
            return transform;
        }
    }

    public Vector2 WorldPosition
    {
        get
        {
            var position = Position;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                position += entity.Position;
            return position;
        }
    }

    public Vector2 WorldScale
    {
        get
        {
            var scale = Scale;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                scale *= entity.Scale;
            return scale;
        }
    }

    public float WorldRotation
    {
        get
        {
            var rotation = Rotation;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                rotation += entity.Rotation;
            return rotation;
        }
    }

    public Vector2 WorldPivotPoint
    {
        get
        {
            var pivotPoint = PivotPoint;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                pivotPoint += entity.PivotPoint;
            return pivotPoint;
        }
    }

    public int WorldZIndex
    {
        get
        {
            var zIndex = ZIndex;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                zIndex += entity.ZIndex;
            return zIndex;
        }
    }

    public Components Components => _entity.Has<Components>() ? _entity.Get<Components>() : Components.Empty;

    public static bool operator ==(Entity a, Entity b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Equals(entity);
    }

    public bool Equals(Entity other)
    {
        return _entity == other._entity;
    }

    public override int GetHashCode()
    {
        return _entity.GetHashCode();
    }

    public ref Entity SetTransform(Transform transform)
    {
        Transform = transform;
        return ref this;
    }

    public ref Entity SetPosition(float v1, float? v2 = null)
    {
        Position = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref Entity SetPosition(Vector2 position)
    {
        Position = position;
        return ref this;
    }

    public ref Entity SetScale(float v1, float? v2 = null)
    {
        Scale = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref Entity SetScale(Vector2 scale)
    {
        Scale = scale;
        return ref this;
    }

    public ref Entity SetRotation(float rotation)
    {
        Rotation = rotation;
        return ref this;
    }

    public ref Entity SetPivotPoint(float v1, float? v2 = null)
    {
        PivotPoint = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref Entity SetPivotPoint(Vector2 pivotPoint)
    {
        PivotPoint = pivotPoint;
        return ref this;
    }

    public ref Entity SetZIndex(int zIndex)
    {
        ZIndex = zIndex;
        return ref this;
    }

    public T Get<T>()
    {
        return _entity.Get<T>();
    }

    public ref Entity Set<T>(T data)
    {
        Set(data, true);
        return ref this;
    }

    public ref Entity Set<T>(ref T data)
    {
        Set(data, true);
        return ref this;
    }

    private ref Entity Set<T>(T data, bool updateComponents)
    {
        Set(ref data, updateComponents);
        return ref this;
    }

    private ref Entity Set<T>(ref T data, bool updateComponents)
    {
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be set.");
        var hadT = _entity.Has<T>();
        if (updateComponents)
            Scene.DeferSetComponent(_entity, type, data);
        _entity.Set(ref data);
        if (hadT)
            _entity.CsWorld().Event<SetEvent>().Id<T>().Entity(_entity).Enqueue();
        else
            _entity.CsWorld().Event<AddEvent>().Id<T>().Entity(_entity).Enqueue();
        return ref this;
    }

    public ref Entity Remove<T>()
    {
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be removed.");
        Scene.DeferRemoveComponent(_entity, type);
        _entity.Remove<T>();
        return ref this;
    }

    public void Destroy()
    {
        _entity.Destruct();
    }

    public ref Entity Scope(Action action)
    {
        _entity.Scope(action);
        return ref this;
    }

    public ref Entity ChildOf(Entity parent)
    {
        _entity.ChildOf(parent._entity);
        return ref this;
    }

    public bool IsChildOf(Entity parent)
    {
        return _entity.Has(Ecs.ChildOf, parent._entity);
    }

    public ref Entity Children(Action<Entity> action)
    {
        var scene = Scene;
        scene.DeferBegin();
        _entity.Children(entity => action.Invoke(new Entity(entity, scene)));
        scene.DeferEnd();
        return ref this;
    }

    #region Traverse

    public ref Entity Traverse(Action<Entity> action)
    {
        action.Invoke(this);
        Children(child => child.Traverse(action));
        return ref this;
    }

    public ref Entity Traverse<T>(Action<Entity> action)
    {
        if (Has<T>())
            action.Invoke(this);
        Children(child => child.Traverse<T>(action));
        return ref this;
    }

    public ref Entity Traverse<T>(Action<T> action)
    {
        if (Has<T>())
            action.Invoke(Get<T>());
        Children(child => child.Traverse(action));
        return ref this;
    }

    public ref Entity Traverse<T>(Action<Entity, T> action)
    {
        if (Has<T>())
            action.Invoke(this, Get<T>());
        Children(child => child.Traverse(action));
        return ref this;
    }

    #endregion

    #region TryGet

    public bool TryGet<T0>(out T0 t)
    {
        var result = Has<T0>();
        t = default!;
        if (result)
            t = Get<T0>();
        return result;
    }

    public bool TryGet<T0, T1>(out T0 t0, out T1 t1)
    {
        t0 = default!;
        t1 = default!;
        return TryGet(out t0) && TryGet(out t1);
    }

    public bool TryGet<T0, T1, T2>(out T0 t0, out T1 t1, out T2 t2)
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        return TryGet(out t0) && TryGet(out t1) && TryGet(out t2);
    }

    public bool TryGet<T0, T1, T2, T3>(out T0 t0, out T1 t1, out T2 t2, out T3 t3)
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        return TryGet(out t0) && TryGet(out t1) && TryGet(out t2) && TryGet(out t3);
    }

    public bool TryGet<T0, T1, T2, T3, T4>(out T0 t0, out T1 t1, out T2 t2, out T3 t3, out T4 t4)
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        return TryGet(out t0) && TryGet(out t1) && TryGet(out t2) && TryGet(out t3) && TryGet(out t4);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5>(out T0 t0, out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5)
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        return TryGet(out t0) && TryGet(out t1) && TryGet(out t2) && TryGet(out t3) && TryGet(out t4) && TryGet(out t5);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9,
        out T10 t10
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        t10 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9)
            && TryGet(out t10);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9,
        out T10 t10,
        out T11 t11
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        t10 = default!;
        t11 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9)
            && TryGet(out t10)
            && TryGet(out t11);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9,
        out T10 t10,
        out T11 t11,
        out T12 t12
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        t10 = default!;
        t11 = default!;
        t12 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9)
            && TryGet(out t10)
            && TryGet(out t11)
            && TryGet(out t12);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9,
        out T10 t10,
        out T11 t11,
        out T12 t12,
        out T13 t13
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        t10 = default!;
        t11 = default!;
        t12 = default!;
        t13 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9)
            && TryGet(out t10)
            && TryGet(out t11)
            && TryGet(out t12)
            && TryGet(out t13);
    }

    public bool TryGet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        out T0 t0,
        out T1 t1,
        out T2 t2,
        out T3 t3,
        out T4 t4,
        out T5 t5,
        out T6 t6,
        out T7 t7,
        out T8 t8,
        out T9 t9,
        out T10 t10,
        out T11 t11,
        out T12 t12,
        out T13 t13,
        out T14 t14
    )
    {
        t0 = default!;
        t1 = default!;
        t2 = default!;
        t3 = default!;
        t4 = default!;
        t5 = default!;
        t6 = default!;
        t7 = default!;
        t8 = default!;
        t9 = default!;
        t10 = default!;
        t11 = default!;
        t12 = default!;
        t13 = default!;
        t14 = default!;
        return TryGet(out t0)
            && TryGet(out t1)
            && TryGet(out t2)
            && TryGet(out t3)
            && TryGet(out t4)
            && TryGet(out t5)
            && TryGet(out t6)
            && TryGet(out t7)
            && TryGet(out t8)
            && TryGet(out t9)
            && TryGet(out t10)
            && TryGet(out t11)
            && TryGet(out t12)
            && TryGet(out t13)
            && TryGet(out t14);
    }

    #endregion

    #region Has

    public bool Has<T0>()
    {
        return _entity.Has<T0>();
    }

    public bool Has<T0, T1>()
    {
        return _entity.Has<T0>() && _entity.Has<T1>();
    }

    public bool Has<T0, T1, T2>()
    {
        return _entity.Has<T0>() && _entity.Has<T1>() && _entity.Has<T2>();
    }

    public bool Has<T0, T1, T2, T3>()
    {
        return _entity.Has<T0>() && _entity.Has<T1>() && _entity.Has<T2>() && _entity.Has<T3>();
    }

    public bool Has<T0, T1, T2, T3, T4>()
    {
        return _entity.Has<T0>() && _entity.Has<T1>() && _entity.Has<T2>() && _entity.Has<T3>() && _entity.Has<T4>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>()
            && _entity.Has<T10>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>()
            && _entity.Has<T10>()
            && _entity.Has<T11>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>()
            && _entity.Has<T10>()
            && _entity.Has<T11>()
            && _entity.Has<T12>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>()
            && _entity.Has<T10>()
            && _entity.Has<T11>()
            && _entity.Has<T12>()
            && _entity.Has<T13>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
    {
        return _entity.Has<T0>()
            && _entity.Has<T1>()
            && _entity.Has<T2>()
            && _entity.Has<T3>()
            && _entity.Has<T4>()
            && _entity.Has<T5>()
            && _entity.Has<T6>()
            && _entity.Has<T7>()
            && _entity.Has<T8>()
            && _entity.Has<T9>()
            && _entity.Has<T10>()
            && _entity.Has<T11>()
            && _entity.Has<T12>()
            && _entity.Has<T13>()
            && _entity.Has<T14>();
    }

    #endregion
}
