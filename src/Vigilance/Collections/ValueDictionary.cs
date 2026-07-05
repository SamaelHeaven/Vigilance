using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IStructEnumerable<ValueDictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private const int StartOfFreeList = -3;

    private int[]? _buckets;
    private Entry[]? _entries;
    private ulong _fastModMultiplier;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private readonly IEqualityComparer<TKey>? _comparer;
    private readonly IEqualityComparer<TKey>? _underlyingComparer;

    public ValueDictionary()
        : this(0) { }

    public ValueDictionary(IEqualityComparer<TKey>? comparer)
        : this(0, comparer) { }

    public ValueDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > 0)
            Initialize(capacity);
        if (!typeof(TKey).IsValueType)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _underlyingComparer = comparer;
            if (
                typeof(TKey) == typeof(string)
                && NonRandomizedStringEqualityComparer.GetStringComparer(_comparer!) is { } stringComparer
            )
                _comparer = (IEqualityComparer<TKey>)stringComparer;
        }
        else if (comparer is not null && !ReferenceEquals(comparer, EqualityComparer<TKey>.Default))
        {
            _comparer = comparer;
            _underlyingComparer = comparer;
        }
    }

    public ValueDictionary(in ValueDictionary<TKey, TValue> source)
    {
        _comparer = source._comparer;
        _underlyingComparer = source._underlyingComparer;
        _count = source._count;
        _freeList = source._freeList;
        _freeCount = source._freeCount;
        _fastModMultiplier = source._fastModMultiplier;
        _buckets = (int[]?)source._buckets?.Clone();
        _entries = (Entry[]?)source._entries?.Clone();
    }

    public ValueDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer = null)
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        : this(dictionary?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        foreach (var pair in dictionary)
            Add(pair.Key, pair.Value);
    }

    public ValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer = null)
        : this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        foreach (var pair in collection)
            Add(pair.Key, pair.Value);
    }

    public ValueDictionary(IEnumerable<(TKey Key, TValue Value)> collection, IEqualityComparer<TKey>? comparer = null)
        : this((collection as ICollection<(TKey Key, TValue Value)>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        foreach (var pair in collection)
            Add(pair.Key, pair.Value);
    }

    public readonly IEqualityComparer<TKey> Comparer => _underlyingComparer ?? EqualityComparer<TKey>.Default;

    public readonly int Count => _count - _freeCount;

    public readonly int Capacity => _entries?.Length ?? 0;

    public readonly KeyCollection Keys => new(this);

    public readonly ValueCollection Values => new(this);

    public TValue this[in TKey key]
    {
        readonly get
        {
            ref var value = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref value))
                return value;
            ThrowKeyNotFound(key);
            return default;
        }
        set
        {
            // ReSharper disable once RedundantAssignment
            var modified = TryInsert(key, value, InsertionBehavior.OverwriteExisting);
            Debug.Assert(modified);
        }
    }

    public void Add(in TKey key, in TValue value)
    {
        // ReSharper disable once RedundantAssignment
        var modified = TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
        Debug.Assert(modified);
    }

    public bool TryAdd(in TKey key, in TValue value)
    {
        return TryInsert(key, value, InsertionBehavior.None);
    }

    public void Clear()
    {
        var count = _count;
        if (count <= 0)
            return;
        Debug.Assert(_buckets != null);
        Debug.Assert(_entries != null);
        Array.Clear(_buckets);
        _count = 0;
        _freeList = -1;
        _freeCount = 0;
        Array.Clear(_entries, 0, count);
    }

    public readonly bool ContainsKey(in TKey key)
    {
        return !Unsafe.IsNullRef(ref FindValue(key));
    }

    public readonly bool ContainsValue(in TValue value)
    {
        var entries = _entries;
        if (value == null)
        {
            for (var i = 0; i < _count; i++)
                if (entries![i].Next >= -1 && entries[i].Value == null)
                    return true;
        }
        else if (typeof(TValue).IsValueType)
        {
            for (var i = 0; i < _count; i++)
                if (entries![i].Next >= -1 && EqualityComparer<TValue>.Default.Equals(entries[i].Value, value))
                    return true;
        }
        else
        {
            var defaultComparer = EqualityComparer<TValue>.Default;
            for (var i = 0; i < _count; i++)
                if (entries![i].Next >= -1 && defaultComparer.Equals(entries[i].Value, value))
                    return true;
        }

        return false;
    }

    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        ref var valRef = ref FindValue(key);
        if (!Unsafe.IsNullRef(ref valRef))
        {
            value = valRef;
            return true;
        }

        value = default;
        return false;
    }

    public bool Remove(in TKey key)
    {
        if (!typeof(TKey).IsValueType)
            ArgumentNullException.ThrowIfNull(key);
        if (_buckets == null)
            return false;
        Debug.Assert(_entries != null);
        uint collisionCount = 0;
        var comparer = _comparer;
        Debug.Assert(typeof(TKey).IsValueType || comparer is not null);
        var hashCode = (uint)(
            typeof(TKey).IsValueType && comparer == null ? key.GetHashCode() : comparer!.GetHashCode(key)
        );
        ref var bucket = ref GetBucket(hashCode);
        var entries = _entries;
        var last = -1;
        var i = bucket - 1;
        while (i >= 0)
        {
            ref var entry = ref entries[i];

            if (
                entry.HashCode == hashCode
                && (
                    typeof(TKey).IsValueType && comparer == null
                        ? EqualityComparer<TKey>.Default.Equals(entry.Key, key)
                        : comparer!.Equals(entry.Key, key)
                )
            )
            {
                if (last < 0)
                    bucket = entry.Next + 1;
                else
                    entries[last].Next = entry.Next;
                Debug.Assert(StartOfFreeList - _freeList < 0);
                entry.Next = StartOfFreeList - _freeList;
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                    entry.Key = default!;
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                    entry.Value = default!;
                _freeList = i;
                _freeCount++;
                return true;
            }

            last = i;
            i = entry.Next;
            collisionCount++;
            if (collisionCount > (uint)entries.Length)
                ThrowConcurrentOperation();
        }

        return false;
    }

    public bool Remove(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!typeof(TKey).IsValueType)
            ArgumentNullException.ThrowIfNull(key);
        if (_buckets != null)
        {
            Debug.Assert(_entries != null);
            uint collisionCount = 0;
            var comparer = _comparer;
            Debug.Assert(typeof(TKey).IsValueType || comparer is not null);
            var hashCode = (uint)(
                typeof(TKey).IsValueType && comparer == null ? key.GetHashCode() : comparer!.GetHashCode(key)
            );
            ref var bucket = ref GetBucket(hashCode);
            var entries = _entries;
            var last = -1;
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref entries[i];

                if (
                    entry.HashCode == hashCode
                    && (
                        typeof(TKey).IsValueType && comparer == null
                            ? EqualityComparer<TKey>.Default.Equals(entry.Key, key)
                            : comparer!.Equals(entry.Key, key)
                    )
                )
                {
                    if (last < 0)
                        bucket = entry.Next + 1;
                    else
                        entries[last].Next = entry.Next;
                    value = entry.Value;
                    Debug.Assert(StartOfFreeList - _freeList < 0);
                    entry.Next = StartOfFreeList - _freeList;
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>())
                        entry.Key = default!;
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
                        entry.Value = default!;
                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }

        value = default;
        return false;
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    readonly ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    public ref TValue GetValueRefOrNullRef(in TKey key)
    {
        return ref FindValue(key);
    }

    public ref TValue? GetValueRefOrAddDefault(in TKey key, out bool exists)
    {
        if (!typeof(TKey).IsValueType)
            ArgumentNullException.ThrowIfNull(key);
        if (_buckets == null)
            Initialize(0);
        Debug.Assert(_buckets != null);
        var entries = _entries;
        Debug.Assert(entries != null);
        var comparer = _comparer;
        Debug.Assert(comparer is not null || typeof(TKey).IsValueType);
        var hashCode = (uint)(
            typeof(TKey).IsValueType && comparer == null ? key.GetHashCode() : comparer!.GetHashCode(key)
        );
        uint collisionCount = 0;
        ref var bucket = ref GetBucket(hashCode);
        var i = bucket - 1;
        if (typeof(TKey).IsValueType && comparer == null)
        {
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                {
                    exists = true;
                    return ref entries[i].Value!;
                }

                i = entries[i].Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && comparer.Equals(entries[i].Key, key))
                {
                    exists = true;
                    return ref entries[i].Value!;
                }

                i = entries[i].Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            Debug.Assert(StartOfFreeList - entries[_freeList].Next >= -1);
            _freeList = StartOfFreeList - entries[_freeList].Next;
            _freeCount--;
        }
        else
        {
            var count = _count;
            if (count == entries.Length)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }

            index = count;
            _count = count + 1;
            entries = _entries;
        }

        ref var entry = ref entries![index];
        entry.HashCode = hashCode;
        entry.Next = bucket - 1;
        entry.Key = key;
        entry.Value = default!;
        bucket = index + 1;
        exists = false;
        return ref entry.Value!;
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        var currentCapacity = _entries?.Length ?? 0;
        if (currentCapacity >= capacity)
            return currentCapacity;
        if (_buckets == null)
            return Initialize(capacity);
        var newSize = HashHelpers.GetPrime(capacity);
        Resize(newSize);
        return newSize;
    }

    public void TrimExcess()
    {
        TrimExcess(Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        var newSize = HashHelpers.GetPrime(capacity);
        var oldEntries = _entries;
        var currentCapacity = oldEntries?.Length ?? 0;
        if (newSize >= currentCapacity)
            return;
        var oldCount = _count;
        Initialize(newSize);
        Debug.Assert(oldEntries is not null);
        CopyEntries(oldEntries, oldCount);
    }

    internal readonly ref TValue FindValue(in TKey key)
    {
        if (!typeof(TKey).IsValueType)
            ArgumentNullException.ThrowIfNull(key);
        ref var entry = ref Unsafe.NullRef<Entry>();
        if (_buckets != null)
        {
            Debug.Assert(_entries != null);
            var comparer = _comparer;
            if (typeof(TKey).IsValueType && comparer == null)
            {
                var hashCode = (uint)key.GetHashCode();
                var i = GetBucket(hashCode);
                var entries = _entries;
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)entries.Length)
                        goto ReturnNotFound;
                    entry = ref entries[i];
                    if (entry.HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                        goto ReturnFound;
                    i = entry.Next;
                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);

                goto ConcurrentOperation;
            }
            else
            {
                Debug.Assert(comparer is not null);
                var hashCode = (uint)comparer.GetHashCode(key);
                var i = GetBucket(hashCode);
                var entries = _entries;
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)entries.Length)
                        goto ReturnNotFound;
                    entry = ref entries[i];
                    if (entry.HashCode == hashCode && comparer.Equals(entry.Key, key))
                        goto ReturnFound;
                    i = entry.Next;
                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);

                goto ConcurrentOperation;
            }
        }

        goto ReturnNotFound;
        ConcurrentOperation:
        ThrowConcurrentOperation();
        ReturnFound:
        // ReSharper disable once SuggestVarOrType_SimpleTypes
        ref TValue value = ref entry.Value;
        Return:
        return ref value;
        ReturnNotFound:
        value = ref Unsafe.NullRef<TValue>();
        goto Return;
    }

    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var buckets = new int[size];
        var entries = new Entry[size];
        _freeList = -1;
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)size);
        _buckets = buckets;
        _entries = entries;
        return size;
    }

    private bool TryInsert(in TKey key, in TValue value, InsertionBehavior behavior)
    {
        if (!typeof(TKey).IsValueType)
            ArgumentNullException.ThrowIfNull(key);
        if (_buckets == null)
            Initialize(0);
        Debug.Assert(_buckets != null);
        var entries = _entries;
        Debug.Assert(entries != null);
        var comparer = _comparer;
        Debug.Assert(comparer is not null || typeof(TKey).IsValueType);
        var hashCode = (uint)(
            typeof(TKey).IsValueType && comparer == null ? key.GetHashCode() : comparer!.GetHashCode(key)
        );
        uint collisionCount = 0;
        ref var bucket = ref GetBucket(hashCode);
        var i = bucket - 1;
        if (typeof(TKey).IsValueType && comparer == null)
        {
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].Key, key))
                {
                    switch (behavior)
                    {
                        case InsertionBehavior.OverwriteExisting:
                            entries[i].Value = value;
                            return true;
                        case InsertionBehavior.ThrowOnExisting:
                            ThrowDuplicateKey(key);
                            break;
                    }

                    return false;
                }

                i = entries[i].Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            while ((uint)i < (uint)entries.Length)
            {
                if (entries[i].HashCode == hashCode && comparer.Equals(entries[i].Key, key))
                {
                    switch (behavior)
                    {
                        case InsertionBehavior.OverwriteExisting:
                            entries[i].Value = value;
                            return true;
                        case InsertionBehavior.ThrowOnExisting:
                            ThrowDuplicateKey(key);
                            break;
                    }

                    return false;
                }

                i = entries[i].Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            Debug.Assert(StartOfFreeList - entries[_freeList].Next >= -1);
            _freeList = StartOfFreeList - entries[_freeList].Next;
            _freeCount--;
        }
        else
        {
            var count = _count;
            if (count == entries.Length)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }

            index = count;
            _count = count + 1;
            entries = _entries;
        }

        ref var entry = ref entries![index];
        entry.HashCode = hashCode;
        entry.Next = bucket - 1;
        entry.Key = key;
        entry.Value = value;
        bucket = index + 1;
        return true;
    }

    private void Resize()
    {
        Resize(HashHelpers.ExpandPrime(_count));
    }

    private void Resize(int newSize)
    {
        Debug.Assert(_entries != null);
        Debug.Assert(newSize >= _entries.Length);
        var entries = new Entry[newSize];
        var count = _count;
        Array.Copy(_entries, entries, count);
        _buckets = new int[newSize];
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
        for (var i = 0; i < count; i++)
            if (entries[i].Next >= -1)
            {
                ref var bucket = ref GetBucket(entries[i].HashCode);
                entries[i].Next = bucket - 1;
                bucket = i + 1;
            }

        _entries = entries;
    }

    private void CopyEntries(Entry[] entries, int count)
    {
        Debug.Assert(_entries is not null);
        var newEntries = _entries;
        var newCount = 0;
        for (var i = 0; i < count; i++)
        {
            var hashCode = entries[i].HashCode;
            if (entries[i].Next < -1)
                continue;
            ref var entry = ref newEntries[newCount];
            entry = entries[i];
            ref var bucket = ref GetBucket(hashCode);
            entry.Next = bucket - 1;
            bucket = newCount + 1;
            newCount++;
        }

        _count = newCount;
        _freeCount = 0;
    }

    private readonly void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), index);
    }

    public readonly void CopyTo(in Span<KeyValuePair<TKey, TValue>> span, int arrayIndex = 0)
    {
        if ((uint)arrayIndex > (uint)span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array is not long enough.", nameof(span));
        var count = _count;
        var entries = _entries;
        var index = arrayIndex;
        for (var i = 0; i < count; i++)
            if (entries![i].Next >= -1)
                span[index++] = new KeyValuePair<TKey, TValue>(entries[i].Key, entries[i].Value);
    }

    public readonly void CopyTo(ref ValueDictionary<TKey, TValue> dictionary)
    {
        dictionary = new ValueDictionary<TKey, TValue>(in this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucket(uint hashCode)
    {
        var buckets = _buckets!;
        return ref buckets[HashHelpers.FastMod(hashCode, (uint)buckets.Length, _fastModMultiplier)];
    }

    [DoesNotReturn]
    private static void ThrowConcurrentOperation()
    {
        throw new InvalidOperationException(
            "Operations that change non-concurrent collections must have exclusive access."
        );
    }

    [DoesNotReturn]
    private static void ThrowDuplicateKey(in TKey key)
    {
        throw new ArgumentException($"An item with the same key has already been added. Key: {key}", nameof(key));
    }

    [DoesNotReturn]
    private static void ThrowKeyNotFound(in TKey key)
    {
        throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
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

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        Add(key, value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
    {
        Add(keyValuePair.Key, keyValuePair.Value);
    }

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
    {
        ref var value = ref FindValue(keyValuePair.Key);
        return !Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
    {
        ref var value = ref FindValue(keyValuePair.Key);
        if (!Unsafe.IsNullRef(ref value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
            return Remove(keyValuePair.Key);
        return false;
    }

    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        CopyTo(array, index);
    }

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return Remove(key);
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    private enum InsertionBehavior : byte
    {
        None = 0,
        OverwriteExisting = 1,
        ThrowOnExisting = 2,
    }

    private struct Entry
    {
        public uint HashCode;
        public int Next;
        public TKey Key;
        public TValue Value;
    }

    public struct Enumerator
        : IStructEnumerator<KeyValuePair<TKey, TValue>>,
            IValueEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly ValueDictionary<TKey, TValue> _dictionary;
        private int _index;

        internal Enumerator(in ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _index = 0;
            Current = default;
        }

        public bool MoveNext()
        {
            while ((uint)_index < (uint)_dictionary._count)
            {
                ref var entry = ref _dictionary._entries![_index++];
                if (entry.Next < -1)
                    continue;
                Current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                return true;
            }

            _index = _dictionary._count + 1;
            Current = default;
            return false;
        }

        public KeyValuePair<TKey, TValue> Current { get; private set; }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public void Dispose() { }

        public bool TryGetNext(out KeyValuePair<TKey, TValue> current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _dictionary.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<KeyValuePair<TKey, TValue>> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<KeyValuePair<TKey, TValue>> destination, Index offset)
        {
            return false;
        }
    }

    public readonly struct KeyCollection
        : ICollection<TKey>,
            IReadOnlyCollection<TKey>,
            IStructEnumerable<KeyCollection.Enumerator, TKey>
    {
        private readonly ValueDictionary<TKey, TValue> _dictionary;

        internal KeyCollection(in ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public int Count => _dictionary.Count;

        bool ICollection<TKey>.IsReadOnly => true;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_dictionary);
        }

        public ValueEnumerable<StructEnumerator<Enumerator, TKey>, TKey> AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, TKey>(GetEnumerator());
        }

        public void CopyTo(TKey[] array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);
            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (array.Length - index < _dictionary.Count)
                throw new ArgumentException("Destination array is not long enough.", nameof(array));
            var count = _dictionary._count;
            var entries = _dictionary._entries;
            for (var i = 0; i < count; i++)
                if (entries![i].Next >= -1)
                    array[index++] = entries[i].Key;
        }

        public bool Contains(TKey item)
        {
            return _dictionary.ContainsKey(item);
        }

        void ICollection<TKey>.Add(TKey item)
        {
            throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
        }

        void ICollection<TKey>.Clear()
        {
            throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
        }

        bool ICollection<TKey>.Remove(TKey item)
        {
            throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
        }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public struct Enumerator : IStructEnumerator<TKey>
        {
            private readonly ValueDictionary<TKey, TValue> _dictionary;
            private int _index;
            private TKey? _currentKey;

            internal Enumerator(in ValueDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _currentKey = default;
            }

            public bool MoveNext()
            {
                while ((uint)_index < (uint)_dictionary._count)
                {
                    ref var entry = ref _dictionary._entries![_index++];
                    if (entry.Next < -1)
                        continue;
                    _currentKey = entry.Key;
                    return true;
                }

                _index = _dictionary._count + 1;
                _currentKey = default;
                return false;
            }

            public TKey Current => _currentKey!;

            public void Reset()
            {
                _index = 0;
                _currentKey = default;
            }

            public void Dispose() { }
        }
    }

    public readonly struct ValueCollection
        : ICollection<TValue>,
            IReadOnlyCollection<TValue>,
            IStructEnumerable<ValueCollection.Enumerator, TValue>
    {
        private readonly ValueDictionary<TKey, TValue> _dictionary;

        internal ValueCollection(in ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public int Count => _dictionary.Count;

        bool ICollection<TValue>.IsReadOnly => true;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_dictionary);
        }

        public ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
        }

        public void CopyTo(TValue[] array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);
            if ((uint)index > (uint)array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (array.Length - index < _dictionary.Count)
                throw new ArgumentException("Destination array is not long enough.", nameof(array));

            var count = _dictionary._count;
            var entries = _dictionary._entries;
            for (var i = 0; i < count; i++)
                if (entries![i].Next >= -1)
                    array[index++] = entries[i].Value;
        }

        bool ICollection<TValue>.Contains(TValue item)
        {
            return _dictionary.ContainsValue(item);
        }

        void ICollection<TValue>.Add(TValue item)
        {
            throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
        }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public struct Enumerator : IStructEnumerator<TValue>
        {
            private readonly ValueDictionary<TKey, TValue> _dictionary;
            private int _index;
            private TValue? _currentValue;

            internal Enumerator(in ValueDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _currentValue = default;
            }

            public bool MoveNext()
            {
                while ((uint)_index < (uint)_dictionary._count)
                {
                    ref var entry = ref _dictionary._entries![_index++];
                    if (entry.Next < -1)
                        continue;
                    _currentValue = entry.Value;
                    return true;
                }

                _index = _dictionary._count + 1;
                _currentValue = default;
                return false;
            }

            public TValue Current => _currentValue!;

            public void Reset()
            {
                _index = 0;
                _currentValue = default;
            }

            public void Dispose() { }
        }
    }
}

public static class ValueDictionaryExtensions
{
    extension<TKey, TValue>(in ValueDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        public ValueDictionary<TKey, TValue> ToValueDictionary()
        {
            return new ValueDictionary<TKey, TValue>(dictionary);
        }
    }

    extension<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        where TKey : notnull
    {
        public ValueDictionary<TKey, TValue> ToValueDictionary()
        {
            return new ValueDictionary<TKey, TValue>(enumerable);
        }
    }

    extension<TKey, TValue>(IEnumerable<(TKey Key, TValue Value)> enumerable)
        where TKey : notnull
    {
        public ValueDictionary<TKey, TValue> ToValueDictionary()
        {
            return new ValueDictionary<TKey, TValue>(enumerable);
        }
    }

    extension<TEnumerator, TKey, TValue>(in ValueEnumerable<TEnumerator, KeyValuePair<TKey, TValue>> enumerable)
        where TEnumerator : struct, IValueEnumerator<KeyValuePair<TKey, TValue>>, allows ref struct
        where TKey : notnull
    {
        public ValueDictionary<TKey, TValue> ToValueDictionary()
        {
            using var enumerator = enumerable.Enumerator;
            var result = new ValueDictionary<TKey, TValue>();
            if (enumerator.TryGetNonEnumeratedCount(out var count))
                result.EnsureCapacity(count);
            while (enumerator.TryGetNext(out var pair))
                result.Add(pair.Key, pair.Value);
            return result;
        }
    }

    extension<TEnumerator, TKey, TValue>(in ValueEnumerable<TEnumerator, (TKey Key, TValue Value)> enumerable)
        where TEnumerator : struct, IValueEnumerator<(TKey Key, TValue Value)>, allows ref struct
        where TKey : notnull
    {
        public ValueDictionary<TKey, TValue> ToValueDictionary()
        {
            using var enumerator = enumerable.Enumerator;
            var result = new ValueDictionary<TKey, TValue>();
            if (enumerator.TryGetNonEnumeratedCount(out var count))
                result.EnsureCapacity(count);
            while (enumerator.TryGetNext(out var pair))
                result.Add(pair.Key, pair.Value);
            return result;
        }
    }
}
