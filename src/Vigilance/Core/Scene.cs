using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Vigilance.Collections;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;
using ZLinq.Internal;

namespace Vigilance.Core;

public sealed unsafe partial class Scene
{
    private readonly Dictionary<Type, (ICollection Queue, Action EmitAction)> _events = new();
    private readonly Dictionary<Type, Delegate> _listeners = new();
    private readonly List<RenderCommand> _renderCommands = [];
    private readonly GameSystemsFunc _systemsFunc;
    private Action? _deferredAction;
    private Action? _fixedUpdateAction;
    private Action? _initializeAction;
    private Action? _onDispose;
    private Action? _postFixedUpdateAction;
    private Action? _postRenderAction;
    private Action? _postUpdateAction;
    private Action? _preFixedUpdateAction;
    private Action? _preRenderAction;
    private Action? _preUpdateAction;
    private Action<RenderCommands>? _renderAction;
    private Action? _startAction;
    private bool _started;
    private Action? _stopAction;
    private List<IGameSystem> _systems = null!;
    private float _time;
    private Action? _updateAction;
    internal CachedData Cache;
    internal World World = World.Create();

    public Scene(GameSystemsFunc? systems = null)
    {
        _systemsFunc = systems ?? Array.Empty<IGameSystem>;
        Cache = new CachedData(this);
        OnSetParent(SetParentCallback);
    }

    public ListView<IGameSystem> Systems => _systems ?? throw new NullReferenceException();

    public Camera Camera { get; } = new();

    public bool IsInitialized { get; private set; }

    public bool IsDeferred => DeferredCount != 0;

    public int DeferredCount { get; private set; }

    public EntityEnumerable Entities => GetEntities();

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
        if (name.IsEmpty)
            entity = World.Entity();
        else
            entity =
                World.Lookup(name, false).Id.Value != 0
                    ? throw new InvalidOperationException($"Entity \"{name}\" already exists.")
                    : World.Entity(name);
        var id = entity.Id.Value;
        var result = new Entity(id, this);
        var deferred = IsDeferred;
        if (deferred)
            flecs.ecs_defer_suspend(World);
        entity.Set(new ZIndex());
        entity.Set(new Position());
        entity.Set(new Scale());
        entity.Set(new Rotation());
        entity.Set(new PivotPoint());
        if (deferred)
            flecs.ecs_defer_resume(World);
        Cache.TransformMap.Add(id, new Transform());
        Cache.NameMap.Add(id, name.IsEmpty ? $"#{id}" : name);
        World.Event<AddEvent>().Id<ZIndex>().Entity(id).Enqueue();
        return result;
    }

    public Component Component<T>()
    {
        EnsureInitialized();
        ComponentMetadata<T>.EnsureInitialized();
        return new Component(Type<T>.Id(World), this, typeof(T));
    }

    public Component Component(ulong id)
    {
        EnsureInitialized();
        if (id == 0)
            return default;
        ref Type? type = ref CollectionsMarshal.GetValueRefOrNullRef(Cache.ComponentMap, id)!;
        if (!Unsafe.IsNullRef(in type))
            return new Component(id, this, type);
        var entity = new Flecs.NET.Core.Entity(World, id);
        if (!entity.IsAlive())
            return default;
        type = ref entity.GetSafe<Type>()!;
        if (Unsafe.IsNullRef(in type))
            return default;
        Cache.ComponentMap.Add(id, type);
        return new Component(id, this, type);
    }

    public ComponentEnumerable Components()
    {
        EnsureInitialized();
        return new ComponentEnumerable(this);
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
        ref var events = ref CollectionsMarshal.GetValueRefOrAddDefault(_events, type, out var exists);
        if (!exists)
        {
            var queue = new Queue<T>();
            events = (
                queue,
                () =>
                {
                    while (queue.TryDequeue(out var @event))
                        Emit(@event);
                }
            );
        }

        ((Queue<T>)events.Queue).Enqueue(@event);
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
        if (0 != DeferredCount++)
            return;
        World.DeferBegin();
    }

    public void EndDefer()
    {
        if (!IsDeferred)
            throw new InvalidOperationException("Scene is not in a deferred state.");
        if (--DeferredCount != 0)
            return;
        World.DeferEnd();
        var action = _deferredAction;
        _deferredAction = null;
        action?.Invoke();
        EmitEvents();
    }

    public void SuspendDefer()
    {
        EnsureInitialized();
        World.DeferSuspend();
    }

    public void ResumeDefer()
    {
        EnsureInitialized();
        World.DeferResume();
    }

    public Entity SetScope(in Entity entity)
    {
        var oldScope = World.SetScope(entity.Id);
        return new Entity(oldScope.Id.Value, this);
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

    private void Initialize()
    {
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

    private void EmitEvents()
    {
        foreach (var events in _events.Values)
            events.EmitAction.Invoke();
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
        public readonly Dictionary<ulong, string> NameMap = new();
        public readonly Dictionary<ulong, Entity> ParentMap = new();
        public readonly Dictionary<ulong, Transform> TransformMap = new();
        public readonly Dictionary<ulong, Type> ComponentMap = new();
        public readonly ulong PositionId;
        public readonly ulong ScaleId;
        public readonly ulong RotationId;
        public readonly ulong PivotPointId;
        public readonly ulong ZIndexId;

        public CachedData(Scene scene)
        {
            PositionId = Type<Position>.Id(scene.World);
            ScaleId = Type<Scale>.Id(scene.World);
            RotationId = Type<Rotation>.Id(scene.World);
            PivotPointId = Type<PivotPoint>.Id(scene.World);
            ZIndexId = Type<ZIndex>.Id(scene.World);
        }
    }

    public readonly struct ComponentEnumerable : IStructEnumerable<ComponentEnumerator, Component>
    {
        private readonly Scene _scene;

        internal ComponentEnumerable(Scene scene)
        {
            _scene = scene;
        }

        public ComponentEnumerator GetEnumerator()
        {
            return new ComponentEnumerator(_scene);
        }

        public ValueEnumerable<ComponentEnumerator, Component> AsValueEnumerable()
        {
            return new ValueEnumerable<ComponentEnumerator, Component>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<ComponentEnumerator, Component>, Component> IStructEnumerable<
            ComponentEnumerator,
            Component
        >.AsValueEnumerable()
        {
            return new StructEnumerator<ComponentEnumerator, Component>(GetEnumerator());
        }
    }

    public struct ComponentEnumerator : IStructEnumerator<Component>, IValueEnumerator<Component>
    {
        private readonly Scene _scene;
        private int _index;
        private int _count;
        private Component[]? _array;

        internal ComponentEnumerator(Scene scene)
        {
            _index = -1;
            _scene = scene;
        }

        public Component Current => _array![_index];

        public bool MoveNext()
        {
            if (_array is null)
            {
                _count = ComponentMetadata.Map.Count;
                _array = ArrayPool<Component>.Shared.Rent(_count);
                var i = 0;
                foreach (var metadata in ComponentMetadata.Map.Values)
                    _array[i++] = new Component(metadata.IdFunc.Invoke(_scene), _scene, metadata.Type);
            }

            if (_index + 1 >= _count)
                return false;
            _index++;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public bool TryGetNext(out Component current)
        {
            if (!MoveNext())
            {
                Unsafe.SkipInit(out current);
                return false;
            }

            current = Current;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<Component> span)
        {
            span = new ReadOnlySpan<Component>(_array, 0, _count);
            return true;
        }

        public bool TryCopyTo(scoped Span<Component> destination, Index offset)
        {
            if (
                !EnumeratorHelper.TryGetSlice(
                    new ReadOnlySpan<Component>(_array, 0, _count),
                    offset,
                    destination.Length,
                    out var slice
                )
            )
                return false;
            slice.CopyTo(destination);
            return true;
        }

        public void Dispose()
        {
            if (_array is not null)
                ArrayPool<Component>.Shared.Return(_array);
        }
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
        Cache.NameMap.Remove(entity.Id);
        Cache.TransformMap.Remove(entity.Id);
    }

    #endregion

    #region OnAdd

    public void OnAdd<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                (it, i, ref _) =>
                {
                    action.Invoke(new Entity(it.Handle->entities[i], this));
                }
            );
    }

    public void OnAdd<T>(Action<T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                (_, _, ref t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnAdd<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<AddEvent>()
            .Each(
                (it, i, ref t) =>
                {
                    action.Invoke(new Entity(it.Handle->entities[i], this), t);
                }
            );
    }

    #endregion

    #region OnSet

    public void OnSet<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                (it, i, ref _) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnSet<T>(Action<T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                (_, _, ref t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnSet<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event<SetEvent>()
            .Each(
                (it, i, ref t) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity, t);
                }
            );
    }

    #endregion

    #region OnAddOrSet

    public void OnAddOrSet<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                (it, i, ref _) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity);
                }
            );
    }

    public void OnAddOrSet<T>(Action<T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                (_, _, ref t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnAddOrSet<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnSet)
            .Each(
                (it, i, ref t) =>
                {
                    var entity = new Entity(it.Handle->entities[i], this);
                    action.Invoke(entity, t);
                }
            );
    }

    #endregion

    #region OnRemove

    public void OnRemove<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i, ref _) =>
                {
                    action.Invoke(new Entity(it.Handle->entities[i], this));
                }
            );
    }

    public void OnRemove<T>(Action<T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (_, _, ref t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnRemove<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        World
            .Observer<T>()
            .With(Flecs.NET.Core.Ecs.Disabled)
            .Optional()
            .Event(Flecs.NET.Core.Ecs.OnRemove)
            .Each(
                (it, i, ref t) =>
                {
                    action.Invoke(new Entity(it.Handle->entities[i], this), t);
                }
            );
    }

    #endregion

    #region OnSetPosition

    public void OnSetPosition(Action<Entity> action)
    {
        OnSet<Position>(action);
    }

    public void OnSetPosition(Action<Entity, Vector2> action)
    {
        OnSet(
            (Entity entity, Position position) =>
            {
                action.Invoke(entity, position.Value);
            }
        );
    }

    #endregion

    #region OnSetScale

    public void OnSetScale(Action<Entity> action)
    {
        OnSet<Scale>(action);
    }

    public void OnSetScale(Action<Entity, Vector2> action)
    {
        OnSet(
            (Entity entity, Scale scale) =>
            {
                action.Invoke(entity, scale.Value);
            }
        );
    }

    #endregion

    #region OnSetRotation

    public void OnSetRotation(Action<Entity> action)
    {
        OnSet<Rotation>(action);
    }

    public void OnSetRotation(Action<Entity, float> action)
    {
        OnSet(
            (Entity entity, Rotation rotation) =>
            {
                action.Invoke(entity, rotation.Value);
            }
        );
    }

    #endregion

    #region OnSetPivotPoint

    public void OnSetPivotPoint(Action<Entity> action)
    {
        OnSet<PivotPoint>(action);
    }

    public void OnSetPivotPoint(Action<Entity, Vector2> action)
    {
        OnSet(
            (Entity entity, PivotPoint pivotPoint) =>
            {
                action.Invoke(entity, pivotPoint.Value);
            }
        );
    }

    #endregion

    #region OnSetZIndex

    public void OnSetZIndex(Action<Entity> action)
    {
        OnSet<ZIndex>(action);
    }

    public void OnSetZIndex(Action<Entity, int> action)
    {
        OnSet(
            (Entity entity, ZIndex zIndex) =>
            {
                action.Invoke(entity, zIndex.Value);
            }
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
