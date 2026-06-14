using System.Diagnostics.CodeAnalysis;
using Vigilance.Collections;
using Vigilance.Core;

namespace Vigilance.Drawing;

public readonly ref partial struct RenderCommands
{
    public Scene Scene { get; }

    internal RenderCommands(Scene scene)
    {
        Scene = scene;
    }

    public void Add(Action action, int layer, int sequence)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(Scene, action, layer, sequence));
    }

    public void Add(Action action, ulong order)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(Scene, action, order));
    }

    public void Add(in Entity entity, Action<Entity> action)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(Scene, entity, action));
    }

    public void Add<TComponent>(in Entity entity, in TComponent component, Action<Entity, TComponent> action)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(Scene, entity, component, action));
    }

    public void Add<TSystem, TComponent>(
        TSystem system,
        in Entity entity,
        in TComponent component,
        Action<TSystem, Entity, TComponent> action
    )
    {
        Scene.RenderCommands.Add(RenderCommand.Make(Scene, entity, system, component, action));
    }

    public void AddRange<TComponent>(Scene.EntryEnumerable<TComponent> entries, Action<Entity, TComponent> action)
    {
        foreach (var (entity, component) in entries)
            Add(entity, component, action);
    }

    public void AddRange<TSystem, TComponent>(
        TSystem system,
        Scene.EntryEnumerable<TComponent> entries,
        Action<TSystem, Entity, TComponent> action
    )
    {
        foreach (var (entity, component) in entries)
            Add(system, entity, component, action);
    }

    internal void Execute()
    {
        var scene = Scene;
        ref var commands = ref scene.RenderCommands;
        commands.Sort();
        try
        {
            foreach (ref var command in commands.AsSpan())
                command.Invoke(scene);
        }
        finally
        {
            commands.Clear();
            scene.RenderDataList.Clear();
            foreach (var table in Scene.RenderComponentsList)
                table.Clear();
        }
    }
}

public readonly unsafe struct RenderCommand : IComparable<RenderCommand>
{
    private readonly ulong _order;
    private readonly int _dataIndex;

    private RenderCommand(
        Scene scene,
        delegate* <ref readonly RenderCommand, ref readonly RenderData, Scene, void> invoker,
        Delegate action,
        in Entity entity = default,
        object? system = null,
        int componentIndex = -1,
        object? components = null,
        ulong order = 0
    )
    {
        _order = entity.IsNull ? order : GetOrder(entity.WorldZIndex, entity.Index);
        _dataIndex = scene.RenderDataList.Count;
        scene.RenderDataList.Add(
            new RenderData(invoker, action, components, system, entity.IsNull ? -1 : entity.Version, componentIndex)
        );
    }

    public static ulong GetOrder(int layer, int sequence)
    {
        return ((ulong)(uint)(layer ^ int.MinValue) << 32) | (uint)(sequence ^ int.MinValue);
    }

    public static int GetLayer(ulong order)
    {
        return (int)((uint)(order >> 32) ^ int.MinValue);
    }

    public static int GetSequence(ulong order)
    {
        return (int)((uint)order ^ int.MinValue);
    }

    public int CompareTo(RenderCommand other)
    {
        return _order.CompareTo(other._order);
    }

    internal static RenderCommand Make(Scene scene, Action action, int layer, int sequence)
    {
        return new RenderCommand(scene, &VoidInvoker, action, order: GetOrder(layer, sequence));
    }

    internal static RenderCommand Make(Scene scene, Action action, ulong order)
    {
        return new RenderCommand(scene, &VoidInvoker, action, order: order);
    }

    internal static RenderCommand Make(Scene scene, in Entity entity, Action<Entity> action)
    {
        return new RenderCommand(scene, &EntityInvoker, action, entity);
    }

    internal static RenderCommand Make<TComponent>(
        Scene scene,
        in Entity entity,
        in TComponent component,
        Action<Entity, TComponent> action
    )
    {
        if (!typeof(TComponent).IsValueType)
            return new RenderCommand(scene, &MonoInvoker<TComponent>, action, entity, components: component);
        var components = scene.RenderComponents<TComponent>();
        var index = components.Components.Count;
        components.Components.Add(component);
        return new RenderCommand(scene, &MonoInvoker<TComponent>, action, entity, null, index, components);
    }

    internal static RenderCommand Make<TSystem, TComponent>(
        Scene scene,
        in Entity entity,
        TSystem system,
        in TComponent component,
        Action<TSystem, Entity, TComponent> action
    )
    {
        if (!typeof(TComponent).IsValueType)
            return new RenderCommand(
                scene,
                &BiInvoker<TSystem, TComponent>,
                action,
                entity,
                system,
                components: component
            );
        var components = scene.RenderComponents<TComponent>();
        var index = components.Components.Count;
        components.Components.Add(component);
        return new RenderCommand(scene, &BiInvoker<TSystem, TComponent>, action, entity, system, index, components);
    }

    internal void Invoke(Scene scene)
    {
        ref var data = ref scene.RenderDataList[_dataIndex];
        data.Invoker(in this, in data, scene);
    }

    private static void VoidInvoker(ref readonly RenderCommand command, ref readonly RenderData data, Scene scene)
    {
        ((Action)data.Action).Invoke();
    }

    private static void EntityInvoker(ref readonly RenderCommand command, ref readonly RenderData data, Scene scene)
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        ((Action<Entity>)data.Action).Invoke(entity);
    }

    private static void MonoInvoker<TComponent>(
        ref readonly RenderCommand command,
        ref readonly RenderData data,
        Scene scene
    )
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        ((Action<Entity, TComponent>)data.Action).Invoke(
            entity,
            data.ComponentIndex == -1
                ? (TComponent)data.Components!
                : ((RenderComponents<TComponent>)data.Components!).Components[data.ComponentIndex]
        );
    }

    private static void BiInvoker<TSystem, TComponent>(
        ref readonly RenderCommand command,
        ref readonly RenderData data,
        Scene scene
    )
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        ((Action<TSystem, Entity, TComponent>)data.Action).Invoke(
            (TSystem)data.System!,
            entity,
            data.ComponentIndex == -1
                ? (TComponent)data.Components!
                : ((RenderComponents<TComponent>)data.Components!).Components[data.ComponentIndex]
        );
    }
}

internal readonly unsafe struct RenderData
{
    internal readonly delegate* <ref readonly RenderCommand, ref readonly RenderData, Scene, void> Invoker;
    internal readonly Delegate Action;
    internal readonly object? Components;
    internal readonly object? System;
    internal readonly int EntityVersion;
    internal readonly int ComponentIndex;

    internal RenderData(
        delegate* <ref readonly RenderCommand, ref readonly RenderData, Scene, void> invoker,
        Delegate action,
        object? components,
        object? system,
        int entityVersion,
        int componentIndex
    )
    {
        Invoker = invoker;
        Action = action;
        Components = components;
        System = system;
        EntityVersion = entityVersion;
        ComponentIndex = componentIndex;
    }
}

internal abstract class RenderComponents
{
    internal static int CurrentIndex = -1;

    internal abstract void Clear();
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
internal sealed class RenderComponents<T> : RenderComponents
{
    internal ValueList<T> Components = [];

    internal static int Index { get; } = Interlocked.Increment(ref CurrentIndex);

    internal override void Clear()
    {
        Components.Clear();
    }
}
