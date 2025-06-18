using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed unsafe class Scene
{
    private static readonly delegate* unmanaged[Cdecl]<ulong, void*, ulong, void*, int> OrderByCallback =
        &CompareEntities;

    private static Scene _context = null!;
    private readonly Dictionary<Type, object> _events = new();
    private readonly SystemsFunc _systemsFunc;
    private Action? _deferredAction;
    private Action? _fixedUpdateAction;
    private Action? _initializeAction;
    private Action? _onDestroy;
    private Query<ZIndex> _orderedQuery;
    private Action<Entity>? _renderAction;
    private Action? _renderBeginAction;
    private Action? _renderEndAction;
    private Action? _startAction;
    private bool _started;
    private Action? _stopAction;
    private ImmutableList<ISystem> _systems = ImmutableList<ISystem>.Empty;
    private float _time;
    private Action? _updateAction;
    private World _world = World.Create();

    public Scene(SystemsFunc? systems = null)
    {
        _systemsFunc = systems ?? Array.Empty<ISystem>;
        _orderedQuery = BuildOrderedQuery();
    }

    public IReadOnlyList<ISystem> Systems
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

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int CompareEntities(ulong id1, void* zIndex1, ulong id2, void* zIndex2)
    {
        var scene = _context;
        var e1 = new Entity(scene._world.Entity(id1), scene);
        var e2 = new Entity(scene._world.Entity(id2), scene);
        var result = e1.WorldZIndex.CompareTo(e2.WorldZIndex);
        return result == 0 ? id1.CompareTo(id2) : result;
    }

    public void Restart()
    {
        if (!Initialized)
            return;
        var current = Game.Scene == this;
        if (current || _world.IsDeferred())
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
            Each(entity => entity.Destroy());
            _initializeAction?.Invoke();
        }
    }

    public Entity Entity(string name = "")
    {
        EnsureInitialized();
        var entity = _world.Entity(name);
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

    public void OnRenderBegin(Action action)
    {
        EnsureNotInitialized();
        _renderBeginAction += action;
    }

    public void OnRenderEnd(Action action)
    {
        EnsureNotInitialized();
        _renderEndAction += action;
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
        if (!_world.IsDeferred())
        {
            action.Invoke();
            return;
        }

        _deferredAction += action;
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

    private void Initialize()
    {
        _systems = Game.Systems.Invoke().Concat(_systemsFunc.Invoke()).ToImmutableList();
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

    internal void DeferBegin()
    {
        _context = this;
        if (!_world.IsDeferred())
            _world.DeferBegin();
    }

    internal void DeferEnd()
    {
        if (_world.IsDeferred())
        {
            _world.DeferEnd();
            var action = _deferredAction;
            _deferredAction = null;
            action?.Invoke();
        }

        _context = null!;
    }

    private void FixedUpdate()
    {
        _fixedUpdateAction?.Invoke();
    }

    private void Render()
    {
        _renderBeginAction?.Invoke();
        OrderedEach(entity =>
        {
            _renderAction?.Invoke(entity);
        });
        _renderEndAction?.Invoke();
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

    #region Each

    public void Each(Action<Entity> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) => action.Invoke(new Entity(entity, this)));
        DeferEnd();
    }

    public void Each<T0>(Action<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0) => action.Invoke(t0));
        DeferEnd();
    }

    public void Each<T0>(Action<Entity, T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((Flecs.NET.Core.Entity entity, ref T0 t0) => action.Invoke(new Entity(entity, this), t0));
        DeferEnd();
    }

    public void Each<T0, T1>(Action<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1) => action.Invoke(t0, t1));
        DeferEnd();
    }

    public void Each<T0, T1>(Action<Entity, T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1) => action.Invoke(new Entity(entity, this), t0, t1)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2>(Action<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2) => action.Invoke(t0, t1, t2));
        DeferEnd();
    }

    public void Each<T0, T1, T2>(Action<Entity, T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2) =>
                action.Invoke(new Entity(entity, this), t0, t1, t2)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3>(Action<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) => action.Invoke(t0, t1, t2, t3));
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3>(Action<Entity, T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) =>
                action.Invoke(new Entity(entity, this), t0, t1, t2, t3)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) => action.Invoke(t0, t1, t2, t3, t4));
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4>(Action<Entity, T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) =>
                action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) => action.Invoke(t0, t1, t2, t3, t4, t5)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5>(Action<Entity, T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) =>
                action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6) =>
                action.Invoke(t0, t1, t2, t3, t4, t5, t6)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(Action<Entity, T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7) =>
                action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8) =>
                action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12,
                ref T13 t13
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12,
                ref T13 t13
            ) => action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12,
                ref T13 t13,
                ref T14 t14
            ) => action.Invoke(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (
                Flecs.NET.Core.Entity entity,
                ref T0 t0,
                ref T1 t1,
                ref T2 t2,
                ref T3 t3,
                ref T4 t4,
                ref T5 t5,
                ref T6 t6,
                ref T7 t7,
                ref T8 t8,
                ref T9 t9,
                ref T10 t10,
                ref T11 t11,
                ref T12 t12,
                ref T13 t13,
                ref T14 t14
            ) =>
                action.Invoke(new Entity(entity, this), t0, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14)
        );
        DeferEnd();
    }

    #endregion

    #region OrderedEach

    public void OrderedEach(Action<Entity> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) => action.Invoke(new Entity(entity, this)));
        DeferEnd();
    }

    public void OrderedEach<T0>(Action<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(entity.Get<T0>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0>(Action<Entity, T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(new Entity(entity, this), entity.Get<T0>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1>(Action<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(entity.Get<T0>(), entity.Get<T1>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1>(Action<Entity, T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(new Entity(entity, this), entity.Get<T0>(), entity.Get<T1>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2>(Action<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(entity.Get<T0>(), entity.Get<T1>(), entity.Get<T2>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2>(Action<Entity, T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(new Entity(entity, this), entity.Get<T0>(), entity.Get<T1>(), entity.Get<T2>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3>(Action<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>())
                    action.Invoke(entity.Get<T0>(), entity.Get<T1>(), entity.Get<T2>(), entity.Get<T3>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3>(Action<Entity, T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>())
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>() && entity.Has<T4>())
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(Action<Entity, T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>() && entity.Has<T4>())
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(Action<Entity, T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(Action<Entity, T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                    && entity.Has<T13>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>(),
                        entity.Get<T13>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                    && entity.Has<T13>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>(),
                        entity.Get<T13>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                    && entity.Has<T13>()
                    && entity.Has<T14>()
                )
                    action.Invoke(
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>(),
                        entity.Get<T13>(),
                        entity.Get<T14>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Action<Entity, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (
                    entity.Has<T0>()
                    && entity.Has<T1>()
                    && entity.Has<T2>()
                    && entity.Has<T3>()
                    && entity.Has<T4>()
                    && entity.Has<T5>()
                    && entity.Has<T6>()
                    && entity.Has<T7>()
                    && entity.Has<T8>()
                    && entity.Has<T9>()
                    && entity.Has<T10>()
                    && entity.Has<T11>()
                    && entity.Has<T12>()
                    && entity.Has<T13>()
                    && entity.Has<T14>()
                )
                    action.Invoke(
                        new Entity(entity, this),
                        entity.Get<T0>(),
                        entity.Get<T1>(),
                        entity.Get<T2>(),
                        entity.Get<T3>(),
                        entity.Get<T4>(),
                        entity.Get<T5>(),
                        entity.Get<T6>(),
                        entity.Get<T7>(),
                        entity.Get<T8>(),
                        entity.Get<T9>(),
                        entity.Get<T10>(),
                        entity.Get<T11>(),
                        entity.Get<T12>(),
                        entity.Get<T13>(),
                        entity.Get<T14>()
                    );
            }
        );
        DeferEnd();
    }

    #endregion
}
