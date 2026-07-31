namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        ForEach<IPreUpdatable>((entity, component) => component.PreUpdate(entity));
    }

    public override void Update()
    {
        ForEach<IUpdatable>((entity, component) => component.Update(entity));
    }

    public override void PostUpdate()
    {
        ForEach<IPostUpdatable>((entity, component) => component.PostUpdate(entity));
    }

    public override void PreFixedUpdate()
    {
        ForEach<IPreFixedUpdatable>((entity, component) => component.PreFixedUpdate(entity));
    }

    public override void FixedUpdate()
    {
        ForEach<IFixedUpdatable>((entity, component) => component.FixedUpdate(entity));
    }

    public override void PostFixedUpdate()
    {
        ForEach<IPostFixedUpdatable>((entity, component) => component.PostFixedUpdate(entity));
    }

    public override void PreRender()
    {
        ForEach<IPreRenderable>((entity, component) => component.PreRender(entity));
    }

    public override void Render(RenderCommands commands)
    {
        ForEach<ComponentSystem, IRenderable>(
            (system, entity, component) => component.Render(entity, new RenderCommands(system.Scene))
        );
    }

    public override void PostRender()
    {
        ForEach<IPostRenderable>((entity, component) => component.PostRender(entity));
    }
}
