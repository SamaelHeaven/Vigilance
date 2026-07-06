using System.Runtime.InteropServices;

namespace Vigilance.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Wrapper<T>(T Value)
{
    public static implicit operator Wrapper<T>(T t)
    {
        return new Wrapper<T>(t);
    }

    public static implicit operator T(Wrapper<T> source)
    {
        return source.Value;
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}
