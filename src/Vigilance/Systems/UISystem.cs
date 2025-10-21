using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void BeginRender()
    {
        foreach (var (entity, component) in Scene.Entries<UIComponent>().WithDisabled())
        {
            var element = component.Element;
            var layoutReady = element.IsLayoutReady;
            element.CalculateLayout();
            if (!layoutReady || !entity.IsDisabled)
                element.Update(entity);
        }
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange(
            this,
            Scene.Entries<UIComponent>().WithDisabled(),
            static (entity, self, component) => component.Element.Render(entity.WorldTransform, self.Graphics)
        );
    }
}
