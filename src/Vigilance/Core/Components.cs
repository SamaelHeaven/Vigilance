using System.Collections;

namespace Vigilance.Core;

public readonly record struct Components : IReadOnlyList<Component>
{
    internal readonly List<Component> Values = new();

    public Components() { }

    public static Components Empty { get; } = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public IEnumerator<Component> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public OfTypeIterator<T> OfType<T>()
    {
        return new OfTypeIterator<T>(this);
    }

    public override string ToString()
    {
        return Values.Count == 0 ? "[]" : $"[\n  {string.Join(",\n  ", Values)}\n]";
    }

    public struct OfTypeIterator<T> : IValueIterator<OfTypeIterator<T>, T>
    {
        private readonly Components _components;
        private int _index;

        internal OfTypeIterator(Components components)
        {
            _components = components;
            _index = -1;
            Current = default!;
        }

        public OfTypeIterator<T> GetEnumerator()
        {
            return this;
        }

        public T Current { get; private set; }

        public bool MoveNext()
        {
            while (++_index < _components.Count)
            {
                if (_components[_index].Data is not T t)
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
