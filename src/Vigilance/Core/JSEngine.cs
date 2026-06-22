using JetBrains.Annotations;

namespace Vigilance.Core;

public static unsafe class JSEngine
{
    [MustUseReturnValue]
    public static JSResult Eval([LanguageInjection(InjectedLanguage.JAVASCRIPT)] string script)
    {
        if (!Platform.Web.IsCurrent)
            throw new PlatformNotSupportedException();
        var result = Emscripten.RunScriptString(script);
        return new JSResult(Utf8Ptr.GetString(result));
    }

    [MustUseReturnValue]
    public static JSResult Eval([LanguageInjection(InjectedLanguage.JAVASCRIPT)] ReadOnlySpan<byte> script)
    {
        if (!Platform.Web.IsCurrent)
            throw new PlatformNotSupportedException();
        fixed (byte* ptr = script)
        {
            var result = Emscripten.RunScriptString(ptr);
            return new JSResult(Utf8Ptr.GetString(result));
        }
    }

    public static void Run([LanguageInjection(InjectedLanguage.JAVASCRIPT)] string script)
    {
        if (!Platform.Web.IsCurrent)
            throw new PlatformNotSupportedException();
        Emscripten.RunScript(script);
    }

    public static void Run([LanguageInjection(InjectedLanguage.JAVASCRIPT)] ReadOnlySpan<byte> script)
    {
        if (!Platform.Web.IsCurrent)
            throw new PlatformNotSupportedException();
        fixed (byte* ptr = script)
        {
            Emscripten.RunScript(ptr);
        }
    }
}

public readonly record struct JSResult(string Value)
{
    public static implicit operator string(JSResult result)
    {
        return result.Value;
    }

    public static implicit operator char(JSResult result)
    {
        return result.Value.IsEmpty ? '\0' : result.Value[0];
    }

    public static implicit operator bool(JSResult result)
    {
        return bool.TryParse(result, out var value) ? value : !result.Value.IsEmpty;
    }

    public static implicit operator sbyte(JSResult result)
    {
        return (sbyte)(double)result;
    }

    public static implicit operator short(JSResult result)
    {
        return (short)(double)result;
    }

    public static implicit operator int(JSResult result)
    {
        return (int)(double)result;
    }

    public static implicit operator long(JSResult result)
    {
        return (long)(double)result;
    }

    public static implicit operator byte(JSResult result)
    {
        return (byte)(double)result;
    }

    public static implicit operator ushort(JSResult result)
    {
        return (ushort)(double)result;
    }

    public static implicit operator uint(JSResult result)
    {
        return (uint)(double)result;
    }

    public static implicit operator ulong(JSResult result)
    {
        return (ulong)(double)result;
    }

    public static implicit operator float(JSResult result)
    {
        return (float)(double)result;
    }

    public static implicit operator double(JSResult result)
    {
        return double.TryParse(result.Value, out var value) ? value : 0;
    }

    public static implicit operator decimal(JSResult result)
    {
        return (decimal)(double)result;
    }

    public override string ToString()
    {
        return Value;
    }
}
