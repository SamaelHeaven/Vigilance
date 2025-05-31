using System.Runtime.InteropServices;
using System.Text.Json;

namespace Vigilance.Core;

public static partial class JSEngine
{
    public static JSResult Eval(string script)
    {
        if (!Platform.Web.IsCurrent())
            throw new PlatformNotSupportedException();
        var ptr = emscripten_run_script_string(script);
        return new JSResult { Value = Marshal.PtrToStringUTF8(ptr) ?? "" };
    }

    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint emscripten_run_script_string(string script);
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

public static class StringExtensions
{
    public static string ToJson(this string str)
    {
        return $"\"{JsonEncodedText.Encode(str)}\"";
    }
}
