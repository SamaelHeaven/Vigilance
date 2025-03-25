using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public struct GraphicsSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnRender(static entity =>
        {
            var graphics = Renderer.Graphics;
            if (entity.Has<Rectangle>())
                graphics.DrawRectangle(entity.WorldTransform, entity.Get<Rectangle>());
        });
    }
}
