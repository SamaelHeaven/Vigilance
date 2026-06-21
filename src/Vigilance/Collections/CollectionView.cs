using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Collections;

public interface IReadOnlySpan<TValue>
{
    ReadOnlySpan<TValue> AsSpan();
}

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

public readonly ref struct ValueListView<TValue>
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

    public IEnumerable<TValue> AsEnumerable()
    {
        return new Enumerable(_list);
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

    public static implicit operator ValueListView<TValue>(in ValueList<TValue> list)
    {
        return new ValueListView<TValue>(ref Unsafe.AsRef(in list));
    }

    public readonly struct Enumerable : IValueListView<TValue>
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

public readonly ref struct ValueQueueView<TValue>
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

    public IEnumerable<TValue> AsEnumerable()
    {
        return new Enumerable(_queue);
    }

    public ValueEnumerable<ValueQueue<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _queue.AsValueEnumerable();
    }

    public int Count => _queue.Count;

    public static implicit operator ValueQueueView<TValue>(in ValueQueue<TValue> queue)
    {
        return new ValueQueueView<TValue>(ref Unsafe.AsRef(in queue));
    }

    public readonly struct Enumerable : IValueQueueView<TValue>
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

public readonly ref struct ValueStackView<TValue>
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

    public IEnumerable<TValue> AsEnumerable()
    {
        return new Enumerable(_stack);
    }

    public ValueEnumerable<ValueStack<TValue>.Enumerator, TValue> AsValueEnumerable()
    {
        return _stack.AsValueEnumerable();
    }

    public int Count => _stack.Count;

    public static implicit operator ValueStackView<TValue>(in ValueStack<TValue> stack)
    {
        return new ValueStackView<TValue>(ref Unsafe.AsRef(in stack));
    }

    public readonly struct Enumerable : IValueStackView<TValue>
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
}
