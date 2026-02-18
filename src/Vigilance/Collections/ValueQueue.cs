using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueQueue<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueQueue<T>.Enumerator, T>
{
    private T[] _array;
    private int _head;
    private int _tail;

    public ValueQueue()
    {
        _array = [];
    }

    public ValueQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _array = new T[capacity];
    }

    public ValueQueue(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        _array = collection.ToValueList().AsArray(out var length);
        Count = length;
        if (Count != _array.Length)
            _tail = Count;
    }

    public readonly int Capacity => _array.Length;

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
                    Array.Clear(_array, _head, Count);
                }
                else
                {
                    Array.Clear(_array, _head, _array.Length - _head);
                    Array.Clear(_array, 0, _tail);
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
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array was not long enough.");
        var numToCopy = Count;
        if (numToCopy == 0)
            return;
        var firstPart = System.Math.Min(_array.Length - _head, numToCopy);
        Array.Copy(_array, _head, array, arrayIndex, firstPart);
        numToCopy -= firstPart;
        if (numToCopy > 0)
            Array.Copy(_array, 0, array, arrayIndex + _array.Length - _head, numToCopy);
    }

    public void Enqueue(T item)
    {
        if (Count == _array.Length)
            Grow(Count + 1);
        _array[_tail] = item;
        MoveNext(ref _tail);
        Count++;
    }

    public T Dequeue()
    {
        var head = _head;
        var array = _array;
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
        var array = _array;
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

    public readonly T Peek()
    {
        if (Count == 0)
            ThrowForEmptyQueue();
        return _array[_head];
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (Count == 0)
        {
            result = default;
            return false;
        }

        result = _array[_head];
        return true;
    }

    public readonly bool Contains(T item)
    {
        if (Count == 0)
            return false;
        if (_head < _tail)
            return Array.IndexOf(_array, item, _head, Count) >= 0;
        return Array.IndexOf(_array, item, _head, _array.Length - _head) >= 0
            || Array.IndexOf(_array, item, 0, _tail) >= 0;
    }

    public readonly T[] ToArray()
    {
        if (Count == 0)
            return [];
        var arr = new T[Count];
        if (_head < _tail)
        {
            Array.Copy(_array, _head, arr, 0, Count);
        }
        else
        {
            Array.Copy(_array, _head, arr, 0, _array.Length - _head);
            Array.Copy(_array, 0, arr, _array.Length - _head, _tail);
        }

        return arr;
    }

    private void SetCapacity(int capacity)
    {
        Debug.Assert(capacity >= Count);
        var newArray = new T[capacity];
        if (Count > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_array, _head, newArray, 0, Count);
            }
            else
            {
                Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
                Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
            }
        }

        _array = newArray;
        _head = 0;
        _tail = Count == capacity ? 0 : Count;
    }

    private readonly void MoveNext(ref int index)
    {
        var tmp = index + 1;
        if (tmp == _array.Length)
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
        var threshold = (int)(_array.Length * 0.9);
        if (Count < threshold)
            SetCapacity(Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity == _array.Length)
            return;
        SetCapacity(capacity);
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (_array.Length < capacity)
            Grow(capacity);
        return _array.Length;
    }

    private void Grow(int capacity)
    {
        Debug.Assert(_array.Length < capacity);
        const int growFactor = 2;
        const int minimumGrow = 4;
        var newCapacity = growFactor * _array.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        newCapacity = System.Math.Max(newCapacity, _array.Length + minimumGrow);
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
                var array = q._array;
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
            return false;
        }
    }
}
