namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem(queryWithDisabled: true)
{
    private TimeSpan _delta;
    private ValueList<IAnimation> _resume = [];

    public override void Update()
    {
        _delta = Time.Delta;
        Scene.BeginDefer();
        try
        {
            foreach (var table in Scene.Tables<IAnimation>())
                if (table.Type.IsValueType)
                    table.ForEach<AnimationSystem, IAnimation>(
                        this,
                        static (system, animation) => animation.Update(system._delta)
                    );
                else
                    table.ForEach<AnimationSystem, IAnimation>(
                        this,
                        static (system, animation) =>
                        {
                            if (animation.IsPaused)
                                return;
                            animation.Update(system._delta);
                            if (animation.IsPaused)
                                return;
                            system._resume.Add(animation);
                            animation.IsPaused = true;
                        }
                    );

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
        ForEach<IAnimation>((entity, animation) => animation.Apply(entity));
    }
}
