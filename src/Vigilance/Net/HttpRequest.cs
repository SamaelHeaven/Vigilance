using System.Text;

namespace Vigilance.Net;

public sealed class HttpRequest(string url, string method, Action<HttpResponse>? onComplete)
{
    public HttpRequest(string url, Action<HttpResponse>? onComplete)
        : this(url, "GET", onComplete) { }

    public string Url { get; } = url;
    public string Method { get; init; } = method;
    public TimeSpan Timeout { get; set; }
    public HttpHeaders Headers { get; init; } = new();
    public byte[]? Body { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Body ?? []);
        set => Body = Encoding.UTF8.GetBytes(value);
    }

    public event Action<HttpResponse>? OnComplete = onComplete;

    internal void Complete(HttpResponse response)
    {
        OnComplete?.Invoke(response);
    }
}
