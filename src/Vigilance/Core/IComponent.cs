namespace Vigilance.Core;

public interface IComponent
{
    void Update(Entity entity) { }

    void FixedUpdate(Entity entity) { }

    void PreRender(Entity entity) { }

    void Render(Entity entity) { }

    void PostRender(Entity entity) { }
}
