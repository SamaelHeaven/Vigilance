using Vigilance.Core;

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
}
