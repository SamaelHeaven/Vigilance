using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem : ISystem
{
    public Graphics Graphics { get; set; } = Renderer.Graphics;

    public void Configure(Scene scene)
    {
        scene.OnRender(entity =>
        {
            foreach (var element in entity.Components.OfType<UIElement>())
            {
                element.Update(entity);
                element.Render(entity.WorldTransform, Graphics);
            }
        });
    }
}
