using Vigilance.Core;

namespace Vigilance.Systems;

public sealed class TweenSystem : GameSystem
{
    public override void Update()
    {
        foreach (var tween in Components<Tween>())
            tween.Update();
    }
}
