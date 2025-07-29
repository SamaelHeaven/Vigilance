using System.Collections;

namespace Vigilance.Core;

public interface IValueEnumerable<out TEnumerator, out TValue> : IEnumerable<TValue>
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
}

public interface IValueEnumerator<out TSelf, out TValue> : IEnumerator<TValue>
    where TSelf : struct, IValueEnumerator<TSelf, TValue>
{
    new TValue Current { get; }

    object? IEnumerator.Current => Current;
}

public interface IValueIterator<out TSelf, out TValue>
    : IValueEnumerator<TSelf, TValue>,
        IValueEnumerable<TSelf, TValue>
    where TSelf : struct, IValueEnumerator<TSelf, TValue>;
