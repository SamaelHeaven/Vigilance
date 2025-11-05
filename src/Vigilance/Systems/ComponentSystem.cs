using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class ComponentSystem() : GameSystem(withDisabled: WithDisabled.Yes)
{
    public override void Update()
    {
        foreach (var (entity, components) in Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.Update(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var (entity, components) in Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.FixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var (entity, components) in Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var (entity, components) in Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            commands.Add(entity, component, static (entity, component) => component.Render(entity));
    }

    public override void PostRender()
    {
        foreach (var (entity, components) in Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.PostRender(entity);
    }
}
