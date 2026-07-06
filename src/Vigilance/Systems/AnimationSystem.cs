using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(queryWithDisabled: true)
{
    public override void Update()
    {
        foreach (var (entity, animation) in AssignableEntries<IAnimation>())
        {
            animation.Update();
            animation.Apply(entity);
        }
    }
}
