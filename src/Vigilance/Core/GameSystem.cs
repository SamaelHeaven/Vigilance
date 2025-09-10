using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
        var type = GetType();
        if (IsOverridden(type, nameof(Initialize)))
            scene.OnInitialize(Initialize);
        if (IsOverridden(type, nameof(Start)))
            scene.OnStart(Start);
        if (IsOverridden(type, nameof(Stop)))
            scene.OnStop(Stop);
        if (IsOverridden(type, nameof(Update)))
            scene.OnUpdate(Update);
        if (IsOverridden(type, nameof(FixedUpdate)))
            scene.OnFixedUpdate(FixedUpdate);
        if (IsOverridden(type, nameof(RenderBegin)))
            scene.OnRenderBegin(RenderBegin);
        if (IsOverridden(type, nameof(RenderEnd)))
            scene.OnRenderEnd(RenderEnd);
        if (IsOverridden(type, nameof(Render)))
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

    [UnconditionalSuppressMessage("Trimming", "IL2070")]
    private static bool IsOverridden(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return method is null || method.DeclaringType != typeof(GameSystem);
    }
}
