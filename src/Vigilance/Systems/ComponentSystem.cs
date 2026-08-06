namespace Vigilance.Systems;

public sealed class ComponentSystem : GameSystem<ComponentSystem>
{
    [GenericRegistry]
    public static void RegisterPreUpdatable<T>()
        where T : IPreUpdatable
    {
        ConfigureEach(
            (typeof(IPreUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnPreUpdate(system.PreUpdate<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterUpdatable<T>()
        where T : IUpdatable
    {
        ConfigureEach(
            (typeof(IUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnUpdate(system.Update<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterPostUpdatable<T>()
        where T : IPostUpdatable
    {
        ConfigureEach(
            (typeof(IPostUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnPostUpdate(system.PostUpdate<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterPreFixedUpdatable<T>()
        where T : IPreFixedUpdatable
    {
        ConfigureEach(
            (typeof(IPreFixedUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnPreFixedUpdate(system.PreFixedUpdate<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterFixedUpdatable<T>()
        where T : IFixedUpdatable
    {
        ConfigureEach(
            (typeof(IFixedUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnFixedUpdate(system.FixedUpdate<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterPostFixedUpdatable<T>()
        where T : IPostFixedUpdatable
    {
        ConfigureEach(
            (typeof(IPostFixedUpdatable), typeof(T)),
            system =>
            {
                system.Scene.OnPostFixedUpdate(system.PostFixedUpdate<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterPreRenderable<T>()
        where T : IPreRenderable
    {
        ConfigureEach(
            (typeof(IPreRenderable), typeof(T)),
            system =>
            {
                system.Scene.OnPreRender(system.PreRender<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterRenderable<T>()
        where T : IRenderable
    {
        ConfigureEach(
            (typeof(IRenderable), typeof(T)),
            system =>
            {
                system.Scene.OnRender(system.Render<T>);
            }
        );
    }

    [GenericRegistry]
    public static void RegisterPostRenderable<T>()
        where T : IPostRenderable
    {
        ConfigureEach(
            (typeof(IPostRenderable), typeof(T)),
            system =>
            {
                system.Scene.OnPostRender(system.PostRender<T>);
            }
        );
    }

    private void PreUpdate<T>()
        where T : IPreUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PreUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void Update<T>()
        where T : IUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.Update(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void PostUpdate<T>()
        where T : IPostUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PostUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void PreFixedUpdate<T>()
        where T : IPreFixedUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PreFixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void FixedUpdate<T>()
        where T : IFixedUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.FixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void PostFixedUpdate<T>()
        where T : IPostFixedUpdatable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PostFixedUpdate(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void PreRender<T>()
        where T : IPreRenderable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PreRender(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void Render<T>(RenderCommands commands)
        where T : IRenderable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.Render(entity, commands);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    private void PostRender<T>()
        where T : IPostRenderable
    {
        foreach (var (entity, componentRef) in RefEntries<T>())
            try
            {
                componentRef.AsWritable().Value.PostRender(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }
}
