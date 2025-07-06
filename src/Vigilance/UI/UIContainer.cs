using FlexLayoutSharp;
using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.UI;

public class UIContainer : UIElement
{
    private LinkedList<UIElement> _children = new();

    public Direction Direction
    {
        get => (Direction)Node.StyleGetFlexDirection();
        set => Node.StyleSetFlexDirection((FlexDirection)value);
    }

    public Justify Justify
    {
        get => (Justify)Node.StyleGetJustifyContent();
        set => Node.StyleSetJustifyContent((FlexLayoutSharp.Justify)value);
    }

    public Align AlignItems
    {
        get => (Align)Node.StyleGetAlignItems();
        set => Node.StyleSetAlignItems((FlexLayoutSharp.Align)value);
    }

    public Align AlignContent
    {
        get => (Align)Node.StyleGetAlignContent();
        set => Node.StyleSetAlignContent((FlexLayoutSharp.Align)value);
    }

    public Wrap Wrap
    {
        get => (Wrap)Node.StyleGetFlexWrap();
        set => Node.StyleSetFlexWrap((FlexLayoutSharp.Wrap)value);
    }

    public int ChildCount => _children.Count;

    public UIContainer this[params IEnumerable<UIElement> elements]
    {
        get
        {
            Add(elements);
            return this;
        }
    }

    public IEnumerable<UIElement> Children
    {
        get
        {
            var element = _children.First;
            while (element?.Value != null)
            {
                var next = element.Next;
                yield return element.Value;
                element = next;
            }
        }
    }

    public override void Render(Graphics graphics, CameraFunc? camera)
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
            if (element is not UIContainer container)
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
            if (element is not UIContainer container)
                continue;
            foreach (var child in container.SelectAll<T>(selector))
                yield return child;
        }
    }

    public void Add(params IEnumerable<UIElement> elements)
    {
        foreach (var element in elements)
        {
            Node.AddChild(element.Node);
            if (Node.IndexOfChild(element.Node) == -1)
                return;
            _children.AddLast(element);
            element.Parent = this;
        }
    }

    public void Insert(int index, UIElement element)
    {
        if ((UIElement?)element is null)
            throw new NullReferenceException();
        var oldNode = _children.ElementAtOrDefault(index);
        Node.InsertChild(element.Node, index);
        if (oldNode is null)
            _children.AddLast(element);
        else
            _children.AddBefore(_children.Find(oldNode)!, element);
        element.Parent = this;
    }

    public int IndexOf(UIElement element)
    {
        return Node.IndexOfChild(element.Node);
    }

    public void Replace(int index, UIElement element)
    {
        var oldNode = _children.ElementAt(index);
        Node.ReplaceChild(index, element.Node);
        _children.Find(oldNode)!.Value = element;
    }

    public void Clear()
    {
        foreach (var element in Children)
            element.Remove();
    }

    public override object DeepClone()
    {
        var result = (UIContainer)base.DeepClone();
        result._children = new LinkedList<UIElement>();
        result.Add(_children.Select(el => el.DeepClone<UIElement>()));
        return result;
    }

    internal override void MarkReady()
    {
        base.MarkReady();
        foreach (var element in _children)
            element.MarkReady();
    }

    internal void Remove(UIElement element)
    {
        Node.RemoveChild(element.Node);
        _children.Remove(element);
        element.Parent = null;
    }
}
