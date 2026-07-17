using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: true)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Update()
    {
        foreach (var (entity, element) in AssignableEntries<UIElement>())
        {
            if (!element.IsLayoutReady)
            {
                if (element.IsImmediate)
                    element.Update(entity);
                element.CalculateLayout();
            }

            element.Update(entity);
            element.CalculateLayout();
        }
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddAssignableEntries<UISystem, UIElement>(
            this,
            (system, entity, element) => element.Render(entity.RenderTransform, system.Graphics)
        );
    }
}
