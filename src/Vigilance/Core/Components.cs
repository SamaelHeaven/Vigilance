using System.Collections;

namespace Vigilance.Core;

public readonly struct Components : IReadOnlyList<Component>
{
    public static Components Empty { get; } = new();

    internal readonly List<Component> Values = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public Components() { }

    public IEnumerable<T> OfType<T>()
    {
        foreach (var component in Values)
            if (component.Data is T t)
                yield return t;
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
