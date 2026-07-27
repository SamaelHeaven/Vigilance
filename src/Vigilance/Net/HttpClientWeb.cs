using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Vigilance.Net;

internal static unsafe class HttpClientWeb
{
    private static readonly ConcurrentDictionary<nint, HttpRequest> _requests = [];
    private static long _requestId = 0;

    public static void Fetch(HttpRequest request)
    {
        var headersBuffer = nint.Zero;
        nint[]? headerBuffers = null;
        var headerBuffersLength = 0;
        byte[]? methodBytes = null;
        try
        {
            var id = (nint)Interlocked.Increment(ref _requestId);
            var methodByteCount = Encoding.UTF8.GetByteCount(request.Method);
            methodBytes = ArrayPool<byte>.Shared.Rent(methodByteCount);
            var methodLength = Encoding.UTF8.GetBytes(request.Method, methodBytes);
            var attr = new EmscriptenFetchAttr();
            Emscripten.FetchAttrInit(ref attr);
            attr.UserData = id;
            attr.Attributes = 1;
            for (var i = 0; i < methodLength.Min(EmscriptenFetchAttr.RequestMethodSize); i++)
                attr.RequestMethod[i] = methodBytes[i];
            attr.RequestMethod[EmscriptenFetchAttr.RequestMethodSize - 1] = 0;
            attr.TimeoutMSecs = (uint)request.Timeout.TotalMilliseconds;
            if (request.Headers is { Count: > 0 } headers)
            {
                var elements = headers.Count * 2 + 1;
                headersBuffer = Marshal.AllocHGlobal(elements * nint.Size);
                headerBuffers = ArrayPool<nint>.Shared.Rent(elements);
                headerBuffersLength = elements;
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
            if (methodBytes is not null)
                ArrayPool<byte>.Shared.Return(methodBytes);
            if (headerBuffers is not null)
            {
                for (var i = 0; i < headerBuffersLength; i++)
                    if (headerBuffers[i] != nint.Zero)
                        Marshal.FreeCoTaskMem(headerBuffers[i]);
                ArrayPool<nint>.Shared.Return(headerBuffers, true);
                Marshal.FreeHGlobal(headersBuffer);
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnFetchComplete(EmscriptenFetch* fetch)
    {
        var id = fetch->UserData;
        var response = new HttpResponse();
        byte[]? headersBytes = null;
        try
        {
            response.StatusCode = fetch->Status;
            response.StatusText = Utf8Ptr.GetString(fetch->StatusText);
            response.Body = GC.AllocateUninitializedArray<byte>((int)fetch->TotalBytes);
            if (fetch->Data != nint.Zero)
                Marshal.Copy(fetch->Data, response.Body, 0, response.Body.Length);
            var headersLength = Emscripten.FetchGetResponseHeadersLength(fetch);
            headersBytes = ArrayPool<byte>.Shared.Rent((int)headersLength + 1);
            fixed (byte* headersBuffer = headersBytes)
            {
                Emscripten.FetchGetResponseHeaders(fetch, headersBuffer, headersLength + 1);
            }

            var headersText = Encoding.UTF8.GetString(headersBytes, 0, (int)headersLength);
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
            if (headersBytes is not null)
                ArrayPool<byte>.Shared.Return(headersBytes);
            Emscripten.FetchClose(fetch);
            if (_requests.Remove(id, out var request))
            {
                response.Request = request;
                Http.CompleteFetch(response);
            }
        }
    }
}
