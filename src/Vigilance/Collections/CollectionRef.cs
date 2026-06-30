using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;

namespace Vigilance.Collections;

public ref struct ValueListRef<T> : IList<T>, IStructEnumerable<ValueList<T>.Enumerator, T>, IReadOnlySpan<T>
{
    private readonly ref ValueList<T> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueList<T> _list;

    public ValueListRef()
    {
        _list = [];
        _ref = ref Unsafe.AsRef(ref _list);
    }

    public ValueListRef(ref ValueList<T> list)
    {
        _ref = ref list;
    }

    public int Count
    {
        readonly get => _ref.Count;
        set => _ref.Count = value;
    }

    public readonly bool IsReadOnly => _ref.IsReadOnly;

    public int Capacity
    {
        readonly get => _ref.Capacity;
        set => _ref.Capacity = value;
    }

    public readonly ref T this[int index] => ref _ref[index];

    T IList<T>.this[int index]
    {
        get => _ref[index];
        set => _ref[index] = value;
    }

    public readonly ValueList<T>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueList<T>.Enumerator, T> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    public readonly Span<T> AsSpan()
    {
        return _ref.AsSpan();
    }

    public readonly T[] AsArray(out int length)
    {
        return _ref.AsArray(out length);
    }

    public readonly ValueListView<T>.Enumerable AsEnumerable()
    {
        return new ValueListView<T>.Enumerable(_ref);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ValueEnumerable<StructEnumerator<ValueList<T>.Enumerator, T>, T> IStructEnumerable<
        ValueList<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueList<T>.Enumerator, T>(GetEnumerator());
    }

    readonly ReadOnlySpan<T> IReadOnlySpan<T>.AsSpan()
    {
        return AsSpan();
    }

    public void Add(in T item)
    {
        _ref.Add(item);
    }

    void ICollection<T>.Add(T item)
    {
        _ref.Add(item);
    }

    public void AddRange(IEnumerable<T> collection)
    {
        _ref.AddRange(collection);
    }

    public void AddRange(in ValueList<T> list)
    {
        _ref.AddRange(list);
    }

    [OverloadResolutionPriority(1)]
    public void AddRange(in ReadOnlySpan<T> span)
    {
        _ref.AddRange(span);
    }

    public readonly int BinarySearch(int index, int count, in T item, IComparer<T>? comparer)
    {
        return _ref.BinarySearch(index, count, item, comparer);
    }

    public readonly int BinarySearch(in T item)
    {
        return _ref.BinarySearch(item);
    }

    public readonly int BinarySearch(in T item, IComparer<T>? comparer)
    {
        return _ref.BinarySearch(item, comparer);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(in T item)
    {
        return _ref.Contains(item);
    }

    readonly bool ICollection<T>.Contains(T item)
    {
        return _ref.Contains(item);
    }

    public readonly ValueList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        return _ref.ConvertAll(converter);
    }

    public readonly void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        _ref.CopyTo(index, array, arrayIndex, count);
    }

    public readonly void CopyTo(T[] array, int arrayIndex = 0)
    {
        _ref.CopyTo(array, arrayIndex);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueList<T> list)
    {
        _ref.CopyTo(ref list);
    }

    public int EnsureCapacity(int capacity)
    {
        return _ref.EnsureCapacity(capacity);
    }

    public readonly bool Exists(Predicate<T> match)
    {
        return _ref.Exists(match);
    }

    public readonly T? Find(Predicate<T> match)
    {
        return _ref.Find(match);
    }

    public readonly ValueList<T> FindAll(Predicate<T> match)
    {
        return _ref.FindAll(match);
    }

    public readonly int FindIndex(Predicate<T> match)
    {
        return _ref.FindIndex(match);
    }

    public readonly int FindIndex(int startIndex, Predicate<T> match)
    {
        return _ref.FindIndex(startIndex, match);
    }

    public readonly int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        return _ref.FindIndex(startIndex, count, match);
    }

    public readonly T? FindLast(Predicate<T> match)
    {
        return _ref.FindLast(match);
    }

    public readonly int FindLastIndex(Predicate<T> match)
    {
        return _ref.FindLastIndex(match);
    }

    public readonly int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return _ref.FindLastIndex(startIndex, match);
    }

    public readonly int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        return _ref.FindLastIndex(startIndex, count, match);
    }

    public readonly void ForEach(Action<T> action)
    {
        _ref.ForEach(action);
    }

    public readonly ValueList<T> GetRange(int index, int count)
    {
        return _ref.GetRange(index, count);
    }

    public readonly ValueList<T> Slice(int start, int length)
    {
        return _ref.Slice(start, length);
    }

    public readonly int IndexOf(in T item)
    {
        return _ref.IndexOf(item);
    }

    readonly int IList<T>.IndexOf(T item)
    {
        return _ref.IndexOf(item);
    }

    public readonly int IndexOf(in T item, int index)
    {
        return _ref.IndexOf(item, index);
    }

    public readonly int IndexOf(in T item, int index, int count)
    {
        return _ref.IndexOf(item, index, count);
    }

    public void Insert(int index, in T item)
    {
        _ref.Insert(index, item);
    }

    void IList<T>.Insert(int index, T item)
    {
        _ref.Insert(index, item);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        _ref.InsertRange(index, collection);
    }

    public readonly int LastIndexOf(in T item)
    {
        return _ref.LastIndexOf(item);
    }

    public readonly int LastIndexOf(in T item, int index)
    {
        return _ref.LastIndexOf(item, index);
    }

    public readonly int LastIndexOf(in T item, int index, int count)
    {
        return _ref.LastIndexOf(item, index, count);
    }

    public bool Remove(in T item)
    {
        return _ref.Remove(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return _ref.Remove(item);
    }

    public int RemoveAll(Predicate<T> match)
    {
        return _ref.RemoveAll(match);
    }

    public int RemoveAll(in ValueList<T> list)
    {
        return _ref.RemoveAll(list);
    }

    public int RemoveAll(in ReadOnlySpan<T> span)
    {
        return _ref.RemoveAll(span);
    }

    public void RemoveAt(int index)
    {
        _ref.RemoveAt(index);
    }

    public void RemoveRange(int index, int count)
    {
        _ref.RemoveRange(index, count);
    }

    public void Reverse()
    {
        _ref.Reverse();
    }

    public void Reverse(int index, int count)
    {
        _ref.Reverse(index, count);
    }

    public void Sort()
    {
        _ref.Sort();
    }

    public void Sort(IComparer<T>? comparer)
    {
        _ref.Sort(comparer);
    }

    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        _ref.Sort(index, count, comparer);
    }

    public void Sort(Comparison<T> comparison)
    {
        _ref.Sort(comparison);
    }

    public readonly T[] ToArray()
    {
        return _ref.ToArray();
    }

    public void TrimExcess()
    {
        _ref.TrimExcess();
    }

    public readonly bool TrueForAll(Predicate<T> match)
    {
        return _ref.TrueForAll(match);
    }

    public static implicit operator ValueListView<T>(in ValueListRef<T> list)
    {
        return new ValueListView<T>(ref list._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueListRef<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueListRef<>)} is not supported.");
    }

    public readonly bool Equals(in ValueListRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(in ValueListRef<T> left, in ValueListRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in ValueListRef<T> left, in ValueListRef<T> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueQueueRef<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueQueue<T>.Enumerator, T>
{
    private readonly ref ValueQueue<T> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueQueue<T> _queue;

    public ValueQueueRef()
    {
        _queue = [];
        _ref = ref Unsafe.AsRef(ref _queue);
    }

    public ValueQueueRef(ref ValueQueue<T> queue)
    {
        _ref = ref queue;
    }

    public int Capacity
    {
        readonly get => _ref.Capacity;
        set => _ref.Capacity = value;
    }

    public readonly int Count => _ref.Count;

    public readonly ValueQueue<T>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueQueue<T>.Enumerator, T> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    public readonly ValueQueueView<T>.Enumerable AsEnumerable()
    {
        return new ValueQueueView<T>.Enumerable(_ref);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ValueEnumerable<StructEnumerator<ValueQueue<T>.Enumerator, T>, T> IStructEnumerable<
        ValueQueue<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueQueue<T>.Enumerator, T>(GetEnumerator());
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        _ref.CopyTo(array, arrayIndex);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueQueue<T> queue)
    {
        _ref.CopyTo(ref queue);
    }

    public void Enqueue(in T item)
    {
        _ref.Enqueue(item);
    }

    public T Dequeue()
    {
        return _ref.Dequeue();
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        return _ref.TryDequeue(out result);
    }

    public readonly ref T Peek()
    {
        return ref _ref.Peek();
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        return _ref.TryPeek(out result);
    }

    public readonly bool Contains(in T item)
    {
        return _ref.Contains(item);
    }

    public readonly T[] ToArray()
    {
        return _ref.ToArray();
    }

    public void TrimExcess()
    {
        _ref.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _ref.TrimExcess(capacity);
    }

    public int EnsureCapacity(int capacity)
    {
        return _ref.EnsureCapacity(capacity);
    }

    public static implicit operator ValueQueueView<T>(ValueQueueRef<T> queue)
    {
        return new ValueQueueView<T>(ref queue._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueQueueRef<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueQueueRef<>)} is not supported.");
    }

    public readonly bool Equals(in ValueQueueRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(in ValueQueueRef<T> left, in ValueQueueRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in ValueQueueRef<T> left, in ValueQueueRef<T> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueStackRef<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueStack<T>.Enumerator, T>
{
    private readonly ref ValueStack<T> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueStack<T> _stack;

    public ValueStackRef()
    {
        _stack = [];
        _ref = ref Unsafe.AsRef(ref _stack);
    }

    public ValueStackRef(ref ValueStack<T> stack)
    {
        _ref = ref stack;
    }

    public int Capacity
    {
        readonly get => _ref.Capacity;
        set => _ref.Capacity = value;
    }

    public readonly int Count => _ref.Count;

    public readonly ValueStack<T>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueStack<T>.Enumerator, T> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    public readonly ValueStackView<T>.Enumerable AsEnumerable()
    {
        return new ValueStackView<T>.Enumerable(_ref);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ValueEnumerable<StructEnumerator<ValueStack<T>.Enumerator, T>, T> IStructEnumerable<
        ValueStack<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueStack<T>.Enumerator, T>(GetEnumerator());
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(in T item)
    {
        return _ref.Contains(item);
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        _ref.CopyTo(array, arrayIndex);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueStack<T> stack)
    {
        _ref.CopyTo(ref stack);
    }

    public void TrimExcess()
    {
        _ref.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _ref.TrimExcess(capacity);
    }

    public readonly ref T Peek()
    {
        return ref _ref.Peek();
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        return _ref.TryPeek(out result);
    }

    public T Pop()
    {
        return _ref.Pop();
    }

    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        return _ref.TryPop(out result);
    }

    public void Push(in T item)
    {
        _ref.Push(item);
    }

    public int EnsureCapacity(int capacity)
    {
        return _ref.EnsureCapacity(capacity);
    }

    public readonly T[] ToArray()
    {
        return _ref.ToArray();
    }

    public static implicit operator ValueStackView<T>(ValueStackRef<T> stack)
    {
        return new ValueStackView<T>(ref stack._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueStackRef<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueStackRef<>)} is not supported.");
    }

    public readonly bool Equals(in ValueStackRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(in ValueStackRef<T> left, in ValueStackRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in ValueStackRef<T> left, in ValueStackRef<T> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueDictionaryRef<TKey, TValue>
    : IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly ref ValueDictionary<TKey, TValue> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueDictionary<TKey, TValue> _dictionary;

    public ValueDictionaryRef()
    {
        _dictionary = [];
        _ref = ref Unsafe.AsRef(ref _dictionary);
    }

    public ValueDictionaryRef(ref ValueDictionary<TKey, TValue> dictionary)
    {
        _ref = ref dictionary;
    }

    public readonly IEqualityComparer<TKey> Comparer => _ref.Comparer;

    public readonly int Count => _ref.Count;

    public readonly int Capacity => _ref.Capacity;

    public readonly ValueDictionary<TKey, TValue>.KeyCollection Keys => _ref.Keys;

    public readonly ValueDictionary<TKey, TValue>.ValueCollection Values => _ref.Values;

    public TValue this[in TKey key]
    {
        readonly get => _ref[key];
        set => _ref[key] = value;
    }

    public void Add(in TKey key, in TValue value)
    {
        _ref.Add(key, value);
    }

    public bool TryAdd(in TKey key, in TValue value)
    {
        return _ref.TryAdd(key, value);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool ContainsKey(in TKey key)
    {
        return _ref.ContainsKey(key);
    }

    public readonly bool ContainsValue(in TValue value)
    {
        return _ref.ContainsValue(value);
    }

    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public bool Remove(in TKey key)
    {
        return _ref.Remove(key);
    }

    public bool Remove(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.Remove(key, out value);
    }

    public readonly ValueDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<
        StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    public readonly ValueDictionaryView<TKey, TValue>.Enumerable AsEnumerable()
    {
        return new ValueDictionaryView<TKey, TValue>.Enumerable(_ref);
    }

    public readonly void CopyTo(scoped in Span<KeyValuePair<TKey, TValue>> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueDictionary<TKey, TValue> dictionary)
    {
        _ref.CopyTo(ref dictionary);
    }

    public ref TValue GetValueRefOrNullRef(in TKey key)
    {
        return ref _ref.GetValueRefOrNullRef(key);
    }

    public ref TValue? GetValueRefOrAddDefault(in TKey key, out bool exists)
    {
        return ref _ref.GetValueRefOrAddDefault(key, out exists);
    }

    public int EnsureCapacity(int capacity)
    {
        return _ref.EnsureCapacity(capacity);
    }

    public void TrimExcess()
    {
        _ref.TrimExcess();
    }

    public void TrimExcess(int capacity)
    {
        _ref.TrimExcess(capacity);
    }

    readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        readonly get => this[key];
        set => this[key] = value;
    }

    readonly TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    readonly ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        _ref.Add(key, value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
    {
        _ref.Add(keyValuePair.Key, keyValuePair.Value);
    }

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
    {
        return _ref.TryGetValue(keyValuePair.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
    {
        return _ref.TryGetValue(keyValuePair.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value)
            && _ref.Remove(keyValuePair.Key);
    }

    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        if ((uint)index > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (array.Length - index < Count)
            throw new ArgumentException("Destination array is not long enough.", nameof(array));
        foreach (var pair in _ref)
            array[index++] = pair;
    }

    readonly bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _ref.ContainsKey(key);
    }

    readonly bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _ref.ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return _ref.Remove(key);
    }

    readonly bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    readonly bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public static implicit operator ValueDictionaryView<TKey, TValue>(ValueDictionaryRef<TKey, TValue> dictionary)
    {
        return new ValueDictionaryView<TKey, TValue>(ref dictionary._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueDictionaryRef<,>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueDictionaryRef<,>)} is not supported."
        );
    }

    public readonly bool Equals(in ValueDictionaryRef<TKey, TValue> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(in ValueDictionaryRef<TKey, TValue> left, in ValueDictionaryRef<TKey, TValue> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(in ValueDictionaryRef<TKey, TValue> left, in ValueDictionaryRef<TKey, TValue> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueSparseSetRef<TKey, TValue, TStorage>
    : IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        IStructEnumerable<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    private readonly ref ValueSparseSet<TKey, TValue, TStorage> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueSparseSet<TKey, TValue, TStorage> _sparseSet;

    public ValueSparseSetRef(
        in TStorage storage,
        Func<TKey, int> keyIndexFunc,
        int sparseChunkSize = ValueSparseSet<TKey, TValue, TStorage>.DefaultSparseChunkSize
    )
    {
        _sparseSet = new ValueSparseSet<TKey, TValue, TStorage>(storage, keyIndexFunc, sparseChunkSize);
        _ref = ref Unsafe.AsRef(ref _sparseSet);
    }

    public ValueSparseSetRef(ref ValueSparseSet<TKey, TValue, TStorage> sparseSet)
    {
        _ref = ref sparseSet;
    }

    public readonly ValueSparseSet<TKey, TValue, TStorage>.ValueEnumerable Values => _ref.Values;

    public readonly ValueListView<TKey> Keys => _ref.Keys;

    public readonly int Count => _ref.Count;

    public TValue this[in TKey key]
    {
        readonly get => _ref[key];
        set => _ref[key] = value;
    }

    public readonly KeyValuePair<TKey, TValue> this[int index] => _ref[index];

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool ContainsKey(in TKey key)
    {
        return _ref.ContainsKey(key);
    }

    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public bool Remove(in TKey key)
    {
        return _ref.Remove(key);
    }

    public readonly int GetKeyIndex(in TKey key)
    {
        return _ref.GetKeyIndex(key);
    }

    public readonly ValueSparseSet<TKey, TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<
        StructEnumerator<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    public readonly ValueSparseSetView<TKey, TValue, TStorage>.Enumerable AsEnumerable()
    {
        return new ValueSparseSetView<TKey, TValue, TStorage>.Enumerable(_ref);
    }

    public readonly void CopyTo(scoped in Span<KeyValuePair<TKey, TValue>> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueSparseSet<TKey, TValue, TStorage> sparseSet)
    {
        _ref.CopyTo(ref sparseSet);
    }

    readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ICollection<TValue> IDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)_ref).Values;

    readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)_ref).Keys;

    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys.AsEnumerable();

    readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        readonly get => this[key];
        set => this[key] = value;
    }

    readonly TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        if (_ref.ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        _ref[item.Key] = item.Value;
    }

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return _ref.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(array));
        for (var i = 0; i < Count; i++)
            array[arrayIndex + i] = _ref[i];
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return _ref.TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && _ref.Remove(item.Key);
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        if (_ref.ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        _ref[key] = value;
    }

    readonly bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _ref.ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return _ref.Remove(key);
    }

    readonly bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    readonly bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _ref.ContainsKey(key);
    }

    readonly bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public static implicit operator ValueSparseSetView<TKey, TValue, TStorage>(
        ValueSparseSetRef<TKey, TValue, TStorage> sparseSet
    )
    {
        return new ValueSparseSetView<TKey, TValue, TStorage>(ref sparseSet._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueSparseSetRef<,,>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueSparseSetRef<,,>)} is not supported."
        );
    }

    public readonly bool Equals(in ValueSparseSetRef<TKey, TValue, TStorage> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(
        in ValueSparseSetRef<TKey, TValue, TStorage> left,
        in ValueSparseSetRef<TKey, TValue, TStorage> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        in ValueSparseSetRef<TKey, TValue, TStorage> left,
        in ValueSparseSetRef<TKey, TValue, TStorage> right
    )
    {
        return !left.Equals(right);
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
