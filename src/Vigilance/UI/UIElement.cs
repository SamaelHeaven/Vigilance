using System.Numerics;
using FlexLayoutSharp;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;
using Display = FlexLayoutSharp.Display;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public abstract class UIElement : IDeepCloneable
{
    private bool _click;
    internal Node Node = Flex.CreateDefaultNode();

    protected UIElement()
    {
        var measure = Measure;
        Node.StyleSetAlignItems(FlexLayoutSharp.Align.FlexStart);
        LayoutCustom = this is not UIContainer && measure.Method.DeclaringType != typeof(UIElement);
        if (LayoutCustom)
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

    public bool LayoutOverflow => Node.LayoutGetHadOverflow();

    public Vector2 LayoutPosition =>
        new(LayoutLeft + (Parent?.LayoutPosition.X ?? 0), LayoutTop + (Parent?.LayoutPosition.Y ?? 0));

    public Vector2 LayoutSize => new(LayoutWidth, LayoutHeight);

    public Transform LayoutTransform =>
        new(Translate.Calculate(LayoutSize), Scale, Rotation, PivotPoint.Calculate(LayoutSize));

    public bool LayoutCustom { get; }

    public bool Dirty => Node.IsDirty;

    public int ZIndex { get; set; }

    public bool RenderedOutside { get; private set; } = true;

    public Quad RenderedBounds { get; private set; }

    public Matrix3x2 RenderedMatrix { get; private set; }

    public Camera? RenderedCamera { get; private set; }

    public Graphics? RenderedGraphics { get; private set; }

    public Box? RenderedClip { get; private set; }

    public bool LayoutReady { get; private set; }

    public bool MouseInside { get; private set; }

    public Action<UIEvent> OnUpdate
    {
        init => OnUpdateEvent += value;
    }

    public Action<UIEvent> OnClick
    {
        init => OnClickEvent += value;
    }

    public Action<UIEvent> OnPress
    {
        init => OnPressEvent += value;
    }

    public Action<UIEvent> OnRelease
    {
        init => OnReleaseEvent += value;
    }

    public Action<UIEvent> OnMouseEnter
    {
        init => OnMouseEnterEvent += value;
    }

    public Action<UIEvent> OnMouseLeave
    {
        init => OnMouseLeaveEvent += value;
    }

    public CameraProvider Camera { get; set; } = Core.Camera.Null;

    public UIParent? Parent { get; internal set; }

    public UIParent? Root
    {
        get
        {
            var parent = Parent;
            while (parent?.Parent is not null)
                parent = parent.Parent;
            return parent;
        }
    }

    public bool Visible
    {
        get
        {
            var visible = LayoutReady && Display != DisplayMode.None && !RenderedOutside;
            return (Parent?.Visible ?? true) && visible;
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

    public Position Position
    {
        get => (Position)Node.StyleGetPositionType();
        set => Node.StyleSetPositionType((PositionType)value);
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

    object IDeepCloneable.DeepClone()
    {
        return DeepClone();
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

    public UIElement? Closest(UISelector? selector = null)
    {
        return Closest<UIElement>(selector);
    }

    public T? Closest<T>(UISelector? selector = null)
        where T : UIElement
    {
        selector ??= static _ => true;
        if (this is T t && selector.Invoke(t))
            return t;
        return Parent?.Closest<T>(selector);
    }

    public void CalculateLayout(Vector2 size)
    {
        CalculateLayout(size.X, size.Y);
    }

    public void CalculateLayout(float? width = null, float? height = null)
    {
        MarkReady();
        Flex.CalculateLayout(Node, width ?? float.NaN, height ?? float.NaN, FlexLayoutSharp.Direction.LTR);
    }

    public virtual void Update(Entity entity)
    {
        var e = new UIEvent { Entity = entity, Element = this };
        var oldMouseInside = MouseInside;
        MouseInside =
            RenderedGraphics == Renderer.Graphics
            && Visible
            && Collision.CheckPointQuad(Mouse.Position, RenderedBounds);
        OnUpdateEvent?.Invoke(e);
        switch (oldMouseInside)
        {
            case false when MouseInside:
                OnMouseEnterEvent?.Invoke(e);
                break;
            case true when !MouseInside:
                OnMouseLeaveEvent?.Invoke(e);
                break;
        }

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            _click = MouseInside;
            if (MouseInside)
                OnPressEvent?.Invoke(e);
        }

        if (!Mouse.IsButtonReleased(MouseButton.Left))
            return;
        _click = _click && MouseInside;
        if (_click)
            OnClickEvent?.Invoke(e);
        if (MouseInside)
            OnReleaseEvent?.Invoke(e);
    }

    public void Render(Transform transform, Graphics graphics)
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
        Render(LayoutTransform, graphics, Camera);
    }

    public RenderTexture ToTexture(Vector2 size)
    {
        return ToTexture(size.X, size.Y);
    }

    public RenderTexture ToTexture(float? width = null, float? height = null)
    {
        var el = (UIElement)DeepClone();
        el.CalculateLayout(width, height);
        var texture = new RenderTexture(el.LayoutSize.X, el.LayoutSize.Y);
        el.Render(texture.Graphics);
        return texture;
    }

    protected virtual Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return Vector2.NaN;
    }

    protected abstract void Render(Graphics graphics, CameraProvider camera);

    protected virtual object DeepClone()
    {
        var result = (UIElement)MemberwiseClone();
        result._click = false;
        result.LayoutReady = false;
        result.Parent = null;
        result.Node = Flex.CreateDefaultNode();
        Flex.NodeCopyStyle(result.Node, Node);
        result.Attributes = Attributes.DeepClone();
        if (LayoutCustom)
            result.Node.SetMeasureFunc(
                (_, width, widthMode, height, heightMode) =>
                {
                    var size = result.Measure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
                    return new Size(size.X, size.Y);
                }
            );
        return result;
    }

    protected void MarkDirty()
    {
        Node.MarkAsDirty();
    }

    internal void Render(Transform transform, Graphics graphics, CameraProvider camera)
    {
        if (!LayoutReady || Display == DisplayMode.None)
            return;
        Matrix3x2? oldMatrix = null;
        var position = LayoutPosition;
        var size = LayoutSize;
        var offset = position + size * 0.5f;
        if (Position == Position.Absolute && Parent is not null)
        {
            oldMatrix = graphics.PopMatrix();
            offset = new Vector2(LayoutLeft, LayoutTop) + size * 0.5f;
        }

        graphics.PushMatrix();
        graphics.Translate(transform.Position + offset);
        graphics.Scale(transform.Scale);
        graphics.Skew(Skew);
        graphics.Translate(-offset);
        graphics.Rotate(transform.Rotation, transform.PivotPoint + position + size * 0.5f);
        var matrix = graphics.GetMatrix();
        RenderedGraphics = graphics;
        RenderedMatrix = matrix;
        RenderedCamera = camera.Get();
        if (RenderedCamera is not null)
            matrix *= RenderedCamera.Matrix;
        RenderedBounds = new Quad(new Transform(offset, size)).Transform(matrix);
        var layoutBox = new Box(RenderedBounds);
        var oldClip = graphics.GetClip();
        RenderedOutside = oldClip.HasValue && !Collision.CheckBoxes(oldClip.Value, layoutBox);
        var overflowHidden = Overflow == Overflow.Hidden;
        if (overflowHidden)
        {
            var newClip = layoutBox;
            if (oldClip.HasValue)
                newClip = Collision.CheckBoxes(oldClip.Value, newClip, out var intersection) ? intersection : new Box();
            graphics.SetClip(newClip);
        }

        RenderedClip = graphics.GetClip();
        Render(graphics, camera);
        if (overflowHidden)
            graphics.SetClip(oldClip);
        graphics.PopMatrix();
        if (oldMatrix.HasValue)
            graphics.PushMatrix(oldMatrix.Value);
    }

    private void MarkReady()
    {
        LayoutReady = true;
        if (this is not UIParent parent)
            return;
        foreach (var element in parent.Children)
        {
            if (element.Dirty)
                MarkDirty();
            element.MarkReady();
        }
    }
}

public static class UIElementExtensions
{
    public static T Ref<T>(this T self, out T element)
        where T : UIElement
    {
        element = self;
        return element;
    }
}
