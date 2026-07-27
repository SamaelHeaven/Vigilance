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
        bool queryWithDisabled = false,
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
    public bool QueryWithDisabled { get; set; }
    public bool QueryDeferred { get; protected set; }
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
        var worldFilter = WorldFilter;
        var worldContactBegin = WorldContactBegin;
        var worldContactEnd = WorldContactEnd;
        var worldContactHit = WorldContactHit;
        var worldSensorBegin = WorldSensorBegin;
        var worldSensorEnd = WorldSensorEnd;
        if (initialize.Method.DeclaringType != baseType)
            scene.OnInitialize(initialize);
        if (start.Method.DeclaringType != baseType)
            scene.OnStart(start);
        if (stop.Method.DeclaringType != baseType)
            scene.OnStop(stop);
        if (preUpdate.Method.DeclaringType != baseType)
            scene.OnPreUpdate(InternalPreUpdate);
        if (update.Method.DeclaringType != baseType)
            scene.OnUpdate(InternalUpdate);
        if (postUpdate.Method.DeclaringType != baseType)
            scene.OnPostUpdate(InternalPostUpdate);
        if (preFixedUpdate.Method.DeclaringType != baseType)
            scene.OnPreFixedUpdate(InternalPreFixedUpdate);
        if (fixedUpdate.Method.DeclaringType != baseType)
            scene.OnFixedUpdate(InternalFixedUpdate);
        if (postFixedUpdate.Method.DeclaringType != baseType)
            scene.OnPostFixedUpdate(InternalPostFixedUpdate);
        if (preRender.Method.DeclaringType != baseType)
            scene.OnPreRender(InternalPreRender);
        if (render.Method.DeclaringType != baseType)
            scene.OnRender(InternalRender);
        if (postRender.Method.DeclaringType != baseType)
            scene.OnPostRender(InternalPostRender);
        if (worldFilter.Method.DeclaringType != baseType)
            scene.World.OnFilter(worldFilter);
        if (worldContactBegin.Method.DeclaringType != baseType)
            scene.World.OnContactBegin(worldContactBegin);
        if (worldContactEnd.Method.DeclaringType != baseType)
            scene.World.OnContactEnd(worldContactEnd);
        if (worldContactHit.Method.DeclaringType != baseType)
            scene.World.OnContactHit(worldContactHit);
        if (worldSensorBegin.Method.DeclaringType != baseType)
            scene.World.OnSensorBegin(worldSensorBegin);
        if (worldSensorEnd.Method.DeclaringType != baseType)
            scene.World.OnSensorEnd(worldSensorEnd);
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

    public virtual bool WorldFilter(Shape shapeA, Shape shapeB)
    {
        return true;
    }

    public virtual void WorldContactBegin(Shape shapeA, Shape shapeB) { }

    public virtual void WorldContactEnd(Shape shapeA, Shape shapeB) { }

    public virtual void WorldContactHit(ContactHit contact) { }

    public virtual void WorldSensorBegin(Shape sensor, Shape visitor) { }

    public virtual void WorldSensorEnd(Shape sensor, Shape visitor) { }

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
