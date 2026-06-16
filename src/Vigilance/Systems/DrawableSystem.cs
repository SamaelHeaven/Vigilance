using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class DrawableSystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: true)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Render(RenderCommands commands)
    {
        commands.AddAssignableEntries<DrawableSystem, IDrawable>(
            this,
            (system, entity, drawable) => drawable.Render(entity.WorldTransform, system.Graphics)
        );
    }
}
