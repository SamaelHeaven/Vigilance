#pragma warning disable CS9084

using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public unsafe struct Entity
{
    public static Entity Null => new(Flecs.NET.Core.Entity.Null());

    internal Flecs.NET.Core.Entity FEntity;

    internal Entity(Flecs.NET.Core.Entity entity)
    {
        FEntity = entity;
    }

    public ulong Id => FEntity.Id.Value;

    public string Name => FEntity.Name();

    public bool IsValid => FEntity.IsValid();

    public bool IsSingleton => FEntity.CsWorld().Has(FEntity);

    public Entity Parent => new(FEntity.Parent());

    public ref Transform Transform => ref FEntity.GetMut<Transform>();

    public ref Vector2 Position => ref Transform.Position;

    public ref Vector2 Scale => ref Transform.Scale;

    public ref float Rotation => ref Transform.Rotation;

    public ref Vector2 PivotPoint => ref Transform.PivotPoint;

    public ref int ZIndex => ref FEntity.GetMut<int>();

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
        return ref FEntity.GetMut<T>();
    }

    public ref Entity Set<T>(T data)
    {
        Set(ref data);
        return ref this;
    }

    public ref Entity Set<T>(ref T data)
    {
        var hadT = FEntity.Has<T>();
        FEntity.Set(ref data);
        if (hadT)
            FEntity.CsWorld().Event<SetEvent>().Id<T>().Entity(FEntity).Emit();
        else
            FEntity.CsWorld().Event<AddEvent>().Id<T>().Entity(FEntity).Emit();
        return ref this;
    }

    public ref Entity Remove<T>()
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException("Cannot remove transform component.");
        if (typeof(T) == typeof(int))
            throw new InvalidOperationException("Cannot remove zindex component.");
        FEntity.Remove<T>();
        return ref this;
    }

    public void Destroy()
    {
        FEntity.Destruct();
    }

    public ref Entity ChildOf(Entity parent)
    {
        FEntity.ChildOf(parent.FEntity);
        return ref this;
    }

    public bool IsChildOf(Entity parent)
    {
        return FEntity.Has(Ecs.ChildOf, parent.FEntity);
    }

    #region Has

    public bool Has<T0>()
    {
        return FEntity.Has<T0>();
    }

    public bool Has<T0, T1>()
    {
        return FEntity.Has<T0>() && FEntity.Has<T1>();
    }

    public bool Has<T0, T1, T2>()
    {
        return FEntity.Has<T0>() && FEntity.Has<T1>() && FEntity.Has<T2>();
    }

    public bool Has<T0, T1, T2, T3>()
    {
        return FEntity.Has<T0>() && FEntity.Has<T1>() && FEntity.Has<T2>() && FEntity.Has<T3>();
    }

    public bool Has<T0, T1, T2, T3, T4>()
    {
        return FEntity.Has<T0>() && FEntity.Has<T1>() && FEntity.Has<T2>() && FEntity.Has<T3>() && FEntity.Has<T4>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>()
            && FEntity.Has<T11>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>()
            && FEntity.Has<T11>()
            && FEntity.Has<T12>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>()
            && FEntity.Has<T11>()
            && FEntity.Has<T12>()
            && FEntity.Has<T13>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>()
            && FEntity.Has<T11>()
            && FEntity.Has<T12>()
            && FEntity.Has<T13>()
            && FEntity.Has<T14>();
    }

    public bool Has<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
    {
        return FEntity.Has<T0>()
            && FEntity.Has<T1>()
            && FEntity.Has<T2>()
            && FEntity.Has<T3>()
            && FEntity.Has<T4>()
            && FEntity.Has<T5>()
            && FEntity.Has<T6>()
            && FEntity.Has<T7>()
            && FEntity.Has<T8>()
            && FEntity.Has<T9>()
            && FEntity.Has<T10>()
            && FEntity.Has<T11>()
            && FEntity.Has<T12>()
            && FEntity.Has<T13>()
            && FEntity.Has<T14>()
            && FEntity.Has<T15>();
    }

    #endregion
}
