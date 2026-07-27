namespace Vigilance.Drawing;

public interface IAnimationFrame
{
    TimeSpan Delay => TimeSpan.Zero;

    void Apply(Entity entity);
}
