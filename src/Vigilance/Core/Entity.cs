#pragma warning disable CS9084

using System.Diagnostics;
using System.Text;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Flecs.NET.Utilities;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
#pragma warning disable CS8656 // Call to non-readonly member from a 'readonly' member results in an implicit copy.

namespace Vigilance.Core;

public readonly unsafe partial record struct Entity : IComparable<Entity>
{
    public const ulong RecycledIdMask = 0x7FFFFFFF;

    public Entity(ulong id, Scene scene)
    {
        Id = id;
        Scene = scene;
    }

    public static Entity Null => new(0, null!);
    public ulong Id { get; }
    public Scene Scene { get; }

    internal Flecs.NET.Core.Entity FlecsEntity => new(Scene.World, Id);

    public string Name
    {
        get
        {
            EnsureValid();
            return Scene.NameMap.GetValueOrDefault(Id, "");
        }
    }

    public bool IsValid => Scene.ZIndexMap.ContainsKey(Id);

    public bool IsNull => Id == 0;

    public Entity Parent
    {
        get
        {
            EnsureValid();
            return Scene.ParentMap.GetValueOrDefault(Id, Null);
        }
    }

    public Transform Transform
    {
        get
        {
            EnsureValid();
            return Scene.TransformMap.GetValueOrDefault(Id, new Transform());
        }
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
        get
        {
            EnsureValid();
            return Scene.PositionMap.GetValueOrDefault(Id, Vector2.Zero);
        }
        set
        {
            EnsureValid();
            if (Precision.AreEqual(value, Scene.ImmediatePositionMap.GetValueOrDefault(Id, Vector2.Zero)))
                return;
            Scene.ImmediatePositionMap[Id] = value;
            SetInternal(new Position(value));
        }
    }

    public Vector2 Scale
    {
        get
        {
            EnsureValid();
            return Scene.ScaleMap.GetValueOrDefault(Id, Vector2.One);
        }
        set
        {
            EnsureValid();
            if (Precision.AreEqual(value, Scene.ImmediateScaleMap.GetValueOrDefault(Id, Vector2.One)))
                return;
            Scene.ImmediateScaleMap[Id] = value;
            SetInternal(new Scale(value));
        }
    }

    public float Rotation
    {
        get
        {
            EnsureValid();
            return Scene.RotationMap.GetValueOrDefault(Id, 0);
        }
        set
        {
            EnsureValid();
            if (Precision.AreEqual(value, Scene.ImmediateRotationMap.GetValueOrDefault(Id, 0)))
                return;
            Scene.ImmediateRotationMap[Id] = value;
            SetInternal(new Rotation(value));
        }
    }

    public Vector2 PivotPoint
    {
        get
        {
            EnsureValid();
            return Scene.PivotPointMap.GetValueOrDefault(Id, Vector2.Zero);
        }
        set
        {
            EnsureValid();
            if (Precision.AreEqual(value, Scene.ImmediatePivotPointMap.GetValueOrDefault(Id, Vector2.Zero)))
                return;
            Scene.ImmediatePivotPointMap[Id] = value;
            SetInternal(new PivotPoint(value));
        }
    }

    public int ZIndex
    {
        get
        {
            EnsureValid();
            return Scene.ZIndexMap.GetValueOrDefault(Id, 0);
        }
        set
        {
            EnsureValid();
            if (value == Scene.ImmediateZIndexMap.GetValueOrDefault(Id, 0))
                return;
            Scene.ImmediateZIndexMap[Id] = value;
            SetInternal(new ZIndex(value));
        }
    }

    public bool IsDisabled
    {
        get
        {
            EnsureValid();
            return Scene.DisabledSet.Contains(Id);
        }
        set
        {
            EnsureValid();
            if (Scene.ImmediateDisabledSet.Contains(Id) == value)
                return;
            if (value)
                Scene.ImmediateDisabledSet.Add(Id);
            else
                Scene.ImmediateDisabledSet.Remove(Id);
            var entity = FlecsEntity;
            flecs.ecs_enable(entity.World, entity.Id, value ? (byte)0 : (byte)1);
        }
    }

    public Transform WorldTransform
    {
        get
        {
            var transform = Transform;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                transform += entity.Transform;
            return transform;
        }
    }

    public Vector2 WorldPosition
    {
        get
        {
            var position = Position;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                position += entity.Position;
            return position;
        }
    }

    public Vector2 WorldScale
    {
        get
        {
            var scale = Scale;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                scale *= entity.Scale;
            return scale;
        }
    }

    public float WorldRotation
    {
        get
        {
            var rotation = Rotation;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                rotation += entity.Rotation;
            return rotation;
        }
    }

    public Vector2 WorldPivotPoint
    {
        get
        {
            var pivotPoint = PivotPoint;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                pivotPoint += entity.PivotPoint;
            return pivotPoint;
        }
    }

    public int WorldZIndex
    {
        get
        {
            var zIndex = ZIndex;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                zIndex += entity.ZIndex;
            return zIndex;
        }
    }

    public Components Components
    {
        get
        {
            EnsureValid();
            return Scene.ComponentMap.GetValueOrDefault(Id, Components.Empty);
        }
    }

    public ChildEnumerable Children => new(this);

    public ulong Order => ((ulong)(uint)(WorldZIndex ^ int.MinValue) << 32) | (Id & RecycledIdMask);

    public int CompareTo(Entity other)
    {
        return Order.CompareTo(other.Order);
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
        IsDisabled = disabled;
        return ref this;
    }

    public T Get<T>()
    {
        EnsureValid();
        return FlecsEntity.Get<T>();
    }

    public ref readonly Entity Set<T>(IComposable<T> composable)
    {
        EnsureValid();
        this.Set(composable.ToComponent());
        return ref this;
    }

    public ref readonly Entity Remove<T>()
    {
        EnsureValid();
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be removed.");
        var entity = FlecsEntity;
        var id = Type<T>.Id(entity.World);
        Scene.DeferRemoveComponent(this, id);
        entity.Remove(id);
        return ref this;
    }

    public ref readonly Entity Remove(in Component component)
    {
        EnsureValid();
        var entity = FlecsEntity;
        Scene.DeferRemoveComponent(this, component.Id);
        entity.Remove(component.Id);
        return ref this;
    }

    public void Destroy()
    {
        EnsureValid();
        FlecsEntity.Destruct();
    }

    public ref readonly Entity Scope(Action action)
    {
        EnsureValid();
        Scene.BeginDefer();
        try
        {
            FlecsEntity.Scope(action);
        }
        finally
        {
            Scene.EndDefer();
        }

        return ref this;
    }

    public ref readonly Entity ChildOf(Entity parent)
    {
        EnsureValid();
        FlecsEntity.ChildOf(parent.Id);
        return ref this;
    }

    public bool IsChildOf(Entity parent)
    {
        EnsureValid();
        return FlecsEntity.Has(Ecs.ChildOf, parent.Id);
    }

    [Conditional("DEBUG")]
    public void EnsureValid()
    {
        if (!IsValid)
            Logger.Fatal($"Entity is not valid.\n{new StackTrace(true).ToString().TrimEnd()}");
    }

    private bool PrintMembers(StringBuilder sb)
    {
        if (Id == 0)
        {
            sb.Append("Null");
            return true;
        }

        sb.Append("Id = ");
        sb.Append(Id);
        if (!IsValid)
        {
            sb.Append(", Valid = ");
            sb.Append(IsValid);
            return true;
        }

        var name = Name;
        if (name != "")
        {
            sb.Append(", Name = ");
            sb.Append(Name);
        }

        if (IsDisabled)
        {
            sb.Append(", Disabled = ");
            sb.Append(IsDisabled);
        }

        sb.Append(", ZIndex = ");
        sb.Append(ZIndex);
        sb.Append(", Transform = ");
        sb.Append(Transform.ToString());
        sb.Append(", Components = ");
        sb.Append(Components.ToString());
        return true;
    }

    private ref readonly Entity SetInternal<T>(T data)
    {
        var entity = FlecsEntity;
        entity.Set(ref data);
        entity.CsWorld().Event<SetEvent>().Id<T>().Entity(Id).Enqueue();
        return ref this;
    }

    public readonly struct ChildEnumerable : IStructEnumerable<ChildEnumerator, Entity>
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

        public ValueEnumerable<StructEnumerator<ChildEnumerator, Entity>, Entity> AsValueEnumerable()
        {
            return new StructEnumerator<ChildEnumerator, Entity>(GetEnumerator());
        }
    }

    public struct ChildEnumerator : IStructEnumerator<Entity>
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
            _iter = flecs.ecs_each_id(_entity.Scene.World, Ecs.Pair(flecs.EcsChildOf, _entity.Id));
            _index = 0;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Ecs.TableLock(iter);
            }
        }

        public readonly Entity Current =>
            _iter.world == null ? Null : new Entity(_iter.entities[_index], _entity.Scene);

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
        if (FlecsEntity.Has<T>())
            action.Invoke(this);
        foreach (var child in Children)
            child.Traverse<T>(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<T> action)
    {
        EnsureValid();
        var entity = FlecsEntity;
        if (entity.Has<T>())
            action.Invoke(entity.Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<Entity, T> action)
    {
        EnsureValid();
        var entity = FlecsEntity;
        if (entity.Has<T>())
            action.Invoke(this, entity.Get<T>());
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    #endregion
}

public static unsafe class EntityExtensions
{
    public static ref readonly Entity Set<T>(in this Entity entity, T data)
    {
        Set(entity, ref data);
        return ref entity;
    }

    public static ref readonly Entity Set<T>(in this Entity entity, ref T data)
    {
        entity.EnsureValid();
        var type = typeof(T);
        if (type == typeof(Components))
            throw new InvalidOperationException("Components cannot be set.");
        var flecsEntity = entity.FlecsEntity;
        var id = Type<T>.Id(flecsEntity.World);
        var hadT = flecsEntity.Has(id);
        entity.Scene.DeferSetComponent(entity, type, data, id);
        flecsEntity.Set(ref data);
        if (hadT)
            flecsEntity.CsWorld().Event<SetEvent>().Id(id).Entity(entity.Id).Enqueue();
        else
            flecsEntity.CsWorld().Event<AddEvent>().Id(id).Entity(entity.Id).Enqueue();
        return ref entity;
    }
}
