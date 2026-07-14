using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Net;

internal sealed unsafe class HttpClientWeb : IHttpClient
{
    private static readonly ConcurrentDictionary<nint, HttpRequest> _requests = new();
    private static long _requestId = 0;

    public void Fetch(HttpRequest request)
    {
        var headersBuffer = nint.Zero;
        nint[]? headerBuffers = null;
        try
        {
            var id = (nint)Interlocked.Increment(ref _requestId);
            var method = Encoding.UTF8.GetBytes(request.Method);
            var attr = new EmscriptenFetchAttr();
            Emscripten.FetchAttrInit(ref attr);
            attr.UserData = id;
            attr.Attributes = 1;
            for (var i = 0; i < method.Length.Min(EmscriptenFetchAttr.RequestMethodSize); i++)
                attr.RequestMethod[i] = method[i];
            attr.RequestMethod[EmscriptenFetchAttr.RequestMethodSize - 1] = 0;
            attr.TimeoutMSecs = (uint)request.Timeout.TotalMilliseconds;
            if (request.Headers is { Count: > 0 } headers)
            {
                var elements = headers.Count * 2 + 1;
                headersBuffer = Marshal.AllocHGlobal(elements * nint.Size);
                headerBuffers = new nint[elements];
                var idx = 0;
                foreach (var (key, value) in headers)
                {
                    headerBuffers[idx++] = Marshal.StringToCoTaskMemUTF8(key);
                    headerBuffers[idx++] = Marshal.StringToCoTaskMemUTF8(value);
                }

                headerBuffers[idx] = nint.Zero;
                Marshal.Copy(headerBuffers, 0, headersBuffer, elements);
                attr.RequestHeaders = headersBuffer;
                attr.RequestHeadersLength = (uint)(elements - 1);
            }

            fixed (byte* body = request.Body)
            {
                attr.RequestData = (nint)body;
                attr.RequestDataSize = (nuint)(request.Body?.Length ?? 0);
                attr.OnSuccess = &OnFetchComplete;
                attr.OnError = &OnFetchComplete;
                _requests[id] = request;
                Emscripten.Fetch(ref attr, request.Url);
            }
        }
        catch (Exception e)
        {
            Http.CompleteFetch(new HttpResponse { Request = request, StatusText = e.Message });
        }
        finally
        {
            if (headerBuffers is not null)
            {
                foreach (var p in headerBuffers)
                    if (p != nint.Zero)
                        Marshal.FreeCoTaskMem(p);
                Marshal.FreeHGlobal(headersBuffer);
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnFetchComplete(EmscriptenFetch* fetch)
    {
        var id = fetch->UserData;
        var response = new HttpResponse();
        try
        {
            response.StatusCode = fetch->Status;
            response.StatusText = Utf8Ptr.GetString(fetch->StatusText);
            response.Body = new byte[fetch->TotalBytes];
            if (fetch->Data != nint.Zero)
                Marshal.Copy(fetch->Data, response.Body, 0, response.Body.Length);
            var headersLength = Emscripten.FetchGetResponseHeadersLength(fetch);
            var headersBytes = new byte[headersLength + 1];
            fixed (byte* headersBuffer = headersBytes)
            {
                Emscripten.FetchGetResponseHeaders(fetch, headersBuffer, headersLength + 1);
            }

            var headersText = Encoding.UTF8.GetString(headersBytes);
            foreach (var line in headersText.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries))
            {
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0)
                    continue;
                var name = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();
                response.Headers[name] = value;
            }
        }
        catch (Exception e)
        {
            response = new HttpResponse { StatusText = e.Message };
        }
        finally
        {
            Emscripten.FetchClose(fetch);
            if (_requests.Remove(id, out var request))
            {
                response.Request = request;
                Http.CompleteFetch(response);
            }
        }
    }
}
