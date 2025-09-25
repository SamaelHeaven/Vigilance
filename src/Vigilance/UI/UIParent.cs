using Vigilance.Core;
using Vigilance.Drawing;
using ZLinq;

namespace Vigilance.UI;

public abstract class UIParent : UIElement
{
    private LinkedList<UIElement> _children = new();

    public UIParent this[UIElement? element]
    {
        get
        {
            Add(element);
            return this;
        }
    }

    public UIParent this[params IEnumerable<UIElement?> elements]
    {
        get
        {
            Add(elements);
            return this;
        }
    }

    public ChildEnumerable Children => new(this);

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        foreach (var element in Children)
            element.Render(element.LayoutTransform, graphics, camera);
    }

    public override void Update(Entity entity)
    {
        foreach (var element in Children)
            element.Update(entity);
        base.Update(entity);
    }

    public UIElement? Select(UISelector? selector = null)
    {
        return Select<UIElement>(selector);
    }

    public T? Select<T>(UISelector? selector = null)
        where T : UIElement
    {
        selector ??= static _ => true;
        foreach (var element in Children)
            if (element is T t && selector.Invoke(t))
                return t;
        foreach (var element in Children)
        {
            if (element is not UIParent container)
                continue;
            var result = container.Select<T>(selector);
            if (result is not null)
                return result;
        }

        return null;
    }

    public IEnumerable<UIElement> SelectAll(UISelector? selector = null)
    {
        return SelectAll<UIElement>(selector);
    }

    public IEnumerable<T> SelectAll<T>(UISelector? selector = null)
        where T : UIElement
    {
        selector ??= static _ => true;
        foreach (var element in Children)
            if (element is T t && selector(t))
                yield return t;
        foreach (var element in Children)
        {
            if (element is not UIParent container)
                continue;
            foreach (var child in container.SelectAll<T>(selector))
                yield return child;
        }
    }

    public void Add(UIElement? element)
    {
        if (element is null)
            return;
        element.Remove();
        _children.AddLast(element);
        element.Parent = this;
        if (!LayoutCustom)
            Node.AddChild(element.Node);
        MarkDirty();
    }

    public void Add(params IEnumerable<UIElement?> elements)
    {
        foreach (var element in elements)
            Add(element);
    }

    public void Insert(int index, UIElement element)
    {
        var oldNode = _children.AsValueEnumerable().ElementAtOrDefault(index);
        element.Remove();
        element.Parent = this;
        if (oldNode is null)
            _children.AddLast(element);
        else
            _children.AddBefore(_children.Find(oldNode)!, element);
        if (!LayoutCustom)
            Node.InsertChild(element.Node, index);
        MarkDirty();
    }

    public int IndexOf(UIElement element)
    {
        var index = 0;
        foreach (var child in _children)
        {
            if (child == element)
                return index;
            index++;
        }

        return -1;
    }

    public bool Replace(int index, UIElement element)
    {
        var oldNode = _children.AsValueEnumerable().ElementAtOrDefault(index);
        if (oldNode is null)
            return false;
        element.Remove();
        element.Parent = this;
        _children.Find(oldNode)!.Value = element;
        if (!LayoutCustom)
            Node.ReplaceChild(index, element.Node);
        MarkDirty();
        return true;
    }

    public void Clear()
    {
        foreach (var element in Children)
            element.Remove();
    }

    protected override object DeepClone()
    {
        var result = (UIParent)base.DeepClone();
        result._children = new LinkedList<UIElement>();
        result.Add(_children.Select(el => el.DeepClone()));
        return result;
    }

    internal void Remove(UIElement element)
    {
        _children.Remove(element);
        element.Parent = null;
        if (!LayoutCustom)
            Node.RemoveChild(element.Node);
        MarkDirty();
    }

    public readonly struct ChildEnumerable
        : IStructEnumerable<ChildEnumerator, UIElement>,
            IReadOnlyCollection<UIElement>
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

        public int Count => _parent._children.Count;
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
            _next = _parent._children.First;
            _current = null;
        }

        public UIElement Current => _current?.Value!;

        public void Dispose() { }
    }
}
