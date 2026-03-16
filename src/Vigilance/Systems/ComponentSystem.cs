using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        foreach (var table in ComponentTables<IPreUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreUpdatable?)component)?.PreUpdate(entity);
    }

    public override void Update()
    {
        foreach (var table in ComponentTables<IUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IUpdatable?)component)?.Update(entity);
    }

    public override void PostUpdate()
    {
        foreach (var table in ComponentTables<IPostUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostUpdatable?)component)?.PostUpdate(entity);
    }

    public override void PreFixedUpdate()
    {
        foreach (var table in ComponentTables<IPreFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreFixedUpdatable?)component)?.PreFixedUpdate(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var table in ComponentTables<IFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IFixedUpdatable?)component)?.FixedUpdate(entity);
    }

    public override void PostFixedUpdate()
    {
        foreach (var table in ComponentTables<IPostFixedUpdatable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostFixedUpdatable?)component)?.PostFixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var table in ComponentTables<IPreRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IPreRenderable?)component)?.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var table in ComponentTables<IRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IRenderable?)component)?.Render(entity, commands);
    }

    public override void PostRender()
    {
        foreach (var table in ComponentTables<IPostRenderable>())
        foreach (var (entity, component) in Entries(table))
            ((IPostRenderable?)component)?.PostRender(entity);
    }

    private ValueEnumerable<Where<StructEnumerator<Scene.TableEnumerator, Table>, Table>, Table> ComponentTables<T>()
    {
        return Scene.Tables.AsValueEnumerable().Where(table => typeof(T).IsAssignableFrom(table.Type));
    }
}
