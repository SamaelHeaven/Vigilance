using System.Runtime.CompilerServices;

namespace Vigilance.Core;

public static class GenericExtensions
{
    extension<T>(T value)
    {
        public T Tap(out T t)
        {
            t = value;
            return value;
        }

        public T Tap(Action<T> action)
        {
            action.Invoke(value);
            return value;
        }

        public SingletonEnumerable<T> AsSingleton()
        {
            return new SingletonEnumerable<T>(in value);
        }

        public ValueEnumerable<SingletonEnumerable<T>.Enumerator, T> AsValueSingleton()
        {
            return new SingletonEnumerable<T>(in value).AsValueEnumerable();
        }
    }
}

public readonly struct SingletonEnumerable<T> : IStructEnumerable<SingletonEnumerable<T>.Enumerator, T>
{
    private readonly T _value;

    public SingletonEnumerable(in T value)
    {
        _value = value;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(in _value);
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
        private readonly T _value;
        private bool _hasValue;

        internal Enumerator(in T value)
        {
            _value = value;
            _hasValue = true;
        }

        public T Current => _value;

        public bool MoveNext()
        {
            if (!_hasValue)
                return false;
            _hasValue = false;
            return true;
        }

        public bool TryGetNext(out T current)
        {
            if (!MoveNext())
            {
                Unsafe.SkipInit(out current);
                return false;
            }

            current = _value;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 1;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            if (offset.GetOffset(1) != 0 || destination.Length == 0)
                return false;
            destination[0] = _value;
            return true;
        }

        public void Reset()
        {
            _hasValue = true;
        }

        public void Dispose()
        {
            _hasValue = false;
        }
    }
}
