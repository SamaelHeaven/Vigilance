using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Core;
using ZLinq;
using ZLinq.Internal;
using ZLinq.Linq;

namespace Vigilance.Collections;

public struct ValueList<T> : IList<T>, IStructEnumerable<ValueList<T>.Enumerator, T>, ISpanView<T>
{
    private const int DefaultCapacity = 4;

    private T[] _items;
    private int _size;

    public ValueList()
    {
        _items = [];
    }

    public ValueList(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);
        _items = capacity == 0 ? [] : new T[capacity];
    }

    public ValueList(IEnumerable<T> enumerable)
    {
        if (enumerable is ICollection<T> collection)
        {
            var count = collection.Count;
            if (count == 0)
            {
                _items = [];
            }
            else
            {
                _items = new T[count];
                collection.CopyTo(_items, 0);
                _size = count;
            }
        }
        else
        {
            _items = [];
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
                Add(enumerator.Current);
        }
    }

    public int Count
    {
        get => _size;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value > Capacity)
                Grow(value);
            else if (value < _size && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Array.Clear(_items, value, _size - value);
            _size = value;
        }
    }

    public readonly bool IsReadOnly => false;

    public int Capacity
    {
        readonly get => _items.Length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, _size);
            if (value == _items.Length)
                return;
            if (value > 0)
            {
                var newItems = new T[value];
                if (_size > 0)
                    Array.Copy(_items, newItems, _size);
                _items = newItems;
            }
            else
            {
                _items = [];
            }
        }
    }

    public readonly ref T this[int index] => ref AsSpan()[index];

    T IList<T>.this[int index]
    {
        readonly get =>
            (uint)index >= (uint)_size ? throw new ArgumentOutOfRangeException(nameof(index)) : _items[index];
        set
        {
            if ((uint)index >= (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index));
            _items[index] = value;
        }
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    public readonly Span<T> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), 0), _size);
    }

    public readonly T[] AsArray(out int length)
    {
        length = _size;
        return _items;
    }

    readonly ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    readonly ValueEnumerable<FromSpan<T>, T> ISpanView<T>.AsValueEnumerable()
    {
        return _items.AsSpan().AsValueEnumerable();
    }

    readonly ValueEnumerator<FromSpan<T>, T> ISpanView<T>.GetEnumerator()
    {
        return new ValueEnumerator<FromSpan<T>, T>(_items.AsSpan().AsValueEnumerable().Enumerator);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly ReadOnlySpan<T> ISpanView<T>.AsSpan()
    {
        return AsSpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T item)
    {
        var array = _items;
        var size = _size;
        if ((uint)size < (uint)array.Length)
        {
            _size = size + 1;
            array[size] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(in T item)
    {
        Debug.Assert(_size == _items.Length);
        var size = _size;
        Grow(size + 1);
        _size = size + 1;
        _items[size] = item;
    }

    public void AddRange(IEnumerable<T> collection)
    {
        if (collection is ICollection<T> c)
        {
            var count = c.Count;
            if (count <= 0)
                return;
            if (_items.Length - _size < count)
                Grow(checked(_size + count));
            c.CopyTo(_items, _size);
            _size += count;
        }
        else
        {
            using var en = collection.GetEnumerator();
            while (en.MoveNext())
                Add(en.Current);
        }
    }

    public readonly int BinarySearch(int index, int count, in T item, IComparer<T>? comparer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return _size - index < count
            ? throw new ArgumentException("Offset and length were out of bounds for the list.")
            : Array.BinarySearch(_items, index, count, item, comparer);
    }

    public readonly int BinarySearch(in T item)
    {
        return BinarySearch(0, _size, item, null);
    }

    public readonly int BinarySearch(in T item, IComparer<T>? comparer)
    {
        return BinarySearch(0, _size, item, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var size = _size;
            _size = 0;
            if (size > 0)
                Array.Clear(_items, 0, size);
        }
        else
        {
            _size = 0;
        }
    }

    public readonly bool Contains(in T item)
    {
        return _size != 0 && IndexOf(item) >= 0;
    }

    readonly bool ICollection<T>.Contains(T item)
    {
        return Contains(item);
    }

    public readonly ValueList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        var list = new ValueList<TOutput>(_size);
        for (var i = 0; i < _size; i++)
            list._items[i] = converter(_items[i]);
        list._size = _size;
        return list;
    }

    public readonly void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (_size - index < count)
            throw new ArgumentException("Offset and length were out of bounds for the list.");
        Array.Copy(_items, index, array, arrayIndex, count);
    }

    public readonly void CopyTo(T[] array, int arrayIndex = 0)
    {
        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (_items.Length < capacity)
            Grow(capacity);
        return _items.Length;
    }

    internal void Grow(int capacity)
    {
        Capacity = GetNewCapacity(capacity);
    }

    internal void GrowForInsertion(int indexToInsert, int insertionCount = 1)
    {
        Debug.Assert(insertionCount > 0);
        var requiredCapacity = checked(_size + insertionCount);
        var newCapacity = GetNewCapacity(requiredCapacity);
        var newItems = new T[newCapacity];
        if (indexToInsert != 0)
            Array.Copy(_items, newItems, indexToInsert);
        if (_size != indexToInsert)
            Array.Copy(_items, indexToInsert, newItems, indexToInsert + insertionCount, _size - indexToInsert);
        _items = newItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int GetNewCapacity(int capacity)
    {
        Debug.Assert(_items.Length < capacity);
        var newCapacity = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        if (newCapacity < capacity)
            newCapacity = capacity;
        return newCapacity;
    }

    public readonly bool Exists(Predicate<T> match)
    {
        return FindIndex(match) != -1;
    }

    public readonly T? Find(Predicate<T> match)
    {
        for (var i = 0; i < _size; i++)
            if (match(_items[i]))
                return _items[i];
        return default;
    }

    public readonly ValueList<T> FindAll(Predicate<T> match)
    {
        var list = new ValueList<T>();
        for (var i = 0; i < _size; i++)
            if (match(_items[i]))
                list.Add(_items[i]);
        return list;
    }

    public readonly int FindIndex(Predicate<T> match)
    {
        return FindIndex(0, _size, match);
    }

    public readonly int FindIndex(int startIndex, Predicate<T> match)
    {
        return FindIndex(startIndex, _size - startIndex, match);
    }

    public readonly int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        if ((uint)startIndex > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0 || startIndex > _size - count)
            throw new ArgumentOutOfRangeException(nameof(count));
        var endIndex = startIndex + count;
        for (var i = startIndex; i < endIndex; i++)
            if (match(_items[i]))
                return i;
        return -1;
    }

    public readonly T? FindLast(Predicate<T> match)
    {
        for (var i = _size - 1; i >= 0; i--)
            if (match(_items[i]))
                return _items[i];
        return default;
    }

    public readonly int FindLastIndex(Predicate<T> match)
    {
        return FindLastIndex(_size - 1, _size, match);
    }

    public readonly int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return FindLastIndex(startIndex, startIndex + 1, match);
    }

    public readonly int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        if (_size == 0)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(startIndex, -1);
        }
        else
        {
            if ((uint)startIndex >= (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (count < 0 || startIndex - count + 1 < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        var endIndex = startIndex - count;
        for (var i = startIndex; i > endIndex; i--)
            if (match(_items[i]))
                return i;

        return -1;
    }

    public readonly void ForEach(Action<T> action)
    {
        for (var i = 0; i < _size; i++)
            action(_items[i]);
    }

    public readonly ValueList<T> GetRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (_size - index < count)
            throw new ArgumentException("Offset and length were out of bounds for the list.");
        var list = new ValueList<T>(count);
        Array.Copy(_items, index, list._items, 0, count);
        list._size = count;
        return list;
    }

    public readonly ValueList<T> Slice(int start, int length)
    {
        return GetRange(start, length);
    }

    public readonly int IndexOf(in T item)
    {
        return Array.IndexOf(_items, item, 0, _size);
    }

    readonly int IList<T>.IndexOf(T item)
    {
        return IndexOf(item);
    }

    public readonly int IndexOf(in T item, int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _size);
        return Array.IndexOf(_items, item, index, _size - index);
    }

    public readonly int IndexOf(in T item, int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _size);
        if (count < 0 || index > _size - count)
            throw new ArgumentOutOfRangeException(nameof(count));
        return Array.IndexOf(_items, item, index, count);
    }

    public void Insert(int index, in T item)
    {
        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (_size == _items.Length)
            GrowForInsertion(index);
        else if (index < _size)
            Array.Copy(_items, index, _items, index + 1, _size - index);
        _items[index] = item;
        _size++;
    }

    void IList<T>.Insert(int index, T item)
    {
        Insert(index, item);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (collection is ICollection<T> c)
        {
            var count = c.Count;
            if (count <= 0)
                return;
            if (_items.Length - _size < count)
                GrowForInsertion(index, count);
            else if (index < _size)
                Array.Copy(_items, index, _items, index + count, _size - index);
            c.CopyTo(_items, index);
            _size += count;
        }
        else
        {
            using var en = collection.GetEnumerator();
            while (en.MoveNext())
                Insert(index++, en.Current);
        }
    }

    public readonly int LastIndexOf(in T item)
    {
        if (_size == 0)
            return -1;
        return LastIndexOf(item, _size - 1, _size);
    }

    public readonly int LastIndexOf(in T item, int index)
    {
        return index >= _size
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : LastIndexOf(item, index, index + 1);
    }

    public readonly int LastIndexOf(in T item, int index, int count)
    {
        if (_size != 0 && index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (_size != 0 && count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (_size == 0)
            return -1;
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
        return count > index + 1
            ? throw new ArgumentOutOfRangeException(nameof(count))
            : Array.LastIndexOf(_items, item, index, count);
    }

    public bool Remove(in T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }

    bool ICollection<T>.Remove(T item)
    {
        return Remove(item);
    }

    public int RemoveAll(Predicate<T> match)
    {
        var freeIndex = 0;
        while (freeIndex < _size && !match(_items[freeIndex]))
            freeIndex++;
        if (freeIndex >= _size)
            return 0;
        var current = freeIndex + 1;
        while (current < _size)
        {
            while (current < _size && match(_items[current]))
                current++;
            if (current < _size)
                _items[freeIndex++] = _items[current++];
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(_items, freeIndex, _size - freeIndex);
        var result = _size - freeIndex;
        _size = freeIndex;
        return result;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index));
        _size--;
        if (index < _size)
            Array.Copy(_items, index + 1, _items, index, _size - index);
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _items[_size] = default!;
    }

    public void RemoveRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (_size - index >= count)
        {
            if (count <= 0)
                return;
            _size -= count;
            if (index < _size)
                Array.Copy(_items, index + count, _items, index, _size - index);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Array.Clear(_items, _size, count);
        }
        else
        {
            throw new ArgumentException("Offset and length were out of bounds for the list.");
        }
    }

    public void Reverse()
    {
        Reverse(0, _size);
    }

    public void Reverse(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (_size - index < count)
            throw new ArgumentException("Offset and length were out of bounds for the list.");
        if (count > 1)
            Array.Reverse(_items, index, count);
    }

    public void Sort()
    {
        Sort(0, _size, null);
    }

    public void Sort(IComparer<T>? comparer)
    {
        Sort(0, _size, comparer);
    }

    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (_size - index < count)
            throw new ArgumentException("Offset and length were out of bounds for the list.");
        if (count > 1)
            Array.Sort(_items, index, count, comparer);
    }

    public void Sort(Comparison<T> comparison)
    {
        if (_size > 1)
            new Span<T>(_items, 0, _size).Sort(comparison);
    }

    public readonly T[] ToArray()
    {
        if (_size == 0)
            return [];
        var array = new T[_size];
        Array.Copy(_items, array, _size);
        return array;
    }

    public void TrimExcess()
    {
        var threshold = (int)(_items.Length * 0.9);
        if (_size < threshold)
            Capacity = _size;
    }

    public readonly bool TrueForAll(Predicate<T> match)
    {
        for (var i = 0; i < _size; i++)
            if (!match(_items[i]))
                return false;
        return true;
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly ValueList<T> _list;
        private int _index;

        internal Enumerator(ValueList<T> list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_list._size)
            {
                Current = _list._items[_index];
                _index++;
                return true;
            }

            Current = default!;
            _index = -1;
            return false;
        }

        public T Current { get; private set; } = default!;

        public void Reset()
        {
            _index = 0;
            Current = default!;
        }

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _list._size;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = _list.AsSpan();
            return true;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            if (!EnumeratorHelper.TryGetSlice(_list.AsSpan(), offset, destination.Length, out var slice))
                return false;
            slice.CopyTo(destination);
            return true;
        }
    }
}

public static class ValueListExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public ValueList<T> ToValueList()
        {
            return new ValueList<T>(enumerable);
        }
    }

    extension<TEnumerator, T>(ValueEnumerable<TEnumerator, T> enumerable)
        where TEnumerator : struct, IValueEnumerator<T>, allows ref struct
    {
        public ValueList<T> ToValueList()
        {
            using var enumerator = enumerable.Enumerator;
            var result = new ValueList<T>();
            if (enumerator.TryGetNonEnumeratedCount(out var count))
                result.Capacity = count;
            if (
                enumerator.TryCopyTo(result.AsSpan(), 0)
                || (enumerator.TryGetSpan(out var span) && span.TryCopyTo(result.AsSpan()))
            )
                return result;
            while (enumerator.TryGetNext(out var item))
                result.Add(item);
            return result;
        }
    }
}
