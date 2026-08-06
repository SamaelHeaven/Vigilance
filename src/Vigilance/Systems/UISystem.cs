namespace Vigilance.Systems;

public sealed class UISystem(Graphics graphics) : GameSystem<UISystem>(queryWithDisabled: true)
{
    public UISystem()
        : this(Renderer.Graphics) { }

    public Graphics Graphics { get; set; } = graphics;

    [GenericRegistry]
    public static void Register<T>()
        where T : UIElement
    {
        ConfigureEach(
            typeof(T),
            system =>
            {
                system.Scene.OnUpdate(system.Update<T>);
                system.Scene.OnRender(system.Render<T>);
            }
        );
    }

    private void Update<T>()
        where T : UIElement
    {
        foreach (var (entity, elementRef) in RefEntries<T>())
        {
            var element = elementRef.Read;
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

    private void Render<T>(RenderCommands commands)
        where T : UIElement
    {
        commands.AddEntries<UISystem, T>(
            this,
            (system, entity, element) => element.Render(entity.RenderTransform, system.Graphics)
        );
    }
}
