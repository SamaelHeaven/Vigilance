using Vigilance.Collections;

// ReSharper disable StaticMemberInGenericType

namespace Vigilance.Core;

public static class EnumExtensions<T>
    where T : struct, Enum
{
    private static T[]? _values;
    private static string[]? _names;
    private static ValueDictionary<string, T> _valuesByName;
    private static volatile bool _hasValuesByName;
    private static ValueDictionary<T, string> _namesByValue;
    private static volatile bool _hasNamesByValue;

    public static ArrayView<T> Values()
    {
        return _values ??= Enum.GetValues<T>();
    }

    public static ArrayView<string> Names()
    {
        return _names ??= Enum.GetNames<T>();
    }

    public static ValueDictionaryView<string, T>.Enumerable ValuesByName()
    {
        if (_hasValuesByName)
            return _valuesByName.AsView().AsEnumerable();
        var values = Values();
        var names = Names();
        var valuesByName = new ValueDictionary<string, T>(values.Count);
        for (var i = 0; i < values.Count; i++)
            valuesByName.Add(names[i], values[i]);
        _valuesByName = valuesByName;
        _hasValuesByName = true;
        return valuesByName.AsView().AsEnumerable();
    }

    public static ValueDictionaryView<T, string>.Enumerable NamesByValue()
    {
        if (_hasNamesByValue)
            return _namesByValue.AsView().AsEnumerable();
        var values = Values();
        var names = Names();
        var namesByValue = new ValueDictionary<T, string>(values.Count);
        for (var i = 0; i < values.Count; i++)
            namesByValue.Add(values[i], names[i]);
        _namesByValue = namesByValue;
        _hasNamesByValue = true;
        return namesByValue.AsView().AsEnumerable();
    }
}

public static class EnumExtensions
{
    extension<T>(T)
        where T : struct, Enum
    {
        public static ArrayView<T> Values()
        {
            return EnumExtensions<T>.Values();
        }

        public static ArrayView<string> Names()
        {
            return EnumExtensions<T>.Names();
        }

        public static ValueDictionaryView<string, T>.Enumerable ValuesByName()
        {
            return EnumExtensions<T>.ValuesByName();
        }

        public static ValueDictionaryView<T, string>.Enumerable NamesByValue()
        {
            return EnumExtensions<T>.NamesByValue();
        }
    }
}
