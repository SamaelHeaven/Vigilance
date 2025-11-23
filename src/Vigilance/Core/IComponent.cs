using Vigilance.Drawing;

namespace Vigilance.Core;

public interface IComponent
{
    void PreUpdate(Entity entity) { }

    void Update(Entity entity) { }

    void PostUpdate(Entity entity) { }

    void PreFixedUpdate(Entity entity) { }

    void FixedUpdate(Entity entity) { }

    void PostFixedUpdate(Entity entity) { }

    void PreRender(Entity entity) { }

    void Render(Entity entity, RenderCommands commands) { }

    void PostRender(Entity entity) { }
}
