#pragma warning disable CS9084

using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public unsafe struct Entity
{
    public static Entity Null => new(Flecs.NET.Core.Entity.Null());

    private Flecs.NET.Core.Entity _entity;

    internal Entity(Flecs.NET.Core.Entity entity)
    {
        _entity = entity;
    }

    public ulong Id => _entity.Id.Value;

    public string Name => _entity.Name();

    public bool IsValid =>
        this != Null && _entity.IsValid() && _entity.IsAlive() && _entity.Has<int>() && _entity.Has<Transform>();

    public bool IsSingleton => _entity.CsWorld().Has(_entity);

    public Entity Parent => new(_entity.Parent());

    public ref Transform Transform => ref _entity.GetMut<Transform>();

    public ref Vector2 Position => ref Transform.Position;

    public ref Vector2 Scale => ref Transform.Scale;

    public ref float Rotation => ref Transform.Rotation;

    public ref Vector2 PivotPoint => ref Transform.PivotPoint;

    public ref int ZIndex => ref _entity.GetMut<int>();

    public Transform WorldTransform
    {
        get
        {
            var transform = Transform;
            for (var entity = Parent; entity.IsValid; entity = entity.Parent)
                transform += entity.Transform;
            return transform;
        }
    }

    public Vector2 WorldPosition
    {
        get
        {
            var position = Position;
            for (var entity = Parent; entity.IsValid; entity = entity.Parent)
                position += entity.Position;
            return position;
        }
    }

    public Vector2 WorldScale
    {
        get
        {
            var scale = Scale;
            for (var entity = Parent; entity.IsValid; entity = entity.Parent)
                scale *= entity.Scale;
            return scale;
        }
    }

    public float WorldRotation
    {
        get
        {
            var rotation = Rotation;
            for (var entity = Parent; entity.IsValid; entity = entity.Parent)
                rotation += entity.Rotation;
            return rotation;
        }
    }

    public Vector2 WorldPivotPoint
    {
        get
        {
            var pivotPoint = PivotPoint;
            for (var entity = Parent; entity.IsValid; entity = entity.Parent)
                pivotPoint += entity.PivotPoint;
            return pivotPoint;
        }
    }

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
        return obj is Entity entity && _entity == entity._entity;
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

    public ref T Get<T>()
    {
        return ref _entity.GetMut<T>();
    }

    public ref Entity Set<T>(T data)
    {
        Set(ref data);
        return ref this;
    }

    public ref Entity Set<T>(ref T data)
    {
        var hadT = _entity.Has<T>();
        _entity.Set(ref data);
        if (hadT)
            _entity.CsWorld().Event<SetEvent>().Id<T>().Entity(_entity).Emit();
        else
            _entity.CsWorld().Event<AddEvent>().Id<T>().Entity(_entity).Emit();
        return ref this;
    }

    public ref Entity Remove<T>()
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Cannot remove transform component.");
        if (typeof(T) == typeof(int))
            throw new InvalidOperationException("Cannot remove zindex component.");
        _entity.Remove<T>();
        return ref this;
    }

    public void Destroy()
    {
        _entity.Destruct();
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
