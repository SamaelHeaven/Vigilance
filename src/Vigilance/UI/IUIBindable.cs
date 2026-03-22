namespace Vigilance.UI;

public interface IUIBindable
{
    void Bind(UIElement element);

    void Unbind(UIElement element);
}

public interface IUIBindable<in T> : IUIBindable
    where T : UIElement
{
    void IUIBindable.Bind(UIElement element)
    {
        Bind((T)element);
    }

    void IUIBindable.Unbind(UIElement element)
    {
        Unbind((T)element);
    }

    void Bind(T element);

    void Unbind(T element);
}
