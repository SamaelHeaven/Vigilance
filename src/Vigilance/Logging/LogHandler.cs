using System.Runtime.CompilerServices;

namespace Vigilance.Logging;

[InterpolatedStringHandler]
public ref struct LogHandler
{
    private DefaultInterpolatedStringHandler _inner;
    private readonly LogLevel _level;

    public bool IsEnabled => _level > Log.LogLevel;

    public LogHandler(int literalLength, int formattedCount, LogLevel level)
    {
        _level = level;
        _inner = IsEnabled ? new DefaultInterpolatedStringHandler(literalLength, formattedCount) : default;
    }

    public void AppendLiteral(string str)
    {
        if (IsEnabled)
            _inner.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        if (IsEnabled)
            _inner.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return IsEnabled ? _inner.ToStringAndClear() : "";
    }
}

[InterpolatedStringHandler]
public ref struct TraceLogHandler
{
    private LogHandler _handler;

    public TraceLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Trace);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}

[InterpolatedStringHandler]
public ref struct DebugLogHandler
{
    private LogHandler _handler;

    public DebugLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Debug);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}

[InterpolatedStringHandler]
public ref struct InfoLogHandler
{
    private LogHandler _handler;

    public InfoLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Info);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}

[InterpolatedStringHandler]
public ref struct WarningLogHandler
{
    private LogHandler _handler;

    public WarningLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Warning);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}

[InterpolatedStringHandler]
public ref struct ErrorLogHandler
{
    private LogHandler _handler;

    public ErrorLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Error);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}

[InterpolatedStringHandler]
public ref struct FatalLogHandler
{
    private LogHandler _handler;

    public FatalLogHandler(int literalLength, int formattedCount)
    {
        _handler = new LogHandler(literalLength, formattedCount, LogLevel.Fatal);
    }

    public void AppendLiteral(string str)
    {
        _handler.AppendLiteral(str);
    }

    public void AppendFormatted<T>(T value)
    {
        _handler.AppendFormatted(value);
    }

    public string GetFormattedText()
    {
        return _handler.GetFormattedText();
    }
}
