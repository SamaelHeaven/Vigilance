using System.Diagnostics.CodeAnalysis;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Core;

public static class ZLinqExtensions
{
    extension<TEnumerator, TValue>(in ValueEnumerable<TEnumerator, TValue> enumerable)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public ZLinqIterator<TEnumerator, TValue> AsIterator()
        {
            return new ZLinqIterator<TEnumerator, TValue>(enumerable);
        }
    }

    extension<TEnumerator, TValue>(Func<ValueEnumerable<TEnumerator, TValue>> enumerableFunc)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public ZLinqEnumerable<TEnumerator, TValue> AsEnumerable()
        {
            return new ZLinqEnumerable<TEnumerator, TValue>(enumerableFunc);
        }
    }
}

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
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> IStructEnumerable<
        Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
    }

    ~ZLinqIterator()
    {
        Logger.Warning(
            $"{nameof(ZLinqIterator<,>)}<{typeof(TEnumerator).Name}, {typeof(TValue).Name}> finalizer called. Ensure that {nameof(Dispose)} or {nameof(GetEnumerator)} is called explicitly."
        );
        ReleaseUnmanagedResources();
    }

    public ValueEnumerable<TEnumerator, TValue> AsValueEnumerable()
    {
        return new ValueEnumerable<TEnumerator, TValue>(GetEnumeratorValue());
    }

    private void ReleaseUnmanagedResources()
    {
        _enumerator?.Dispose();
        _enumerator = null;
    }

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    private TEnumerator GetEnumeratorValue()
    {
        var result =
            _enumerator
            ?? throw new InvalidOperationException($"{nameof(ZLinqIterator<,>)} can only be enumerated once.");
        _enumerator = null;
        GC.SuppressFinalize(this);
        return result;
    }

    public struct Enumerator : IStructEnumerator<TValue>
    {
        private TEnumerator _enumerator;

        public Enumerator(ZLinqIterator<TEnumerator, TValue> iterator)
        {
            _enumerator = iterator.GetEnumeratorValue();
        }

        public TValue Current { get; private set; } = default!;

        public bool MoveNext()
        {
            if (!_enumerator.TryGetNext(out var value))
                return false;
            Current = value;
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}

public readonly struct ZLinqEnumerable<TEnumerator, TValue>
    : IStructEnumerable<ZLinqEnumerable<TEnumerator, TValue>.Enumerator, TValue>
    where TEnumerator : struct, IValueEnumerator<TValue>
{
    private readonly Func<ValueEnumerable<TEnumerator, TValue>> _enumerableFunc;

    internal ZLinqEnumerable(Func<ValueEnumerable<TEnumerator, TValue>> enumerableFunc)
    {
        _enumerableFunc = enumerableFunc;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<TEnumerator, TValue> AsValueEnumerable()
    {
        return _enumerableFunc.Invoke();
    }

    ValueEnumerable<StructEnumerator<Enumerator, TValue>, TValue> IStructEnumerable<
        Enumerator,
        TValue
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, TValue>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<TValue>
    {
        private readonly ZLinqEnumerable<TEnumerator, TValue> _enumerable;
        private TEnumerator _enumerator;
        private bool _disposed = true;

        internal Enumerator(ZLinqEnumerable<TEnumerator, TValue> enumerable)
        {
            _enumerable = enumerable;
            Reset();
        }

        public bool MoveNext()
        {
            if (!_enumerator.TryGetNext(out var value))
                return false;
            Current = value;
            return true;
        }

        public void Reset()
        {
            Dispose();
            _disposed = false;
            _enumerator = _enumerable._enumerableFunc.Invoke().Enumerator;
        }

        public TValue Current { get; private set; } = default!;

        public void Dispose()
        {
            if (_disposed)
                return;
            _enumerator.Dispose();
            _disposed = true;
        }
    }
}
