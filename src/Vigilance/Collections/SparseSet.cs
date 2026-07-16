using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;

namespace Vigilance.Collections;

public class SparseSet<TKey, TValue> : SparseSet<TKey, TValue, ValueList<TValue>>
{
    public SparseSet(Func<TKey, int> keyIndexFunc, int sparseChunkSize = DefaultSparseChunkSize)
        : base([], keyIndexFunc, sparseChunkSize) { }
}

public class SparseSet<TKey, TValue, TStorage>
    : ISparseSet<TKey, TValue, TStorage>,
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        IStructEnumerable<SparseSet<TKey, TValue, TStorage>.Enumerator, KeyValuePair<TKey, TValue>>
    where TStorage : IList<TValue>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueSparseSet<TKey, TValue, TStorage> _sparseSet;

    public SparseSet(in TStorage storage, Func<TKey, int> keyIndexFunc, int sparseChunkSize = DefaultSparseChunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sparseChunkSize, 1);
        if (storage.Count != 0)
            throw new ArgumentException("Storage must be empty", nameof(storage));
        _sparseSet = new ValueSparseSet<TKey, TValue, TStorage>(in storage, keyIndexFunc, sparseChunkSize);
    }

    public ValueListView<TKey> Keys => _sparseSet.Keys;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        if (ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        this[item.Key] = item.Value;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(array));
        for (var i = 0; i < Count; i++)
            array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(_sparseSet.Keys[i], _sparseSet.Values[i]);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        this[key] = value;
    }

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
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

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values => _sparseSet.Values.AsFastEnumerable();

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _sparseSet.Keys.AsEnumerable().AsFastEnumerable();

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _sparseSet.Keys.AsEnumerable().AsFastEnumerable();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _sparseSet.Values.AsFastEnumerable();

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return ContainsKey(key);
    }

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    [OverloadResolutionPriority(1)]
    public TValue this[in TKey key]
    {
        get => _sparseSet[key];
        set => _sparseSet[key] = value;
    }

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public int Count => _sparseSet.Count;

    public KeyValuePair<TKey, TValue> this[int index] => _sparseSet[index];

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

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>,
        KeyValuePair<TKey, TValue>
    > IStructEnumerable<Enumerator, KeyValuePair<TKey, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    public ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, KeyValuePair<TKey, TValue>>(GetEnumerator());
    }

    public struct Enumerator
        : IStructEnumerator<KeyValuePair<TKey, TValue>>,
            IValueEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly SparseSet<TKey, TValue, TStorage> _sparseSet;
        private int _index;

        internal Enumerator(SparseSet<TKey, TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_sparseSet.Keys.Count)
            {
                Current = new KeyValuePair<TKey, TValue>(_sparseSet.Keys[_index], _sparseSet.Values[_index]);
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
