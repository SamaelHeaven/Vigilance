using System.Collections;

namespace Vigilance.Core;

public readonly struct Components : IReadOnlyList<Component>
{
    public static Components Empty { get; } = new();

    internal readonly List<Component> Values = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public Components() { }

    public OfTypeIterator<T> OfType<T>()
    {
        return new OfTypeIterator<T>(this);
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct OfTypeIterator<T> : IValueIterator<OfTypeIterator<T>, T>
    {
        private readonly Components _components;
        private int _index;
        private T _current;

        internal OfTypeIterator(Components components)
        {
            _components = components;
            _index = -1;
            _current = default!;
        }

        public OfTypeIterator<T> GetEnumerator()
        {
            return this;
        }

        public T Current => _current;

        public bool MoveNext()
        {
            while (++_index < _components.Count)
            {
                if (_components[_index].Data is not T t)
                    continue;
                _current = t;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
            _current = default!;
        }

        public void Dispose() { }
    }
}
