using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : GameSystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Render(Entity entity)
    {
        Graphics.DrawEntity(entity);
    }
}
