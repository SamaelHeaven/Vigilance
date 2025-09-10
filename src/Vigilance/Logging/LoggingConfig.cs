using Vigilance.Core;

namespace Vigilance.Logging;

public sealed class LoggingConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.All;
    public ILogger? Logger { get; set; } = new ConsoleLogger();
}

public static class LoggingConfigExtensions
{
    public static ConfigsBuilder Logging(this ConfigsBuilder configs, LoggingConfig config)
    {
        return configs.AddConfig(config);
    }
}
