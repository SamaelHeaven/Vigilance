using Vigilance.Core;

namespace Vigilance.Drawing;

public readonly partial struct RenderCommands
{
    private readonly List<RenderCommand> _commands = new();

    public RenderCommands() { }

    public void Add(in RenderCommand command)
    {
        _commands.Add(command);
    }

    public void Add(Action action, ulong? order = null)
    {
        Add(RenderCommand.From(action, order));
    }

    public void Add(Entity entity, Action<Entity> action)
    {
        Add(RenderCommand.From(entity, action));
    }

    public void Add<T>(Entity entity, T t, Action<Entity, T> action)
    {
        Add(RenderCommand.From(entity, t, action));
    }

    public void Add<T0, T1>(Entity entity, T0 t0, T1 t1, Action<Entity, T0, T1> action)
    {
        Add(RenderCommand.From(entity, t0, t1, action));
    }

    public void AddRange<T>(T enumerable)
        where T : IEnumerable<RenderCommand>
    {
        foreach (var command in enumerable)
            Add(command);
    }

    public void AddRange<TComponent>(Scene.EntryEnumerable<TComponent> entries, Action<Entity, TComponent> action)
    {
        foreach (var (entity, component) in entries)
            Add(entity, component, action);
    }

    public void AddRange<TContext, TComponent>(
        TContext context,
        Scene.EntryEnumerable<TComponent> entries,
        Action<Entity, TContext, TComponent> action
    )
    {
        foreach (var (entity, component) in entries)
            Add(entity, context, component, action);
    }

    public void Execute()
    {
        _commands.Sort();
        foreach (var command in _commands)
            command.Invoke();
        _commands.Clear();
    }
}

public readonly struct RenderCommand : IComparable<RenderCommand>
{
    private readonly ulong _order;
    private readonly Invoker _invoker;
    private readonly Entity _entity;
    private readonly object? _t0;
    private readonly object? _t1;
    private readonly object _action;

    private RenderCommand(
        Invoker invoker,
        object action,
        Entity entity,
        object? t0 = null,
        object? t1 = null,
        ulong? order = null
    )
    {
        _order = order ?? (entity.IsNull ? 0 : entity.Order);
        _invoker = invoker;
        _entity = entity;
        _t0 = t0;
        _t1 = t1;
        _action = action;
    }

    public static RenderCommand From(Action action, ulong? order = null)
    {
        return new RenderCommand(VoidInvoker, action, Entity.Null, order: order);
    }

    public static RenderCommand From(Entity entity, Action<Entity> action)
    {
        return new RenderCommand(EntityInvoker, action, entity);
    }

    public static RenderCommand From<T>(Entity entity, T t, Action<Entity, T> action)
    {
        return new RenderCommand(MonoInvoker<T>, action, entity, t);
    }

    public static RenderCommand From<T0, T1>(Entity entity, T0 t0, T1 t1, Action<Entity, T0, T1> action)
    {
        return new RenderCommand(BiInvoker<T0, T1>, action, entity, t0, t1);
    }

    public void Invoke()
    {
        _invoker.Invoke(this);
    }

    public int CompareTo(RenderCommand other)
    {
        return _order.CompareTo(other._order);
    }

    private static void VoidInvoker(in RenderCommand command)
    {
        ((Action)command._action).Invoke();
    }

    private static void EntityInvoker(in RenderCommand command)
    {
        ((Action<Entity>)command._action).Invoke(command._entity);
    }

    private static void MonoInvoker<T>(in RenderCommand command)
    {
        ((Action<Entity, T>)command._action).Invoke(command._entity, (T)command._t0!);
    }

    private static void BiInvoker<T0, T1>(in RenderCommand command)
    {
        ((Action<Entity, T0, T1>)command._action).Invoke(command._entity, (T0)command._t0!, (T1)command._t1!);
    }

    private delegate void Invoker(in RenderCommand command);
}
