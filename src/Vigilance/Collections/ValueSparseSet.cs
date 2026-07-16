#pragma warning disable CS9084

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueSparseSet<TKey, TValue, TStorage>
    : ISparseSet<TKey, TValue, TStorage>,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        IStructEnumerable<ValueSparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    public const int DefaultSparseChunkSize = 2048;
    private readonly int _sparseChunkSize;
    private readonly ulong _fastModMultiplier;
    private readonly Func<TKey, int> _keyIndexFunc;
    private ValueList<TKey> _keys = [];
    private ValueList<int[]?> _sparseChunks = [];
    private TStorage _values;

    public ValueSparseSet(
        in TStorage storage,
        Func<TKey, int> keyIndexFunc,
        int sparseChunkSize = DefaultSparseChunkSize
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sparseChunkSize, 1);
        if (storage.Count != 0)
            throw new ArgumentException("Storage must be empty", nameof(storage));
        _values = storage;
        _keyIndexFunc = keyIndexFunc;
        _sparseChunkSize = sparseChunkSize;
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)sparseChunkSize);
    }

    public readonly ISparseSet<TValue, TStorage>.ValueEnumerable Values
    {
        get
        {
            AssertValid();
            return new ISparseSet<TValue, TStorage>.ValueEnumerable(_values);
        }
    }

    public readonly ValueListView<TKey> Keys
    {
        get
        {
            AssertValid();
            return new ValueListView<TKey>(ref Unsafe.AsRef(in _keys));
        }
    }

    [OverloadResolutionPriority(1)]
    public TValue this[in TKey key]
    {
        readonly get
        {
            AssertValid();
            return !TryGetValue(key, out var item) ? throw new KeyNotFoundException(key?.ToString()) : item;
        }
        set
        {
            AssertValid();
            var keyIndex = _keyIndexFunc.Invoke(key);
            EnsureChunk(keyIndex);
            var chunkIndex = keyIndex / _sparseChunkSize;
            var withinChunk = WithinChunk(keyIndex);
            var chunk = _sparseChunks[chunkIndex]!;
            var sparseValue = chunk[withinChunk];
            if (sparseValue == -1)
            {
                var index = _values.Count;
                _values.Add(value);
                _keys.Add(key);
                chunk[withinChunk] = index;
                return;
            }

            _values[sparseValue] = value;
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        AssertValid();
        if (ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        this[item.Key] = item.Value;
    }

    public void Clear()
    {
        AssertValid();
        _values.Clear();
        _keys.Clear();
        _sparseChunks.Clear();
    }

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        AssertValid();
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    public readonly void CopyTo(in Span<KeyValuePair<TKey, TValue>> span, int arrayIndex = 0)
    {
        AssertValid();
        if ((uint)arrayIndex > (uint)span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(span));
        for (var i = 0; i < Count; i++)
            span[arrayIndex + i] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
    }

    public readonly void CopyTo(ref ValueSparseSet<TKey, TValue, TStorage> sparseSet)
    {
        AssertValid();
        sparseSet.Clear();
        for (var i = 0; i < Count; i++)
            sparseSet[_keys[i]] = _values[i];
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        AssertValid();
        return TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);
    }

    public readonly int Count
    {
        get
        {
            AssertValid();
            return _keys.Count;
        }
    }

    readonly bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
        get
        {
            AssertValid();
            return false;
        }
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        AssertValid();
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        this[key] = value;
    }

    readonly bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        AssertValid();
        return ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        AssertValid();
        return Remove(key);
    }

    readonly bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        AssertValid();
        return TryGetValue(key, out value);
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        readonly get
        {
            AssertValid();
            return this[key];
        }
        set
        {
            AssertValid();
            this[key] = value;
        }
    }

    readonly ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            AssertValid();
            return _values.AsFastEnumerable();
        }
    }

    readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get
        {
            AssertValid();
            return _keys.AsFastEnumerable();
        }
    }

    readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keys.AsFastEnumerable();

    readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _values.AsFastEnumerable();

    readonly bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        AssertValid();
        return TryGetValue(key, out value);
    }

    readonly TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key]
    {
        get
        {
            AssertValid();
            return this[key];
        }
    }

    readonly bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        AssertValid();
        return ContainsKey(key);
    }

    public readonly KeyValuePair<TKey, TValue> this[int index]
    {
        get
        {
            AssertValid();
            return new KeyValuePair<TKey, TValue>(_keys[index], _values[index]);
        }
    }

    public readonly Enumerator GetEnumerator()
    {
        AssertValid();
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        AssertValid();
        return new ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    readonly ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        AssertValid();
        return new StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int WithinChunk(int keyIndex)
    {
        return (int)HashHelpers.FastMod((uint)keyIndex, (uint)_sparseChunkSize, _fastModMultiplier);
    }

    public readonly bool ContainsKey(in TKey key)
    {
        AssertValid();
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / _sparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return false;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return false;
        var withinChunk = WithinChunk(keyIndex);
        var sparseValue = chunk[withinChunk];
        return sparseValue != -1;
    }

    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        AssertValid();
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / _sparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
        {
            Unsafe.SkipInit(out value);
            return false;
        }

        var chunk = _sparseChunks[chunkIndex];
        if (chunk is null)
        {
            Unsafe.SkipInit(out value);
            return false;
        }

        var withinChunk = WithinChunk(keyIndex);
        var sparseValue = chunk[withinChunk];
        if (sparseValue == -1)
        {
            Unsafe.SkipInit(out value);
            return false;
        }

        value = _values[sparseValue];
        return true;
    }

    public bool Remove(in TKey key)
    {
        AssertValid();
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / _sparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return false;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return false;
        var withinChunk = WithinChunk(keyIndex);
        var sparseValue = chunk[withinChunk];
        if (sparseValue == -1)
            return false;
        var lastDenseIndex = _values.Count - 1;
        if (sparseValue != lastDenseIndex)
        {
            _values[sparseValue] = _values[lastDenseIndex];
            var movedKey = _keys[lastDenseIndex];
            _keys[sparseValue] = movedKey;
            var movedKeyIndex = _keyIndexFunc.Invoke(movedKey);
            var movedChunkIndex = movedKeyIndex / _sparseChunkSize;
            var movedWithinChunk = WithinChunk(movedKeyIndex);
            var movedChunk = _sparseChunks[movedChunkIndex]!;
            movedChunk[movedWithinChunk] = sparseValue;
        }

        _values.RemoveAt(lastDenseIndex);
        _keys.RemoveAt(lastDenseIndex);
        chunk[withinChunk] = -1;
        return true;
    }

    public readonly int GetKeyIndex(in TKey key)
    {
        AssertValid();
        return _keyIndexFunc.Invoke(key);
    }

    private void EnsureChunk(int index)
    {
        var chunkIndex = index / _sparseChunkSize;
        while (_sparseChunks.Count <= chunkIndex)
            _sparseChunks.Add(null);
        if (_sparseChunks[chunkIndex] != null)
            return;
        var chunk = new int[_sparseChunkSize];
        Array.Fill(chunk, -1);
        _sparseChunks[chunkIndex] = chunk;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void AssertValid()
    {
        Debug.Assert(_values.Count == _keys.Count);
    }

    public struct Enumerator
        : IStructEnumerator<KeyValuePair<TKey, TValue>>,
            IValueEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly ValueSparseSet<TKey, TValue, TStorage> _sparseSet;
        private int _index;

        internal Enumerator(in ValueSparseSet<TKey, TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
            Reset();
        }

        public bool MoveNext()
        {
            _sparseSet.AssertValid();
            if ((uint)_index < (uint)_sparseSet._keys.Count)
            {
                Current = new KeyValuePair<TKey, TValue>(_sparseSet._keys[_index], _sparseSet._values[_index]);
                _index++;
                return true;
            }

            Current = default!;
            _index = -1;
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
            _sparseSet.AssertValid();
            count = _sparseSet.Count;
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
}
