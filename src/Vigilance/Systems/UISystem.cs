using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.UI;

namespace Vigilance.Systems;

public sealed class UISystem(Graphics? graphics = null) : GameSystem(queryWithDisabled: Inclusion.Include)
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Update()
    {
        foreach (var (entity, element) in Entries<UIElement>())
        {
            element.CalculateLayout();
            if (!entity.IsDisabled)
                element.Update(entity);
        }
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddRange<UISystem, UIElement>(
            this,
            static (entity, self, element) => element.Render(entity.WorldTransform, self.Graphics)
        );
    }
}
