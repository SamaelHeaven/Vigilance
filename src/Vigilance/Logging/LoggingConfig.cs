using Vigilance.Core;

namespace Vigilance.Logging;

public sealed class LoggingConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.All;
    public ILogger? Logger { get; set; } = new ConsoleLogger();
}

public static class LoggingConfigExtensions
{
    public static ConfigBuilder Logging(this ConfigBuilder builder, LoggingConfig config)
    {
        return builder.Add(config);
    }
}
