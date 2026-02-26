using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: true)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Update()
    {
        foreach (var (entity, element) in Entries<UIElement>())
        {
            var ready = element.IsLayoutReady;
            element.CalculateLayout();
            if (entity.IsDisabled)
                continue;
            element.Update(entity);
            if (ready)
                continue;
            element.CalculateLayout();
            if (!entity.IsDisabled)
                element.Update(entity);
        }
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange<UISystem, UIElement>(
            this,
            static (system, entity, element) => element.Render(entity.WorldTransform, system.Graphics)
        );
    }
}
