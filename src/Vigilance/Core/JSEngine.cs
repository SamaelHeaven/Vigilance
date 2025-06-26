using System.Runtime.InteropServices;

namespace Vigilance.Core;

public static class JSEngine
{
    public static JSResult Eval(string script)
    {
        if (!Platform.Web.IsCurrent())
            throw new PlatformNotSupportedException();
        var ptr = Emscripten.RunScriptString(script);
        return new JSResult { Value = Marshal.PtrToStringUTF8(ptr) ?? "" };
    }
}

public readonly struct JSResult
{
    public string Value { get; init; }

    public static implicit operator string(JSResult result)
    {
        return result.Value;
    }

    public static implicit operator char(JSResult result)
    {
        return char.Parse(result);
    }

    public static implicit operator bool(JSResult result)
    {
        return bool.Parse(result);
    }

    public static implicit operator sbyte(JSResult result)
    {
        return sbyte.Parse(result);
    }

    public static implicit operator short(JSResult result)
    {
        return short.Parse(result);
    }

    public static implicit operator int(JSResult result)
    {
        return int.Parse(result);
    }

    public static implicit operator long(JSResult result)
    {
        return long.Parse(result);
    }

    public static implicit operator byte(JSResult result)
    {
        return byte.Parse(result);
    }

    public static implicit operator ushort(JSResult result)
    {
        return ushort.Parse(result);
    }

    public static implicit operator uint(JSResult result)
    {
        return uint.Parse(result);
    }

    public static implicit operator ulong(JSResult result)
    {
        return ulong.Parse(result);
    }

    public static implicit operator float(JSResult result)
    {
        return float.Parse(result);
    }

    public static implicit operator double(JSResult result)
    {
        return double.Parse(result);
    }

    public static implicit operator decimal(JSResult result)
    {
        return decimal.Parse(result);
    }

    public override string ToString()
    {
        return Value;
    }
}
