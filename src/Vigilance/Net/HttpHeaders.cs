namespace Vigilance.Net;

public class HttpHeaders : Dictionary<string, string>
{
    public HttpHeaders()
        : base(StringComparer.OrdinalIgnoreCase) { }
}
