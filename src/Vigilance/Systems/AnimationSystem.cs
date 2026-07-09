using Vigilance.Core;
using Vigilance.Drawing;
using ZLinq;

namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(queryWithDisabled: true)
{
    public override void Update()
    {
        foreach (var animation in AssignableComponents<IAnimation>().AsValueEnumerable().Distinct())
            animation.Update();
    }

    public override void PreRender()
    {
        foreach (var (entity, animation) in AssignableEntries<IAnimation>())
            animation.Apply(entity);
    }
}
