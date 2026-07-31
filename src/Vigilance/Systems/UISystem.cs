namespace Vigilance.Systems;

public sealed class UISystem(Graphics graphics) : GameSystem(queryWithDisabled: true)
{
    public UISystem()
        : this(Renderer.Graphics) { }

    public Graphics Graphics { get; set; } = graphics;

    public override void Update()
    {
        ForEach<UIElement>(
            (entity, element) =>
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
        );
    }

    public override void Render(RenderCommands commands)
    {
        commands.AddEachEntries<UISystem, UIElement>(
            this,
            (system, entity, element) => element.Render(entity.RenderTransform, system.Graphics)
        );
    }
}
