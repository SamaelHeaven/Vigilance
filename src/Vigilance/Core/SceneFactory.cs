namespace Vigilance.Core;

public abstract class SceneFactory : GameSystem
{
    public static Scene BuildScene<T>(GameSystemsFunc? systems = null)
        where T : SceneFactory, new()
    {
        return new Scene(() => systems is null ? [new T()] : systems.Invoke().Concat([new T()]));
    }

    public static Scene BuildScene<T>(Func<T> factory, GameSystemsFunc? systems = null)
        where T : SceneFactory
    {
        return new Scene(() => systems is null ? [factory.Invoke()] : systems.Invoke().Concat([factory.Invoke()]));
    }
}
