namespace Vigilance.Core;

public static class Ecs
{
    private static EcsConfig _config = null!;

    public static GameSystemsFunc Systems => _config.Systems;

    internal static void Initialize()
    {
        _config = Game.Config.Take<EcsConfig>() ?? new EcsConfig();
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
