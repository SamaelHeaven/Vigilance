using System.Text;

namespace Vigilance.Net;

public sealed class HttpRequest(string url, string method, Action<HttpResponse>? onComplete)
{
    public HttpRequest()
        : this("", null) { }

    public HttpRequest(string url, Action<HttpResponse>? onComplete)
        : this(url, "GET", onComplete) { }

    public string Url { get; set; } = url;
    public string Method { get; set; } = method;
    public TimeSpan Timeout { get; set; }
    public HttpHeaders Headers { get; set; } = [];
    public byte[]? Body { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Body ?? []);
        set => Body = Encoding.UTF8.GetBytes(value);
    }

    public Action<HttpResponse>? OnComplete { get; set; } = onComplete;
}
