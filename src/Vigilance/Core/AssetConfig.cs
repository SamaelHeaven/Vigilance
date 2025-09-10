namespace Vigilance.Core;

public sealed class AssetConfig
{
    public CacheType DefaultCacheType { get; set; } = CacheType.Weak;
}

public static class AssetConfigExtensions
{
    public static ConfigsBuilder Asset(this ConfigsBuilder configs, AssetConfig config)
    {
        return configs.AddConfig(config);
    }
}
