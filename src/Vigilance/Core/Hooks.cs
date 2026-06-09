namespace Vigilance.Core;

public static class Hooks
{
    public delegate void Exception(System.Exception exception, out bool rethrow);

    public delegate void Quit();
    public delegate void SetScene(Scene oldScene, Scene newScene);

    private static HooksConfig _config = new();

    public static Quit? OnQuit
    {
        get => _config.OnQuit;
        set => _config.OnQuit = value;
    }

    public static Exception? OnException
    {
        get => _config.OnException;
        set => _config.OnException = value;
    }

    public static SetScene? OnSetScene
    {
        get => _config.OnSetScene;
        set => _config.OnSetScene = value;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<HooksConfig>() ?? _config;
    }
}

public class HooksConfig
{
    public Hooks.Quit? OnQuit { get; set; }
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
