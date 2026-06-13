using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLinq;
using ZLinq.Internal;

namespace Vigilance.Collections;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public FastEnumerable<T> FastEnumerate()
        {
            return new FastEnumerable<T>(enumerable);
        }

        public IEnumerable<(T Left, TRight Right)> Cross<TRight>(IEnumerable<TRight> other)
        {
            var otherEnumerable = other.FastEnumerate();
            foreach (var left in enumerable.FastEnumerate())
            foreach (var right in otherEnumerable)
                yield return (left, right);
        }
    }
}

public readonly struct FastEnumerable<T> : IStructEnumerable<FastEnumerable<T>.Enumerator, T>
{
    private readonly IEnumerable<T>? _enumerable;
    private readonly T[]? _array;
    private readonly List<T>? _list;
    private readonly IReadOnlySpan<T>? _span;
    private readonly SourceKind _kind;

    private enum SourceKind
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
        private T _current = default!;

        internal Enumerator(in FastEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
            _listEnumerator = default;
            _hasListEnumerator = false;
            _index = -1;
        }

        public T Current => _current;

        public bool MoveNext()
        {
            switch (_enumerable._kind)
            {
                case SourceKind.Array:
                {
                    var array = _enumerable._array!;
                    var newIndex = _index + 1;
                    if (newIndex >= array.Length)
                        return false;
                    _index = newIndex;
                    _current = array[newIndex];
                    return true;
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
                    _current = _listEnumerator.Current;
                    return true;
                }
                case SourceKind.IReadOnlySpan:
                {
                    var span = _enumerable._span!.AsSpan();
                    var newIndex = _index + 1;
                    if (newIndex >= span.Length)
                        return false;
                    _index = newIndex;
                    _current = span[newIndex];
                    return true;
                }
                default:
                    _enumerator ??= _enumerable._enumerable!.GetEnumerator();
                    if (!_enumerator.MoveNext())
                        return false;
                    _current = _enumerator.Current;
                    return true;
            }
        }

        public bool TryGetNext(out T current)
        {
            if (MoveNext())
            {
                current = _current;
                return true;
            }

            Unsafe.SkipInit(out current);
            return false;
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
                    if (_enumerable._enumerable is ICollection<T> collection)
                    {
                        count = collection.Count;
                        return true;
                    }

                    count = 0;
                    return false;
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
            if (!TryGetSpan(out var span))
                return false;
            if (!EnumeratorHelper.TryGetSlice(span, offset, destination.Length, out var slice))
                return false;
            slice.CopyTo(destination);
            return true;
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
            _index = -1;
            _current = default!;
        }
    }
}
