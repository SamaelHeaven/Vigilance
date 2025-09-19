using Vigilance.Core;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void Update()
    {
        foreach (var (entity, components) in Scene.Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.Update(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var (entity, components) in Scene.Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.FixedUpdate(entity);
    }

    public override void RenderBegin()
    {
        foreach (var (entity, components) in Scene.Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.RenderBegin(entity);
    }

    public override void RenderEnd()
    {
        foreach (var (entity, components) in Scene.Entries<Components>())
        foreach (var component in components.OfType<IComponent>())
            component.RenderEnd(entity);
    }

    public override void Render(Entity entity)
    {
        foreach (var component in entity.Components.OfType<IComponent>())
            component.Render(entity);
    }
}
