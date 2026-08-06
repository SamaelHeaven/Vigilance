using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

    public void Add(scoped in T item)
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

    public void AddRange(scoped in ValueList<T> list)
    {
        _ref.AddRange(list);
    }

    [OverloadResolutionPriority(1)]
    public void AddRange(scoped in ReadOnlySpan<T> span)
    {
        _ref.AddRange(span);
    }

    public readonly int BinarySearch(int index, int count, scoped in T item, IComparer<T>? comparer)
    {
        return _ref.BinarySearch(index, count, item, comparer);
    }

    public readonly int BinarySearch(scoped in T item)
    {
        return _ref.BinarySearch(item);
    }

    public readonly int BinarySearch(scoped in T item, IComparer<T>? comparer)
    {
        return _ref.BinarySearch(item, comparer);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(scoped in T item)
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

    public readonly int IndexOf(scoped in T item)
    {
        return _ref.IndexOf(item);
    }

    readonly int IList<T>.IndexOf(T item)
    {
        return _ref.IndexOf(item);
    }

    public readonly int IndexOf(scoped in T item, int index)
    {
        return _ref.IndexOf(item, index);
    }

    public readonly int IndexOf(scoped in T item, int index, int count)
    {
        return _ref.IndexOf(item, index, count);
    }

    public void Insert(int index, scoped in T item)
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

    public readonly int LastIndexOf(scoped in T item)
    {
        return _ref.LastIndexOf(item);
    }

    public readonly int LastIndexOf(scoped in T item, int index)
    {
        return _ref.LastIndexOf(item, index);
    }

    public readonly int LastIndexOf(scoped in T item, int index, int count)
    {
        return _ref.LastIndexOf(item, index, count);
    }

    public bool Remove(scoped in T item)
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

    public int RemoveAll(scoped in ValueList<T> list)
    {
        return _ref.RemoveAll(list);
    }

    public int RemoveAll(scoped in ReadOnlySpan<T> span)
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
        // ReSharper disable once UseCollectionExpression
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

    public readonly bool Equals(scoped in ValueListRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueListRef<T> left, scoped in ValueListRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueListRef<T> left, scoped in ValueListRef<T> right)
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

    public void Enqueue(scoped in T item)
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

    public readonly bool Contains(scoped in T item)
    {
        return _ref.Contains(item);
    }

    public readonly T[] ToArray()
    {
        // ReSharper disable once UseCollectionExpression
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

    public readonly bool Equals(scoped in ValueQueueRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueQueueRef<T> left, scoped in ValueQueueRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueQueueRef<T> left, scoped in ValueQueueRef<T> right)
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

    public readonly bool Contains(scoped in T item)
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

    public void Push(scoped in T item)
    {
        _ref.Push(item);
    }

    public int EnsureCapacity(int capacity)
    {
        return _ref.EnsureCapacity(capacity);
    }

    public readonly T[] ToArray()
    {
        // ReSharper disable once UseCollectionExpression
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

    public readonly bool Equals(scoped in ValueStackRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueStackRef<T> left, scoped in ValueStackRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueStackRef<T> left, scoped in ValueStackRef<T> right)
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

    public TValue this[scoped in TKey key]
    {
        readonly get => _ref[key];
        set => _ref[key] = value;
    }

    public void Add(scoped in TKey key, scoped in TValue value)
    {
        _ref.Add(key, value);
    }

    public bool TryAdd(scoped in TKey key, scoped in TValue value)
    {
        return _ref.TryAdd(key, value);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool ContainsKey(scoped in TKey key)
    {
        return _ref.ContainsKey(key);
    }

    public readonly bool ContainsValue(scoped in TValue value)
    {
        return _ref.ContainsValue(value);
    }

    public readonly bool TryGetValue(scoped in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public bool Remove(scoped in TKey key)
    {
        return _ref.Remove(key);
    }

    public bool Remove(scoped in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.Remove(key, out value);
    }

    public readonly ValueDictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<
        ValueDictionary<TKey, TValue>.Enumerator,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<
        StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>(
            GetEnumerator()
        );
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

    public readonly ref TValue GetValueRefOrNullRef(in TKey key)
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

    public readonly bool Equals(scoped in ValueDictionaryRef<TKey, TValue> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(
        scoped in ValueDictionaryRef<TKey, TValue> left,
        scoped in ValueDictionaryRef<TKey, TValue> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        scoped in ValueDictionaryRef<TKey, TValue> left,
        scoped in ValueDictionaryRef<TKey, TValue> right
    )
    {
        return !left.Equals(right);
    }
}

public ref struct ValueHashSetRef<T> : ISet<T>, IReadOnlySet<T>, IStructEnumerable<ValueHashSet<T>.Enumerator, T>
{
    private readonly ref ValueHashSet<T> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueHashSet<T> _hashSet;

    public ValueHashSetRef()
    {
        _hashSet = [];
        _ref = ref Unsafe.AsRef(ref _hashSet);
    }

    public ValueHashSetRef(ref ValueHashSet<T> hashSet)
    {
        _ref = ref hashSet;
    }

    public readonly IEqualityComparer<T> Comparer => _ref.Comparer;

    public readonly int Count => _ref.Count;

    public readonly int Capacity => _ref.Capacity;

    readonly bool ICollection<T>.IsReadOnly => false;

    public bool Add(scoped in T item)
    {
        return _ref.Add(item);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(scoped in T item)
    {
        return _ref.Contains(item);
    }

    public bool Remove(scoped in T item)
    {
        return _ref.Remove(item);
    }

    public readonly bool TryGetValue(scoped in T equalValue, [MaybeNullWhen(false)] out T actualValue)
    {
        return _ref.TryGetValue(equalValue, out actualValue);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        _ref.UnionWith(other);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        _ref.IntersectWith(other);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        _ref.ExceptWith(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        _ref.SymmetricExceptWith(other);
    }

    public readonly bool IsSubsetOf(IEnumerable<T> other)
    {
        return _ref.IsSubsetOf(other);
    }

    public readonly bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _ref.IsProperSubsetOf(other);
    }

    public readonly bool IsSupersetOf(IEnumerable<T> other)
    {
        return _ref.IsSupersetOf(other);
    }

    public readonly bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _ref.IsProperSupersetOf(other);
    }

    public readonly bool Overlaps(IEnumerable<T> other)
    {
        return _ref.Overlaps(other);
    }

    public readonly bool SetEquals(IEnumerable<T> other)
    {
        return _ref.SetEquals(other);
    }

    public int RemoveWhere(Predicate<T> match)
    {
        return _ref.RemoveWhere(match);
    }

    public readonly void CopyTo(T[] array)
    {
        _ref.CopyTo(array);
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        _ref.CopyTo(array, arrayIndex);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex, int count)
    {
        _ref.CopyTo(span, arrayIndex, count);
    }

    public readonly void CopyTo(scoped ref ValueHashSet<T> hashSet)
    {
        _ref.CopyTo(ref hashSet);
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

    public readonly ValueHashSet<T>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueHashSet<T>.Enumerator, T> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<StructEnumerator<ValueHashSet<T>.Enumerator, T>, T> IStructEnumerable<
        ValueHashSet<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueHashSet<T>.Enumerator, T>(GetEnumerator());
    }

    public readonly ValueHashSetView<T>.Enumerable AsEnumerable()
    {
        return new ValueHashSetView<T>.Enumerable(_ref);
    }

    void ICollection<T>.Add(T item)
    {
        _ref.Add(item);
    }

    bool ISet<T>.Add(T item)
    {
        return _ref.Add(item);
    }

    readonly bool ICollection<T>.Contains(T item)
    {
        return _ref.Contains(item);
    }

    readonly bool IReadOnlySet<T>.Contains(T item)
    {
        return _ref.Contains(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return _ref.Remove(item);
    }

    readonly void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        _ref.CopyTo(array, arrayIndex);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator ValueHashSetView<T>(ValueHashSetRef<T> hashSet)
    {
        return new ValueHashSetView<T>(ref hashSet._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueHashSetRef<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueHashSetRef<>)} is not supported.");
    }

    public readonly bool Equals(scoped in ValueHashSetRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueHashSetRef<T> left, scoped in ValueHashSetRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueHashSetRef<T> left, scoped in ValueHashSetRef<T> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueSparseSetRef<TKey, TValue, TStorage>
    : ISparseSet<TKey, TValue, TStorage>,
        IDictionary<TKey, TValue>,
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

    public readonly ISparseSet<TValue, TStorage>.ValueEnumerable Values => _ref.Values;

    public readonly ValueListView<TKey> Keys => _ref.Keys;

    public readonly int Count => _ref.Count;

    public TValue this[scoped in TKey key]
    {
        readonly get => _ref[key];
        set => _ref[key] = value;
    }

    public readonly KeyValuePair<TKey, TValue> this[int index] => _ref[index];

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool ContainsKey(scoped in TKey key)
    {
        return _ref.ContainsKey(key);
    }

    public readonly bool TryGetValue(scoped in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public readonly TValue? GetValueOrDefault(scoped in TKey key)
    {
        return _ref.GetValueOrDefault(key);
    }

    public readonly TValue GetValueOrDefault(scoped in TKey key, in TValue defaultValue)
    {
        return _ref.GetValueOrDefault(key, defaultValue);
    }

    public bool Remove(scoped in TKey key)
    {
        return _ref.Remove(key);
    }

    public readonly int GetKeyIndex(scoped in TKey key)
    {
        return _ref.GetKeyIndex(key);
    }

    public readonly ValueSparseSet<TKey, TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<
        ValueSparseSet<TKey, TValue, TStorage>.Enumerator,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<
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

    public readonly bool Equals(scoped in ValueSparseSetRef<TKey, TValue, TStorage> other)
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

public ref struct ValueSparseSetRef<T>
    : ISparseSet<T>,
        ISet<T>,
        IReadOnlySet<T>,
        IReadOnlyList<T>,
        IStructEnumerable<ValueSparseSet<T>.Enumerator, T>
{
    private readonly ref ValueSparseSet<T> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueSparseSet<T> _sparseSet;

    public ValueSparseSetRef(Func<T, int> keyIndexFunc, int sparseChunkSize = ValueSparseSet<T>.DefaultSparseChunkSize)
    {
        _sparseSet = new ValueSparseSet<T>(keyIndexFunc, sparseChunkSize);
        _ref = ref Unsafe.AsRef(ref _sparseSet);
    }

    public ValueSparseSetRef(ref ValueSparseSet<T> sparseSet)
    {
        _ref = ref sparseSet;
    }

    public readonly ValueListView<T> Keys => _ref.Keys;

    public readonly int Count => _ref.Count;

    public readonly T this[int index] => _ref[index];

    readonly bool ICollection<T>.IsReadOnly => false;

    public bool Add(scoped in T key)
    {
        return _ref.Add(key);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(scoped in T key)
    {
        return _ref.Contains(key);
    }

    public bool Remove(scoped in T key)
    {
        return _ref.Remove(key);
    }

    public readonly int GetKeyIndex(scoped in T key)
    {
        return _ref.GetKeyIndex(key);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        _ref.UnionWith(other);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        _ref.IntersectWith(other);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        _ref.ExceptWith(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        _ref.SymmetricExceptWith(other);
    }

    public readonly bool IsSubsetOf(IEnumerable<T> other)
    {
        return _ref.IsSubsetOf(other);
    }

    public readonly bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _ref.IsProperSubsetOf(other);
    }

    public readonly bool IsSupersetOf(IEnumerable<T> other)
    {
        return _ref.IsSupersetOf(other);
    }

    public readonly bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _ref.IsProperSupersetOf(other);
    }

    public readonly bool Overlaps(IEnumerable<T> other)
    {
        return _ref.Overlaps(other);
    }

    public readonly bool SetEquals(IEnumerable<T> other)
    {
        return _ref.SetEquals(other);
    }

    public readonly void CopyTo(T[] array)
    {
        _ref.CopyTo(array);
    }

    public readonly void CopyTo(scoped in Span<T> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueSparseSet<T> sparseSet)
    {
        _ref.CopyTo(ref sparseSet);
    }

    public readonly ValueSparseSet<T>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueSparseSet<T>.Enumerator, T> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<StructEnumerator<ValueSparseSet<T>.Enumerator, T>, T> IStructEnumerable<
        ValueSparseSet<T>.Enumerator,
        T
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueSparseSet<T>.Enumerator, T>(GetEnumerator());
    }

    public readonly ValueSparseSetView<T>.Enumerable AsEnumerable()
    {
        return new ValueSparseSetView<T>.Enumerable(_ref);
    }

    void ICollection<T>.Add(T item)
    {
        _ref.Add(item);
    }

    bool ISet<T>.Add(T item)
    {
        return _ref.Add(item);
    }

    readonly bool ICollection<T>.Contains(T item)
    {
        return _ref.Contains(item);
    }

    readonly bool IReadOnlySet<T>.Contains(T item)
    {
        return _ref.Contains(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return _ref.Remove(item);
    }

    readonly void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        _ref.CopyTo(array.AsSpan(), arrayIndex);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator ValueSparseSetView<T>(ValueSparseSetRef<T> sparseSet)
    {
        return new ValueSparseSetView<T>(ref sparseSet._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueSparseSetRef<>)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException($"{nameof(GetHashCode)}() on {nameof(ValueSparseSetRef<>)} is not supported.");
    }

    public readonly bool Equals(scoped in ValueSparseSetRef<T> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueSparseSetRef<T> left, scoped in ValueSparseSetRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueSparseSetRef<T> left, scoped in ValueSparseSetRef<T> right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueEntitySparseSetRef
    : ISparseSet<Entity>,
        ISet<Entity>,
        IReadOnlySet<Entity>,
        IReadOnlyList<Entity>,
        IStructEnumerable<ValueEntitySparseSet.Enumerator, Entity>
{
    private readonly ref ValueEntitySparseSet _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueEntitySparseSet _sparseSet;

    public ValueEntitySparseSetRef(Scene scene, int sparseChunkSize = ValueEntitySparseSet.DefaultSparseChunkSize)
    {
        _sparseSet = new ValueEntitySparseSet(scene, sparseChunkSize);
        _ref = ref Unsafe.AsRef(ref _sparseSet);
    }

    public ValueEntitySparseSetRef(ref ValueEntitySparseSet sparseSet)
    {
        _ref = ref sparseSet;
    }

    public readonly Scene Scene => _ref.Scene;

    public readonly int Count => _ref.Count;

    public readonly Entity this[int index] => _ref[index];

    readonly bool ICollection<Entity>.IsReadOnly => false;

    public bool Add(scoped in Entity key)
    {
        return _ref.Add(key);
    }

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool Contains(scoped in Entity key)
    {
        return _ref.Contains(key);
    }

    public bool Remove(scoped in Entity key)
    {
        return _ref.Remove(key);
    }

    public readonly int GetKeyIndex(scoped in Entity key)
    {
        return _ref.GetKeyIndex(key);
    }

    public void UnionWith(IEnumerable<Entity> other)
    {
        _ref.UnionWith(other);
    }

    public void IntersectWith(IEnumerable<Entity> other)
    {
        _ref.IntersectWith(other);
    }

    public void ExceptWith(IEnumerable<Entity> other)
    {
        _ref.ExceptWith(other);
    }

    public void SymmetricExceptWith(IEnumerable<Entity> other)
    {
        _ref.SymmetricExceptWith(other);
    }

    public readonly bool IsSubsetOf(IEnumerable<Entity> other)
    {
        return _ref.IsSubsetOf(other);
    }

    public readonly bool IsProperSubsetOf(IEnumerable<Entity> other)
    {
        return _ref.IsProperSubsetOf(other);
    }

    public readonly bool IsSupersetOf(IEnumerable<Entity> other)
    {
        return _ref.IsSupersetOf(other);
    }

    public readonly bool IsProperSupersetOf(IEnumerable<Entity> other)
    {
        return _ref.IsProperSupersetOf(other);
    }

    public readonly bool Overlaps(IEnumerable<Entity> other)
    {
        return _ref.Overlaps(other);
    }

    public readonly bool SetEquals(IEnumerable<Entity> other)
    {
        return _ref.SetEquals(other);
    }

    public readonly void CopyTo(Entity[] array)
    {
        _ref.CopyTo(array);
    }

    public readonly void CopyTo(scoped in Span<Entity> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueEntitySparseSet sparseSet)
    {
        _ref.CopyTo(ref sparseSet);
    }

    public readonly ValueEntitySparseSet.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<ValueEntitySparseSet.Enumerator, Entity> AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>, Entity> IStructEnumerable<
        ValueEntitySparseSet.Enumerator,
        Entity
    >.AsValueEnumerable()
    {
        return new StructEnumerator<ValueEntitySparseSet.Enumerator, Entity>(GetEnumerator());
    }

    public readonly ValueEntitySparseSetView.Enumerable AsEnumerable()
    {
        return new ValueEntitySparseSetView.Enumerable(_ref);
    }

    void ICollection<Entity>.Add(Entity item)
    {
        _ref.Add(item);
    }

    bool ISet<Entity>.Add(Entity item)
    {
        return _ref.Add(item);
    }

    readonly bool ICollection<Entity>.Contains(Entity item)
    {
        return _ref.Contains(item);
    }

    readonly bool IReadOnlySet<Entity>.Contains(Entity item)
    {
        return _ref.Contains(item);
    }

    bool ICollection<Entity>.Remove(Entity item)
    {
        return _ref.Remove(item);
    }

    readonly void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        _ref.CopyTo(array.AsSpan(), arrayIndex);
    }

    readonly IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator ValueEntitySparseSetView(ValueEntitySparseSetRef sparseSet)
    {
        return new ValueEntitySparseSetView(ref sparseSet._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException($"{nameof(Equals)}() on {nameof(ValueEntitySparseSetRef)} is not supported.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueEntitySparseSetRef)} is not supported."
        );
    }

    public readonly bool Equals(scoped in ValueEntitySparseSetRef other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(scoped in ValueEntitySparseSetRef left, scoped in ValueEntitySparseSetRef right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(scoped in ValueEntitySparseSetRef left, scoped in ValueEntitySparseSetRef right)
    {
        return !left.Equals(right);
    }
}

public ref struct ValueEntitySparseSetRef<TValue, TStorage>
    : ISparseSet<Entity, TValue, TStorage>,
        IDictionary<Entity, TValue>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>,
        IStructEnumerable<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    private readonly ref ValueEntitySparseSet<TValue, TStorage> _ref;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private ValueEntitySparseSet<TValue, TStorage> _sparseSet;

    public ValueEntitySparseSetRef(
        Scene scene,
        in TStorage storage,
        int sparseChunkSize = ValueEntitySparseSet<TValue, TStorage>.DefaultSparseChunkSize
    )
    {
        _sparseSet = new ValueEntitySparseSet<TValue, TStorage>(scene, storage, sparseChunkSize);
        _ref = ref Unsafe.AsRef(ref _sparseSet);
    }

    public ValueEntitySparseSetRef(ref ValueEntitySparseSet<TValue, TStorage> sparseSet)
    {
        _ref = ref sparseSet;
    }

    public readonly Scene Scene => _ref.Scene;

    public readonly ISparseSet<TValue, TStorage>.ValueEnumerable Values => _ref.Values;

    public readonly ValueEntitySparseSet<TValue, TStorage>.KeyEnumerable Keys => _ref.Keys;

    public readonly int Count => _ref.Count;

    public TValue this[scoped in Entity key]
    {
        readonly get => _ref[key];
        set => _ref[key] = value;
    }

    public readonly KeyValuePair<Entity, TValue> this[int index] => _ref[index];

    public void Clear()
    {
        _ref.Clear();
    }

    public readonly bool ContainsKey(scoped in Entity key)
    {
        return _ref.ContainsKey(key);
    }

    public readonly bool TryGetValue(scoped in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public readonly TValue? GetValueOrDefault(scoped in Entity key)
    {
        return _ref.GetValueOrDefault(key);
    }

    public readonly TValue GetValueOrDefault(scoped in Entity key, in TValue defaultValue)
    {
        return _ref.GetValueOrDefault(key, defaultValue);
    }

    public bool Remove(scoped in Entity key)
    {
        return _ref.Remove(key);
    }

    public readonly int GetKeyIndex(scoped in Entity key)
    {
        return _ref.GetKeyIndex(key);
    }

    public readonly void CopyTo(scoped in Span<KeyValuePair<Entity, TValue>> span, int arrayIndex = 0)
    {
        _ref.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(scoped ref ValueEntitySparseSet<TValue, TStorage> sparseSet)
    {
        _ref.CopyTo(ref sparseSet);
    }

    public readonly ValueEntitySparseSet<TValue, TStorage>.Enumerator GetEnumerator()
    {
        return _ref.GetEnumerator();
    }

    public readonly ValueEnumerable<
        ValueEntitySparseSet<TValue, TStorage>.Enumerator,
        KeyValuePair<Entity, TValue>
    > AsValueEnumerable()
    {
        return _ref.AsValueEnumerable();
    }

    readonly ValueEnumerable<
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

    public readonly ValueEntitySparseSetView<TValue, TStorage>.Enumerable AsEnumerable()
    {
        return new ValueEntitySparseSetView<TValue, TStorage>.Enumerable(_ref);
    }

    readonly IEnumerator<KeyValuePair<Entity, TValue>> IEnumerable<KeyValuePair<Entity, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ICollection<TValue> IDictionary<Entity, TValue>.Values => ((IDictionary<Entity, TValue>)_ref).Values;

    readonly ICollection<Entity> IDictionary<Entity, TValue>.Keys => ((IDictionary<Entity, TValue>)_ref).Keys;

    readonly IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

    readonly IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => Values;

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.IsReadOnly => false;

    TValue IDictionary<Entity, TValue>.this[Entity key]
    {
        readonly get => this[key];
        set => this[key] = value;
    }

    readonly TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    void ICollection<KeyValuePair<Entity, TValue>>.Add(KeyValuePair<Entity, TValue> item)
    {
        if (_ref.ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        _ref[item.Key] = item.Value;
    }

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.Contains(KeyValuePair<Entity, TValue> item)
    {
        return _ref.TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    readonly void ICollection<KeyValuePair<Entity, TValue>>.CopyTo(KeyValuePair<Entity, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        _ref.CopyTo(array.AsSpan(), arrayIndex);
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.Remove(KeyValuePair<Entity, TValue> item)
    {
        return _ref.TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && _ref.Remove(item.Key);
    }

    void IDictionary<Entity, TValue>.Add(Entity key, TValue value)
    {
        if (_ref.ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        _ref[key] = value;
    }

    readonly bool IDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return _ref.ContainsKey(key);
    }

    bool IDictionary<Entity, TValue>.Remove(Entity key)
    {
        return _ref.Remove(key);
    }

    readonly bool IDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    readonly bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return _ref.ContainsKey(key);
    }

    readonly bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _ref.TryGetValue(key, out value);
    }

    public static implicit operator ValueEntitySparseSetView<TValue, TStorage>(
        ValueEntitySparseSetRef<TValue, TStorage> sparseSet
    )
    {
        return new ValueEntitySparseSetView<TValue, TStorage>(ref sparseSet._ref);
    }

    public override bool Equals(object? obj)
    {
        throw new NotSupportedException(
            $"{nameof(Equals)}() on {nameof(ValueEntitySparseSetRef<,>)} is not supported."
        );
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException(
            $"{nameof(GetHashCode)}() on {nameof(ValueEntitySparseSetRef<,>)} is not supported."
        );
    }

    public readonly bool Equals(scoped in ValueEntitySparseSetRef<TValue, TStorage> other)
    {
        return Unsafe.AreSame(ref _ref, ref other._ref);
    }

    public static bool operator ==(
        in ValueEntitySparseSetRef<TValue, TStorage> left,
        in ValueEntitySparseSetRef<TValue, TStorage> right
    )
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        in ValueEntitySparseSetRef<TValue, TStorage> left,
        in ValueEntitySparseSetRef<TValue, TStorage> right
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

    public static ValueSparseSetRef<T> AsRef<T>(this ref ValueSparseSet<T> sparseSet)
    {
        return new ValueSparseSetRef<T>(ref sparseSet);
    }

    public static ValueEntitySparseSetRef AsRef(this ref ValueEntitySparseSet sparseSet)
    {
        return new ValueEntitySparseSetRef(ref sparseSet);
    }

    public static ValueEntitySparseSetRef<TValue, TStorage> AsRef<TValue, TStorage>(
        this ref ValueEntitySparseSet<TValue, TStorage> sparseSet
    )
        where TStorage : IList<TValue>
    {
        return new ValueEntitySparseSetRef<TValue, TStorage>(ref sparseSet);
    }

    public static ValueEntitySparseSetRef<TValue, ValueList<TValue>> AsRef<TValue>(
        this ref ValueEntitySparseSet<TValue> sparseSet
    )
    {
        return new ValueEntitySparseSetRef<TValue, ValueList<TValue>>(ref sparseSet.Storage);
    }

    public static ValueHashSetRef<T> AsRef<T>(this ref ValueHashSet<T> hashSet)
    {
        return new ValueHashSetRef<T>(ref hashSet);
    }
}
