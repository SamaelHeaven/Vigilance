namespace Vigilance.Core;

public static class Hooks
{
    public delegate void Exception(System.Exception exception, out bool rethrow);

    public delegate void Exit();

    private static HooksConfig _config = new();

    public static Exit? OnExit { get; set; } = _config.OnExit;

    public static Exception? OnException { get; set; } = _config.OnException;

    internal static void Initialize()
    {
        _config = Game.Config.Take<HooksConfig>() ?? _config;
        OnExit = _config.OnExit;
        OnException = _config.OnException;
    }
}

public class HooksConfig
{
    public Hooks.Exit? OnExit { get; set; }
    public Hooks.Exception? OnException { get; set; }
}

public static class HooksConfigExtension
{
    public static ConfigBuilder Hooks(this ConfigBuilder builder, Action<HooksConfig> config)
    {
        return builder.Add(config);
    }
}
