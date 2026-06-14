using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        foreach (var table in Scene.Tables<IPreUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreUpdatable?)component)?.PreUpdate(entity);
    }

    public override void Update()
    {
        foreach (var table in Scene.Tables<IUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IUpdatable?)component)?.Update(entity);
    }

    public override void PostUpdate()
    {
        foreach (var table in Scene.Tables<IPostUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostUpdatable?)component)?.PostUpdate(entity);
    }

    public override void PreFixedUpdate()
    {
        foreach (var table in Scene.Tables<IPreFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreFixedUpdatable?)component)?.PreFixedUpdate(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var table in Scene.Tables<IFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IFixedUpdatable?)component)?.FixedUpdate(entity);
    }

    public override void PostFixedUpdate()
    {
        foreach (var table in Scene.Tables<IPostFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostFixedUpdatable?)component)?.PostFixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var table in Scene.Tables<IPreRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreRenderable?)component)?.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var table in Scene.Tables<IRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IRenderable?)component)?.Render(entity, commands);
    }

    public override void PostRender()
    {
        foreach (var table in Scene.Tables<IPostRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostRenderable?)component)?.PostRender(entity);
    }
}
