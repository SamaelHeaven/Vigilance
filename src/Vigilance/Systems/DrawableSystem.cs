namespace Vigilance.Systems;

public sealed class DrawableSystem(Graphics graphics) : GameSystem(queryWithDisabled: true)
{
    public DrawableSystem()
        : this(Renderer.Graphics) { }

    public Graphics Graphics { get; set; } = graphics;

    public override void Render(RenderCommands commands)
    {
        commands.AddAssignableEntries<DrawableSystem, IDrawable>(
            this,
            (system, entity, drawable) => drawable.Draw(entity.RenderTransform, system.Graphics)
        );
    }
}
