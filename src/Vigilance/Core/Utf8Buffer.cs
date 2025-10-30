using System.Runtime.InteropServices;

namespace Vigilance.Core;

public readonly unsafe ref struct Utf8Buffer
{
    private readonly nint _data;

    public Utf8Buffer(string str)
    {
        _data = Marshal.StringToCoTaskMemUTF8(str);
    }

    public void Dispose()
    {
        Marshal.ZeroFreeCoTaskMemUTF8(_data);
    }

    public static implicit operator nint(Utf8Buffer buffer)
    {
        return buffer._data;
    }

    public static implicit operator byte*(Utf8Buffer buffer)
    {
        return (byte*)buffer._data;
    }

    public static implicit operator sbyte*(Utf8Buffer buffer)
    {
        return (sbyte*)buffer._data;
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
