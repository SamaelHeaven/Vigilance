using Vigilance.Collections;

// ReSharper disable StaticMemberInGenericType

namespace Vigilance.Core;

public static class EnumExtensions<T>
    where T : struct, Enum
{
    private static readonly T[] _values;
    private static readonly string[] _names;
    private static readonly ValueDictionary<string, T> _valuesByName;
    private static readonly ValueDictionary<T, string> _namesByValue;

    static EnumExtensions()
    {
        _values = Enum.GetValues<T>();
        _names = Enum.GetNames<T>();
        _valuesByName = new ValueDictionary<string, T>(_values.Length);
        _namesByValue = new ValueDictionary<T, string>(_values.Length);
        for (var i = 0; i < _values.Length; i++)
        {
            _valuesByName.Add(_names[i], _values[i]);
            _namesByValue.Add(_values[i], _names[i]);
        }
    }

    public static ArrayView<T> Values()
    {
        return _values;
    }

    public static ArrayView<string> Names()
    {
        return _names;
    }

    public static ValueDictionaryView<string, T>.Enumerable ValuesByName()
    {
        return _valuesByName.AsView().AsEnumerable();
    }

    public static ValueDictionaryView<T, string>.Enumerable NamesByValue()
    {
        return _namesByValue.AsView().AsEnumerable();
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
