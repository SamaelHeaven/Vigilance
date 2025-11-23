using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: Inclusion.Include)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void PostUpdate()
    {
        foreach (var (entity, component) in Entries<UIComponent>())
        {
            var element = component.Element;
            element.CalculateLayout();
            if (!entity.IsDisabled)
                element.Update(entity);
        }
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange<UISystem, UIComponent>(
            this,
            static (entity, self, component) =>
            {
                var element = component.Element;
                element.Render(entity.WorldTransform, self.Graphics);
            }
        );
    }
}
