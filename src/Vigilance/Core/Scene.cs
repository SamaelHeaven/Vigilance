#pragma warning disable CS9084

using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Drawing;
using Vigilance.Logging;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public sealed unsafe partial class Scene
{
    private readonly GameSystemsFunc _systemsFunc;
    private ValueDictionary<Type, (Delegate EnqueueAction, Action DequeueAction)> _customEvents = [];
    private Action? _deferredAction;
    private int _deferredCount;
    private Action<Entity>? _destroyAction;
    private ValueList<(int Index, int Version)> _entities = [];
    private ValueQueue<Event> _events = [];
    private Action? _fixedUpdateAction;
    private ValueQueue<int> _freeIndices = [];
    private Action? _initializeAction;
    private Action<Entity>? _instantiateAction;
    private bool _isFlushing;
    private ValueDictionary<Type, Delegate> _listeners = [];
    private ValueDictionary<string, ulong> _nameMap = [];
    private Action? _onDispose;
    private Action? _postFixedUpdateAction;
    private Action? _postRenderAction;
    private Action? _postUpdateAction;
    private Action? _preFixedUpdateAction;
    private Action? _preRenderAction;
    private Action? _preUpdateAction;
    private Action<RenderCommands>? _renderAction;
    private ValueList<RenderComponents?> _sparseRenderComponentsList = [];
    private ValueList<Table?> _sparseTables = [];
    private Action? _startAction;
    private Action? _stopAction;
    private ValueStack<int> _suspendStack = [];
    private ValueList<IGameSystem> _systems = [];
    private ValueList<Table> _tables = [];
    private Action? _updateAction;
    internal Table<Child> ChildTable;
    internal Table<Disabled> DisabledTable;
    internal Table<EntityTag> EntityTagTable;
    internal Table<Interpolation> InterpolationTable;
    internal Table<Name> NameTable;
    internal Table<Parent> ParentTable;
    internal Table<PivotPoint> PivotPointTable;
    internal Table<Position> PositionTable;
    internal ValueList<RenderCommand> RenderCommands = [];
    internal ValueList<RenderComponents> RenderComponentsList = [];
    internal ValueList<RenderData> RenderDataList = [];
    internal Table<Rotation> RotationTable;
    internal Table<Scale> ScaleTable;
    internal Table<Transform> TransformTable;
    internal Table<ZIndex> ZIndexTable;

    public Scene(GameSystemsFunc? systems = null)
    {
        _entities.Add((0, 0));
        _systemsFunc = systems ?? Array.Empty<IGameSystem>;
        EntityTagTable = Table<EntityTag>();
        NameTable = Table<Name>();
        ZIndexTable = Table<ZIndex>();
        PositionTable = Table<Position>();
        ScaleTable = Table<Scale>();
        RotationTable = Table<Rotation>();
        PivotPointTable = Table<PivotPoint>();
        TransformTable = Table<Transform>();
        DisabledTable = Table<Disabled>();
        ChildTable = Table<Child>();
        ParentTable = Table<Parent>();
        InterpolationTable = Table<Interpolation>();
        OnSet<Position>(OnSetPosition);
        OnSet<Scale>(OnSetScale);
        OnSet<Rotation>(OnSetRotation);
        OnSet<PivotPoint>(OnSetPivotPoint);
        OnSet<Transform>(OnSetTransform);
        OnAdd<Child>(OnAddChild);
        OnSet<Child>(OnSetChild);
        OnRemove<Child>(OnRemoveChild);
        OnRemove<Parent>(OnRemoveParent);
        OnRemove<Name>(OnRemoveName);
    }

    public Camera Camera { get; } = new();

    public Entity Scope
    {
        get;
        set
        {
            ThrowIfNotConfigured();
            if (!value.IsNull)
                value.AssertValid();
            field = value;
        }
    }

    public bool IsConfigured { get; private set; }

    public bool IsInitialized { get; private set; }

    public bool IsStarted { get; private set; }

    public bool IsDeferred => _deferredCount != 0;

    public TableEnumerable Tables()
    {
        return new TableEnumerable(this);
    }

    public TableEnumerable<T> Tables<T>()
    {
        return new TableEnumerable<T>(this);
    }

    public SystemEnumerable Systems()
    {
        return new SystemEnumerable(this);
    }

    public SystemEnumerable<T> Systems<T>()
        where T : IGameSystem
    {
        return new SystemEnumerable<T>(this);
    }

    public T System<T>()
        where T : IGameSystem
    {
        return SystemOrDefault<T>() ?? throw new InvalidOperationException($"Cannot find system of type {typeof(T)}");
    }

    public T? SystemOrDefault<T>()
        where T : IGameSystem
    {
        return _systems.AsValueEnumerable().OfType<T>().FirstOrDefault();
    }

    public void Restart()
    {
        if (!IsInitialized)
            return;
        if (Game.Scene == this)
        {
            Game.Defer(RestartAction);
            return;
        }

        RestartAction();
    }

    public Entity Entity(string? name = null)
    {
        ThrowIfNotConfigured();
        ulong id;
        var recycle = _freeIndices.Count > 0;
        ref var info = ref Unsafe.NullRef<(int Index, int Version)>();
        if (recycle)
        {
            var index = _freeIndices.Peek();
            info = ref _entities[index];
            id = Core.Entity.GetId(index, info.Version + 1);
        }
        else
        {
            id = Core.Entity.GetId(_entities.Count, 0);
        }

        if (name is not null)
        {
            ref var nameId = ref _nameMap.GetValueRefOrAddDefault(name, out var exists);
            if (exists)
                throw new InvalidOperationException($"Entity \"{name}\" already exists.");
            nameId = id;
        }

        if (recycle)
        {
            info.Index = Core.Entity.GetIndex(id);
            info.Version++;
            _freeIndices.Dequeue();
        }
        else
        {
            _entities.Add((_entities.Count, 0));
        }

        var entity = new Entity(id, this);
        SuspendDefer();
        try
        {
            EntityTagTable.Set(entity, new EntityTag(), Core.Table.Flags.ForceMutable);
            if (name is not null)
                NameTable.Set(entity, new Name(name), Core.Table.Flags.ForceMutable);
            if (!Scope.IsNull)
                ChildTable.Set(entity, new Child(Scope.Id));
        }
        finally
        {
            ResumeDefer();
            if (IsDeferred)
                Enqueue(Event.Instantiate(entity));
            else
                _instantiateAction?.Invoke(entity);
        }

        return entity;
    }

    public Entity Lookup(int index, int version)
    {
        ThrowIfNotConfigured();
        if (index == 0 || index >= _entities.Count)
            return Core.Entity.Null;
        var info = _entities[index];
        if (info.Index != index || info.Version != version)
            return Core.Entity.Null;
        return new Entity(index, version, this);
    }

    public Entity Lookup(ulong id)
    {
        ThrowIfNotConfigured();
        return Lookup(Core.Entity.GetIndex(id), Core.Entity.GetVersion(id));
    }

    public Entity Lookup(string name)
    {
        ThrowIfNotConfigured();
        ref var id = ref _nameMap.GetValueRefOrNullRef(name);
        return Unsafe.IsNullRef(ref id) ? Core.Entity.Null : new Entity(id, this);
    }

    public void On<T>(Action<T> action)
    {
        ThrowIfConfigured();
        var type = typeof(T);
        ref var handlers = ref _listeners.GetValueRefOrAddDefault(type, out _)!;
        var signal = new Signal<T>(ref Unsafe.As<Delegate, Func<T, bool>>(ref handlers)!);
        signal.Subscribe(action);
    }

    public void On<T>(Func<T, bool> handler)
    {
        ThrowIfConfigured();
        var type = typeof(T);
        ref var handlers = ref _listeners.GetValueRefOrAddDefault(type, out _)!;
        var signal = new Signal<T>(ref Unsafe.As<Delegate, Func<T, bool>>(ref handlers)!);
        signal.Subscribe(handler);
    }

    public void OnInitialize(Action action)
    {
        ThrowIfConfigured();
        _initializeAction += action;
    }

    public void OnStart(Action action)
    {
        ThrowIfConfigured();
        _startAction += action;
    }

    public void OnStop(Action action)
    {
        ThrowIfConfigured();
        _stopAction += action;
    }

    public void OnDispose(Action action)
    {
        ThrowIfConfigured();
        _onDispose += action;
    }

    public void OnPreUpdate(Action action)
    {
        ThrowIfConfigured();
        _preUpdateAction += action;
    }

    public void OnUpdate(Action action)
    {
        ThrowIfConfigured();
        _updateAction += action;
    }

    public void OnPostUpdate(Action action)
    {
        ThrowIfConfigured();
        _postUpdateAction += action;
    }

    public void OnPreFixedUpdate(Action action)
    {
        ThrowIfConfigured();
        _preFixedUpdateAction += action;
    }

    public void OnFixedUpdate(Action action)
    {
        ThrowIfConfigured();
        _fixedUpdateAction += action;
    }

    public void OnPostFixedUpdate(Action action)
    {
        ThrowIfConfigured();
        _postFixedUpdateAction += action;
    }

    public void OnPreRender(Action action)
    {
        ThrowIfConfigured();
        _preRenderAction += action;
    }

    public void OnRender(Action<RenderCommands> action)
    {
        ThrowIfConfigured();
        _renderAction += action;
    }

    public void OnPostRender(Action action)
    {
        ThrowIfConfigured();
        _postRenderAction += action;
    }

    public void Emit<T>(in T @event)
    {
        ThrowIfNotConfigured();
        var type = typeof(T);
        if (!_listeners.TryGetValue(type, out var handlers))
            return;
        Signal<T>.Invoke((Func<T, bool>)handlers, @event);
    }

    public void Enqueue<T>(in T @event)
    {
        ThrowIfNotConfigured();
        if (!IsDeferred)
        {
            Emit(@event);
            return;
        }

        var type = typeof(T);
        ref var events = ref _customEvents.GetValueRefOrAddDefault(type, out var exists);
        if (!exists)
        {
            if (!_listeners.TryGetValue(type, out var handlers))
                return;
            var queue = new ValueQueue<T>();
            events = (
                (T value) => queue.Enqueue(value),
                () =>
                {
                    if (queue.TryDequeue(out var @event))
                        Signal<T>.Invoke((Func<T, bool>)handlers, @event);
                }
            );
        }

        ((Action<T>)events.EnqueueAction).Invoke(@event);
        Enqueue(Event.Custom(type));
    }

    public void Defer(Action action)
    {
        if (IsDeferred)
        {
            _deferredAction += action;
            return;
        }

        action.Invoke();
    }

    public void ThrowIfNotConfigured()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Scene has not been configured.");
    }

    public void ThrowIfConfigured()
    {
        if (IsConfigured)
            throw new InvalidOperationException("Scene has been configured.");
    }

    public void BeginDefer()
    {
        _deferredCount++;
    }

    public void EndDefer()
    {
        if (_deferredCount == 0)
            throw new InvalidOperationException("Scene is not in a deferred state.");
        _deferredCount--;
        TryFlush();
    }

    public void SuspendDefer()
    {
        _suspendStack.Push(_deferredCount);
        _deferredCount = 0;
    }

    public void ResumeDefer()
    {
        if (_suspendStack.Count == 0)
            throw new InvalidOperationException("Scene is not in a suspended state.");
        _deferredCount += _suspendStack.Pop();
    }

    public Entity SetScope(in Entity entity)
    {
        ThrowIfNotConfigured();
        var oldScope = Scope;
        Scope = entity;
        return oldScope;
    }

    public Table<T> Table<T>()
    {
        var index = Core.Table<T>.Index;
        while (_sparseTables.Count <= index)
            _sparseTables.Add(null);
        var table = (Table<T>?)_sparseTables[index];
        if (table is not null)
            return table;
        _sparseTables[index] = table = new Table<T>(this);
        _tables.Add(table);
        return table;
    }

    public void Clear()
    {
        SuspendDefer();
        try
        {
            var entities = Entities().WithDisabled();
            do
            {
                foreach (var entity in entities)
                    entity.Destroy();
            } while (entities.AsValueEnumerable().Any());
        }
        finally
        {
            ResumeDefer();
        }
    }

    internal RenderComponents<T> RenderComponents<T>()
    {
        var index = Drawing.RenderComponents<T>.Index;
        while (_sparseRenderComponentsList.Count <= index)
            _sparseRenderComponentsList.Add(null);
        var table = (RenderComponents<T>?)_sparseRenderComponentsList[index];
        if (table is not null)
            return table;
        _sparseRenderComponentsList[index] = table = new RenderComponents<T>();
        RenderComponentsList.Add(table);
        return table;
    }

    internal void Stop()
    {
        _stopAction?.Invoke();
        IsStarted = false;
    }

    internal void Update()
    {
        if (!IsInitialized)
            Initialize();
        if (!IsStarted)
            Start();
        _preUpdateAction?.Invoke();
        _updateAction?.Invoke();
        _postUpdateAction?.Invoke();
        for (
            Time.FixedAccumulator += Time.Delta;
            Time.FixedAccumulator >= Time.FixedDelta;
            Time.FixedAccumulator -= Time.FixedDelta
        )
            FixedUpdate();
        Render();
    }

    internal void Destroy(in Entity entity)
    {
        if (IsDeferred)
        {
            Enqueue(Event.Destroy(entity));
            return;
        }

        _destroyAction?.Invoke(entity);
        var tables = Tables().WithHidden().AsValueEnumerable().Where(t => t.Type != typeof(EntityTag));
        var entityTables = entity.Tables().WithHidden().AsValueEnumerable().Where(t => t.Type != typeof(EntityTag));
        var flag = Core.Table.Flags.SilentOnImmutable;
        do
        {
            foreach (var table in tables)
                table.Remove(entity, flag);
            flag = Core.Table.Flags.ForceMutable;
        } while (entityTables.Any());

        EntityTagTable.Remove(entity, Core.Table.Flags.ForceMutable);
        if (Scope == entity)
            Scope = Core.Entity.Null;
        ref var info = ref _entities[entity.Index];
        info.Index = 0;
        _freeIndices.Enqueue(entity.Index);
    }

    internal void Enqueue(in Event @event)
    {
        _events.Enqueue(@event);
    }

    internal bool IsValid(in Entity entity)
    {
        if ((uint)entity.Index == 0 || entity.Index >= _entities.Count)
            return false;
        var info = _entities[entity.Index];
        return info.Index == entity.Index && info.Version == entity.Version;
    }

    private void Initialize()
    {
        if (!IsConfigured)
        {
            _systems = Ecs.Systems.Invoke().AsValueEnumerable().Concat(_systemsFunc.Invoke()).ToValueList();
            _systems.Sort();
            foreach (var system in _systems)
                system.Configure(this);
            IsConfigured = true;
        }

        IsInitialized = true;
        _initializeAction?.Invoke();
        Time.Restart();
    }

    private void Start()
    {
        _startAction?.Invoke();
        IsStarted = true;
    }

    private void FixedUpdate()
    {
        UpdateInterpolatedEntities();
        _preFixedUpdateAction?.Invoke();
        _fixedUpdateAction?.Invoke();
        _postFixedUpdateAction?.Invoke();
    }

    private void RestartAction()
    {
        if (IsStarted)
            Stop();
        Clear();
        IsInitialized = false;
    }

    private void TryFlush()
    {
        if (_deferredCount != 0 || _isFlushing)
            return;
        _isFlushing = true;
        while (_events.TryDequeue(out var @event))
            try
            {
                switch (@event.EventType)
                {
                    case EventType.Instantiate:
                        _instantiateAction?.Invoke(new Entity(@event.EntityId, this));
                        break;
                    case EventType.Destroy:
                        Destroy(new Entity(@event.EntityId, this));
                        break;
                    case EventType.Custom:
                        _customEvents[(Type)@event.Data].DequeueAction.Invoke();
                        break;
                    case EventType.TableOperation:
                        ((Table)@event.Data).DequeueOperation();
                        break;
                    case EventType.TableEvent:
                        ((Table)@event.Data).DequeueEvent();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        _isFlushing = false;
        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
    }

    private void Render()
    {
        var commands = new RenderCommands(this);
        _preRenderAction?.Invoke();
        try
        {
            _renderAction?.Invoke(commands);
        }
        finally
        {
            commands.Execute();
        }

        _postRenderAction?.Invoke();
    }

    private void UpdateInterpolatedEntities()
    {
        foreach (var entity in AssignableEntities<IInterpolated>())
        {
            ref var interpolation = ref InterpolationTable.GetRef(entity).Value;
            var transform = entity.Transform;
            Interpolation oldInterpolation;
            if (Unsafe.IsNullRef(ref interpolation))
            {
                SuspendDefer();
                interpolation = ref InterpolationTable.Set(entity, new Interpolation(transform, transform)).Value;
                oldInterpolation = new Interpolation();
                ResumeDefer();
            }
            else
            {
                oldInterpolation = interpolation;
                interpolation.Start = transform;
                if (Precision.AreEqual(interpolation.Start, oldInterpolation.Start))
                    continue;
            }

            InterpolationTable.Enqueue(Core.Table.Event<Interpolation>.Set(entity, oldInterpolation, interpolation));
        }
    }

    ~Scene()
    {
        if (_onDispose is not null)
            Game.Defer(_onDispose);
    }

    public void OnInstantiate(Action<Entity> action)
    {
        ThrowIfConfigured();
        _instantiateAction += action;
    }

    public void OnDestroy(Action<Entity> action)
    {
        ThrowIfConfigured();
        _destroyAction += action;
    }

    public void OnAdd<T>(Action<Entity> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd((entity, _) => action.Invoke(entity));
    }

    public void OnAdd<T>(Action<T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd((_, value) => action.Invoke(value));
    }

    public void OnAdd<T>(Action<Entity, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd(action);
    }

    public void OnAddOrSet<T>(Action<Entity> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd((entity, _) => action.Invoke(entity));
        Table<T>().OnSet((entity, _, _) => action.Invoke(entity));
    }

    public void OnAddOrSet<T>(Action<T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd((_, value) => action.Invoke(value));
        Table<T>().OnSet((_, _, value) => action.Invoke(value));
    }

    public void OnAddOrSet<T>(Action<Entity, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnAdd(action);
        Table<T>().OnSet((entity, _, value) => action.Invoke(entity, value));
    }

    public void OnSet<T>(Action<Entity> action)
    {
        ThrowIfConfigured();
        Table<T>().OnSet((entity, _, _) => action.Invoke(entity));
    }

    public void OnSet<T>(Action<T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnSet((_, _, value) => action.Invoke(value));
    }

    public void OnSet<T>(Action<T, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnSet((_, oldValue, newValue) => action.Invoke(oldValue, newValue));
    }

    public void OnSet<T>(Action<Entity, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnSet((entity, _, value) => action.Invoke(entity, value));
    }

    public void OnSet<T>(Action<Entity, T, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnSet(action);
    }

    public void OnRemove<T>(Action<Entity> action)
    {
        ThrowIfConfigured();
        Table<T>().OnRemove((entity, _) => action.Invoke(entity));
    }

    public void OnRemove<T>(Action<T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnRemove((_, value) => action.Invoke(value));
    }

    public void OnRemove<T>(Action<Entity, T> action)
    {
        ThrowIfConfigured();
        Table<T>().OnRemove(action);
    }

    public struct TableEnumerable : IStructEnumerable<TableEnumerator, Table>
    {
        private readonly Scene _scene;
        private bool _withHidden;

        internal TableEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public TableEnumerator GetEnumerator()
        {
            return new TableEnumerator(_scene, _withHidden);
        }

        public ValueEnumerable<StructEnumerator<TableEnumerator, Table>, Table> AsValueEnumerable()
        {
            return new StructEnumerator<TableEnumerator, Table>(GetEnumerator());
        }

        public ref TableEnumerable WithHidden(bool withHidden = true)
        {
            _withHidden = withHidden;
            return ref this;
        }
    }

    public struct TableEnumerator : IStructEnumerator<Table>
    {
        private readonly Scene _scene;
        private readonly bool _withHidden;
        private int _index;

        internal TableEnumerator(Scene scene, bool withHidden)
        {
            _scene = scene;
            _withHidden = withHidden;
            Reset();
        }

        public bool MoveNext()
        {
            while (true)
            {
                if ((uint)_index < (uint)_scene._tables.Count)
                {
                    var table = _scene._tables[_index];
                    _index++;
                    if (!_withHidden && table.IsHidden)
                        continue;
                    Current = table;
                    return true;
                }

                Current = null!;
                _index = -1;
                return false;
            }
        }

        public void Reset()
        {
            _index = 0;
            Current = null!;
        }

        public Table Current { get; private set; } = null!;

        public void Dispose() { }
    }

    public struct TableEnumerable<T> : IStructEnumerable<TableEnumerator<T>, Table>
    {
        private readonly Scene _scene;
        private bool _withHidden;

        internal TableEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public TableEnumerator<T> GetEnumerator()
        {
            return new TableEnumerator<T>(_scene, _withHidden);
        }

        public ValueEnumerable<StructEnumerator<TableEnumerator<T>, Table>, Table> AsValueEnumerable()
        {
            return new StructEnumerator<TableEnumerator<T>, Table>(GetEnumerator());
        }

        public ref TableEnumerable<T> WithHidden(bool withHidden = true)
        {
            _withHidden = withHidden;
            return ref this;
        }
    }

    public struct TableEnumerator<T> : IStructEnumerator<Table>
    {
        private readonly Scene _scene;
        private readonly bool _withHidden;
        private TableEnumerator _enumerator;

        internal TableEnumerator(Scene scene, bool withHidden)
        {
            _scene = scene;
            _withHidden = withHidden;
            Reset();
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var table = _enumerator.Current;
                if (!typeof(T).IsAssignableFrom(table.Type))
                    continue;
                Current = table;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _enumerator = _scene.Tables().WithHidden(_withHidden).GetEnumerator();
            Current = null!;
        }

        public Table Current { get; private set; } = null!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public readonly struct SystemEnumerable : IStructEnumerable<SystemEnumerator, IGameSystem>
    {
        private readonly Scene _scene;

        internal SystemEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public SystemEnumerator GetEnumerator()
        {
            return new SystemEnumerator(_scene);
        }

        public ValueEnumerable<SystemEnumerator, IGameSystem> AsValueEnumerable()
        {
            return new ValueEnumerable<SystemEnumerator, IGameSystem>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<SystemEnumerator, IGameSystem>, IGameSystem> IStructEnumerable<
            SystemEnumerator,
            IGameSystem
        >.AsValueEnumerable()
        {
            return new StructEnumerator<SystemEnumerator, IGameSystem>(GetEnumerator());
        }
    }

    public struct SystemEnumerator : IStructEnumerator<IGameSystem>, IValueEnumerator<IGameSystem>
    {
        private readonly Scene _scene;
        private int _index;

        internal SystemEnumerator(Scene scene)
        {
            _scene = scene;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_scene._systems.Count)
            {
                Current = _scene._systems[_index];
                _index++;
                return true;
            }

            Current = null!;
            _index = -1;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            Current = null!;
        }

        public IGameSystem Current { get; private set; } = null!;

        public void Dispose() { }

        public bool TryGetNext(out IGameSystem current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _scene._systems.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<IGameSystem> span)
        {
            span = _scene._systems.AsSpan();
            return true;
        }

        public bool TryCopyTo(scoped Span<IGameSystem> destination, Index offset)
        {
            return _scene._systems.AsSpan().TryCopyTo(destination, offset);
        }
    }

    public readonly struct SystemEnumerable<T> : IStructEnumerable<SystemEnumerator<T>, T>
        where T : IGameSystem
    {
        private readonly Scene _scene;

        internal SystemEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public SystemEnumerator<T> GetEnumerator()
        {
            return new SystemEnumerator<T>(_scene);
        }

        public ValueEnumerable<StructEnumerator<SystemEnumerator<T>, T>, T> AsValueEnumerable()
        {
            return new StructEnumerator<SystemEnumerator<T>, T>(GetEnumerator());
        }
    }

    public struct SystemEnumerator<T> : IStructEnumerator<T>
        where T : IGameSystem
    {
        private readonly Scene _scene;
        private SystemEnumerator _enumerator;

        internal SystemEnumerator(Scene scene)
        {
            _scene = scene;
            Reset();
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current is not T system)
                    continue;
                Current = system;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _enumerator = _scene.Systems().GetEnumerator();
            Current = default!;
        }

        public T Current { get; private set; } = default!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    internal readonly record struct Event
    {
        public Event(EventType eventType, ulong entityId, object data)
        {
            EntityId = entityId;
            Data = data;
            EventType = eventType;
        }

        public ulong EntityId { get; }
        public object Data { get; }
        public EventType EventType { get; }

        public static Event Instantiate(in Entity entity)
        {
            return new Event(EventType.Instantiate, entity.Id, null!);
        }

        public static Event Destroy(in Entity entity)
        {
            return new Event(EventType.Destroy, entity.Id, null!);
        }

        public static Event Custom(Type type)
        {
            return new Event(EventType.Custom, 0, type);
        }

        public static Event TableOperation(Table table)
        {
            return new Event(EventType.TableOperation, 0, table);
        }

        public static Event TableEvent(Table table)
        {
            return new Event(EventType.TableEvent, 0, table);
        }
    }

    internal enum EventType : byte
    {
        Instantiate,
        Destroy,
        Custom,
        TableOperation,
        TableEvent,
    }

    #region Callbacks

    private void OnSetPosition(Entity entity, Position position)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var nullTransform = Unsafe.IsNullRef(ref transform);
        var oldTransform = nullTransform ? new Transform() : transform;
        if (!nullTransform && Precision.AreEqual(oldTransform.Position, position))
            return;
        if (nullTransform)
        {
            SuspendDefer();
            transform = ref TransformTable.Set(entity, new Transform { Position = position }).Value;
            ResumeDefer();
        }
        else
        {
            transform.Position = position;
        }

        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetScale(Entity entity, Scale scale)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var nullTransform = Unsafe.IsNullRef(ref transform);
        var oldTransform = nullTransform ? new Transform() : transform;
        if (!nullTransform && Precision.AreEqual(oldTransform.Scale, scale))
            return;
        if (nullTransform)
        {
            SuspendDefer();
            transform = ref TransformTable.Set(entity, new Transform { Scale = scale }).Value;
            ResumeDefer();
        }
        else
        {
            transform.Scale = scale;
        }

        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetRotation(Entity entity, Rotation rotation)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var nullTransform = Unsafe.IsNullRef(ref transform);
        var oldTransform = nullTransform ? new Transform() : transform;
        if (!nullTransform && Precision.AreEqual(oldTransform.Rotation, rotation))
            return;
        if (nullTransform)
        {
            SuspendDefer();
            transform = ref TransformTable.Set(entity, new Transform { Rotation = rotation }).Value;
            ResumeDefer();
        }
        else
        {
            transform.Rotation = rotation;
        }

        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetPivotPoint(Entity entity, PivotPoint pivotPoint)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var nullTransform = Unsafe.IsNullRef(ref transform);
        var oldTransform = nullTransform ? new Transform() : transform;
        if (!nullTransform && Precision.AreEqual(oldTransform.PivotPoint, pivotPoint))
            return;
        if (nullTransform)
        {
            SuspendDefer();
            transform = ref TransformTable.Set(entity, new Transform { PivotPoint = pivotPoint }).Value;
            ResumeDefer();
        }
        else
        {
            transform.PivotPoint = pivotPoint;
        }

        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetTransform(Entity entity, Transform transform)
    {
        ref var position = ref PositionTable.GetRef(entity).Value;
        var positionNull = Unsafe.IsNullRef(ref position);
        var oldPosition = positionNull ? default : position;
        var positionChanged = positionNull || !Precision.AreEqual(transform.Position, oldPosition);
        ref var scale = ref ScaleTable.GetRef(entity).Value;
        var scaleNull = Unsafe.IsNullRef(ref scale);
        var oldScale = scaleNull ? new Scale() : scale;
        var scaleChanged = scaleNull || !Precision.AreEqual(transform.Scale, oldScale);
        ref var rotation = ref RotationTable.GetRef(entity).Value;
        var rotationNull = Unsafe.IsNullRef(ref rotation);
        var oldRotation = rotationNull ? default : rotation;
        var rotationChanged = rotationNull || !Precision.AreEqual(transform.Rotation, oldRotation);
        ref var pivotPoint = ref PivotPointTable.GetRef(entity).Value;
        var pivotPointNull = Unsafe.IsNullRef(ref pivotPoint);
        var oldPivotPoint = pivotPointNull ? default : pivotPoint;
        var pivotPointChanged = pivotPointNull || !Precision.AreEqual(transform.PivotPoint, oldPivotPoint);
        ref var interpolation = ref InterpolationTable.GetRef(entity).Value;
        var interpolationNull = Unsafe.IsNullRef(ref interpolation);
        var oldInterpolation = interpolationNull ? new Interpolation() : interpolation;
        var interpolationChanged = interpolationNull || !Precision.AreEqual(transform, interpolation.End);
        if (!positionChanged && !scaleChanged && !rotationChanged && !pivotPointChanged && !interpolationChanged)
            return;
        if (positionChanged)
        {
            if (positionNull)
            {
                SuspendDefer();
                position = ref PositionTable.Set(entity, transform.Position).Value;
                ResumeDefer();
            }
            else
            {
                position.Value = transform.Position;
            }
        }

        if (scaleChanged)
        {
            if (scaleNull)
            {
                SuspendDefer();
                scale = ref ScaleTable.Set(entity, transform.Scale).Value;
                ResumeDefer();
            }
            else
            {
                scale.Value = transform.Scale;
            }
        }

        if (rotationChanged)
        {
            if (rotationNull)
            {
                SuspendDefer();
                rotation = ref RotationTable.Set(entity, transform.Rotation).Value;
                ResumeDefer();
            }
            else
            {
                rotation.Value = transform.Rotation;
            }
        }

        if (pivotPointChanged)
        {
            if (pivotPointNull)
            {
                SuspendDefer();
                pivotPoint = ref PivotPointTable.Set(entity, transform.PivotPoint).Value;
                ResumeDefer();
            }
            else
            {
                pivotPoint.Value = transform.PivotPoint;
            }
        }

        if (interpolationChanged)
        {
            if (interpolationNull)
            {
                SuspendDefer();
                interpolation = ref InterpolationTable.Set(entity, new Interpolation(null, transform)).Value;
                ResumeDefer();
            }
            else
            {
                interpolation.End = transform;
            }
        }

        if (positionChanged)
            PositionTable.Emit(Core.Table.Event<Position>.Set(entity, oldPosition, transform.Position));
        if (scaleChanged)
            ScaleTable.Emit(Core.Table.Event<Scale>.Set(entity, oldScale, transform.Scale));
        if (rotationChanged)
            RotationTable.Emit(Core.Table.Event<Rotation>.Set(entity, oldRotation, transform.Rotation));
        if (pivotPointChanged)
            PivotPointTable.Emit(Core.Table.Event<PivotPoint>.Set(entity, oldPivotPoint, transform.PivotPoint));
        if (interpolationChanged)
            InterpolationTable.Emit(Core.Table.Event<Interpolation>.Set(entity, oldInterpolation, interpolation));
    }

    private void OnAddChild(Entity entity, Child child)
    {
        var parentId = child.ParentId;
        if (parentId == 0)
            return;
        var parentEntity = new Entity(parentId, this);
        parentEntity.AssertValid();
        var parentRef = ParentTable.GetRef(parentEntity);
        if (parentRef.IsNull)
        {
            SuspendDefer();
            parentRef = ParentTable.Set(parentEntity, new Parent(), Core.Table.Flags.ForceMutable);
            ResumeDefer();
        }

        ref var parent = ref parentRef.Value;
        ref var childRef = ref ChildTable.GetRef(entity).Value;
        var childId = entity.Id;
        childRef.PreviousSiblingId = parent.LastChildId;
        childRef.NextSiblingId = 0;
        if (parent.LastChildId != 0)
        {
            var lastEntity = new Entity(parent.LastChildId, this);
            ref var lastChild = ref ChildTable.GetRef(lastEntity).Value;
            lastChild.NextSiblingId = childId;
        }
        else
        {
            parent.FirstChildId = childId;
        }

        parent.LastChildId = childId;
    }

    private void OnSetChild(Entity entity, Child oldChild, Child newChild)
    {
        var oldParentId = oldChild.ParentId;
        var newParentId = newChild.ParentId;
        if (oldParentId == newParentId)
            return;
        if (oldParentId != 0)
            OnRemoveChild(entity, oldChild);
        if (newParentId != 0)
            OnAddChild(entity, newChild);
    }

    private void OnRemoveChild(Entity entity, Child child)
    {
        var parentId = child.ParentId;
        if (parentId == 0)
            return;
        var parentEntity = new Entity(parentId, this);
        var parentRef = ParentTable.GetRef(parentEntity);
        if (parentRef.IsNull)
            return;
        ref var parent = ref parentRef.Value;
        var prevId = child.PreviousSiblingId;
        var nextId = child.NextSiblingId;
        if (prevId != 0)
        {
            var prevEntity = new Entity(prevId, this);
            ref var prev = ref ChildTable.GetRef(prevEntity).Value;
            prev.NextSiblingId = nextId;
        }
        else
        {
            parent.FirstChildId = nextId;
        }

        if (nextId != 0)
        {
            var nextEntity = new Entity(nextId, this);
            ref var next = ref ChildTable.GetRef(nextEntity).Value;
            next.PreviousSiblingId = prevId;
        }
        else
        {
            parent.LastChildId = prevId;
        }

        if (parent.FirstChildId == 0)
            ParentTable.Remove(parentEntity, Core.Table.Flags.ForceMutable);
    }

    private void OnRemoveParent(Parent parent)
    {
        var childId = parent.FirstChildId;
        while (childId != 0)
        {
            var child = new Entity(childId, this);
            ref var childRef = ref ChildTable.GetRef(child).Value;
            childId = childRef.NextSiblingId;
            ChildTable.Remove(child);
        }
    }

    private void OnRemoveName(Name name)
    {
        _nameMap.Remove(name);
    }

    #endregion
}
