namespace Vigilance.Core;

public delegate IReadOnlyCollection<ISystem> SystemsFunc();

public interface ISystem
{
    public void Configure(Scene scene);
}
