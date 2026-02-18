using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueStack<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueStack<T>.Enumerator, T>
{
    private const int DefaultCapacity = 4;

    private T[] _array;

    public ValueStack()
    {
        _array = [];
    }

    public ValueStack(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _array = new T[capacity];
    }

    public ValueStack(IEnumerable<T> collection)
    {
        _array = collection.ToValueList().AsArray(out var length);
        Count = length;
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
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(_array, 0, Count);
        Count = 0;
    }

    public readonly bool Contains(T item)
    {
        return Count != 0 && Array.LastIndexOf(_array, item, Count - 1) >= 0;
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array was not long enough.");
        Debug.Assert(array != _array);
        var srcIndex = 0;
        var dstIndex = arrayIndex + Count;
        while (srcIndex < Count)
            array[--dstIndex] = _array[srcIndex++];
    }

    public void TrimExcess()
    {
        var threshold = (int)(_array.Length * 0.9);
        if (Count < threshold)
            Array.Resize(ref _array, Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity == _array.Length)
            return;
        Array.Resize(ref _array, capacity);
    }

    public readonly T Peek()
    {
        var size = Count - 1;
        var array = _array;
        if ((uint)size >= (uint)array.Length)
            ThrowForEmptyStack();
        return array[size];
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        var size = Count - 1;
        var array = _array;
        if ((uint)size >= (uint)array.Length)
        {
            result = default;
            return false;
        }

        result = array[size];
        return true;
    }

    public T Pop()
    {
        var size = Count - 1;
        var array = _array;
        if ((uint)size >= (uint)array.Length)
            ThrowForEmptyStack();
        Count = size;
        var item = array[size];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[size] = default!;
        return item;
    }

    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        var size = Count - 1;
        var array = _array;
        if ((uint)size >= (uint)array.Length)
        {
            result = default;
            return false;
        }

        Count = size;
        result = array[size];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[size] = default!;
        return true;
    }

    public void Push(T item)
    {
        var size = Count;
        var array = _array;
        if ((uint)size < (uint)array.Length)
        {
            array[size] = item;
            Count = size + 1;
        }
        else
        {
            PushWithResize(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PushWithResize(T item)
    {
        Debug.Assert(Count == _array.Length);
        Grow(Count + 1);
        _array[Count] = item;
        Count++;
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
        var newCapacity = _array.Length == 0 ? DefaultCapacity : 2 * _array.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        if (newCapacity < capacity)
            newCapacity = capacity;
        Array.Resize(ref _array, newCapacity);
    }

    public readonly T[] ToArray()
    {
        if (Count == 0)
            return [];
        var array = new T[Count];
        var i = 0;
        while (i < Count)
        {
            array[i] = _array[Count - i - 1];
            i++;
        }

        return array;
    }

    private readonly void ThrowForEmptyStack()
    {
        Debug.Assert(Count == 0);
        throw new InvalidOperationException("Stack is empty.");
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly ValueStack<T> _stack;
        private int _index;
        private T? _currentElement;

        internal Enumerator(ValueStack<T> stack)
        {
            _stack = stack;
            _index = stack.Count;
            _currentElement = default;
        }

        public bool MoveNext()
        {
            var array = _stack._array;
            var index = _index - 1;
            if ((uint)index < (uint)array.Length)
            {
                _currentElement = array[index];
                _index = index;
                return true;
            }

            _currentElement = default;
            _index = -1;
            return false;
        }

        public void Dispose()
        {
            _index = -1;
            _currentElement = default;
        }

        public T Current => _currentElement!;

        public void Reset()
        {
            _index = _stack.Count;
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
            count = _stack.Count;
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
