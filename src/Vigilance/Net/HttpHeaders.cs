using System.Runtime.CompilerServices;
using Vigilance.Collections;

namespace Vigilance.Net;

public sealed class HttpHeaders : Dictionary<string, string>
{
    [OverloadResolutionPriority(2)]
    public HttpHeaders()
        : base(StringComparer.OrdinalIgnoreCase) { }

    [OverloadResolutionPriority(1)]
    public HttpHeaders(params ReadOnlySpan<(string, string)> headers)
        : this()
    {
        foreach (var (key, value) in headers)
            Add(key, value);
    }

    [OverloadResolutionPriority(1)]
    public HttpHeaders(params ReadOnlySpan<KeyValuePair<string, string>> headers)
        : this()
    {
        foreach (var (key, value) in headers)
            Add(key, value);
    }

    public HttpHeaders(IEnumerable<(string, string)> headers)
        : this()
    {
        foreach (var (key, value) in headers.AsFastEnumerable())
            Add(key, value);
    }

    public HttpHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        : this()
    {
        foreach (var (key, value) in headers.AsFastEnumerable())
            Add(key, value);
    }
}
