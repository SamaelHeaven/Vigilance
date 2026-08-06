using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Drawing;

public readonly partial struct RenderCommands
{
    public Scene Scene { get; }

    public RenderCommands(Scene scene)
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

    public void AddEntries<TComponent>(
        ValueEnumerable<Scene.EntryEnumerator<TComponent>, (Entity Entity, TComponent Component)> entries,
        Action<Entity, TComponent> action
    )
    {
        foreach (var (entity, component) in entries)
            Add(entity, component, action);
    }

    public void AddEntries<TComponent>(Scene.RefEntryEnumerable<TComponent> entries, Action<Entity, TComponent> action)
    {
        foreach (var (entity, component) in entries)
            Add(entity, component.Read, action);
    }

    public void AddEntries<TSystem, TComponent>(
        TSystem system,
        ValueEnumerable<Scene.EntryEnumerator<TComponent>, (Entity Entity, TComponent Component)> entries,
        Action<TSystem, Entity, TComponent> action
    )
    {
        foreach (var (entity, component) in entries)
            Add(system, entity, component, action);
    }

    public void AddEntries<TSystem, TComponent>(
        TSystem system,
        Scene.RefEntryEnumerable<TComponent> entries,
        Action<TSystem, Entity, TComponent> action
    )
    {
        foreach (var (entity, component) in entries)
            Add(system, entity, component.Read, action);
    }

    public void Execute()
    {
        ref var commands = ref Scene.RenderCommands;
        commands.Sort();
        foreach (ref var command in commands.AsSpan())
            try
            {
                command.Invoke(Scene);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        commands.Clear();
        Scene.RenderDataList.Clear();
        foreach (var table in Scene.RenderComponentsList)
            table.Clear();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly unsafe struct RenderCommand : IComparable<RenderCommand>
{
    private readonly ulong _order;
    private readonly int _dataIndex;

    private RenderCommand(
        Scene scene,
        delegate* <ref readonly RenderCommand, ref RenderData, Scene, void> invoker,
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
        return (ulong)(uint)(layer ^ int.MinValue) << 32 | (uint)(sequence ^ int.MinValue);
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
        data.Invoker(in this, ref data, scene);
    }

    private static void VoidInvoker(ref readonly RenderCommand command, ref RenderData data, Scene scene)
    {
        Unsafe.As<Delegate, Action>(ref data.Action).Invoke();
    }

    private static void EntityInvoker(ref readonly RenderCommand command, ref RenderData data, Scene scene)
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        Unsafe.As<Delegate, Action<Entity>>(ref data.Action).Invoke(entity);
    }

    private static void MonoInvoker<TComponent>(ref readonly RenderCommand command, ref RenderData data, Scene scene)
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        Unsafe
            .As<Delegate, Action<Entity, TComponent>>(ref data.Action)
            .Invoke(
                entity,
                data.ComponentIndex == -1
                    ? Unsafe.As<object, TComponent>(ref data.Components!)
                    : Unsafe.As<object, RenderComponents<TComponent>>(ref data.Components!).Components[
                        data.ComponentIndex
                    ]
            );
    }

    private static void BiInvoker<TSystem, TComponent>(
        ref readonly RenderCommand command,
        ref RenderData data,
        Scene scene
    )
    {
        var entity =
            data.EntityVersion == -1 ? Entity.Null : new Entity(GetSequence(command._order), data.EntityVersion, scene);
        Unsafe
            .As<Delegate, Action<TSystem, Entity, TComponent>>(ref data.Action)
            .Invoke(
                Unsafe.As<object, TSystem>(ref data.System!),
                entity,
                data.ComponentIndex == -1
                    ? Unsafe.As<object, TComponent>(ref data.Components!)
                    : Unsafe.As<object, RenderComponents<TComponent>>(ref data.Components!).Components[
                        data.ComponentIndex
                    ]
            );
    }
}

internal unsafe struct RenderData
{
    internal delegate* <ref readonly RenderCommand, ref RenderData, Scene, void> Invoker;
    internal Delegate Action;
    internal object? Components;
    internal object? System;
    internal int EntityVersion;
    internal int ComponentIndex;

    internal RenderData(
        delegate* <ref readonly RenderCommand, ref RenderData, Scene, void> invoker,
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
