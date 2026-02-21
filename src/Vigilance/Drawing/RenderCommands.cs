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

    public void Add(Action action, ulong? order = null)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(action, order));
    }

    public void Add(in Entity entity, Action<Entity> action)
    {
        Scene.RenderCommands.Add(RenderCommand.Make(entity, action));
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
            foreach (var table in Scene.RenderTables)
                table?.Clear();
        }
    }
}

internal readonly unsafe struct RenderCommand : IComparable<RenderCommand>
{
    private readonly delegate* <ref readonly RenderCommand, Scene, void> _invoker;
    private readonly Delegate _action;
    private readonly RenderTable? _table;
    private readonly object? _system;
    private readonly ulong _entityId;
    private readonly ulong _order;
    private readonly int _index;

    private RenderCommand(
        delegate* <ref readonly RenderCommand, Scene, void> invoker,
        Delegate action,
        in Entity entity = default,
        object? system = null,
        int index = -1,
        RenderTable? table = null,
        ulong? order = null
    )
    {
        _order = order ?? (entity.IsNull ? 0 : entity.Order);
        _entityId = entity.Id;
        _invoker = invoker;
        _action = action;
        _system = system;
        _index = index;
        _table = table;
    }

    public int CompareTo(RenderCommand other)
    {
        return _order.CompareTo(other._order);
    }

    internal static RenderCommand Make(Action action, ulong? order)
    {
        return new RenderCommand(&VoidInvoker, action, order: order);
    }

    internal static RenderCommand Make(in Entity entity, Action<Entity> action)
    {
        return new RenderCommand(&EntityInvoker, action, entity);
    }

    internal static RenderCommand Make<TComponent>(
        Scene scene,
        in Entity entity,
        in TComponent component,
        Action<Entity, TComponent> action
    )
    {
        var table = scene.RenderTable<TComponent>();
        var index = table.Components.Count;
        table.Components.Add(component);
        return new RenderCommand(&MonoInvoker<TComponent>, action, entity, null, index, table);
    }

    internal static RenderCommand Make<TSystem, TComponent>(
        Scene scene,
        in Entity entity,
        TSystem system,
        in TComponent component,
        Action<TSystem, Entity, TComponent> action
    )
    {
        var table = scene.RenderTable<TComponent>();
        var index = table.Components.Count;
        table.Components.Add(component);
        return new RenderCommand(&BiInvoker<TSystem, TComponent>, action, entity, system, index, table);
    }

    internal void Invoke(Scene scene)
    {
        _invoker(in this, scene);
    }

    private static void VoidInvoker(ref readonly RenderCommand command, Scene scene)
    {
        ((Action)command._action).Invoke();
    }

    private static void EntityInvoker(ref readonly RenderCommand command, Scene scene)
    {
        ((Action<Entity>)command._action).Invoke(new Entity(command._entityId, scene));
    }

    private static void MonoInvoker<TComponent>(ref readonly RenderCommand command, Scene scene)
    {
        ((Action<Entity, TComponent>)command._action).Invoke(
            new Entity(command._entityId, scene),
            ((RenderTable<TComponent>)command._table!).Components[command._index]
        );
    }

    private static void BiInvoker<TSystem, TComponent>(ref readonly RenderCommand command, Scene scene)
    {
        ((Action<TSystem, Entity, TComponent>)command._action).Invoke(
            (TSystem)command._system!,
            new Entity(command._entityId, scene),
            ((RenderTable<TComponent>)command._table!).Components[command._index]
        );
    }
}

internal abstract class RenderTable
{
    internal static int CurrentIndex = -1;

    internal abstract void Clear();
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
internal sealed class RenderTable<T> : RenderTable
{
    internal ValueList<T> Components = [];

    internal static int Index { get; } = Interlocked.Increment(ref CurrentIndex);

    internal override void Clear()
    {
        Components.Clear();
    }
}
