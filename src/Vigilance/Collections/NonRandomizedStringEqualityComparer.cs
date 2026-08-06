using System.Reflection;

namespace Vigilance.Collections;

public static class NonRandomizedStringEqualityComparer
{
    private static readonly FieldInfo? _comparerFieldInfo = typeof(Dictionary<string, byte>).GetField(
        "_comparer",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    private static IEqualityComparer<string>? DefaultComparer =>
        field ??= GetDictionaryComparer(new Dictionary<string, byte>(EqualityComparer<string?>.Default));

    private static IEqualityComparer<string>? StringComparerOrdinal =>
        field ??= GetDictionaryComparer(new Dictionary<string, byte>(StringComparer.Ordinal));

    private static IEqualityComparer<string>? StringComparerOrdinalIgnoreCase =>
        field ??= GetDictionaryComparer(new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase));

    private static IEqualityComparer<string>? GetDictionaryComparer(Dictionary<string, byte> dictionary)
    {
        return (IEqualityComparer<string>?)_comparerFieldInfo?.GetValue(dictionary);
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
