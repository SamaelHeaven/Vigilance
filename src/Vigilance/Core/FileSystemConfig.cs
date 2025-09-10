namespace Vigilance.Core;

public sealed class FileSystemConfig
{
    public string WorkingDirectory { get; set; } = "";

    public string WorkingNamespace { get; set; } = $"{Assemblies.Game.GetName().Name}.Resources";
}

public static class FileSystemConfigExtensions
{
    public static ConfigsBuilder FileSystem(this ConfigsBuilder configs, FileSystemConfig config)
    {
        return configs.AddConfig(config);
    }
}
