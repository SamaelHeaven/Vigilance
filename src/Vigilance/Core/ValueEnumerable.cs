using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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

public interface IEnumerableList<TValue> : IValueEnumerable<List<TValue>.Enumerator, TValue>;

public interface IEnumerableDictionary<TKey, TValue>
    : IValueEnumerable<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull;

public interface IEnumerableSortedDictionary<TKey, TValue>
    : IValueEnumerable<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull;

public interface IEnumerableHashSet<TValue> : IValueEnumerable<HashSet<TValue>.Enumerator, TValue>;

public interface IEnumerableSortedSet<TValue> : IValueEnumerable<SortedSet<TValue>.Enumerator, TValue>;

public interface IEnumerableLinkedList<TValue> : IValueEnumerable<LinkedList<TValue>.Enumerator, TValue>;

public interface IEnumerableQueue<TValue> : IValueEnumerable<Queue<TValue>.Enumerator, TValue>;

public interface IEnumerableStack<TValue> : IValueEnumerable<Stack<TValue>.Enumerator, TValue>;

public interface IEnumerableArray<TValue> : IValueEnumerable<ArrayEnumerator<TValue>, TValue>;

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

    public ReadOnlySpan<TValue> AsSpan()
    {
        return CollectionsMarshal.AsSpan(list);
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

    public Dictionary<TKey, TValue>.KeyCollection Keys => dictionary.Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => dictionary.Keys;

    public Dictionary<TKey, TValue>.ValueCollection Values => dictionary.Values;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => dictionary.Values;

    public static implicit operator EnumerableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new EnumerableDictionary<TKey, TValue>(dictionary);
    }
}

public readonly struct EnumerableSortedDictionary<TKey, TValue>(SortedDictionary<TKey, TValue> sortedDictionary)
    : IEnumerableSortedDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    public SortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return sortedDictionary.GetEnumerator();
    }

    public int Count => sortedDictionary.Count;

    public bool ContainsKey(TKey key)
    {
        return sortedDictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return sortedDictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] => sortedDictionary[key];

    public SortedDictionary<TKey, TValue>.KeyCollection Keys => sortedDictionary.Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => sortedDictionary.Keys;

    public SortedDictionary<TKey, TValue>.ValueCollection Values => sortedDictionary.Values;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => sortedDictionary.Values;

    public static implicit operator EnumerableSortedDictionary<TKey, TValue>(SortedDictionary<TKey, TValue> dictionary)
    {
        return new EnumerableSortedDictionary<TKey, TValue>(dictionary);
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

public readonly struct EnumerableSortedSet<TValue>(SortedSet<TValue> sortedSet)
    : IEnumerableSortedSet<TValue>,
        IReadOnlySet<TValue>
{
    public SortedSet<TValue>.Enumerator GetEnumerator()
    {
        return sortedSet.GetEnumerator();
    }

    public int Count => sortedSet.Count;

    public bool Contains(TValue item)
    {
        return sortedSet.Contains(item);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return sortedSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return sortedSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return sortedSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return sortedSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return sortedSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return sortedSet.SetEquals(other);
    }

    public static implicit operator EnumerableSortedSet<TValue>(SortedSet<TValue> sortedSet)
    {
        return new EnumerableSortedSet<TValue>(sortedSet);
    }
}

public readonly struct EnumerableLinkedList<TValue>(LinkedList<TValue> linkedList)
    : IEnumerableLinkedList<TValue>,
        IReadOnlyCollection<TValue>
{
    public LinkedList<TValue>.Enumerator GetEnumerator()
    {
        return linkedList.GetEnumerator();
    }

    public int Count => linkedList.Count;

    public static implicit operator EnumerableLinkedList<TValue>(LinkedList<TValue> linkedList)
    {
        return new EnumerableLinkedList<TValue>(linkedList);
    }
}

public readonly struct EnumerableQueue<TValue>(Queue<TValue> queue)
    : IEnumerableQueue<TValue>,
        IReadOnlyCollection<TValue>
{
    public Queue<TValue>.Enumerator GetEnumerator()
    {
        return queue.GetEnumerator();
    }

    public int Count => queue.Count;

    public static implicit operator EnumerableQueue<TValue>(Queue<TValue> queue)
    {
        return new EnumerableQueue<TValue>(queue);
    }
}

public readonly struct EnumerableStack<TValue>(Stack<TValue> stack)
    : IEnumerableStack<TValue>,
        IReadOnlyCollection<TValue>
{
    public Stack<TValue>.Enumerator GetEnumerator()
    {
        return stack.GetEnumerator();
    }

    public int Count => stack.Count;

    public static implicit operator EnumerableStack<TValue>(Stack<TValue> stack)
    {
        return new EnumerableStack<TValue>(stack);
    }
}

public readonly struct EnumerableArray<TValue>(TValue[] array) : IEnumerableArray<TValue>, IReadOnlyList<TValue>
{
    private readonly TValue[] _array = array;

    public ArrayEnumerator<TValue> GetEnumerator()
    {
        return new ArrayEnumerator<TValue>(_array);
    }

    public int Count => _array.Length;

    public TValue this[int index] => _array[index];

    public static implicit operator EnumerableArray<TValue>(TValue[] array)
    {
        return new EnumerableArray<TValue>(array);
    }

    public static implicit operator ReadOnlySpan<TValue>(EnumerableArray<TValue> array)
    {
        return array._array;
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

    public readonly TValue Current => _array[_index];

    public void Dispose() { }
}
