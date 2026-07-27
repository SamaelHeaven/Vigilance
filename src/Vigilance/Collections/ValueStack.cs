using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

[CollectionBuilder(typeof(ValueStackBuilder), nameof(ValueStackBuilder.Create))]
public struct ValueStack<T> : IReadOnlyCollection<T>, IStructEnumerable<ValueStack<T>.Enumerator, T>
{
    private const int DefaultCapacity = 4;

    private T[] _items;

    public ValueStack()
    {
        _items = [];
    }

    public ValueStack(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

    public ValueStack(in ValueStack<T> source)
    {
        _items = source._items.Length == 0 ? [] : (T[])source._items.Clone();
        Count = source.Count;
    }

    public ValueStack(IEnumerable<T> collection)
    {
        _items = collection.ToValueList().AsArray(out var length);
        Count = length;
    }

    [OverloadResolutionPriority(1)]
    public ValueStack(in ReadOnlySpan<T> span)
    {
        _items = span.AsValueEnumerable().ToValueList().AsArray(out var length);
        Count = length;
    }

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
            Array.Clear(_items, 0, Count);
        Count = 0;
    }

    public readonly bool Contains(in T item)
    {
        return Count != 0 && Array.LastIndexOf(_items, item, Count - 1) >= 0;
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        Debug.Assert(array != _items);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    public readonly void CopyTo(in Span<T> span, int arrayIndex = 0)
    {
        if (arrayIndex < 0 || arrayIndex > span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array was not long enough.");
        var srcIndex = 0;
        var dstIndex = arrayIndex + Count;
        while (srcIndex < Count)
            span[--dstIndex] = _items[srcIndex++];
    }

    public readonly void CopyTo(ref ValueStack<T> stack)
    {
        stack.Clear();
        stack.EnsureCapacity(Count);
        _items.AsSpan(0, Count).CopyTo(stack._items);
        stack.Count = Count;
    }

    public void TrimExcess()
    {
        var threshold = (int)(_items.Length * 0.9);
        if (Count < threshold)
            Array.Resize(ref _items, Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        if (capacity == _items.Length)
            return;
        Array.Resize(ref _items, capacity);
    }

    public readonly ref T Peek()
    {
        var size = Count - 1;
        var array = _items;
        if ((uint)size >= (uint)array.Length)
            ThrowForEmptyStack();
        return ref array[size];
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        var size = Count - 1;
        var array = _items;
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
        var array = _items;
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
        var array = _items;
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

    public void Push(in T item)
    {
        var size = Count;
        var array = _items;
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
    private void PushWithResize(in T item)
    {
        Debug.Assert(Count == _items.Length);
        Grow(Count + 1);
        _items[Count] = item;
        Count++;
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
        var newCapacity = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;
        if ((uint)newCapacity > Array.MaxLength)
            newCapacity = Array.MaxLength;
        if (newCapacity < capacity)
            newCapacity = capacity;
        Array.Resize(ref _items, newCapacity);
    }

    public readonly T[] ToArray()
    {
        if (Count == 0)
            return [];
        var array = new T[Count];
        var i = 0;
        while (i < Count)
        {
            array[i] = _items[Count - i - 1];
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
            var array = _stack._items;
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
            var count = _stack.Count;
            var start = offset.GetOffset(count);
            if ((uint)start > (uint)count)
                return false;
            if ((uint)destination.Length > (uint)(count - start))
                return false;
            if (destination.IsEmpty)
                return true;
            var array = _stack._items;
            var sourceIndex = count - start - 1;
            for (var i = 0; i < destination.Length; i++)
                destination[i] = array[sourceIndex - i];
            return true;
        }
    }
}

public static class ValueStackBuilder
{
    public static ValueStack<T> Create<T>(ReadOnlySpan<T> span)
    {
        return new ValueStack<T>(span);
    }
}
