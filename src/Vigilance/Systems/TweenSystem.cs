namespace Vigilance.Systems;

public sealed class TweenSystem : GameSystem
{
    private ValueList<Tween> _resume = [];

    public override void Update()
    {
        var delta = Time.Delta;
        Scene.BeginDefer();
        try
        {
            foreach (var tweenRef in RefComponents<ValueTween>())
                tweenRef.Write.Update(delta);

            foreach (var tweenRef in RefComponents<Tween>())
            {
                var tween = tweenRef.Read;
                if (tween.IsPaused)
                    continue;
                tween.Update(delta);
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
