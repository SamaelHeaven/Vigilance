using System.Text;

// ReSharper disable CollectionNeverQueried.Global

namespace Vigilance.Net;

public sealed class HttpResponse
{
    public HttpRequest Request { get; set; } = null!;
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = "";
    public HttpHeaders Headers { get; set; } = [];
    public byte[] Body { get; set; } = [];

    public string Text => field ??= Encoding.UTF8.GetString(Body);

    public bool IsSuccess => StatusCode is >= 200 and <= 299;
}
