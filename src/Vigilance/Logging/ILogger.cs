namespace Vigilance.Logging;

public interface ILogger
{
    void Log(LogLevel level, string message);
}
