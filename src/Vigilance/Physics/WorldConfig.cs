using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Physics;

public sealed class WorldConfig
{
    public Vector2 DefaultGravity { get; set; } = new(0, World.MetersToPixels(9.807f));
}

public static class WorldConfigExtensions
{
    public static ConfigBuilder World(this ConfigBuilder configs, Action<WorldConfig> config)
    {
        return configs.Add(config);
    }
}
