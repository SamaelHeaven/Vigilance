using System.Text;

namespace Vigilance.Net;

public sealed class HttpRequest(string url, string? method = null, Action<HttpResponse>? onComplete = null)
{
    public HttpRequest(string url, Action<HttpResponse>? onComplete = null)
        : this(url, null, onComplete) { }

    public string Url { get; } = url;
    public Action<HttpResponse>? OnComplete { get; } = onComplete;
    public string Method { get; init; } = method ?? "GET";
    public TimeSpan Timeout { get; set; }
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string? ContentType { get; set; }
    public byte[]? Body { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Body ?? Array.Empty<byte>());
        set => Body = Encoding.UTF8.GetBytes(value);
    }
}
