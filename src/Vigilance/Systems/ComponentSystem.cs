using Vigilance.Core;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void Update()
    {
        foreach (var (entity, components) in Scene.Entries<Components>().WithDisabled())
        foreach (var component in components.OfType<IComponent>())
            component.Update(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var (entity, components) in Scene.Entries<Components>().WithDisabled())
        foreach (var component in components.OfType<IComponent>())
            component.FixedUpdate(entity);
    }

    public override void BeginRender()
    {
        foreach (var (entity, components) in Scene.Entries<Components>().WithDisabled())
        foreach (var component in components.OfType<IComponent>())
            component.BeginRender(entity);
    }

    public override void EndRender()
    {
        foreach (var (entity, components) in Scene.Entries<Components>().WithDisabled())
        foreach (var component in components.OfType<IComponent>())
            component.EndRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var (entity, components) in Scene.Entries<Components>().WithDisabled())
        foreach (var component in components.OfType<IComponent>())
            commands.Add(entity, component.Render);
    }
}
