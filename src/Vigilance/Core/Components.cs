using System.Collections;

namespace Vigilance.Core;

public readonly struct Components : IReadOnlyList<Component>
{
    public static Components Empty { get; } = new();

    internal readonly List<Component> Values = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public Components() { }

    public OfTypeEnumerable<T> OfType<T>()
    {
        return new OfTypeEnumerable<T>(Values);
    }

    public IEnumerator<Component> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public readonly struct OfTypeEnumerable<T> : IEnumerable<T>
    {
        private readonly List<Component> _components;

        internal OfTypeEnumerable(List<Component> components)
        {
            _components = components;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_components);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly List<Component> _components;
            private int _index;
            private T _current;

            internal Enumerator(List<Component> components)
            {
                _components = components;
                _index = -1;
                _current = default!;
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
                throw new NotSupportedException();
            }

            public void Dispose() { }
        }
    }
}
