namespace Vigilance.Systems;

public sealed class TimerSystem : GameSystem
{
    private ValueList<Timer> _resume = [];

    public override void Update()
    {
        var delta = Time.Delta;
        foreach (var timerRef in RefComponents<ValueTimer>())
            timerRef.Write.Update(delta);

        Scene.BeginDefer();
        try
        {
            foreach (var timer in Components<Timer>())
            {
                if (timer.IsPaused)
                    continue;
                timer.Update(delta);
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
