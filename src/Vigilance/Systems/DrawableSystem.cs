namespace Vigilance.Systems;

public sealed class DrawableSystem(Graphics graphics) : GameSystem<DrawableSystem>(queryWithDisabled: true)
{
    public DrawableSystem()
        : this(Renderer.Graphics) { }

    public Graphics Graphics { get; set; } = graphics;

    [GenericRegistry]
    public static void Register<T>()
        where T : IDrawable
    {
        ConfigureEach(
            typeof(T),
            system =>
            {
                system.Scene.OnRender(system.Render<T>);
            }
        );
    }

    private void Render<T>(RenderCommands commands)
        where T : IDrawable
    {
        commands.AddEntries<DrawableSystem, T>(
            this,
            (system, entity, drawable) => drawable.Draw(entity.RenderTransform, system.Graphics)
        );
    }
}
