#pragma warning disable CS9084

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Vigilance.Collections;
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

    public string Path => this.AncestorsAndSelf().Select(e => e.Name).Reverse().JoinToString(".");

    public bool IsValid => Scene?.Cache.TransformMap.ContainsKey(Id) ?? false;

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
        get
        {
            EnsureValid();
            return FlecsEntity.Get<Position>().Value;
        }
        set
        {
            EnsureValid();
            var flecsEntity = FlecsEntity;
            ref var position = ref flecsEntity.GetSafe<Position>();
            if (Precision.AreEqual(value, position.Value))
                return;
            position.Value = value;
            ref var transform = ref CollectionsMarshal.GetValueRefOrNullRef(Scene.Cache.TransformMap, Id);
            transform.Position = value;
            flecsEntity.CsWorld().Event<SetEvent>().Id<Position>().Entity(Id).Enqueue();
        }
    }

    public Vector2 Scale
    {
        get
        {
            EnsureValid();
            return FlecsEntity.Get<Scale>().Value;
        }
        set
        {
            EnsureValid();
            var flecsEntity = FlecsEntity;
            ref var scale = ref flecsEntity.GetSafe<Scale>();
            if (Precision.AreEqual(value, scale.Value))
                return;
            scale.Value = value;
            ref var transform = ref CollectionsMarshal.GetValueRefOrNullRef(Scene.Cache.TransformMap, Id);
            transform.Scale = value;
            flecsEntity.CsWorld().Event<SetEvent>().Id<Scale>().Entity(Id).Enqueue();
        }
    }

    public float Rotation
    {
        get
        {
            EnsureValid();
            return FlecsEntity.Get<Rotation>().Value;
        }
        set
        {
            EnsureValid();
            var flecsEntity = FlecsEntity;
            ref var rotation = ref flecsEntity.GetSafe<Rotation>();
            if (Precision.AreEqual(value, rotation.Value))
                return;
            rotation.Value = value;
            ref var transform = ref CollectionsMarshal.GetValueRefOrNullRef(Scene.Cache.TransformMap, Id);
            transform.Rotation = value;
            flecsEntity.CsWorld().Event<SetEvent>().Id<Rotation>().Entity(Id).Enqueue();
        }
    }

    public Vector2 PivotPoint
    {
        get
        {
            EnsureValid();
            return FlecsEntity.Get<PivotPoint>().Value;
        }
        set
        {
            EnsureValid();
            var flecsEntity = FlecsEntity;
            ref var pivotPoint = ref flecsEntity.GetSafe<PivotPoint>();
            if (Precision.AreEqual(value, pivotPoint.Value))
                return;
            pivotPoint.Value = value;
            ref var transform = ref CollectionsMarshal.GetValueRefOrNullRef(Scene.Cache.TransformMap, Id);
            transform.PivotPoint = value;
            flecsEntity.CsWorld().Event<SetEvent>().Id<PivotPoint>().Entity(Id).Enqueue();
        }
    }

    public int ZIndex
    {
        get
        {
            EnsureValid();
            return FlecsEntity.Get<ZIndex>().Value;
        }
        set
        {
            EnsureValid();
            var flecsEntity = FlecsEntity;
            ref var zIndex = ref flecsEntity.GetSafe<ZIndex>();
            if (value == zIndex.Value)
                return;
            zIndex.Value = value;
            flecsEntity.CsWorld().Event<SetEvent>().Id<ZIndex>().Entity(Id).Enqueue();
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

    public Components Components => new(this);

    public ChildEnumerable Children => new(this);

    public ulong Order => ((ulong)(uint)(WorldZIndex ^ int.MinValue) << 32) | (Id & RecycledIdMask);

    public int CompareTo(Entity other)
    {
        return Order.CompareTo(other.Order);
    }

    public ref readonly Entity SetTransform(in Transform transform)
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

    public object? Get(in Component component)
    {
        EnsureValid();
        var metadata = component.Metadata;
        if (metadata.IsTag)
            return FlecsEntity.Has(component.Id) ? metadata.DefaultFunc.Invoke() : null;
        var ptr = flecs.ecs_get_id(Scene.World, Id, component.Id);
        return metadata.FromPointerFunc.Invoke((nint)ptr);
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

    public bool TryGet(in Component component, out object value)
    {
        EnsureValid();
        value = null!;
        var metadata = component.Metadata;
        var flecsEntity = FlecsEntity;
        if (metadata.IsTag)
        {
            if (!flecsEntity.Has(component.Id))
                return false;
            value = metadata.DefaultFunc.Invoke()!;
            return true;
        }

        var ptr = flecs.ecs_get_id(Scene.World, Id, component.Id);
        var data = metadata.FromPointerFunc.Invoke((nint)ptr);
        if (data is null)
            return false;
        value = data;
        return true;
    }

    public T GetOrDefault<T>(in T defaultValue)
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

    public object? GetOrDefault(in Component component, object? defaultValue)
    {
        EnsureValid();
        var metadata = component.Metadata;
        if (metadata.IsTag)
            return FlecsEntity.Has(component.Id) ? metadata.DefaultFunc.Invoke() : defaultValue;
        var ptr = flecs.ecs_get_id(Scene.World, Id, component.Id);
        var value = metadata.FromPointerFunc.Invoke((nint)ptr);
        return value ?? defaultValue;
    }

    public object? GetOrDefault(in Component component, Func<object?> defaultValue)
    {
        EnsureValid();
        var metadata = component.Metadata;
        if (metadata.IsTag)
            return FlecsEntity.Has(component.Id) ? metadata.DefaultFunc.Invoke() : defaultValue.Invoke();
        var ptr = flecs.ecs_get_id(Scene.World, Id, component.Id);
        var value = metadata.FromPointerFunc.Invoke((nint)ptr);
        return value ?? defaultValue.Invoke();
    }

    public ref T GetRef<T>()
    {
        EnsureValid();
        return ref Type<T>.IsTag ? ref Unsafe.NullRef<T>() : ref FlecsEntity.GetSafe<T>();
    }

    public void* GetPointer<T>()
    {
        EnsureValid();
        return Type<T>.IsTag ? null : flecs.ecs_get_id(Scene.World, Id, Type<T>.Id(Scene.World));
    }

    public void* GetPointer(in Component component)
    {
        EnsureValid();
        return component.Metadata.IsTag ? null : flecs.ecs_get_id(Scene.World, Id, component.Id);
    }

    [OverloadResolutionPriority(1)]
    public ref readonly Entity Set<T>(IComposable<T> composable)
    {
        Set(composable.ToComponent());
        return ref this;
    }

    public ref readonly Entity Set<T>(in T data)
    {
        EnsureValid();
        ComponentMetadata<T>.EnsureInitialized();
        var flecsEntity = FlecsEntity;
        var id = Type<T>.Id(flecsEntity.World);
        var hadT = flecsEntity.Has(id);
        var isTag = Type<T>.IsTag;
        if (!isTag)
            flecsEntity.Set(data);
        else
            flecsEntity.Add<T>();
        if (!isTag && hadT)
            flecsEntity.CsWorld().Event<SetEvent>().Id(id).Entity(Id).Enqueue();
        else if (!hadT)
            flecsEntity.CsWorld().Event<AddEvent>().Id(id).Entity(Id).Enqueue();
        return ref this;
    }

    public ref readonly Entity Set(in Component component, object? value)
    {
        EnsureValid();
        component.Metadata.SetAction.Invoke(this, value);
        return ref this;
    }

    public void TriggerSet<T>()
    {
        EnsureValid();
        FlecsEntity.CsWorld().Event<SetEvent>().Id<T>().Entity(Id).Enqueue();
    }

    public void TriggerSet(in Component component)
    {
        EnsureValid();
        FlecsEntity.CsWorld().Event<SetEvent>().Id(component.Id).Entity(Id).Enqueue();
    }

    public ref readonly Entity Remove<T>()
    {
        EnsureValid();
        var flecsEntity = FlecsEntity;
        var id = Type<T>.Id(flecsEntity.World);
        flecsEntity.Remove(id);
        return ref this;
    }

    public ref readonly Entity Remove(in Component component)
    {
        EnsureValid();
        FlecsEntity.Remove(component.Id);
        return ref this;
    }

    public void Clear()
    {
        EnsureValid();
        foreach (var component in Components)
            Remove(component);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureValid()
    {
        Debug.Assert(IsValid, "Entity must be valid.");
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
        sb.Append(", Components = ");
        sb.Append(Components.ToString());
        return true;
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
        }

        public bool MoveNext()
        {
            if (_iter.world is null)
                Reset();
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
            _iter.world is null ? Null : new Entity(_iter.entities[_index], _entity.Scene);

        public void Dispose()
        {
            if (_iter.world is null)
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

    public struct Traverser : ITraverser<Traverser, Entity>
    {
        private ChildEnumerator _enumerator;
        private bool _hasEnumerator;

        private readonly bool _deferred;

        public Entity Origin { get; }

        internal Traverser(in Entity origin, bool deferred = true)
        {
            origin.EnsureValid();
            Origin = origin;
            _deferred = deferred;
        }

        public Traverser ConvertToTraverser(Entity next)
        {
            return new Traverser(next, _deferred);
        }

        public bool TryGetChildCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            hasChild = false;
            return false;
        }

        public bool TryGetParent(out Entity parent)
        {
            parent = Origin.Parent;
            return !parent.IsNull;
        }

        public bool TryGetNextChild(out Entity child)
        {
            if (!_hasEnumerator)
            {
                _enumerator = Origin.Children.Deferred(_deferred).GetEnumerator();
                _hasEnumerator = true;
            }

            if (_enumerator.MoveNext())
            {
                child = _enumerator.Current;
                return true;
            }

            child = Null;
            return false;
        }

        public bool TryGetNextSibling(out Entity next)
        {
            BEGIN:
            if (_hasEnumerator)
            {
                if (_enumerator.MoveNext())
                {
                    next = _enumerator.Current;
                    return true;
                }
            }
            else if (TryGetParent(out var parent))
            {
                _enumerator = parent.Children.Deferred(_deferred).GetEnumerator();
                _hasEnumerator = true;
                while (_enumerator.MoveNext())
                    if (_enumerator.Current.Id == Origin.Id)
                        goto BEGIN;
            }

            next = Null;
            return false;
        }

        public bool TryGetPreviousSibling(out Entity previous)
        {
            BEGIN:
            if (_hasEnumerator)
            {
                if (_enumerator.MoveNext())
                {
                    previous = _enumerator.Current;
                    if (previous.Id != Origin.Id)
                        return true;
                }
            }
            else if (TryGetParent(out var parent))
            {
                _enumerator = parent.Children.Deferred(_deferred).GetEnumerator();
                _hasEnumerator = true;
                goto BEGIN;
            }

            previous = Null;
            return false;
        }

        public void Dispose()
        {
            if (!_hasEnumerator)
                return;
            _enumerator.Dispose();
            _hasEnumerator = false;
        }
    }
}

public static unsafe partial class EntityExtensions
{
    extension(Flecs.NET.Core.Entity entity)
    {
        public ref T GetSafe<T>()
        {
            var ptr = flecs.ecs_get_id(entity.World, entity.Id, Type<T>.Id(entity.World));
            return ref Component.FromPointer<T>((nint)ptr);
        }
    }
}
