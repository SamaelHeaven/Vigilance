using ZLinq;

namespace Vigilance.Net;

internal sealed class HttpClientCore : IHttpClient
{
    private static readonly HttpClient Client = new();

    public async void Fetch(HttpRequest request)
    {
        HttpResponse response = null!;
        try
        {
            response = new HttpResponse();
            using var requestMessage = new HttpRequestMessage();
            requestMessage.Method = new HttpMethod(request.Method);
            requestMessage.RequestUri = new Uri(request.Url);
            requestMessage.Content = request.Body is { Length: > 0 } ? new ByteArrayContent(request.Body) : null;
            foreach (
                var header in request
                    .Headers.AsValueEnumerable()
                    .Where(header =>
                        // ReSharper disable once AccessToDisposedClosure
                        !requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value)
                    )
            )
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
            HttpResponseMessage responseMessage;
            if (request.Timeout != TimeSpan.Zero)
            {
                using var cancellationTokenSource = new CancellationTokenSource(request.Timeout);
                responseMessage = await Client.SendAsync(
                    requestMessage,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationTokenSource.Token
                );
            }
            else
            {
                responseMessage = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
            }

            response.StatusCode = (int)responseMessage.StatusCode;
            response.StatusText = responseMessage.ReasonPhrase ?? "";
            response.Body = await responseMessage.Content.ReadAsByteArrayAsync();
            foreach (var header in responseMessage.Headers)
                response.Headers[header.Key] = header.Value.AsValueEnumerable().JoinToString(", ");
            foreach (var header in responseMessage.Content.Headers)
                response.Headers[header.Key] = header.Value.AsValueEnumerable().JoinToString(", ");
        }
        catch (Exception e)
        {
            response = new HttpResponse { StatusText = e.Message };
        }
        finally
        {
            Http.CompleteFetch(request, response);
        }
    }
}
