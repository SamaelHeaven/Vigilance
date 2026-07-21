using Vigilance.Collections;

// ReSharper disable StaticMemberInGenericType

namespace Vigilance.Core;

public static class EnumExtensions<T>
    where T : struct, Enum
{
    private static T[]? _values;
    private static string[]? _names;
    private static ValueDictionary<string, T> _valuesByName;
    private static bool _hasValuesByName;
    private static ValueDictionary<T, string> _namesByValue;
    private static bool _hasNamesByValue;

    public static ArrayView<T> Values()
    {
        if (_values is not null)
            return _values;
        var values = Enum.GetValues<T>();
        _values = values;
        return values;
    }

    public static ArrayView<string> Names()
    {
        if (_names is not null)
            return _names;
        var names = Enum.GetNames<T>();
        _names = names;
        return _names;
    }

    public static ValueDictionaryView<string, T>.Enumerable ValuesByName()
    {
        if (Volatile.Read(ref _hasValuesByName))
            return _valuesByName.AsView().AsEnumerable();
        var values = Values();
        var names = Names();
        var valuesByName = new ValueDictionary<string, T>(values.Count);
        for (var i = 0; i < values.Count; i++)
            valuesByName.Add(names[i], values[i]);
        _valuesByName = valuesByName;
        Volatile.Write(ref _hasValuesByName, true);
        return valuesByName.AsView().AsEnumerable();
    }

    public static ValueDictionaryView<T, string>.Enumerable NamesByValue()
    {
        if (Volatile.Read(ref _hasNamesByValue))
            return _namesByValue.AsView().AsEnumerable();
        var values = Values();
        var names = Names();
        var namesByValue = new ValueDictionary<T, string>(values.Count);
        for (var i = 0; i < values.Count; i++)
            namesByValue.Add(values[i], names[i]);
        _namesByValue = namesByValue;
        Volatile.Write(ref _hasNamesByValue, true);
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
