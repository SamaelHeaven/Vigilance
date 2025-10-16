using Flecs.NET.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public sealed unsafe partial class Scene
{
    private readonly Queue<(
        ComponentOperation Operation,
        ulong EntityId,
        Type Type,
        object? Data
    )> _componentOperations = new();

    private readonly Dictionary<Type, object> _events = new();
    private readonly List<ulong> _sortedEntities = [];
    private readonly Dictionary<ulong, int> _sortedEntityIndex = [];
    private readonly GameSystemsFunc _systemsFunc;
    private Action? _beginRenderAction;
    private int _deferred;
    private Action? _deferredAction;
    private Action? _endRenderAction;
    private Action? _fixedUpdateAction;
    private Action? _initializeAction;
    private Action? _onDispose;
    private Action<Entity>? _renderAction;
    private Action? _startAction;
    private bool _started;
    private Action? _stopAction;
    private List<IGameSystem> _systems = [];
    private float _time;
    private Action? _updateAction;
    private World _world = World.Create();

    public Scene(GameSystemsFunc? systems = null)
    {
        _systemsFunc = systems ?? Array.Empty<IGameSystem>;
        OnInstantiate(AddSortedEntity);
        OnSetZIndex(UpdateSortedEntity);
        OnDestroy(RemoveSortedEntity);
    }

    public ListView<IGameSystem> Systems
    {
        get
        {
            EnsureInitialized();
            return _systems;
        }
    }

    public SortedEntityEnumerable SortedEntities => new(this);

    public Camera Camera { get; } = new();

    public bool Initialized { get; private set; }

    public bool Deferred => _deferred != 0;

    public EntityEnumerable Entities => GetEntities();

    public static Scene Build<T>(GameSystemsFunc? systems = null)
        where T : IGameSystem, new()
    {
        return new Scene(() => systems is null ? [new T()] : systems.Invoke().Concat([new T()]));
    }

    public static Scene Build<T>(Func<T> factory, GameSystemsFunc? systems = null)
        where T : IGameSystem
    {
        return new Scene(() => systems is null ? [factory.Invoke()] : systems.Invoke().Concat([factory.Invoke()]));
    }

    public void Restart()
    {
        if (!Initialized)
            return;
        var current = Game.Scene == this;
        if (current || Deferred)
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
        Flecs.NET.Core.Entity entity;
        if (name == "")
            entity = _world.Entity();
        else
            entity =
                _world.Lookup(name) != Flecs.NET.Core.Entity.Null()
                    ? throw new InvalidOperationException($"Entity \"{name}\" already exists.")
                    : _world.Entity(name);
        entity.Set(new Position());
        entity.Set(new Scale());
        entity.Set(new Rotation());
        entity.Set(new PivotPoint());
        entity.Set(new ZIndex());
        _world.Event<AddEvent>().Id<ZIndex>().Entity(entity).Enqueue();
        return new Entity(entity, this);
    }

    public Entity Lookup(ulong id)
    {
        EnsureInitialized();
        var result = new Entity(new Flecs.NET.Core.Entity(_world.Handle, id), this);
        return result.Valid ? result : Core.Entity.Null;
    }

    public Entity Lookup(string name)
    {
        EnsureInitialized();
        return new Entity(_world.Lookup(name), this);
    }

    public void On<T>(Action<T> action)
    {
        EnsureNotInitialized();
        var type = typeof(T);
        if (!_events.TryGetValue(type, out var value))
        {
            value = action;
            _events.Add(type, value);
            return;
        }

        var existing = (Action<T>)value;
        existing += action;
        _events[type] = existing;
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

    public void OnUpdate(Action action)
    {
        EnsureNotInitialized();
        _updateAction += action;
    }

    public void OnFixedUpdate(Action action)
    {
        EnsureNotInitialized();
        _fixedUpdateAction += action;
    }

    public void OnBeginRender(Action action)
    {
        EnsureNotInitialized();
        _beginRenderAction += action;
    }

    public void OnEndRender(Action action)
    {
        EnsureNotInitialized();
        _endRenderAction += action;
    }

    public void OnRender(Action<Entity> action)
    {
        EnsureNotInitialized();
        _renderAction += action;
    }

    public void Emit<T>(T @event)
    {
        Emit(ref @event);
    }

    public void Emit<T>(ref T @event)
    {
        EnsureInitialized();
        var type = typeof(T);
        if (!_events.TryGetValue(type, out var action))
            return;
        ((Action<T>)action).Invoke(@event);
    }

    public void Enqueue<T>(T @event)
    {
        Enqueue(ref @event);
    }

    public void Enqueue<T>(ref T @event)
    {
        EnsureInitialized();
        if (!Deferred)
        {
            Emit(@event);
            return;
        }

        var data = @event;
        Defer(() => Emit(data));
    }

    public int Count()
    {
        EnsureInitialized();
        return _sortedEntities.Count;
    }

    public int Count<T>()
    {
        EnsureInitialized();
        return _world.Count<T>();
    }

    public void Defer(Action action)
    {
        EnsureInitialized();
        if (Deferred)
        {
            _deferredAction += action;
            return;
        }

        action.Invoke();
    }

    public void EnsureInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Scene has not been initialized.");
    }

    public void EnsureNotInitialized()
    {
        if (Initialized)
            throw new InvalidOperationException("Scene has been initialized.");
    }

    internal void DeferSetComponent(Flecs.NET.Core.Entity entity, Type type, object? date)
    {
        if (Deferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Set, entity, type, date));
            return;
        }

        SetComponent(entity, type, date);
    }

    internal void DeferRemoveComponent(Flecs.NET.Core.Entity entity, Type type)
    {
        if (Deferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Remove, entity, type, null));
            return;
        }

        RemoveComponent(entity, type);
    }

    internal void Stop()
    {
        _stopAction?.Invoke();
        _started = false;
    }

    internal void Update()
    {
        if (!Initialized)
            Initialize();
        if (!_started)
            Start();
        _updateAction?.Invoke();
        for (_time += Time.DeltaSeconds; _time >= Time.FixedDeltaSeconds; _time -= Time.FixedDeltaSeconds)
            FixedUpdate();
        Render();
    }

    internal void BeginDefer()
    {
        if (0 != _deferred++)
            return;
        _world.DeferBegin();
    }

    internal void EndDefer()
    {
        if (!Deferred)
            return;
        if (--_deferred != 0)
            return;
        _world.DeferEnd();
        ExecuteComponentOperations();
        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
    }

    private void Initialize()
    {
        _systems = Game.Systems.Invoke().AsValueEnumerable().Concat(_systemsFunc.Invoke()).ToList();
        foreach (var system in _systems)
            system.Configure(this);
        Initialized = true;
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
        _fixedUpdateAction?.Invoke();
    }

    private void Render()
    {
        _beginRenderAction?.Invoke();
        if (_renderAction is not null)
            foreach (var entity in SortedEntities)
                _renderAction.Invoke(entity);
        _endRenderAction?.Invoke();
    }

    private void ExecuteComponentOperations()
    {
        if (_componentOperations.Count == 0)
            return;
        while (_componentOperations.TryDequeue(out var component))
            switch (component.Operation)
            {
                case ComponentOperation.Set:
                    SetComponent(new Flecs.NET.Core.Entity(_world, component.EntityId), component.Type, component.Data);
                    break;
                case ComponentOperation.Remove:
                    RemoveComponent(new Flecs.NET.Core.Entity(_world, component.EntityId), component.Type);
                    break;
            }
    }

    private void AddSortedEntity(Entity entity)
    {
        var id = entity.Id;
        var index = BinarySearchSortedEntity(entity);
        _sortedEntities.Insert(index, id);
        _sortedEntityIndex[id] = index;
    }

    private void UpdateSortedEntity(Entity entity)
    {
        var id = entity.Id;
        var oldIndex = _sortedEntityIndex[id];
        _sortedEntities.RemoveAt(oldIndex);
        var index = BinarySearchSortedEntity(entity);
        _sortedEntities.Insert(index, id);
        _sortedEntityIndex[id] = index;
    }

    private void RemoveSortedEntity(Entity entity)
    {
        var id = entity.Id;
        var oldIndex = _sortedEntityIndex[id];
        _sortedEntities.RemoveAt(oldIndex);
    }

    private int BinarySearchSortedEntity(in Entity entity)
    {
        var order = entity.Order;
        var start = 0;
        var end = _sortedEntities.Count;
        var middle = end / 2;
        while (end > start)
        {
            int comparison;
            var otherEntity = new Entity(new Flecs.NET.Core.Entity(_world, _sortedEntities[middle]), this);
            if ((comparison = otherEntity.Order.CompareTo(order)) == 0)
                return middle;
            if (comparison > 0)
                end = middle;
            else
                start = middle + 1;
            middle = start + (end - start) / 2;
        }

        return middle;
    }

    private static void SetComponent(Flecs.NET.Core.Entity entity, Type type, object? data)
    {
        Components components;
        if (!entity.Has<Components>())
        {
            components = new Components();
            entity.Set(components);
        }
        else
        {
            components = entity.Get<Components>();
        }

        var component = new Component(type, data);
        components.Values.Remove(component);
        components.Values.Add(component);
    }

    private static void RemoveComponent(Flecs.NET.Core.Entity entity, Type type)
    {
        if (!entity.Has<Components>())
            return;
        var components = entity.Get<Components>();
        components.Values.Remove(new Component(type));
        if (components.Count == 0)
            entity.Remove<Components>();
    }

    ~Scene()
    {
        Game.Defer(() =>
        {
            _onDispose?.Invoke();
            _world.Dispose();
        });
    }

    public void OnInstantiate(Action<Entity> action)
    {
        OnAdd<ZIndex>(action);
    }

    public void OnDestroy(Action<Entity> action)
    {
        OnRemove<ZIndex>(action);
    }

    public readonly struct SortedEntityEnumerable
        : IStructEnumerable<SortedEntityEnumerator, Entity>,
            IReadOnlyCollection<Entity>
    {
        private readonly Scene _scene;

        internal SortedEntityEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public SortedEntityEnumerator GetEnumerator()
        {
            return new SortedEntityEnumerator(_scene);
        }

        public ValueEnumerable<StructEnumerator<SortedEntityEnumerator, Entity>, Entity> AsValueEnumerable()
        {
            return new StructEnumerator<SortedEntityEnumerator, Entity>(GetEnumerator());
        }

        public int Count => _scene._sortedEntities.Count;
    }

    public struct SortedEntityEnumerator : IStructEnumerator<Entity>
    {
        private readonly Scene _scene;
        private readonly List<ulong> _sortedEntities;
        private int _index;

        internal SortedEntityEnumerator(Scene scene)
        {
            _scene = scene;
            _sortedEntities = scene._sortedEntities;
            Reset();
        }

        public bool MoveNext()
        {
            return ++_index < _sortedEntities.Count;
        }

        public void Reset()
        {
            _index = -1;
            _scene.BeginDefer();
        }

        public Entity Current => new(new Flecs.NET.Core.Entity(_scene._world, _sortedEntities[_index]), _scene);

        public void Dispose()
        {
            if (_index == -1)
                return;
            _scene.EndDefer();
            _index = -1;
        }
    }

    private enum ComponentOperation
    {
        Set,
        Remove,
    }

    #region OnAdd

    public void OnAdd<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref _) =>
                    {
                        action.Invoke(new Entity(it.Entity(i), this));
                    }
            );
    }

    public void OnAdd<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (_, _, ref t) =>
                    {
                        action.Invoke(t);
                    }
            );
    }

    public void OnAdd<T>(Action<Entity, T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        action.Invoke(new Entity(it.Entity(i), this), t);
                    }
            );
    }

    #endregion

    #region OnSet

    public void OnSet<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse<T>(action);
                    }
                    : (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        action.Invoke(entity);
                    }
            );
    }

    public void OnSet<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (_, _, ref t) =>
                    {
                        action.Invoke(t);
                    }
            );
    }

    public void OnSet<T>(Action<Entity, T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        action.Invoke(entity, t);
                    }
            );
    }

    #endregion

    #region OnAddOrSet

    public void OnAddOrSet<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse<T>(action);
                    }
                    : (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        action.Invoke(entity);
                    }
            );
    }

    public void OnAddOrSet<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (_, _, ref t) =>
                    {
                        action.Invoke(t);
                    }
            );
    }

    public void OnAddOrSet<T>(Action<Entity, T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        action.Invoke(entity, t);
                    }
            );
    }

    #endregion

    #region OnRemove

    public void OnRemove<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref _) =>
                    {
                        action.Invoke(new Entity(it.Entity(i), this));
                    }
            );
    }

    public void OnRemove<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (_, _, ref t) =>
                    {
                        action.Invoke(t);
                    }
            );
    }

    public void OnRemove<T>(Action<Entity, T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .With(Ecs.Disabled)
            .Optional()
            .Event(Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Entity(i), this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        action.Invoke(new Entity(it.Entity(i), this), t);
                    }
            );
    }

    #endregion

    #region OnSetPosition

    public void OnSetPosition(Action<Entity> action, bool traverse = true)
    {
        OnSet<Position>(action, traverse);
    }

    public void OnSetPosition(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, Position position) =>
            {
                action.Invoke(entity, position.Value);
            },
            traverse
        );
    }

    #endregion

    #region OnSetScale

    public void OnSetScale(Action<Entity> action, bool traverse = true)
    {
        OnSet<Scale>(action, traverse);
    }

    public void OnSetScale(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, Scale scale) =>
            {
                action.Invoke(entity, scale.Value);
            },
            traverse
        );
    }

    #endregion

    #region OnSetRotation

    public void OnSetRotation(Action<Entity> action, bool traverse = true)
    {
        OnSet<Rotation>(action, traverse);
    }

    public void OnSetRotation(Action<Entity, float> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, Rotation rotation) =>
            {
                action.Invoke(entity, rotation.Value);
            },
            traverse
        );
    }

    #endregion

    #region OnSetPivotPoint

    public void OnSetPivotPoint(Action<Entity> action, bool traverse = true)
    {
        OnSet<PivotPoint>(action, traverse);
    }

    public void OnSetPivotPoint(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, PivotPoint pivotPoint) =>
            {
                action.Invoke(entity, pivotPoint.Value);
            },
            traverse
        );
    }

    #endregion

    #region OnSetZIndex

    public void OnSetZIndex(Action<Entity> action, bool traverse = true)
    {
        OnSet<ZIndex>(action, traverse);
    }

    public void OnSetZIndex(Action<Entity, int> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ZIndex zIndex) =>
            {
                action.Invoke(entity, zIndex.Value);
            },
            traverse
        );
    }

    #endregion

    #region OnSetDisabled

    public void OnSetDisabled(Action<Entity> action)
    {
        EnsureNotInitialized();
        _world
            .Observer()
            .Flags(Ecs.Disabled)
            .Event(Ecs.OnAdd)
            .Event(Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Entity(i), this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnSetDisabled(Action<Entity, bool> action)
    {
        EnsureNotInitialized();
        _world
            .Observer()
            .Flags(Ecs.Disabled)
            .Event(Ecs.OnAdd)
            .Event(Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Entity(i), this);
                    action.Invoke(entity, it.Event() == Ecs.OnAdd);
                }
            );
    }

    #endregion
}
