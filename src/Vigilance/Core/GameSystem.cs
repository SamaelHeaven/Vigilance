namespace Vigilance.Core;

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
        scene.OnRenderBegin(RenderBegin);
        scene.OnRenderEnd(RenderEnd);
        scene.OnRender(Render);
        Configure();
    }

    public virtual void Configure() { }

    public virtual void Initialize() { }

    public virtual void Start() { }

    public virtual void Stop() { }

    public virtual void Update() { }

    public virtual void FixedUpdate() { }

    public virtual void RenderBegin() { }

    public virtual void RenderEnd() { }

    public virtual void Render(Entity entity) { }
}
