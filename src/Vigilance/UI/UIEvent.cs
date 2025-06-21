using Vigilance.Core;

namespace Vigilance.UI;

public struct UIEvent
{
    public Entity Entity { get; set; }
    public UIElement Element { get; set; }

    public T GetElement<T>()
        where T : UIElement
    {
        return (T)Element;
    }
}
