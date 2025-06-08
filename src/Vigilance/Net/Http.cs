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
        Client.Fetch(request);
    }

    internal static void CompleteFetch(HttpRequest request, HttpResponse response)
    {
        var method = request.Method.ToUpper();
        var url = Uri.EscapeDataString(request.Url).Replace("%2F", "/").Replace("%3A", ":");
        var statusCode = response.StatusCode;
        var statusText = response.StatusText;
        var logLevel = response.Success ? LogLevel.Info : LogLevel.Error;
        var logMessage = $"FETCH: {method} \"{url}\"{(statusCode == 0 ? "" : $" {statusCode}")} ({statusText})";
        Game.Log(logLevel, logMessage);
        request.OnComplete?.Invoke(response);
    }
}
