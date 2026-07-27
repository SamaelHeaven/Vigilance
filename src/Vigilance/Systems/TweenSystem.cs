namespace Vigilance.Systems;

public sealed class TweenSystem : GameSystem
{
    private ValueList<Tween> _resume = [];

    public override void Update()
    {
        Scene.BeginDefer();
        try
        {
            foreach (var tween in Components<Tween>())
            {
                if (tween.IsPaused)
                    continue;
                tween.Update();
                if (tween.IsPaused)
                    continue;
                _resume.Add(tween);
                tween.IsPaused = true;
            }

            foreach (var tween in _resume)
                tween.IsPaused = false;
            _resume.Clear();
        }
        finally
        {
            Scene.EndDefer();
        }
    }
}
