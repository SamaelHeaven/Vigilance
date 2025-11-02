using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        ulong Id,
        Type Type,
        object? Data
    )> _componentOperations = new();

    private readonly Dictionary<Type, object> _events = new();
    private readonly List<RenderCommand> _renderCommands = new();
    private readonly GameSystemsFunc _systemsFunc;
    private readonly Queue<(TransformOperation Operation, ulong EntityId, Vector2 Data)> _transformOperations = new();
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
        OnSetParent(SetParentCallback);
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
        var id = entity.Id.Value;
        var result = new Entity(id, this);
        Cache.TransformMap.Add(id, new Transform());
        Cache.NameMap.Add(id, name == "" ? $"#{id}" : name);
        entity.Set(new ZIndex());
        entity.Set(new Position());
        entity.Set(new Scale());
        entity.Set(new Rotation());
        entity.Set(new PivotPoint());
        World.Event<AddEvent>().Id<ZIndex>().Entity(id).Enqueue();
        return result;
    }

    public Entity Lookup(ulong id)
    {
        EnsureInitialized();
        var result = new Entity(new Flecs.NET.Core.Entity(World.Handle, id), this);
        return result.IsValid ? result : Core.Entity.Null;
    }

    public Entity Lookup(string path, bool recursive = true)
    {
        EnsureInitialized();
        return new Entity(World.Lookup(path, recursive), this);
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
        ExecuteTransformOperations();
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

    internal void DeferSetPosition(ulong entityId, Vector2 position)
    {
        if (IsDeferred)
        {
            _transformOperations.Enqueue((TransformOperation.Position, entityId, position));
            return;
        }

        SetPosition(entityId, position);
    }

    internal void DeferSetScale(ulong entityId, Vector2 scale)
    {
        if (IsDeferred)
        {
            _transformOperations.Enqueue((TransformOperation.Scale, entityId, scale));
            return;
        }

        SetScale(entityId, scale);
    }

    internal void DeferSetRotation(ulong entityId, float rotation)
    {
        if (IsDeferred)
        {
            _transformOperations.Enqueue((TransformOperation.Rotation, entityId, rotation));
            return;
        }

        SetRotation(entityId, rotation);
    }

    internal void DeferSetPivotPoint(ulong entityId, Vector2 pivotPoint)
    {
        if (IsDeferred)
        {
            _transformOperations.Enqueue((TransformOperation.PivotPoint, entityId, pivotPoint));
            return;
        }

        SetPivotPoint(entityId, pivotPoint);
    }

    internal void DeferSetComponent(in Entity entity, Type type, object? data, ulong id)
    {
        if (IsDeferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Set, entity.Id, id, type, data));
            return;
        }

        SetComponent(entity, type, data, id);
    }

    internal void DeferRemoveComponent(in Entity entity, ulong id)
    {
        if (IsDeferred)
        {
            _componentOperations.Enqueue((ComponentOperation.Remove, entity.Id, id, null!, null));
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
        while (_componentOperations.TryDequeue(out var operation))
            switch (operation.Operation)
            {
                case ComponentOperation.Set:
                    SetComponent(new Entity(operation.EntityId, this), operation.Type, operation.Data, operation.Id);
                    break;
                case ComponentOperation.Remove:
                    RemoveComponent(new Entity(operation.EntityId, this), operation.Id);
                    break;
            }
    }

    private void ExecuteTransformOperations()
    {
        while (_transformOperations.TryDequeue(out var operation))
            switch (operation.Operation)
            {
                case TransformOperation.Position:
                    SetPosition(operation.EntityId, operation.Data);
                    break;
                case TransformOperation.Scale:
                    SetScale(operation.EntityId, operation.Data);
                    break;
                case TransformOperation.Rotation:
                    SetRotation(operation.EntityId, *(float*)&operation.Data);
                    break;
                case TransformOperation.PivotPoint:
                    SetPivotPoint(operation.EntityId, operation.Data);
                    break;
            }
    }

    private void SetPosition(ulong entityId, Vector2 position)
    {
        ref var transform = ref CollectionsMarshal.GetValueRefOrAddDefault(
            Cache.TransformMap,
            entityId,
            out var exists
        );
        if (!exists)
            transform.Scale = Vector2.One;
        transform.Position = position;
    }

    private void SetScale(ulong entityId, Vector2 scale)
    {
        ref var transform = ref CollectionsMarshal.GetValueRefOrAddDefault(
            Cache.TransformMap,
            entityId,
            out var exists
        );
        if (!exists)
            transform.Scale = Vector2.One;
        transform.Scale = scale;
    }

    private void SetRotation(ulong entityId, float rotation)
    {
        ref var transform = ref CollectionsMarshal.GetValueRefOrAddDefault(
            Cache.TransformMap,
            entityId,
            out var exists
        );
        if (!exists)
            transform.Scale = Vector2.One;
        transform.Rotation = rotation;
    }

    private void SetPivotPoint(ulong entityId, Vector2 pivotPoint)
    {
        ref var transform = ref CollectionsMarshal.GetValueRefOrAddDefault(
            Cache.TransformMap,
            entityId,
            out var exists
        );
        if (!exists)
            transform.Scale = Vector2.One;
        transform.PivotPoint = pivotPoint;
    }

    private static void SetComponent(in Entity entity, Type type, object? data, ulong id)
    {
        Components components;
        var flecsEntity = entity.FlecsEntity;
        ref readonly var componentsRef = ref flecsEntity.GetSafe<Components>();
        if (Unsafe.IsNullRef(in componentsRef))
        {
            components = new Components();
            entity.FlecsEntity.Set(components);
        }
        else
        {
            components = componentsRef;
        }

        var component = new Component(type, data, id);
        components.Values.Remove(component);
        components.Values.Add(component);
    }

    private static void RemoveComponent(in Entity entity, ulong id)
    {
        var flecsEntity = entity.FlecsEntity;
        ref readonly var components = ref flecsEntity.GetSafe<Components>();
        if (Unsafe.IsNullRef(in components))
            return;
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

    private enum TransformOperation
    {
        Position,
        Scale,
        Rotation,
        PivotPoint,
    }

    #region Callbacks

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

    public void OnSetPosition(Action<Entity> action, bool traverse = false)
    {
        OnSet<Position>(action, traverse);
    }

    public void OnSetPosition(Action<Entity, Vector2> action, bool traverse = false)
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

    public void OnSetScale(Action<Entity> action, bool traverse = false)
    {
        OnSet<Scale>(action, traverse);
    }

    public void OnSetScale(Action<Entity, Vector2> action, bool traverse = false)
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

    public void OnSetRotation(Action<Entity> action, bool traverse = false)
    {
        OnSet<Rotation>(action, traverse);
    }

    public void OnSetRotation(Action<Entity, float> action, bool traverse = false)
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

    public void OnSetPivotPoint(Action<Entity> action, bool traverse = false)
    {
        OnSet<PivotPoint>(action, traverse);
    }

    public void OnSetPivotPoint(Action<Entity, Vector2> action, bool traverse = false)
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

    public void OnSetZIndex(Action<Entity> action, bool traverse = false)
    {
        OnSet<ZIndex>(action, traverse);
    }

    public void OnSetZIndex(Action<Entity, int> action, bool traverse = false)
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
