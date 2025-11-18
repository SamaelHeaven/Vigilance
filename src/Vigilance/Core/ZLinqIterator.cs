using System.Collections;
using ZLinq;

namespace Vigilance.Core;

public sealed class ZLinqIterator<TEnumerator, TValue>
    : IStructEnumerable<ZLinqIterator<TEnumerator, TValue>.Enumerator, TValue>,
        IDisposable
    where TEnumerator : struct, IValueEnumerator<TValue>
{
    private TEnumerator? _enumerator;

    internal ZLinqIterator(in ValueEnumerable<TEnumerator, TValue> enumerable)
    {
        _enumerator = enumerable.Enumerator;
    }

    public void Dispose()
    {
        _enumerator?.Dispose();
    }

    public Enumerator GetEnumerator()
    {
        var result = new Enumerator(this);
        _enumerator = null;
        return result;
    }

    public ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
    }

    public struct Enumerator : IEnumerator<TValue>
    {
        private TEnumerator _enumerator;

        public Enumerator(ZLinqIterator<TEnumerator, TValue> iterator)
        {
            _enumerator =
                iterator._enumerator
                ?? throw new InvalidOperationException($"{nameof(ZLinqIterator<,>)} can only be enumerated once.");
        }

        public TValue Current { get; private set; } = default!;
        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_enumerator.TryGetNext(out var value))
                return false;
            Current = value;
            return true;
        }

        public void Reset() { }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}

public static class ZLinqExtensions
{
    extension<TEnumerator, TValue>(in ValueEnumerable<TEnumerator, TValue> enumerable)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public IEnumerable<TValue> AsIterator()
        {
            return new ZLinqIterator<TEnumerator, TValue>(enumerable);
        }
    }
}
