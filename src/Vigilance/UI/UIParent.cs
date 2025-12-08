using Vigilance.Collections;
using ZLinq;

namespace Vigilance.UI;

public abstract class UIParent : UIElement
{
    internal List<UIElement> ChildrenList = [];
    internal Queue<DeferredData> DeferredQueue = [];

    public UIParent this[UIElement? element]
    {
        get
        {
            Add(element);
            return this;
        }
    }

    public UIParent this[params ReadOnlySpan<UIElement?> elements]
    {
        get
        {
            Add(elements);
            return this;
        }
    }

    public UIParent this[IEnumerable<UIElement?> elements]
    {
        get
        {
            AddRange(elements);
            return this;
        }
    }

    public ChildEnumerable Children => new(this);

    public void Add(UIElement? element)
    {
        if (element is null)
            return;
        element.Remove();
        ChildrenList.Add(element);
        element.Parent = this;
        if (!IsLayoutCustom)
            Node.AddChild(element.Node);
        MarkDirty();
    }

    public void Add(params ReadOnlySpan<UIElement?> elements)
    {
        foreach (var element in elements)
            Add(element);
    }

    public void AddRange<T>(T elements)
        where T : IEnumerable<UIElement?>
    {
        foreach (var element in elements)
            Add(element);
    }

    public void Insert(int index, UIElement element)
    {
        ChildrenList.Insert(index, element);
        element.Remove();
        element.Parent = this;
        if (!IsLayoutCustom)
            Node.InsertChild(element.Node, index);
        MarkDirty();
    }

    public int IndexOf(UIElement element)
    {
        return ChildrenList.IndexOf(element);
    }

    public bool Replace(int index, UIElement element)
    {
        ChildrenList[index].Remove();
        element.Remove();
        element.Parent = this;
        ChildrenList[index] = element;
        if (!IsLayoutCustom)
            Node.ReplaceChild(index, element.Node);
        MarkDirty();
        return true;
    }

    public void Clear()
    {
        foreach (var element in Children)
            element.Remove();
    }

    internal void Remove(UIElement element)
    {
        ChildrenList.Remove(element);
        element.Parent = null;
        if (!IsLayoutCustom)
            Node.RemoveChild(element.Node);
        MarkDirty();
    }

    internal struct DeferredData
    {
        public DeferredOperation Operation;
        public UIElement? Element;
        public int Index;
    }

    internal enum DeferredOperation
    {
        Add,
        Remove,
        Insert,
        Replace,
    }

    public readonly struct ChildEnumerable : IStructEnumerable<ChildEnumerator, UIElement>, IReadOnlyList<UIElement>
    {
        private readonly UIParent _parent;

        internal ChildEnumerable(UIParent parent)
        {
            _parent = parent;
        }

        public ChildEnumerator GetEnumerator()
        {
            return new ChildEnumerator(_parent);
        }

        public ValueEnumerable<StructEnumerator<ChildEnumerator, UIElement>, UIElement> AsValueEnumerable()
        {
            return new StructEnumerator<ChildEnumerator, UIElement>(GetEnumerator());
        }

        public int Count => _parent.ChildrenList.Count;

        public UIElement this[int index] => _parent.ChildrenList[index];
    }

    public struct ChildEnumerator : IStructEnumerator<UIElement>
    {
        private readonly UIParent _parent;
        private LinkedListNode<UIElement>? _current;
        private LinkedListNode<UIElement>? _next;

        internal ChildEnumerator(UIParent parent)
        {
            _parent = parent;
            Reset();
        }

        public bool MoveNext()
        {
            _current = _next;
            if (_current is null)
                return false;
            _next = _current.Next;
            return _current?.Value is not null;
        }

        public void Reset()
        {
            _next = _parent.ChildrenList.First;
            _current = null;
        }

        public UIElement Current => _current?.Value!;

        public void Dispose() { }
    }
}
