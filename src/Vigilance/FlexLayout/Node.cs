using Vigilance.Collections;
using ZLinq;

namespace Vigilance.FlexLayout;

public sealed class Node : Node<Node.Children>
{
    public Node()
        : base([]) { }

    public sealed class Children : List<Node<Children>>;
}

public partial class Node<TStorage> : IStructEnumerable<Node<TStorage>.Enumerator, Node<TStorage>>
    where TStorage : IList<Node<TStorage>>
{
    internal readonly Flex.Layout NodeLayout = new();
    internal readonly Style NodeStyle = new();
    internal readonly Value[] ResolvedDimensions = [Flex.ValueUndefined, Flex.ValueUndefined];
    internal BaselineFunc<TStorage>? BaselineFunc;
    internal int LineIndex;
    internal MeasureFunc<TStorage>? MeasureFunc;
    internal Node<TStorage>? NextChild;
    internal NodeType NodeType = NodeType.Default;
    internal Node<TStorage>? Parent = null;
    internal TStorage Storage;

    public Node(TStorage storage)
    {
        Storage = storage;
    }

    public int ChildrenCount => Storage.Count;

    public bool IsDirty { get; internal set; }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ValueEnumerable<StructEnumerator<Enumerator, Node<TStorage>>, Node<TStorage>> AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, Node<TStorage>>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<Node<TStorage>>
    {
        private readonly Node<TStorage> _node;
        private int _index;

        internal Enumerator(Node<TStorage> node)
        {
            _node = node;
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_node.ChildrenCount)
            {
                Current = _node.Storage[_index];
                _index++;
                return true;
            }

            Current = null!;
            _index = -1;
            return false;
        }

        public Node<TStorage> Current { get; private set; } = null!;

        public void Reset()
        {
            _index = 0;
            Current = null!;
        }

        public void Dispose() { }
    }
}
