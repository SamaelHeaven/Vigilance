using System.Diagnostics.CodeAnalysis;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Core;

public interface IListView<TValue> : IStructEnumerable<List<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<List<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        List<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<List<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromList<TValue>, TValue> AsValueEnumerable();
}

public interface IDictionaryView<TKey, TValue>
    : IStructEnumerable<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    ValueEnumerable<
        StructEnumerator<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    new ValueEnumerable<FromDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable();
}

public interface ISortedDictionaryView<TKey, TValue>
    : IStructEnumerable<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    ValueEnumerable<
        StructEnumerator<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
    }

    new ValueEnumerable<FromSortedDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable();
}

public interface IHashSetView<TValue> : IStructEnumerable<HashSet<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<HashSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        HashSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<HashSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromHashSet<TValue>, TValue> AsValueEnumerable();
}

public interface ISortedSetView<TValue> : IStructEnumerable<SortedSet<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<SortedSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        SortedSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<SortedSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromSortedSet<TValue>, TValue> AsValueEnumerable();
}

public interface ILinkedListView<TValue> : IStructEnumerable<LinkedList<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<LinkedList<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        LinkedList<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<LinkedList<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromLinkedList<TValue>, TValue> AsValueEnumerable();
}

public interface IQueueView<TValue> : IStructEnumerable<Queue<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<Queue<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        Queue<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Queue<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromQueue<TValue>, TValue> AsValueEnumerable();
}

public interface IStackView<TValue> : IStructEnumerable<Stack<TValue>.Enumerator, TValue>
{
    ValueEnumerable<StructEnumerator<Stack<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        Stack<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Stack<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromStack<TValue>, TValue> AsValueEnumerable();
}

public interface IArrayView<TValue> : IStructEnumerable<ArrayEnumerator<TValue>, TValue>
{
    ValueEnumerable<StructEnumerator<ArrayEnumerator<TValue>, TValue>, TValue> IStructEnumerable<
        ArrayEnumerator<TValue>,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ArrayEnumerator<TValue>, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromArray<TValue>, TValue> AsValueEnumerable();
}

public readonly struct ListView<TValue>(List<TValue> list)
    : IListView<TValue>,
        IReadOnlyList<TValue>,
        IReadOnlySpan<TValue>
{
    public List<TValue>.Enumerator GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public ValueEnumerable<FromList<TValue>, TValue> AsValueEnumerable()
    {
        return list.AsValueEnumerable();
    }

    public int Count => list.Count;

    public TValue this[int index] => list[index];

    public static implicit operator ListView<TValue>(List<TValue> list)
    {
        return new ListView<TValue>(list);
    }

    public ReadOnlySpan<TValue> AsSpan()
    {
        return list.AsSpan();
    }
}

public readonly struct DictionaryView<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    : IDictionaryView<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    public ValueEnumerable<FromDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return dictionary.AsValueEnumerable();
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

    public static implicit operator DictionaryView<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new DictionaryView<TKey, TValue>(dictionary);
    }
}

public readonly struct SortedDictionaryView<TKey, TValue>(SortedDictionary<TKey, TValue> sortedDictionary)
    : ISortedDictionaryView<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    public SortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return sortedDictionary.GetEnumerator();
    }

    public ValueEnumerable<FromSortedDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return sortedDictionary.AsValueEnumerable();
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

    public static implicit operator SortedDictionaryView<TKey, TValue>(SortedDictionary<TKey, TValue> dictionary)
    {
        return new SortedDictionaryView<TKey, TValue>(dictionary);
    }
}

public readonly struct HashSetView<TValue>(HashSet<TValue> hashSet) : IHashSetView<TValue>, IReadOnlySet<TValue>
{
    public HashSet<TValue>.Enumerator GetEnumerator()
    {
        return hashSet.GetEnumerator();
    }

    public ValueEnumerable<FromHashSet<TValue>, TValue> AsValueEnumerable()
    {
        return hashSet.AsValueEnumerable();
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

    public static implicit operator HashSetView<TValue>(HashSet<TValue> hashSet)
    {
        return new HashSetView<TValue>(hashSet);
    }
}

public readonly struct SortedSetView<TValue>(SortedSet<TValue> sortedSet) : ISortedSetView<TValue>, IReadOnlySet<TValue>
{
    public SortedSet<TValue>.Enumerator GetEnumerator()
    {
        return sortedSet.GetEnumerator();
    }

    public ValueEnumerable<FromSortedSet<TValue>, TValue> AsValueEnumerable()
    {
        return sortedSet.AsValueEnumerable();
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

    public static implicit operator SortedSetView<TValue>(SortedSet<TValue> sortedSet)
    {
        return new SortedSetView<TValue>(sortedSet);
    }
}

public readonly struct LinkedListView<TValue>(LinkedList<TValue> linkedList)
    : ILinkedListView<TValue>,
        IReadOnlyCollection<TValue>
{
    public LinkedList<TValue>.Enumerator GetEnumerator()
    {
        return linkedList.GetEnumerator();
    }

    public ValueEnumerable<FromLinkedList<TValue>, TValue> AsValueEnumerable()
    {
        return linkedList.AsValueEnumerable();
    }

    public int Count => linkedList.Count;

    public static implicit operator LinkedListView<TValue>(LinkedList<TValue> linkedList)
    {
        return new LinkedListView<TValue>(linkedList);
    }
}

public readonly struct QueueView<TValue>(Queue<TValue> queue) : IQueueView<TValue>, IReadOnlyCollection<TValue>
{
    public Queue<TValue>.Enumerator GetEnumerator()
    {
        return queue.GetEnumerator();
    }

    public ValueEnumerable<FromQueue<TValue>, TValue> AsValueEnumerable()
    {
        return queue.AsValueEnumerable();
    }

    public int Count => queue.Count;

    public static implicit operator QueueView<TValue>(Queue<TValue> queue)
    {
        return new QueueView<TValue>(queue);
    }
}

public readonly struct StackView<TValue>(Stack<TValue> stack) : IStackView<TValue>, IReadOnlyCollection<TValue>
{
    public Stack<TValue>.Enumerator GetEnumerator()
    {
        return stack.GetEnumerator();
    }

    public ValueEnumerable<FromStack<TValue>, TValue> AsValueEnumerable()
    {
        return stack.AsValueEnumerable();
    }

    public int Count => stack.Count;

    public static implicit operator StackView<TValue>(Stack<TValue> stack)
    {
        return new StackView<TValue>(stack);
    }
}

public readonly struct ArrayView<TValue>(TValue[] array)
    : IArrayView<TValue>,
        IReadOnlyList<TValue>,
        IReadOnlySpan<TValue>
{
    private readonly TValue[] _array = array;

    public ArrayEnumerator<TValue> GetEnumerator()
    {
        return new ArrayEnumerator<TValue>(_array);
    }

    public ValueEnumerable<FromArray<TValue>, TValue> AsValueEnumerable()
    {
        return _array.AsValueEnumerable();
    }

    public int Count => _array.Length;

    public TValue this[int index] => _array[index];

    public static implicit operator ArrayView<TValue>(TValue[] array)
    {
        return new ArrayView<TValue>(array);
    }

    public static implicit operator ReadOnlySpan<TValue>(ArrayView<TValue> arrayView)
    {
        return arrayView._array;
    }

    public ReadOnlySpan<TValue> AsSpan()
    {
        return _array;
    }
}

public struct ArrayEnumerator<TValue> : IStructEnumerator<TValue>
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
