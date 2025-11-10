namespace Vigilance.Core;

public sealed class FileSystemConfig
{
    public string WorkingDirectory { get; set; } = "";

    public string WorkingNamespace { get; set; } = "Resources";
}

public static class FileSystemConfigExtensions
{
    public static ConfigBuilder FileSystem(this ConfigBuilder builder, Action<FileSystemConfig> config)
    {
        return builder.Add(config);
    }
}
