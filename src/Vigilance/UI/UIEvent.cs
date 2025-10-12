using Vigilance.Core;

namespace Vigilance.UI;

public readonly record struct UIEvent(Entity Entity, UIElement Element)
{
    public T GetElement<T>()
        where T : UIElement
    {
        return (T)Element;
    }
}

public readonly record struct UIEvent<T>(Entity Entity, T Element)
    where T : UIElement
{
    public UIEvent(UIEvent e)
        : this(e.Entity, (T)e.Element) { }

    public static implicit operator UIEvent<T>(UIEvent e)
    {
        return new UIEvent<T>(e);
    }
}
