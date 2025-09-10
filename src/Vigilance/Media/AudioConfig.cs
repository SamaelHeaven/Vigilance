using Vigilance.Core;

namespace Vigilance.Media;

public sealed class AudioConfig
{
    public float MasterVolume { get; set; } = 1f;

    public int DefaultSoundMaxAliases { get; set; } = 16;
}

public static class AudioConfigExtensions
{
    public static ConfigsBuilder Audio(this ConfigsBuilder configs, AudioConfig config)
    {
        return configs.AddConfig(config);
    }
}
