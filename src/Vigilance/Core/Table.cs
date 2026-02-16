using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public abstract class Table
{
    public enum EventType
    {
        Add,
        Set,
        Remove,
    }

    internal static int CurrentIndex = -1;

    public abstract Scene Scene { get; }

    public abstract Type Type { get; }

    public abstract bool IsHidden { get; }

    public abstract bool SkipAddEvent { get; }

    public abstract bool SkipSetEvent { get; }

    public abstract bool SkipRemoveEvent { get; }

    public abstract bool SkipSetEventIfEqual { get; }

    public abstract bool SetImmutable { get; }

    public abstract bool RemoveImmutable { get; }

    public abstract bool WriteImmutable { get; }

    internal abstract bool Has(in Entity entity);

    internal abstract object? Get(in Entity entity);

    internal abstract void Set(
        in Entity entity,
        object? component,
        OperationStrategy strategy = OperationStrategy.Default
    );

    internal abstract void Remove(in Entity entity, OperationStrategy strategy = OperationStrategy.Default);

    internal abstract void DequeueOperation();

    internal abstract void DequeueEvent();

    internal enum OperationStrategy
    {
        Default,
        IgnoreErrors,
        Force,
    }

    public readonly record struct Event<T>(EventType Type, Entity Entity, T OldValue, T NewValue)
    {
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
public sealed class Table<T> : Table
{
    private const int SparseChunkSize = 2048;
    private readonly Queue<Event<T>> _events = [];
    private readonly Queue<Operation> _operations = [];
    private readonly List<int[]?> _sparseChunks = [];
    internal readonly List<T> Components = [];
    internal readonly List<ulong> DenseIds = [];
    private Action<Entity, T>? _addAction;
    private Action<Entity, T>? _removeAction;
    private Action<Entity, T, T>? _setAction;

    internal Table(Scene scene)
    {
        Scene = scene;
    }

    internal static int Index { get; } = Interlocked.Increment(ref CurrentIndex);

    public override Scene Scene { get; }

    public override Type Type { get; } = typeof(T);

    public override bool IsHidden { get; } = typeof(IHiddenComponent).IsAssignableFrom(typeof(T));

    public override bool SkipAddEvent { get; } = typeof(ISkipAddEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEvent { get; } = typeof(ISkipSetEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipRemoveEvent { get; } = typeof(ISkipRemoveEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEventIfEqual { get; } =
        typeof(ISkipSetEventIfEqualComponent).IsAssignableFrom(typeof(T));

    public override bool SetImmutable { get; } = typeof(ISetImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool RemoveImmutable { get; } = typeof(IRemoveImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool WriteImmutable { get; } = typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public void Enqueue(in Event<T> tableEvent)
    {
        Scene.EnsureInitialized();
        if (Scene.IsDeferred)
        {
            _events.Enqueue(tableEvent);
            Scene.Enqueue(Scene.Event.TableEvent(this));
            return;
        }

        Emit(tableEvent);
    }

    public void Emit(in Event<T> tableEvent)
    {
        Scene.EnsureInitialized();
        switch (tableEvent.Type)
        {
            case EventType.Add:
                if (_addAction is not null && !SkipAddEvent)
                    _addAction.Invoke(tableEvent.Entity, tableEvent.NewValue);
                break;
            case EventType.Set:
                if (
                    _setAction is not null
                    && !SkipSetEvent
                    && (
                        !SkipSetEventIfEqual
                        || !EqualityComparer<T>.Default.Equals(tableEvent.OldValue, tableEvent.NewValue)
                    )
                )
                    _setAction.Invoke(tableEvent.Entity, tableEvent.OldValue, tableEvent.NewValue);
                break;
            case EventType.Remove:
                if (_removeAction is not null && !SkipRemoveEvent)
                    _removeAction.Invoke(tableEvent.Entity, tableEvent.NewValue);
                break;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(tableEvent.Type),
                    (int)tableEvent.Type,
                    typeof(EventType)
                );
        }
    }

    internal override bool Has(in Entity entity)
    {
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

    internal override object? Get(in Entity entity)
    {
        ref var value = ref GetRef(in entity);
        return Unsafe.IsNullRef(ref value) ? null : value;
    }

    internal override void Set(
        in Entity entity,
        object? component,
        OperationStrategy strategy = OperationStrategy.Default
    )
    {
        Set(entity, (T)component!, strategy);
    }

    internal override void Remove(in Entity entity, OperationStrategy strategy = OperationStrategy.Default)
    {
        if (Scene.IsDeferred)
        {
            _operations.Enqueue(new Operation(OperationType.Remove, entity, default!));
            Scene.Enqueue(Scene.Event.TableOperation(this));
            return;
        }

        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return;
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return;
        var denseIndex = sparseValue - 1;
        if (RemoveImmutable)
            switch (strategy)
            {
                case OperationStrategy.Default:
                    throw new InvalidOperationException(
                        $"Cannot remove {Type} because it implements {nameof(IRemoveImmutableComponent)}."
                    );
                case OperationStrategy.IgnoreErrors:
                    return;
            }

        var component = Components[denseIndex];
        var lastDenseIndex = Components.Count - 1;
        if (denseIndex != lastDenseIndex)
        {
            Components[denseIndex] = Components[lastDenseIndex];
            var movedId = DenseIds[lastDenseIndex];
            DenseIds[denseIndex] = movedId;
            var movedEntityIndex = Entity.GetIndex(movedId);
            var movedChunkIndex = movedEntityIndex / SparseChunkSize;
            var movedWithinChunk = movedEntityIndex % SparseChunkSize;
            var movedChunk = _sparseChunks[movedChunkIndex]!;
            movedChunk[movedWithinChunk] = denseIndex + 1;
        }

        Components.RemoveAt(lastDenseIndex);
        DenseIds.RemoveAt(lastDenseIndex);
        chunk[withinChunk] = 0;
        Emit(Event<T>.Remove(entity, component));
    }

    internal override void DequeueOperation()
    {
        if (!_operations.TryDequeue(out var operation))
            return;
        switch (operation.Type)
        {
            case OperationType.Set:
                Set(operation.Entity, operation.Value);
                break;
            case OperationType.Remove:
                Remove(operation.Entity);
                break;
        }
    }

    internal override void DequeueEvent()
    {
        if (!_events.TryDequeue(out var @event))
            return;
        Emit(@event);
    }

    internal ref T GetRef(in Entity entity)
    {
        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return ref Unsafe.NullRef<T>();
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return ref Unsafe.NullRef<T>();
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return ref Unsafe.NullRef<T>();
        var denseIndex = sparseValue - 1;
        return ref Components.AsSpan()[denseIndex];
    }

    internal ref T Set(in Entity entity, in T component, OperationStrategy strategy = OperationStrategy.Default)
    {
        if (Scene.IsDeferred)
        {
            _operations.Enqueue(new Operation(OperationType.Set, entity, component));
            Scene.Enqueue(Scene.Event.TableOperation(this));
            return ref Unsafe.NullRef<T>();
        }

        EnsureChunk(entity.Index);
        var chunkIndex = entity.Index / SparseChunkSize;
        var withinChunk = entity.Index % SparseChunkSize;
        var chunk = _sparseChunks[chunkIndex]!;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
        {
            var index = Components.Count + 1;
            Components.Add(component);
            DenseIds.Add(entity.Id);
            chunk[withinChunk] = index;
            Emit(Event<T>.Add(entity, component));
            return ref Components.AsSpan()[index - 1];
        }

        if (SetImmutable)
            switch (strategy)
            {
                case OperationStrategy.Default:
                    throw new InvalidOperationException(
                        $"Cannot set {Type} because it implements {nameof(ISetImmutableComponent)}."
                    );
                case OperationStrategy.IgnoreErrors:
                    return ref Unsafe.NullRef<T>();
            }

        var denseIndex = sparseValue - 1;
        ref var componentRef = ref Components.AsSpan()[denseIndex];
        var oldValue = componentRef;
        componentRef = component;
        Emit(Event<T>.Set(entity, oldValue, component));
        return ref componentRef!;
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

    private enum OperationType
    {
        Set,
        Remove,
    }

    private readonly record struct Operation(OperationType Type, Entity Entity, T Value);
}
