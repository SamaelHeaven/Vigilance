using System.Collections;

namespace Vigilance.Core;

public interface IValueEnumerable<out TEnumerator, out TValue> : IEnumerable<TValue>
    where TEnumerator : IEnumerator<TValue>
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

public interface IValueEnumerator<out TValue> : IEnumerator<TValue>
{
    new TValue Current { get; }

    object? IEnumerator.Current => Current;
}
