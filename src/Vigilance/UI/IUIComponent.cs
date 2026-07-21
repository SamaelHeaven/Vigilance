namespace Vigilance.UI;

public interface IUIComponent
{
    bool IsPersistant => false;

    void Attach(UIElement element);

    void Detach(UIElement element);
}

public abstract class UIComponent : IUIComponent
{
    public bool IsPersistant { get; set; }

    public abstract void Attach(UIElement element);

    public abstract void Detach(UIElement element);
}
