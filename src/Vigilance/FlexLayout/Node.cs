using System.Runtime.CompilerServices;

namespace Vigilance.FlexLayout;

public sealed class Node : Node<Node.Storage>
{
    public Node()
        : base([]) { }

    public new sealed class Storage : List<Node<Storage>>;
}

public abstract partial class Node<TStorage> : IStructEnumerable<Node<TStorage>.Enumerator, Node<TStorage>>
    where TStorage : IList<Node<TStorage>>
{
    internal BaselineFunc<TStorage>? BaselineFunc;
    internal int LineIndex;
    internal MeasureFunc<TStorage>? MeasureFunc;
    internal Node<TStorage>? NextChild;
    internal Flex.Layout NodeLayout = new();
    internal Style NodeStyle = new();
    internal NodeType NodeType = NodeType.Default;
    internal Node<TStorage>? Parent = null;
    internal InlineArray2<Value> ResolvedDimensions;
    internal TStorage Storage;

    protected Node(in TStorage storage)
    {
        Storage = storage;
        ResolvedDimensions[0] = Flex.ValueUndefined;
        ResolvedDimensions[1] = Flex.ValueUndefined;
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

    public struct Traverser : ITraverser<Traverser, Node<TStorage>>
    {
        private Enumerator _enumerator;
        private bool _hasEnumerator;

        public Node<TStorage> Origin { get; }

        internal Traverser(Node<TStorage> origin)
        {
            Origin = origin;
        }

        public Traverser ConvertToTraverser(Node<TStorage> next)
        {
            return new Traverser(next);
        }

        public bool TryGetChildCount(out int count)
        {
            count = Origin.ChildrenCount;
            return true;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            hasChild = Origin.ChildrenCount > 0;
            return true;
        }

        public bool TryGetParent(out Node<TStorage> parent)
        {
            parent = Origin.Parent!;
            return Origin.Parent is not null;
        }

        public bool TryGetNextChild(out Node<TStorage> child)
        {
            if (!_hasEnumerator)
            {
                if (Origin.ChildrenCount == 0)
                {
                    child = null!;
                    return false;
                }

                _enumerator = Origin.GetEnumerator();
                _hasEnumerator = true;
            }

            if (_enumerator.MoveNext())
            {
                child = _enumerator.Current;
                return true;
            }

            child = null!;
            return false;
        }

        public bool TryGetNextSibling(out Node<TStorage> next)
        {
            BEGIN:
            if (_hasEnumerator)
            {
                if (_enumerator.MoveNext())
                {
                    next = _enumerator.Current;
                    return true;
                }
            }
            else if (TryGetParent(out var parent))
            {
                _enumerator = parent.GetEnumerator();
                _hasEnumerator = true;
                while (_enumerator.MoveNext())
                    if (_enumerator.Current == Origin)
                        goto BEGIN;
            }

            next = null!;
            return false;
        }

        public bool TryGetPreviousSibling(out Node<TStorage> previous)
        {
            BEGIN:
            if (_hasEnumerator)
            {
                if (_enumerator.MoveNext())
                {
                    previous = _enumerator.Current;
                    if (previous != Origin)
                        return true;
                }
            }
            else if (TryGetParent(out var parent))
            {
                _enumerator = parent.GetEnumerator();
                _hasEnumerator = true;
                goto BEGIN;
            }

            previous = null!;
            return false;
        }

        public void Dispose()
        {
            if (!_hasEnumerator)
                return;
            _enumerator.Dispose();
            _hasEnumerator = false;
        }
    }
}
