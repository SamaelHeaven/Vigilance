using System.Runtime.CompilerServices;
using Vigilance.Core;

namespace Vigilance.Collections;

public sealed class SparseSet<TKey, TItem> : SparseSet<TKey, TItem, List<TItem>>
{
    public SparseSet(Func<TKey, int> keyIndexFunc)
        : base([], keyIndexFunc) { }
}

public class SparseSet<TKey, TItem, TStorage>
    where TStorage : IList<TItem>
{
    private const int SparseChunkSize = 2048;
    private readonly Func<TKey, int> _keyIndexFunc;

    private TStorage _items;
    private ValueList<TKey> _keys = [];
    private ValueList<int[]?> _sparseChunks = [];

    public SparseSet(TStorage storage, Func<TKey, int> keyIndexFunc)
    {
        if (storage.Count != 0)
            throw new ArgumentException("Storage must be empty", nameof(storage));
        _items = storage;
        _keyIndexFunc = keyIndexFunc;
    }

    public FastEnumerable<TItem> Items => _items.AsFastEnumerable();
    public ValueListView<TKey> Keys => _keys;

    public int Count => _keys.Count;

    public TItem this[TKey key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    public bool Has(in TKey key)
    {
        Debug.Assert(_keys.Count == _items.Count);
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return false;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return false;
        var withinChunk = keyIndex % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        return sparseValue != 0;
    }

    public bool TryGet(in TKey key, out TItem item)
    {
        Debug.Assert(_keys.Count == _items.Count);
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / SparseChunkSize;
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

        var withinChunk = keyIndex % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
        {
            Unsafe.SkipInit(out item);
            return false;
        }

        var denseIndex = sparseValue - 1;
        item = _items[denseIndex];
        return true;
    }

    public TItem Get(in TKey key)
    {
        return !TryGet(key, out var item) ? throw new KeyNotFoundException(key?.ToString()) : item;
    }

    public TItem? GetOrDefault(in TKey key)
    {
        return TryGet(key, out var item) ? item : default;
    }

    public TItem GetOrDefault(in TKey key, in TItem defaultValue)
    {
        return TryGet(in key, out var value) ? value : defaultValue;
    }

    public void Set(in TKey key, in TItem item)
    {
        Debug.Assert(_keys.Count == _items.Count);
        var keyIndex = _keyIndexFunc.Invoke(key);
        EnsureChunk(keyIndex);
        var chunkIndex = keyIndex / SparseChunkSize;
        var withinChunk = keyIndex % SparseChunkSize;
        var chunk = _sparseChunks[chunkIndex]!;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
        {
            var index = _items.Count + 1;
            _items.Add(item);
            _keys.Add(key);
            chunk[withinChunk] = index;
            return;
        }

        var denseIndex = sparseValue - 1;
        _items[denseIndex] = item;
    }

    public void Remove(in TKey key)
    {
        Debug.Assert(_keys.Count == _items.Count);
        var keyIndex = _keyIndexFunc.Invoke(key);
        var chunkIndex = keyIndex / SparseChunkSize;
        if (chunkIndex >= _sparseChunks.Count)
            return;
        var chunk = _sparseChunks[chunkIndex];
        if (chunk == null)
            return;
        var withinChunk = keyIndex % SparseChunkSize;
        var sparseValue = chunk[withinChunk];
        if (sparseValue == 0)
            return;
        var denseIndex = sparseValue - 1;
        var lastDenseIndex = _items.Count - 1;
        if (denseIndex != lastDenseIndex)
        {
            _items[denseIndex] = _items[lastDenseIndex];
            var movedKey = _keys[lastDenseIndex];
            _keys[denseIndex] = movedKey;
            var movedKeyIndex = _keyIndexFunc.Invoke(movedKey);
            var movedChunkIndex = movedKeyIndex / SparseChunkSize;
            var movedWithinChunk = movedKeyIndex % SparseChunkSize;
            var movedChunk = _sparseChunks[movedChunkIndex]!;
            movedChunk[movedWithinChunk] = denseIndex + 1;
        }

        _items.RemoveAt(lastDenseIndex);
        _keys.RemoveAt(lastDenseIndex);
        chunk[withinChunk] = 0;
    }

    private void EnsureChunk(int index)
    {
        var chunkIndex = index / SparseChunkSize;
        while (_sparseChunks.Count <= chunkIndex)
            _sparseChunks.Add(null);
        if (_sparseChunks[chunkIndex] != null)
            return;
        var chunk = new int[SparseChunkSize];
        _sparseChunks[chunkIndex] = chunk;
    }
}
