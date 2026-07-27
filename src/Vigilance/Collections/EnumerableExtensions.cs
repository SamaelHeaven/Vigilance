using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Collections;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        [OverloadResolutionPriority(-1)]
        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(enumerable);
        }

        public FastEnumerable<T> AsFastEnumerable()
        {
            return new FastEnumerable<T>(enumerable);
        }

        public PooledEnumerable<T> AsPooled()
        {
            return new PooledEnumerable<T>(enumerable);
        }

        public IEnumerable<(T Left, TRight Right)> Cross<TRight>(IEnumerable<TRight> other)
        {
            var otherEnumerable = other.AsFastEnumerable();
            foreach (var left in enumerable.AsFastEnumerable())
            foreach (var right in otherEnumerable)
                yield return (left, right);
        }
    }
}

public readonly struct ReadOnlyCollection<T> : ICollection<T>, IStructEnumerable<ReadOnlyCollection<T>.Enumerator, T>
{
    private readonly IEnumerable<T> _enumerable;

    public ReadOnlyCollection(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private IEnumerator<T>? _enumerator;

        internal Enumerator(ReadOnlyCollection<T> collection)
        {
            _enumerable = collection._enumerable;
        }

        public bool MoveNext()
        {
            _enumerator ??= _enumerable!.GetEnumerator();
            if (!_enumerator.MoveNext())
                return false;
            Current = _enumerator.Current;
            return true;
        }

        public void Reset()
        {
            Dispose();
        }

        public T Current { get; private set; } = default!;

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null;
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
            using var enumerator = _enumerable.AsFastEnumerable().AsValueEnumerable().Enumerator;
            return enumerator.TryGetNonEnumeratedCount(out count);
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            using var enumerator = _enumerable.AsFastEnumerable().AsValueEnumerable().Enumerator;
            return enumerator.TryGetSpan(out span);
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            using var enumerator = _enumerable.AsFastEnumerable().AsValueEnumerable().Enumerator;
            return enumerator.TryCopyTo(destination, offset);
        }
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Contains(T item)
    {
        return AsValueEnumerable().Contains(item);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        AsValueEnumerable().CopyTo(array.AsSpan(arrayIndex));
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    int ICollection<T>.Count => AsValueEnumerable().Count();

    bool ICollection<T>.IsReadOnly => true;
}

public readonly struct FastEnumerable<T> : ICollection<T>, IStructEnumerable<FastEnumerable<T>.Enumerator, T>
{
    private readonly IEnumerable<T> _enumerable;
    private readonly T[]? _array;
    private readonly List<T>? _list;
    private readonly IReadOnlySpan<T>? _span;
    private readonly SourceKind _kind;

    private enum SourceKind : sbyte
    {
        Enumerable,
        Array,
        List,
        IReadOnlySpan,
    }

    public FastEnumerable(IEnumerable<T> enumerable)
    {
        _array = null;
        _list = null;
        _span = null;
        _kind = SourceKind.Enumerable;
        _enumerable = enumerable;
        switch (enumerable)
        {
            case T[] array:
                _array = array;
                _kind = SourceKind.Array;
                break;
            case List<T> list:
                _list = list;
                _kind = SourceKind.List;
                break;
            case IReadOnlySpan<T> span:
                _span = span;
                _kind = SourceKind.IReadOnlySpan;
                break;
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly FastEnumerable<T> _enumerable;
        private IEnumerator<T>? _enumerator;
        private List<T>.Enumerator _listEnumerator;
        private bool _hasListEnumerator;
        private int _index;

        internal Enumerator(in FastEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
            _listEnumerator = default;
            _hasListEnumerator = false;
            _index = 0;
        }

        public T Current { get; private set; } = default!;

        public bool MoveNext()
        {
            switch (_enumerable._kind)
            {
                case SourceKind.Array:
                {
                    var array = _enumerable._array!;
                    if ((uint)_index < (uint)array.Length)
                    {
                        Current = array[_index];
                        _index++;
                        return true;
                    }

                    Current = default!;
                    _index = -1;
                    return false;
                }
                case SourceKind.List:
                {
                    if (!_hasListEnumerator)
                    {
                        _listEnumerator = _enumerable._list!.GetEnumerator();
                        _hasListEnumerator = true;
                    }

                    if (!_listEnumerator.MoveNext())
                        return false;
                    Current = _listEnumerator.Current;
                    return true;
                }
                case SourceKind.IReadOnlySpan:
                {
                    var span = _enumerable._span!.AsSpan();
                    if ((uint)_index < (uint)span.Length)
                    {
                        Current = span[_index];
                        _index++;
                        return true;
                    }

                    Current = default!;
                    _index = -1;
                    return false;
                }
                default:
                    _enumerator ??= _enumerable._enumerable!.GetEnumerator();
                    if (!_enumerator.MoveNext())
                        return false;
                    Current = _enumerator.Current;
                    return true;
            }
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
            switch (_enumerable._kind)
            {
                case SourceKind.Array:
                    count = _enumerable._array!.Length;
                    return true;
                case SourceKind.List:
                    count = _enumerable._list!.Count;
                    return true;
                case SourceKind.IReadOnlySpan:
                    count = _enumerable._span!.AsSpan().Length;
                    return true;
                default:
                    return _enumerable._enumerable.TryGetNonEnumeratedCount(out count);
            }
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            switch (_enumerable._kind)
            {
                case SourceKind.Array:
                    span = _enumerable._array!;
                    return true;
                case SourceKind.List:
                    span = CollectionsMarshal.AsSpan(_enumerable._list!);
                    return true;
                case SourceKind.IReadOnlySpan:
                    span = _enumerable._span!.AsSpan();
                    return true;
                default:
                    span = default;
                    return false;
            }
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            return TryGetSpan(out var span) && span.TryCopyTo(destination, offset);
        }

        public void Reset()
        {
            Dispose();
        }

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null;
            _listEnumerator.Dispose();
            _listEnumerator = default;
            _hasListEnumerator = false;
            _index = 0;
            Current = default!;
        }
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<T>.Contains(T item)
    {
        return AsValueEnumerable().Contains(item);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        AsValueEnumerable().CopyTo(array.AsSpan(arrayIndex));
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    int ICollection<T>.Count => AsValueEnumerable().Count();

    bool ICollection<T>.IsReadOnly => true;
}

public readonly struct PooledEnumerable<T> : IStructEnumerable<PooledEnumerable<T>.Enumerator, T>
{
    private readonly IEnumerable<T> _enumerable;

    public PooledEnumerable(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>, IReadOnlySpan<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private T[]? _array;
        private int _count;
        private int _index;

        internal Enumerator(PooledEnumerable<T> enumerable)
        {
            _enumerable = enumerable._enumerable;
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

        public T Current { get; private set; } = default!;

        public void Dispose()
        {
            if (_array is null)
                return;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Array.Clear(_array, 0, _count);
            ArrayPool<T>.Shared.Return(_array);
            _array = null;
            _index = -1;
            Current = default!;
        }

        private void InitArray()
        {
            if (_array is not null)
                return;
            if (_enumerable.TryGetNonEnumeratedCount(out _count) || _enumerable is IReadOnlySpan<T>)
            {
                switch (_enumerable)
                {
                    case IReadOnlySpan<T> readOnlySpan:
                        var span = readOnlySpan.AsSpan();
                        _count = span.Length;
                        _array = ArrayPool<T>.Shared.Rent(_count);
                        span.CopyTo(_array);
                        break;
                    case ICollection<T> collection:
                        _array = ArrayPool<T>.Shared.Rent(_count);
                        collection.CopyTo(_array, 0);
                        break;
                    default:
                    {
                        _array = ArrayPool<T>.Shared.Rent(_count);
                        var i = 0;
                        foreach (var item in _enumerable)
                        {
                            _array[i] = item;
                            i++;
                        }

                        break;
                    }
                }
            }
            else
            {
                var initialBuffer = default(InlineArray16<T>);
                Span<T> initialBufferSpan = initialBuffer;
                var arrayBuilder = new SegmentedArrayProvider<T>(initialBufferSpan);
                var span = arrayBuilder.GetSpan();
                var i = 0;
                foreach (var item in _enumerable)
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
                _array = ArrayPool<T>.Shared.Rent(_count);
                arrayBuilder.CopyToAndClear(_array);
            }
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
            if (_enumerable.TryGetNonEnumeratedCount(out count))
                return true;
            if (_enumerable is not IReadOnlySpan<T> readOnlySpan)
                return false;
            count = readOnlySpan.AsSpan().Length;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            InitArray();
            span = _array.AsSpan(0, _count);
            return true;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            InitArray();
            return _array.AsSpan(0, _count).TryCopyTo(destination, offset);
        }

        public Span<T> AsSpan()
        {
            InitArray();
            return _array.AsSpan(0, _count);
        }

        ReadOnlySpan<T> IReadOnlySpan<T>.AsSpan()
        {
            InitArray();
            return _array.AsSpan(0, _count);
        }
    }
}
