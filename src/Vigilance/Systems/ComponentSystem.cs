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
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PreUpdate(entity);
    }

    public override void Update()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.Update(entity);
    }

    public override void PostUpdate()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PostUpdate(entity);
    }

    public override void PreFixedUpdate()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PreFixedUpdate(entity);
    }

    public override void FixedUpdate()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.FixedUpdate(entity);
    }

    public override void PostFixedUpdate()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PostFixedUpdate(entity);
    }

    public override void PreRender()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PreRender(entity);
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.Render(entity, commands);
    }

    public override void PostRender()
    {
        foreach (var table in ComponentTables())
        foreach (var (entity, component) in Entries(table))
            ((IComponent?)component)?.PostRender(entity);
    }

    private ValueEnumerable<Where<StructEnumerator<Scene.TableEnumerator, Table>, Table>, Table> ComponentTables()
    {
        return Scene.Tables.AsValueEnumerable().Where(table => typeof(IComponent).IsAssignableFrom(table.Type));
    }
}
