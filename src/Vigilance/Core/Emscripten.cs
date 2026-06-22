using System.Runtime.InteropServices;

namespace Vigilance.Core;

internal static unsafe partial class Emscripten
{
    public const string LibraryName = "libc";

    [LibraryImport(
        LibraryName,
        StringMarshalling = StringMarshalling.Utf8,
        EntryPoint = "emscripten_run_script_string"
    )]
    public static partial nint RunScriptString(string script);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_run_script_string")]
    public static partial nint RunScriptString(byte* script);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_set_main_loop")]
    public static partial void SetMainLoop(delegate* unmanaged[Cdecl]<void> func, int fps, sbyte simulateInfiniteLoop);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_fetch_attr_init")]
    public static partial void FetchAttrInit(ref EmscriptenFetchAttr attr);

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "emscripten_fetch")]
    public static partial void Fetch(ref EmscriptenFetchAttr attr, string url);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_fetch_close")]
    public static partial void FetchClose(EmscriptenFetch* fetch);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_fetch_get_response_headers_length")]
    public static partial nuint FetchGetResponseHeadersLength(EmscriptenFetch* fetch);

    [LibraryImport(LibraryName, EntryPoint = "emscripten_fetch_get_response_headers")]
    public static partial void FetchGetResponseHeaders(EmscriptenFetch* fetch, byte* dst, nuint dstSizeBytes);
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EmscriptenFetchAttr
{
    public const int RequestMethodSize = 32;
    public fixed byte RequestMethod[RequestMethodSize];
    public nint UserData;
    public delegate* unmanaged[Cdecl]<EmscriptenFetch*, void> OnSuccess;
    public delegate* unmanaged[Cdecl]<EmscriptenFetch*, void> OnError;
    public delegate* unmanaged[Cdecl]<EmscriptenFetch*, void> OnProgress;
    public delegate* unmanaged[Cdecl]<EmscriptenFetch*, void> OnReadyStateChange;
    public uint Attributes;
    public uint TimeoutMSecs;
    public sbyte WithCredentials;
    public nint DestinationPath;
    public nint UserName;
    public nint Password;
    public nint RequestHeaders;
    public nint OverriddenMimeType;
    public nint RequestData;
    public uint RequestHeadersLength;
    public nuint RequestDataSize;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EmscriptenFetch
{
    public const int StatusTextSize = 64;
    public uint Id;
    public nint UserData;
    public nint Url;
    public nint Data;
    public ulong NumBytes;
    public ulong DataOffset;
    public ulong TotalBytes;
    public ushort ReadyState;
    public ushort Status;
    public fixed byte StatusText[StatusTextSize];
    public EmscriptenFetchAttr Attributes;
    public nint ResponseUrl;
}
