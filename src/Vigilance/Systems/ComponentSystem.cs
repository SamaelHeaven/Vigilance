using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PreUpdate(entity);
    }

    public override void Update()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.Update(entity);
    }

    public override void PostUpdate()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PostUpdate(entity);
    }

    public override void PreFixedUpdate()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PreFixedUpdate(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.FixedUpdate(entity);
    }

    public override void PostFixedUpdate()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PostFixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.Render(entity, commands);
    }

    public override void PostRender()
    {
        foreach (var entity in Entities)
        foreach (var component in entity.Components.OfType<IComponent>())
            component.PostRender(entity);
    }
}
