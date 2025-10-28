namespace Vigilance.Core;

public sealed class Ecs
{
    private static EcsConfig _config = null!;

    public static GameSystemsFunc Systems => _config.Systems;

    public static bool DefaultEnableRuntimeComponents
    {
        get => _config.DefaultEnableRuntimeComponents;
        set => _config.DefaultEnableRuntimeComponents = value;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<EcsConfig>() ?? new EcsConfig();
    }
}

public sealed class EcsConfig
{
    public GameSystemsFunc Systems { get; set; } = Array.Empty<IGameSystem>;

    public bool DefaultEnableRuntimeComponents { get; set; } = true;
}

public static class EcsConfigExtensions
{
    public static ConfigBuilder Ecs(this ConfigBuilder builder, EcsConfig config)
    {
        return builder.Add(config);
    }
}
