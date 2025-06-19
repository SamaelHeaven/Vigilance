using System.Collections;

namespace Vigilance.Core;

public readonly struct Components : IReadOnlyList<ComponentEntry>
{
    public static Components Empty { get; } = new();

    internal readonly List<ComponentEntry> Values = new();

    public int Count => Values.Count;

    public ComponentEntry this[int index] => Values[index];

    public Components() { }

    public IEnumerable<T> OfType<T>()
    {
        foreach (var component in Values)
            if (component.Data is T t)
                yield return t;
    }

    public IEnumerator<ComponentEntry> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
