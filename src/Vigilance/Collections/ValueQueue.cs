using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

[CollectionBuilder(typeof(ValueQueueBuilder), nameof(ValueQueueBuilder.Create))]
public struct ValueQueue<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueQueue<T>.Enumerator, T>
{
    private T[] _items;
    private int _head;
    private int _tail;

    public ValueQueue()
    {
        _items = [];
    }

    public ValueQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

    public ValueQueue(in ValueQueue<T> source)
    {
        _items = source._items.Length == 0 ? [] : (T[])source._items.Clone();
        _head = source._head;
        _tail = source._tail;
        Count = source.Count;
    }

    public ValueQueue(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        _items = collection.ToValueList().AsArray(out var length);
        Count = length;
        if (Count != _items.Length)
            _tail = Count;
    }

    [OverloadResolutionPriority(1)]
    public ValueQueue(in ReadOnlySpan<T> span)
    {
        _items = span.AsValueEnumerable().ToValueList().AsArray(out var length);
        Count = length;
        if (Count != _items.Length)
            _tail = Count;
    }

    public int Capacity
    {
        readonly get => _items.Length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, Count);
            if (value == _items.Length)
                return;
            SetCapacity(value);
        }
    }

    public int Count { get; private set; }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    readonly ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    public void Clear()
    {
        if (Count != 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                if (_head < _tail)
                {
                    Array.Clear(_items, _head, Count);
                }
                else
                {
                    Array.Clear(_items, _head, _items.Length - _head);
                    Array.Clear(_items, 0, _tail);
                }
            }

            Count = 0;
        }

        _head = 0;
        _tail = 0;
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    public readonly void CopyTo(in Span<T> span, int arrayIndex = 0)
    {
        if (arrayIndex < 0 || arrayIndex > span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array was not long enough.");
        if (Count == 0)
            return;
        var firstPart = System.Math.Min(_items.Length - _head, Count);
        _items.AsSpan(_head, firstPart).CopyTo(span[arrayIndex..]);
        var remaining = Count - firstPart;
        if (remaining > 0)
            _items.AsSpan(0, remaining).CopyTo(span[(arrayIndex + firstPart)..]);
    }

    public readonly void CopyTo(ref ValueQueue<T> queue)
    {
        queue.Clear();
        queue.EnsureCapacity(Count);
        CopyTo(queue._items);
        queue._head = 0;
        queue._tail = queue._items.Length == Count ? 0 : Count;
        queue.Count = Count;
    }

    public void Enqueue(in T item)
    {
        if (Count == _items.Length)
            Grow(Count + 1);
        _items[_tail] = item;
        MoveNext(ref _tail);
        Count++;
    }

    public T Dequeue()
    {
        var head = _head;
        var array = _items;
        if (Count == 0)
            ThrowForEmptyQueue();
        var removed = array[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[head] = default!;
        MoveNext(ref _head);
        Count--;
        return removed;
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        var head = _head;
        var array = _items;
        if (Count == 0)
        {
            result = default;
            return false;
        }

        result = array[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[head] = default!;
        MoveNext(ref _head);
        Count--;
        return true;
    }

    public readonly ref T Peek()
    {
        if (Count == 0)
            ThrowForEmptyQueue();
        return ref _items[_head];
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (Count == 0)
        {
            result = default;
            return false;
        }

        result = _items[_head];
        return true;
    }

    public readonly bool Contains(in T item)
    {
        if (Count == 0)
            return false;
        if (_head < _tail)
            return Array.IndexOf(_items, item, _head, Count) >= 0;
        return Array.IndexOf(_items, item, _head, _items.Length - _head) >= 0
            || Array.IndexOf(_items, item, 0, _tail) >= 0;
    }

    public readonly T[] ToArray()
    {
        if (Count == 0)
            return [];
        var arr = new T[Count];
        if (_head < _tail)
        {
            Array.Copy(_items, _head, arr, 0, Count);
        }
        else
        {
            Array.Copy(_items, _head, arr, 0, _items.Length - _head);
            Array.Copy(_items, 0, arr, _items.Length - _head, _tail);
        }

        return arr;
    }

    private void SetCapacity(int capacity)
    {
        Debug.Assert(capacity >= Count);
        var newArray = capacity == 0 ? [] : new T[capacity];
        if (Count > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_items, _head, newArray, 0, Count);
            }
            else
            {
                Array.Copy(_items, _head, newArray, 0, _items.Length - _head);
                Array.Copy(_items, 0, newArray, _items.Length - _head, _tail);
            }
        }

        _items = newArray;
        _head = 0;
        _tail = Count == capacity ? 0 : Count;
    }

    private readonly void MoveNext(ref int index)
    {
        var tmp = index + 1;
        if (tmp == _items.Length)
            tmp = 0;
        index = tmp;
    }

    private readonly void ThrowForEmptyQueue()
    {
        Debug.Assert(Count == 0);
        throw new InvalidOperationException("Queue is empty.");
    }

    public void TrimExcess()
    {
        var threshold = (int)(_items.Length * 0.9);
        if (Count < threshold)
            SetCapacity(Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity == _items.Length)
            return;
        SetCapacity(capacity);
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (_items.Length < capacity)
            Grow(capacity);
        return _items.Length;
    }

    private void Grow(int capacity)
    {
        Debug.Assert(_items.Length < capacity);
        const int growFactor = 2;
        const int minimumGrow = 4;
        var newCapacity = growFactor * _items.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        newCapacity = System.Math.Max(newCapacity, _items.Length + minimumGrow);
        if (newCapacity < capacity)
            newCapacity = capacity;
        SetCapacity(newCapacity);
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly ValueQueue<T> _queue;
        private int _i;
        private T? _currentElement;

        internal Enumerator(ValueQueue<T> queue)
        {
            _queue = queue;
            _i = -1;
            _currentElement = default;
        }

        public bool MoveNext()
        {
            var q = _queue;
            var size = q.Count;
            var offset = _i + 1;
            if ((uint)offset < (uint)size)
            {
                _i = offset;
                var array = q._items;
                var index = q._head + offset;
                if (!((uint)index < (uint)array.Length))
                    index -= array.Length;
                _currentElement = array[index];
                return true;
            }

            _i = -2;
            _currentElement = default;
            return false;
        }

        public void Dispose()
        {
            _i = -2;
            _currentElement = default;
        }

        public T Current => _currentElement!;

        public void Reset()
        {
            _i = -1;
            _currentElement = default;
        }

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
            count = _queue.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            var count = _queue.Count;
            var start = offset.GetOffset(count);
            if ((uint)start > (uint)count)
                return false;
            if ((uint)destination.Length > (uint)(count - start))
                return false;
            if (destination.IsEmpty)
                return true;
            var array = _queue._items;
            var index = _queue._head + start;
            if (index >= array.Length)
                index -= array.Length;
            var firstPart = System.Math.Min(array.Length - index, destination.Length);
            array.AsSpan(index, firstPart).CopyTo(destination);
            var remaining = destination.Length - firstPart;
            if (remaining > 0)
                array.AsSpan(0, remaining).CopyTo(destination[firstPart..]);
            return true;
        }
    }
}

public static class ValueQueueBuilder
{
    public static ValueQueue<T> Create<T>(ReadOnlySpan<T> span)
    {
        return new ValueQueue<T>(span);
    }
}
