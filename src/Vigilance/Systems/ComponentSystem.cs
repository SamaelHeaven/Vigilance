namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem
{
    public override void PreUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPreUpdatable>())
            try
            {
                component.PreUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void Update()
    {
        foreach (var (entity, component) in AssignableEntries<IUpdatable>())
            try
            {
                component.Update(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PostUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPostUpdatable>())
            try
            {
                component.PostUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PreFixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPreFixedUpdatable>())
            try
            {
                component.PreFixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void FixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IFixedUpdatable>())
            try
            {
                component.FixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PostFixedUpdate()
    {
        foreach (var (entity, component) in AssignableEntries<IPostFixedUpdatable>())
            try
            {
                component.PostFixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PreRender()
    {
        foreach (var (entity, component) in AssignableEntries<IPreRenderable>())
            try
            {
                component.PreRender(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void Render(RenderCommands commands)
    {
        foreach (var (entity, component) in AssignableEntries<IRenderable>())
            try
            {
                component.Render(entity, commands);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PostRender()
    {
        foreach (var (entity, component) in AssignableEntries<IPostRenderable>())
            try
            {
                component.PostRender(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }
}
