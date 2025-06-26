namespace Vigilance.Core;

public interface IComponent
{
    void Update(Entity entity);

    void FixedUpdate(Entity entity);

    void Render(Entity entity);
}

public abstract class Component : IComponent
{
    public virtual void Update(Entity entity) { }

    public virtual void FixedUpdate(Entity entity) { }

    public virtual void Render(Entity entity) { }
}
