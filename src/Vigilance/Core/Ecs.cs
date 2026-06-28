namespace Vigilance.Core;

public static class Ecs
{
    private static EcsConfig _config = new();

    public static GameSystemsFunc Systems { get; private set; } = _config.Systems;

    internal static void Initialize()
    {
        _config = Game.Config.Take<EcsConfig>() ?? _config;
        Systems = _config.Systems;
    }
}

public sealed class EcsConfig
{
    public GameSystemsFunc Systems { get; set; } = Array.Empty<IGameSystem>;
}

public static class EcsConfigExtensions
{
    extension(ConfigBuilder builder)
    {
        public ConfigBuilder Ecs(Action<EcsConfig> config)
        {
            return builder.Add(config);
        }

        public ConfigBuilder Systems(GameSystemsFunc config)
        {
            return builder.Ecs(ecs => ecs.Systems = config);
        }
    }
}
