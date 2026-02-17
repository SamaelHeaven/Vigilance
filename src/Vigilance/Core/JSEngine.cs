namespace Vigilance.Core;

public static class JSEngine
{
    public static JSResult Eval(string script)
    {
        if (!Platform.Web.IsCurrent)
            throw new PlatformNotSupportedException();
        var ptr = Emscripten.RunScriptString(script);
        return new JSResult { Value = Utf8Buffer.GetString(ptr) };
    }
}

public readonly record struct JSResult
{
    public string Value { get; init; }

    public static implicit operator string(JSResult result)
    {
        return result.Value;
    }

    public static implicit operator char(JSResult result)
    {
        return char.TryParse(result, out var value) ? value : '\0';
    }

    public static implicit operator bool(JSResult result)
    {
        return bool.TryParse(result, out var value) && value;
    }

    public static implicit operator sbyte(JSResult result)
    {
        return sbyte.TryParse(result.Value, out var value) ? value : (sbyte)0;
    }

    public static implicit operator short(JSResult result)
    {
        return short.TryParse(result.Value, out var value) ? value : (short)0;
    }

    public static implicit operator int(JSResult result)
    {
        return int.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator long(JSResult result)
    {
        return long.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator byte(JSResult result)
    {
        return byte.TryParse(result.Value, out var value) ? value : (byte)0;
    }

    public static implicit operator ushort(JSResult result)
    {
        return ushort.TryParse(result.Value, out var value) ? value : (ushort)0;
    }

    public static implicit operator uint(JSResult result)
    {
        return uint.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator ulong(JSResult result)
    {
        return ulong.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator float(JSResult result)
    {
        return float.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator double(JSResult result)
    {
        return double.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator decimal(JSResult result)
    {
        return decimal.TryParse(result.Value, out var value) ? value : 0;
    }

    public override string ToString()
    {
        return Value;
    }
}
