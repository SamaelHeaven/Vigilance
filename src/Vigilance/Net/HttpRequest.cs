using System.Text;

namespace Vigilance.Net;

public sealed class HttpRequest(string url, string method, Action<HttpResponse>? onComplete = null)
{
    public HttpRequest(string url, Action<HttpResponse>? onComplete = null)
        : this(url, "GET", onComplete) { }

    public string Url { get; } = url;
    public Action<HttpResponse>? OnComplete { get; } = onComplete;
    public string Method { get; init; } = method;
    public TimeSpan Timeout { get; set; }
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public byte[]? Body { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Body ?? Array.Empty<byte>());
        set => Body = Encoding.UTF8.GetBytes(value);
    }
}
