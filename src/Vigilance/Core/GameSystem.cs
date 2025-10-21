namespace Vigilance.Core;

public delegate IEnumerable<IGameSystem> GameSystemsFunc();

public interface IGameSystem
{
    void Configure(Scene scene);
}

public abstract class GameSystem : IGameSystem
{
    public Scene Scene { get; private set; } = null!;

    public void Configure(Scene scene)
    {
        Scene = scene;
        var initialize = Initialize;
        var start = Start;
        var stop = Stop;
        var update = Update;
        var fixedUpdate = FixedUpdate;
        var beginRender = BeginRender;
        var endRender = EndRender;
        var render = Render;
        var baseType = typeof(GameSystem);
        if (initialize.Method.DeclaringType != baseType)
            scene.OnInitialize(initialize);
        if (start.Method.DeclaringType != baseType)
            scene.OnStart(start);
        if (stop.Method.DeclaringType != baseType)
            scene.OnStop(stop);
        if (update.Method.DeclaringType != baseType)
            scene.OnUpdate(update);
        if (fixedUpdate.Method.DeclaringType != baseType)
            scene.OnFixedUpdate(fixedUpdate);
        if (beginRender.Method.DeclaringType != baseType)
            scene.OnBeginRender(beginRender);
        if (endRender.Method.DeclaringType != baseType)
            scene.OnEndRender(endRender);
        if (render.Method.DeclaringType != baseType)
            scene.OnRender(render);
        Configure();
    }

    public virtual void Configure() { }

    public virtual void Initialize() { }

    public virtual void Start() { }

    public virtual void Stop() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void BeginRender() { }

    public virtual void EndRender() { }

    public virtual void Render(RenderCommands commands) { }
}

public static class GameSystemConfigExtensions
{
    public static ConfigBuilder Systems(this ConfigBuilder builder, GameSystemsFunc systems)
    {
        return builder.Add(systems);
    }
}
