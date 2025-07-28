using System.Collections;

namespace Vigilance.Core;

public readonly struct Components : IReadOnlyList<Component>
{
    public static Components Empty { get; } = new();

    internal readonly List<Component> Values = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public Components() { }

    public OfTypeEnumerator<T> OfType<T>()
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

    public struct OfTypeEnumerator<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly List<Component> _components;
        private int _index;
        private T _current;

        internal OfTypeEnumerator(List<Component> components)
        {
            _components = components;
            _index = -1;
            _current = default!;
        }

        public OfTypeEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Current => _current;

        object IEnumerator.Current => _current!;

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
