namespace Vigilance.Core;

public interface IComposable<out T>
{
    T ToComponent();
}
