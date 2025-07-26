namespace Vigilance.Core;

public interface IComponent
{
    void Update(Entity entity) { }

    void FixedUpdate(Entity entity) { }

    void RenderBegin(Entity entity) { }

    void RenderEnd(Entity entity) { }

    void Render(Entity entity) { }
}
