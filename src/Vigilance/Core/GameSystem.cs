using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Core;

public delegate IEnumerable<IGameSystem> GameSystemsFunc();

public interface IGameSystem : IComparable<IGameSystem>
{
    int IComparable<IGameSystem>.CompareTo(IGameSystem? other)
    {
        return other is null ? 1 : 0;
    }

    void Configure(Scene scene);
}

public abstract partial class GameSystem : IGameSystem, IComparable<GameSystem>
{
    protected GameSystem(
        bool isDisabled = false,
        int order = 0,
        WithDisabled withDisabled = WithDisabled.No,
        bool deferred = true
    )
    {
        IsDisabled = isDisabled;
        Order = order;
        WithDisabled = withDisabled;
        Deferred = deferred;
    }

    public Scene Scene { get; private set; } = null!;
    public bool IsDisabled { get; set; }
    public int Order { get; set; }
    public WithDisabled WithDisabled { get; set; }
    public bool Deferred { get; set; }

    public Scene.EntityEnumerable Entities => GetEntities();

    public int CompareTo(GameSystem? other)
    {
        return other is null ? 1 : Order.CompareTo(other.Order);
    }

    public int CompareTo(IGameSystem? other)
    {
        return other switch
        {
            GameSystem system => CompareTo(system),
            null => 1,
            _ => (int)((long)-other.CompareTo(this)).Clamp(int.MinValue, int.MaxValue),
        };
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

    public Entity Entity(string name = "")
    {
        return Scene.Entity(name);
    }

    private void InternalUpdate()
    {
        if (!IsDisabled)
            Update();
    }

    private void InternalFixedUpdate()
    {
        if (!IsDisabled)
            FixedUpdate();
    }

    private void InternalPreRender()
    {
        if (!IsDisabled)
            PreRender();
    }

    private void InternalRender(RenderCommands commands)
    {
        if (!IsDisabled)
            Render(commands);
    }

    private void InternalPostRender()
    {
        if (!IsDisabled)
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
