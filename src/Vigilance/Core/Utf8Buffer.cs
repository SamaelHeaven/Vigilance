using System.Runtime.InteropServices;

namespace Vigilance.Core;

public readonly ref struct Utf8Buffer
{
    private readonly IntPtr _data;

    public Utf8Buffer(string text)
    {
        _data = Marshal.StringToCoTaskMemUTF8(text);
    }

    public unsafe sbyte* AsPointer()
    {
        return (sbyte*)_data.ToPointer();
    }

    public void Dispose()
    {
        Marshal.ZeroFreeCoTaskMemUTF8(_data);
    }
}
