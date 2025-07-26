using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem : GameSystem
{
    public Graphics Graphics { get; set; } = Renderer.Graphics;

    public override void Render(Entity entity)
    {
        foreach (var element in entity.Components.OfType<UIElement>())
        {
            element.CalculateLayout();
            element.Update(entity);
            element.Render(entity.WorldTransform, Graphics);
        }
    }
}
