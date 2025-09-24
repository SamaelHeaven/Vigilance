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
        scene.OnInitialize(Initialize);
        scene.OnStart(Start);
        scene.OnStop(Stop);
        scene.OnUpdate(Update);
        scene.OnFixedUpdate(FixedUpdate);
        scene.OnBeginRender(BeginRender);
        scene.OnEndRender(EndRender);
        scene.OnRender(Render);
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

    public virtual void Render(Entity entity) { }
}

public static class GameSystemConfigExtensions
{
    public static ConfigBuilder Systems(this ConfigBuilder builder, GameSystemsFunc systems)
    {
        return builder.Add(systems);
    }
}
