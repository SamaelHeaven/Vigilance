using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : ISystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public void Configure(Scene scene)
    {
        scene.OnRender(entity => Graphics.DrawEntity(entity));
    }
}
