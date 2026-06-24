#pragma warning disable CS9084

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueSparseSet<TKey, TValue, TStorage>
    : IDictionary<TKey, TValue>,
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

    public ValueSparseSet(TStorage storage, Func<TKey, int> keyIndexFunc, int sparseChunkSize = DefaultSparseChunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sparseChunkSize, 1);
        if (storage.Count != 0)
            throw new ArgumentException("Storage must be empty", nameof(storage));
        _values = storage;
        _keyIndexFunc = keyIndexFunc;
        _sparseChunkSize = sparseChunkSize;
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)sparseChunkSize);
    }

    public readonly FastEnumerable<TValue> Values
    {
        get
        {
            AssertValid();
            return _values.AsFastEnumerable();
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

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            AssertValid();
            return _values.AsReadOnly();
        }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get
        {
            AssertValid();
            return _keys.AsReadOnly();
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

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        AssertValid();
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        AssertValid();
        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(array));
        for (var i = 0; i < Count; i++)
            array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
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

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
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

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        AssertValid();
        return ContainsKey(key);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        AssertValid();
        return Remove(key);
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        AssertValid();
        return TryGetValue(key, out value);
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get
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

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keys.AsReadOnly();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _values.AsReadOnly();

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        AssertValid();
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key]
    {
        get
        {
            AssertValid();
            return this[key];
        }
    }

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
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

    public readonly ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > AsValueEnumerable()
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

    public readonly bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue item)
    {
        AssertValid();
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / _sparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        var chunk = _sparseChunks[chunkIndex];
        if (chunk is null)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        var withinChunk = WithinChunk(keyIndex);
        var sparseValue = chunk[withinChunk];
        if (sparseValue == -1)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        item = _values[sparseValue];
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

    public struct Enumerator : IStructEnumerator<KeyValuePair<TKey, TValue>>
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
    }
}
