using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public abstract class Table
{
    public enum EventType : sbyte
    {
        Add,
        Set,
        Remove,
    }

    [Flags]
    public enum Flags : byte
    {
        None = 0,
        SilentOnImmutable = 1 << 0,
        ForceMutable = 1 << 1,
    }

    internal static int CurrentIndex = -1;

    public abstract Scene Scene { get; }

    public abstract Type Type { get; }

    public abstract int Count { get; }

    public abstract int Capacity { get; set; }

    public abstract bool IsHidden { get; }

    public abstract bool SkipAddEvent { get; }

    public abstract bool SkipSetEvent { get; }

    public abstract bool SkipRemoveEvent { get; }

    public abstract bool SkipSetEventIfEqual { get; }

    public abstract bool AddImmutable { get; }

    public abstract bool SetImmutable { get; }

    public abstract bool RemoveImmutable { get; }

    public abstract bool WriteImmutable { get; }

    public abstract ValueListView<EntityId> EntityIds { get; }

    public abstract void TrimExcess();

    public abstract void EnsureCapacity(int capacity);

    public abstract bool Has(in Entity entity);

    public abstract object Get(int index);

    public abstract object Get(in Entity entity);

    public abstract bool TryGet(in Entity entity, out object component);

    public abstract void Set(in Entity entity, object component, Flags flags = Flags.None);

    public abstract bool Remove(in Entity entity, Flags flags = Flags.None);

    public abstract bool Remove(in Entity entity, out object component, Flags flags = Flags.None);

    internal abstract void DequeueOperation();

    internal abstract void DequeueEvent();

    public readonly record struct Event<T>
    {
        public Event(EventType type, in Entity entity, T oldValue, T newValue)
        {
            entity.AssertValid();
            Entity = entity;
            OldValue = oldValue;
            NewValue = newValue;
            Type = type;
        }

        public Entity Entity { get; }
        public T OldValue { get; }
        public T NewValue { get; }
        public EventType Type { get; }

        public static Event<T> Add(in Entity entity, in T value)
        {
            return new Event<T>(EventType.Add, entity, default!, value);
        }

        public static Event<T> Set(in Entity entity, in T oldValue, in T newValue)
        {
            return new Event<T>(EventType.Set, entity, oldValue, newValue);
        }

        public static Event<T> Remove(in Entity entity, in T value)
        {
            return new Event<T>(EventType.Remove, entity, default!, value);
        }
    }
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public sealed class Table<T>
    : Table,
        IReadOnlyDictionary<Entity, T>,
        IReadOnlyList<KeyValuePair<Entity, T>>,
        IStructEnumerable<Table<T>.Enumerator, KeyValuePair<Entity, T>>
{
    private const int SparseChunkSize = 2048;
    private Action<Entity, T>? _addAction;
    private ValueList<T> _components = [];
    private ValueList<EntityId> _entityIds = [];
    private ValueQueue<Event<T>> _events = [];
    private ValueQueue<Operation> _operations = [];
    private Action<Entity, T>? _removeAction;
    private Action<Entity, T, T>? _setAction;
    private ValueList<int[]?> _sparseChunks = [];

    internal Table(Scene scene)
    {
        Scene = scene;
    }

    internal static int Index { get; } = Interlocked.Increment(ref CurrentIndex);

    public override Scene Scene { get; }

    public override Type Type { get; } = typeof(T);

    public override int Capacity
    {
        get => _components.Capacity;
        set
        {
            _components.Capacity = value;
            _entityIds.Capacity = value;
        }
    }

    public override bool IsHidden { get; } = typeof(IHiddenComponent).IsAssignableFrom(typeof(T));

    public override bool SkipAddEvent { get; } = typeof(ISkipAddEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEvent { get; } = typeof(ISkipSetEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipRemoveEvent { get; } = typeof(ISkipRemoveEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEventIfEqual { get; } =
        typeof(ISkipSetEventIfEqualComponent).IsAssignableFrom(typeof(T));

    public override bool AddImmutable { get; } = typeof(IAddImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool SetImmutable { get; } = typeof(ISetImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool RemoveImmutable { get; } = typeof(IRemoveImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool WriteImmutable { get; } = typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public override ValueListView<EntityId> EntityIds => _entityIds;

    public ValueListView<T> Components => _components;

    public override int Count => _components.Count;

    bool IReadOnlyDictionary<Entity, T>.ContainsKey(Entity key)
    {
        return Has(key);
    }

    bool IReadOnlyDictionary<Entity, T>.TryGetValue(Entity key, [MaybeNullWhen(false)] out T value)
    {
        var component = GetRef(key);
        if (component.IsNull)
        {
            Unsafe.SkipInit(out value);
            return false;
        }

        value = component.Read;
        return true;
    }

    T IReadOnlyDictionary<Entity, T>.this[Entity key] => GetRef(key);

    IEnumerable<Entity> IReadOnlyDictionary<Entity, T>.Keys =>
        _entityIds.Select(entityId => new Entity(entityId, Scene));

    IEnumerable<T> IReadOnlyDictionary<Entity, T>.Values => _components.AsReadOnly();

    public KeyValuePair<Entity, T> this[int index] => new(new Entity(_entityIds[index], Scene), _components[index]);

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    ValueEnumerable<StructEnumerator<Enumerator, KeyValuePair<Entity, T>>, KeyValuePair<Entity, T>> IStructEnumerable<
        Enumerator,
        KeyValuePair<Entity, T>
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, KeyValuePair<Entity, T>>(GetEnumerator());
    }

    public override void TrimExcess()
    {
        _components.TrimExcess();
        _entityIds.TrimExcess();
        _sparseChunks.TrimExcess();
        _events.TrimExcess();
        _operations.TrimExcess();
    }

    public override void EnsureCapacity(int capacity)
    {
        _components.EnsureCapacity(capacity);
        _entityIds.EnsureCapacity(capacity);
    }

    public ValueEnumerable<Enumerator, KeyValuePair<Entity, T>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, KeyValuePair<Entity, T>>(GetEnumerator());
    }

    public void Enqueue(in Event<T> tableEvent)
    {
        Scene.ThrowIfNotConfigured();
        if (Scene.IsDeferred)
        {
            switch (tableEvent.Type)
            {
                case EventType.Add when _addAction is null || SkipAddEvent:
                case EventType.Set when _setAction is null || SkipSetEvent:
                case EventType.Remove when _removeAction is null || SkipRemoveEvent:
                    return;
            }

            _events.Enqueue(tableEvent);
            Scene.Enqueue(Scene.Event.TableEvent(this));
            return;
        }

        Emit(tableEvent);
    }

    public void Emit(in Event<T> tableEvent)
    {
        Scene.ThrowIfNotConfigured();
        switch (tableEvent.Type)
        {
            case EventType.Add:
                if (!SkipAddEvent)
                    _addAction?.SafeInvoke(tableEvent.Entity, tableEvent.NewValue);
                break;
            case EventType.Set:
                if (
                    !SkipSetEvent
                    && (
                        !SkipSetEventIfEqual
                        || !EqualityComparer<T>.Default.Equals(tableEvent.OldValue, tableEvent.NewValue)
                    )
                )
                    _setAction?.SafeInvoke(tableEvent.Entity, tableEvent.OldValue, tableEvent.NewValue);
                break;
            case EventType.Remove:
                if (!SkipRemoveEvent)
                    _removeAction?.SafeInvoke(tableEvent.Entity, tableEvent.NewValue);
                break;
        }
    }

    public override bool Has(in Entity entity)
    {
        entity.AssertValid();
        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return false;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return false;
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        return sparseValue != 0;
    }

    public override object Get(int index)
    {
        return _components[index]!;
    }

    public override object Get(in Entity entity)
    {
        entity.AssertValid();
        var value = GetRef(in entity);
        return (value.IsNull ? null : value.Read)!;
    }

    public override bool TryGet(in Entity entity, out object component)
    {
        entity.AssertValid();
        var value = GetRef(in entity);
        component = (value.IsNull ? null : value.Read)!;
        return !value.IsNull;
    }

    public bool TryGet(in Entity entity, out T component)
    {
        entity.AssertValid();
        var value = GetRef(in entity);
        if (value.IsNull)
        {
            Unsafe.SkipInit(out component);
            return false;
        }

        component = value.Read;
        return true;
    }

    public bool TryGetRef(scoped in Entity entity, out ComponentRef<T> componentRef)
    {
        entity.AssertValid();
        componentRef = GetRef(in entity);
        return !componentRef.IsNull;
    }

    public override void Set(in Entity entity, object component, Flags flags = Flags.None)
    {
        entity.AssertValid();
        Set(entity, (T)component, flags);
    }

    public override bool Remove(in Entity entity, Flags flags = Flags.None)
    {
        entity.AssertValid();
        return Remove(entity, out _, flags);
    }

    public override bool Remove(in Entity entity, out object component, Flags flags = Flags.None)
    {
        entity.AssertValid();
        var result = Remove(entity, out var value, flags);
        component = result ? value! : null!;
        return result;
    }

    public bool Remove(in Entity entity, out T component, Flags flags = Flags.None)
    {
        entity.AssertValid();
        Unsafe.SkipInit(out component);
        if (RemoveImmutable && (flags & Flags.ForceMutable) == 0)
            if ((flags & Flags.SilentOnImmutable) != 0)
                return false;
            else
                throw new InvalidOperationException(
                    $"Cannot remove {Type} because it implements {nameof(IRemoveImmutableComponent)}."
                );
        if (Scene.IsDeferred)
        {
            _operations.Enqueue(new Operation(entity.Id, default!, OperationType.Remove, flags));
            Scene.Enqueue(Scene.Event.TableOperation(this));
            return false;
        }

        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return false;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return false;
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return false;
        var denseIndex = sparseValue - 1;
        component = _components[denseIndex];
        var lastDenseIndex = _components.Count - 1;
        if (denseIndex != lastDenseIndex)
        {
            _components[denseIndex] = _components[lastDenseIndex];
            var movedId = _entityIds[lastDenseIndex];
            _entityIds[denseIndex] = movedId;
            var movedEntityIndex = movedId.Index;
            var movedChunkIndex = movedEntityIndex / SparseChunkSize;
            var movedWithinChunk = movedEntityIndex % SparseChunkSize;
            var movedChunk = _sparseChunks[movedChunkIndex]!;
            movedChunk[movedWithinChunk] = denseIndex + 1;
        }

        _components.RemoveAt(lastDenseIndex);
        _entityIds.RemoveAt(lastDenseIndex);
        chunk[withinChunk] = 0;
        Emit(Event<T>.Remove(entity, component));
        return true;
    }

    public ComponentRef<T> GetRef(int index)
    {
        return new ComponentRef<T>(ref _components[index], index);
    }

    public ComponentRef<T> GetRef(scoped in Entity entity)
    {
        entity.AssertValid();
        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return ComponentRef<T>.Null;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk is null)
            return ComponentRef<T>.Null;
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return ComponentRef<T>.Null;
        var denseIndex = sparseValue - 1;
        return new ComponentRef<T>(ref _components[denseIndex], denseIndex);
    }

    public ComponentRef<T> Set(scoped in Entity entity, scoped in T component, Flags flags = Flags.None)
    {
        entity.AssertValid();
        if (SetImmutable && AddImmutable && (flags & Flags.ForceMutable) == 0)
            if ((flags & Flags.SilentOnImmutable) != 0)
                return ComponentRef<T>.Null;
            else
                throw new InvalidOperationException(
                    $"Cannot set {Type} because it implements {nameof(IAddImmutableComponent)} and {nameof(ISetImmutableComponent)}."
                );
        if (!typeof(T).IsValueType)
        {
            Debug.Assert(component is not null);
            if ((T?)component is null)
                return ComponentRef<T>.Null;
        }

        if (Scene.IsDeferred)
        {
            _operations.Enqueue(new Operation(entity.Id, component, OperationType.Set, flags));
            Scene.Enqueue(Scene.Event.TableOperation(this));
            return ComponentRef<T>.Null;
        }

        EnsureChunk(entity.Index);
        var chunkIndex = entity.Index / SparseChunkSize;
        var withinChunk = entity.Index % SparseChunkSize;
        var chunk = _sparseChunks[chunkIndex]!;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
        {
            if (AddImmutable && (flags & Flags.ForceMutable) == 0)
                if ((flags & Flags.SilentOnImmutable) != 0)
                    return ComponentRef<T>.Null;
                else
                    throw new InvalidOperationException(
                        $"Cannot add {Type} because it implements {nameof(IAddImmutableComponent)}."
                    );
            var index = _components.Count + 1;
            _components.Add(component);
            _entityIds.Add(entity.Id);
            chunk[withinChunk] = index;
            Emit(Event<T>.Add(entity, component));
            index--;
            return new ComponentRef<T>(ref _components[index], index);
        }

        if (SetImmutable && (flags & Flags.ForceMutable) == 0)
            if ((flags & Flags.SilentOnImmutable) != 0)
                return ComponentRef<T>.Null;
            else
                throw new InvalidOperationException(
                    $"Cannot set {Type} because it implements {nameof(ISetImmutableComponent)}."
                );
        var denseIndex = sparseValue - 1;
        ref var componentRef = ref _components[denseIndex];
        var oldValue = componentRef;
        componentRef = component;
        Emit(Event<T>.Set(entity, oldValue, component));
        return new ComponentRef<T>(ref componentRef!, denseIndex);
    }

    internal override void DequeueOperation()
    {
        if (!_operations.TryDequeue(out var operation))
            return;
        switch (operation.Type)
        {
            case OperationType.Set:
                Set(new Entity(operation.EntityId, Scene), operation.Value, operation.Flags);
                break;
            case OperationType.Remove:
                Remove(new Entity(operation.EntityId, Scene), operation.Flags);
                break;
        }
    }

    internal override void DequeueEvent()
    {
        if (!_events.TryDequeue(out var @event))
            return;
        Emit(@event);
    }

    internal void OnAdd(Action<Entity, T> action)
    {
        _addAction += action;
    }

    internal void OnSet(Action<Entity, T, T> action)
    {
        _setAction += action;
    }

    internal void OnRemove(Action<Entity, T> action)
    {
        _removeAction += action;
    }

    private void EnsureChunk(int entityIndex)
    {
        var chunkIndex = entityIndex / SparseChunkSize;
        while (_sparseChunks.Count <= chunkIndex)
            _sparseChunks.Add(null);
        if (_sparseChunks[chunkIndex] != null)
            return;
        var chunk = new int[SparseChunkSize];
        _sparseChunks[chunkIndex] = chunk;
    }

    private enum OperationType : sbyte
    {
        Set,
        Remove,
    }

    private readonly record struct Operation(EntityId EntityId, T Value, OperationType Type, Flags Flags);

    public struct Enumerator : IStructEnumerator<KeyValuePair<Entity, T>>, IValueEnumerator<KeyValuePair<Entity, T>>
    {
        private readonly Table<T> _table;
        private int _index;

        internal Enumerator(Table<T> table)
        {
            _table = table;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_table._entityIds.Count)
            {
                Current = new KeyValuePair<Entity, T>(
                    new Entity(_table._entityIds[_index], _table.Scene),
                    _table._components[_index]
                );
                _index++;
                return true;
            }

            Current = default;
            _index = -1;
            return false;
        }

        public KeyValuePair<Entity, T> Current { get; private set; }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public void Dispose() { }

        public bool TryGetNext(out KeyValuePair<Entity, T> current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _table._entityIds.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<KeyValuePair<Entity, T>> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<KeyValuePair<Entity, T>> destination, Index offset)
        {
            return false;
        }
    }
}
