namespace Vigilance.Core;

public static class Hooks
{
    private static HooksConfig _config = new();

    public static Action? OnExit { get; set; } = _config.OnExit;

    internal static void Initialize()
    {
        _config = Game.Config.Take<HooksConfig>() ?? _config;
        OnExit = _config.OnExit;
    }
}

public class HooksConfig
{
    public Action? OnExit { get; set; }
}

public static class HooksConfigExtension
{
    public static ConfigBuilder Hooks(this ConfigBuilder builder, Action<HooksConfig> config)
    {
        return builder.Add(config);
    }
}
