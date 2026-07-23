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
                try
                {
                    if (animation.IsPaused)
                        continue;
                    animation.Update();
                    if (!animation.IsPaused)
                        _resume.Add(animation);
                    animation.IsPaused = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

            foreach (var animation in _resume)
                try
                {
                    animation.IsPaused = false;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

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
