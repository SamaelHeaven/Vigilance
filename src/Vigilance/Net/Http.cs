using System.Web;

namespace Vigilance.Net;

public static class Http
{
    public static void Fetch(HttpRequest request)
    {
        switch (Platform.Current)
        {
            case Platform.Web:
                HttpClientWeb.Fetch(request);
                break;
            default:
                HttpClientCore.Fetch(request);
                break;
        }
    }

    internal static void CompleteFetch(HttpResponse response)
    {
        var method = Uri.EscapeDataString(response.Request.Method.ToUpper());
        var url = HttpUtility.UrlPathEncode(response.Request.Url).Replace("\"", "%22");
        var statusCode = response.StatusCode;
        var statusText = response.StatusText;
        var logLevel = response.IsSuccess ? LogLevel.Info : LogLevel.Error;
        Log.Invoke(logLevel, $"FETCH: {method} \"{url}\"{(statusCode == 0 ? "" : $" {statusCode}")} ({statusText})");
        Game.Defer(() => response.Request.OnComplete?.Invoke(response));
    }
}
