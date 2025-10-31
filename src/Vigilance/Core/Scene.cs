using Flecs.NET.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public sealed unsafe partial class Scene
{
    private readonly Queue<(
        ComponentOperation Operation,
        ulong EntityId,
        Type Type,
        object? Data,
        ulong Id
    )> _componentOperations = new();

    private readonly Dictionary<Type, object> _events = new();
    private readonly List<RenderCommand> _renderCommands = new();
    private readonly GameSystemsFunc _systemsFunc;
    private int _deferred;
    private Action? _deferredAction;
    private Action? _fixedUpdateAction;
    private Action? _initializeAction;
    private bool? _isRuntimeComponentsEnabled;
    private Action? _onDispose;
    private Action? _postRenderAction;
    private Action? _preRenderAction;
    private Action<RenderCommands>? _renderAction;
    private Action? _startAction;
    private bool _started;
    private Action? _stopAction;
    private List<IGameSystem> _systems = [];
    private float _time;
    private Action? _updateAction;
    internal CachedData Cache;
    internal World World = World.Create();

    public Scene(GameSystemsFunc? systems = null, bool? isRuntimeComponentsEnabled = null)
    {
        _systemsFunc = systems ?? Array.Empty<IGameSystem>;
        _isRuntimeComponentsEnabled = isRuntimeComponentsEnabled;
        Cache = new CachedData(this);
        OnInstantiate(InstantiateCallback);
        OnSetPosition(SetPositionCallback, false);
        OnSetScale(SetScaleCallback, false);
        OnSetRotation(SetRotationCallback, false);
        OnSetPivotPoint(SetPivotPointCallback, false);
    }

    public ListView<IGameSystem> Systems
    {
        get
        {
            EnsureInitialized();
            return _systems;
        }
    }

    public bool IsRuntimeComponentsEnabled
    {
        get
        {
            EnsureInitialized();
            return _isRuntimeComponentsEnabled!.Value;
        }
    }

    public Camera Camera { get; } = new();

    public bool IsInitialized { get; private set; }

    public bool IsDeferred => _deferred != 0;

    public EntityEnumerable Entities => GetEntities();

    public static Scene Build<T>(GameSystemsFunc? systems = null, bool? isRuntimeComponentsEnabled = null)
        where T : IGameSystem, new()
    {
        return new Scene(
            () => (systems?.Invoke() ?? Array.Empty<IGameSystem>()).Concat([new T()]),
            isRuntimeComponentsEnabled
        );
    }

    public static Scene Build<T>(
        Func<T> factory,
        GameSystemsFunc? systems = null,
        bool? isRuntimeComponentsEnabled = null
    )
        where T : IGameSystem
    {
        return new Scene(
            () => (systems?.Invoke() ?? Array.Empty<IGameSystem>()).Concat([factory.Invoke()]),
            isRuntimeComponentsEnabled
        );
    }

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
        Flecs.NET.Core.Entity entity;
        if (name == "")
            entity = World.Entity();
        else
            entity =
                World.Lookup(name, false) != Flecs.NET.Core.Entity.Null()
                    ? throw new InvalidOperationException($"Entity \"{name}\" already exists.")
                    : World.Entity(name);
        entity.Set(new ZIndex());
        entity.Set(new Position());
        entity.Set(new Scale());
        entity.Set(new Rotation());
        entity.Set(new PivotPoint());
        var id = entity.Id.Value;
        var result = new Entity(id, this);
        World.Event<AddEvent>().Id<ZIndex>().Entity(id).Enqueue();
        return result;
    }

    public Entity Lookup(ulong id)
    {
        EnsureInitialized();
        var result = new Entity(new Flecs.NET.Core.Entity(World.Handle, id), this);
        return result.IsValid ? result : Core.Entity.Null;
    }

    public Entity Lookup(string name, bool recursive = true)
    {
        EnsureInitialized();
        return new Entity(World.Lookup(name, recursive), this);
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
        EnsureInitialized();
        if (!IsDeferred)
        {
            Emit(@event);
            return;
        }

        Defer(() => Emit(@event));
    }

    public void Enqueue<T>(ref T @event)
    {
        EnsureInitialized();
        if (!IsDeferred)
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
        return World.Count<ZIndex>();
    }

    public int Count<T>()
    {
        EnsureInitialized();
        return World.Count<T>();
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
        if (0 != _deferred++)
            return;
        World.DeferBegin();
    }

    public void EndDefer()
    {
        if (!IsDeferred)
            return;
        if (--_deferred != 0)
            return;
        World.DeferEnd();
        ExecuteComponentOperations();
        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
    }

    public Entity SetScope(in Entity entity)
    {
        var oldScope = World.SetScope(entity.Id);
        return new Entity(oldScope.Id.Value, this);
    }

    internal void DeferSetComponent(in Entity entity, Type type, object? data, ulong id)
    {
        if (IsDeferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Set, entity.Id, type, data, id));
            return;
        }

        SetComponent(entity, type, data, id);
    }

    internal void DeferRemoveComponent(in Entity entity, ulong id)
    {
        if (IsDeferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Remove, entity.Id, null!, null, id));
            return;
        }

        RemoveComponent(entity, id);
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
        _updateAction?.Invoke();
        for (_time += Time.DeltaSeconds; _time >= Time.FixedDeltaSeconds; _time -= Time.FixedDeltaSeconds)
            FixedUpdate();
        Render();
    }

    private void Initialize()
    {
        _isRuntimeComponentsEnabled ??= Ecs.DefaultEnableRuntimeComponents;
        _systems = Ecs.Systems.Invoke().AsValueEnumerable().Concat(_systemsFunc.Invoke()).ToList();
        _systems.Sort();
        BeginDefer();
        foreach (var system in _systems)
            system.Configure(this);
        EndDefer();
        OnRemoveParent(RemoveParentCallback);
        OnSetParent(SetParentCallback);
        OnDestroy(DestroyCallback);
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
        _fixedUpdateAction?.Invoke();
    }

    private void Render()
    {
        var commands = new RenderCommands(_renderCommands);
        _preRenderAction?.Invoke();
        _renderAction?.Invoke(commands);
        commands.Execute();
        _postRenderAction?.Invoke();
    }

    private void ExecuteComponentOperations()
    {
        while (_componentOperations.TryDequeue(out var component))
            switch (component.Operation)
            {
                case ComponentOperation.Set:
                    SetComponent(new Entity(component.EntityId, this), component.Type, component.Data, component.Id);
                    break;
                case ComponentOperation.Remove:
                    RemoveComponent(new Entity(component.EntityId, this), component.Id);
                    break;
            }
    }

    private static void SetComponent(in Entity entity, Type type, object? data, ulong id)
    {
        Components components;
        var flecsEntity = entity.FlecsEntity;
        if (flecsEntity.Has<Components>())
        {
            components = flecsEntity.Get<Components>();
        }
        else
        {
            components = new Components();
            entity.FlecsEntity.Set(components);
        }

        var component = new Component(type, data, id);
        components.Values.Remove(component);
        components.Values.Add(component);
    }

    private static void RemoveComponent(in Entity entity, ulong id)
    {
        var flecsEntity = entity.FlecsEntity;
        if (!flecsEntity.Has<Components>())
            return;
        var components = flecsEntity.Get<Components>();
        components.Values.Remove(new Component(null!, null, id));
        if (components.Count == 0)
            flecsEntity.Remove<Components>();
    }

    ~Scene()
    {
        Game.Defer(() =>
        {
            _onDispose?.Invoke();
            World.Dispose();
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

    internal readonly struct CachedData
    {
        public readonly HashSet<ulong> ImmediateDisabledSet = new();
        public readonly Dictionary<ulong, Vector2> ImmediatePivotPointMap = new();
        public readonly Dictionary<ulong, Vector2> ImmediatePositionMap = new();
        public readonly Dictionary<ulong, float> ImmediateRotationMap = new();
        public readonly Dictionary<ulong, Vector2> ImmediateScaleMap = new();
        public readonly Dictionary<ulong, int> ImmediateZIndexMap = new();
        public readonly Dictionary<ulong, string> NameMap = new();
        public readonly Dictionary<ulong, Entity> ParentMap = new();
        public readonly Dictionary<ulong, Transform> TransformMap = new();
        public readonly ulong ComponentsType;

        public CachedData(Scene scene)
        {
            ComponentsType = Type<Components>.Id(scene.World);
        }
    }

    private enum ComponentOperation
    {
        Set,
        Remove,
    }

    #region Callbacks

    private void InstantiateCallback(Entity entity)
    {
        var flecsEntity = entity.FlecsEntity;
        var id = entity.Id;
        var name = flecsEntity.Name();
        Cache.TransformMap.Add(id, new Transform());
        if (name != "")
            Cache.NameMap.Add(id, name);
    }

    private void SetPositionCallback(Entity entity, Vector2 position)
    {
        var id = entity.Id;
        Cache.TransformMap[id] = new Transform(position, entity.Scale, entity.Rotation, entity.PivotPoint);
    }

    private void SetScaleCallback(Entity entity, Vector2 scale)
    {
        var id = entity.Id;
        Cache.TransformMap[id] = new Transform(entity.Position, scale, entity.Rotation, entity.PivotPoint);
    }

    private void SetRotationCallback(Entity entity, float rotation)
    {
        var id = entity.Id;
        Cache.TransformMap[id] = new Transform(entity.Position, entity.Scale, rotation, entity.PivotPoint);
    }

    private void SetPivotPointCallback(Entity entity, Vector2 pivotPoint)
    {
        var id = entity.Id;
        Cache.TransformMap[id] = new Transform(entity.Position, entity.Scale, entity.Rotation, pivotPoint);
    }

    private void RemoveParentCallback(Entity entity)
    {
        Cache.ParentMap.Remove(entity.Id);
    }

    private void SetParentCallback(Entity entity, Entity parent)
    {
        Cache.ParentMap[entity.Id] = parent;
    }

    private void DestroyCallback(Entity entity)
    {
        var id = entity.Id;
        Cache.NameMap.Remove(id);
        Cache.TransformMap.Remove(id);
        Cache.ImmediateZIndexMap.Remove(id);
        Cache.ImmediatePositionMap.Remove(id);
        Cache.ImmediateScaleMap.Remove(id);
        Cache.ImmediateRotationMap.Remove(id);
        Cache.ImmediatePivotPointMap.Remove(id);
        Cache.ImmediateDisabledSet.Remove(id);
    }

    #endregion

    #region OnAdd

    public void OnAdd<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref _) =>
                    {
                        action.Invoke(new Entity(it.Handle->entities[i], this));
                    }
            );
    }

    public void OnAdd<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
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
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        action.Invoke(new Entity(it.Handle->entities[i], this), t);
                    }
            );
    }

    #endregion

    #region OnSet

    public void OnSet<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse<T>(action);
                    }
                    : (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        action.Invoke(entity);
                    }
            );
    }

    public void OnSet<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
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
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        action.Invoke(entity, t);
                    }
            );
    }

    #endregion

    #region OnAddOrSet

    public void OnAddOrSet<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse<T>(action);
                    }
                    : (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        action.Invoke(entity);
                    }
            );
    }

    public void OnAddOrSet<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
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
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        action.Invoke(entity, t);
                    }
            );
    }

    #endregion

    #region OnRemove

    public void OnRemove<T>(Action<Entity> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref _) =>
                    {
                        action.Invoke(new Entity(it.Handle->entities[i], this));
                    }
            );
    }

    public void OnRemove<T>(Action<T> action, bool traverse = false)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
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
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                traverse
                    ? (it, i, ref _) =>
                    {
                        var entity = new Entity(it.Handle->entities[i], this);
                        entity.Traverse(action);
                    }
                    : (it, i, ref t) =>
                    {
                        action.Invoke(new Entity(it.Handle->entities[i], this), t);
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
        World
            .Observer()
            .Flags(Flecs.NET.Core.Ecs.Disabled)
            .Event(Flecs.NET.Core.Ecs.OnAdd)
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnSetDisabled(Action<Entity, bool> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .Flags(Flecs.NET.Core.Ecs.Disabled)
            .Event(Flecs.NET.Core.Ecs.OnAdd)
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity, it.Event() == Flecs.NET.Core.Ecs.OnAdd);
                }
            );
    }

    public void OnDisable(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .Flags(Flecs.NET.Core.Ecs.Disabled)
            .Event(Flecs.NET.Core.Ecs.OnAdd)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnEnable(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .Flags(Flecs.NET.Core.Ecs.Disabled)
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    #endregion

    #region OnSetParent

    public void OnSetParent(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .With<ZIndex>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .With(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard)
            .Event(Flecs.NET.Core.Ecs.OnAdd)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnSetParent(Action<Entity, Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .With<ZIndex>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .With(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard)
            .Event(Flecs.NET.Core.Ecs.OnAdd)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    var parent = new Entity(entity.FlecsEntity.Parent(), this);
                    action.Invoke(entity, parent);
                }
            );
    }

    #endregion

    #region OnRemoveParent

    public void OnRemoveParent(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .With<ZIndex>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .With(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard)
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnRemoveParent(Action<Entity, Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer()
            .With<ZIndex>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .With(Flecs.NET.Core.Ecs.ChildOf, Flecs.NET.Core.Ecs.Wildcard)
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    var parent = new Entity(entity.FlecsEntity.Parent(), this);
                    action.Invoke(entity, parent);
                }
            );
    }

    #endregion
}
