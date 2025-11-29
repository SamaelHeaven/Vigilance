using System.Buffers;
using System.Runtime.CompilerServices;
using Vigilance.Math;
using ZLinq;
using ZLinq.Traversables;

namespace Vigilance.Core;

public static class ZLinqExtensions
{
    extension<TEnumerator, TValue>(in ValueEnumerable<TEnumerator, TValue> enumerable)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public ValueEnumerableAdapter<TEnumerator, TValue> AsEnumerable()
        {
            return new ValueEnumerableAdapter<TEnumerator, TValue>(enumerable);
        }
    }

    extension<TTraverser, T>(TTraverser traverser)
        where TTraverser : struct, ITraverser<TTraverser, T>
    {
        public ValueEnumerable<Descendants<TTraverser, T>, T> DescendantsPreOrder()
        {
            return new ValueEnumerable<Descendants<TTraverser, T>, T>(new Descendants<TTraverser, T>(traverser, false));
        }

        public ValueEnumerable<Descendants<TTraverser, T>, T> DescendantsPreOrderAndSelf()
        {
            return new ValueEnumerable<Descendants<TTraverser, T>, T>(new Descendants<TTraverser, T>(traverser, true));
        }

        public ValueEnumerable<DescendantsPostOrder<TTraverser, T>, T> DescendantsPostOrder()
        {
            return new ValueEnumerable<DescendantsPostOrder<TTraverser, T>, T>(
                new DescendantsPostOrder<TTraverser, T>(traverser, false)
            );
        }

        public ValueEnumerable<DescendantsPostOrder<TTraverser, T>, T> DescendantsPostOrderAndSelf()
        {
            return new ValueEnumerable<DescendantsPostOrder<TTraverser, T>, T>(
                new DescendantsPostOrder<TTraverser, T>(traverser, true)
            );
        }

        public ValueEnumerable<DescendantsLevelOrder<TTraverser, T>, T> DescendantsLevelOrder()
        {
            return new ValueEnumerable<DescendantsLevelOrder<TTraverser, T>, T>(
                new DescendantsLevelOrder<TTraverser, T>(traverser, false)
            );
        }

        public ValueEnumerable<DescendantsLevelOrder<TTraverser, T>, T> DescendantsLevelOrderAndSelf()
        {
            return new ValueEnumerable<DescendantsLevelOrder<TTraverser, T>, T>(
                new DescendantsLevelOrder<TTraverser, T>(traverser, true)
            );
        }
    }
}

public readonly struct ValueEnumerableAdapter<TEnumerator, TValue>
    : IStructEnumerable<ValueEnumerableAdapter<TEnumerator, TValue>.Enumerator, TValue>
    where TEnumerator : struct, IValueEnumerator<TValue>
{
    private readonly TEnumerator _enumerator;

    internal ValueEnumerableAdapter(in ValueEnumerable<TEnumerator, TValue> enumerable)
    {
        _enumerator = enumerable.Enumerator;
    }

    public struct Enumerator : IStructEnumerator<TValue>
    {
        private readonly ValueEnumerableAdapter<TEnumerator, TValue> _adapter;
        private TEnumerator _enumerator;
        private TValue _current = default!;

        internal Enumerator(in ValueEnumerableAdapter<TEnumerator, TValue> adapter)
        {
            _adapter = adapter;
            _enumerator = adapter._enumerator;
        }

        public TValue Current => _current;

        public void Reset()
        {
            _enumerator = _adapter._enumerator;
            _current = default!;
        }

        public bool MoveNext()
        {
            return _enumerator.TryGetNext(out _current);
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<TEnumerator, TValue> AsValueEnumerable()
    {
        return new ValueEnumerable<TEnumerator, TValue>(_enumerator);
    }

    ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> IStructEnumerable<
        Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
    }
}

public struct DescendantsPostOrder<TTraverser, T> : IValueEnumerator<T>
    where TTraverser : struct, ITraverser<TTraverser, T>
{
    private T[]? _stack;
    private int _top;
    private int _count;
    private TTraverser _traverser;
    private readonly bool _withSelf;

    public DescendantsPostOrder(in TTraverser traverser, bool withSelf)
    {
        _traverser = traverser;
        _withSelf = withSelf;
        _stack = null;
        _top = 0;
        _count = 0;
    }

    public bool TryGetNext(out T current)
    {
        Initialize();
        if (_top == 0)
        {
            Unsafe.SkipInit(out current);
            return false;
        }

        current = _stack![--_top];
        return true;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        Initialize();
        count = _count;
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

    public void Dispose()
    {
        _traverser.Dispose();
        if (_stack is not null)
        {
            ArrayPool<T>.Shared.Return(_stack, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            _stack = null;
        }

        _top = 0;
    }

    private void Initialize()
    {
        if (_stack is not null)
            return;
        int count;
        count = _traverser.TryGetChildCount(out count) ? count : -1;
        var top = 0;
        var stack = count >= 0 ? ArrayPool<T>.Shared.Rent(count.Max(16)) : ArrayPool<T>.Shared.Rent(16);
        try
        {
            _stack = count >= 0 ? ArrayPool<T>.Shared.Rent(count.Max(16)) : ArrayPool<T>.Shared.Rent(16);
            if (_withSelf)
                Push(_traverser.Origin, ref stack, ref top);
            else
                foreach (var child in _traverser.Children<TTraverser, T>())
                    Push(child, ref stack, ref top);
            while (top > 0)
            {
                ref readonly var node = ref stack![--top];
                Push(node, ref _stack, ref _top);
                using var traverser = _traverser.ConvertToTraverser(node);
                foreach (var child in traverser.Children<TTraverser, T>())
                    Push(child, ref stack, ref top);
            }
        }
        finally
        {
            ArrayPool<T>.Shared.Return(stack!, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }

        _count = _top;
    }

    private static void Push(in T item, ref T[]? stack, ref int top)
    {
        if (top == stack!.Length)
            Grow(ref stack);
        stack![top++] = item;
    }

    private static void Grow(ref T[]? stack)
    {
        var array = ArrayPool<T>.Shared.Rent(stack!.Length * 2);
        Array.Copy(stack, array, stack.Length);
        ArrayPool<T>.Shared.Return(stack, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        stack = array;
    }
}

public struct DescendantsLevelOrder<TTraverser, T> : IValueEnumerator<T>
    where TTraverser : struct, ITraverser<TTraverser, T>
{
    private T[]? _queue = null;
    private TTraverser _traverser;
    private readonly bool _withSelf;
    private int _head;
    private int _tail;
    private int _count;

    public DescendantsLevelOrder(in TTraverser traverser, bool withSelf)
    {
        _traverser = traverser;
        _withSelf = withSelf;
    }

    public bool TryGetNext(out T current)
    {
        if (_queue is null)
        {
            _queue = _traverser.TryGetChildCount(out var count)
                ? ArrayPool<T>.Shared.Rent(count.Max(16))
                : ArrayPool<T>.Shared.Rent(16);
            if (_withSelf)
                Enqueue(_traverser.Origin);
            else
                foreach (var child in _traverser.Children<TTraverser, T>())
                    Enqueue(child);
        }

        if (!TryDequeue(out current))
            return false;
        using var traverser = _traverser.ConvertToTraverser(current);
        foreach (var child in traverser.Children<TTraverser, T>())
            Enqueue(child);
        return true;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
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

    public void Dispose()
    {
        _traverser.Dispose();
        if (_queue is null)
            return;
        ArrayPool<T>.Shared.Return(_queue, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _queue = null;
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    private void Enqueue(in T item)
    {
        if (_count == _queue!.Length)
            Grow();
        _queue![_tail] = item;
        _tail = (_tail + 1) % _queue!.Length;
        _count++;
    }

    public bool TryDequeue(out T value)
    {
        if (_count == 0)
        {
            Unsafe.SkipInit(out value);
            return false;
        }

        value = _queue![_head];
        _queue![_head] = default!;
        _head = (_head + 1) % _queue!.Length;
        _count--;
        return true;
    }

    private void Grow()
    {
        var newSize = _queue!.Length * 2;
        var newArray = ArrayPool<T>.Shared.Rent(newSize);
        if (_count > 0)
        {
            if (_head < _tail)
            {
                Array.Copy(_queue, _head, newArray, 0, _count);
            }
            else
            {
                var firstPart = _queue.Length - _head;
                Array.Copy(_queue, _head, newArray, 0, firstPart);
                Array.Copy(_queue, 0, newArray, firstPart, _tail);
            }
        }

        ArrayPool<T>.Shared.Return(_queue, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _queue = newArray;
        _head = 0;
        _tail = _count;
    }
}
