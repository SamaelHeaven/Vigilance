#pragma warning disable CS9084

using System.Runtime.CompilerServices;
using System.Text;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Flecs.NET.Utilities;
using Vigilance.Math;

// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
#pragma warning disable CS8656 // Call to non-readonly member from a 'readonly' member results in an implicit copy.

namespace Vigilance.Core;

public readonly unsafe partial record struct Entity : IComparable<Entity>
{
    public const ulong RecycledIdFlag = 0x7FFFFFFF;
    private readonly Flecs.NET.Core.Entity _entity;

    internal Entity(Flecs.NET.Core.Entity entity, Scene scene)
    {
        _entity = entity;
        Scene = scene;
    }

    public static Entity Null { get; } = new(Flecs.NET.Core.Entity.Null(), null!);
    public Scene Scene { get; }

    public ulong Id => _entity.Id.Value;

    public string Name
    {
        get
        {
            EnsureValid();
            return _entity.Name();
        }
    }

    public bool Valid => _entity.IsValid();

    public Entity Parent
    {
        get
        {
            EnsureValid();
            return new Entity(_entity.Parent(), Scene);
        }
    }

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
        get => Has<Position>() ? _entity.Get<Position>().Value : Vector2.Zero;
        set
        {
            EnsureValid();
            if (!Precision.AreEqual(Position, value))
                Set(new Position(value), false);
        }
    }

    public Vector2 Scale
    {
        get => Has<Scale>() ? _entity.Get<Scale>().Value : Vector2.One;
        set
        {
            EnsureValid();
            if (!Precision.AreEqual(Scale, value))
                Set(new Scale(value), false);
        }
    }

    public float Rotation
    {
        get => Has<Rotation>() ? _entity.Get<Rotation>().Value : 0;
        set
        {
            EnsureValid();
            if (!Precision.AreEqual(Rotation, value))
                Set(new Rotation(value), false);
        }
    }

    public Vector2 PivotPoint
    {
        get => Has<PivotPoint>() ? _entity.Get<PivotPoint>().Value : Vector2.Zero;
        set
        {
            EnsureValid();
            if (!Precision.AreEqual(PivotPoint, value))
                Set(new PivotPoint(value), false);
        }
    }

    public int ZIndex
    {
        get => Has<ZIndex>() ? _entity.Get<ZIndex>().Value : 0;
        set
        {
            if (ZIndex != value)
                Set(new ZIndex(value), false);
        }
    }

    public bool Disabled
    {
        get => !Valid || _entity.Has(Ecs.Disabled);
        set
        {
            if (Valid)
                flecs.ecs_enable(_entity.World, _entity.Id, value ? (byte)0 : (byte)1);
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

    public Components Components => Has<Components>() ? _entity.Get<Components>() : Components.Empty;

    public ChildEnumerable Children => new(this);

    public int CompareTo(Entity other)
    {
        return Compare(this, other, Id, other.Id);
    }

    public bool Equals(Entity other)
    {
        return Id == other.Id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Compare(Entity e1, Entity e2, ulong id1, ulong id2)
    {
        var result = e1.WorldZIndex.CompareTo(e2.WorldZIndex);
        return result == 0 ? (id1 & RecycledIdFlag).CompareTo(id2 & RecycledIdFlag) : result;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public ref readonly Entity SetTransform(Transform transform)
    {
        Transform = transform;
        return ref this;
    }

    public ref readonly Entity SetPosition(float v1, float? v2 = null)
    {
        Position = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetPosition(Vector2 position)
    {
        Position = position;
        return ref this;
    }

    public ref readonly Entity SetScale(float v1, float? v2 = null)
    {
        Scale = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetScale(Vector2 scale)
    {
        Scale = scale;
        return ref this;
    }

    public ref readonly Entity SetRotation(float rotation)
    {
        Rotation = rotation;
        return ref this;
    }

    public ref readonly Entity SetPivotPoint(float v1, float? v2 = null)
    {
        PivotPoint = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetPivotPoint(Vector2 pivotPoint)
    {
        PivotPoint = pivotPoint;
        return ref this;
    }

    public ref readonly Entity SetZIndex(int zIndex)
    {
        ZIndex = zIndex;
        return ref this;
    }

    public ref readonly Entity SetDisabled(bool disabled = true)
    {
        Disabled = disabled;
        return ref this;
    }

    public T Get<T>()
    {
        EnsureValid();
        return _entity.Get<T>();
    }

    public ref readonly Entity Set<T>(T data)
    {
        Set(data, true);
        return ref this;
    }

    public ref readonly Entity Set<T>(ref T data)
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
        EnsureValid();
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

    public ref readonly Entity Remove<T>()
    {
        EnsureValid();
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be removed.");
        Scene.DeferRemoveComponent(_entity, type);
        _entity.Remove<T>();
        return ref this;
    }

    public void Destroy()
    {
        EnsureValid();
        _entity.Destruct();
    }

    public ref readonly Entity Scope(Action action)
    {
        EnsureValid();
        Scene.BeginDefer();
        _entity.Scope(action);
        Scene.EndDefer();
        return ref this;
    }

    public ref readonly Entity ChildOf(Entity parent)
    {
        EnsureValid();
        _entity.ChildOf(parent._entity);
        return ref this;
    }

    public bool IsChildOf(Entity parent)
    {
        EnsureValid();
        return _entity.Has(Ecs.ChildOf, parent.Id);
    }

    public void EnsureValid()
    {
        if (!_entity.IsValid())
            throw new InvalidOperationException("Entity is not valid.");
    }

    private bool PrintMembers(StringBuilder sb)
    {
        sb.Append("Id = ");
        sb.Append(Id);
        if (!Valid)
        {
            sb.Append(", Valid = ");
            sb.Append(Valid);
            return true;
        }

        var name = Name;
        if (name != "")
        {
            sb.Append(", Name = ");
            sb.Append(Name);
        }

        if (Disabled)
        {
            sb.Append(", Disabled = ");
            sb.Append(Disabled);
        }

        sb.Append(", Transform = ");
        sb.Append(Transform.ToString());
        sb.Append(", Components = ");
        sb.Append(Components.ToString());
        return true;
    }

    public readonly struct ChildEnumerable : IValueEnumerable<ChildEnumerator, Entity>
    {
        private readonly Entity _entity;

        internal ChildEnumerable(Entity entity)
        {
            _entity = entity;
        }

        public ChildEnumerator GetEnumerator()
        {
            return new ChildEnumerator(_entity);
        }
    }

    public struct ChildEnumerator : IValueEnumerator<Entity>
    {
        private readonly Entity _entity;
        private flecs.ecs_iter_t _iter;
        private int _index;

        internal ChildEnumerator(Entity entity)
        {
            _entity = entity;
            Reset();
        }

        public bool MoveNext()
        {
            if (_iter.world == null)
                return false;
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
            _entity.EnsureValid();
            Dispose();
            _entity.Scene.BeginDefer();
            _iter = flecs.ecs_each_id(_entity._entity.World, Ecs.Pair(flecs.EcsChildOf, _entity.Id));
            _index = 0;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Ecs.TableLock(iter);
            }
        }

        public readonly Entity Current
        {
            get
            {
                if (_iter.world == null)
                    return Null;
                var entity = new Flecs.NET.Core.Entity(_entity._entity.World, _iter.entities[_index]);
                return new Entity(entity, _entity.Scene);
            }
        }

        public void Dispose()
        {
            if (_iter.world == null)
                return;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Ecs.TableUnlock(iter);
            }

            _entity.Scene.EndDefer();
            _iter = default;
            _index = 0;
        }
    }

    #region Traverse

    public ref readonly Entity Traverse(Action<Entity> action)
    {
        EnsureValid();
        action.Invoke(this);
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<Entity> action)
    {
        EnsureValid();
        if (_entity.Has<T>())
            action.Invoke(this);
        foreach (var child in Children)
            child.Traverse<T>(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<T> action)
    {
        EnsureValid();
        if (_entity.Has<T>())
            action.Invoke(_entity.Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<Entity, T> action)
    {
        EnsureValid();
        if (_entity.Has<T>())
            action.Invoke(this, _entity.Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    #endregion
}
