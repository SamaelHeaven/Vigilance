namespace Vigilance.Core;

public interface IComponent
{
    void Update(Entity entity) { }

    void FixedUpdate(Entity entity) { }

    void BeginRender(Entity entity) { }

    void EndRender(Entity entity) { }

    void Render(Entity entity) { }
}
