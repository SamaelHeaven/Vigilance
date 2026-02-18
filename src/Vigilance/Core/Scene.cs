#pragma warning disable CS9084

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public sealed partial class Scene
{
    private readonly Dictionary<Type, (ICollection Queue, Action EmitAction)> _customEvents = [];
    private readonly Dictionary<Type, Delegate> _listeners = [];
    private readonly Dictionary<string, ulong> _nameMap = [];
    private readonly List<RenderCommand> _renderCommands = [];
    private readonly GameSystemsFunc _systemsFunc;
    private Action? _deferredAction;
    private ValueList<Table> _denseTables = [];
    private Action<Entity>? _destroyAction;
    private ValueList<(int Index, int Generation)> _entities = [];
    private ValueQueue<Event> _events = [];
    private Action? _fixedUpdateAction;
    private ValueQueue<int> _freeIndices = [];
    private Action? _initializeAction;
    private Action<Entity>? _instantiateAction;
    private bool _isEndingDefer;
    private Action? _onDispose;
    private Action? _postFixedUpdateAction;
    private Action? _postRenderAction;
    private Action? _postUpdateAction;
    private Action? _preFixedUpdateAction;
    private Action? _preRenderAction;
    private Action? _preUpdateAction;
    private Action<RenderCommands>? _renderAction;
    private ValueList<Table?> _sparseTables = [];
    private Action? _startAction;
    private bool _started;
    private Action? _stopAction;
    private List<IGameSystem> _systems = null!;
    private float _time;
    private Action? _updateAction;
    internal Table<Child> ChildTable;
    internal Table<Disabled> DisabledTable;
    internal Table<Name> NameTable;
    internal Table<Parent> ParentTable;
    internal Table<PivotPoint> PivotPointTable;
    internal Table<Position> PositionTable;
    internal Table<Rotation> RotationTable;
    internal Table<Scale> ScaleTable;
    internal Table<Transform> TransformTable;
    internal Table<ZIndex> ZIndexTable;

    public Scene(GameSystemsFunc? systems = null)
    {
        _entities.Add((0, 0));
        _systemsFunc = systems ?? Array.Empty<IGameSystem>;
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
        OnSet<Position>(OnSetPosition);
        OnSet<Scale>(OnSetScale);
        OnSet<Rotation>(OnSetRotation);
        OnSet<PivotPoint>(OnSetPivotPoint);
        OnSet<Transform>(OnSetTransform);
        OnAdd<Child>(OnAddChild);
        OnSet<Child>(OnSetChild);
        OnRemove<Child>(OnRemoveChild);
    }

    public ListView<IGameSystem> Systems => _systems ?? throw new NullReferenceException();

    public Camera Camera { get; } = new();

    public Entity Scope
    {
        get;
        set
        {
            EnsureInitialized();
            if (!value.IsNull)
                value.AssertValid();
            field = value;
        }
    }

    public bool IsInitialized { get; private set; }

    public bool IsDeferred => DeferredCount != 0 && SuspendedCount == 0;

    public int DeferredCount { get; private set; }

    public int SuspendedCount { get; private set; }

    public EntityEnumerable Entities => GetEntities();

    public TableEnumerable Tables => new(this);

    public void Restart()
    {
        if (!IsInitialized)
            return;
        var current = Game.Scene == this;
        if (current || IsDeferred)
        {
            Game.Defer(RestartAction);
            return;
        }

        RestartAction();
        return;

        void RestartAction()
        {
            if (current && _started)
                Stop();
            _time = 0;
            foreach (var entity in Entities)
                entity.Destroy();
            _initializeAction?.Invoke();
        }
    }

    public Entity Entity(string name = "")
    {
        EnsureInitialized();
        ulong id;
        var recycle = _freeIndices.Count > 0;
        ref var info = ref Unsafe.NullRef<(int Index, int Generation)>();
        if (recycle)
        {
            var index = _freeIndices.Peek();
            info = ref _entities[index];
            id = Core.Entity.GetId(index, info.Generation + 1);
        }
        else
        {
            id = Core.Entity.GetId(_entities.Count, 0);
        }

        name = name.Trim();
        if (name.IsEmpty)
            name = $"#{id}";
        ref var nameId = ref CollectionsMarshal.GetValueRefOrAddDefault(_nameMap, name, out var exists);
        if (exists)
            throw new InvalidOperationException($"Entity \"{name}\" already exists.");
        if (recycle)
        {
            info.Index = Core.Entity.GetIndex(id);
            info.Generation++;
            _freeIndices.Dequeue();
        }
        else
        {
            _entities.Add((_entities.Count, 0));
        }

        nameId = id;
        var entity = new Entity(id, this);
        SuspendDefer();
        NameTable.Set(entity, new Name(name));
        ZIndexTable.Set(entity, new ZIndex());
        PositionTable.Set(entity, new Position());
        ScaleTable.Set(entity, new Scale());
        RotationTable.Set(entity, new Rotation());
        PivotPointTable.Set(entity, new PivotPoint());
        TransformTable.Set(entity, new Transform());
        ResumeDefer();
        if (!Scope.IsNull)
            ChildTable.Set(entity, new Child(Scope.Id));
        if (IsDeferred)
            Enqueue(Event.Instantiate(entity));
        else
            _instantiateAction?.Invoke(entity);
        return entity;
    }

    public Entity Lookup(int index, int generation)
    {
        EnsureInitialized();
        if (index == 0 || index >= _entities.Count)
            return Core.Entity.Null;
        var info = _entities[index];
        if (info.Index != index || info.Generation != generation)
            return Core.Entity.Null;
        return new Entity(index, generation, this);
    }

    public Entity Lookup(ulong id)
    {
        EnsureInitialized();
        return Lookup(Core.Entity.GetIndex(id), Core.Entity.GetGeneration(id));
    }

    public Entity Lookup(string name)
    {
        EnsureInitialized();
        ref var id = ref CollectionsMarshal.GetValueRefOrNullRef(_nameMap, name);
        return Unsafe.IsNullRef(ref id) ? Core.Entity.Null : new Entity(id, this);
    }

    public void On<T>(Action<T> action)
    {
        EnsureNotInitialized();
        var type = typeof(T);
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_listeners, type, out var exists)!;
        if (!exists)
        {
            value = action;
            return;
        }

        var existing = (Action<T>)value;
        existing += action;
        _listeners[type] = existing;
    }

    public void OnInitialize(Action action)
    {
        EnsureNotInitialized();
        _initializeAction += action;
    }

    public void OnStart(Action action)
    {
        EnsureNotInitialized();
        _startAction += action;
    }

    public void OnStop(Action action)
    {
        EnsureNotInitialized();
        _stopAction += action;
    }

    public void OnDispose(Action action)
    {
        EnsureNotInitialized();
        _onDispose += action;
    }

    public void OnPreUpdate(Action action)
    {
        EnsureNotInitialized();
        _preUpdateAction += action;
    }

    public void OnUpdate(Action action)
    {
        EnsureNotInitialized();
        _updateAction += action;
    }

    public void OnPostUpdate(Action action)
    {
        EnsureNotInitialized();
        _postUpdateAction += action;
    }

    public void OnPreFixedUpdate(Action action)
    {
        EnsureNotInitialized();
        _preFixedUpdateAction += action;
    }

    public void OnFixedUpdate(Action action)
    {
        EnsureNotInitialized();
        _fixedUpdateAction += action;
    }

    public void OnPostFixedUpdate(Action action)
    {
        EnsureNotInitialized();
        _postFixedUpdateAction += action;
    }

    public void OnPreRender(Action action)
    {
        EnsureNotInitialized();
        _preRenderAction += action;
    }

    public void OnRender(Action<RenderCommands> action)
    {
        EnsureNotInitialized();
        _renderAction += action;
    }

    public void OnPostRender(Action action)
    {
        EnsureNotInitialized();
        _postRenderAction += action;
    }

    public void Emit<T>(in T @event)
    {
        EnsureInitialized();
        var type = typeof(T);
        if (!_listeners.TryGetValue(type, out var action))
            return;
        ((Action<T>)action).Invoke(@event);
    }

    public void Enqueue<T>(in T @event)
    {
        EnsureInitialized();
        if (!IsDeferred)
        {
            Emit(@event);
            return;
        }

        var type = typeof(T);
        ref var events = ref CollectionsMarshal.GetValueRefOrAddDefault(_customEvents, type, out var exists);
        if (!exists)
        {
            if (!_listeners.TryGetValue(type, out var action))
                return;
            var listener = (Action<T>)action;
            var queue = new Queue<T>();
            events = (
                queue,
                () =>
                {
                    if (queue.TryDequeue(out var @event))
                        listener.Invoke(@event);
                }
            );
        }

        ((Queue<T>)events.Queue).Enqueue(@event);
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

    public void EnsureInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Scene has not been initialized.");
    }

    public void EnsureNotInitialized()
    {
        if (IsInitialized)
            throw new InvalidOperationException("Scene has been initialized.");
    }

    public void BeginDefer()
    {
        DeferredCount++;
    }

    public void EndDefer()
    {
        if (!IsDeferred)
            throw new InvalidOperationException("Scene is not in a deferred state.");
        if (--DeferredCount != 0 || _isEndingDefer)
            return;
        _isEndingDefer = true;
        while (_events.TryDequeue(out var @event))
            switch (@event.EventType)
            {
                case EventType.Instantiate:
                    _instantiateAction?.Invoke(new Entity(@event.EntityId, this));
                    break;
                case EventType.Destroy:
                    Destroy(new Entity(@event.EntityId, this));
                    break;
                case EventType.Custom:
                    _customEvents[(Type)@event.Data].EmitAction.Invoke();
                    break;
                case EventType.TableOperation:
                    ((Table)@event.Data).DequeueOperation();
                    break;
                case EventType.TableEvent:
                    ((Table)@event.Data).DequeueEvent();
                    break;
            }

        _isEndingDefer = false;
        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
    }

    public void SuspendDefer()
    {
        SuspendedCount++;
    }

    public void ResumeDefer()
    {
        SuspendedCount--;
    }

    public Entity SetScope(in Entity entity)
    {
        EnsureInitialized();
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
        _denseTables.Add(table);
        return table;
    }

    internal void Stop()
    {
        _stopAction?.Invoke();
        _started = false;
    }

    internal void Update()
    {
        if (!IsInitialized)
            Initialize();
        if (!_started)
            Start();
        _preUpdateAction?.Invoke();
        _updateAction?.Invoke();
        _postUpdateAction?.Invoke();
        for (_time += Time.DeltaSeconds; _time >= Time.FixedDeltaSeconds; _time -= Time.FixedDeltaSeconds)
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
        var name = entity.Name;
        var tables = entity.Tables.WithHidden();
        do
        {
            foreach (var table in tables)
                table.Remove(entity, Core.Table.Flags.ForceMutable);
        } while (tables.AsValueEnumerable().Any());

        _nameMap.Remove(name);
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
        if (entity.Index == 0 || entity.Index >= _entities.Count)
            return false;
        var info = _entities[entity.Index];
        return info.Index == entity.Index && info.Generation == entity.Generation;
    }

    private void Initialize()
    {
        _systems = Ecs.Systems.Invoke().AsValueEnumerable().Concat(_systemsFunc.Invoke()).ToList();
        _systems.Sort();
        foreach (var system in _systems)
            system.Configure(this);
        IsInitialized = true;
        _initializeAction?.Invoke();
        Time.Restart();
    }

    private void Start()
    {
        _startAction?.Invoke();
        _started = true;
    }

    private void FixedUpdate()
    {
        _preFixedUpdateAction?.Invoke();
        _fixedUpdateAction?.Invoke();
        _postFixedUpdateAction?.Invoke();
    }

    private void Render()
    {
        var commands = new RenderCommands(_renderCommands);
        _preRenderAction?.Invoke();
        _renderAction?.Invoke(commands);
        commands.Execute();
        _postRenderAction?.Invoke();
    }

    ~Scene()
    {
        if (_onDispose is not null)
            Game.Defer(_onDispose);
    }

    public void OnInstantiate(Action<Entity> action)
    {
        EnsureNotInitialized();
        _instantiateAction += action;
    }

    public void OnDestroy(Action<Entity> action)
    {
        EnsureNotInitialized();
        _destroyAction += action;
    }

    public void OnAdd<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd((entity, _) => action.Invoke(entity));
    }

    public void OnAdd<T>(Action<T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd((_, value) => action.Invoke(value));
    }

    public void OnAdd<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd(action);
    }

    public void OnAddOrSet<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd((entity, _) => action.Invoke(entity));
        Table<T>().OnSet((entity, _, _) => action.Invoke(entity));
    }

    public void OnAddOrSet<T>(Action<T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd((_, value) => action.Invoke(value));
        Table<T>().OnSet((_, _, value) => action.Invoke(value));
    }

    public void OnAddOrSet<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnAdd(action);
        Table<T>().OnSet((entity, _, value) => action.Invoke(entity, value));
    }

    public void OnSet<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        Table<T>().OnSet((entity, _, _) => action.Invoke(entity));
    }

    public void OnSet<T>(Action<T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnSet((_, _, value) => action.Invoke(value));
    }

    public void OnSet<T>(Action<T, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnSet((_, oldValue, newValue) => action.Invoke(oldValue, newValue));
    }

    public void OnSet<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnSet((entity, _, value) => action.Invoke(entity, value));
    }

    public void OnSet<T>(Action<Entity, T, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnSet(action);
    }

    public void OnRemove<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        Table<T>().OnRemove((entity, _) => action.Invoke(entity));
    }

    public void OnRemove<T>(Action<T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnRemove((_, value) => action.Invoke(value));
    }

    public void OnRemove<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        Table<T>().OnRemove(action);
    }

    public unsafe struct TableEnumerable : IStructEnumerable<TableEnumerator, Table>
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
            do
            {
                if (_index + 1 >= _scene._denseTables.Count)
                    return false;
                Current = _scene._denseTables[++_index];
            } while (!_withHidden && Current.IsHidden);

            return true;
        }

        public void Reset()
        {
            _index = -1;
            Current = null!;
        }

        public Table Current { get; private set; } = null!;

        public void Dispose() { }
    }

    internal readonly record struct Event(EventType EventType, ulong EntityId, object Data)
    {
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

    internal enum EventType
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
        var oldTransform = transform;
        if (Precision.AreEqual(oldTransform.Position, position))
            return;
        transform.Position = position;
        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetScale(Entity entity, Scale scale)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var oldTransform = transform;
        if (Precision.AreEqual(oldTransform.Scale, scale))
            return;
        transform.Scale = scale;
        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetRotation(Entity entity, Rotation rotation)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var oldTransform = transform;
        if (Precision.AreEqual(oldTransform.Rotation, rotation))
            return;
        transform.Rotation = rotation;
        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetPivotPoint(Entity entity, PivotPoint pivotPoint)
    {
        ref var transform = ref TransformTable.GetRef(entity).Value;
        var oldTransform = transform;
        if (Precision.AreEqual(oldTransform.PivotPoint, pivotPoint))
            return;
        transform.PivotPoint = pivotPoint;
        TransformTable.Emit(Core.Table.Event<Transform>.Set(entity, oldTransform, transform));
    }

    private void OnSetTransform(Entity entity, Transform transform)
    {
        ref var position = ref PositionTable.GetRef(entity).Value;
        var oldPosition = position;
        var positionChanged = !Precision.AreEqual(transform.Position, oldPosition);
        if (positionChanged)
            position.Value = transform.Position;
        ref var scale = ref ScaleTable.GetRef(entity).Value;
        var oldScale = scale;
        var scaleChanged = !Precision.AreEqual(transform.Scale, oldScale);
        if (scaleChanged)
            scale.Value = transform.Scale;
        ref var rotation = ref RotationTable.GetRef(entity).Value;
        var oldRotation = rotation;
        var rotationChanged = !Precision.AreEqual(transform.Rotation, oldRotation);
        if (rotationChanged)
            rotation.Value = transform.Rotation;
        ref var pivotPoint = ref PivotPointTable.GetRef(entity).Value;
        var oldPivotPoint = pivotPoint;
        var pivotPointChanged = !Precision.AreEqual(transform.PivotPoint, oldPivotPoint);
        if (pivotPointChanged)
            pivotPoint.Value = transform.PivotPoint;
        if (positionChanged)
            PositionTable.Emit(Core.Table.Event<Position>.Set(entity, oldPosition, transform.Position));
        if (scaleChanged)
            ScaleTable.Emit(Core.Table.Event<Scale>.Set(entity, oldScale, transform.Scale));
        if (rotationChanged)
            RotationTable.Emit(Core.Table.Event<Rotation>.Set(entity, oldRotation, transform.Rotation));
        if (pivotPointChanged)
            PivotPointTable.Emit(Core.Table.Event<PivotPoint>.Set(entity, oldPivotPoint, transform.PivotPoint));
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
            parentRef = ParentTable.Set(parentEntity, new Parent());
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
            ParentTable.Remove(parentEntity);
    }

    #endregion
}
