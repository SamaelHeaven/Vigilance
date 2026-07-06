using Vigilance.Core;

namespace Vigilance.Drawing;

public interface IAnimation
{
    void Update(TimeSpan? step = null);

    void Apply(Entity entity);

    void Reset();
}
