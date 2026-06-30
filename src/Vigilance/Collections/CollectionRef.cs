using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ZLinq;

namespace Vigilance.Collections;

public readonly ref struct ValueListRef<T> : IList<T>, IStructEnumerable<ValueList<T>.Enumerator, T>, IReadOnlySpan<T>
{
    private readonly ref ValueList<T> _list;

    public ValueListRef(ref ValueList<T> list)
    {
        _list = ref list;
    }

    public int Count
    {
        get => _list.Count;
        set => _list.Count = value;
    }

    public bool IsReadOnly => _list.IsReadOnly;

    public int Capacity
    {
        get => _list.Capacity;
        set => _list.Capacity = value;
    }

    public ref T this[int index] => ref _list[index];

    T IList<T>.this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public ValueList<T>.Enumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public ValueEnumerable<ValueList<T>.Enumerator, T> AsValueEnumerable()
    {
        return _list.AsValueEnumerable();
    }

    public Span<T> AsSpan()
    {
        return _list.AsSpan();
    }

    public T[] AsArray(out int length)
    {
        return _list.AsArray(out length);
    }

    public ValueListView<T>.Enumerable AsEnumerable()
    {
        return new ValueListView<T>.Enumerable(_list);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueList<T>.Enumerator, T>, T> IStructEnumerable<
        ValueList<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueList<T>.Enumerator, T>(GetEnumerator());
    }

    ReadOnlySpan<T> IReadOnlySpan<T>.AsSpan()
    {
        return AsSpan();
    }

    public void Add(in T item)
    {
        _list.Add(item);
    }

    void ICollection<T>.Add(T item)
    {
        _list.Add(item);
    }

    public void AddRange(IEnumerable<T> collection)
    {
        _list.AddRange(collection);
    }

    public int BinarySearch(int index, int count, in T item, IComparer<T>? comparer)
    {
        return _list.BinarySearch(index, count, item, comparer);
    }

    public int BinarySearch(in T item)
    {
        return _list.BinarySearch(item);
    }

    public int BinarySearch(in T item, IComparer<T>? comparer)
    {
        return _list.BinarySearch(item, comparer);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(in T item)
    {
        return _list.Contains(item);
    }

    bool ICollection<T>.Contains(T item)
    {
        return _list.Contains(item);
    }

    public ValueList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        return _list.ConvertAll(converter);
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        _list.CopyTo(index, array, arrayIndex, count);
    }

    public void CopyTo(T[] array, int arrayIndex = 0)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public int EnsureCapacity(int capacity)
    {
        return _list.EnsureCapacity(capacity);
    }

    public bool Exists(Predicate<T> match)
    {
        return _list.Exists(match);
    }

    public T? Find(Predicate<T> match)
    {
        return _list.Find(match);
    }

    public ValueList<T> FindAll(Predicate<T> match)
    {
        return _list.FindAll(match);
    }

    public int FindIndex(Predicate<T> match)
    {
        return _list.FindIndex(match);
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
        return _list.FindIndex(startIndex, match);
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        return _list.FindIndex(startIndex, count, match);
    }

    public T? FindLast(Predicate<T> match)
    {
        return _list.FindLast(match);
    }

    public int FindLastIndex(Predicate<T> match)
    {
        return _list.FindLastIndex(match);
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return _list.FindLastIndex(startIndex, match);
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        return _list.FindLastIndex(startIndex, count, match);
    }

    public void ForEach(Action<T> action)
    {
        _list.ForEach(action);
    }

    public ValueList<T> GetRange(int index, int count)
    {
        return _list.GetRange(index, count);
    }

    public ValueList<T> Slice(int start, int length)
    {
        return _list.Slice(start, length);
    }

    public int IndexOf(in T item)
    {
        return _list.IndexOf(item);
    }

    int IList<T>.IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public int IndexOf(in T item, int index)
    {
        return _list.IndexOf(item, index);
    }

    public int IndexOf(in T item, int index, int count)
    {
        return _list.IndexOf(item, index, count);
    }

    public void Insert(int index, in T item)
    {
        _list.Insert(index, item);
    }

    void IList<T>.Insert(int index, T item)
    {
        _list.Insert(index, item);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        _list.InsertRange(index, collection);
    }

    public int LastIndexOf(in T item)
    {
        return _list.LastIndexOf(item);
    }

    public int LastIndexOf(in T item, int index)
    {
        return _list.LastIndexOf(item, index);
    }

    public int LastIndexOf(in T item, int index, int count)
    {
        return _list.LastIndexOf(item, index, count);
    }

    public bool Remove(in T item)
    {
        return _list.Remove(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return _list.Remove(item);
    }

    public int RemoveAll(Predicate<T> match)
    {
        return _list.RemoveAll(match);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public void RemoveRange(int index, int count)
    {
        _list.RemoveRange(index, count);
    }

    public void Reverse()
    {
        _list.Reverse();
    }

    public void Reverse(int index, int count)
    {
        _list.Reverse(index, count);
    }

    public void Sort()
    {
        _list.Sort();
    }

    public void Sort(IComparer<T>? comparer)
    {
        _list.Sort(comparer);
    }

    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        _list.Sort(index, count, comparer);
    }

    public void Sort(Comparison<T> comparison)
    {
        _list.Sort(comparison);
    }

    public T[] ToArray()
    {
        return _list.ToArray();
    }

    public void TrimExcess()
    {
        _list.TrimExcess();
    }

    public bool TrueForAll(Predicate<T> match)
    {
        return _list.TrueForAll(match);
    }

    public static implicit operator ValueListView<T>(ValueListRef<T> list)
    {
        return new ValueListView<T>(ref list._list);
    }
}

public readonly ref struct ValueQueueRef<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueQueue<T>.Enumerator, T>
{
    private readonly ref ValueQueue<T> _queue;

    public ValueQueueRef(ref ValueQueue<T> queue)
    {
        _queue = ref queue;
    }

    public int Capacity
    {
        get => _queue.Capacity;
        set => _queue.Capacity = value;
    }

    public int Count => _queue.Count;

    public ValueQueue<T>.Enumerator GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    public ValueEnumerable<ValueQueue<T>.Enumerator, T> AsValueEnumerable()
    {
        return _queue.AsValueEnumerable();
    }

    public ValueQueueView<T>.Enumerable AsEnumerable()
    {
        return new ValueQueueView<T>.Enumerable(_queue);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueQueue<T>.Enumerator, T>, T> IStructEnumerable<
        ValueQueue<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueQueue<T>.Enumerator, T>(GetEnumerator());
    }

    public void Clear()
    {
        _queue.Clear();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _queue.CopyTo(array, arrayIndex);
    }

    public void Enqueue(in T item)
    {
        _queue.Enqueue(item);
    }

    public T Dequeue()
    {
        return _queue.Dequeue();
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        return _queue.TryDequeue(out result);
    }

    public ref T Peek()
    {
        return ref _queue.Peek();
    }

    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        return _queue.TryPeek(out result);
    }

    public bool Contains(in T item)
    {
        return _queue.Contains(item);
    }

    public T[] ToArray()
    {
        return _queue.ToArray();
    }

    public void TrimExcess()
    {
        _queue.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _queue.TrimExcess(capacity);
    }

    public int EnsureCapacity(int capacity)
    {
        return _queue.EnsureCapacity(capacity);
    }

    public static implicit operator ValueQueueView<T>(ValueQueueRef<T> queue)
    {
        return new ValueQueueView<T>(ref queue._queue);
    }
}

public readonly ref struct ValueStackRef<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueStack<T>.Enumerator, T>
{
    private readonly ref ValueStack<T> _stack;

    public ValueStackRef(ref ValueStack<T> stack)
    {
        _stack = ref stack;
    }

    public int Capacity
    {
        get => _stack.Capacity;
        set => _stack.Capacity = value;
    }

    public int Count => _stack.Count;

    public ValueStack<T>.Enumerator GetEnumerator()
    {
        return _stack.GetEnumerator();
    }

    public ValueEnumerable<ValueStack<T>.Enumerator, T> AsValueEnumerable()
    {
        return _stack.AsValueEnumerable();
    }

    public ValueStackView<T>.Enumerable AsEnumerable()
    {
        return new ValueStackView<T>.Enumerable(_stack);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ValueEnumerable<StructEnumerator<ValueStack<T>.Enumerator, T>, T> IStructEnumerable<
        ValueStack<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueStack<T>.Enumerator, T>(GetEnumerator());
    }

    public void Clear()
    {
        _stack.Clear();
    }

    public bool Contains(in T item)
    {
        return _stack.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _stack.CopyTo(array, arrayIndex);
    }

    public void TrimExcess()
    {
        _stack.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _stack.TrimExcess(capacity);
    }

    public ref T Peek()
    {
        return ref _stack.Peek();
    }

    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        return _stack.TryPeek(out result);
    }

    public T Pop()
    {
        return _stack.Pop();
    }

    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        return _stack.TryPop(out result);
    }

    public void Push(in T item)
    {
        _stack.Push(item);
    }

    public int EnsureCapacity(int capacity)
    {
        return _stack.EnsureCapacity(capacity);
    }

    public T[] ToArray()
    {
        return _stack.ToArray();
    }

    public static implicit operator ValueStackView<T>(ValueStackRef<T> stack)
    {
        return new ValueStackView<T>(ref stack._stack);
    }
}

public readonly ref struct ValueDictionaryRef<TKey, TValue>
    : IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly ref ValueDictionary<TKey, TValue> _dictionary;

    public ValueDictionaryRef(ref ValueDictionary<TKey, TValue> dictionary)
    {
        _dictionary = ref dictionary;
    }

    public IEqualityComparer<TKey> Comparer => _dictionary.Comparer;

    public int Count => _dictionary.Count;

    public int Capacity => _dictionary.Capacity;

    public ValueDictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;

    public ValueDictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    public TValue this[in TKey key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public void Add(in TKey key, in TValue value)
    {
        _dictionary.Add(key, value);
    }

    public bool TryAdd(in TKey key, in TValue value)
    {
        return _dictionary.TryAdd(key, value);
    }

    public void Clear()
    {
        _dictionary.Clear();
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

    public bool Remove(in TKey key)
    {
        return _dictionary.Remove(key);
    }

    public bool Remove(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.Remove(key, out value);
    }

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

    public ValueDictionaryView<TKey, TValue>.Enumerable AsEnumerable()
    {
        return new ValueDictionaryView<TKey, TValue>.Enumerable(_dictionary);
    }

    public ref TValue GetValueRefOrNullRef(in TKey key)
    {
        return ref _dictionary.GetValueRefOrNullRef(key);
    }

    public ref TValue? GetValueRefOrAddDefault(in TKey key, out bool exists)
    {
        return ref _dictionary.GetValueRefOrAddDefault(key, out exists);
    }

    public int EnsureCapacity(int capacity)
    {
        return _dictionary.EnsureCapacity(capacity);
    }

    public void TrimExcess()
    {
        _dictionary.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _dictionary.TrimExcess(capacity);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
    {
        _dictionary.Add(keyValuePair.Key, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
    {
        return _dictionary.TryGetValue(keyValuePair.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
    {
        return _dictionary.TryGetValue(keyValuePair.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value)
            && _dictionary.Remove(keyValuePair.Key);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        if ((uint)index > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (array.Length - index < Count)
            throw new ArgumentException("Destination array is not long enough.", nameof(array));
        foreach (var pair in _dictionary)
            array[index++] = pair;
    }

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return _dictionary.Remove(key);
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public static implicit operator ValueDictionaryView<TKey, TValue>(ValueDictionaryRef<TKey, TValue> dictionary)
    {
        return new ValueDictionaryView<TKey, TValue>(ref dictionary._dictionary);
    }
}

public readonly ref struct ValueSparseSetRef<TKey, TValue, TStorage>
    : IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        IStructEnumerable<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    private readonly ref ValueSparseSet<TKey, TValue, TStorage> _sparseSet;

    public ValueSparseSetRef(ref ValueSparseSet<TKey, TValue, TStorage> sparseSet)
    {
        _sparseSet = ref sparseSet;
    }

    public ValueSparseSet<TKey, TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    public int Count => _sparseSet.Count;

    public TValue this[in TKey key]
    {
        get => _sparseSet[key];
        set => _sparseSet[key] = value;
    }

    public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public bool ContainsKey(in TKey key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public bool Remove(in TKey key)
    {
        return _sparseSet.Remove(key);
    }

    public int GetKeyIndex(in TKey key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

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

    public ValueSparseSetView<TKey, TValue, TStorage>.Enumerable AsEnumerable()
    {
        return new ValueSparseSetView<TKey, TValue, TStorage>.Enumerable(_sparseSet);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)_sparseSet).Values;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)_sparseSet).Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys.AsEnumerable();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        if (_sparseSet.ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        _sparseSet[item.Key] = item.Value;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return _sparseSet.TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(array));
        for (var i = 0; i < Count; i++)
            array[arrayIndex + i] = _sparseSet[i];
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return _sparseSet.TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && _sparseSet.Remove(item.Key);
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        if (_sparseSet.ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        _sparseSet[key] = value;
    }

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _sparseSet.ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return _sparseSet.Remove(key);
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _sparseSet.ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public static implicit operator ValueSparseSetView<TKey, TValue, TStorage>(
        ValueSparseSetRef<TKey, TValue, TStorage> sparseSet
    )
    {
        return new ValueSparseSetView<TKey, TValue, TStorage>(ref sparseSet._sparseSet);
    }
}

public static class CollectionRefExtensions
{
    public static ValueListRef<T> AsRef<T>(this ref ValueList<T> list)
    {
        return new ValueListRef<T>(ref list);
    }

    public static ValueQueueRef<T> AsRef<T>(this ref ValueQueue<T> queue)
    {
        return new ValueQueueRef<T>(ref queue);
    }

    public static ValueStackRef<T> AsRef<T>(this ref ValueStack<T> stack)
    {
        return new ValueStackRef<T>(ref stack);
    }

    public static ValueDictionaryRef<TKey, TValue> AsRef<TKey, TValue>(
        this ref ValueDictionary<TKey, TValue> dictionary
    )
        where TKey : notnull
    {
        return new ValueDictionaryRef<TKey, TValue>(ref dictionary);
    }

    public static ValueSparseSetRef<TKey, TValue, TStorage> AsRef<TKey, TValue, TStorage>(
        this ref ValueSparseSet<TKey, TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return new ValueSparseSetRef<TKey, TValue, TStorage>(ref sparseSet);
    }
}
