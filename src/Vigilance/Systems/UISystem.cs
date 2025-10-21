using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Render(RenderCommands commands)
    {
        commands.AddRange(
            this,
            Scene.Entries<UIComponent>().WithDisabled(),
            static (entity, self, component) =>
            {
                var element = component.Element;
                var layoutReady = element.LayoutReady;
                element.CalculateLayout();
                if (!layoutReady || !entity.Disabled)
                    element.Update(entity);
                element.Render(entity.WorldTransform, self.Graphics);
            }
        );
    }
}
