using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.FlexLayout;
using Display = Vigilance.FlexLayout.Display;

namespace Vigilance.UI;

public sealed class UINode : Node<UIElement.NodeStorage>
{
    internal UINode(UIElement element)
        : base(new UIElement.NodeStorage(element)) { }
}

public abstract class UIElement : IFullCloneable
{
    [Flags]
    public enum CloneOptions : byte
    {
        None = 0,
        SkipChildren = 1 << 0,
        ClearSignals = 1 << 1,
        DeepDefaults = None,
        ShallowDefaults = SkipChildren,
    }

    internal ValueDictionary<ImmediateCounter, uint> ImmediateCounters = new(ImmediateCounterComparer.Instance);
    internal ValueDictionary<ImmediateEntry, ImmediateValue> ImmediateEntries = new(ImmediateEntryComparer.Instance);
    internal UINode Node;
    private bool _click;
    private ValueList<IUIComponent> _components = [];
    private uint _immediateGeneration;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onBeginRenderHandlers;
    private Func<UIElement, bool>? _onClickHandlers;
    private Func<UIElement, bool>? _onCloneHandlers;
    private Func<UIElement, bool>? _onDirtyHandlers;
    private Func<UIElement, bool>? _onDisabledUpdateHandlers;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onEndRenderHandlers;
    private Func<UIElement, bool>? _onImmediateHandlers;
    private Func<UIElement, bool>? _onLayoutHandlers;
    private Func<UIElement, bool>? _onMouseEnterHandlers;
    private Func<UIElement, bool>? _onMouseExitHandlers;
    private Func<UIElement, bool>? _onPressHandlers;
    private Func<UIElement, bool>? _onReleaseHandlers;
    private Func<UIElement, Graphics, CameraProvider, bool>? _onRenderHandlers;
    private Func<UIElement, bool>? _onResetLayoutAndTransformHandlers;
    private Func<UIElement, bool>? _onUpdateHandlers;
    private RenderData _renderData;

    protected UIElement()
    {
        Node = new UINode(this);
        Node.StyleSetAlignItems(FlexLayout.Align.Start);
        var onImmediate = OnImmediate;
        IsImmediate = onImmediate.Method.DeclaringType != typeof(UIElement);
        var measure = Measure;
        IsLayoutCustom = this is not UIContainer && measure.Method.DeclaringType != typeof(UIElement);
        if (IsLayoutCustom)
            Node.SetMeasureFunc(
                (_, width, widthMode, height, heightMode) =>
                {
                    Vector2 size;
                    try
                    {
                        size = Measure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        size = Vector2.NaN;
                    }

                    return new Size(size.X, size.Y);
                }
            );
    }

    public ReadOnlySpan<IUIComponent> Components
    {
        get => _components.AsSpan();
        init
        {
            _components.AddRange(value);
            foreach (var component in value)
                component.Attach(this);
        }
    }

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

    public bool IsImmediate => field || _onImmediateHandlers is not null;
    public bool IsPressed { get; private set; }

    public bool IsClicked { get; private set; }
    public bool IsReleased { get; private set; }

    public bool IsMouseEntered { get; private set; }

    public bool IsMouseExited { get; private set; }

    public bool IsDirty => Node.IsDirty;

    public int ZIndex { get; set; }

    public BlendMode? BlendMode { get; set; } = null;

    public Shader? Shader { get; set; } = null;

    public ShapeTexture? ShapeTexture { get; set; } = null;

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

    public UIElement Root
    {
        get
        {
            var element = this;
            while (element.Parent is not null)
                element = element.Parent;
            return element;
        }
    }

    public bool IsDisabled { get; set; }

    public bool IsPersistent { get; set; }

    public bool IsVisible
    {
        get
        {
            foreach (var element in this.AncestorsAndSelf())
                if (element is not { IsLayoutReady: true, Hidden: false, WasRenderedOutside: false })
                    return false;
            return true;
        }
    }

    public bool Hidden
    {
        get => Node.StyleGetDisplay() == Display.None;
        set => Node.StyleSetDisplay(value ? Display.None : Display.Flex);
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

    public Unit PaddingY
    {
        set
        {
            PaddingTop = value;
            PaddingBottom = value;
        }
    }

    public Unit PaddingX
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

    public Unit MarginY
    {
        set
        {
            MarginTop = value;
            MarginBottom = value;
        }
    }

    public Unit MarginX
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
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Edge.Top,
                (node, e) => node.StyleSetMarginAuto(e),
                (node, e, margin) => node.StyleSetMargin(e, margin),
                (node, e, margin) => node.StyleSetMarginPercent(e, margin)
            );
        }
    }

    public Unit MarginRight
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Right));
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Edge.Right,
                (node, e) => node.StyleSetMarginAuto(e),
                (node, e, margin) => node.StyleSetMargin(e, margin),
                (node, e, margin) => node.StyleSetMarginPercent(e, margin)
            );
        }
    }

    public Unit MarginBottom
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Bottom));
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Edge.Bottom,
                (node, e) => node.StyleSetMarginAuto(e),
                (node, e, margin) => node.StyleSetMargin(e, margin),
                (node, e, margin) => node.StyleSetMarginPercent(e, margin)
            );
        }
    }

    public Unit MarginLeft
    {
        get => Unit.FromValue(Node.StyleGetMargin(Edge.Left));
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Edge.Left,
                (node, e) => node.StyleSetMarginAuto(e),
                (node, e, margin) => node.StyleSetMargin(e, margin),
                (node, e, margin) => node.StyleSetMarginPercent(e, margin)
            );
        }
    }

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
        set => Node.StyleSetAlignSelf((FlexLayout.Align)value);
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
        set => Node.StyleSetPositionType((FlexLayout.PositionType)value);
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

    public Signal<UIElement> OnImmediateSignal => new(ref _onImmediateHandlers);

    public Signal<UIElement> OnUpdateSignal => new(ref _onUpdateHandlers);

    public Signal<UIElement> OnDisabledUpdateSignal => new(ref _onDisabledUpdateHandlers);

    public Signal<UIElement> OnDirtySignal => new(ref _onDirtyHandlers);

    public Signal<UIElement> OnLayoutSignal => new(ref _onLayoutHandlers);

    public Signal<UIElement> OnMouseEnterSignal => new(ref _onMouseEnterHandlers);

    public Signal<UIElement> OnMouseExitSignal => new(ref _onMouseExitHandlers);

    public Signal<UIElement> OnClickSignal => new(ref _onClickHandlers);

    public Signal<UIElement> OnPressSignal => new(ref _onPressHandlers);

    public Signal<UIElement> OnReleaseSignal => new(ref _onReleaseHandlers);

    public Signal<UIElement> OnCloneSignal => new(ref _onCloneHandlers);

    public Signal<UIElement> OnResetLayoutAndTransformSignal => new(ref _onResetLayoutAndTransformHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnBeginRenderSignal => new(ref _onBeginRenderHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnRenderSignal => new(ref _onRenderHandlers);

    public Signal<UIElement, Graphics, CameraProvider> OnEndRenderSignal => new(ref _onEndRenderHandlers);

    object IDeepCloneable.DeepClone()
    {
        return DeepClone(CloneOptions.DeepDefaults);
    }

    object IShallowCloneable.ShallowClone()
    {
        return ShallowClone(CloneOptions.ShallowDefaults);
    }

    internal object DeepClone(CloneOptions options)
    {
        if ((options & CloneOptions.SkipChildren) != 0)
        {
            var self = Clone(this, options);
            DeepCloneComponents(self);
            if ((options & CloneOptions.ClearSignals) != 0)
                self.ClearSignals();
            try
            {
                self.OnClone();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            OnCloneSignal.SafeInvoke(self);
            return self;
        }

        var cloneMap = new ValueDictionary<UIElement, UIElement>(this.DescendantsAndSelf().Count());
        foreach (var node in this.DescendantsPostOrderAndSelf())
        {
            var clone = Clone(node, options);
            DeepCloneComponents(clone);
            if (clone is UIParent parent)
                foreach (var child in node.Children())
                    parent.Add(cloneMap[child]);
            if ((options & CloneOptions.ClearSignals) != 0)
                clone.ClearSignals();
            try
            {
                clone.OnClone();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            OnCloneSignal.SafeInvoke(clone);
            cloneMap[node] = clone;
        }

        return cloneMap[this];
    }

    internal object ShallowClone(CloneOptions options)
    {
        var clone = Clone(this, options);
        clone._components = clone._components.ToValueList();
        if ((options & CloneOptions.SkipChildren) == 0 && clone is UIParent parent)
            foreach (var child in this.Children())
                parent.Add(child);
        if ((options & CloneOptions.ClearSignals) != 0)
            clone.ClearSignals();
        try
        {
            clone.OnClone();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        OnCloneSignal.SafeInvoke(clone);
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
        Flex.CalculateLayout(Node, width, height, FlexLayout.Direction.LeftToRight);
        foreach (var element in this.DescendantsPostOrderAndSelf())
        {
            try
            {
                element.OnLayout();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnLayoutSignal.SafeInvoke(element);
        }
    }

    public void Attach(IUIComponent component)
    {
        _components.Add(component);
        try
        {
            component.Attach(this);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public void Detach(IUIComponent component)
    {
        _components.Remove(component);
        try
        {
            component.Detach(this);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public void DetachAll(bool keepPersistent = false)
    {
        using var pool = keepPersistent
            ? _components.AsValueEnumerable().Where(component => !component.IsPersistant).ToArrayPool()
            : _components.AsValueEnumerable().ToArrayPool();
        if (keepPersistent)
            _components.RemoveAll(component => !component.IsPersistant);
        else
            _components.Clear();
        foreach (var component in pool.Span)
            try
            {
                component.Detach(this);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
    }

    public T Immediate<T>(object? key, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        where T : new()
    {
        return Immediate<T>(new ImmediateKey(key), file, line);
    }

    [OverloadResolutionPriority(1)]
    public T Immediate<T>(ImmediateKey? key = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        where T : new()
    {
        return Immediate(() => new T(), key, file, line);
    }

    public T Immediate<T>(
        Func<T> factory,
        object? key,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    )
    {
        return Immediate(factory, new ImmediateKey(key), file, line);
    }

    [OverloadResolutionPriority(1)]
    public T Immediate<T>(
        Func<T> factory,
        ImmediateKey? key = null,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    )
    {
        Debug.Assert(string.IsInterned(file) is not null);
        ImmediateKey keyValue;
        if (key.HasValue)
        {
            keyValue = key.Value;
        }
        else
        {
            ref var counter = ref ImmediateCounters.GetValueRefOrAddDefault(
                new ImmediateCounter(typeof(T), file, line),
                out _
            );
            keyValue = ImmediateKey.FromCounter(counter++);
        }

        ref var entryRef = ref ImmediateEntries.GetValueRefOrAddDefault(
            new ImmediateEntry(keyValue, typeof(T), file, line),
            out var exists
        );
        if (!exists)
        {
            var newEntry = factory.SafeInvoke()!;
            entryRef.Value = newEntry;
            if (newEntry is UIParent { IsPersistent: true } parent)
                foreach (var child in parent.Children())
                    child.IsPersistent = true;
        }

        entryRef.Generation = _immediateGeneration;
        var entry = (T)entryRef.Value;
        switch (entry)
        {
            case UIElement element:
            {
                if (!element.IsImmediate)
                {
                    element.ImmediateCounters.Clear();
                    element.DetachAll(keepPersistent: true);
                    if (element is UIParent parent)
                    {
                        if (ReconcileSession.Current is { } session)
                            parent.BeginReconcile(session);
                        else
                            parent.Clear(keepPersistent: true);
                    }
                }

                (this as UIParent)?.Add(element);
                break;
            }
            case IUIComponent component:
                Attach(component);
                break;
        }

        return entry;
    }

    public void Update()
    {
        Update(Entity.Null);
    }

    public void Update(in Entity entity)
    {
        Entity = entity;
        if (!IsImmediate && !IsLayoutReady)
            return;
        foreach (var element in this.DescendantsPostOrderAndSelf())
            Update(element, entity);
        foreach (var element in this.DescendantsPostOrderAndSelf())
            element.DispatchDirty();
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
        try
        {
            OnResetLayoutAndTransform();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        OnResetLayoutAndTransformSignal.SafeInvoke(this);
    }

    public void ClearSignals()
    {
        OnImmediateSignal.Clear();
        OnUpdateSignal.Clear();
        OnDisabledUpdateSignal.Clear();
        OnDirtySignal.Clear();
        OnLayoutSignal.Clear();
        OnMouseEnterSignal.Clear();
        OnMouseExitSignal.Clear();
        OnClickSignal.Clear();
        OnPressSignal.Clear();
        OnReleaseSignal.Clear();
        OnCloneSignal.Clear();
        OnResetLayoutAndTransformSignal.Clear();
        OnBeginRenderSignal.Clear();
        OnRenderSignal.Clear();
        OnEndRenderSignal.Clear();
        try
        {
            OnClearSignals();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }

    public RenderTexture ToTexture(Vector2 size, bool pool = true)
    {
        return ToTexture(size.X, size.Y, pool);
    }

    public RenderTexture ToTexture(float width = float.NaN, float height = float.NaN, bool pool = true)
    {
        CalculateLayout(width, height);
        var texture = new RenderTexture(
            new Vector2(float.IsNaN(width) ? LayoutWidth : width, float.IsNaN(height) ? LayoutHeight : height),
            pool: pool
        );
        Render(texture.Graphics);
        return texture;
    }

    protected virtual void OnImmediate() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnDisabledUpdate() { }

    protected virtual void OnDirty() { }

    protected virtual void OnLayout() { }

    protected virtual void OnMouseEnter() { }

    protected virtual void OnMouseExit() { }

    protected virtual void OnClick() { }

    protected virtual void OnPress() { }

    protected virtual void OnRelease() { }

    protected virtual void OnClone() { }

    protected virtual void OnResetLayoutAndTransform() { }

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

    private static void Update(UIElement element, in Entity entity)
    {
        element.Entity = entity;
        if (!element.IsLayoutReady)
        {
            if (element.IsImmediate)
                element.RunImmediate();
            return;
        }

        var oldMouseInside = element.IsMouseInside;
        element.IsMouseInside =
            element.RenderedGraphics == Renderer.Graphics
            && Mouse.OnScreen
            && element.IsVisible
            && Collision.CheckPointQuad(Mouse.Position, element.RenderedBounds);
        element.IsMouseEntered = !oldMouseInside && element.IsMouseInside;
        element.IsMouseExited = oldMouseInside && !element.IsMouseInside;
        var pressed = Mouse.IsButtonPressed(MouseButton.Left);
        var released = Mouse.IsButtonReleased(MouseButton.Left);
        element.IsPressed = pressed && element.IsMouseInside;
        if (pressed)
            element._click = element.IsMouseInside;
        element.IsClicked = released && element is { _click: true, IsMouseInside: true };
        element.IsReleased = released && element.IsMouseInside;
        if (released)
            element._click = false;
        var disabled = element.IsDisabled || entity is { IsNull: false, IsDisabled: true };
        if (disabled)
        {
            try
            {
                element.OnDisabledUpdate();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnDisabledUpdateSignal.SafeInvoke(element);
            return;
        }

        if (element.IsImmediate)
            element.RunImmediate();
        try
        {
            element.OnUpdate();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        element.OnUpdateSignal.SafeInvoke(element);
        if (element.IsMouseEntered)
        {
            try
            {
                element.OnMouseEnter();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnMouseEnterSignal.SafeInvoke(element);
        }

        if (element.IsMouseExited)
        {
            try
            {
                element.OnMouseExit();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnMouseExitSignal.SafeInvoke(element);
        }

        if (element.IsPressed)
        {
            try
            {
                element.OnPress();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnPressSignal.SafeInvoke(element);
        }

        if (element.IsClicked)
        {
            try
            {
                element.OnClick();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnClickSignal.SafeInvoke(element);
        }

        if (element.IsReleased)
        {
            try
            {
                element.OnRelease();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            element.OnReleaseSignal.SafeInvoke(element);
        }
    }

    private void RunImmediate()
    {
        ImmediateCounters.Clear();
        foreach (var element in this.DescendantsAndSelf())
            element._immediateGeneration++;
        DetachAll(keepPersistent: true);
        var session = ReconcileSession.Begin();
        try
        {
            if (this is UIParent parent)
                parent.BeginReconcile(session);
            try
            {
                OnImmediate();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            OnImmediateSignal.SafeInvoke(this);
        }
        finally
        {
            session.End();
        }

        foreach (var element in this.DescendantsAndSelf())
        {
            if (element.ImmediateEntries.Count == 0)
                continue;
            var generation = element._immediateGeneration;
            foreach (
                var entry in element
                    .ImmediateEntries.AsValueEnumerable()
                    .Cross(generation.AsValueSingleton())
                    .Where(cross => cross.Left.Value.Generation != cross.Right)
                    .Select(cross => cross.Left.Key)
                    .AsPooled()
            )
                element.ImmediateEntries.Remove(entry);
        }
    }

    private void DispatchDirty()
    {
        if (!IsLayoutReady || !IsDirty)
            return;
        try
        {
            OnDirty();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        OnDirtySignal.SafeInvoke(this);
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

        if (element.ShapeTexture is not null)
        {
            data.OldShapesTexture = graphics.GetShapeTexture();
            graphics.SetShapeTexture(element.ShapeTexture);
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
        {
            var span = stack.AsSpan(i, count - i);
            span.AsValueEnumerable().OrderByDescending(e => e.ZIndex).CopyTo(span);
        }

        try
        {
            element.OnBeginRender(graphics, camera);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        element.OnBeginRenderSignal.SafeInvoke(element, graphics, camera);
        try
        {
            element.OnRender(graphics, camera);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        element.OnRenderSignal.SafeInvoke(element, graphics, camera);
    }

    private static void EndRender(UIElement element, Graphics graphics, CameraProvider camera)
    {
        ref var data = ref element._renderData;
        if (!data.ShouldRender)
            return;
        element.OnEndRenderSignal.SafeInvoke(element, graphics, camera);
        try
        {
            element.OnEndRender(graphics, camera);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        if (data.OldCulling.HasValue)
            graphics.SetCulling(data.OldCulling.Value);
        if (element.ShapeTexture is not null)
            graphics.SetShapeTexture(data.OldShapesTexture);
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
        result._immediateGeneration = 0;
        result.IsLayoutReady = false;
        result.Parent = null;
        result.Node = new UINode(result);
        result.ImmediateEntries = new ValueDictionary<ImmediateEntry, ImmediateValue>(ImmediateEntryComparer.Instance);
        result.ImmediateCounters = new ValueDictionary<ImmediateCounter, uint>(ImmediateCounterComparer.Instance);
        Flex.NodeCopyStyle(result.Node, element.Node);
        if (element.IsLayoutCustom)
            result.Node.SetMeasureFunc(
                (_, width, widthMode, height, heightMode) =>
                {
                    Vector2 size;
                    try
                    {
                        size = result.Measure(width, (MeasureMode)widthMode, height, (MeasureMode)heightMode);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        size = Vector2.NaN;
                    }

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
            try
            {
                component.Detach(clone);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        foreach (var component in components)
            clone.Attach(Cloner.CloneOrSelf(component));
    }

    private void MarkReady()
    {
        foreach (var element in this.DescendantsAndSelf())
            element.IsLayoutReady = true;
    }

    public readonly struct NodeStorage : IList<Node<NodeStorage>>
    {
        public UIElement Element { get; }

        internal NodeStorage(UIElement element)
        {
            Element = element;
        }

        IEnumerator<Node<NodeStorage>> IEnumerable<Node<NodeStorage>>.GetEnumerator()
        {
            if (Element is not UIParent { IsLayoutCustom: false } parent)
                yield break;
            foreach (var child in new UIParent.ChildEnumerable(parent))
                yield return child.Node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Node<NodeStorage>>)this).GetEnumerator();
        }

        void ICollection<Node<NodeStorage>>.Add(Node<NodeStorage> item)
        {
            if (Element is not UIParent { IsLayoutCustom: false } parent)
                throw new NotSupportedException();
            parent.Add(item.Storage.Element);
        }

        void ICollection<Node<NodeStorage>>.Clear()
        {
            if (Element is not UIParent { IsLayoutCustom: false } parent)
                throw new NotSupportedException();
            parent.Clear();
        }

        bool ICollection<Node<NodeStorage>>.Contains(Node<NodeStorage> item)
        {
            if (Element is UIParent { IsLayoutCustom: false } parent)
                return parent.Children().Contains(item.Storage.Element);
            return false;
        }

        void ICollection<Node<NodeStorage>>.CopyTo(Node<NodeStorage>[] array, int arrayIndex)
        {
            if (Element is UIParent { IsLayoutCustom: false } parent)
                parent.Children().Select(Node<NodeStorage> (p) => p.Node).CopyTo(array.AsSpan(arrayIndex));
        }

        bool ICollection<Node<NodeStorage>>.Remove(Node<NodeStorage> item)
        {
            return Element is not UIParent { IsLayoutCustom: false } parent
                ? throw new NotSupportedException()
                : parent.Remove(item.Storage.Element);
        }

        int ICollection<Node<NodeStorage>>.Count =>
            Element is not UIParent { IsLayoutCustom: false } parent ? 0 : parent.ChildrenCount;

        bool ICollection<Node<NodeStorage>>.IsReadOnly => Element is not UIParent { IsLayoutCustom: false };

        int IList<Node<NodeStorage>>.IndexOf(Node<NodeStorage> item)
        {
            if (Element is UIParent { IsLayoutCustom: false } parent)
                return parent.IndexOf(item.Storage.Element);
            return -1;
        }

        void IList<Node<NodeStorage>>.Insert(int index, Node<NodeStorage> item)
        {
            if (Element is not UIParent { IsLayoutCustom: false } parent)
                throw new NotSupportedException();
            parent.Insert(index, item.Storage.Element);
        }

        void IList<Node<NodeStorage>>.RemoveAt(int index)
        {
            if (Element is not UIParent { IsLayoutCustom: false } parent)
                throw new NotSupportedException();
            parent[index].Remove();
        }

        Node<NodeStorage> IList<Node<NodeStorage>>.this[int index]
        {
            get
            {
                if (Element is UIParent { IsLayoutCustom: false } parent)
                    return parent[index].Node;
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set
            {
                if (Element is not UIParent { IsLayoutCustom: false } parent)
                    throw new NotSupportedException();
                if (index < parent.ChildrenCount)
                    parent[index].Remove();
                parent.Insert(index, value.Storage.Element);
            }
        }
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public readonly record struct ImmediateKey
    {
        private readonly long _numericKey;
        private readonly object? _objectKey;

        public ImmediateKey(long key = 0)
        {
            _numericKey = key;
        }

        public ImmediateKey(object? key)
        {
            _objectKey = key;
        }

        private uint CounterKey { get; init; }

        internal static ImmediateKey FromCounter(uint count)
        {
            return new ImmediateKey { CounterKey = count };
        }

        public static implicit operator ImmediateKey(string? key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(char key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(bool key)
        {
            return new ImmediateKey(key ? 1 : 0);
        }

        public static implicit operator ImmediateKey(sbyte key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(short key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(int key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(long key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(byte key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(ushort key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(uint key)
        {
            return new ImmediateKey(key);
        }

        public static implicit operator ImmediateKey(ulong key)
        {
            return new ImmediateKey((long)key);
        }

        public static implicit operator ImmediateKey(float key)
        {
            return new ImmediateKey(BitConverter.DoubleToInt64Bits(key));
        }

        public static implicit operator ImmediateKey(double key)
        {
            return new ImmediateKey(BitConverter.DoubleToInt64Bits(key));
        }
    }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
    internal readonly record struct ImmediateEntry(ImmediateKey Key, Type Type, string File, int Line);

    internal struct ImmediateValue
    {
        public object Value;
        public uint Generation;
    }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
    internal readonly record struct ImmediateCounter(Type Type, string File, int Line);

    private sealed class ImmediateEntryComparer : IEqualityComparer<ImmediateEntry>
    {
        public static readonly ImmediateEntryComparer Instance = new();

        public bool Equals(ImmediateEntry x, ImmediateEntry y)
        {
            return x.Type == y.Type && x.Key == y.Key && ReferenceEquals(x.File, y.File) && x.Line == y.Line;
        }

        public int GetHashCode(ImmediateEntry obj)
        {
            return HashCode.Combine(obj.Key, obj.Type, RuntimeHelpers.GetHashCode(obj.File), obj.Line);
        }
    }

    private sealed class ImmediateCounterComparer : IEqualityComparer<ImmediateCounter>
    {
        public static readonly ImmediateCounterComparer Instance = new();

        public bool Equals(ImmediateCounter x, ImmediateCounter y)
        {
            return x.Type == y.Type && ReferenceEquals(x.File, y.File) && x.Line == y.Line;
        }

        public int GetHashCode(ImmediateCounter obj)
        {
            return HashCode.Combine(obj.Type, RuntimeHelpers.GetHashCode(obj.File), obj.Line);
        }
    }

    private struct RenderData
    {
        public Matrix3x2? OldMatrix;
        public Box? OldClip;
        public BlendMode? OldBlendMode;
        public Shader? OldShader;
        public ShapeTexture? OldShapesTexture;
        public bool? OldCulling;
        public bool OverflowHidden;
        public readonly bool ShouldRender;
        public RenderPhase Phase;

        public RenderData(UIElement element)
        {
            ShouldRender = element is { IsLayoutReady: true, Hidden: false };
        }
    }

    private enum RenderPhase : sbyte
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
            count = Origin is UIParent parent ? parent.ChildrenCount : 0;
            return true;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            hasChild = Origin is UIParent { ChildrenCount: > 0 };
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

                _enumerator = new UIParent.ChildEnumerable(parent).Deferred(_deferred).GetEnumerator();
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
                _enumerator = new UIParent.ChildEnumerable(parent).Deferred(_deferred).GetEnumerator();
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
                _enumerator = new UIParent.ChildEnumerable(parent).Deferred(_deferred).GetEnumerator();
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

        public Action<T> OnImmediate
        {
            set => element.OnImmediateSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnUpdate
        {
            set => element.OnUpdateSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnDisabledUpdate
        {
            set => element.OnDisabledUpdateSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnDirty
        {
            set => element.OnDirtySignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnLayout
        {
            set => element.OnLayoutSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnMouseEnter
        {
            set => element.OnMouseEnterSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnMouseExit
        {
            set => element.OnMouseExitSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnClick
        {
            set => element.OnClickSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnPress
        {
            set => element.OnPressSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnRelease
        {
            set => element.OnReleaseSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnClone
        {
            set => element.OnCloneSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<T> OnResetLayoutAndTransform
        {
            set => element.OnResetLayoutAndTransformSignal.Subscribe(e => value.Invoke(Unsafe.As<UIElement, T>(ref e)));
        }

        public Action<UIElement, Graphics, CameraProvider> OnBeginRender
        {
            set =>
                element.OnBeginRenderSignal.Subscribe(
                    (e, graphics, camera) => value.Invoke(Unsafe.As<UIElement, T>(ref e), graphics, camera)
                );
        }

        public Action<UIElement, Graphics, CameraProvider> OnRender
        {
            set =>
                element.OnRenderSignal.Subscribe(
                    (e, graphics, camera) => value.Invoke(Unsafe.As<UIElement, T>(ref e), graphics, camera)
                );
        }

        public Action<UIElement, Graphics, CameraProvider> OnEndRender
        {
            set =>
                element.OnEndRenderSignal.Subscribe(
                    (e, graphics, camera) => value.Invoke(Unsafe.As<UIElement, T>(ref e), graphics, camera)
                );
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
