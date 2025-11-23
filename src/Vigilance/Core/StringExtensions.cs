using System.Text.Json;

namespace Vigilance.Core;

public static class StringExtensions
{
    extension(string str)
    {
        public bool IsEmpty => str.Length == 0;

        public bool IsNotEmpty => str.Length != 0;

        public string ToJson()
        {
            return $"\"{JsonEncodedText.Encode(str)}\"";
        }

        public Utf8Buffer ToUtf8Buffer()
        {
            return new Utf8Buffer(str);
        }
    }
}
