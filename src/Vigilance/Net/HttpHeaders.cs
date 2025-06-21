namespace Vigilance.Net;

public sealed class HttpHeaders : Dictionary<string, string>
{
    public HttpHeaders()
        : base(StringComparer.OrdinalIgnoreCase) { }
}
