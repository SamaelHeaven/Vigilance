using Vigilance.Drawing;

namespace Vigilance.Core;

public delegate IEnumerable<IGameSystem> GameSystemsFunc();

public interface IGameSystem : IComparable<IGameSystem>
{
    public int Order => 0;

    int IComparable<IGameSystem>.CompareTo(IGameSystem? other)
    {
        return other is null ? 1 : Order.CompareTo(other.Order);
    }

    void Configure(Scene scene);
}

public abstract partial class GameSystem : IGameSystem
{
    protected GameSystem(
        bool isDisabled = false,
        int order = 0,
        Inclusion queryWithDisabled = Inclusion.Exclude,
        bool queryDeferred = true
    )
    {
        IsDisabled = isDisabled;
        Order = order;
        QueryWithDisabled = queryWithDisabled;
        QueryDeferred = queryDeferred;
    }

    public Scene Scene { get; private set; } = null!;
    public bool IsDisabled { get; set; }
    public Inclusion QueryWithDisabled { get; set; }
    public bool QueryDeferred { get; set; }

    public Scene.EntityEnumerable Entities => GetEntities();
    public int Order { get; set; }

    public void Configure(Scene scene)
    {
        Scene = scene;
        var baseType = typeof(GameSystem);
        var initialize = Initialize;
        var start = Start;
        var stop = Stop;
        var preUpdate = PreUpdate;
        var update = Update;
        var postUpdate = PostUpdate;
        var preFixedUpdate = PreFixedUpdate;
        var fixedUpdate = FixedUpdate;
        var postFixedUpdate = PostFixedUpdate;
        var preRender = PreRender;
        var render = Render;
        var postRender = PostRender;
        if (initialize.Method.DeclaringType != baseType)
            scene.OnInitialize(initialize);
        if (start.Method.DeclaringType != baseType)
            scene.OnStart(start);
        if (stop.Method.DeclaringType != baseType)
            scene.OnStop(stop);
        if (preUpdate.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalPreUpdate);
        if (update.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalUpdate);
        if (postUpdate.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalPostUpdate);
        if (preFixedUpdate.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalPreFixedUpdate);
        if (fixedUpdate.Method.DeclaringType != baseType)
            scene.OnFixedUpdate(InternalFixedUpdate);
        if (postFixedUpdate.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalPostFixedUpdate);
        if (preRender.Method.DeclaringType != baseType)
            scene.OnPreRender(InternalPreRender);
        if (render.Method.DeclaringType != baseType)
            scene.OnRender(InternalRender);
        if (postRender.Method.DeclaringType != baseType)
            scene.OnPostRender(InternalPostRender);
        Configure();
    }

    public virtual void Configure() { }

    public virtual void Initialize() { }

    public virtual void Start() { }

    public virtual void Stop() { }

    public virtual void PreUpdate() { }

    public virtual void Update() { }

    public virtual void PostUpdate() { }

    public virtual void PreFixedUpdate() { }

    public virtual void FixedUpdate() { }

    public virtual void PostFixedUpdate() { }

    public virtual void PreRender() { }

    public virtual void Render(RenderCommands commands) { }

    public virtual void PostRender() { }

    private void InternalPreUpdate()
    {
        if (!IsDisabled)
            PreUpdate();
    }

    private void InternalUpdate()
    {
        if (!IsDisabled)
            Update();
    }

    private void InternalPostUpdate()
    {
        if (!IsDisabled)
            PostUpdate();
    }

    private void InternalPreFixedUpdate()
    {
        if (!IsDisabled)
            PreFixedUpdate();
    }

    private void InternalFixedUpdate()
    {
        if (!IsDisabled)
            FixedUpdate();
    }

    private void InternalPostFixedUpdate()
    {
        if (!IsDisabled)
            PostFixedUpdate();
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
