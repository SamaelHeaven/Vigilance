using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Logging;

namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(queryWithDisabled: true)
{
    private ValueList<IAnimation> _resume = [];

    public override void Update()
    {
        Scene.BeginDefer();
        try
        {
            foreach (var animation in AssignableComponents<IAnimation>())
            {
                if (animation.IsPaused)
                    continue;
                try
                {
                    animation.Update();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                if (!animation.IsPaused)
                    _resume.Add(animation);
                animation.IsPaused = true;
            }

            foreach (var animation in _resume)
                animation.IsPaused = false;
            _resume.Clear();
        }
        finally
        {
            Scene.EndDefer();
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
