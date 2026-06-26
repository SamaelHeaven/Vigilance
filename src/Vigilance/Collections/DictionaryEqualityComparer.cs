using System.Reflection;
using Vigilance.Core;

namespace Vigilance.Collections;

internal static class DictionaryEqualityComparer
{
    private static IEqualityComparer<string>? DefaultComparer =>
        field ??= GetDictionaryComparer(new Dictionary<string, bool>(EqualityComparer<string?>.Default));

    private static IEqualityComparer<string>? StringComparerOrdinal =>
        field ??= GetDictionaryComparer(new Dictionary<string, bool>(StringComparer.Ordinal));

    private static IEqualityComparer<string>? StringComparerOrdinalIgnoreCase =>
        field ??= GetDictionaryComparer(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));

    private static Wrapper<FieldInfo?>? ComparerFieldInfo
    {
        get
        {
            if (field.HasValue)
                return field.Value;
            var type = typeof(Dictionary<string, bool>);
            return field = type.GetField("_comparer", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    private static IEqualityComparer<string>? GetDictionaryComparer(Dictionary<string, bool> dictionary)
    {
        return (IEqualityComparer<string>?)ComparerFieldInfo?.Value?.GetValue(dictionary);
    }

    public static IEqualityComparer<string>? GetStringComparer(object comparer)
    {
        if (ReferenceEquals(comparer, EqualityComparer<string>.Default))
            return DefaultComparer;
        if (ReferenceEquals(comparer, StringComparer.Ordinal))
            return StringComparerOrdinal;
        return ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase) ? StringComparerOrdinalIgnoreCase : null;
    }
}
