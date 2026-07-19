using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(queryWithDisabled: true)
{
    public override void Update()
    {
        foreach (var animation in AssignableComponents<IAnimation>().AsValueEnumerable().Distinct())
            try
            {
                animation.Update();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public override void PreRender()
    {
        foreach (var (entity, animation) in AssignableEntries<IAnimation>())
            try
            {
                animation.Apply(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }
}
