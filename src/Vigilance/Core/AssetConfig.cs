namespace Vigilance.Core;

public sealed class AssetConfig
{
    public CacheType DefaultCacheType { get; set; } = CacheType.Weak;
}

public static class AssetConfigExtensions
{
    public static ConfigBuilder Asset(this ConfigBuilder builder, AssetConfig config)
    {
        return builder.Add(config);
    }
}
