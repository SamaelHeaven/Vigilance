using System.Buffers;
using System.Numerics;
using FlexLayoutSharp;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;
using ZLinq;
using Display = FlexLayoutSharp.Display;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public abstract class UIElement : IComposable<UIElement>, IComparable<UIElement>, IFullCloneable
{
    [Flags]
    public enum CloneOptions
    {
        None = 0,
        SkipChildren = 1,
    }

    private bool _click;
    private RenderData _renderData;
    internal Node Node = Flex.CreateDefaultNode();

    protected UIElement()
    {
        var measure = Measure;
        Node.StyleSetAlignItems(FlexLayoutSharp.Align.FlexStart);
        IsLayoutCustom = this is not UIContainer && measure.Method.DeclaringType != typeof(UIElement);
        if (IsLayoutCustom)
            Node.SetMeasureFunc(
                (_, width, widthMode, height, heightMode) =>
                {
                    var size = Measure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
                    return new Size(size.X, size.Y);
                }
            );
    }

    public string Id { get; set; } = "";

    public Attributes Attributes { get; private set; } = new();

    public float LayoutLeft => Node.LayoutGetLeft();

    public float LayoutTop => Node.LayoutGetTop();

    public float LayoutWidth => Node.LayoutGetWidth();

    public float LayoutHeight => Node.LayoutGetHeight();

    public Insets LayoutPadding =>
        new()
        {
            Top = Node.LayoutGetPadding(Edge.Top),
            Right = Node.LayoutGetPadding(Edge.Right),
            Bottom = Node.LayoutGetPadding(Edge.Bottom),
            Left = Node.LayoutGetPadding(Edge.Left),
        };

    public Insets LayoutMargin =>
        new()
        {
            Top = Node.LayoutGetMargin(Edge.Top),
            Right = Node.LayoutGetMargin(Edge.Right),
            Bottom = Node.LayoutGetMargin(Edge.Bottom),
            Left = Node.LayoutGetMargin(Edge.Left),
        };

    public bool LayoutHadOverflow => Node.LayoutGetHadOverflow();

    public Vector2 LayoutPosition
    {
        get
        {
            var x = LayoutLeft;
            var y = LayoutTop;
            foreach (var parent in this.Ancestors())
            {
                x += parent.LayoutLeft;
                y += parent.LayoutTop;
            }

            return new Vector2(x, y);
        }
    }

    public Vector2 LayoutSize => new(LayoutWidth, LayoutHeight);

    public Transform LayoutTransform =>
        new(Translate.Calculate(LayoutSize), Scale, Rotation, PivotPoint.Calculate(LayoutSize));

    public bool IsLayoutCustom { get; }

    public bool IsDirty => Node.IsDirty;

    public int ZIndex { get; set; }

    public bool? Culling { get; set; } = null;

    public bool WasRenderedOutside { get; private set; } = true;

    public Quad RenderedBounds { get; private set; }

    public Matrix3x2 RenderedMatrix { get; private set; }

    public Camera? RenderedCamera { get; private set; }

    public Graphics? RenderedGraphics { get; private set; }

    public Box? RenderedClip { get; private set; }

    public bool IsLayoutReady { get; private set; }

    public bool IsMouseInside { get; private set; }

    public CameraProvider Camera { get; set; } = Core.Camera.Null;

    public UIParent? Parent { get; internal set; }

    public UIParent? Root => (UIParent?)this.Ancestors().LastOrDefault();

    public bool IsVisible
    {
        get
        {
            foreach (var element in this.AncestorsAndSelf())
                if (!(element.IsLayoutReady && element.Display != DisplayMode.None && !element.WasRenderedOutside))
                    return false;
            return true;
        }
    }

    public DisplayMode Display
    {
        get => (DisplayMode)Node.StyleGetDisplay();
        set => Node.StyleSetDisplay((Display)value);
    }

    public Overflow Overflow { get; set; }

    public Insets Padding
    {
        get =>
            new()
            {
                Top = PaddingTop,
                Right = PaddingRight,
                Bottom = PaddingBottom,
                Left = PaddingLeft,
            };
        set
        {
            PaddingTop = value.Top;
            PaddingRight = value.Right;
            PaddingBottom = value.Bottom;
            PaddingLeft = value.Left;
        }
    }

    public Unit PaddingVertical
    {
        set
        {
            PaddingTop = value;
            PaddingBottom = value;
        }
    }

    public Unit PaddingHorizontal
    {
        set
        {
            PaddingLeft = value;
            PaddingRight = value;
        }
    }

    public Unit PaddingTop
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Top));
        set => Unit.SetUnit(value, Edge.Top, Node.StyleSetPadding, Node.StyleSetPaddingPercent);
    }

    public Unit PaddingRight
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Right));
        set => Unit.SetUnit(value, Edge.Right, Node.StyleSetPadding, Node.StyleSetPaddingPercent);
    }

    public Unit PaddingBottom
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Bottom));
        set => Unit.SetUnit(value, Edge.Bottom, Node.StyleSetPadding, Node.StyleSetPaddingPercent);
    }

    public Unit PaddingLeft
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Left));
        set => Unit.SetUnit(value, Edge.Left, Node.StyleSetPadding, Node.StyleSetPaddingPercent);
    }

    public Insets Margin
    {
        get =>
            new()
            {
                Top = MarginTop,
                Right = MarginRight,
                Bottom = MarginBottom,
                Left = MarginLeft,
            };
        set
        {
            MarginTop = value.Top;
            MarginRight = value.Right;
            MarginBottom = value.Bottom;
            MarginLeft = value.Left;
        }
    }

    public Unit MarginVertical
    {
        set
        {
            MarginTop = value;
            MarginBottom = value;
        }
    }

    public Unit MarginHorizontal
    {
        set
        {
            MarginLeft = value;
            MarginRight = value;
        }
    }

    public Unit MarginTop
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Top));
        set => Unit.SetUnit(value, Edge.Top, Node.StyleSetMarginAuto, Node.StyleSetMargin, Node.StyleSetMarginPercent);
    }

    public Unit MarginRight
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Right));
        set =>
            Unit.SetUnit(value, Edge.Right, Node.StyleSetMarginAuto, Node.StyleSetMargin, Node.StyleSetMarginPercent);
    }

    public Unit MarginBottom
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Bottom));
        set =>
            Unit.SetUnit(value, Edge.Bottom, Node.StyleSetMarginAuto, Node.StyleSetMargin, Node.StyleSetMarginPercent);
    }

    public Unit MarginLeft
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Left));
        set => Unit.SetUnit(value, Edge.Left, Node.StyleSetMarginAuto, Node.StyleSetMargin, Node.StyleSetMarginPercent);
    }

    public Unit Width
    {
        get => Unit.FromValue(Node.StyleGetWidth());
        set => Unit.SetUnit(value, Node.StyleSetWidthAuto, Node.StyleSetWidth, Node.StyleSetWidthPercent);
    }

    public Unit Height
    {
        get => Unit.FromValue(Node.StyleGetHeight());
        set => Unit.SetUnit(value, Node.StyleSetHeightAuto, Node.StyleSetHeight, Node.StyleSetHeightPercent);
    }

    public Dimensions Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Unit MinWidth
    {
        get => Unit.FromValue(Node.StyleGetMinWidth());
        set => Unit.SetUnit(value, Node.StyleSetMinWidth, Node.StyleSetMinWidthPercent);
    }

    public Unit MinHeight
    {
        get => Unit.FromValue(Node.StyleGetMinHeight());
        set => Unit.SetUnit(value, Node.StyleSetMinHeight, Node.StyleSetMinHeightPercent);
    }

    public Dimensions MinSize
    {
        get => new(MinWidth, MinHeight);
        set
        {
            MinWidth = value.X;
            MinHeight = value.Y;
        }
    }

    public Unit MaxWidth
    {
        get => Unit.FromValue(Node.StyleGetMaxWidth());
        set => Unit.SetUnit(value, Node.StyleSetMaxWidth, Node.StyleSetMaxWidthPercent);
    }

    public Unit MaxHeight
    {
        get => Unit.FromValue(Node.StyleGetMaxHeight());
        set => Unit.SetUnit(value, Node.StyleSetMaxHeight, Node.StyleSetMaxHeightPercent);
    }

    public Dimensions MaxSize
    {
        get => new(MaxWidth, MaxHeight);
        set
        {
            MaxWidth = value.X;
            MaxHeight = value.Y;
        }
    }

    public Align AlignSelf
    {
        get => (Align)Node.StyleGetAlignSelf();
        set => Node.StyleSetAlignSelf((FlexLayoutSharp.Align)value);
    }

    public Unit Basis
    {
        get => Unit.FromValue(Node.NodeStyleGetFlexBasis());
        set =>
            Unit.SetUnit(value, Node.NodeStyleSetFlexBasisAuto, Node.StyleSetFlexBasis, Node.StyleSetFlexBasisPercent);
    }

    public float Grow
    {
        get => Node.StyleGetFlexGrow();
        set => Node.StyleSetFlexGrow(value);
    }

    public float Shrink
    {
        get => Node.StyleGetFlexShrink();
        set => Node.StyleSetFlexShrink(value);
    }

    public PositionType Position
    {
        get => (PositionType)Node.StyleGetPositionType();
        set => Node.StyleSetPositionType((FlexLayoutSharp.PositionType)value);
    }

    public Insets Insets
    {
        get =>
            new()
            {
                Top = Top,
                Right = Right,
                Bottom = Bottom,
                Left = Left,
            };
        set
        {
            Top = value.Top;
            Right = value.Right;
            Bottom = value.Bottom;
            Left = value.Left;
        }
    }

    public Unit Top
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Top));
        set => Unit.SetUnit(value, Edge.Top, Node.StyleSetPosition, Node.StyleSetPositionPercent);
    }

    public Unit Right
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Right));
        set => Unit.SetUnit(value, Edge.Right, Node.StyleSetPosition, Node.StyleSetPositionPercent);
    }

    public Unit Bottom
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Bottom));
        set => Unit.SetUnit(value, Edge.Bottom, Node.StyleSetPosition, Node.StyleSetPositionPercent);
    }

    public Unit Left
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Left));
        set => Unit.SetUnit(value, Edge.Left, Node.StyleSetPosition, Node.StyleSetPositionPercent);
    }

    public Dimensions Translate { get; set; } = new();

    public Unit TranslateX
    {
        get => Translate.X;
        set => Translate = new Dimensions(value, Translate.Y);
    }

    public Unit TranslateY
    {
        get => Translate.Y;
        set => Translate = new Dimensions(Translate.X, value);
    }

    public Vector2 Scale { get; set; } = Vector2.One;

    public float ScaleX
    {
        get => Scale.X;
        set => Scale = new Vector2(value, Scale.Y);
    }

    public float ScaleY
    {
        get => Scale.Y;
        set => Scale = new Vector2(Scale.X, value);
    }

    public Vector2 Skew { get; set; }

    public float SkewX
    {
        get => Skew.X;
        set => Skew = new Vector2(value, Skew.Y);
    }

    public float SkewY
    {
        get => Skew.Y;
        set => Skew = new Vector2(Skew.X, value);
    }

    public float Rotation { get; set; } = 0;

    public Dimensions PivotPoint { get; set; } = new();

    public Unit PivotPointX
    {
        get => PivotPoint.X;
        set => PivotPoint = new Dimensions(value, PivotPoint.Y);
    }

    public Unit PivotPointY
    {
        get => PivotPoint.Y;
        set => PivotPoint = new Dimensions(PivotPoint.X, value);
    }

    int IComparable<UIElement>.CompareTo(UIElement? other)
    {
        return other is null ? 1 : ZIndex.CompareTo(other.ZIndex);
    }

    UIElement IComposable<UIElement>.ToComponent()
    {
        return this;
    }

    object IDeepCloneable.DeepClone()
    {
        return DeepClone(CloneOptions.None);
    }

    object IShallowCloneable.ShallowClone()
    {
        return ShallowClone(CloneOptions.None);
    }

    internal object DeepClone(CloneOptions options)
    {
        Dictionary<UIElement, UIElement>? cloneMap = null;
        UIElement clone = null!;
        foreach (var node in this.DescendantsPostOrderAndSelf())
        {
            clone = Clone(node, options);
            if ((options & CloneOptions.SkipChildren) != 0 && clone is UIParent parent)
                foreach (var child in node.Children())
                    parent.Add(
                        (cloneMap ??= new Dictionary<UIElement, UIElement>(this.DescendantsAndSelf().Count()))[child]
                    );
            clone.CloneSelf();
            if ((options & CloneOptions.SkipChildren) == 0)
                break;
            cloneMap?[node] = clone;
        }

        return cloneMap is null ? clone : cloneMap[this];
    }

    internal object ShallowClone(CloneOptions options)
    {
        var clone = Clone(this, options);
        if ((options & CloneOptions.SkipChildren) != 0 && clone is UIParent parent)
            foreach (var child in this.Children())
                parent.Add(child);
        clone.CloneSelf();
        return clone;
    }

    public event Action<UIEvent>? OnUpdateEvent;

    public event Action<UIEvent>? OnMouseEnterEvent;

    public event Action<UIEvent>? OnMouseLeaveEvent;

    public event Action<UIEvent>? OnClickEvent;

    public event Action<UIEvent>? OnPressEvent;

    public event Action<UIEvent>? OnReleaseEvent;

    public void Remove()
    {
        Parent?.Remove(this);
    }

    public void CalculateLayout(Vector2 size)
    {
        CalculateLayout(size.X, size.Y);
    }

    public void CalculateLayout(float width = float.NaN, float height = float.NaN)
    {
        MarkReady();
        Flex.CalculateLayout(Node, width, height, FlexLayoutSharp.Direction.LTR);
    }

    public void Update()
    {
        Update(Entity.Null);
    }

    public void Update(Entity entity)
    {
        if (!IsLayoutReady)
            return;
        foreach (var element in this.DescendantsPostOrderAndSelf())
            Update(element, entity);
    }

    public void Render(in Transform transform, Graphics graphics)
    {
        graphics.PushMatrix();
        graphics.Translate(transform.Position);
        graphics.Scale(transform.Scale);
        graphics.Pivot(
            new Transform
            {
                Position = LayoutPosition,
                Scale = LayoutSize,
                Rotation = transform.Rotation,
                PivotPoint = transform.PivotPoint,
            },
            true
        );
        Render(graphics);
        graphics.PopMatrix();
    }

    public void Render(Graphics graphics)
    {
        Render(graphics, Camera);
    }

    public RenderTexture ToTexture(Vector2 size)
    {
        return ToTexture(size.X, size.Y);
    }

    public RenderTexture ToTexture(float width = float.NaN, float height = float.NaN)
    {
        CalculateLayout(width, height);
        var texture = new RenderTexture(LayoutSize);
        Render(texture.Graphics);
        return texture;
    }

    protected virtual void CloneSelf() { }

    protected virtual void UpdateSelf(Entity entity) { }

    protected virtual void BeginRender(Graphics graphics, CameraProvider camera) { }

    protected virtual void RenderSelf(Graphics graphics, CameraProvider camera) { }

    protected virtual void EndRender(Graphics graphics, CameraProvider camera) { }

    protected virtual Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return Vector2.NaN;
    }

    protected void MarkDirty()
    {
        Node.MarkAsDirty();
    }

    private static void Update(UIElement element, in Entity entity)
    {
        if (!element.IsLayoutReady)
            return;
        var @event = new UIEvent { Entity = entity, Element = element };
        var oldMouseInside = element.IsMouseInside;
        element.IsMouseInside =
            element.RenderedGraphics == Renderer.Graphics
            && Mouse.OnScreen
            && element.IsVisible
            && Collision.CheckPointQuad(Mouse.Position, element.RenderedBounds);
        element.OnUpdateEvent?.Invoke(@event);
        switch (oldMouseInside)
        {
            case false when element.IsMouseInside:
                element.OnMouseEnterEvent?.Invoke(@event);
                break;
            case true when !element.IsMouseInside:
                element.OnMouseLeaveEvent?.Invoke(@event);
                break;
        }

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            element._click = element.IsMouseInside;
            if (element.IsMouseInside)
                element.OnPressEvent?.Invoke(@event);
        }

        if (Mouse.IsButtonReleased(MouseButton.Left))
        {
            element._click = element is { _click: true, IsMouseInside: true };
            if (element._click)
                element.OnClickEvent?.Invoke(@event);
            if (element.IsMouseInside)
                element.OnReleaseEvent?.Invoke(@event);
        }

        element.UpdateSelf(entity);
    }

    private void Render(Graphics graphics, CameraProvider camera)
    {
        _renderData = new RenderData(this);
        if (!_renderData.ShouldRender)
            return;
        var capacity = this.DescendantsAndSelf().Count();
        var stack = ArrayPool<UIElement>.Shared.Rent(capacity);
        var count = 0;
        var maxCount = 0;
        try
        {
            stack[count++] = this;
            while (count != 0)
            {
                maxCount = maxCount.Max(count);
                var element = stack[--count];
                switch (element._renderData.Phase)
                {
                    case RenderPhase.Begin:
                        BeginRender(ref stack, ref count, ref maxCount, element, graphics, camera);
                        break;
                    case RenderPhase.End:
                        EndRender(element, graphics, camera);
                        break;
                }
            }
        }
        finally
        {
            Array.Clear(stack, 0, maxCount);
            ArrayPool<UIElement>.Shared.Return(stack);
        }
    }

    private static void BeginRender(
        ref UIElement[] stack,
        ref int count,
        ref int maxCount,
        UIElement element,
        Graphics graphics,
        CameraProvider camera
    )
    {
        ref var data = ref stack[count++]._renderData;
        if (!data.ShouldRender)
            return;
        var transform = element.LayoutTransform;
        var position = element.LayoutPosition;
        var size = element.LayoutSize;
        var offset = position + size * 0.5f;
        if (element is { Position: PositionType.Absolute, Parent: not null })
        {
            data.OldMatrix = graphics.PopMatrix();
            offset = new Vector2(element.LayoutLeft, element.LayoutTop) + size * 0.5f;
        }

        graphics.PushMatrix();
        graphics.Translate(transform.Position + offset);
        graphics.Scale(transform.Scale);
        graphics.Skew(element.Skew);
        graphics.Translate(-offset);
        graphics.Rotate(transform.Rotation, transform.PivotPoint + position + size * 0.5f);
        var matrix = graphics.GetMatrix();
        element.RenderedGraphics = graphics;
        element.RenderedMatrix = matrix;
        element.RenderedCamera = camera.Get();
        if (element.RenderedCamera is not null)
            matrix *= element.RenderedCamera.Matrix;
        element.RenderedBounds = new Quad(new Transform(offset, size)).Transform(matrix);
        var layoutBox = new Box(element.RenderedBounds);
        data.OldClip = graphics.GetClip();
        element.WasRenderedOutside = data.OldClip.HasValue && !Collision.CheckBoxes(data.OldClip.Value, layoutBox);
        data.OverflowHidden = element.Overflow == Overflow.Hidden;
        if (data.OverflowHidden)
        {
            var newClip = layoutBox;
            if (data.OldClip.HasValue)
                newClip = Collision.CheckBoxes(data.OldClip.Value, newClip, out var intersection)
                    ? intersection
                    : new Box();
            graphics.SetClip(newClip);
        }

        element.RenderedClip = graphics.GetClip();
        if (element.Culling.HasValue)
        {
            data.OldCulling = graphics.Culling();
            graphics.SetCulling(element.Culling!.Value);
        }

        data.Phase = RenderPhase.End;
        var children = element.Children();
        var childrenCount = children.Count();
        count += childrenCount;
        if (count > stack.Length)
        {
            var newStack = ArrayPool<UIElement>.Shared.Rent(count * 2);
            Array.Copy(stack, newStack, maxCount);
            Array.Clear(stack, 0, maxCount);
            ArrayPool<UIElement>.Shared.Return(stack);
            stack = newStack;
        }

        var shouldSort = false;
        var i = count;
        foreach (var child in children)
        {
            stack[--i] = child;
            child._renderData = new RenderData(child);
            if (child.ZIndex != 0)
                shouldSort = true;
        }

        if (shouldSort)
            stack.AsSpan(i, count - i).Sort();
        element.BeginRender(graphics, camera);
        element.RenderSelf(graphics, camera);
    }

    private static void EndRender(UIElement element, Graphics graphics, CameraProvider camera)
    {
        ref var data = ref element._renderData;
        if (!data.ShouldRender)
            return;
        element.EndRender(graphics, camera);
        if (data.OldCulling.HasValue)
            graphics.SetCulling(data.OldCulling.Value);
        if (data.OverflowHidden)
            graphics.SetClip(data.OldClip);
        graphics.PopMatrix();
        if (data.OldMatrix.HasValue)
            graphics.PushMatrix(data.OldMatrix.Value);
    }

    private static UIElement Clone(UIElement element, CloneOptions options)
    {
        var result = (UIElement)element.MemberwiseClone();
        result._click = false;
        result.IsLayoutReady = false;
        result.Parent = null;
        result.Node = Flex.CreateDefaultNode();
        Flex.NodeCopyStyle(result.Node, element.Node);
        result.Attributes = element.Attributes.ShallowClone();
        if (element.IsLayoutCustom)
            result.Node.SetMeasureFunc(
                (_, width, widthMode, height, heightMode) =>
                {
                    var size = result.Measure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
                    return new Size(size.X, size.Y);
                }
            );
        if (result is not UIParent parent)
            return result;
        parent.ChildrenList = options.HasFlag(CloneOptions.SkipChildren)
            ? []
            : new ValueList<UIElement>(parent.ChildrenList.Count);
        parent.ChildrenOperations = [];
        parent.DeferredCount = 0;
        return result;
    }

    private void MarkReady()
    {
        foreach (var element in this.DescendantsAndSelf())
            element.IsLayoutReady = true;
    }

    private struct RenderData
    {
        public RenderPhase Phase;
        public Matrix3x2? OldMatrix;
        public Box? OldClip;
        public bool? OldCulling;
        public bool OverflowHidden;
        public readonly bool ShouldRender;

        public RenderData(UIElement element)
        {
            ShouldRender = element.IsLayoutReady && element.Display != DisplayMode.None;
        }
    }

    private enum RenderPhase
    {
        Begin,
        End,
    }

    public struct Traverser : ITraverser<Traverser, UIElement>
    {
        private UIParent.ChildEnumerator _enumerator;
        private readonly bool _deferred;
        private bool _hasEnumerator;

        public UIElement Origin { get; }

        internal Traverser(UIElement origin, bool deferred = true)
        {
            Origin = origin;
            _deferred = deferred;
        }

        public Traverser ConvertToTraverser(UIElement next)
        {
            return new Traverser(next, _deferred);
        }

        public bool TryGetChildCount(out int count)
        {
            count = Origin is UIParent parent ? parent.Children.Count : 0;
            return true;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            hasChild = Origin is UIParent { Children.Count: > 0 };
            return true;
        }

        public bool TryGetParent(out UIParent parent)
        {
            parent = Origin.Parent!;
            return Origin.Parent is not null;
        }

        bool ITraverser<Traverser, UIElement>.TryGetParent(out UIElement parent)
        {
            parent = Origin.Parent!;
            return Origin.Parent is not null;
        }

        public bool TryGetNextChild(out UIElement child)
        {
            if (!_hasEnumerator)
            {
                if (Origin is not UIParent parent)
                {
                    child = null!;
                    return false;
                }

                _enumerator = parent.Children.Deferred(_deferred).GetEnumerator();
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

        public bool TryGetNextSibling(out UIElement next)
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
                _enumerator = parent.Children.Deferred(_deferred).GetEnumerator();
                _hasEnumerator = true;
                while (_enumerator.MoveNext())
                    if (_enumerator.Current == Origin)
                        goto BEGIN;
            }

            next = null!;
            return false;
        }

        public bool TryGetPreviousSibling(out UIElement previous)
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
                _enumerator = parent.Children.Deferred(_deferred).GetEnumerator();
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

public static partial class UIElementExtensions
{
    extension<T>(T element)
        where T : UIElement
    {
        public Action<UIEvent<T>> OnUpdate
        {
            set => element.OnUpdateEvent += e => value.Invoke(e);
        }

        public Action<UIEvent<T>> OnClick
        {
            set => element.OnClickEvent += e => value.Invoke(e);
        }

        public Action<UIEvent<T>> OnPress
        {
            set => element.OnPressEvent += e => value.Invoke(e);
        }

        public Action<UIEvent<T>> OnRelease
        {
            set => element.OnReleaseEvent += e => value.Invoke(e);
        }

        public Action<UIEvent<T>> OnMouseEnter
        {
            set => element.OnMouseEnterEvent += e => value.Invoke(e);
        }

        public Action<UIEvent<T>> OnMouseLeave
        {
            set => element.OnMouseLeaveEvent += e => value.Invoke(e);
        }

        public T Ref(out T el)
        {
            el = element;
            return el;
        }

        public T DeepClone(UIElement.CloneOptions options)
        {
            return (T)element.DeepClone(options);
        }

        public T ShallowClone(UIElement.CloneOptions options)
        {
            return (T)element.ShallowClone(options);
        }
    }
}
