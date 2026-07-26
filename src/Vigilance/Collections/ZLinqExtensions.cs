using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Traversables;

namespace Vigilance.Collections;

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

    extension<TEnumerator, TValue>(in ValueEnumerable<TEnumerator, TValue> enumerable)
        where TEnumerator : struct, IValueEnumerator<TValue>, allows ref struct
    {
        public ValueEnumerable<PooledEnumerator<TEnumerator, TValue>, TValue> AsPooled()
        {
            return new ValueEnumerable<PooledEnumerator<TEnumerator, TValue>, TValue>(
                new PooledEnumerator<TEnumerator, TValue>(enumerable)
            );
        }

        public ValueEnumerable<
            CrossEnumerator<TEnumerator, TValue, TEnumerator2, TRight>,
            (TValue Left, TRight Right)
        > Cross<TEnumerator2, TRight>(in ValueEnumerable<TEnumerator2, TRight> other)
            where TEnumerator2 : struct, IValueEnumerator<TRight>, allows ref struct
        {
            return new ValueEnumerable<CrossEnumerator<TEnumerator, TValue, TEnumerator2, TRight>, (TValue, TRight)>(
                new CrossEnumerator<TEnumerator, TValue, TEnumerator2, TRight>(enumerable, other)
            );
        }

        public ReadOnlySpan<TValue> AsSpan()
        {
            using var enumerator = enumerable.Enumerator;
            if (enumerator.TryGetSpan(out var span))
                return span;
            return enumerable.ToArray();
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

public ref struct CrossEnumerator<TEnumerator, TLeft, TEnumerator2, TRight>
    : IValueEnumerator<(TLeft Left, TRight Right)>
    where TEnumerator : struct, IValueEnumerator<TLeft>, allows ref struct
    where TEnumerator2 : struct, IValueEnumerator<TRight>, allows ref struct
{
    private TEnumerator _outer;
    private readonly ValueEnumerable<TEnumerator2, TRight> _otherEnumerable;
    private TEnumerator2 _inner;
    private TLeft _outerCurrent;
    private bool _hasOuterCurrent;
    private bool _innerInitialized;

    public CrossEnumerator(
        in ValueEnumerable<TEnumerator, TLeft> enumerable,
        in ValueEnumerable<TEnumerator2, TRight> other
    )
    {
        _outer = enumerable.Enumerator;
        _otherEnumerable = other;
        _inner = default;
        _outerCurrent = default!;
        _hasOuterCurrent = false;
        _innerInitialized = false;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
    }

    public bool TryGetSpan(out ReadOnlySpan<(TLeft Left, TRight Right)> span)
    {
        span = default;
        return false;
    }

    public bool TryCopyTo(scoped Span<(TLeft Left, TRight Right)> destination, Index offset)
    {
        return false;
    }

    public bool TryGetNext(out (TLeft Left, TRight Right) current)
    {
        while (true)
        {
            if (!_hasOuterCurrent)
            {
                if (!_outer.TryGetNext(out _outerCurrent))
                {
                    Unsafe.SkipInit(out current);
                    return false;
                }

                _inner = _otherEnumerable.Enumerator;
                _innerInitialized = true;
                _hasOuterCurrent = true;
            }

            if (_inner.TryGetNext(out var otherCurrent))
            {
                current = (_outerCurrent, otherCurrent);
                return true;
            }

            _inner.Dispose();
            _inner = default;
            _innerInitialized = false;
            _hasOuterCurrent = false;
        }
    }

    public void Dispose()
    {
        if (_innerInitialized)
            _inner.Dispose();
        _outer.Dispose();
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

public ref struct PooledEnumerator<TEnumerator, TValue> : IValueEnumerator<TValue>, IReadOnlySpan<TValue>
    where TEnumerator : struct, IValueEnumerator<TValue>, allows ref struct
{
    private readonly ValueEnumerable<TEnumerator, TValue> _enumerable;
    private TValue[]? _array;
    private int _count;
    private int _index;

    internal PooledEnumerator(in ValueEnumerable<TEnumerator, TValue> enumerable)
    {
        _enumerable = enumerable;
        Reset();
    }

    public bool MoveNext()
    {
        InitArray();
        if ((uint)_index < (uint)_count)
        {
            Current = _array![_index];
            _index++;
            return true;
        }

        Current = default!;
        _index = -1;
        return false;
    }

    public void Reset()
    {
        Dispose();
        _array = null;
        _index = 0;
        Current = default!;
    }

    public TValue Current { get; private set; } = default!;

    public void Dispose()
    {
        if (_array is null)
            return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            Array.Clear(_array, 0, _count);
        ArrayPool<TValue>.Shared.Return(_array);
        _array = null;
        _index = -1;
        Current = default!;
    }

    private void InitArray()
    {
        if (_array is not null)
            return;
        using var enumerator = _enumerable.Enumerator;
        if (enumerator.TryGetNonEnumeratedCount(out _count))
        {
            _array = ArrayPool<TValue>.Shared.Rent(_count);
            if (enumerator.TryGetSpan(out var span))
            {
                span.CopyTo(_array);
            }
            else if (!enumerator.TryCopyTo(_array.AsSpan(0, _count), Index.Start))
            {
                var i = 0;
                while (enumerator.TryGetNext(out var item))
                {
                    _array[i] = item;
                    i++;
                }
            }
        }
        else
        {
            var initialBuffer = default(InlineArray16<TValue>);
            Span<TValue> initialBufferSpan = initialBuffer;
            var arrayBuilder = new SegmentedArrayProvider<TValue>(initialBufferSpan);
            var span = arrayBuilder.GetSpan();
            var i = 0;
            while (enumerator.TryGetNext(out var item))
            {
                if (i == span.Length)
                {
                    arrayBuilder.Advance(i);
                    span = arrayBuilder.GetSpan();
                    i = 0;
                }

                span[i] = item;
                i++;
            }

            arrayBuilder.Advance(i);
            _count = arrayBuilder.Count;
            _array = ArrayPool<TValue>.Shared.Rent(_count);
            arrayBuilder.CopyToAndClear(_array);
        }
    }

    public bool TryGetNext(out TValue current)
    {
        Unsafe.SkipInit(out current);
        var result = MoveNext();
        if (result)
            current = Current;
        return result;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        return _enumerable.TryGetNonEnumeratedCount(out count);
    }

    public bool TryGetSpan(out ReadOnlySpan<TValue> span)
    {
        InitArray();
        span = _array.AsSpan(0, _count);
        return true;
    }

    public bool TryCopyTo(scoped Span<TValue> destination, Index offset)
    {
        InitArray();
        return _array.AsSpan(0, _count).TryCopyTo(destination, offset);
    }

    public Span<TValue> AsSpan()
    {
        InitArray();
        return _array.AsSpan(0, _count);
    }

    ReadOnlySpan<TValue> IReadOnlySpan<TValue>.AsSpan()
    {
        InitArray();
        return _array.AsSpan(0, _count);
    }
}

public struct DescendantsPostOrder<TTraverser, T> : IValueEnumerator<T>
    where TTraverser : struct, ITraverser<TTraverser, T>
{
    private RefStack<TTraverser>? _stack;
    private TTraverser _traverser;
    private bool _withSelf;

    public DescendantsPostOrder(in TTraverser traverser, bool withSelf)
    {
        _stack = null;
        _traverser = traverser;
        _withSelf = withSelf;
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

    public bool TryGetNext(out T current)
    {
        if (_stack == RefStack<TTraverser>.DisposeSentinel)
        {
            Unsafe.SkipInit(out current);
            return false;
        }

        if (_stack is null)
        {
            _stack = RefStack<TTraverser>.Rent();
            foreach (var child in _traverser.Children<TTraverser, T>())
                _stack.Push(_traverser.ConvertToTraverser(child));
            _stack.AsSpan().Reverse();
        }

        ref var traverser = ref _stack.PeekRefOrNullRef();
        while (!Unsafe.IsNullRef(ref traverser))
        {
            _stack.Pop();
            if (traverser.TryGetNextChild(out var child))
            {
                _stack.Push(traverser);
                using var subTraversable = _traverser.ConvertToTraverser(child);
                _stack.Push(subTraversable);
            }
            else
            {
                current = traverser.Origin;
                traverser.Dispose();
                return true;
            }

            traverser = ref _stack.PeekRefOrNullRef();
        }

        if (_withSelf)
        {
            current = _traverser.Origin;
            _withSelf = false;
            return true;
        }

        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_stack is null)
            return;
        RefStack<TTraverser>.Return(_stack);
        _stack = RefStack<TTraverser>.DisposeSentinel;
    }
}

public struct DescendantsLevelOrder<TTraverser, T> : IValueEnumerator<T>
    where TTraverser : struct, ITraverser<TTraverser, T>
{
    private RefQueue<Children<TTraverser, T>>? _queue;
    private TTraverser _traverser;
    private bool _withSelf;

    public DescendantsLevelOrder(in TTraverser traverser, bool withSelf)
    {
        _queue = null;
        _traverser = traverser;
        _withSelf = withSelf;
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

    public bool TryCopyTo(Span<T> destination, Index offset)
    {
        return false;
    }

    public bool TryGetNext(out T current)
    {
        if (_queue == RefQueue<Children<TTraverser, T>>.DisposeSentinel)
        {
            Unsafe.SkipInit(out current);
            return false;
        }

        if (_withSelf)
        {
            current = _traverser.Origin;
            _withSelf = false;
            return true;
        }

        if (_queue is null)
        {
            var children = _traverser.Children<TTraverser, T>();
            _queue = RefQueue<Children<TTraverser, T>>.Rent();
            _queue.Enqueue(children.Enumerator);
        }

        ref var enumerator = ref _queue.PeekRefOrNullRef();
        while (!Unsafe.IsNullRef(ref enumerator))
        {
            while (enumerator.TryGetNext(out var value))
            {
                current = value;
                using var subTraversable = _traverser.ConvertToTraverser(value);
                if (subTraversable.TryGetHasChild(out var hasChild) && !hasChild)
                    return true;
                var children = subTraversable.Children<TTraverser, T>();
                _queue.Enqueue(children.Enumerator);
                return true;
            }

            enumerator.Dispose();
            _queue.Dequeue();
            enumerator = ref _queue.PeekRefOrNullRef();
        }

        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_queue is null)
            return;
        RefQueue<Children<TTraverser, T>>.Return(_queue);
        _queue = RefQueue<Children<TTraverser, T>>.DisposeSentinel;
    }
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
internal sealed class RefStack<T>
    where T : IDisposable
{
    internal static readonly RefStack<T> DisposeSentinel = new(0);
    private static volatile int _gate = 0;
    private static volatile RefStack<T>? _last = null;
    private T[] _array;
    private RefStack<T>? _prev = null;
    private int _size = 0;

    private RefStack(int initialSize)
    {
        _array = initialSize == 0 ? [] : new T[initialSize];
        _size = 0;
    }

    public static RefStack<T> Rent()
    {
        if (Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            return new RefStack<T>(4);
        if (_last == null)
        {
            _gate = 0;
            return new RefStack<T>(4);
        }

        var rent = _last;
        _last = _last._prev;
        _gate = 0;
        return rent;
    }

    public static void Return(RefStack<T> stack)
    {
        stack.Reset();
        if (Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            return;
        stack._prev = _last;
        _last = stack;
        _gate = 0;
    }

    public Span<T> AsSpan()
    {
        return _array.AsSpan(0, _size);
    }

    public void Push(in T value)
    {
        if (_size == _array.Length)
            Array.Resize(ref _array, _array.Length * 2);
        _array[_size++] = value;
    }

    public void Pop()
    {
        _size--;
    }

    public ref T PeekRefOrNullRef()
    {
        if (_size == 0)
            return ref Unsafe.NullRef<T>();
        return ref _array[_size - 1];
    }

    public void Reset()
    {
        for (var i = 0; i < _size; i++)
            _array[i].Dispose();
        Array.Clear(_array, 0, _size);
        _size = 0;
    }
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
internal sealed class RefQueue<T>
    where T : IDisposable
{
    internal static readonly RefQueue<T> DisposeSentinel = new(0);
    private static volatile int _gate = 0;
    private static volatile RefQueue<T>? _last = null;
    private T[] _array;
    private int _head = 0;
    private RefQueue<T>? _prev = null;
    private int _tail = 0;

    private RefQueue(int initialSize)
    {
        _array = initialSize == 0 ? [] : new T[initialSize];
    }

    public static RefQueue<T> Rent()
    {
        if (Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            return new RefQueue<T>(4);
        if (_last is null)
        {
            _gate = 0;
            return new RefQueue<T>(4);
        }

        var rent = _last;
        _last = _last._prev;
        _gate = 0;
        return rent;
    }

    public static void Return(RefQueue<T> queue)
    {
        queue.Reset();
        if (Interlocked.CompareExchange(ref _gate, 1, 0) != 0)
            return;
        queue._prev = _last;
        _last = queue;
        _gate = 0;
    }

    public ref T PeekRefOrNullRef()
    {
        if (_head == _tail)
            return ref Unsafe.NullRef<T>();
        return ref _array[_head];
    }

    public void Enqueue(in T value)
    {
        if (_tail == _array.Length)
        {
            if (_head > 0)
            {
                Array.Copy(_array, _head, _array, 0, _tail - _head);
                _tail -= _head;
                _head = 0;
            }
            else
            {
                Array.Resize(ref _array, _array.Length * 2);
            }
        }

        _array[_tail++] = value;
    }

    public void Dequeue()
    {
        _head++;
    }

    public void Reset()
    {
        for (var i = _head; i < _tail; i++)
            _array[i].Dispose();
        Array.Clear(_array, _head, _tail - _head);
        _head = 0;
        _tail = 0;
    }
}
