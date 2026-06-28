using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

public interface IValueSparseSetView<TKey, TValue, TStorage>
    : IStructEnumerable<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();
}

public interface IValueDictionaryView<TKey, TValue>
    : IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => AsValueEnumerable().Count();
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

    public bool Equals(ValueListView<TValue> other)
    {
        return Unsafe.AreSame(ref _list, ref other._list);
    }

    public static bool operator ==(ValueListView<TValue> left, ValueListView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueListView<TValue> left, ValueListView<TValue> right)
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

    public bool Equals(ValueQueueView<TValue> other)
    {
        return Unsafe.AreSame(ref _queue, ref other._queue);
    }

    public static bool operator ==(ValueQueueView<TValue> left, ValueQueueView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueQueueView<TValue> left, ValueQueueView<TValue> right)
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

    public bool Equals(ValueStackView<TValue> other)
    {
        return Unsafe.AreSame(ref _stack, ref other._stack);
    }

    public static bool operator ==(ValueStackView<TValue> left, ValueStackView<TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueStackView<TValue> left, ValueStackView<TValue> right)
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

    public SparseSet<TKey, TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

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

    public ValueEnumerable<
        StructEnumerator<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
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

    public int GetKeyIndex(in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public static implicit operator SparseSetView<TKey, TValue, TStorage>(SparseSet<TKey, TValue, TStorage> sparseSet)
    {
        return new SparseSetView<TKey, TValue, TStorage>(sparseSet);
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

    public ValueSparseSet<TKey, TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public TValue this[in TKey key] => _sparseSet[key];

    public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

    public int Count => _sparseSet.Count;

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
        StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
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

    public bool Equals(ValueSparseSetView<TKey, TValue, TStorage> other)
    {
        return Unsafe.AreSame(ref _sparseSet, ref other._sparseSet);
    }

    public static bool operator ==(
        ValueSparseSetView<TKey, TValue, TStorage> left,
        ValueSparseSetView<TKey, TValue, TStorage> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        ValueSparseSetView<TKey, TValue, TStorage> left,
        ValueSparseSetView<TKey, TValue, TStorage> right
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

        public ValueSparseSet<TKey, TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

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

        public ValueEnumerable<
            StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
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

    public TValue this[in TKey key] => _dictionary[key];

    public int Count => _dictionary.Count;

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

    public ValueDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public Enumerable AsEnumerable()
    {
        return new Enumerable(_dictionary);
    }

    public ValueEnumerable<
        StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _dictionary.AsValueEnumerable();
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

    public bool Equals(ValueDictionaryView<TKey, TValue> other)
    {
        return Unsafe.AreSame(ref _dictionary, ref other._dictionary);
    }

    public static bool operator ==(ValueDictionaryView<TKey, TValue> left, ValueDictionaryView<TKey, TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueDictionaryView<TKey, TValue> left, ValueDictionaryView<TKey, TValue> right)
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

        public ValueEnumerable<
            StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
            KeyValuePair<TKey, TValue>
        > AsValueEnumerable()
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

public static class ViewExtensions
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

    public static ValueDictionaryView<TKey, TValue> AsView<TKey, TValue>(
        in this ValueDictionary<TKey, TValue> dictionary
    )
        where TKey : notnull
    {
        return dictionary;
    }
}
