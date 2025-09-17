namespace Vigilance.Core;

public readonly record struct Components
    : IValueEnumerable<List<Component>.Enumerator, Component>,
        IReadOnlyList<Component>
{
    internal readonly List<Component> Values = new();

    public Components() { }

    public static Components Empty { get; } = new();

    public int Count => Values.Count;

    public Component this[int index] => Values[index];

    public List<Component>.Enumerator GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public OfTypeEnumerable<T> OfType<T>()
    {
        return new OfTypeEnumerable<T>(this);
    }

    public override string ToString()
    {
        return Values.Count == 0 ? "[]" : $"[\n  {string.Join(",\n  ", Values)}\n]";
    }

    public readonly struct OfTypeEnumerable<T> : IValueEnumerable<OfTypeEnumerator<T>, T>
    {
        private readonly Components _components;

        internal OfTypeEnumerable(Components components)
        {
            _components = components;
        }

        public OfTypeEnumerator<T> GetEnumerator()
        {
            return new OfTypeEnumerator<T>(_components);
        }
    }

    public struct OfTypeEnumerator<T> : IValueEnumerator<T>
    {
        private readonly Components _components;
        private int _index;

        internal OfTypeEnumerator(Components components)
        {
            _components = components;
            _index = -1;
            Current = default!;
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
