namespace Vigilance.Core;

public interface IComponent
{
    public void Update(Entity entity) { }

    public void FixedUpdate(Entity entity) { }

    public void RenderBegin(Entity entity) { }

    public void Render(Entity entity) { }

    public void RenderEnd(Entity entity) { }
}
