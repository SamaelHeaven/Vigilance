#pragma warning disable CS9084

using System.Collections;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Flecs.NET.Utilities;
using Vigilance.Math;

// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
#pragma warning disable CS8656 // Call to non-readonly member from a 'readonly' member results in an implicit copy.

namespace Vigilance.Core;

public unsafe partial struct Entity : IEquatable<Entity>
{
    public static Entity Null { get; } = new(Flecs.NET.Core.Entity.Null(), null!);
    public Scene Scene { get; }

    private Flecs.NET.Core.Entity _entity;

    internal Entity(Flecs.NET.Core.Entity entity, Scene scene)
    {
        _entity = entity;
        Scene = scene;
    }

    public readonly ulong Id => _entity.Id.Value;

    public readonly string Name => _entity.Name();

    public readonly bool Valid => _entity.IsValid();

    public readonly Entity Parent => new(_entity.Parent(), null!);

    public readonly Transform Transform
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

    public readonly Vector2 Position
    {
        get => Has<Position>() ? Get<Position>().Value : Vector2.Zero;
        set
        {
            if (!Precision.AreEqual(Position, value))
                Set(new Position { Value = value }, false);
        }
    }

    public readonly Vector2 Scale
    {
        get => Has<Scale>() ? Get<Scale>().Value : Vector2.One;
        set
        {
            if (!Precision.AreEqual(Scale, value))
                Set(new Scale { Value = value }, false);
        }
    }

    public readonly float Rotation
    {
        get => Has<Rotation>() ? Get<Rotation>().Value : 0;
        set
        {
            if (!Precision.AreEqual(Rotation, value))
                Set(new Rotation { Value = value }, false);
        }
    }

    public readonly Vector2 PivotPoint
    {
        get => Has<PivotPoint>() ? Get<PivotPoint>().Value : Vector2.Zero;
        set
        {
            if (!Precision.AreEqual(PivotPoint, value))
                Set(new PivotPoint { Value = value }, false);
        }
    }

    public readonly int ZIndex
    {
        get => Has<ZIndex>() ? Get<ZIndex>().Value : 0;
        set
        {
            if (ZIndex != value)
                Set(new ZIndex { Value = value }, false);
        }
    }

    public readonly Transform WorldTransform
    {
        get
        {
            var transform = Transform;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                transform += entity.Transform;
            return transform;
        }
    }

    public readonly Vector2 WorldPosition
    {
        get
        {
            var position = Position;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                position += entity.Position;
            return position;
        }
    }

    public readonly Vector2 WorldScale
    {
        get
        {
            var scale = Scale;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                scale *= entity.Scale;
            return scale;
        }
    }

    public readonly float WorldRotation
    {
        get
        {
            var rotation = Rotation;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                rotation += entity.Rotation;
            return rotation;
        }
    }

    public readonly Vector2 WorldPivotPoint
    {
        get
        {
            var pivotPoint = PivotPoint;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                pivotPoint += entity.PivotPoint;
            return pivotPoint;
        }
    }

    public readonly int WorldZIndex
    {
        get
        {
            var zIndex = ZIndex;
            for (var entity = Parent; entity.Valid; entity = entity.Parent)
                zIndex += entity.ZIndex;
            return zIndex;
        }
    }

    public readonly Components Components => _entity.Has<Components>() ? _entity.Get<Components>() : Components.Empty;

    public static bool operator ==(Entity a, Entity b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Entity entity && Equals(entity);
    }

    public readonly bool Equals(Entity other)
    {
        return _entity == other._entity;
    }

    public override readonly int GetHashCode()
    {
        return _entity.GetHashCode();
    }

    public readonly ref readonly Entity SetTransform(Transform transform)
    {
        Transform = transform;
        return ref this;
    }

    public readonly ref readonly Entity SetPosition(float v1, float? v2 = null)
    {
        Position = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public readonly ref readonly Entity SetPosition(Vector2 position)
    {
        Position = position;
        return ref this;
    }

    public readonly ref readonly Entity SetScale(float v1, float? v2 = null)
    {
        Scale = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public readonly ref readonly Entity SetScale(Vector2 scale)
    {
        Scale = scale;
        return ref this;
    }

    public readonly ref readonly Entity SetRotation(float rotation)
    {
        Rotation = rotation;
        return ref this;
    }

    public readonly ref readonly Entity SetPivotPoint(float v1, float? v2 = null)
    {
        PivotPoint = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public readonly ref readonly Entity SetPivotPoint(Vector2 pivotPoint)
    {
        PivotPoint = pivotPoint;
        return ref this;
    }

    public readonly ref readonly Entity SetZIndex(int zIndex)
    {
        ZIndex = zIndex;
        return ref this;
    }

    public readonly T Get<T>()
    {
        return _entity.Get<T>();
    }

    public readonly ref readonly Entity Set<T>(T data)
    {
        Set(data, true);
        return ref this;
    }

    public readonly ref readonly Entity Set<T>(ref T data)
    {
        Set(data, true);
        return ref this;
    }

    private ref readonly Entity Set<T>(T data, bool updateComponents)
    {
        Set(ref data, updateComponents);
        return ref this;
    }

    private ref readonly Entity Set<T>(ref T data, bool updateComponents)
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

    public readonly ref readonly Entity Remove<T>()
    {
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be removed.");
        Scene.DeferRemoveComponent(_entity, type);
        _entity.Remove<T>();
        return ref this;
    }

    public readonly void Destroy()
    {
        _entity.Destruct();
    }

    public readonly ref readonly Entity Scope(Action action)
    {
        Scene.DeferBegin();
        _entity.Scope(action);
        Scene.DeferEnd();
        return ref this;
    }

    public readonly ref readonly Entity ChildOf(Entity parent)
    {
        _entity.ChildOf(parent._entity);
        return ref this;
    }

    public readonly bool IsChildOf(Entity parent)
    {
        return _entity.Has(Ecs.ChildOf, parent._entity);
    }

    public readonly IEnumerable<Entity> Children => new ChildEnumerator(this);

    #region Traverse

    public readonly ref readonly Entity Traverse(Action<Entity> action)
    {
        action.Invoke(this);
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public readonly ref readonly Entity Traverse<T>(Action<Entity> action)
    {
        if (Has<T>())
            action.Invoke(this);
        foreach (var child in Children)
            child.Traverse<T>(action);
        return ref this;
    }

    public readonly ref readonly Entity Traverse<T>(Action<T> action)
    {
        if (Has<T>())
            action.Invoke(Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public readonly ref readonly Entity Traverse<T>(Action<Entity, T> action)
    {
        if (Has<T>())
            action.Invoke(this, Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    #endregion

    public readonly bool TryGet<T0>(out T0 t)
    {
        var result = Has<T0>();
        t = default!;
        if (result)
            t = Get<T0>();
        return result;
    }

    private class ChildEnumerator : IEnumerator<Entity>, IEnumerable<Entity>
    {
        private Entity _entity;
        private int _index;
        private flecs.ecs_iter_t _iter;

        public ChildEnumerator(Entity entity)
        {
            _entity = entity;
            Reset();
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_index < _iter.count)
            {
                _index++;
                if (_index < _iter.count)
                    return true;
            }

            _index = 0;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                return Utils.Bool(flecs.ecs_each_next(iter));
            }
        }

        public void Reset()
        {
            _entity.Scene.DeferBegin();
            _iter = flecs.ecs_each_id(_entity._entity.World, Ecs.Pair(flecs.EcsChildOf, _entity.Id));
            _index = 0;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Ecs.TableLock(iter);
            }
        }

        public Entity Current
        {
            get
            {
                if (_iter == default)
                    return Null;
                var entity = new Flecs.NET.Core.Entity(_entity._entity.World, _iter.entities[_index]);
                return new Entity(entity, _entity.Scene);
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            if (_iter == default)
                return;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Ecs.TableUnlock(iter);
            }

            _entity.Scene.DeferEnd();
            _entity = Null;
            _iter = default;
            _index = 0;
        }
    }
}
