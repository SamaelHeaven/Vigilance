namespace Vigilance.Core;

public delegate IEnumerable<ISystem> SystemsFunc();

public interface ISystem
{
    void Configure(Scene scene);
}
