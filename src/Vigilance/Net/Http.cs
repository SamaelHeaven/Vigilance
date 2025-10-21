using System.Web;
using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Net;

public static class Http
{
    private static readonly IHttpClient _client;

    static Http()
    {
        _client = Platform.Current switch
        {
            Platform.Web => new HttpClientWeb(),
            _ => new HttpClientCore(),
        };
    }

    public static void Fetch(HttpRequest request)
    {
        _client.Fetch(request);
    }

    internal static void CompleteFetch(HttpRequest request, HttpResponse response)
    {
        var method = Uri.EscapeDataString(request.Method.ToUpper());
        var url = HttpUtility.UrlPathEncode(request.Url).Replace("\"", "%22");
        var statusCode = response.StatusCode;
        var statusText = response.StatusText;
        var logLevel = response.IsSuccess ? LogLevel.Info : LogLevel.Error;
        Logger.Log(logLevel, $"FETCH: {method} \"{url}\"{(statusCode == 0 ? "" : $" {statusCode}")} ({statusText})");
        Game.Defer(() => request.OnComplete?.Invoke(response));
    }
}
