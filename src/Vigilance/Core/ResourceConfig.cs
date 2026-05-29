namespace Vigilance.Core;

public sealed class ResourceConfig
{
    public string WorkingNamespace { get; set; } = "Resources";
}

public static class ResourceConfigExtensions
{
    public static ConfigBuilder Resource(this ConfigBuilder builder, Action<ResourceConfig> config)
    {
        return builder.Add(config);
    }
}
