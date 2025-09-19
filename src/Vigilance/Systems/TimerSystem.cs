using Vigilance.Core;
using Timer = Vigilance.Core.Timer;

namespace Vigilance.Systems;

public sealed class TimerSystem : GameSystem
{
    public override void Update()
    {
        var step = Time.Delta;
        foreach (var timer in Scene.Components<Timer>())
            timer.Update(step);
    }
}
