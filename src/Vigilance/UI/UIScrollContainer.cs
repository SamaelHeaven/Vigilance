using System.ComponentModel;

namespace Vigilance.UI;

public class UIScrollContainer : UIContainer
{
    private bool _layout = false;
    private Rectangle _scrollBarThumbRectangle = new();
    private Rectangle _scrollBarTrackRectangle = new();
    private float? _thumbMouseDownX = null;
    private float? _thumbMouseDownY = null;

    public UIScrollContainer()
    {
        Overflow = Overflow.Hidden;
        ScrollBarTrackFill = Color.Neutral700;
        ScrollBarThumbFill = Color.Neutral400;
    }

    public Vector2 ChildrenLayoutSize { get; private set; }

    public Vector2 ScrollOffset { get; private set; }

    public Quad RenderedHorizontalScrollBarTrackBounds { get; private set; }

    public Quad RenderedVerticalScrollBarTrackBounds { get; private set; }

    public Quad RenderedHorizontalScrollBarThumbBounds { get; private set; }

    public Quad RenderedVerticalScrollBarThumbBounds { get; private set; }

    public Vector2 MouseScrollForce { get; set; } = new(15);

    public Vector2 ScrollBarTrackSize { get; set; } = new(16);

    public Vector2 ScrollBarThumbSize { get; set; } = new(16);

    public Vector2 ScrollBarSize
    {
        get => ScrollBarTrackSize.Max(ScrollBarThumbSize);
        set
        {
            ScrollBarTrackSize = value;
            ScrollBarThumbSize = value;
        }
    }

    public Insets ScrollBarTrackMargin { get; set; }

    public Unit ScrollBarTrackMarginX
    {
        set
        {
            ScrollBarTrackMarginLeft = value;
            ScrollBarTrackMarginRight = value;
        }
    }

    public Unit ScrollBarTrackMarginY
    {
        set
        {
            ScrollBarTrackMarginTop = value;
            ScrollBarTrackMarginBottom = value;
        }
    }

    public Unit ScrollBarTrackMarginTop
    {
        get => ScrollBarTrackMargin.Top;
        set
        {
            var margin = ScrollBarTrackMargin;
            margin.Top = value;
            ScrollBarTrackMargin = margin;
        }
    }

    public Unit ScrollBarTrackMarginBottom
    {
        get => ScrollBarTrackMargin.Bottom;
        set
        {
            var margin = ScrollBarTrackMargin;
            margin.Bottom = value;
            ScrollBarTrackMargin = margin;
        }
    }

    public Unit ScrollBarTrackMarginLeft
    {
        get => ScrollBarTrackMargin.Left;
        set
        {
            var margin = ScrollBarTrackMargin;
            margin.Left = value;
            ScrollBarTrackMargin = margin;
        }
    }

    public Unit ScrollBarTrackMarginRight
    {
        get => ScrollBarTrackMargin.Right;
        set
        {
            var margin = ScrollBarTrackMargin;
            margin.Right = value;
            ScrollBarTrackMargin = margin;
        }
    }

    public Insets ScrollBarThumbMargin { get; set; } =
        new()
        {
            Top = 1,
            Bottom = 1,
            Left = 2,
            Right = 2,
        };

    public Unit ScrollBarThumbMarginX
    {
        set
        {
            ScrollBarThumbMarginLeft = value;
            ScrollBarThumbMarginRight = value;
        }
    }

    public Unit ScrollBarThumbMarginY
    {
        set
        {
            ScrollBarThumbMarginTop = value;
            ScrollBarThumbMarginBottom = value;
        }
    }

    public Unit ScrollBarThumbMarginTop
    {
        get => ScrollBarThumbMargin.Top;
        set
        {
            var margin = ScrollBarThumbMargin;
            margin.Top = value;
            ScrollBarThumbMargin = margin;
        }
    }

    public Unit ScrollBarThumbMarginBottom
    {
        get => ScrollBarThumbMargin.Bottom;
        set
        {
            var margin = ScrollBarThumbMargin;
            margin.Bottom = value;
            ScrollBarThumbMargin = margin;
        }
    }

    public Unit ScrollBarThumbMarginLeft
    {
        get => ScrollBarThumbMargin.Left;
        set
        {
            var margin = ScrollBarThumbMargin;
            margin.Left = value;
            ScrollBarThumbMargin = margin;
        }
    }

    public Unit ScrollBarThumbMarginRight
    {
        get => ScrollBarThumbMargin.Right;
        set
        {
            var margin = ScrollBarThumbMargin;
            margin.Right = value;
            ScrollBarThumbMargin = margin;
        }
    }

    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }

    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }

    public ScrollBarVisibility ScrollBarVisibility
    {
        set
        {
            HorizontalScrollBarVisibility = value;
            VerticalScrollBarVisibility = value;
        }
    }

    public bool IsHorizontalScrollBarVisible
    {
        get
        {
            return HorizontalScrollBarVisibility switch
            {
                ScrollBarVisibility.Visible => true,
                ScrollBarVisibility.Hidden => false,
                _ => ChildrenLayoutSize.X > LayoutSize.X,
            };
        }
    }

    public bool IsVerticalScrollBarVisible
    {
        get
        {
            return VerticalScrollBarVisibility switch
            {
                ScrollBarVisibility.Visible => true,
                ScrollBarVisibility.Hidden => false,
                _ => ChildrenLayoutSize.Y > LayoutSize.Y,
            };
        }
    }

    public Color ScrollBarTrackFill
    {
        get => _scrollBarTrackRectangle.Fill;
        set => _scrollBarTrackRectangle.Fill = value;
    }

    public Color ScrollBarTrackStroke
    {
        get => _scrollBarTrackRectangle.Stroke;
        set => _scrollBarTrackRectangle.Stroke = value;
    }

    public float ScrollBarTrackStrokeWidth
    {
        get => _scrollBarTrackRectangle.StrokeWidth;
        set => _scrollBarTrackRectangle.StrokeWidth = value;
    }

    public Unit ScrollBarTrackRadius { get; set; }

    public int ScrollBarTrackSegments
    {
        get => _scrollBarTrackRectangle.Segments;
        set => _scrollBarTrackRectangle.Segments = value;
    }

    public DrawOrder ScrollBarTrackDrawOrder
    {
        get => _scrollBarTrackRectangle.DrawOrder;
        set => _scrollBarTrackRectangle.DrawOrder = value;
    }

    public Color ScrollBarThumbFill
    {
        get => _scrollBarThumbRectangle.Fill;
        set => _scrollBarThumbRectangle.Fill = value;
    }

    public Color ScrollBarThumbStroke
    {
        get => _scrollBarThumbRectangle.Stroke;
        set => _scrollBarThumbRectangle.Stroke = value;
    }

    public float ScrollBarThumbStrokeWidth
    {
        get => _scrollBarThumbRectangle.StrokeWidth;
        set => _scrollBarThumbRectangle.StrokeWidth = value;
    }

    public Unit ScrollBarThumbRadius { get; set; } = Unit.Full;

    public int ScrollBarThumbSegments
    {
        get => _scrollBarThumbRectangle.Segments;
        set => _scrollBarThumbRectangle.Segments = value;
    }

    public DrawOrder ScrollBarThumbDrawOrder
    {
        get => _scrollBarThumbRectangle.DrawOrder;
        set => _scrollBarThumbRectangle.DrawOrder = value;
    }

    public bool IsMouseInsideNestedScrollContainer { get; set; }

    protected override void OnLayout()
    {
        if (_layout)
            return;
        _layout = true;
        var offset = Vector2.Zero;
        var size = Vector2.Zero;
        var direction = Direction;
        var camera = RenderedCamera;
        var localMousePosition = Mouse.Position;
        var worldMousePosition = camera is null
            ? localMousePosition
            : Coordinates.LocalToWorld(localMousePosition, camera);
        var mousePressed = Mouse.IsButtonPressed(MouseButton.Left);
        var mouseReleased = Mouse.IsButtonReleased(MouseButton.Left);
        foreach (var element in Children().Where(element => element.Position != PositionType.Absolute))
        {
            size.X = size.X.Max(element.LayoutLeft + element.LayoutWidth);
            size.Y = size.Y.Max(element.LayoutTop + element.LayoutHeight);
        }

        ChildrenLayoutSize = size;
        IsMouseInsideNestedScrollContainer = this.Descendants()
            .OfType<UIScrollContainer>()
            .Where(container =>
                container.IsMouseInside
                && (container.IsHorizontalScrollBarVisible || container.IsVerticalScrollBarVisible)
            )
            .Any();

        var scroll =
            IsMouseInside && !IsMouseInsideNestedScrollContainer ? Mouse.Scroll * MouseScrollForce : Vector2.Zero;
        if (_thumbMouseDownY.HasValue)
        {
            var deltaY = worldMousePosition.Y - _thumbMouseDownY.Value;
            var trackBox = GetScrollBarTrackBox(ScrollBarDirection.Vertical);
            var thumbBox = GetScrollBarThumbBox(ScrollBarDirection.Vertical);
            var scrollableHeight = ChildrenLayoutSize.Y - LayoutSize.Y;
            var trackMovableHeight = trackBox.Height - thumbBox.Height;
            if (trackMovableHeight > 0)
                scroll.Y = -(deltaY * scrollableHeight / trackMovableHeight);
            _thumbMouseDownY = worldMousePosition.Y;
        }

        if (_thumbMouseDownX.HasValue)
        {
            var deltaX = worldMousePosition.X - _thumbMouseDownX.Value;
            var trackBox = GetScrollBarTrackBox(ScrollBarDirection.Horizontal);
            var thumbBox = GetScrollBarThumbBox(ScrollBarDirection.Horizontal);
            var scrollableWidth = ChildrenLayoutSize.X - LayoutSize.X;
            var trackMovableWidth = trackBox.Width - thumbBox.Width;
            if (trackMovableWidth > 0)
                scroll.X = -(deltaX * scrollableWidth / trackMovableWidth);
            _thumbMouseDownX = worldMousePosition.X;
        }

        if (IsMouseInside && mousePressed)
        {
            if (Collision.CheckPointQuad(localMousePosition, RenderedHorizontalScrollBarThumbBounds))
                _thumbMouseDownX = worldMousePosition.X;
            if (Collision.CheckPointQuad(localMousePosition, RenderedVerticalScrollBarThumbBounds))
                _thumbMouseDownY = worldMousePosition.Y;
        }

        if (mouseReleased)
        {
            _thumbMouseDownX = null;
            _thumbMouseDownY = null;
        }

        var horizontalVisible = IsHorizontalScrollBarVisible;
        var verticalVisible = IsVerticalScrollBarVisible;
        var minSize = -(
            size
            - LayoutSize
            + new Vector2(verticalVisible ? ScrollBarSize.Y : 0, horizontalVisible ? ScrollBarSize.X : 0)
        );
        if (minSize.X < 0 && horizontalVisible)
            offset.X =
                direction == Direction.RightToLeft
                    ? (ScrollOffset.X + scroll.X).Clamp(0, -minSize.X)
                    : (ScrollOffset.X + scroll.X).Clamp(minSize.X, 0);
        if (minSize.Y < 0 && verticalVisible)
            offset.Y =
                direction == Direction.BottomToTop
                    ? (ScrollOffset.Y + scroll.Y).Clamp(0, -minSize.Y)
                    : (ScrollOffset.Y + scroll.Y).Clamp(minSize.Y, 0);
        ScrollOffset = offset;
    }

    public void ScrollTo(Vector2 offset)
    {
        ScrollOffset = -offset;
    }

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        graphics.PushMatrix();
        graphics.Translate(ScrollOffset);
    }

    protected override void OnEndRender(Graphics graphics, CameraProvider camera)
    {
        graphics.PopMatrix();
        Box box;
        var matrix = graphics.GetMatrix(camera);
        var horizontalVisible = IsHorizontalScrollBarVisible;
        var verticalVisible = IsVerticalScrollBarVisible;
        if (horizontalVisible)
        {
            box = GetScrollBarTrackBox(ScrollBarDirection.Horizontal);
            RenderScrollBarTrack(graphics, box, camera);
            RenderedHorizontalScrollBarTrackBounds = box.Transform(matrix);
        }

        if (verticalVisible)
        {
            box = GetScrollBarTrackBox(ScrollBarDirection.Vertical);
            RenderScrollBarTrack(graphics, box, camera);
            RenderedVerticalScrollBarTrackBounds = box.Transform(matrix);
        }

        if (horizontalVisible)
        {
            box = GetScrollBarThumbBox(ScrollBarDirection.Horizontal);
            RenderScrollBarThumb(graphics, box, camera);
            RenderedHorizontalScrollBarThumbBounds = box.Transform(matrix);
        }

        if (verticalVisible)
        {
            box = GetScrollBarThumbBox(ScrollBarDirection.Vertical);
            RenderScrollBarThumb(graphics, box, camera);
            RenderedVerticalScrollBarThumbBounds = box.Transform(matrix);
        }

        _layout = false;
    }

    protected override void OnClone()
    {
        _scrollBarTrackRectangle = _scrollBarTrackRectangle.DeepClone();
        _scrollBarThumbRectangle = _scrollBarThumbRectangle.DeepClone();
    }

    protected virtual void RenderScrollBarTrack(Graphics graphics, Box box, CameraProvider camera)
    {
        _scrollBarTrackRectangle.Camera = camera;
        _scrollBarTrackRectangle.Radius = ScrollBarTrackRadius.Calculate(box.Size.Abs().Min());
        graphics.DrawRectangle(box, _scrollBarTrackRectangle);
    }

    protected virtual void RenderScrollBarThumb(Graphics graphics, Box box, CameraProvider camera)
    {
        _scrollBarThumbRectangle.Camera = camera;
        _scrollBarThumbRectangle.Radius = ScrollBarThumbRadius.Calculate(box.Size.Abs().Min());
        graphics.DrawRectangle(box, _scrollBarThumbRectangle);
    }

    private Box GetScrollBarTrackBox(ScrollBarDirection direction)
    {
        var position = LayoutPosition;
        var trackSize = ScrollBarTrackSize;
        var barSize = ScrollBarSize;
        var size = LayoutSize;
        var isVertical = direction == ScrollBarDirection.Vertical;
        var marginTop = isVertical ? ScrollBarTrackMarginTop : ScrollBarTrackMarginLeft;
        var marginRight = isVertical ? ScrollBarTrackMarginRight : ScrollBarTrackMarginBottom;
        var marginBottom = isVertical ? ScrollBarTrackMarginBottom : ScrollBarTrackMarginRight;
        var marginLeft = isVertical ? ScrollBarTrackMarginLeft : ScrollBarTrackMarginTop;
        var topInset = marginTop.Calculate(size.Y).Max(0);
        var rightInset = marginRight.Calculate(size.X).Max(0);
        var bottomInset = marginBottom.Calculate(size.Y).Max(0);
        var leftInset = marginLeft.Calculate(size.X).Max(0);
        return direction switch
        {
            ScrollBarDirection.Horizontal => new Box(
                new Vector2(position.X + leftInset, position.Y + size.Y - (barSize.X + trackSize.X) / 2 + topInset),
                new Vector2(size.X - leftInset - rightInset, trackSize.X - topInset - bottomInset)
            ),
            ScrollBarDirection.Vertical => new Box(
                new Vector2(position.X + size.X - (barSize.Y + trackSize.Y) / 2 + leftInset, position.Y + topInset),
                new Vector2(trackSize.Y - leftInset - rightInset, size.Y - topInset - bottomInset)
            ),
            _ => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ScrollBarDirection)),
        };
    }

    private Box GetScrollBarThumbBox(ScrollBarDirection direction)
    {
        var contentSize = ChildrenLayoutSize;
        var position = LayoutPosition;
        var thumbThickness = ScrollBarThumbSize;
        var barSize = ScrollBarSize;
        var size = LayoutSize;
        var isVertical = direction == ScrollBarDirection.Vertical;
        var marginTop = isVertical ? ScrollBarThumbMarginTop : ScrollBarThumbMarginLeft;
        var marginRight = isVertical ? ScrollBarThumbMarginRight : ScrollBarThumbMarginBottom;
        var marginBottom = isVertical ? ScrollBarThumbMarginBottom : ScrollBarThumbMarginRight;
        var marginLeft = isVertical ? ScrollBarThumbMarginLeft : ScrollBarThumbMarginTop;
        var topInset = marginTop.Calculate(size.Y).Max(0);
        var rightInset = marginRight.Calculate(size.X).Max(0);
        var bottomInset = marginBottom.Calculate(size.Y).Max(0);
        var leftInset = marginLeft.Calculate(size.X).Max(0);
        var scroll = ScrollOffset;
        var thumbSize = Vector2.Zero;
        var thumbOffset = Vector2.Zero;
        var horizontalVisible = IsHorizontalScrollBarVisible;
        var verticalVisible = IsVerticalScrollBarVisible;

        {
            size.X = (size.X - (verticalVisible ? barSize.Y : 0)).Max(0);
            var visibleRatio = size.X / contentSize.X.Max(1f);
            thumbSize.X = size.X.Min(visibleRatio * size.X);
            var maxScroll = (contentSize.X - size.X + leftInset + rightInset).Max(1f);
            thumbOffset.X = -scroll.X / maxScroll * (size.X - thumbSize.X);
        }

        size = LayoutSize;
        {
            size.Y = (size.Y - (horizontalVisible ? barSize.X : 0)).Max(0);
            var visibleRatio = size.Y / contentSize.Y.Max(1f);
            thumbSize.Y = size.Y.Min(visibleRatio * size.Y);
            var maxScroll = (contentSize.Y - size.Y + topInset + bottomInset).Max(1f);
            thumbOffset.Y = -scroll.Y / maxScroll * (size.Y - thumbSize.Y);
        }

        size = LayoutSize;
        return direction switch
        {
            ScrollBarDirection.Horizontal => new Box(
                new Vector2(
                    position.X + thumbOffset.X + leftInset,
                    position.Y + size.Y - (barSize.X + thumbThickness.X) / 2 + topInset
                ),
                new Vector2(thumbSize.X - rightInset, thumbThickness.X - topInset - bottomInset)
            ),
            ScrollBarDirection.Vertical => new Box(
                new Vector2(
                    position.X + size.X - (barSize.Y + thumbThickness.Y) / 2 + leftInset,
                    position.Y + thumbOffset.Y + topInset
                ),
                new Vector2(thumbThickness.Y - leftInset - rightInset, thumbSize.Y - bottomInset)
            ),
            _ => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ScrollBarDirection)),
        };
    }

    private enum ScrollBarDirection : sbyte
    {
        Horizontal,
        Vertical,
    }
}
