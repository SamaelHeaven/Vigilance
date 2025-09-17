using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Vigilance.Core;

public interface IValueEnumerable<out TEnumerator, out TValue> : IEnumerable<TValue>
    where TEnumerator : IEnumerator<TValue>
{
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    new TEnumerator GetEnumerator();
}

public interface IValueEnumerator<out TValue> : IEnumerator<TValue>
{
    new TValue Current { get; }

    object? IEnumerator.Current => Current;
}

public interface IEnumerableList<TValue> : IValueEnumerable<List<TValue>.Enumerator, TValue> { }

public interface IEnumerableDictionary<TKey, TValue>
    : IValueEnumerable<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull { }

public interface IEnumerableHashSet<TValue> : IValueEnumerable<HashSet<TValue>.Enumerator, TValue> { }

public interface IEnumerableArray<TValue> : IValueEnumerable<ArrayEnumerator<TValue>, TValue> { }

public readonly struct EnumerableList<TValue>(List<TValue> list) : IEnumerableList<TValue>
{
    public List<TValue>.Enumerator GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public int Count => list.Count;

    public TValue this[int index] => list[index];

    public static implicit operator EnumerableList<TValue>(List<TValue> list)
    {
        return new EnumerableList<TValue>(list);
    }
}

public readonly struct EnumerableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    : IEnumerableDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    public int Count => dictionary.Count;

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] => dictionary[key];

    public IEnumerable<TKey> Keys => dictionary.Keys;

    public IEnumerable<TValue> Values => dictionary.Values;

    public static implicit operator EnumerableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new EnumerableDictionary<TKey, TValue>(dictionary);
    }
}

public readonly struct EnumerableHashSet<TValue>(HashSet<TValue> hashSet)
    : IEnumerableHashSet<TValue>,
        IReadOnlySet<TValue>
{
    public HashSet<TValue>.Enumerator GetEnumerator()
    {
        return hashSet.GetEnumerator();
    }

    public int Count => hashSet.Count;

    public bool Contains(TValue item)
    {
        return hashSet.Contains(item);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return hashSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return hashSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return hashSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return hashSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return hashSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return hashSet.SetEquals(other);
    }

    public static implicit operator EnumerableHashSet<TValue>(HashSet<TValue> hashSet)
    {
        return new EnumerableHashSet<TValue>(hashSet);
    }
}

public readonly struct EnumerableArray<TValue>(TValue[] array) : IEnumerableArray<TValue>, IReadOnlyList<TValue>
{
    public ArrayEnumerator<TValue> GetEnumerator()
    {
        return new ArrayEnumerator<TValue>(array);
    }

    public int Count => array.Length;

    public TValue this[int index] => array[index];

    public static implicit operator EnumerableArray<TValue>(TValue[] array)
    {
        return new EnumerableArray<TValue>(array);
    }
}

public struct ArrayEnumerator<TValue> : IValueEnumerator<TValue>
{
    private readonly TValue[] _array;
    private int _index;

    internal ArrayEnumerator(TValue[] array)
    {
        _array = array;
        Reset();
    }

    public bool MoveNext()
    {
        return ++_index < _array.Length;
    }

    public void Reset()
    {
        _index = -1;
    }

    public TValue Current => _array[_index];

    public void Dispose() { }
}
