using Vigilance.Collections;
using Vigilance.Core;
using Timer = Vigilance.Core.Timer;

namespace Vigilance.Systems;

public sealed class TimerSystem : GameSystem
{
    private ValueList<Timer> _resume = [];

    public override void Update()
    {
        Scene.BeginDefer();
        try
        {
            foreach (var timer in Components<Timer>())
            {
                if (timer.IsPaused)
                    continue;
                timer.Update();
                if (timer.IsPaused)
                    continue;
                _resume.Add(timer);
                timer.IsPaused = true;
            }

            foreach (var timer in _resume)
                timer.IsPaused = false;
            _resume.Clear();
        }
        finally
        {
            Scene.EndDefer();
        }
    }
}
