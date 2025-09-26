using System.Collections;
using System.Runtime.CompilerServices;
using ZLinq;

namespace Vigilance.Core;

public interface IStructEnumerable<TEnumerator, TValue> : IEnumerable<TValue>
    where TEnumerator : struct, IEnumerator<TValue>
{
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    new TEnumerator GetEnumerator();

    ValueEnumerable<StructEnumerator<TEnumerator, TValue>, TValue> AsValueEnumerable();
}

public struct StructEnumerator<TEnumerator, TValue> : IStructEnumerator<TValue>, IValueEnumerator<TValue>
    where TEnumerator : struct, IEnumerator<TValue>
{
    private TEnumerator _enumerator;

    public StructEnumerator(TEnumerator enumerator)
    {
        _enumerator = enumerator;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public bool TryGetNext(out TValue current)
    {
        if (_enumerator.MoveNext())
        {
            current = _enumerator.Current;
            return true;
        }

        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetNonEnumeratedCount(out int count)
    {
        count = 0;
        return false;
    }

    public bool TryGetSpan(out ReadOnlySpan<TValue> span)
    {
        span = default;
        return false;
    }

    public bool TryCopyTo(scoped Span<TValue> destination, Index offset)
    {
        return false;
    }

    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    public void Reset()
    {
        _enumerator.Reset();
    }

    public TValue Current => _enumerator.Current;

    public static implicit operator ValueEnumerable<StructEnumerator<TEnumerator, TValue>, TValue>(
        StructEnumerator<TEnumerator, TValue> enumerator
    )
    {
        return new ValueEnumerable<StructEnumerator<TEnumerator, TValue>, TValue>(enumerator);
    }
}

public interface IStructEnumerator<out TValue> : IEnumerator<TValue>
{
    new TValue Current { get; }

    object? IEnumerator.Current => Current;
}
