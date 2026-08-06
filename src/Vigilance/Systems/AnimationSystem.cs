namespace Vigilance.Systems;

public sealed class AnimationSystem() : GameSystem<AnimationSystem>(queryWithDisabled: true)
{
    private ValueList<IAnimation> _resume = [];

    [GenericRegistry]
    public static void Register<T>()
        where T : IAnimation
    {
        ConfigureEach(
            typeof(T),
            system =>
            {
                system.Scene.OnUpdate(system.Update<T>);
                system.Scene.OnPreRender(system.PreRender<T>);
            }
        );
    }

    private void Update<T>()
        where T : IAnimation
    {
        var delta = Time.Delta;
        if (typeof(T).IsValueType)
        {
            foreach (var animationRef in RefComponents<T>())
                try
                {
                    animationRef.AsWritable().Value.Update(delta);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

            return;
        }

        Scene.BeginDefer();
        foreach (var animationRef in RefComponents<T>())
            try
            {
                var animation = animationRef.Read;
                if (animation.IsPaused)
                    continue;
                animation.Update(delta);
                if (animation.IsPaused)
                    continue;
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
        Scene.EndDefer();
    }

    private void PreRender<T>()
        where T : IAnimation
    {
        foreach (var (entity, animationRef) in RefEntries<T>())
            try
            {
                animationRef.AsWritable().Value.Apply(entity);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }
}
