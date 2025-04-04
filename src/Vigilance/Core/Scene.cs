﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flecs.NET.Core;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed unsafe class Scene
{
    private readonly List<Action> _fixedUpdateActions = [];
    private readonly List<Action> _initializeActions = [];
    private readonly List<Action<Entity>> _renderActions = [];
    private readonly List<Action> _updateActions = [];
    private Query<int> _orderedQuery;
    private float _time;
    private World _world = World.Create();

    public Scene()
    {
        delegate* unmanaged[Cdecl]<ulong, void*, ulong, void*, int> orderByCallback = &CompareEntities;
        var queryBuilder = _world.QueryBuilder<int>();
        queryBuilder.Desc.order_by = Type<int>.Id(_world);
        queryBuilder.Desc.order_by_callback = (nint)orderByCallback;
        _orderedQuery = queryBuilder.Build();
    }

    public bool Initialized { get; private set; }

    public ref Camera Camera => ref Get<Camera>();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int CompareEntities(ulong e1, void* zIndex1, ulong e2, void* zIndex2)
    {
        return (*(int*)zIndex1).CompareTo(*(int*)zIndex2);
    }

    public Entity Entity(string name = "")
    {
        EnsureInitialized();
        var result = new Entity(_world.Entity(name));
        result.Set(new Transform());
        result.Set(0);
        return result;
    }

    public Entity Lookup(string name)
    {
        EnsureInitialized();
        return new Entity(_world.Lookup(name));
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

    public void OnRender(Action<Entity> action)
    {
        EnsureNotInitialized();
        _renderActions.Add(action);
    }

    public ref T Get<T>()
    {
        EnsureInitialized();
        return ref _world.GetMut<T>();
    }

    public void Set<T>(ref T data)
    {
        EnsureInitialized();
        var hadT = _world.Has<T>();
        _world.Set(ref data);
        var entity = _world.Singleton<T>();
        if (hadT)
        {
            _world.Event<SetEvent>().Id<T>().Entity(entity).Emit();
            return;
        }

        if (typeof(T) != typeof(Transform))
            entity.Set(new Transform());
        if (typeof(T) != typeof(int))
            entity.Set(0);
        _world.Event<AddEvent>().Id<T>().Entity(entity).Emit();
    }

    public void Set<T>(T data)
    {
        Set(ref data);
    }

    public int Count<T>()
    {
        EnsureInitialized();
        return _world.Count<T>();
    }

    public void Remove<T>()
    {
        EnsureInitialized();
        _world.Remove<T>();
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
        for (_time += Time.Delta; _time >= Time.FixedDelta; _time -= Time.FixedDelta)
            FixedUpdate();
        Render();
    }

    private void FixedUpdate()
    {
        foreach (var action in _fixedUpdateActions)
            action.Invoke();
    }

    private void Render()
    {
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                var e = new Entity(entity);
                foreach (var action in _renderActions)
                    action.Invoke(e);
            }
        );
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

    public void OnAdd<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<AddEvent>()
            .Each(
                (Iter it, int i, ref T _) =>
                {
                    action.Invoke(new Entity(it.Entity(i)));
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
                    action.Invoke(new Entity(it.Entity(i)), t);
                }
            );
    }

    #endregion

    #region OnSet

    public void OnSet<T>(Action<Entity> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<SetEvent>()
            .Each(
                (Iter it, int i, ref T _) =>
                {
                    action.Invoke(new Entity(it.Entity(i)));
                }
            );
    }

    public void OnSet<T>(Action<T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<SetEvent>()
            .Each(
                (Iter _, int _, ref T t) =>
                {
                    action.Invoke(t);
                }
            );
    }

    public void OnSet<T>(Action<Entity, T> action)
    {
        EnsureNotInitialized();
        _world
            .Observer<T>()
            .Event<SetEvent>()
            .Each(
                (Iter it, int i, ref T t) =>
                {
                    action.Invoke(new Entity(it.Entity(i)), t);
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
                    action.Invoke(new Entity(it.Entity(i)));
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
                    action.Invoke(new Entity(it.Entity(i)), t);
                }
            );
    }

    #endregion

    #region Each

    public void Each(EachEntityAction action)
    {
        EnsureInitialized();
        _world.Each((Flecs.NET.Core.Entity entity, ref int _) => action.Invoke(new Entity(entity)));
    }

    public void Each<T0>(EachAction<T0> action)
    {
        EnsureInitialized();
        _world.Each((ref T0 t0) => action.Invoke(ref t0));
    }

    public void Each<T0>(EachEntityAction<T0> action)
    {
        EnsureInitialized();
        _world.Each((Flecs.NET.Core.Entity entity, ref T0 t0) => action.Invoke(new Entity(entity), ref t0));
    }

    public void Each<T0, T1>(EachAction<T0, T1> action)
    {
        EnsureInitialized();
        _world.Each((ref T0 t0, ref T1 t1) => action.Invoke(ref t0, ref t1));
    }

    public void Each<T0, T1>(EachEntityAction<T0, T1> action)
    {
        EnsureInitialized();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1) => action.Invoke(new Entity(entity), ref t0, ref t1)
        );
    }

    public void Each<T0, T1, T2>(EachAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2) => action.Invoke(ref t0, ref t1, ref t2));
    }

    public void Each<T0, T1, T2>(EachEntityAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2) =>
                action.Invoke(new Entity(entity), ref t0, ref t1, ref t2)
        );
    }

    public void Each<T0, T1, T2, T3>(EachAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        _world.Each((ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) => action.Invoke(ref t0, ref t1, ref t2, ref t3));
    }

    public void Each<T0, T1, T2, T3>(EachEntityAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3) =>
                action.Invoke(new Entity(entity), ref t0, ref t1, ref t2, ref t3)
        );
    }

    public void Each<T0, T1, T2, T3, T4>(EachAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4)
        );
    }

    public void Each<T0, T1, T2, T3, T4>(EachEntityAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4) =>
                action.Invoke(new Entity(entity), ref t0, ref t1, ref t2, ref t3, ref t4)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5>(EachAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5>(EachEntityAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        _world.Each(
            (Flecs.NET.Core.Entity entity, ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5) =>
                action.Invoke(new Entity(entity), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(EachAction<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6>(EachEntityAction<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
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
            ) => action.Invoke(new Entity(entity), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(EachAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7>(EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
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
            ) => action.Invoke(new Entity(entity), ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        _world.Each(
            (ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8) =>
                action.Invoke(ref t0, ref t1, ref t2, ref t3, ref t4, ref t5, ref t6, ref t7, ref t8)
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
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
                    new Entity(entity),
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
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action
    )
    {
        EnsureInitialized();
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
                ref T14 t14,
                ref T15 t15
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
                    ref t14,
                    ref t15
                )
        );
    }

    public void Each<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action
    )
    {
        EnsureInitialized();
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
                ref T14 t14,
                ref T15 t15
            ) =>
                action.Invoke(
                    new Entity(entity),
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
                    ref t14,
                    ref t15
                )
        );
    }

    #endregion

    #region OrderedEach

    public void OrderedEach(EachEntityAction action)
    {
        EnsureInitialized();
        _orderedQuery.Each((Flecs.NET.Core.Entity entity, ref int _) => action.Invoke(new Entity(entity)));
    }

    public void OrderedEach<T0>(EachAction<T0> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(ref entity.GetMut<T0>());
            }
        );
    }

    public void OrderedEach<T0>(EachEntityAction<T0> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>())
                    action.Invoke(new Entity(entity), ref entity.GetMut<T0>());
            }
        );
    }

    public void OrderedEach<T0, T1>(EachAction<T0, T1> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(ref entity.GetMut<T0>(), ref entity.GetMut<T1>());
            }
        );
    }

    public void OrderedEach<T0, T1>(EachEntityAction<T0, T1> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>())
                    action.Invoke(new Entity(entity), ref entity.GetMut<T0>(), ref entity.GetMut<T1>());
            }
        );
    }

    public void OrderedEach<T0, T1, T2>(EachAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(ref entity.GetMut<T0>(), ref entity.GetMut<T1>(), ref entity.GetMut<T2>());
            }
        );
    }

    public void OrderedEach<T0, T1, T2>(EachEntityAction<T0, T1, T2> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>())
                    action.Invoke(
                        new Entity(entity),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>()
                    );
            }
        );
    }

    public void OrderedEach<T0, T1, T2, T3>(EachAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3>(EachEntityAction<T0, T1, T2, T3> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>())
                    action.Invoke(
                        new Entity(entity),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>()
                    );
            }
        );
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(EachAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4>(EachEntityAction<T0, T1, T2, T3, T4> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
            {
                if (entity.Has<T0>() && entity.Has<T1>() && entity.Has<T2>() && entity.Has<T3>() && entity.Has<T4>())
                    action.Invoke(
                        new Entity(entity),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>()
                    );
            }
        );
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(EachAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5>(EachEntityAction<T0, T1, T2, T3, T4, T5> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
                        ref entity.GetMut<T0>(),
                        ref entity.GetMut<T1>(),
                        ref entity.GetMut<T2>(),
                        ref entity.GetMut<T3>(),
                        ref entity.GetMut<T4>(),
                        ref entity.GetMut<T5>()
                    );
            }
        );
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(EachAction<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6>(EachEntityAction<T0, T1, T2, T3, T4, T5, T6> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(EachAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7>(EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EachAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
    }

    public void OrderedEach<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        EachEntityAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action
    )
    {
        EnsureInitialized();
        _orderedQuery.Each(
            (Flecs.NET.Core.Entity entity, ref int _) =>
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
                        new Entity(entity),
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
    }

    #endregion
}
