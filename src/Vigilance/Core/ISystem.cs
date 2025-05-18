namespace Vigilance.Core;

public delegate IReadOnlyCollection<ISystem> GetSystemsDelegate();

public interface ISystem
{
    public void Configure(Scene scene);
}
