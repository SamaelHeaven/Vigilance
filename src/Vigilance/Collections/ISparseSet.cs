using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZLinq;

namespace Vigilance.Collections;

public interface ISparseSet<TValue, TStorage>
    where TStorage : IList<TValue>
{
    public readonly struct ValueEnumerable
        : IReadOnlyList<TValue>,
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
                if (typeof(IReadOnlySpan<TValue>).IsAssignableFrom(typeof(TStorage)))
                {
                    span = ((IReadOnlySpan<TValue>)_values).AsSpan();
                    return true;
                }

                span = default;
                return false;
            }

            public bool TryCopyTo(scoped Span<TValue> destination, Index offset)
            {
                if (!typeof(IReadOnlySpan<TValue>).IsAssignableFrom(typeof(TStorage)))
                    return false;
                ((IReadOnlySpan<TValue>)_values).AsSpan().TryCopyTo(destination, offset);
                return true;
            }

            public void Reset()
            {
                _index = 0;
                Current = default!;
            }

            public void Dispose() { }
        }

        public int Count => _values.Count;

        public TValue this[int index] => _values[index];
    }
}

public interface ISparseSet<TKey, TValue, TStorage> : ISparseSet<TValue, TStorage>
    where TStorage : IList<TValue>
{
    ValueEnumerable Values { get; }

    TValue this[in TKey key] { get; set; }

    int Count { get; }

    KeyValuePair<TKey, TValue> this[int index] { get; }

    void Clear();

    bool ContainsKey(in TKey key);

    bool TryGetValue(in TKey key, [MaybeNullWhen(false)] out TValue value);

    bool Remove(in TKey key);

    int GetKeyIndex(in TKey key);
}
