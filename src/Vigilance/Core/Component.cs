namespace Vigilance.Core;

public abstract class Component
{
    public virtual void Update(Entity entity) { }

    public virtual void FixedUpdate(Entity entity) { }

    public virtual void RenderBegin(Entity entity) { }

    public virtual void Render(Entity entity) { }

    public virtual void RenderEnd(Entity entity) { }
}
