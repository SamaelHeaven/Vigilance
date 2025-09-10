using System.Web;
using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Net;

public static class Http
{
    private static readonly IHttpClient Client;

    static Http()
    {
        Client = Game.Platform switch
        {
            Platform.Web => new HttpClientWeb(),
            _ => new HttpClientCore(),
        };
    }

    public static void Fetch(HttpRequest request)
    {
        Game.EnsureRunning();
        Client.Fetch(request);
    }

    internal static void CompleteFetch(HttpRequest request, HttpResponse response)
    {
        var method = Uri.EscapeDataString(request.Method.ToUpper());
        var url = HttpUtility.UrlPathEncode(request.Url).Replace("\"", "%22");
        var statusCode = response.StatusCode;
        var statusText = response.StatusText;
        var logLevel = response.Success ? LogLevel.Info : LogLevel.Error;
        var logMessage = $"FETCH: {method} \"{url}\"{(statusCode == 0 ? "" : $" {statusCode}")} ({statusText})";
        Logger.Log(logLevel, logMessage);
        Game.Defer(() => request.OnComplete?.Invoke(response));
    }
}
