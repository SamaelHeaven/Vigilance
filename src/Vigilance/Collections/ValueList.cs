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
                Count = count;
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

    public int Count { get; private set; }

    public readonly bool IsReadOnly => false;

    public int Capacity
    {
        readonly get => _items.Length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, Count);
            if (value == _items.Length)
                return;
            if (value > 0)
            {
                var newItems = new T[value];
                if (Count > 0)
                    Array.Copy(_items, newItems, Count);
                _items = newItems;
            }
            else
            {
                _items = [];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        var array = _items;
        var size = Count;
        if ((uint)size < (uint)array.Length)
        {
            Count = size + 1;
            array[size] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var size = Count;
            Count = 0;
            if (size > 0)
                Array.Clear(_items, 0, size);
        }
        else
        {
            Count = 0;
        }
    }

    public readonly bool Contains(T item)
    {
        return Count != 0 && IndexOf(item) >= 0;
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_items, 0, array, arrayIndex, Count);
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }

    public readonly int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, Count);
    }

    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (Count == _items.Length)
            GrowForInsertion(index);
        else if (index < Count)
            Array.Copy(_items, index, _items, index + 1, Count - index);
        _items[index] = item;
        Count++;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        Count--;
        if (index < Count)
            Array.Copy(_items, index + 1, _items, index, Count - index);
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _items[Count] = default!;
    }

    public readonly ref T this[int index] => ref AsSpan()[index];

    T IList<T>.this[int index]
    {
        get => (uint)index >= (uint)Count ? throw new ArgumentOutOfRangeException(nameof(index)) : _items[index];
        set
        {
            if ((uint)index >= (uint)Count)
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
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), 0), Count);
    }

    ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    ValueEnumerable<FromSpan<T>, T> ISpanView<T>.AsValueEnumerable()
    {
        return _items.AsSpan().AsValueEnumerable();
    }

    ValueEnumerator<FromSpan<T>, T> ISpanView<T>.GetEnumerator()
    {
        return new ValueEnumerator<FromSpan<T>, T>(_items.AsSpan().AsValueEnumerable().Enumerator);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ReadOnlySpan<T> ISpanView<T>.AsSpan()
    {
        return AsSpan();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(Count == _items.Length);
        var size = Count;
        Grow(size + 1);
        Count = size + 1;
        _items[size] = item;
    }

    private void Grow(int capacity)
    {
        Capacity = GetNewCapacity(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(_items.Length < capacity);
        var newCapacity = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        if (newCapacity < capacity)
            newCapacity = capacity;
        return newCapacity;
    }

    private void GrowForInsertion(int indexToInsert, int insertionCount = 1)
    {
        Debug.Assert(insertionCount > 0);
        var requiredCapacity = checked(Count + insertionCount);
        var newCapacity = GetNewCapacity(requiredCapacity);
        var newItems = new T[newCapacity];
        if (indexToInsert != 0)
            Array.Copy(_items, newItems, indexToInsert);
        if (Count != indexToInsert)
            Array.Copy(_items, indexToInsert, newItems, indexToInsert + insertionCount, Count - indexToInsert);
        _items = newItems;
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly ValueList<T> _list;
        private int _index;
        private T _current = default!;

        internal Enumerator(ValueList<T> list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_list.Count)
            {
                _current = _list._items[_index];
                _index++;
                return true;
            }

            _current = default!;
            _index = -1;
            return false;
        }

        public T Current => _current!;

        public void Reset()
        {
            _index = 0;
            _current = default!;
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
            count = _list.Count;
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
            if (enumerator.TryCopyTo(result.AsSpan(), 0))
                return result;
            while (enumerator.TryGetNext(out var item))
                result.Add(item);
            return result;
        }
    }
}
