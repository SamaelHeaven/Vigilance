using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using Vigilance.Events;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed unsafe class Scene
{
    private static Scene _current = null!;
    private readonly Dictionary<Type, List<object>> _events = new();
    private readonly List<Action> _fixedUpdateActions = [];
    private readonly List<Action> _initializeActions = [];
    private readonly delegate* unmanaged[Cdecl]<ulong, void*, ulong, void*, int> _orderByCallback = &CompareEntities;
    private readonly List<Action<Entity>> _renderActions = [];
    private readonly List<Action> _renderEndActions = [];
    private readonly List<Action> _renderStartActions = [];
    private readonly List<Action> _updateActions = [];
    private Camera _camera = new();
    private Query<ZIndex> _orderedQuery;
    private float _time;
    private World _world = World.Create();

    public Scene(IImmutableList<ISystem>? systems = null)
    {
        Systems = systems ?? ImmutableList<ISystem>.Empty;
        var orderedQueryBuilder = _world.QueryBuilder<ZIndex>();
        orderedQueryBuilder.Desc.order_by = Type<ZIndex>.Id(_world);
        orderedQueryBuilder.Desc.order_by_callback = (nint)_orderByCallback;
        _orderedQuery = orderedQueryBuilder.Build();
    }

    public IImmutableList<ISystem> Systems { get; }

    public ref Camera Camera => ref _camera;

    public bool Initialized { get; private set; }

    // ReSharper disable once UseCollectionExpression
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int CompareEntities(ulong id1, void* zIndex1, ulong id2, void* zIndex2)
    {
        var scene = _current;
        var e1 = new Entity(scene._world.Entity(id1), scene);
        var e2 = new Entity(scene._world.Entity(id2), scene);
        var result = e1.WorldZIndex.CompareTo(e2.WorldZIndex);
        return result == 0 ? id1.CompareTo(id2) : result;
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

    public void On<T>(Action action)
    {
        On((ref T _) => action.Invoke());
    }

    public void On<T>(RefAction<T> action)
    {
        EnsureNotInitialized();
        var type = typeof(T);
        if (!_events.ContainsKey(type))
            _events.Add(type, []);
        _events[type].Add(action);
    }

    public void OnInitialize(Action action)
    {
        EnsureNotInitialized();
        _initializeActions.Add(action);
    }

    public void OnUpdate(Action action)
    {
        EnsureNotInitialized();
        _updateActions.Add(action);
    }

    public void OnFixedUpdate(Action action)
    {
        EnsureNotInitialized();
        _fixedUpdateActions.Add(action);
    }

    public void OnRenderStart(Action action)
    {
        EnsureNotInitialized();
        _renderStartActions.Add(action);
    }

    public void OnRenderEnd(Action action)
    {
        EnsureNotInitialized();
        _renderEndActions.Add(action);
    }

    public void OnRender(Action<Entity> action)
    {
        EnsureNotInitialized();
        _renderActions.Add(action);
    }

    public void Emit<T>(T @event)
    {
        Emit(ref @event);
    }

    public void Emit<T>(ref T @event)
    {
        EnsureInitialized();
        var type = typeof(T);
        if (!_events.TryGetValue(type, out var actions))
            return;
        foreach (var action in actions)
            ((RefAction<T>)action).Invoke(ref @event);
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
        foreach (var system in Game.Systems)
            system.Configure(this);
        foreach (var system in Systems)
            system.Configure(this);
        Initialized = true;
        foreach (var action in _initializeActions)
            action.Invoke();
        Time.Restart();
    }

    internal void Update()
    {
        if (!Initialized)
            Initialize();
        foreach (var action in _updateActions)
            action.Invoke();
        for (_time += Time.DeltaSeconds; _time >= Time.FixedDeltaSeconds; _time -= Time.FixedDeltaSeconds)
            FixedUpdate();
        Render();
    }

    internal void DeferBegin()
    {
        _current = this;
        if (!_world.IsDeferred())
            _world.DeferBegin();
    }

    internal void DeferEnd()
    {
        if (_world.IsDeferred())
            _world.DeferEnd();
        _current = null!;
    }

    private void FixedUpdate()
    {
        foreach (var action in _fixedUpdateActions)
            action.Invoke();
    }

    private void Render()
    {
        foreach (var action in _renderStartActions)
            action.Invoke();
        OrderedEach(entity =>
        {
            foreach (var action in _renderActions)
                action.Invoke(entity);
        });
        foreach (var action in _renderEndActions)
            action.Invoke();
    }

    ~Scene()
    {
        Game.RunLater(() =>
        {
            _orderedQuery.Dispose();
            _world.Dispose();
        });
    }

    #region OnAdd

    public void OnAdd<T>(EntityAction action)
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

    public void OnAdd<T>(RefAction<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter _, int _, ref T t) =>
                {
                    action.Invoke(ref t);
                }
            );
    }

    public void OnAdd<T>(EntityAction<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    action.Invoke(new Entity(it.Entity(i), this), ref t);
                }
            );
    }

    #endregion

    #region OnSet

    public void OnSet<T>(EntityAction action, bool traverse = false)
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

    public void OnSet<T>(RefAction<T> action, bool traverse = false)
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
                        action.Invoke(ref t);
                        return;
                    }

                    var entity = new Entity(it.Entity(i), this);
                    entity.Traverse(action);
                }
            );
    }

    public void OnSet<T>(EntityAction<T> action, bool traverse = false)
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
                        action.Invoke(entity, ref t);
                        return;
                    }

                    entity.Traverse(action);
                }
            );
    }

    #endregion

    #region OnRemove

    public void OnRemove<T>(EntityAction action)
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

    public void OnRemove<T>(RefAction<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event(Ecs.OnRemove)
            .Each(
                (Iter _, int _, ref T t) =>
                {
                    action.Invoke(ref t);
                }
            );
    }

    public void OnRemove<T>(EntityAction<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event(Ecs.OnRemove)
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    action.Invoke(new Entity(it.Entity(i), this), ref t);
                }
            );
    }

    #endregion

    #region OnSetPosition

    public void OnSetPosition(Action<Entity> action, bool traverse = true)
    {
        OnSet<Position>(action.Invoke, traverse);
    }

    public void OnSetPosition(Action<Vector2> action, bool traverse = true)
    {
        OnSet(
            (ref Position position) =>
            {
                action.Invoke(position.Value);
            },
            traverse
        );
    }

    public void OnSetPosition(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ref Position position) =>
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

    public void OnSetScale(Action<Vector2> action, bool traverse = true)
    {
        OnSet(
            (ref Scale scale) =>
            {
                action.Invoke(scale.Value);
            },
            traverse
        );
    }

    public void OnSetScale(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ref Scale scale) =>
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

    public void OnSetRotation(Action<float> action, bool traverse = true)
    {
        OnSet(
            (ref Rotation rotation) =>
            {
                action.Invoke(rotation.Value);
            },
            traverse
        );
    }

    public void OnSetRotation(Action<Entity, float> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ref Rotation rotation) =>
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

    public void OnSetPivotPoint(Action<Vector2> action, bool traverse = true)
    {
        OnSet(
            (ref PivotPoint pivotPoint) =>
            {
                action.Invoke(pivotPoint.Value);
            },
            traverse
        );
    }

    public void OnSetPivotPoint(Action<Entity, Vector2> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ref PivotPoint pivotPoint) =>
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

    public void OnSetZIndex(Action<int> action, bool traverse = true)
    {
        OnSet(
            (ref ZIndex zIndex) =>
            {
                action.Invoke(zIndex.Value);
            },
            traverse
        );
    }

    public void OnSetZIndex(Action<Entity, int> action, bool traverse = true)
    {
        OnSet(
            (Entity entity, ref ZIndex zIndex) =>
            {
                action.Invoke(entity, zIndex.Value);
            },
            traverse
        );
    }

    #endregion

    #region Each

    public void Each(EntityAction action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) => action.Invoke(new Entity(entity, this)));
        DeferEnd();
    }

    public void Each<T0>(RefAction<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0) => action.Invoke(ref t0));
        DeferEnd();
    }

    public void Each<T0>(EntityAction<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((Flecs.NET.Core.Entity entity, ref T0 t0) => action.Invoke(new Entity(entity, this), ref t0));
        DeferEnd();
    }

    public void Each<T0, T1>(RefAction<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1) => action.Invoke(ref t0, ref t1));
        DeferEnd();
    }

    public void Each<T0, T1>(EntityAction<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1) =>
                action.Invoke(new Entity(entity, this), ref t0, ref t1)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2>(RefAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2) => action.Invoke(ref t0, ref t1, ref t2));
        DeferEnd();
    }

    public void Each<T0, T1, T2>(EntityAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2) =>
                action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3>(RefAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) => action.Invoke(ref t0, ref t1, ref t2, ref t3));
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3>(EntityAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) =>
                action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2, ref t3)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4>(RefAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4>(EntityAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) =>
                action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2, ref t3, ref t4)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5>(RefAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5>(EntityAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) =>
                action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(RefAction<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(EntityAction<T0, T1, T2, T3, T4, T5, T6> action)
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
            ) => action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(RefAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(EntityAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
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
            ) => action.Invoke(new Entity(entity, this), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        DeferBegin();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
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
            ) => action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8, ref t9)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
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
            ) => action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8, ref t9, ref t10)
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
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
            ) =>
                action.Invoke(
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
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
            ) =>
                action.Invoke(
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
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
            ) =>
                action.Invoke(
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12,
                    ref t13
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
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
            ) =>
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12,
                    ref t13
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
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
            ) =>
                action.Invoke(
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12,
                    ref t13,
                    ref t14
                )
        );
        DeferEnd();
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
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
                action.Invoke(
                    new Entity(entity, this),
                    ref t0,
                    ref t1,
                    ref t2,
                    ref t3,
                    ref t4,
                    ref t5,
                    ref t6,
                    ref t7,
                    ref t8,
                    ref t9,
                    ref t10,
                    ref t11,
                    ref t12,
                    ref t13,
                    ref t14
                )
        );
        DeferEnd();
    }

    #endregion

    #region OrderedEach

    public void OrderedEach(EntityAction action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) => action.Invoke(new Entity(entity, this)));
        DeferEnd();
    }

    public void OrderedEach<T0>(RefAction<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(ref entity.GetMut<T0>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0>(EntityAction<T0> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(new Entity(entity, this), ref entity.GetMut<T0>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1>(RefAction<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(ref entity.GetMut<T0>(), ref entity.GetMut<T1>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1>(EntityAction<T0, T1> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(new Entity(entity, this), ref entity.GetMut<T0>(), ref entity.GetMut<T1>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2>(RefAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(ref entity.GetMut<T0>(), ref entity.GetMut<T1>(), ref entity.GetMut<T2>());
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2>(EntityAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(
                        new Entity(entity, this),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3>(RefAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>())
                    action.Invoke(
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3>(EntityAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>())
                    action.Invoke(
                        new Entity(entity, this),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(RefAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>() && entity.Has<T4>())
                    action.Invoke(
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(EntityAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        DeferBegin();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref ZIndex _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>() && entity.Has<T4>())
                    action.Invoke(
                        new Entity(entity, this),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(RefAction<T0, T1, T2, T3, T4, T5> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(EntityAction<T0, T1, T2, T3, T4, T5> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(RefAction<T0, T1, T2, T3, T4, T5, T6> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(EntityAction<T0, T1, T2, T3, T4, T5, T6> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(RefAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(EntityAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>(),
                        ref entity.GetMut<T13>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>(),
                        ref entity.GetMut<T13>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        RefAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>(),
                        ref entity.GetMut<T13>(),
                        ref entity.GetMut<T14>()
                    );
            }
        );
        DeferEnd();
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
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
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>(),
                        ref entity.GetMut<T6>(),
                        ref entity.GetMut<T7>(),
                        ref entity.GetMut<T8>(),
                        ref entity.GetMut<T9>(),
                        ref entity.GetMut<T10>(),
                        ref entity.GetMut<T11>(),
                        ref entity.GetMut<T12>(),
                        ref entity.GetMut<T13>(),
                        ref entity.GetMut<T14>()
                    );
            }
        );
        DeferEnd();
    }

    #endregion
}
