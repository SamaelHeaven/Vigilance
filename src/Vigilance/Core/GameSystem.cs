using Vigilance.Drawing;

namespace Vigilance.Core;

public delegate IEnumerable<IGameSystem> GameSystemsFunc();

public interface IGameSystem : IComparable<IGameSystem>
{
    void Configure(Scene scene);
}

public abstract class GameSystem : IGameSystem, IComparable<GameSystem>
{
    public Scene Scene { get; private set; } = null!;
    public bool Disabled { get; set; }
    public int Order { get; set; }

    public int CompareTo(GameSystem? other)
    {
        return other is null ? 1 : Order.CompareTo(other.Order);
    }

    public int CompareTo(IGameSystem? other)
    {
        return other is GameSystem system ? CompareTo(system) : 1;
    }

    public void Configure(Scene scene)
    {
        Scene = scene;
        var initialize = Initialize;
        var start = Start;
        var stop = Stop;
        var update = Update;
        var fixedUpdate = FixedUpdate;
        var beginRender = PreRender;
        var render = Render;
        var endRender = PostRender;
        var baseType = typeof(GameSystem);
        if (initialize.Method.DeclaringType != baseType)
            scene.OnInitialize(initialize);
        if (start.Method.DeclaringType != baseType)
            scene.OnStart(start);
        if (stop.Method.DeclaringType != baseType)
            scene.OnStop(stop);
        if (update.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalUpdate);
        if (fixedUpdate.Method.DeclaringType != baseType)
            scene.OnFixedUpdate(InternalFixedUpdate);
        if (beginRender.Method.DeclaringType != baseType)
            scene.OnPreRender(InternalPreRender);
        if (render.Method.DeclaringType != baseType)
            scene.OnRender(InternalRender);
        if (endRender.Method.DeclaringType != baseType)
            scene.OnPostRender(InternalPostRender);
        Configure();
    }

    public virtual void Configure() { }

    public virtual void Initialize() { }

    public virtual void Start() { }

    public virtual void Stop() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void PreRender() { }

    public virtual void Render(RenderCommands commands) { }

    public virtual void PostRender() { }

    private void InternalUpdate()
    {
        if (!Disabled)
            Update();
    }

    private void InternalFixedUpdate()
    {
        if (!Disabled)
            FixedUpdate();
    }

    private void InternalPreRender()
    {
        if (!Disabled)
            PreRender();
    }

    private void InternalRender(RenderCommands commands)
    {
        if (!Disabled)
            Render(commands);
    }

    private void InternalPostRender()
    {
        if (!Disabled)
            PostRender();
    }
}

public static class GameSystemConfigExtensions
{
    public static ConfigBuilder Systems(this ConfigBuilder builder, GameSystemsFunc systems)
    {
        return builder.Add(systems);
    }
}
