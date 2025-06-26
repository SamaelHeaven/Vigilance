using System.Text.Json;

namespace Vigilance.Core;

public static class StringExtensions
{
    public static string ToJson(this string str)
    {
        return $"\"{JsonEncodedText.Encode(str)}\"";
    }

    public static Utf8Buffer ToUtf8Buffer(this string str)
    {
        return new Utf8Buffer(str);
    }
}
