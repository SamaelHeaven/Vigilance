using System.Diagnostics.CodeAnalysis;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Core;

public static class ZLinqExtensions
{
    extension<TEnumerator, TValue>(in ValueEnumerable<TEnumerator, TValue> enumerable)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public ValueIterator<TEnumerator, TValue> AsIterator()
        {
            return new ValueIterator<TEnumerator, TValue>(enumerable);
        }
    }

    extension<TEnumerator, TValue>(ValueEnumerableFunc<TEnumerator, TValue> func)
        where TEnumerator : struct, IValueEnumerator<TValue>
    {
        public ValueEnumerableProvider<TEnumerator, TValue> AsEnumerable()
        {
            return new ValueEnumerableProvider<TEnumerator, TValue>(func);
        }
    }
}

public sealed class ValueIterator<TEnumerator, TValue>
    : IStructEnumerable<ValueIterator<TEnumerator, TValue>.Enumerator, TValue>,
        IDisposable
    where TEnumerator : struct, IValueEnumerator<TValue>
{
    private TEnumerator? _enumerator;

    internal ValueIterator(in ValueEnumerable<TEnumerator, TValue> enumerable)
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

    public ValueEnumerable<TEnumerator, TValue> AsValueEnumerable()
    {
        return new ValueEnumerable<TEnumerator, TValue>(GetEnumeratorValue());
    }

    ~ValueIterator()
    {
        Logger.Warning(
            $"{nameof(ValueIterator<,>)}<{typeof(TEnumerator).Name}, {typeof(TValue).Name}> finalizer called. Ensure that {nameof(Dispose)} or {nameof(GetEnumerator)} is called explicitly."
        );
        ReleaseUnmanagedResources();
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
            ?? throw new InvalidOperationException($"{nameof(ValueIterator<,>)} can only be enumerated once.");
        _enumerator = null;
        GC.SuppressFinalize(this);
        return result;
    }

    public struct Enumerator : IStructEnumerator<TValue>
    {
        private TEnumerator _enumerator;

        public Enumerator(ValueIterator<TEnumerator, TValue> iterator)
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

public delegate ValueEnumerable<TEnumerator, TValue> ValueEnumerableFunc<TEnumerator, TValue>()
    where TEnumerator : struct, IValueEnumerator<TValue>;

public readonly struct ValueEnumerableProvider<TEnumerator, TValue>
    : IStructEnumerable<ValueEnumerableProvider<TEnumerator, TValue>.Enumerator, TValue>
    where TEnumerator : struct, IValueEnumerator<TValue>
{
    private readonly ValueEnumerableFunc<TEnumerator, TValue> _func;

    internal ValueEnumerableProvider(ValueEnumerableFunc<TEnumerator, TValue> func)
    {
        _func = func;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<TEnumerator, TValue> AsValueEnumerable()
    {
        return _func.Invoke();
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
        private readonly ValueEnumerableProvider<TEnumerator, TValue> _enumerableProvider;
        private TEnumerator _enumerator;
        private bool _disposed = true;

        internal Enumerator(ValueEnumerableProvider<TEnumerator, TValue> enumerableProvider)
        {
            _enumerableProvider = enumerableProvider;
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
            _enumerator = _enumerableProvider._func.Invoke().Enumerator;
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
