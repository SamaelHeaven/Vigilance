using System.Text;

namespace Vigilance.Net;

public sealed class HttpResponse
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = "";
    public HttpHeaders Headers { get; } = new();
    public byte[] Body { get; set; } = Array.Empty<byte>();

    public string Text
    {
        get => Encoding.UTF8.GetString(Body);
        set => Body = Encoding.UTF8.GetBytes(value);
    }

    public bool Success => StatusCode is >= 200 and <= 299;
}
