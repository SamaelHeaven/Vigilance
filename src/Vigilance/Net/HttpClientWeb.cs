using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Net;

internal sealed unsafe class HttpClientWeb : IHttpClient
{
    private static readonly Dictionary<nint, Action<HttpResponse>?> FetchCallbacks = new();
    private static nint _fetchId = 0;

    public void Fetch(HttpRequest request)
    {
        var headersBuffer = nint.Zero;
        nint[]? headerBuffers = null;
        try
        {
            var id = _fetchId++;
            var method = Encoding.UTF8.GetBytes(request.Method);
            using var overriddenMimeType = request.ContentType.ToUtf8Buffer();
            var attr = new EmscriptenFetchAttr();
            Emscripten.FetchAttrInit(ref attr);
            attr.UserData = id;
            attr.Attributes = 1;
            for (var i = 0; i < System.Math.Min(method.Length, EmscriptenFetchAttr.RequestMethodSize); i++)
                attr.RequestMethod[i] = method[i];
            attr.RequestMethod[EmscriptenFetchAttr.RequestMethodSize - 1] = 0;
            attr.TimeoutMSecs = (uint)request.Timeout.TotalMilliseconds;
            attr.OverriddenMimeType = (nint)overriddenMimeType.AsPointer();
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
                FetchCallbacks[id] = request.OnComplete;
                Emscripten.Fetch(ref attr, request.Url);
            }
        }
        catch (Exception e)
        {
            request.OnComplete?.Invoke(new HttpResponse { StatusText = e.Message });
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

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnFetchComplete(EmscriptenFetch* fetch)
    {
        var id = fetch->UserData;
        var response = new HttpResponse();
        try
        {
            response.StatusCode = fetch->Status;
            response.StatusText = Marshal.PtrToStringUTF8((nint)fetch->StatusText) ?? "";
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
            FetchCallbacks[id]?.Invoke(response);
            FetchCallbacks.Remove(id);
        }
    }
}
