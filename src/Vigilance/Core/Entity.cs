#pragma warning disable CS9084

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
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
            return Scene.Cache.NameMap[Id];
        }
    }

    public string Path
    {
        get
        {
            EnsureValid();
            var name = Name;
            var parent = Parent;
            while (!parent.IsNull)
            {
                name = $"{parent.Name}.{name}";
                parent = parent.Parent;
            }

            return name;
        }
    }

    public bool IsValid => Scene.Cache.TransformMap.ContainsKey(Id);

    public bool IsNull => Id == 0;

    public Entity Parent
    {
        get
        {
            EnsureValid();
            return Scene.Cache.ParentMap.GetValueOrDefault(Id, Null);
        }
    }

    public Transform Transform
    {
        get
        {
            EnsureValid();
            return Scene.Cache.TransformMap[Id];
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
        get => GetOrDefault(new Position()).Value;
        set
        {
            EnsureValid();
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
                Scene.Cache.ImmediatePositionMap,
                Id,
                out _
            );
            if (Precision.AreEqual(value, current))
                return;
            current = value;
            Scene.DeferSetPosition(Id, value);
            SetInternal(new Position(value));
        }
    }

    public Vector2 Scale
    {
        get => GetOrDefault(new Scale()).Value;
        set
        {
            EnsureValid();
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
                Scene.Cache.ImmediateScaleMap,
                Id,
                out var exists
            );
            if (!exists)
                current = Vector2.One;
            if (Precision.AreEqual(value, current))
                return;
            current = value;
            Scene.DeferSetScale(Id, value);
            SetInternal(new Scale(value));
        }
    }

    public float Rotation
    {
        get => GetOrDefault(new Rotation()).Value;
        set
        {
            EnsureValid();
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
                Scene.Cache.ImmediateRotationMap,
                Id,
                out _
            );
            if (Precision.AreEqual(value, current))
                return;
            current = value;
            Scene.DeferSetRotation(Id, value);
            SetInternal(new Rotation(value));
        }
    }

    public Vector2 PivotPoint
    {
        get => GetOrDefault(new PivotPoint()).Value;
        set
        {
            EnsureValid();
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
                Scene.Cache.ImmediatePivotPointMap,
                Id,
                out _
            );
            if (Precision.AreEqual(value, current))
                return;
            current = value;
            Scene.DeferSetPivotPoint(Id, value);
            SetInternal(new PivotPoint(value));
        }
    }

    public int ZIndex
    {
        get => GetOrDefault(new ZIndex()).Value;
        set
        {
            EnsureValid();
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(Scene.Cache.ImmediateZIndexMap, Id, out _);
            if (Precision.AreEqual(value, current))
                return;
            current = value;
            SetInternal(new ZIndex(value));
        }
    }

    public bool IsDisabled
    {
        get
        {
            EnsureValid();
            return FlecsEntity.Has(Flecs.NET.Core.Ecs.Disabled);
        }
        set
        {
            EnsureValid();
            if (value)
            {
                if (!Scene.Cache.ImmediateDisabledSet.Add(Id))
                    return;
            }
            else
            {
                if (!Scene.Cache.ImmediateDisabledSet.Remove(Id))
                    return;
            }

            var flecsEntity = FlecsEntity;
            flecs.ecs_enable(flecsEntity.World, flecsEntity.Id, value);
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
            if (Scene.IsRuntimeComponentsEnabled)
            {
                var flecsEntity = FlecsEntity;
                ref readonly var components = ref flecsEntity.GetSafe<Components>();
                return Unsafe.IsNullRef(in components) ? Components.Empty : components;
            }

            Logger.Warning("ECS: Runtime components are disabled");
            return Components.Empty;
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
        if (Type<T>.IsTag)
            return FlecsEntity.Has<T>() ? default! : Unsafe.NullRef<T>();
        return FlecsEntity.Get<T>();
    }

    public ref readonly T GetRef<T>()
    {
        EnsureValid();
        return ref FlecsEntity.GetSafe<T>();
    }

    public bool TryGet<T>(out T value)
    {
        EnsureValid();
        Unsafe.SkipInit(out value);
        var flecsEntity = FlecsEntity;
        if (Type<T>.IsTag)
        {
            if (!flecsEntity.Has<T>())
                return false;
            value = default!;
            return true;
        }

        ref readonly var data = ref flecsEntity.GetSafe<T>();
        if (Unsafe.IsNullRef(in data))
            return false;
        value = data;
        return true;
    }

    public T GetOrDefault<T>(T defaultValue)
    {
        EnsureValid();
        if (Type<T>.IsTag)
            return FlecsEntity.Has<T>() ? default! : defaultValue;
        ref readonly var value = ref FlecsEntity.GetSafe<T>();
        return Unsafe.IsNullRef(in value) ? defaultValue : value;
    }

    public T GetOrDefault<T>(ref T defaultValue)
    {
        EnsureValid();
        if (Type<T>.IsTag)
            return FlecsEntity.Has<T>() ? default! : defaultValue;
        ref readonly var value = ref FlecsEntity.GetSafe<T>();
        return Unsafe.IsNullRef(in value) ? defaultValue : value;
    }

    public T GetOrDefault<T>(Func<T> defaultFunc)
    {
        EnsureValid();
        if (Type<T>.IsTag)
            return FlecsEntity.Has<T>() ? default! : defaultFunc.Invoke();
        ref readonly var value = ref FlecsEntity.GetSafe<T>();
        return Unsafe.IsNullRef(in value) ? defaultFunc.Invoke() : value;
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
        var flecsEntity = FlecsEntity;
        var id = Type<T>.Id(flecsEntity.World);
        if (id == Scene.Cache.ComponentsType)
            throw new InvalidOperationException("Components cannot be removed.");
        if (Scene.IsRuntimeComponentsEnabled)
            Scene.DeferRemoveComponent(this, id);
        flecsEntity.Remove(id);
        return ref this;
    }

    public ref readonly Entity Remove(in Component component)
    {
        EnsureValid();
        var flecsEntity = FlecsEntity;
        if (Scene.IsRuntimeComponentsEnabled)
            Scene.DeferRemoveComponent(this, component.Id);
        flecsEntity.Remove(component.Id);
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
        var previousScope = Scene.SetScope(this);
        try
        {
            action.Invoke();
        }
        finally
        {
            Scene.SetScope(previousScope);
        }

        return ref this;
    }

    public ref readonly Entity Scope(Action<Scene> action)
    {
        EnsureValid();
        var previousScope = Scene.SetScope(this);
        try
        {
            action.Invoke(Scene);
        }
        finally
        {
            Scene.SetScope(previousScope);
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
        return FlecsEntity.Has(Flecs.NET.Core.Ecs.ChildOf, parent.Id);
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
        if (name != $"#{Id}")
        {
            sb.Append(", Name = ");
            sb.Append(Name);
        }

        var path = Path;
        if (path != name)
        {
            sb.Append(", Path = ");
            sb.Append(Path);
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

        Components components;
        if (!Scene.IsRuntimeComponentsEnabled || (components = Components).Count == 0)
            return true;
        sb.Append(", Components = ");
        sb.Append(components.ToString());
        return true;
    }

    private ref readonly Entity SetInternal<T>(T data)
    {
        var flecsEntity = FlecsEntity;
        flecsEntity.Set(ref data);
        flecsEntity.CsWorld().Event<SetEvent>().Id<T>().Entity(Id).Enqueue();
        return ref this;
    }

    public struct ChildEnumerable : IStructEnumerable<ChildEnumerator, Entity>
    {
        private readonly Entity _entity;
        private bool _deferred;

        internal ChildEnumerable(Entity entity)
        {
            _entity = entity;
            _deferred = true;
        }

        public ChildEnumerator GetEnumerator()
        {
            return new ChildEnumerator(_entity, _deferred);
        }

        public ValueEnumerable<StructEnumerator<ChildEnumerator, Entity>, Entity> AsValueEnumerable()
        {
            return new StructEnumerator<ChildEnumerator, Entity>(GetEnumerator());
        }

        public ref ChildEnumerable Deferred(bool deferred = true)
        {
            _deferred = deferred;
            return ref this;
        }
    }

    public struct ChildEnumerator : IStructEnumerator<Entity>
    {
        private readonly Entity _entity;
        private readonly bool _deferred;
        private flecs.ecs_iter_t _iter;
        private int _index;

        internal ChildEnumerator(Entity entity, bool deferred)
        {
            _entity = entity;
            _deferred = deferred;
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
                return flecs.ecs_each_next(iter);
            }
        }

        public void Reset()
        {
            _entity.EnsureValid();
            Dispose();
            if (_deferred)
                _entity.Scene.BeginDefer();
            _iter = flecs.ecs_each_id(_entity.Scene.World, Flecs.NET.Core.Ecs.Pair(flecs.EcsChildOf, _entity.Id));
            _index = 0;
            fixed (flecs.ecs_iter_t* iter = &_iter)
            {
                Flecs.NET.Core.Ecs.TableLock(iter);
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
                Flecs.NET.Core.Ecs.TableUnlock(iter);
            }

            if (_deferred)
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
        if (TryGet(out T t))
            action.Invoke(t);
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    public ref readonly Entity Traverse<T>(Action<Entity, T> action)
    {
        EnsureValid();
        if (TryGet(out T t))
            action.Invoke(this, t);
        foreach (var child in Children)
            child.Traverse(action);
        return ref this;
    }

    #endregion
}

public static unsafe class EntityExtensions
{
    extension(in Entity entity)
    {
        public ref readonly Entity Set<T>(T data)
        {
            Set(entity, ref data);
            return ref entity;
        }

        public ref readonly Entity Set<T>(ref T data)
        {
            entity.EnsureValid();
            var type = typeof(T);
            var flecsEntity = entity.FlecsEntity;
            var id = Type<T>.Id(flecsEntity.World);
            if (id == entity.Scene.Cache.ComponentsType)
                throw new InvalidOperationException("Components cannot be set.");
            var hadT = flecsEntity.Has(id);
            if (entity.Scene.IsRuntimeComponentsEnabled)
                entity.Scene.DeferSetComponent(entity, type, data, id);
            var isTag = Type<T>.IsTag;
            if (!isTag)
                flecsEntity.Set(ref data);
            else
                flecsEntity.Add<T>();
            if (!isTag && hadT)
                flecsEntity.CsWorld().Event<SetEvent>().Id(id).Entity(entity.Id).Enqueue();
            else if (!hadT)
                flecsEntity.CsWorld().Event<AddEvent>().Id(id).Entity(entity.Id).Enqueue();
            return ref entity;
        }
    }

    extension(Flecs.NET.Core.Entity entity)
    {
        public ref readonly T GetSafe<T>()
        {
            var data = flecs.ecs_get_id(entity.World, entity.Id, Type<T>.Id(entity.World));
            if (data == null)
                return ref Unsafe.NullRef<T>();
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                return ref Unsafe.AsRef<T>(data);
            var handle = GCHandle.FromIntPtr(*(nint*)data);
            var box = (StrongBox<T>)handle.Target!;
            return ref box.Value!;
        }
    }
}
