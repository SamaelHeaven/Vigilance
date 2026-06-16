using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPreUpdatable>())
            component.PreUpdate(entity);
    }

    public override void Update()
    {
        foreach (var (entity, component) in AssignableEntries<IUpdatable>())
            component.Update(entity);
    }

    public override void PostUpdate()
    {
        foreach (var (entity, component) in Scene.AssignableEntries<IPostUpdatable>())
            component.PostUpdate(entity);
    }

    public override void PreFixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPreFixedUpdatable>())
            component.PreFixedUpdate(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IFixedUpdatable>())
            component.FixedUpdate(entity);
    }

    public override void PostFixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPostFixedUpdatable>())
            component.PostFixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var (entity, component) in AssignableEntries<IPreRenderable>())
            component.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var (entity, component) in AssignableEntries<IRenderable>())
            component.Render(entity, commands);
    }

    public override void PostRender()
    {
        foreach (var (entity, component) in AssignableEntries<IPostRenderable>())
            component.PostRender(entity);
    }
}
