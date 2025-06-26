using Vigilance.Core;
using Timer = Vigilance.Core.Timer;

namespace Vigilance.Systems;

public sealed class TimerSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnUpdate(() =>
        {
            var step = Time.Delta;
            scene.Each((Timer timer) => timer.Update(step));
        });
    }
}
