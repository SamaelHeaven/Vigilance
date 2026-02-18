using System.Diagnostics.CodeAnalysis;
using Vigilance.Collections;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace Vigilance.Core;

public abstract class Table
{
    public enum EventType
    {
        Add,
        Set,
        Remove,
    }

    [Flags]
    public enum Flags
    {
        Default = 0,
        SilentOnImmutable = 1 << 0,
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

    public abstract bool SetImmutable { get; }

    public abstract bool RemoveImmutable { get; }

    public abstract bool WriteImmutable { get; }

    public abstract bool Has(in Entity entity);

    public abstract object? Get(in Entity entity);

    public abstract void Set(in Entity entity, object? component, Flags flags = Flags.Default);

    public abstract void Remove(in Entity entity, Flags flags = Flags.Default);

    internal abstract void DequeueOperation();

    internal abstract void DequeueEvent();

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

public static class TableExtensions
{
    extension(Table.Flags)
    {
        internal static Table.Flags ForceMutable => (Table.Flags)(1 << 30);
    }
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public sealed class Table<T> : Table
{
    private const int SparseChunkSize = 2048;
    private Action<Entity, T>? _addAction;
    private ValueQueue<Event<T>> _events = [];
    private ValueQueue<Operation> _operations = [];
    private Action<Entity, T>? _removeAction;
    private Action<Entity, T, T>? _setAction;
    private ValueList<int[]?> _sparseChunks = [];
    internal ValueList<T> Components = [];
    internal ValueList<ulong> DenseIds = [];

    internal Table(Scene scene)
    {
        Scene = scene;
    }

    internal static int Index { get; } = Interlocked.Increment(ref CurrentIndex);

    public override Scene Scene { get; }

    public override Type Type { get; } = typeof(T);

    public override int Count => Components.Count;

    public override int Capacity
    {
        get => Components.Capacity;
        set
        {
            Components.Capacity = value;
            DenseIds.Capacity = value;
        }
    }

    public override bool IsHidden { get; } = typeof(IHiddenComponent).IsAssignableFrom(typeof(T));

    public override bool SkipAddEvent { get; } = typeof(ISkipAddEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEvent { get; } = typeof(ISkipSetEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipRemoveEvent { get; } = typeof(ISkipRemoveEventComponent).IsAssignableFrom(typeof(T));

    public override bool SkipSetEventIfEqual { get; } =
        typeof(ISkipSetEventIfEqualComponent).IsAssignableFrom(typeof(T));

    public override bool SetImmutable { get; } = typeof(ISetImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool RemoveImmutable { get; } = typeof(IRemoveImmutableComponent).IsAssignableFrom(typeof(T));

    public override bool WriteImmutable { get; } = typeof(IWriteImmutableComponent).IsAssignableFrom(typeof(T));

    public ReadOnlySpan<T> AsSpan()
    {
        return Components.AsSpan();
    }

    public void Enqueue(in Event<T> tableEvent)
    {
        Scene.ThrowIfNotInitialized();
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
        Scene.ThrowIfNotInitialized();
        switch (tableEvent.Type)
        {
            case EventType.Add:
                if (_addAction is not null && !SkipAddEvent)
                {
                    Scene.BeginDefer();
                    _addAction.Invoke(tableEvent.Entity, tableEvent.NewValue);
                    Scene.EndDefer();
                }

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
                {
                    Scene.BeginDefer();
                    _setAction.Invoke(tableEvent.Entity, tableEvent.OldValue, tableEvent.NewValue);
                    Scene.EndDefer();
                }

                break;
            case EventType.Remove:
                if (_removeAction is not null && !SkipRemoveEvent)
                {
                    Scene.BeginDefer();
                    _removeAction.Invoke(tableEvent.Entity, tableEvent.NewValue);
                    Scene.EndDefer();
                }

                break;
        }
    }

    public override bool Has(in Entity entity)
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

    public override object? Get(in Entity entity)
    {
        var value = GetRef(in entity);
        return value.IsNull ? null : value.Read;
    }

    public override void Set(in Entity entity, object? component, Flags flags = Flags.Default)
    {
        Set(entity, (T)component!, flags);
    }

    public override void Remove(in Entity entity, Flags flags = Flags.Default)
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
        if (RemoveImmutable && (flags & Flags.ForceMutable) == 0)
            if ((flags & Flags.SilentOnImmutable) != 0)
                return;
            else
                throw new InvalidOperationException(
                    $"Cannot remove {Type} because it implements {nameof(IRemoveImmutableComponent)}."
                );
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

    public ComponentRef<T> GetRef(in Entity entity)
    {
        var chunkIndex = entity.Index / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return ComponentRef<T>.Null;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return ComponentRef<T>.Null;
        var withinChunk = entity.Index % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return ComponentRef<T>.Null;
        var denseIndex = sparseValue - 1;
        return new ComponentRef<T>(ref Components[denseIndex]);
    }

    public ComponentRef<T> Set(in Entity entity, scoped in T component, Flags flags = Flags.Default)
    {
        if (Scene.IsDeferred)
        {
            _operations.Enqueue(new Operation(OperationType.Set, entity, component));
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
            var index = Components.Count + 1;
            Components.Add(component);
            DenseIds.Add(entity.Id);
            chunk[withinChunk] = index;
            Emit(Event<T>.Add(entity, component));
            return new ComponentRef<T>(ref Components[index - 1]);
        }

        if (RemoveImmutable && (flags & Flags.ForceMutable) == 0)
            if ((flags & Flags.SilentOnImmutable) != 0)
                return ComponentRef<T>.Null;
            else
                throw new InvalidOperationException(
                    $"Cannot set {Type} because it implements {nameof(IRemoveImmutableComponent)}."
                );
        var denseIndex = sparseValue - 1;
        ref var componentRef = ref Components[denseIndex];
        var oldValue = componentRef;
        componentRef = component;
        Emit(Event<T>.Set(entity, oldValue, component));
        return new ComponentRef<T>(ref componentRef!);
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
