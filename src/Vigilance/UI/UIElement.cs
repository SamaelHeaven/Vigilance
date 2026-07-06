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

public abstract class UIElement : IFullCloneable
{
    [Flags]
    public enum CloneOptions
    {
        None = 0,
        SkipChildren = 1 << 0,
        ClearSignals = 1 << 1,
    }

    private Unit _appliedMarginBottom = Unit.Undefined;
    private Unit _appliedMarginLeft = Unit.Undefined;
    private Unit _appliedMarginRight = Unit.Undefined;
    private Unit _appliedMarginTop = Unit.Undefined;

    private bool _click;
    private ValueList<IUIComponent> _components = [];
    private Unit _marginBottom = Unit.Undefined;
    private Unit _marginLeft = Unit.Undefined;
    private Unit _marginRight = Unit.Undefined;
    private Unit _marginTop = Unit.Undefined;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onBeginRenderHandlers;
    private Func<UIElement, bool>? _onClickHandlers;
    private Func<UIElement, bool>? _onCloneHandlers;
    private Func<UIElement, bool>? _onDirtyHandlers;
    private Func<UIElement, bool>? _onDisabledUpdateHandlers;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onEndRenderHandlers;
    private Func<UIElement, bool>? _onMouseEnterHandlers;
    private Func<UIElement, bool>? _onMouseLeaveHandlers;
    private Func<UIElement, bool>? _onPressHandlers;
    private Func<UIElement, bool>? _onReleaseHandlers;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onRenderHandlers;
    private Func<UIElement, bool>? _onUpdateHandlers;
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

    public ReadOnlySpan<IUIComponent> Components
    {
        get => _components.AsSpan();
        init
        {
            _components.EnsureCapacity(value.Length);
            foreach (var component in value)
                Attach(component);
        }
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

    public BlendMode? BlendMode { get; set; } = null;

    public Shader? Shader { get; set; } = null;

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

    public bool IsDisabled { get; set; }

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
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Top,
                (node, edge, padding) => node.StyleSetPadding(edge, padding),
                (node, edge, padding) => node.StyleSetPaddingPercent(edge, padding)
            );
    }

    public Unit PaddingRight
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Right));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Right,
                (node, edge, padding) => node.StyleSetPadding(edge, padding),
                (node, edge, padding) => node.StyleSetPaddingPercent(edge, padding)
            );
    }

    public Unit PaddingBottom
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Bottom));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Bottom,
                (node, edge, padding) => node.StyleSetPadding(edge, padding),
                (node, edge, padding) => node.StyleSetPaddingPercent(edge, padding)
            );
    }

    public Unit PaddingLeft
    {
        get => Unit.FromValue(Node.StyleGetPadding(Edge.Left));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Left,
                (node, edge, padding) => node.StyleSetPadding(edge, padding),
                (node, edge, padding) => node.StyleSetPaddingPercent(edge, padding)
            );
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
        get => _marginTop;
        set
        {
            _marginTop = value;
            if (!IsMarginManagedByGap())
                ApplyDeclaredMargin();
            else
                MarkDirty();
        }
    }

    public Unit MarginRight
    {
        get => _marginRight;
        set
        {
            _marginRight = value;
            if (!IsMarginManagedByGap())
                ApplyDeclaredMargin();
            else
                MarkDirty();
        }
    }

    public Unit MarginBottom
    {
        get => _marginBottom;
        set
        {
            _marginBottom = value;
            if (!IsMarginManagedByGap())
                ApplyDeclaredMargin();
            else
                MarkDirty();
        }
    }

    public Unit MarginLeft
    {
        get => _marginLeft;
        set
        {
            _marginLeft = value;
            if (!IsMarginManagedByGap())
                ApplyDeclaredMargin();
            else
                MarkDirty();
        }
    }

    internal Insets DeclaredMargin =>
        new()
        {
            Top = _marginTop,
            Right = _marginRight,
            Bottom = _marginBottom,
            Left = _marginLeft,
        };

    public Unit Width
    {
        get => Unit.FromValue(Node.StyleGetWidth());
        set =>
            Unit.SetUnit(
                Node,
                value,
                node => node.StyleSetWidthAuto(),
                (node, width) => node.StyleSetWidth(width),
                (node, width) => node.StyleSetWidthPercent(width)
            );
    }

    public Unit Height
    {
        get => Unit.FromValue(Node.StyleGetHeight());
        set =>
            Unit.SetUnit(
                Node,
                value,
                node => node.StyleSetHeightAuto(),
                (node, height) => node.StyleSetHeight(height),
                (node, height) => node.StyleSetHeightPercent(height)
            );
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
        set =>
            Unit.SetUnit(
                Node,
                value,
                (node, width) => node.StyleSetMinWidth(width),
                (node, width) => node.StyleSetMinWidthPercent(width)
            );
    }

    public Unit MinHeight
    {
        get => Unit.FromValue(Node.StyleGetMinHeight());
        set =>
            Unit.SetUnit(
                Node,
                value,
                (node, height) => node.StyleSetMinHeight(height),
                (node, height) => node.StyleSetMinHeightPercent(height)
            );
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
        set =>
            Unit.SetUnit(
                Node,
                value,
                (node, width) => node.StyleSetMaxWidth(width),
                (node, width) => node.StyleSetMaxWidthPercent(width)
            );
    }

    public Unit MaxHeight
    {
        get => Unit.FromValue(Node.StyleGetMaxHeight());
        set =>
            Unit.SetUnit(
                Node,
                value,
                (node, height) => node.StyleSetMaxHeight(height),
                (node, height) => node.StyleSetMaxHeightPercent(height)
            );
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
            Unit.SetUnit(
                Node,
                value,
                node => node.NodeStyleSetFlexBasisAuto(),
                (node, basis) => node.StyleSetFlexBasis(basis),
                (node, basis) => node.StyleSetFlexBasisPercent(basis)
            );
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
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Top,
                (node, edge, top) => node.StyleSetPosition(edge, top),
                (node, edge, top) => node.StyleSetPositionPercent(edge, top)
            );
    }

    public Unit Right
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Right));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Right,
                (node, edge, right) => node.StyleSetPosition(edge, right),
                (node, edge, right) => node.StyleSetPositionPercent(edge, right)
            );
    }

    public Unit Bottom
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Bottom));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Bottom,
                (node, edge, bottom) => node.StyleSetPosition(edge, bottom),
                (node, edge, bottom) => node.StyleSetPositionPercent(edge, bottom)
            );
    }

    public Unit Left
    {
        get => Unit.FromValue(Node.StyleGetPosition(Edge.Left));
        set =>
            Unit.SetUnit(
                Node,
                value,
                Edge.Left,
                (node, edge, left) => node.StyleSetPosition(edge, left),
                (node, edge, left) => node.StyleSetPositionPercent(edge, left)
            );
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

    public Entity Entity { get; private set; }

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

    public Signal<UIElement> OnUpdateSignal => new(ref _onUpdateHandlers);

    public Signal<UIElement> OnDisabledUpdateSignal => new(ref _onDisabledUpdateHandlers);

    public Signal<UIElement> OnDirtySignal => new(ref _onDirtyHandlers);

    public Signal<UIElement> OnMouseEnterSignal => new(ref _onMouseEnterHandlers);

    public Signal<UIElement> OnMouseLeaveSignal => new(ref _onMouseLeaveHandlers);

    public Signal<UIElement> OnClickSignal => new(ref _onClickHandlers);

    public Signal<UIElement> OnPressSignal => new(ref _onPressHandlers);

    public Signal<UIElement> OnReleaseSignal => new(ref _onReleaseHandlers);

    public Signal<UIElement> OnCloneSignal => new(ref _onCloneHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnBeginRenderSignal => new(ref _onBeginRenderHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnRenderSignal => new(ref _onRenderHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnEndRenderSignal => new(ref _onEndRenderHandlers);

    object IDeepCloneable.DeepClone()
    {
        return DeepClone(CloneOptions.None);
    }

    object IShallowCloneable.ShallowClone()
    {
        return ShallowClone(CloneOptions.None);
    }

    internal bool ApplyDeclaredMargin()
    {
        return ApplyComputedMargin(DeclaredMargin);
    }

    internal bool ApplyComputedMargin(in Insets margin)
    {
        if (
            _appliedMarginTop == margin.Top
            && _appliedMarginRight == margin.Right
            && _appliedMarginBottom == margin.Bottom
            && _appliedMarginLeft == margin.Left
        )
            return false;
        SetNodeMargin(Edge.Top, margin.Top);
        SetNodeMargin(Edge.Right, margin.Right);
        SetNodeMargin(Edge.Bottom, margin.Bottom);
        SetNodeMargin(Edge.Left, margin.Left);
        _appliedMarginTop = margin.Top;
        _appliedMarginRight = margin.Right;
        _appliedMarginBottom = margin.Bottom;
        _appliedMarginLeft = margin.Left;
        MarkDirty();
        return true;
    }

    internal object DeepClone(CloneOptions options)
    {
        ValueDictionary<UIElement, UIElement> cloneMap = default;
        var hasCloneMap = false;
        UIElement clone = null!;
        foreach (var node in this.DescendantsPostOrderAndSelf())
        {
            clone = Clone(node, options);
            DeepCloneComponents(clone);
            if ((options & CloneOptions.SkipChildren) != 0 && clone is UIParent parent)
                foreach (var child in node.Children())
                {
                    if (!hasCloneMap)
                    {
                        cloneMap = new ValueDictionary<UIElement, UIElement>(this.DescendantsAndSelf().Count());
                        hasCloneMap = true;
                    }

                    parent.Add(cloneMap[child]);
                }

            if ((options & CloneOptions.ClearSignals) != 0)
                clone.ClearSignals();
            clone.OnClone();
            OnCloneSignal.Invoke(clone);
            if ((options & CloneOptions.SkipChildren) == 0)
                break;
            cloneMap[node] = clone;
        }

        return hasCloneMap ? cloneMap[this] : clone;
    }

    internal object ShallowClone(CloneOptions options)
    {
        var clone = Clone(this, options);
        clone._components = clone._components.AsValueEnumerable().ToValueList();
        if ((options & CloneOptions.SkipChildren) != 0 && clone is UIParent parent)
            foreach (var child in this.Children())
                parent.Add(child);
        if ((options & CloneOptions.ClearSignals) != 0)
            clone.ClearSignals();
        clone.OnClone();
        OnCloneSignal.Invoke(clone);
        return clone;
    }

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
        var wrapMinSizeCapacity = 0;
        foreach (var element in this.DescendantsAndSelf())
        {
            element.ApplyDeclaredMargin();
            wrapMinSizeCapacity++;
        }

        var wrapMinSizes = ArrayPool<WrapMinSizeState>.Shared.Rent(wrapMinSizeCapacity);
        try
        {
            var wrapMinSizeCount = PrepareWrapMinSizes(wrapMinSizes);
            if (HasGapContainers())
            {
                bool changed;
                do
                {
                    Flex.CalculateLayout(Node, width, height, FlexLayoutSharp.Direction.LTR);
                    changed = false;
                    foreach (var element in this.DescendantsAndSelf())
                        if (element is UIContainer container)
                            changed |= container.ApplyGapMargins();
                } while (changed);
            }
            else
            {
                Flex.CalculateLayout(Node, width, height, FlexLayoutSharp.Direction.LTR);
            }

            var saved = wrapMinSizes.AsSpan(0, wrapMinSizeCount);
            if (ApplyWrapMinSizeFloors(saved, width, height))
                Flex.CalculateLayout(Node, width, height, FlexLayoutSharp.Direction.LTR);
            RestoreWrapMinSizeDimensions(saved);
        }
        finally
        {
            ArrayPool<WrapMinSizeState>.Shared.Return(wrapMinSizes);
        }
    }

    public void Attach(IUIComponent component)
    {
        _components.Add(component);
        component.Attach(this);
    }

    public void Detach(IUIComponent component)
    {
        component.Detach(this);
        _components.Remove(component);
    }

    public void Update()
    {
        Update(Entity.Null);
    }

    public void Update(in Entity entity)
    {
        Entity = entity;
        if (!IsLayoutReady)
            return;
        foreach (var element in this.DescendantsPostOrderAndSelf())
            Update(element, entity);
    }

    public void Render(in Transform transform, Graphics graphics)
    {
        graphics.PushMatrix();
        graphics.Translate(transform.Position);
        graphics.Scale(transform.Scale.Abs());
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

    public void ResetLayoutAndTransform()
    {
        Camera = Core.Camera.Null;
        Margin = Unit.Undefined;
        Position = PositionType.Relative;
        Insets = Unit.Undefined;
        Translate = Unit.Undefined;
        Scale = Vector2.One;
        Skew = Vector2.Zero;
        Rotation = 0;
        PivotPoint = Vector2.Zero;
    }

    public void ClearSignals()
    {
        OnUpdateSignal.Clear();
        OnDisabledUpdateSignal.Clear();
        OnDirtySignal.Clear();
        OnMouseEnterSignal.Clear();
        OnMouseLeaveSignal.Clear();
        OnClickSignal.Clear();
        OnPressSignal.Clear();
        OnReleaseSignal.Clear();
        OnCloneSignal.Clear();
        OnBeginRenderSignal.Clear();
        OnRenderSignal.Clear();
        OnEndRenderSignal.Clear();
        OnClearSignals();
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

    protected virtual void OnUpdate() { }

    protected virtual void OnDisabledUpdate() { }

    protected virtual void OnDirty() { }

    protected virtual void OnMouseEnter() { }

    protected virtual void OnMouseLeave() { }

    protected virtual void OnClick() { }

    protected virtual void OnPress() { }

    protected virtual void OnRelease() { }

    protected virtual void OnClone() { }

    protected virtual void OnBeginRender(Graphics graphics, CameraProvider camera) { }

    protected virtual void OnRender(Graphics graphics, CameraProvider camera) { }

    protected virtual void OnEndRender(Graphics graphics, CameraProvider camera) { }

    protected virtual void OnClearSignals() { }

    protected virtual Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return Vector2.NaN;
    }

    protected void MarkDirty()
    {
        Node.MarkAsDirty();
    }

    private bool IsMarginManagedByGap()
    {
        return Parent is UIContainer { HasGap: true };
    }

    private bool HasGapContainers()
    {
        foreach (var element in this.DescendantsAndSelf())
            if (element is UIContainer { HasGap: true })
                return true;
        return false;
    }

    private int PrepareWrapMinSizes(WrapMinSizeState[] saved)
    {
        var count = 0;
        foreach (var element in this.DescendantsAndSelf())
        {
            if (element is not UIContainer { Wrap: not Wrap.NoWrap } container)
                continue;
            if (!HasMinSize(container.MinWidth) && !HasMinSize(container.MinHeight))
                continue;
            saved[count++] = new WrapMinSizeState
            {
                Container = container,
                MinWidth = container.MinWidth,
                MinHeight = container.MinHeight,
                Width = container.Width,
                Height = container.Height,
            };
            container.MinWidth = Unit.Undefined;
            container.MinHeight = Unit.Undefined;
        }

        return count;
    }

    private static bool ApplyWrapMinSizeFloors(
        in ReadOnlySpan<WrapMinSizeState> saved,
        float layoutWidth,
        float layoutHeight
    )
    {
        var needsRelayout = false;
        foreach (var state in saved)
        {
            var container = state.Container;
            container.MinWidth = state.MinWidth;
            container.MinHeight = state.MinHeight;
            var minWidth = ResolveMinSize(state.MinWidth, container, layoutWidth, layoutHeight, false);
            if (minWidth > 0 && container.LayoutWidth + 0.5f < minWidth)
            {
                container.Width = Unit.Fixed(minWidth);
                needsRelayout = true;
            }

            var minHeight = ResolveMinSize(state.MinHeight, container, layoutWidth, layoutHeight, true);
            if (!(minHeight > 0) || !(container.LayoutHeight + 0.5f < minHeight))
                continue;
            container.Height = Unit.Fixed(minHeight);
            needsRelayout = true;
        }

        return needsRelayout;
    }

    private static void RestoreWrapMinSizeDimensions(in ReadOnlySpan<WrapMinSizeState> saved)
    {
        foreach (var state in saved)
        {
            state.Container.Width = state.Width;
            state.Container.Height = state.Height;
            state.Container.MinWidth = state.MinWidth;
            state.Container.MinHeight = state.MinHeight;
        }
    }

    private static bool HasMinSize(Unit unit)
    {
        return unit.Type switch
        {
            UnitType.Fixed or UnitType.Percent => !float.IsNaN(unit.Value),
            _ => false,
        };
    }

    private static float ResolveMinSize(
        Unit min,
        UIContainer container,
        float layoutWidth,
        float layoutHeight,
        bool crossAxis
    )
    {
        if (!HasMinSize(min))
            return 0;
        var parentSize = crossAxis
            ? container.Parent?.LayoutHeight ?? layoutHeight
            : container.Parent?.LayoutWidth ?? layoutWidth;
        if (float.IsNaN(parentSize))
            parentSize = crossAxis ? container.LayoutHeight : container.LayoutWidth;
        return min.Calculate(parentSize);
    }

    private void SetNodeMargin(Edge edge, Unit value)
    {
        Unit.SetUnit(
            Node,
            value,
            edge,
            static (node, e) => node.StyleSetMarginAuto(e),
            static (node, e, margin) => node.StyleSetMargin(e, margin),
            static (node, e, margin) => node.StyleSetMarginPercent(e, margin)
        );
    }

    private static void Update(UIElement element, in Entity entity)
    {
        element.Entity = entity;
        if (!element.IsLayoutReady)
            return;
        element.IsMouseInside =
            element.RenderedGraphics == Renderer.Graphics
            && Mouse.OnScreen
            && element.IsVisible
            && Collision.CheckPointQuad(Mouse.Position, element.RenderedBounds);
        if (element.IsDisabled || entity is { IsNull: false, IsDisabled: true })
        {
            element._click = false;
            element.OnDisabledUpdate();
            element.OnDisabledUpdateSignal.Invoke(element);
            if (!element.IsDirty)
                return;
            element.OnDirty();
            element.OnDirtySignal.Invoke(element);
            return;
        }

        var oldMouseInside = element.IsMouseInside;
        element.OnUpdate();
        element.OnUpdateSignal.Invoke(element);
        switch (oldMouseInside)
        {
            case false when element.IsMouseInside:
                element.OnMouseEnter();
                element.OnMouseEnterSignal.Invoke(element);
                break;
            case true when !element.IsMouseInside:
                element.OnMouseLeave();
                element.OnMouseLeaveSignal.Invoke(element);
                break;
        }

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            element._click = element.IsMouseInside;
            if (element.IsMouseInside)
            {
                element.OnPress();
                element.OnPressSignal.Invoke(element);
            }
        }

        if (Mouse.IsButtonReleased(MouseButton.Left))
        {
            element._click = element is { _click: true, IsMouseInside: true };
            if (element._click)
            {
                element.OnClick();
                element.OnClickSignal.Invoke(element);
            }

            if (element.IsMouseInside)
            {
                element.OnRelease();
                element.OnReleaseSignal.Invoke(element);
            }
        }

        if (!element.IsDirty)
            return;
        element.OnDirty();
        element.OnDirtySignal.Invoke(element);
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
            data.OldMatrix = graphics.PopMatrix();
        graphics.PushMatrix();
        graphics.Translate(transform.Position + offset);
        graphics.Scale(transform.Scale.Abs());
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
        if (element.BlendMode.HasValue)
        {
            data.OldBlendMode = graphics.GetBlendMode();
            graphics.SetBlendMode(element.BlendMode.Value);
        }

        if (element.Shader is not null)
        {
            data.OldShader = graphics.GetShader();
            graphics.SetShader(element.Shader);
        }

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
            stack.AsSpan(i, count - i).Sort((a, b) => b.ZIndex.CompareTo(a.ZIndex));
        element.OnBeginRender(graphics, camera);
        element.OnBeginRenderSignal.Invoke(element, graphics, camera);
        element.OnRender(graphics, camera);
        element.OnRenderSignal.Invoke(element, graphics, camera);
    }

    private static void EndRender(UIElement element, Graphics graphics, CameraProvider camera)
    {
        ref var data = ref element._renderData;
        if (!data.ShouldRender)
            return;
        element.OnEndRenderSignal.Invoke(element, graphics, camera);
        element.OnEndRender(graphics, camera);
        if (data.OldCulling.HasValue)
            graphics.SetCulling(data.OldCulling.Value);
        if (data.OldShader is not null)
            graphics.SetShader(data.OldShader);
        if (data.OldBlendMode.HasValue)
            graphics.SetBlendMode(data.OldBlendMode.Value);
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
        result.ApplyDeclaredMargin();
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
        parent.Clone(options);
        return result;
    }

    private static void DeepCloneComponents(UIElement clone)
    {
        var components = clone._components;
        clone._components = new ValueList<IUIComponent>(components.Count);
        foreach (var component in components)
            component.Detach(clone);
        foreach (var component in components)
            clone.Attach(Cloner.CloneOrSelf(component));
    }

    private void MarkReady()
    {
        foreach (var element in this.DescendantsAndSelf())
            element.IsLayoutReady = true;
    }

    private readonly struct WrapMinSizeState
    {
        public required UIContainer Container { get; init; }
        public required Unit MinWidth { get; init; }
        public required Unit MinHeight { get; init; }
        public required Unit Width { get; init; }
        public required Unit Height { get; init; }
    }

    private struct RenderData
    {
        public Matrix3x2? OldMatrix;
        public Box? OldClip;
        public BlendMode? OldBlendMode;
        public Shader? OldShader;
        public bool? OldCulling;
        public bool OverflowHidden;
        public readonly bool ShouldRender;
        public RenderPhase Phase;

        public RenderData(UIElement element)
        {
            ShouldRender = element.IsLayoutReady && element.Display != DisplayMode.None;
        }
    }

    private enum RenderPhase : byte
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
            count = Origin is UIParent parent ? parent.Children().Count : 0;
            return true;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            hasChild = Origin is UIParent parent && parent.Children().Count > 0;
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

                _enumerator = parent.Children().Deferred(_deferred).GetEnumerator();
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
                _enumerator = parent.Children().Deferred(_deferred).GetEnumerator();
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
                _enumerator = parent.Children().Deferred(_deferred).GetEnumerator();
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
        public Action<T> With
        {
            set => value.Invoke(element);
        }

        public Action<T> OnUpdate
        {
            set => element.OnUpdateSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnDisabledUpdate
        {
            set => element.OnDisabledUpdateSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnDirty
        {
            set => element.OnDirtySignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnMouseEnter
        {
            set => element.OnMouseEnterSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnMouseLeave
        {
            set => element.OnMouseLeaveSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnClick
        {
            set => element.OnClickSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnPress
        {
            set => element.OnPressSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnRelease
        {
            set => element.OnReleaseSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<T> OnClone
        {
            set => element.OnCloneSignal.Subscribe(e => value.Invoke((T)e));
        }

        public Action<UIElement, Graphics, CameraProvider> OnBeginRender
        {
            set => element.OnBeginRenderSignal.Subscribe((e, graphics, camera) => value.Invoke((T)e, graphics, camera));
        }

        public Action<UIElement, Graphics, CameraProvider> OnRender
        {
            set => element.OnRenderSignal.Subscribe((e, graphics, camera) => value.Invoke((T)e, graphics, camera));
        }

        public Action<UIElement, Graphics, CameraProvider> OnEndRender
        {
            set => element.OnEndRenderSignal.Subscribe((e, graphics, camera) => value.Invoke((T)e, graphics, camera));
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
