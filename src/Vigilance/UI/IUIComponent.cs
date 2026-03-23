namespace Vigilance.UI;

public interface IUIComponent
{
    void Attach(UIElement element);

    void Detach(UIElement element);
}
