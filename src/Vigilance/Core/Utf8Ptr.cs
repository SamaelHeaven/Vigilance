using System.Runtime.InteropServices;
using System.Text;

namespace Vigilance.Core;

public readonly unsafe ref struct Utf8Ptr : IDisposable
{
    private readonly nint _data;

    public Utf8Ptr(string str)
    {
        _data = Marshal.StringToCoTaskMemUTF8(str);
    }

    public Utf8Ptr(scoped in ReadOnlySpan<char> str)
    {
        var byteCount = Encoding.UTF8.GetByteCount(str);
        var ptr = (byte*)Marshal.AllocCoTaskMem(byteCount + 1);
        var written = Encoding.UTF8.GetBytes(str, new Span<byte>(ptr, byteCount));
        ptr[written] = 0;
        _data = (nint)ptr;
    }

    public void Dispose()
    {
        Marshal.ZeroFreeCoTaskMemUTF8(_data);
    }

    public static implicit operator nint(Utf8Ptr ptr)
    {
        return ptr._data;
    }

    public static implicit operator byte*(Utf8Ptr ptr)
    {
        return (byte*)ptr._data;
    }

    public static implicit operator sbyte*(Utf8Ptr ptr)
    {
        return (sbyte*)ptr._data;
    }

    public static string GetString(nint ptr, string defaultValue = "")
    {
        return Marshal.PtrToStringUTF8(ptr) ?? defaultValue;
    }

    public static string GetString(byte* ptr, string defaultValue = "")
    {
        return Marshal.PtrToStringUTF8((nint)ptr) ?? defaultValue;
    }

    public static string GetString(sbyte* ptr, string defaultValue = "")
    {
        return Marshal.PtrToStringUTF8((nint)ptr) ?? defaultValue;
    }
}
