namespace Vigilance.Core;

public static class Hooks
{
    public delegate void Exception(System.Exception exception, out bool rethrow);

    public delegate void Exit();

    public delegate void SetScene(Scene oldScene, Scene newScene);

    private static HooksConfig _config = new();

    public static Exit? OnExit { get; set; } = _config.OnExit;

    public static Exception? OnException { get; set; } = _config.OnException;

    public static SetScene? OnSetScene { get; set; } = _config.OnSetScene;

    internal static void Initialize()
    {
        _config = Game.Config.Take<HooksConfig>() ?? _config;
        OnExit = _config.OnExit;
        OnException = _config.OnException;
        OnSetScene = _config.OnSetScene;
    }
}

public class HooksConfig
{
    public Hooks.Exit? OnExit { get; set; }
    public Hooks.Exception? OnException { get; set; }
    public Hooks.SetScene? OnSetScene { get; set; }
}

public static class HooksConfigExtension
{
    public static ConfigBuilder Hooks(this ConfigBuilder builder, Action<HooksConfig> config)
    {
        return builder.Add(config);
    }
}
