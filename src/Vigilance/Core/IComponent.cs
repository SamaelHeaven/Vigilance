namespace Vigilance.Core;

public interface IPreUpdatable
{
    void PreUpdate(Entity entity);
}

public interface IUpdatable
{
    void Update(Entity entity);
}

public interface IPostUpdatable
{
    void PostUpdate(Entity entity);
}

public interface IPreFixedUpdatable
{
    void PreFixedUpdate(Entity entity);
}

public interface IFixedUpdatable
{
    void FixedUpdate(Entity entity);
}

public interface IPostFixedUpdatable
{
    void PostFixedUpdate(Entity entity);
}

public interface IPreRenderable
{
    void PreRender(Entity entity);
}

public interface IRenderable
{
    void Render(Entity entity, RenderCommands commands)
    {
        commands.Add(entity, this, (entity, component) => component.Render(entity));
    }

    void Render(Entity entity);
}

public interface IPostRenderable
{
    void PostRender(Entity entity);
}

public interface IComponent
    : IPreUpdatable,
        IUpdatable,
        IPostUpdatable,
        IPreFixedUpdatable,
        IFixedUpdatable,
        IPostFixedUpdatable,
        IPreRenderable,
        IRenderable,
        IPostRenderable
{
    void IFixedUpdatable.FixedUpdate(Entity entity) { }

    void IPostFixedUpdatable.PostFixedUpdate(Entity entity) { }

    void IPostRenderable.PostRender(Entity entity) { }

    void IPostUpdatable.PostUpdate(Entity entity) { }

    void IPreFixedUpdatable.PreFixedUpdate(Entity entity) { }

    void IPreRenderable.PreRender(Entity entity) { }

    void IPreUpdatable.PreUpdate(Entity entity) { }

    void IRenderable.Render(Entity entity) { }

    void IUpdatable.Update(Entity entity) { }
}
