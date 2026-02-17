using Vigilance.Collections;

namespace Vigilance.Net;

public sealed class HttpHeaders : Dictionary<string, string>
{
    public HttpHeaders()
        : base(StringComparer.OrdinalIgnoreCase) { }

    public HttpHeaders(params ReadOnlySpan<(string, string)> headers)
        : this()
    {
        foreach (var (key, value) in headers)
            Add(key, value);
    }

    public HttpHeaders(params ReadOnlySpan<KeyValuePair<string, string>> headers)
        : this()
    {
        foreach (var (key, value) in headers)
            Add(key, value);
    }

    public HttpHeaders(IEnumerable<(string, string)> headers)
        : this(headers.AsSpan()) { }

    public HttpHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        : this(headers.AsSpan()) { }
}
