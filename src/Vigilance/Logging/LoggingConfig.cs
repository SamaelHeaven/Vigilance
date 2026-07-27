namespace Vigilance.Logging;

public sealed class LoggingConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.All;
    public ILogger? Logger { get; set; } = new ConsoleLogger();
}

public static class LoggingConfigExtensions
{
    public static ConfigBuilder Logging(this ConfigBuilder builder, Action<LoggingConfig> config)
    {
        return builder.Add(config);
    }
}
