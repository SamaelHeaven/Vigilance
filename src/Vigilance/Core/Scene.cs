using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed unsafe partial class Scene
{
    private static readonly delegate* unmanaged[Cdecl]<ulong, void*, ulong, void*, int> OrderByCallback =
        &CompareEntities;

    private static readonly Stack<Scene> Contexts = new();
    private static Scene _context = null!;

    private readonly Queue<(
        ComponentOperation Operation,
        Flecs.NET.Core.Entity Entity,
        Type Type,
        object? Data
    )> _componentOperations = new();

    private readonly Dictionary<Type, object> _events = new();
    private readonly GameSystemsFunc _systemsFunc;
    private Action? _deferredAction;
    private Action? _fixedUpdateAction;
    private Action? _initializeAction;
    private Action? _onDestroy;
    private Query<ZIndex> _orderedQuery;
    private Action<Entity>? _renderAction;
    private Action? _beginRenderAction;
    private Action? _endRenderAction;
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
        _orderedQuery = BuildOrderedQuery();
    }

    public EnumerableList<IGameSystem> Systems
    {
        get
        {
            EnsureInitialized();
            return _systems;
        }
    }

    public Camera Camera { get; } = new();

    public bool Initialized { get; private set; }

    public bool Deferred => _world.IsDeferred();

    public EntityEnumerable Entities => GetEntities();

    public OrderedEntityEnumerable OrderedEntities => GetOrderedEntities();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int CompareEntities(ulong id1, void* zIndex1, ulong id2, void* zIndex2)
    {
        var scene = _context;
        var e1 = new Entity(scene._world.Entity(id1), scene);
        var e2 = new Entity(scene._world.Entity(id2), scene);
        var result = e1.WorldZIndex.CompareTo(e2.WorldZIndex);
        return result == 0 ? id1.CompareTo(id2) : result;
    }

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
        var entity = name == "" ? _world.Entity() : _world.Entity(name);
        if (!entity.Has<ZIndex>())
            entity.Set(new ZIndex());
        if (!entity.Has<Position>())
            entity.Set(new Position());
        if (!entity.Has<Scale>())
            entity.Set(new Scale());
        if (!entity.Has<Rotation>())
            entity.Set(new Rotation());
        if (!entity.Has<PivotPoint>())
            entity.Set(new PivotPoint());
        return new Entity(entity, this);
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

        var existing = value as Action<T>;
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

    public void OnDestroy(Action action)
    {
        EnsureNotInitialized();
        _onDestroy += action;
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

    public int Count()
    {
        EnsureInitialized();
        return _world.Count<ZIndex>();
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
        Contexts.Push(this);
        _context = this;
        if (!Deferred)
            _world.DeferBegin();
    }

    internal void EndDefer()
    {
        if (!Deferred || !_world.DeferEnd())
            return;
        while (_componentOperations.TryDequeue(out var component))
            switch (component.Operation)
            {
                case ComponentOperation.Set:
                    SetComponent(component.Entity, component.Type, component.Data);
                    break;
                case ComponentOperation.Remove:
                    RemoveComponent(component.Entity, component.Type);
                    break;
            }

        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
        _context = Contexts.Count == 0 ? null! : Contexts.Pop();
    }

    private void Initialize()
    {
        _systems = Game.Systems.Invoke().Concat(_systemsFunc.Invoke()).ToList();
        foreach (var system in _systems)
            system.Configure(this);
        Initialized = true;
        _initializeAction?.Invoke();
        Time.Restart();
    }

    private Query<ZIndex> BuildOrderedQuery()
    {
        var queryBuilder = _world.QueryBuilder<ZIndex>();
        queryBuilder.Desc.order_by = Type<ZIndex>.Id(_world);
        queryBuilder.Desc.order_by_callback = (nint)OrderByCallback;
        return queryBuilder.Build();
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
            foreach (var entity in OrderedEntities)
                _renderAction.Invoke(entity);
        _endRenderAction?.Invoke();
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

        components.Values.Remove(new Component(type));
    }

    ~Scene()
    {
        Game.Defer(() =>
        {
            _onDestroy?.Invoke();
            _orderedQuery.Dispose();
            _world.Dispose();
        });
    }

    private enum ComponentOperation
    {
        Set,
        Remove,
    }

    #region OnAdd

    public void OnAdd<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter it, int i, ref T _) =>
                {
                    action.Invoke(new Entity(it.Entity(i), this));
                }
            );
    }

    public void OnAdd<T>(Action<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter _, int _, ref T t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnAdd<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter it, int i, ref T t) =>
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
            .Event<SetEvent>()
            .Each(
                (Iter it, int i, ref T _) =>
                {
                    var entity = new Entity(it.Entity(i), this);
                    if (!traverse)
                    {
                        action.Invoke(entity);
                        return;
                    }

                    entity.Traverse<T>(action);
                }
            );
    }

    public void OnSet<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<SetEvent>()
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    if (!traverse)
                    {
                        action.Invoke(t);
                        return;
                    }

                    var entity = new Entity(it.Entity(i), this);
                    entity.Traverse(action);
                }
            );
    }

    public void OnSet<T>(Action<Entity, T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<SetEvent>()
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    var entity = new Entity(it.Entity(i), this);
                    if (!traverse)
                    {
                        action.Invoke(entity, t);
                        return;
                    }

                    entity.Traverse(action);
                }
            );
    }

    #endregion

    #region OnRemove

    public void OnRemove<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event(Ecs.OnRemove)
            .Each(
                (Iter it, int i, ref T _) =>
                {
                    action.Invoke(new Entity(it.Entity(i), this));
                }
            );
    }

    public void OnRemove<T>(Action<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event(Ecs.OnRemove)
            .Each(
                (Iter _, int _, ref T t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnRemove<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event(Ecs.OnRemove)
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    action.Invoke(new Entity(it.Entity(i), this), t);
                }
            );
    }

    #endregion

    #region OnSetPosition

    public void OnSetPosition(Action<Entity> action, bool traverse = true)
    {
        OnSet<Position>(action.Invoke, traverse);
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
        OnSet<Scale>(action.Invoke, traverse);
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
        OnSet<Rotation>(action.Invoke, traverse);
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
        OnSet<PivotPoint>(action.Invoke, traverse);
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
        OnSet<ZIndex>(action.Invoke, traverse);
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
}
