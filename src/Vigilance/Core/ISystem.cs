namespace Vigilance.Core;

public delegate IEnumerable<ISystem> SystemsFunc();

public interface ISystem
{
    public void Configure(Scene scene);
}
