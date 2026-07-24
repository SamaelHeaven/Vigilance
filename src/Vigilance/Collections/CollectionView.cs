using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Collections;

public interface ISpanView<TValue>
    : IStructEnumerable<SpanViewEnumerator<TValue>, TValue>,
        IReadOnlyCollection<TValue>,
        IReadOnlySpan<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsSpan().Length;

    ValueEnumerable<StructEnumerator<SpanViewEnumerator<TValue>, TValue>, TValue> IStructEnumerable<
        SpanViewEnumerator<TValue>,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<SpanViewEnumerator<TValue>, TValue>(new SpanViewEnumerator<TValue>(this));
    }

    SpanViewEnumerator<TValue> IStructEnumerable<SpanViewEnumerator<TValue>, TValue>.GetEnumerator()
    {
        return new SpanViewEnumerator<TValue>(this);
    }

    new ValueEnumerable<FromSpan<TValue>, TValue> AsValueEnumerable();

    new ValueEnumerator<FromSpan<TValue>, TValue> GetEnumerator();
}

public interface IListView<TValue>
    : IStructEnumerable<List<TValue>.Enumerator, TValue>,
        IReadOnlyList<TValue>,
        IReadOnlySpan<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    TValue IReadOnlyList<TValue>.this[int index] => AsSpan()[index];

    ReadOnlySpan<TValue> IReadOnlySpan<TValue>.AsSpan()
    {
        using var enumerator = AsValueEnumerable().Enumerator;
        enumerator.TryGetSpan(out var span);
        return span;
    }

    ValueEnumerable<StructEnumerator<List<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        List<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<List<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromList<TValue>, TValue> AsValueEnumerable();
}

public interface IValueListView<TValue>
    : IStructEnumerable<ValueList<TValue>.Enumerator, TValue>,
        IReadOnlyList<TValue>,
        IReadOnlySpan<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    TValue IReadOnlyList<TValue>.this[int index] => AsSpan()[index];

    ReadOnlySpan<TValue> IReadOnlySpan<TValue>.AsSpan()
    {
        using var enumerator = GetEnumerator();
        enumerator.TryGetSpan(out var span);
        return span;
    }

    ValueEnumerable<StructEnumerator<ValueList<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueList<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueList<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<ValueList<TValue>.Enumerator, TValue> AsValueEnumerable();
}

public interface IDictionaryView<TKey, TValue>
    : IStructEnumerable<Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();

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
    : IStructEnumerable<SortedDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();

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

public interface IHashSetView<TValue>
    : IStructEnumerable<HashSet<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<HashSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        HashSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<HashSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromHashSet<TValue>, TValue> AsValueEnumerable();
}

public interface ISortedSetView<TValue>
    : IStructEnumerable<SortedSet<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<SortedSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        SortedSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<SortedSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromSortedSet<TValue>, TValue> AsValueEnumerable();
}

public interface ILinkedListView<TValue>
    : IStructEnumerable<LinkedList<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<LinkedList<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        LinkedList<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<LinkedList<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromLinkedList<TValue>, TValue> AsValueEnumerable();
}

public interface IQueueView<TValue> : IStructEnumerable<Queue<TValue>.Enumerator, TValue>, IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<Queue<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        Queue<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Queue<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromQueue<TValue>, TValue> AsValueEnumerable();
}

public interface IValueQueueView<TValue>
    : IStructEnumerable<ValueQueue<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<ValueQueue<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueQueue<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueQueue<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<ValueQueue<TValue>.Enumerator, TValue> AsValueEnumerable();
}

public interface IStackView<TValue> : IStructEnumerable<Stack<TValue>.Enumerator, TValue>, IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<Stack<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        Stack<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Stack<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromStack<TValue>, TValue> AsValueEnumerable();
}

public interface IValueStackView<TValue>
    : IStructEnumerable<ValueStack<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<ValueStack<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueStack<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueStack<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<ValueStack<TValue>.Enumerator, TValue> AsValueEnumerable();
}

public interface IArrayView<TValue>
    : IStructEnumerable<ArrayEnumerator<TValue>, TValue>,
        IReadOnlyList<TValue>,
        IReadOnlySpan<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    TValue IReadOnlyList<TValue>.this[int index] => AsSpan()[index];

    ReadOnlySpan<TValue> IReadOnlySpan<TValue>.AsSpan()
    {
        using var enumerator = AsValueEnumerable().Enumerator;
        enumerator.TryGetSpan(out var span);
        return span;
    }

    ValueEnumerable<StructEnumerator<ArrayEnumerator<TValue>, TValue>, TValue> IStructEnumerable<
        ArrayEnumerator<TValue>,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ArrayEnumerator<TValue>, TValue>(GetEnumerator());
    }

    new ValueEnumerable<FromArray<TValue>, TValue> AsValueEnumerable();
}

public interface ISparseSetView<TKey, TValue, TStorage>
    : IStructEnumerable<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();
}

public interface IEntitySparseSetView<TValue, TStorage>
    : IStructEnumerable<EntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>,
        IReadOnlyCollection<KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    int IReadOnlyCollection<KeyValuePair<Entity, TValue>>.Count => AsValueEnumerable().Count();
}

public interface IValueSparseSetView<TKey, TValue, TStorage>
    : IStructEnumerable<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();
}

public interface ISparseSetView<TKey> : IStructEnumerable<SparseSet<TKey>.Enumerator, TKey>, IReadOnlyCollection<TKey>
{
    int IReadOnlyCollection<TKey>.Count => AsValueEnumerable().Count();
}

public interface IEntitySparseSetView
    : IStructEnumerable<EntitySparseSet.Enumerator, Entity>,
        IReadOnlyCollection<Entity>
{
    int IReadOnlyCollection<Entity>.Count => AsValueEnumerable().Count();
}

public interface IValueSparseSetView<TKey>
    : IStructEnumerable<ValueSparseSet<TKey>.Enumerator, TKey>,
        IReadOnlyCollection<TKey>
{
    int IReadOnlyCollection<TKey>.Count => AsValueEnumerable().Count();
}

public interface IValueEntitySparseSetView
    : IStructEnumerable<ValueEntitySparseSet.Enumerator, Entity>,
        IReadOnlyCollection<Entity>
{
    int IReadOnlyCollection<Entity>.Count => AsValueEnumerable().Count();
}

public interface IValueEntitySparseSetView<TValue, TStorage>
    : IStructEnumerable<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>,
        IReadOnlyCollection<KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    int IReadOnlyCollection<KeyValuePair<Entity, TValue>>.Count => AsValueEnumerable().Count();
}

public interface IValueDictionaryView<TKey, TValue>
    : IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();

    ValueEnumerable<
        StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
    }

    new ValueEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable();
}

public interface IValueHashSetView<TValue>
    : IStructEnumerable<ValueHashSet<TValue>.Enumerator, TValue>,
        IReadOnlyCollection<TValue>
{
    int IReadOnlyCollection<TValue>.Count => AsValueEnumerable().Count();

    ValueEnumerable<StructEnumerator<ValueHashSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueHashSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueHashSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    new ValueEnumerable<ValueHashSet<TValue>.Enumerator, TValue> AsValueEnumerable();
}

public readonly record struct ListView<TValue> : IListView<TValue>
{
    private readonly List<TValue> _list;

    public ListView(List<TValue> list)
    {
        _list = list;
    }

    public List<TValue>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public ValueEnumerable<FromList<TValue>, TValue> AsValueEnumerable()
    {
        return _list.AsValueEnumerable();
    }

    public int Count => _list.Count;

    public ReadOnlySpan<TValue> AsSpan()
    {
        return _list.AsSpan();
    }

    public TValue this[int index] => _list[index];

    public static implicit operator ListView<TValue>(List<TValue> list)
    {
        return new ListView<TValue>(list);
    }
}

public readonly ref struct ValueListView<TValue> : IValueListView<TValue>
{
    private readonly ref ValueList<TValue> _list;

    public ValueListView(ref ValueList<TValue> list)
    {
        _list = ref list;
    }

    public ValueList<TValue>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_list);
    }

    public ValueEnumerable<ValueList<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _list.AsValueEnumerable();
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueList<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueList<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueList<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    public int Count => _list.Count;

    public ReadOnlySpan<TValue> AsSpan()
    {
        return _list.AsSpan();
    }

    public TValue this[int index] => _list[index];

    public static implicit operator ValueListView<TValue>(in ValueList<TValue> list)
    {
        return new ValueListView<TValue>(ref Unsafe.AsRef(in list));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueListView<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueListView<>)} is not supported.");
    }

    public bool Equals(scoped ValueListView<TValue> other)
    {
        return Unsafe.AreSame(ref _list, ref other._list);
    }

    public static bool operator ==(scoped ValueListView<TValue> left, scoped ValueListView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueListView<TValue> left, scoped ValueListView<TValue> right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueListView<TValue>
    {
        private readonly ValueList<TValue> _list;

        public Enumerable(ValueList<TValue> list)
        {
            _list = list;
        }

        public ValueList<TValue>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public ValueEnumerable<ValueList<TValue>.Enumerator, TValue> AsValueEnumerable()
        {
            return _list.AsValueEnumerable();
        }

        public int Count => _list.Count;

        public ReadOnlySpan<TValue> AsSpan()
        {
            return _list.AsSpan();
        }

        public TValue this[int index] => _list[index];
    }
}

public readonly record struct DictionaryView<TKey, TValue>
    : IDictionaryView<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;

    public DictionaryView(Dictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }

    public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public ValueEnumerable<FromDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return _dictionary.AsValueEnumerable();
    }

    public int Count => _dictionary.Count;

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] => _dictionary[key];

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

    public static implicit operator DictionaryView<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new DictionaryView<TKey, TValue>(dictionary);
    }
}

public readonly record struct SortedDictionaryView<TKey, TValue>
    : ISortedDictionaryView<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly SortedDictionary<TKey, TValue> _sortedDictionary;

    public SortedDictionaryView(SortedDictionary<TKey, TValue> sortedDictionary)
    {
        _sortedDictionary = sortedDictionary;
    }

    public SortedDictionary<TKey, TValue>.KeyCollection Keys => _sortedDictionary.Keys;

    public SortedDictionary<TKey, TValue>.ValueCollection Values => _sortedDictionary.Values;

    public bool ContainsKey(TKey key)
    {
        return _sortedDictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sortedDictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] => _sortedDictionary[key];

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _sortedDictionary.Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _sortedDictionary.Values;

    public SortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _sortedDictionary.GetEnumerator();
    }

    public ValueEnumerable<FromSortedDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return _sortedDictionary.AsValueEnumerable();
    }

    public int Count => _sortedDictionary.Count;

    public static implicit operator SortedDictionaryView<TKey, TValue>(SortedDictionary<TKey, TValue> dictionary)
    {
        return new SortedDictionaryView<TKey, TValue>(dictionary);
    }
}

public readonly record struct HashSetView<TValue> : IHashSetView<TValue>, IReadOnlySet<TValue>
{
    private readonly HashSet<TValue> _hashSet;

    public HashSetView(HashSet<TValue> hashSet)
    {
        _hashSet = hashSet;
    }

    public HashSet<TValue>.Enumerator GetEnumerator()
    {
        return _hashSet.GetEnumerator();
    }

    public ValueEnumerable<FromHashSet<TValue>, TValue> AsValueEnumerable()
    {
        return _hashSet.AsValueEnumerable();
    }

    public int Count => _hashSet.Count;

    public bool Contains(TValue item)
    {
        return _hashSet.Contains(item);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return _hashSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return _hashSet.SetEquals(other);
    }

    public static implicit operator HashSetView<TValue>(HashSet<TValue> hashSet)
    {
        return new HashSetView<TValue>(hashSet);
    }
}

public readonly record struct SortedSetView<TValue> : ISortedSetView<TValue>, IReadOnlySet<TValue>
{
    private readonly SortedSet<TValue> _sortedSet;

    public SortedSetView(SortedSet<TValue> sortedSet)
    {
        _sortedSet = sortedSet;
    }

    public bool Contains(TValue item)
    {
        return _sortedSet.Contains(item);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return _sortedSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return _sortedSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return _sortedSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return _sortedSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return _sortedSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return _sortedSet.SetEquals(other);
    }

    public SortedSet<TValue>.Enumerator GetEnumerator()
    {
        return _sortedSet.GetEnumerator();
    }

    public ValueEnumerable<FromSortedSet<TValue>, TValue> AsValueEnumerable()
    {
        return _sortedSet.AsValueEnumerable();
    }

    public int Count => _sortedSet.Count;

    public static implicit operator SortedSetView<TValue>(SortedSet<TValue> sortedSet)
    {
        return new SortedSetView<TValue>(sortedSet);
    }
}

public readonly record struct LinkedListView<TValue> : ILinkedListView<TValue>
{
    private readonly LinkedList<TValue> _linkedList;

    public LinkedListView(LinkedList<TValue> linkedList)
    {
        _linkedList = linkedList;
    }

    public LinkedList<TValue>.Enumerator GetEnumerator()
    {
        return _linkedList.GetEnumerator();
    }

    public ValueEnumerable<FromLinkedList<TValue>, TValue> AsValueEnumerable()
    {
        return _linkedList.AsValueEnumerable();
    }

    public int Count => _linkedList.Count;

    public static implicit operator LinkedListView<TValue>(LinkedList<TValue> linkedList)
    {
        return new LinkedListView<TValue>(linkedList);
    }
}

public readonly record struct QueueView<TValue> : IQueueView<TValue>
{
    private readonly Queue<TValue> _queue;

    public QueueView(Queue<TValue> queue)
    {
        _queue = queue;
    }

    public Queue<TValue>.Enumerator GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    public ValueEnumerable<FromQueue<TValue>, TValue> AsValueEnumerable()
    {
        return _queue.AsValueEnumerable();
    }

    public int Count => _queue.Count;

    public static implicit operator QueueView<TValue>(Queue<TValue> queue)
    {
        return new QueueView<TValue>(queue);
    }
}

public readonly ref struct ValueQueueView<TValue> : IValueQueueView<TValue>
{
    private readonly ref ValueQueue<TValue> _queue;

    public ValueQueueView(ref ValueQueue<TValue> queue)
    {
        _queue = ref queue;
    }

    public ValueQueue<TValue>.Enumerator GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_queue);
    }

    public ValueEnumerable<ValueQueue<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _queue.AsValueEnumerable();
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueQueue<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueQueue<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueQueue<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    public int Count => _queue.Count;

    public static implicit operator ValueQueueView<TValue>(in ValueQueue<TValue> queue)
    {
        return new ValueQueueView<TValue>(ref Unsafe.AsRef(in queue));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueQueueView<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueQueueView<>)} is not supported.");
    }

    public bool Equals(scoped ValueQueueView<TValue> other)
    {
        return Unsafe.AreSame(ref _queue, ref other._queue);
    }

    public static bool operator ==(scoped ValueQueueView<TValue> left, scoped ValueQueueView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueQueueView<TValue> left, scoped ValueQueueView<TValue> right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueQueueView<TValue>
    {
        private readonly ValueQueue<TValue> _queue;

        public Enumerable(ValueQueue<TValue> queue)
        {
            _queue = queue;
        }

        public ValueQueue<TValue>.Enumerator GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        public ValueEnumerable<ValueQueue<TValue>.Enumerator, TValue> AsValueEnumerable()
        {
            return _queue.AsValueEnumerable();
        }

        public int Count => _queue.Count;
    }
}

public readonly record struct StackView<TValue> : IStackView<TValue>
{
    private readonly Stack<TValue> _stack;

    public StackView(Stack<TValue> stack)
    {
        _stack = stack;
    }

    public Stack<TValue>.Enumerator GetEnumerator()
    {
        return _stack.GetEnumerator();
    }

    public ValueEnumerable<FromStack<TValue>, TValue> AsValueEnumerable()
    {
        return _stack.AsValueEnumerable();
    }

    public int Count => _stack.Count;

    public static implicit operator StackView<TValue>(Stack<TValue> stack)
    {
        return new StackView<TValue>(stack);
    }
}

public readonly ref struct ValueStackView<TValue> : IValueStackView<TValue>
{
    private readonly ref ValueStack<TValue> _stack;

    public ValueStackView(ref ValueStack<TValue> stack)
    {
        _stack = ref stack;
    }

    public ValueStack<TValue>.Enumerator GetEnumerator()
    {
        return _stack.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_stack);
    }

    public ValueEnumerable<ValueStack<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _stack.AsValueEnumerable();
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueStack<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueStack<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueStack<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    public int Count => _stack.Count;

    public static implicit operator ValueStackView<TValue>(in ValueStack<TValue> stack)
    {
        return new ValueStackView<TValue>(ref Unsafe.AsRef(in stack));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueStackView<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueStackView<>)} is not supported.");
    }

    public bool Equals(scoped ValueStackView<TValue> other)
    {
        return Unsafe.AreSame(ref _stack, ref other._stack);
    }

    public static bool operator ==(scoped ValueStackView<TValue> left, scoped ValueStackView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueStackView<TValue> left, scoped ValueStackView<TValue> right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueStackView<TValue>
    {
        private readonly ValueStack<TValue> _stack;

        public Enumerable(ValueStack<TValue> stack)
        {
            _stack = stack;
        }

        public ValueStack<TValue>.Enumerator GetEnumerator()
        {
            return _stack.GetEnumerator();
        }

        public ValueEnumerable<ValueStack<TValue>.Enumerator, TValue> AsValueEnumerable()
        {
            return _stack.AsValueEnumerable();
        }

        public int Count => _stack.Count;
    }
}

public readonly record struct SparseSetView<TKey, TValue, TStorage>
    : ISparseSetView<TKey, TValue, TStorage>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    private readonly SparseSet<TKey, TValue, TStorage> _sparseSet;

    public SparseSetView(SparseSet<TKey, TValue, TStorage> sparseSet)
    {
        _sparseSet = sparseSet;
    }

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public TValue this[in TKey key] => _sparseSet[key];

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys.AsEnumerable();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

    public int Count => _sparseSet.Count;

    public SparseSet<TKey, TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    ValueEnumerable<
        StructEnumerator<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
    }

    public ValueEnumerable<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    public bool ContainsKey(in TKey key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public int GetKeyIndex(in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public static implicit operator SparseSetView<TKey, TValue, TStorage>(SparseSet<TKey, TValue, TStorage> sparseSet)
    {
        return new SparseSetView<TKey, TValue, TStorage>(sparseSet);
    }
}

public readonly record struct SparseSetView<TKey> : ISparseSetView<TKey>, IReadOnlySet<TKey>, IReadOnlyList<TKey>
{
    private readonly SparseSet<TKey> _sparseSet;

    public SparseSetView(SparseSet<TKey> sparseSet)
    {
        _sparseSet = sparseSet;
    }

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public TKey this[int index] => _sparseSet[index];

    public bool IsProperSubsetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TKey> other)
    {
        return _sparseSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TKey> other)
    {
        return _sparseSet.SetEquals(other);
    }

    bool IReadOnlySet<TKey>.Contains(TKey item)
    {
        return Contains(item);
    }

    public int Count => _sparseSet.Count;

    public SparseSet<TKey>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<SparseSet<TKey>.Enumerator, TKey>, TKey> IStructEnumerable<
        SparseSet<TKey>.Enumerator,
        TKey
    >.AsValueEnumerable()
    {
        return new StructEnumerator<SparseSet<TKey>.Enumerator, TKey>(GetEnumerator());
    }

    public bool Contains(in TKey key)
    {
        return _sparseSet.Contains(key);
    }

    public int GetKeyIndex(in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public ValueEnumerable<SparseSet<TKey>.Enumerator, TKey> AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    public static implicit operator SparseSetView<TKey>(SparseSet<TKey> sparseSet)
    {
        return new SparseSetView<TKey>(sparseSet);
    }
}

public readonly record struct EntitySparseSetView : IEntitySparseSetView, IReadOnlySet<Entity>, IReadOnlyList<Entity>
{
    private readonly EntitySparseSet _sparseSet;

    public EntitySparseSetView(EntitySparseSet sparseSet)
    {
        _sparseSet = sparseSet;
    }

    public Scene Scene => _sparseSet.Scene;

    public int Count => _sparseSet.Count;

    public EntitySparseSet.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<EntitySparseSet.Enumerator, Entity>, Entity> IStructEnumerable<
        EntitySparseSet.Enumerator,
        Entity
    >.AsValueEnumerable()
    {
        return new StructEnumerator<EntitySparseSet.Enumerator, Entity>(GetEnumerator());
    }

    public Entity this[int index] => _sparseSet[index];

    public bool IsProperSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<Entity> other)
    {
        return _sparseSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<Entity> other)
    {
        return _sparseSet.SetEquals(other);
    }

    bool IReadOnlySet<Entity>.Contains(Entity item)
    {
        return Contains(item);
    }

    public bool Contains(in Entity key)
    {
        return _sparseSet.Contains(key);
    }

    public int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public ValueEnumerable<EntitySparseSet.Enumerator, Entity> AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    public static implicit operator EntitySparseSetView(EntitySparseSet sparseSet)
    {
        return new EntitySparseSetView(sparseSet);
    }
}

public readonly record struct EntitySparseSetView<TValue, TStorage>
    : IEntitySparseSetView<TValue, TStorage>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    private readonly EntitySparseSet<TValue, TStorage> _sparseSet;

    public EntitySparseSetView(EntitySparseSet<TValue, TStorage> sparseSet)
    {
        _sparseSet = sparseSet;
    }

    public Scene Scene => _sparseSet.Scene;

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public EntitySparseSet<TValue, TStorage>.KeyEnumerable Keys => _sparseSet.Keys;

    public TValue this[in Entity key] => _sparseSet[key];

    public int Count => _sparseSet.Count;

    public EntitySparseSet<TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    ValueEnumerable<
        StructEnumerator<EntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>,
        KeyValuePair<Entity, TValue>
    > IStructEnumerable<EntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<EntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>(
            GetEnumerator()
        );
    }

    IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => Values;

    bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    public KeyValuePair<Entity, TValue> this[int index] => _sparseSet[index];

    public ValueEnumerable<
        EntitySparseSet<TValue, TStorage>.Enumerator,
        KeyValuePair<Entity, TValue>
    > AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    public bool ContainsKey(in Entity key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public bool TryGetValue(in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public static implicit operator EntitySparseSetView<TValue, TStorage>(EntitySparseSet<TValue, TStorage> sparseSet)
    {
        return new EntitySparseSetView<TValue, TStorage>(sparseSet);
    }
}

public readonly ref struct ValueSparseSetView<TKey, TValue, TStorage>
    : IValueSparseSetView<TKey, TValue, TStorage>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    private readonly ref ValueSparseSet<TKey, TValue, TStorage> _sparseSet;

    public ValueSparseSetView(ref ValueSparseSet<TKey, TValue, TStorage> sparseSet)
    {
        _sparseSet = ref sparseSet;
    }

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public TValue this[scoped in TKey key] => _sparseSet[key];

    public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

    public int Count => _sparseSet.Count;

    public bool ContainsKey(scoped in TKey key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public bool TryGetValue(scoped in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public int GetKeyIndex(scoped in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public ValueSparseSet<TKey, TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_sparseSet);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ValueEnumerable<
        ValueSparseSet<TKey, TValue, TStorage>.Enumerator,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    ValueEnumerable<
        StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<
        ValueSparseSet<TKey, TValue, TStorage>.Enumerator,
        KeyValuePair<TKey, TValue>
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys.AsEnumerable();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    public static implicit operator ValueSparseSetView<TKey, TValue, TStorage>(
        in ValueSparseSet<TKey, TValue, TStorage> sparseSet
    )
    {
        return new ValueSparseSetView<TKey, TValue, TStorage>(ref Unsafe.AsRef(in sparseSet));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueSparseSetView<,,>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueSparseSetView<,,>)} is not supported."
        );
    }

    public bool Equals(scoped ValueSparseSetView<TKey, TValue, TStorage> other)
    {
        return Unsafe.AreSame(ref _sparseSet, ref other._sparseSet);
    }

    public static bool operator ==(
        scoped ValueSparseSetView<TKey, TValue, TStorage> left,
        scoped ValueSparseSetView<TKey, TValue, TStorage> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        scoped ValueSparseSetView<TKey, TValue, TStorage> left,
        scoped ValueSparseSetView<TKey, TValue, TStorage> right
    )
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable
        : IValueSparseSetView<TKey, TValue, TStorage>,
            IReadOnlyDictionary<TKey, TValue>,
            IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        private readonly ValueSparseSet<TKey, TValue, TStorage> _sparseSet;

        public Enumerable(ValueSparseSet<TKey, TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
        }

        public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

        [UnscopedRef]
        public ValueListView<TKey> Keys => _sparseSet.Keys;

        public TValue this[in TKey key] => _sparseSet[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys.AsEnumerable();

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return ContainsKey(key);
        }

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return TryGetValue(key, out value);
        }

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

        public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

        public int Count => _sparseSet.Count;

        public ValueSparseSet<TKey, TValue, TStorage>.Enumerator GetEnumerator()
        {
            return _sparseSet.GetEnumerator();
        }

        ValueEnumerable<
            StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
            KeyValuePair<TKey, TValue>
        > IStructEnumerable<
            ValueSparseSet<TKey, TValue, TStorage>.Enumerator,
            KeyValuePair<TKey, TValue>
        >.AsValueEnumerable()
        {
            return new StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>(
                GetEnumerator()
            );
        }

        public ValueEnumerable<
            ValueSparseSet<TKey, TValue, TStorage>.Enumerator,
            KeyValuePair<TKey, TValue>
        > AsValueEnumerable()
        {
            return _sparseSet.AsValueEnumerable();
        }

        public bool ContainsKey(in TKey key)
        {
            return _sparseSet.ContainsKey(key);
        }

        public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _sparseSet.TryGetValue(key, out value);
        }
    }
}

public readonly ref struct ValueSparseSetView<TKey> : IValueSparseSetView<TKey>, IReadOnlySet<TKey>, IReadOnlyList<TKey>
{
    private readonly ref ValueSparseSet<TKey> _sparseSet;

    public ValueSparseSetView(ref ValueSparseSet<TKey> sparseSet)
    {
        _sparseSet = ref sparseSet;
    }

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public int Count => _sparseSet.Count;

    public TKey this[int index] => _sparseSet[index];

    public bool Contains(scoped in TKey key)
    {
        return _sparseSet.Contains(key);
    }

    public int GetKeyIndex(scoped in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public bool IsProperSubsetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TKey> other)
    {
        return _sparseSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TKey> other)
    {
        return _sparseSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TKey> other)
    {
        return _sparseSet.SetEquals(other);
    }

    bool IReadOnlySet<TKey>.Contains(TKey item)
    {
        return Contains(item);
    }

    public ValueSparseSet<TKey>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_sparseSet);
    }

    public ValueEnumerable<ValueSparseSet<TKey>.Enumerator, TKey> AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    ValueEnumerable<StructEnumerator<ValueSparseSet<TKey>.Enumerator, TKey>, TKey> IStructEnumerable<
        ValueSparseSet<TKey>.Enumerator,
        TKey
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueSparseSet<TKey>.Enumerator, TKey>(GetEnumerator());
    }

    IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator ValueSparseSetView<TKey>(in ValueSparseSet<TKey> sparseSet)
    {
        return new ValueSparseSetView<TKey>(ref Unsafe.AsRef(in sparseSet));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueSparseSetView<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueSparseSetView<>)} is not supported.");
    }

    public bool Equals(scoped ValueSparseSetView<TKey> other)
    {
        return Unsafe.AreSame(ref _sparseSet, ref other._sparseSet);
    }

    public static bool operator ==(scoped ValueSparseSetView<TKey> left, scoped ValueSparseSetView<TKey> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueSparseSetView<TKey> left, scoped ValueSparseSetView<TKey> right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueSparseSetView<TKey>, IReadOnlySet<TKey>, IReadOnlyList<TKey>
    {
        private readonly ValueSparseSet<TKey> _sparseSet;

        public Enumerable(ValueSparseSet<TKey> sparseSet)
        {
            _sparseSet = sparseSet;
        }

        [UnscopedRef]
        public ValueListView<TKey> Keys => _sparseSet.Keys;

        public TKey this[int index] => _sparseSet[index];

        public bool IsProperSubsetOf(IEnumerable<TKey> other)
        {
            return _sparseSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<TKey> other)
        {
            return _sparseSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<TKey> other)
        {
            return _sparseSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<TKey> other)
        {
            return _sparseSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<TKey> other)
        {
            return _sparseSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<TKey> other)
        {
            return _sparseSet.SetEquals(other);
        }

        bool IReadOnlySet<TKey>.Contains(TKey item)
        {
            return Contains(item);
        }

        public int Count => _sparseSet.Count;

        public ValueSparseSet<TKey>.Enumerator GetEnumerator()
        {
            return _sparseSet.GetEnumerator();
        }

        ValueEnumerable<StructEnumerator<ValueSparseSet<TKey>.Enumerator, TKey>, TKey> IStructEnumerable<
            ValueSparseSet<TKey>.Enumerator,
            TKey
        >.AsValueEnumerable()
        {
            return new StructEnumerator<ValueSparseSet<TKey>.Enumerator, TKey>(GetEnumerator());
        }

        public bool Contains(in TKey key)
        {
            return _sparseSet.Contains(key);
        }

        public int GetKeyIndex(in TKey key)
        {
            return _sparseSet.GetKeyIndex(key);
        }

        public ValueEnumerable<ValueSparseSet<TKey>.Enumerator, TKey> AsValueEnumerable()
        {
            return _sparseSet.AsValueEnumerable();
        }
    }
}

public readonly ref struct ValueEntitySparseSetView
    : IValueEntitySparseSetView,
        IReadOnlySet<Entity>,
        IReadOnlyList<Entity>
{
    private readonly ref ValueEntitySparseSet _sparseSet;

    public ValueEntitySparseSetView(ref ValueEntitySparseSet sparseSet)
    {
        _sparseSet = ref sparseSet;
    }

    public Scene Scene => _sparseSet.Scene;

    public int Count => _sparseSet.Count;

    public Entity this[int index] => _sparseSet[index];

    public bool Contains(scoped in Entity key)
    {
        return _sparseSet.Contains(key);
    }

    public int GetKeyIndex(scoped in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public bool IsProperSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<Entity> other)
    {
        return _sparseSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<Entity> other)
    {
        return _sparseSet.SetEquals(other);
    }

    bool IReadOnlySet<Entity>.Contains(Entity item)
    {
        return Contains(item);
    }

    public ValueEntitySparseSet.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_sparseSet);
    }

    public ValueEnumerable<ValueEntitySparseSet.Enumerator, Entity> AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    ValueEnumerable<StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>, Entity> IStructEnumerable<
        ValueEntitySparseSet.Enumerator,
        Entity
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>(GetEnumerator());
    }

    IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator ValueEntitySparseSetView(in ValueEntitySparseSet sparseSet)
    {
        return new ValueEntitySparseSetView(ref Unsafe.AsRef(in sparseSet));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueEntitySparseSetView)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueEntitySparseSetView)} is not supported."
        );
    }

    public bool Equals(scoped ValueEntitySparseSetView other)
    {
        return Unsafe.AreSame(ref _sparseSet, ref other._sparseSet);
    }

    public static bool operator ==(scoped ValueEntitySparseSetView left, scoped ValueEntitySparseSetView right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueEntitySparseSetView left, scoped ValueEntitySparseSetView right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueEntitySparseSetView, IReadOnlySet<Entity>, IReadOnlyList<Entity>
    {
        private readonly ValueEntitySparseSet _sparseSet;

        public Enumerable(ValueEntitySparseSet sparseSet)
        {
            _sparseSet = sparseSet;
        }

        public Scene Scene => _sparseSet.Scene;

        public Entity this[int index] => _sparseSet[index];

        public bool IsProperSubsetOf(IEnumerable<Entity> other)
        {
            return _sparseSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<Entity> other)
        {
            return _sparseSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<Entity> other)
        {
            return _sparseSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<Entity> other)
        {
            return _sparseSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<Entity> other)
        {
            return _sparseSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<Entity> other)
        {
            return _sparseSet.SetEquals(other);
        }

        bool IReadOnlySet<Entity>.Contains(Entity item)
        {
            return Contains(item);
        }

        public int Count => _sparseSet.Count;

        public ValueEntitySparseSet.Enumerator GetEnumerator()
        {
            return _sparseSet.GetEnumerator();
        }

        ValueEnumerable<StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>, Entity> IStructEnumerable<
            ValueEntitySparseSet.Enumerator,
            Entity
        >.AsValueEnumerable()
        {
            return new StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>(GetEnumerator());
        }

        public bool Contains(in Entity key)
        {
            return _sparseSet.Contains(key);
        }

        public int GetKeyIndex(in Entity key)
        {
            return _sparseSet.GetKeyIndex(key);
        }

        public ValueEnumerable<ValueEntitySparseSet.Enumerator, Entity> AsValueEnumerable()
        {
            return _sparseSet.AsValueEnumerable();
        }
    }
}

public readonly ref struct ValueEntitySparseSetView<TValue, TStorage>
    : IValueEntitySparseSetView<TValue, TStorage>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    private readonly ref ValueEntitySparseSet<TValue, TStorage> _sparseSet;

    public ValueEntitySparseSetView(ref ValueEntitySparseSet<TValue, TStorage> sparseSet)
    {
        _sparseSet = ref sparseSet;
    }

    public Scene Scene => _sparseSet.Scene;

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public ValueEntitySparseSet<TValue, TStorage>.KeyEnumerable Keys => _sparseSet.Keys;

    public TValue this[scoped in Entity key] => _sparseSet[key];

    public KeyValuePair<Entity, TValue> this[int index] => _sparseSet[index];

    public int Count => _sparseSet.Count;

    public bool ContainsKey(scoped in Entity key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public bool TryGetValue(scoped in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public int GetKeyIndex(scoped in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public ValueEntitySparseSet<TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_sparseSet);
    }

    public ValueEnumerable<
        ValueEntitySparseSet<TValue, TStorage>.Enumerator,
        KeyValuePair<Entity, TValue>
    > AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    ValueEnumerable<
        StructEnumerator<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>,
        KeyValuePair<Entity, TValue>
    > IStructEnumerable<
        ValueEntitySparseSet<TValue, TStorage>.Enumerator,
        KeyValuePair<Entity, TValue>
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>(
            GetEnumerator()
        );
    }

    IEnumerator<KeyValuePair<Entity, TValue>> IEnumerable<KeyValuePair<Entity, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => Values;

    bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    public static implicit operator ValueEntitySparseSetView<TValue, TStorage>(
        in ValueEntitySparseSet<TValue, TStorage> sparseSet
    )
    {
        return new ValueEntitySparseSetView<TValue, TStorage>(ref Unsafe.AsRef(in sparseSet));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException(
            $"{nameof(Equals)}() on {nameof(ValueEntitySparseSetView<,>)} is not supported."
        );
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueEntitySparseSetView<,>)} is not supported."
        );
    }

    public bool Equals(scoped ValueEntitySparseSetView<TValue, TStorage> other)
    {
        return Unsafe.AreSame(ref _sparseSet, ref other._sparseSet);
    }

    public static bool operator ==(
        scoped ValueEntitySparseSetView<TValue, TStorage> left,
        scoped ValueEntitySparseSetView<TValue, TStorage> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        scoped ValueEntitySparseSetView<TValue, TStorage> left,
        scoped ValueEntitySparseSetView<TValue, TStorage> right
    )
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable
        : IValueEntitySparseSetView<TValue, TStorage>,
            IReadOnlyDictionary<Entity, TValue>,
            IReadOnlyList<KeyValuePair<Entity, TValue>>
    {
        private readonly ValueEntitySparseSet<TValue, TStorage> _sparseSet;

        public Enumerable(ValueEntitySparseSet<TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
        }

        public Scene Scene => _sparseSet.Scene;

        public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

        public ValueEntitySparseSet<TValue, TStorage>.KeyEnumerable Keys => _sparseSet.Keys;

        public TValue this[in Entity key] => _sparseSet[key];

        IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => Values;

        bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
        {
            return ContainsKey(key);
        }

        bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
        {
            return TryGetValue(key, out value);
        }

        TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

        public KeyValuePair<Entity, TValue> this[int index] => _sparseSet[index];

        public int Count => _sparseSet.Count;

        public ValueEntitySparseSet<TValue, TStorage>.Enumerator GetEnumerator()
        {
            return _sparseSet.GetEnumerator();
        }

        ValueEnumerable<
            StructEnumerator<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>,
            KeyValuePair<Entity, TValue>
        > IStructEnumerable<
            ValueEntitySparseSet<TValue, TStorage>.Enumerator,
            KeyValuePair<Entity, TValue>
        >.AsValueEnumerable()
        {
            return new StructEnumerator<
                ValueEntitySparseSet<TValue, TStorage>.Enumerator,
                KeyValuePair<Entity, TValue>
            >(GetEnumerator());
        }

        public bool ContainsKey(in Entity key)
        {
            return _sparseSet.ContainsKey(key);
        }

        public bool TryGetValue(in Entity key, [MaybeNullWhen(false)] out TValue value)
        {
            return _sparseSet.TryGetValue(key, out value);
        }

        public int GetKeyIndex(in Entity key)
        {
            return _sparseSet.GetKeyIndex(key);
        }

        public ValueEnumerable<
            ValueEntitySparseSet<TValue, TStorage>.Enumerator,
            KeyValuePair<Entity, TValue>
        > AsValueEnumerable()
        {
            return _sparseSet.AsValueEnumerable();
        }
    }
}

public readonly ref struct ValueDictionaryView<TKey, TValue>
    : IValueDictionaryView<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly ref ValueDictionary<TKey, TValue> _dictionary;

    public ValueDictionaryView(ref ValueDictionary<TKey, TValue> dictionary)
    {
        _dictionary = ref dictionary;
    }

    public ValueDictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

    public ValueDictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    public TValue this[scoped in TKey key] => _dictionary[key];

    public int Count => _dictionary.Count;

    public bool ContainsKey(scoped in TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool ContainsValue(scoped in TValue value)
    {
        return _dictionary.ContainsValue(value);
    }

    public bool TryGetValue(scoped in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public ValueDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_dictionary);
    }

    public ValueEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return _dictionary.AsValueEnumerable();
    }

    ValueEnumerable<
        StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    public static implicit operator ValueDictionaryView<TKey, TValue>(in ValueDictionary<TKey, TValue> dictionary)
    {
        return new ValueDictionaryView<TKey, TValue>(ref Unsafe.AsRef(in dictionary));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueDictionaryView<,>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueDictionaryView<,>)} is not supported."
        );
    }

    public bool Equals(scoped ValueDictionaryView<TKey, TValue> other)
    {
        return Unsafe.AreSame(ref _dictionary, ref other._dictionary);
    }

    public static bool operator ==(
        scoped ValueDictionaryView<TKey, TValue> left,
        scoped ValueDictionaryView<TKey, TValue> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        scoped ValueDictionaryView<TKey, TValue> left,
        scoped ValueDictionaryView<TKey, TValue> right
    )
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueDictionaryView<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ValueDictionary<TKey, TValue> _dictionary;

        public Enumerable(ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public ValueDictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

        public ValueDictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

        public TValue this[in TKey key] => _dictionary[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return ContainsKey(key);
        }

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return TryGetValue(key, out value);
        }

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

        public int Count => _dictionary.Count;

        public ValueDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public ValueEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
        {
            return _dictionary.AsValueEnumerable();
        }

        public bool ContainsKey(in TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(in TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}

public readonly ref struct ValueHashSetView<TValue> : IValueHashSetView<TValue>, IReadOnlySet<TValue>
{
    private readonly ref ValueHashSet<TValue> _hashSet;

    public ValueHashSetView(ref ValueHashSet<TValue> hashSet)
    {
        _hashSet = ref hashSet;
    }

    public int Count => _hashSet.Count;

    public bool Contains(scoped in TValue item)
    {
        return _hashSet.Contains(item);
    }

    public bool IsProperSubsetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<TValue> other)
    {
        return _hashSet.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<TValue> other)
    {
        return _hashSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<TValue> other)
    {
        return _hashSet.SetEquals(other);
    }

    public ValueHashSet<TValue>.Enumerator GetEnumerator()
    {
        return _hashSet.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_hashSet);
    }

    public ValueEnumerable<ValueHashSet<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _hashSet.AsValueEnumerable();
    }

    ValueEnumerable<StructEnumerator<ValueHashSet<TValue>.Enumerator, TValue>, TValue> IStructEnumerable<
        ValueHashSet<TValue>.Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueHashSet<TValue>.Enumerator, TValue>(GetEnumerator());
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool IReadOnlySet<TValue>.Contains(TValue item)
    {
        return Contains(item);
    }

    public static implicit operator ValueHashSetView<TValue>(in ValueHashSet<TValue> hashSet)
    {
        return new ValueHashSetView<TValue>(ref Unsafe.AsRef(in hashSet));
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueHashSetView<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueHashSetView<>)} is not supported.");
    }

    public bool Equals(scoped ValueHashSetView<TValue> other)
    {
        return Unsafe.AreSame(ref _hashSet, ref other._hashSet);
    }

    public static bool operator ==(scoped ValueHashSetView<TValue> left, scoped ValueHashSetView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped ValueHashSetView<TValue> left, scoped ValueHashSetView<TValue> right)
    {
        return !left.Equals(right);
    }

    public readonly record struct Enumerable : IValueHashSetView<TValue>, IReadOnlySet<TValue>
    {
        private readonly ValueHashSet<TValue> _hashSet;

        public Enumerable(ValueHashSet<TValue> hashSet)
        {
            _hashSet = hashSet;
        }

        bool IReadOnlySet<TValue>.Contains(TValue item)
        {
            return Contains(item);
        }

        public bool IsProperSubsetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<TValue> other)
        {
            return _hashSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<TValue> other)
        {
            return _hashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<TValue> other)
        {
            return _hashSet.SetEquals(other);
        }

        public int Count => _hashSet.Count;

        public ValueHashSet<TValue>.Enumerator GetEnumerator()
        {
            return _hashSet.GetEnumerator();
        }

        public ValueEnumerable<ValueHashSet<TValue>.Enumerator, TValue> AsValueEnumerable()
        {
            return _hashSet.AsValueEnumerable();
        }

        public bool Contains(in TValue item)
        {
            return _hashSet.Contains(item);
        }
    }
}

public readonly record struct ArrayView<TValue> : IArrayView<TValue>
{
    private readonly TValue[] _array;

    public ArrayView(TValue[] array)
    {
        _array = array;
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ArrayEnumerator<TValue> GetEnumerator()
    {
        return new ArrayEnumerator<TValue>(_array);
    }

    public ValueEnumerable<FromArray<TValue>, TValue> AsValueEnumerable()
    {
        return _array.AsValueEnumerable();
    }

    public int Count => _array.Length;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ReadOnlySpan<TValue> AsSpan()
    {
        return _array;
    }

    public TValue this[int index] => _array[index];

    public static implicit operator ArrayView<TValue>(TValue[] array)
    {
        return new ArrayView<TValue>(array);
    }

    public static implicit operator ReadOnlySpan<TValue>(ArrayView<TValue> arrayView)
    {
        return arrayView._array;
    }
}

public struct ArrayEnumerator<TValue> : IStructEnumerator<TValue>
{
    private readonly TValue[] _array;
    private int _index;

    public ArrayEnumerator(TValue[] array)
    {
        _array = array;
        Reset();
    }

    public bool MoveNext()
    {
        if ((uint)_index < (uint)_array.Length)
        {
            Current = _array[_index];
            _index++;
            return true;
        }

        Current = default!;
        _index = -1;
        return false;
    }

    public void Reset()
    {
        _index = 0;
        Current = default!;
    }

    public TValue Current { get; private set; } = default!;

    public void Dispose() { }

    public static implicit operator ArrayEnumerator<TValue>(TValue[] array)
    {
        return new ArrayEnumerator<TValue>(array);
    }
}

public struct SpanViewEnumerator<TValue> : IStructEnumerator<TValue>, ISpanView<TValue>
{
    private readonly ISpanView<TValue> _spanView;
    private int _index;

    public SpanViewEnumerator(ISpanView<TValue> spanView)
    {
        _spanView = spanView;
        Reset();
    }

    public bool MoveNext()
    {
        var span = _spanView.AsSpan();
        if ((uint)_index < (uint)span.Length)
        {
            Current = span[_index];
            _index++;
            return true;
        }

        Current = default!;
        _index = -1;
        return false;
    }

    public void Reset()
    {
        _index = 0;
        Current = default!;
    }

    public TValue Current { get; private set; } = default!;

    public void Dispose() { }

    public ReadOnlySpan<TValue> AsSpan()
    {
        return _spanView.AsSpan();
    }

    public ValueEnumerator<FromSpan<TValue>, TValue> GetEnumerator()
    {
        return new ValueEnumerator<FromSpan<TValue>, TValue>(AsValueEnumerable().Enumerator);
    }

    public ValueEnumerable<FromSpan<TValue>, TValue> AsValueEnumerable()
    {
        return _spanView.AsValueEnumerable();
    }
}

public static class CollectionViewExtensions
{
    public static ListView<T> AsView<T>(this List<T> list)
    {
        return list;
    }

    public static ValueListView<T> AsView<T>(in this ValueList<T> list)
    {
        return list;
    }

    public static DictionaryView<TKey, TValue> AsView<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        return dictionary;
    }

    public static SortedDictionaryView<TKey, TValue> AsView<TKey, TValue>(
        this SortedDictionary<TKey, TValue> sortedDictionary
    )
        where TKey : notnull
    {
        return sortedDictionary;
    }

    public static HashSetView<TValue> AsView<TValue>(this HashSet<TValue> hashSet)
    {
        return hashSet;
    }

    public static SortedSetView<TValue> AsView<TValue>(this SortedSet<TValue> sortedSet)
    {
        return sortedSet;
    }

    public static LinkedListView<TValue> AsView<TValue>(this LinkedList<TValue> linkedList)
    {
        return linkedList;
    }

    public static QueueView<TValue> AsView<TValue>(this Queue<TValue> queue)
    {
        return queue;
    }

    public static ValueQueue<TValue> AsView<TValue>(in this ValueQueue<TValue> queue)
    {
        return queue;
    }

    public static StackView<TValue> AsView<TValue>(this Stack<TValue> stack)
    {
        return stack;
    }

    public static ValueStack<TValue> AsView<TValue>(in this ValueStack<TValue> stack)
    {
        return stack;
    }

    public static ArrayView<TValue> AsView<TValue>(this TValue[] array)
    {
        return array;
    }

    public static SparseSetView<TKey, TValue, TStorage> AsView<TKey, TValue, TStorage>(
        this SparseSet<TKey, TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }

    public static ValueSparseSetView<TKey, TValue, TStorage> AsView<TKey, TValue, TStorage>(
        in this ValueSparseSet<TKey, TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }

    public static EntitySparseSetView<TValue, TStorage> AsView<TValue, TStorage>(
        this EntitySparseSet<TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }

    public static SparseSetView<TKey> AsView<TKey>(this SparseSet<TKey> sparseSet)
    {
        return sparseSet;
    }

    public static ValueSparseSetView<TKey> AsView<TKey>(in this ValueSparseSet<TKey> sparseSet)
    {
        return sparseSet;
    }

    public static EntitySparseSetView AsView(this EntitySparseSet sparseSet)
    {
        return sparseSet;
    }

    public static ValueEntitySparseSetView AsView(in this ValueEntitySparseSet sparseSet)
    {
        return sparseSet;
    }

    public static ValueEntitySparseSetView<TValue, TStorage> AsView<TValue, TStorage>(
        in this ValueEntitySparseSet<TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }

    public static ValueEntitySparseSetView<TValue, ValueList<TValue>> AsView<TValue>(
        in this ValueEntitySparseSet<TValue> sparseSet
    )
    {
        return new ValueEntitySparseSetView<TValue, ValueList<TValue>>(ref Unsafe.AsRef(in sparseSet).Storage);
    }

    public static ValueDictionaryView<TKey, TValue> AsView<TKey, TValue>(
        in this ValueDictionary<TKey, TValue> dictionary
    )
        where TKey : notnull
    {
        return dictionary;
    }

    public static ValueHashSetView<TValue> AsView<TValue>(in this ValueHashSet<TValue> hashSet)
    {
        return hashSet;
    }

    public static ValueListView<T> AsView<T>(in this ValueListRef<T> list)
    {
        return list;
    }

    public static ValueQueueView<TValue> AsView<TValue>(in this ValueQueueRef<TValue> queue)
    {
        return queue;
    }

    public static ValueStackView<TValue> AsView<TValue>(in this ValueStackRef<TValue> stack)
    {
        return stack;
    }

    public static ValueDictionaryView<TKey, TValue> AsView<TKey, TValue>(
        in this ValueDictionaryRef<TKey, TValue> dictionary
    )
        where TKey : notnull
    {
        return dictionary;
    }

    public static ValueHashSetView<TValue> AsView<TValue>(in this ValueHashSetRef<TValue> hashSet)
    {
        return hashSet;
    }

    public static ValueSparseSetView<TKey, TValue, TStorage> AsView<TKey, TValue, TStorage>(
        in this ValueSparseSetRef<TKey, TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }

    public static ValueSparseSetView<TKey> AsView<TKey>(in this ValueSparseSetRef<TKey> sparseSet)
    {
        return sparseSet;
    }

    public static ValueEntitySparseSetView AsView(in this ValueEntitySparseSetRef sparseSet)
    {
        return sparseSet;
    }

    public static ValueEntitySparseSetView<TValue, TStorage> AsView<TValue, TStorage>(
        in this ValueEntitySparseSetRef<TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return sparseSet;
    }
}
