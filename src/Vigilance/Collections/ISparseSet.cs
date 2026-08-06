using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

public interface ISparseSet
{
    int Count { get; }

    void Clear();
}

public interface ISparseSet<TValue, TStorage> : ISparseSet
    where TStorage : IList<TValue>
{
    public readonly struct ValueEnumerable
        : IReadOnlyList<TValue>,
            ICollection<TValue>,
            IStructEnumerable<ValueEnumerable.Enumerator, TValue>
    {
        private readonly TStorage _values;

        public ValueEnumerable(TStorage values)
        {
            _values = values;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_values);
        }

        public ValueEnumerable<Enumerator, TValue> AsValueEnumerable()
        {
            return new ValueEnumerable<Enumerator, TValue>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> IStructEnumerable<
            Enumerator,
            TValue
        >.AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public struct Enumerator : IStructEnumerator<TValue>, IValueEnumerator<TValue>
        {
            private readonly TStorage _values;
            private int _index;

            internal Enumerator(TStorage values)
            {
                _values = values;
                Reset();
            }

            public bool MoveNext()
            {
                if ((uint)_index < (uint)_values.Count)
                {
                    Current = _values[_index];
                    _index++;
                    return true;
                }

                Current = default!;
                _index = -1;
                return false;
            }

            public TValue Current { get; private set; } = default!;

            public bool TryGetNext(out TValue current)
            {
                if (MoveNext())
                {
                    current = Current;
                    return true;
                }

                Unsafe.SkipInit(out current);
                return false;
            }

            public bool TryGetNonEnumeratedCount(out int count)
            {
                count = _values.Count;
                return true;
            }

            public bool TryGetSpan(out ReadOnlySpan<TValue> span)
            {
                switch (_values)
                {
                    case IReadOnlySpan<TValue> values:
                        span = values.AsSpan();
                        return true;
                    case List<TValue> list:
                        span = list.AsSpan();
                        return true;
                    default:
                        span = default;
                        return false;
                }
            }

            public bool TryCopyTo(scoped Span<TValue> destination, Index offset)
            {
                if (!TryGetSpan(out var span))
                    return false;
                span.TryCopyTo(destination, offset);
                return true;
            }

            public void Reset()
            {
                _index = 0;
                Current = default!;
            }

            public void Dispose() { }
        }

        void ICollection<TValue>.Add(TValue item)
        {
            throw new NotSupportedException($"{nameof(ValueEnumerable)} is read-only.");
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotSupportedException($"{nameof(ValueEnumerable)} is read-only.");
        }

        bool ICollection<TValue>.Remove(TValue item)
        {
            throw new NotSupportedException($"{nameof(ValueEnumerable)} is read-only.");
        }

        public bool Contains(TValue item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public int Count => _values.Count;

        bool ICollection<TValue>.IsReadOnly => true;

        public TValue this[int index] => _values[index];
    }
}

public interface ISparseSet<TKey> : ISparseSet
{
    TKey this[int index] { get; }

    bool Add(in TKey key);

    bool Contains(in TKey key);

    bool Remove(in TKey key);

    int GetKeyIndex(in TKey key);
}

public interface ISparseSet<TKey, TValue, TStorage> : ISparseSet<TValue, TStorage>
    where TStorage : IList<TValue>
{
    ValueEnumerable Values { get; }

    TValue this[in TKey key] { get; set; }

    KeyValuePair<TKey, TValue> this[int index] { get; }

    bool ContainsKey(in TKey key);

    bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value);

    TValue? GetValueOrDefault(in TKey key);

    TValue GetValueOrDefault(in TKey key, in TValue defaultValue);

    bool Remove(in TKey key);

    int GetKeyIndex(in TKey key);
}
