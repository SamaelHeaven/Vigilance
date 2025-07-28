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
        return new OfTypeEnumerator<T>(Values);
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class OfTypeEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly List<Component> _components;
        private int _index = -1;

        public OfTypeEnumerator(List<Component> components)
        {
            _components = components;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Current { get; private set; } = default!;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            while (++_index < _components.Count)
            {
                var data = _components[_index].Data;
                if (data is not T t)
                    continue;
                Current = t;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
            Current = default!;
        }

        public void Dispose() { }
    }
}
