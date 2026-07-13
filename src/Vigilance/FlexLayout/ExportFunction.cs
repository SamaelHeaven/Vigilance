// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Vigilance.FlexLayout;

public partial class Node
{
    public void CopyStyle(Node other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        Style.Copy(NodeStyle, other.NodeStyle);
    }

    public void MarkAsDirty()
    {
        Flex.NodeMarkDirtyInternal(this);
    }

    #region Style

    // StyleSetWidth sets width
    public void StyleSetWidth(float width)
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Width];
        if (dim.Number != width || dim.Unit != Unit.Point)
        {
            dim.Number = width;
            dim.Unit = Unit.Point;
            if (Flex.FloatIsUndefined(width))
                dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetWidthPercent sets width percent
    public void StyleSetWidthPercent(float width)
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Width];
        if (dim.Number != width || dim.Unit != Unit.Percent)
        {
            dim.Number = width;
            dim.Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(width))
                dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetWidthAuto sets width auto
    public void StyleSetWidthAuto()
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Width];
        if (dim.Unit != Unit.Auto)
        {
            dim.Number = float.NaN;
            dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetWidth gets width
    public Value StyleGetWidth()
    {
        return NodeStyle.Dimensions[(int)Dimension.Width];
    }

    // StyleSetHeight sets height
    public void StyleSetHeight(float height)
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Height];
        if (dim.Number != height || dim.Unit != Unit.Point)
        {
            dim.Number = height;
            dim.Unit = Unit.Point;
            if (Flex.FloatIsUndefined(height))
                dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetHeightPercent sets height percent
    public void StyleSetHeightPercent(float height)
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Height];
        if (dim.Number != height || dim.Unit != Unit.Percent)
        {
            dim.Number = height;
            dim.Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(height))
                dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetHeightAuto sets height auto
    public void StyleSetHeightAuto()
    {
        var dim = NodeStyle.Dimensions[(int)Dimension.Height];
        if (dim.Unit != Unit.Auto)
        {
            dim.Number = float.NaN;
            dim.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetHeight gets height
    public Value StyleGetHeight()
    {
        return NodeStyle.Dimensions[(int)Dimension.Height];
    }

    // StyleSetPositionType sets position type
    public void StyleSetPositionType(PositionType positionType)
    {
        if (NodeStyle.PositionType != positionType)
        {
            NodeStyle.PositionType = positionType;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public PositionType StyleGetPositionType()
    {
        return NodeStyle.PositionType;
    }

    // StyleSetPosition sets position
    public void StyleSetPosition(Edge edge, float position)
    {
        var pos = NodeStyle.Position[(int)edge];
        if (pos.Number != position || pos.Unit != Unit.Point)
        {
            pos.Number = position;
            pos.Unit = Unit.Point;
            if (Flex.FloatIsUndefined(position))
                pos.Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetPositionPercent sets position percent
    public void StyleSetPositionPercent(Edge edge, float position)
    {
        var pos = NodeStyle.Position[(int)edge];
        if (pos.Number != position || pos.Unit != Unit.Percent)
        {
            pos.Number = position;
            pos.Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(position))
                pos.Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetPosition gets position
    public Value StyleGetPosition(Edge edge)
    {
        return NodeStyle.Position[(int)edge];
    }

    // StyleSetDirection sets direction
    public void StyleSetDirection(Direction direction)
    {
        if (NodeStyle.Direction != direction)
        {
            NodeStyle.Direction = direction;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Direction StyleGetDirection()
    {
        return NodeStyle.Direction;
    }

    // StyleSetFlexDirection sets flex directions
    public void StyleSetFlexDirection(FlexDirection flexDirection)
    {
        if (NodeStyle.FlexDirection != flexDirection)
        {
            NodeStyle.FlexDirection = flexDirection;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public FlexDirection StyleGetFlexDirection()
    {
        return NodeStyle.FlexDirection;
    }

    // StyleSetJustifyContent sets justify content
    public void StyleSetJustifyContent(Justify justifyContent)
    {
        if (NodeStyle.JustifyContent != justifyContent)
        {
            NodeStyle.JustifyContent = justifyContent;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Justify StyleGetJustifyContent()
    {
        return NodeStyle.JustifyContent;
    }

    // StyleSetAlignContent sets align content
    public void StyleSetAlignContent(Align alignContent)
    {
        if (NodeStyle.AlignContent != alignContent)
        {
            NodeStyle.AlignContent = alignContent;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Align StyleGetAlignContent()
    {
        return NodeStyle.AlignContent;
    }

    // StyleSetAlignItems sets align content
    public void StyleSetAlignItems(Align alignItems)
    {
        if (NodeStyle.AlignItems != alignItems)
        {
            NodeStyle.AlignItems = alignItems;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Align StyleGetAlignItems()
    {
        return NodeStyle.AlignItems;
    }

    // StyleSetAlignSelf sets align self
    public void StyleSetAlignSelf(Align alignSelf)
    {
        if (NodeStyle.AlignSelf != alignSelf)
        {
            NodeStyle.AlignSelf = alignSelf;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Align StyleGetAlignSelf()
    {
        return NodeStyle.AlignSelf;
    }

    // StyleSetFlexWrap sets flex wrap
    public void StyleSetFlexWrap(Wrap flexWrap)
    {
        if (NodeStyle.FlexWrap != flexWrap)
        {
            NodeStyle.FlexWrap = flexWrap;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Wrap StyleGetFlexWrap()
    {
        return NodeStyle.FlexWrap;
    }

    // StyleSetOverflow sets overflow
    public void StyleSetOverflow(Overflow overflow)
    {
        if (NodeStyle.Overflow != overflow)
        {
            NodeStyle.Overflow = overflow;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Overflow StyleGetOverflow()
    {
        return NodeStyle.Overflow;
    }

    // StyleSetDisplay sets display
    public void StyleSetDisplay(Display display)
    {
        if (NodeStyle.Display != display)
        {
            NodeStyle.Display = display;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Display StyleGetDisplay()
    {
        return NodeStyle.Display;
    }

    // StyleSetFlex sets flex
    public void StyleSetFlex(float flex)
    {
        if (NodeStyle.Flex != flex)
        {
            NodeStyle.Flex = flex;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public float StyleGetFlex()
    {
        return NodeStyle.Flex;
    }

    // StyleSetFlexGrow sets flex grow
    public void StyleSetFlexGrow(float flexGrow)
    {
        if (NodeStyle.FlexGrow != flexGrow)
        {
            NodeStyle.FlexGrow = flexGrow;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetFlexGrow gets flex grow
    public float StyleGetFlexGrow()
    {
        if (float.IsNaN(NodeStyle.FlexGrow))
            return Constant.DefaultFlexGrow;
        return NodeStyle.FlexGrow;
    }

    // StyleGetFlexShrink gets flex shrink
    public float StyleGetFlexShrink()
    {
        if (float.IsNaN(NodeStyle.FlexShrink))
            return Constant.DefaultFlexShrink;

        return NodeStyle.FlexShrink;
    }

    // StyleSetFlexShrink sets flex shrink
    public void StyleSetFlexShrink(float flexShrink)
    {
        if (NodeStyle.FlexShrink != flexShrink)
        {
            NodeStyle.FlexShrink = flexShrink;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetFlexBasis sets flex basis
    public void StyleSetFlexBasis(float flexBasis)
    {
        if (NodeStyle.FlexBasis.Number != flexBasis || NodeStyle.FlexBasis.Unit != Unit.Point)
        {
            NodeStyle.FlexBasis.Number = flexBasis;
            NodeStyle.FlexBasis.Unit = Unit.Point;
            if (Flex.FloatIsUndefined(flexBasis))
                NodeStyle.FlexBasis.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetFlexBasisPercent sets flex basis percent
    public void StyleSetFlexBasisPercent(float flexBasis)
    {
        if (NodeStyle.FlexBasis.Number != flexBasis || NodeStyle.FlexBasis.Unit != Unit.Percent)
        {
            NodeStyle.FlexBasis.Number = flexBasis;
            NodeStyle.FlexBasis.Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(flexBasis))
                NodeStyle.FlexBasis.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // NodeStyleSetFlexBasisAuto sets flex basis auto
    public void NodeStyleSetFlexBasisAuto()
    {
        if (NodeStyle.FlexBasis.Unit != Unit.Auto)
        {
            NodeStyle.FlexBasis.Number = float.NaN;
            NodeStyle.FlexBasis.Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    public Value NodeStyleGetFlexBasis()
    {
        return NodeStyle.FlexBasis;
    }

    // StyleSetMargin sets margin
    public void StyleSetMargin(Edge edge, float margin)
    {
        if (NodeStyle.Margin[(int)edge].Number != margin || NodeStyle.Margin[(int)edge].Unit != Unit.Point)
        {
            NodeStyle.Margin[(int)edge].Number = margin;
            NodeStyle.Margin[(int)edge].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(margin))
                NodeStyle.Margin[(int)edge].Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetMarginPercent sets margin percent
    public void StyleSetMarginPercent(Edge edge, float margin)
    {
        if (NodeStyle.Margin[(int)edge].Number != margin || NodeStyle.Margin[(int)edge].Unit != Unit.Percent)
        {
            NodeStyle.Margin[(int)edge].Number = margin;
            NodeStyle.Margin[(int)edge].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(margin))
                NodeStyle.Margin[(int)edge].Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetMargin gets margin
    public Value StyleGetMargin(Edge edge)
    {
        return NodeStyle.Margin[(int)edge];
    }

    // StyleSetMarginAuto sets margin auto
    public void StyleSetMarginAuto(Edge edge)
    {
        if (NodeStyle.Margin[(int)edge].Unit != Unit.Auto)
        {
            NodeStyle.Margin[(int)edge].Number = float.NaN;
            NodeStyle.Margin[(int)edge].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetPadding sets padding
    public void StyleSetPadding(Edge edge, float padding)
    {
        if (NodeStyle.Padding[(int)edge].Number != padding || NodeStyle.Padding[(int)edge].Unit != Unit.Point)
        {
            NodeStyle.Padding[(int)edge].Number = padding;
            NodeStyle.Padding[(int)edge].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(padding))
                NodeStyle.Padding[(int)edge].Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetPaddingPercent sets padding percent
    public void StyleSetPaddingPercent(Edge edge, float padding)
    {
        if (NodeStyle.Padding[(int)edge].Number != padding || NodeStyle.Padding[(int)edge].Unit != Unit.Percent)
        {
            NodeStyle.Padding[(int)edge].Number = padding;
            NodeStyle.Padding[(int)edge].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(padding))
                NodeStyle.Padding[(int)edge].Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetPadding gets padding
    public Value StyleGetPadding(Edge edge)
    {
        return NodeStyle.Padding[(int)edge];
    }

    // StyleSetBorder sets border
    public void StyleSetBorder(Edge edge, float border)
    {
        if (NodeStyle.Border[(int)edge].Number != border || NodeStyle.Border[(int)edge].Unit != Unit.Point)
        {
            NodeStyle.Border[(int)edge].Number = border;
            NodeStyle.Border[(int)edge].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(border))
                NodeStyle.Border[(int)edge].Unit = Unit.Undefined;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetBorder gets border
    public float StyleGetBorder(Edge edge)
    {
        return NodeStyle.Border[(int)edge].Number;
    }

    // StyleSetMinWidth sets min width
    public void StyleSetMinWidth(float minWidth)
    {
        if (
            NodeStyle.MinDimensions[(int)Dimension.Width].Number != minWidth
            || NodeStyle.MinDimensions[(int)Dimension.Width].Unit != Unit.Point
        )
        {
            NodeStyle.MinDimensions[(int)Dimension.Width].Number = minWidth;
            NodeStyle.MinDimensions[(int)Dimension.Width].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(minWidth))
                NodeStyle.MinDimensions[(int)Dimension.Width].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetMinWidthPercent sets width percent
    public void StyleSetMinWidthPercent(float minWidth)
    {
        if (
            NodeStyle.MinDimensions[(int)Dimension.Width].Number != minWidth
            || NodeStyle.MinDimensions[(int)Dimension.Width].Unit != Unit.Percent
        )
        {
            NodeStyle.MinDimensions[(int)Dimension.Width].Number = minWidth;
            NodeStyle.MinDimensions[(int)Dimension.Width].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(minWidth))
                NodeStyle.MinDimensions[(int)Dimension.Width].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetMinWidth gets min width
    public Value StyleGetMinWidth()
    {
        return NodeStyle.MinDimensions[(int)Dimension.Width];
    }

    // StyleSetMinHeight sets min width
    public void StyleSetMinHeight(float minHeight)
    {
        if (
            NodeStyle.MinDimensions[(int)Dimension.Height].Number != minHeight
            || NodeStyle.MinDimensions[(int)Dimension.Height].Unit != Unit.Point
        )
        {
            NodeStyle.MinDimensions[(int)Dimension.Height].Number = minHeight;
            NodeStyle.MinDimensions[(int)Dimension.Height].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(minHeight))
                NodeStyle.MinDimensions[(int)Dimension.Height].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetMinHeightPercent sets min height percent
    public void StyleSetMinHeightPercent(float minHeight)
    {
        if (
            NodeStyle.MinDimensions[(int)Dimension.Height].Number != minHeight
            || NodeStyle.MinDimensions[(int)Dimension.Height].Unit != Unit.Percent
        )
        {
            NodeStyle.MinDimensions[(int)Dimension.Height].Number = minHeight;
            NodeStyle.MinDimensions[(int)Dimension.Height].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(minHeight))
                NodeStyle.MinDimensions[(int)Dimension.Height].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetMinHeight gets min height
    public Value StyleGetMinHeight()
    {
        return NodeStyle.MinDimensions[(int)Dimension.Height];
    }

    // StyleSetMaxWidth sets max width
    public void StyleSetMaxWidth(float maxWidth)
    {
        if (
            NodeStyle.MaxDimensions[(int)Dimension.Width].Number != maxWidth
            || NodeStyle.MaxDimensions[(int)Dimension.Width].Unit != Unit.Point
        )
        {
            NodeStyle.MaxDimensions[(int)Dimension.Width].Number = maxWidth;
            NodeStyle.MaxDimensions[(int)Dimension.Width].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(maxWidth))
                NodeStyle.MaxDimensions[(int)Dimension.Width].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetMaxWidthPercent sets max width percent
    public void StyleSetMaxWidthPercent(float maxWidth)
    {
        if (
            NodeStyle.MaxDimensions[(int)Dimension.Width].Number != maxWidth
            || NodeStyle.MaxDimensions[(int)Dimension.Width].Unit != Unit.Percent
        )
        {
            NodeStyle.MaxDimensions[(int)Dimension.Width].Number = maxWidth;
            NodeStyle.MaxDimensions[(int)Dimension.Width].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(maxWidth))
                NodeStyle.MaxDimensions[(int)Dimension.Width].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetMaxWidth gets max width
    public Value StyleGetMaxWidth()
    {
        return NodeStyle.MaxDimensions[(int)Dimension.Width];
    }

    // StyleSetMaxHeight sets max width
    public void StyleSetMaxHeight(float maxHeight)
    {
        if (
            NodeStyle.MaxDimensions[(int)Dimension.Height].Number != maxHeight
            || NodeStyle.MaxDimensions[(int)Dimension.Height].Unit != Unit.Point
        )
        {
            NodeStyle.MaxDimensions[(int)Dimension.Height].Number = maxHeight;
            NodeStyle.MaxDimensions[(int)Dimension.Height].Unit = Unit.Point;
            if (Flex.FloatIsUndefined(maxHeight))
                NodeStyle.MaxDimensions[(int)Dimension.Height].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleSetMaxHeightPercent sets max height percent
    public void StyleSetMaxHeightPercent(float maxHeight)
    {
        if (
            NodeStyle.MaxDimensions[(int)Dimension.Height].Number != maxHeight
            || NodeStyle.MaxDimensions[(int)Dimension.Height].Unit != Unit.Percent
        )
        {
            NodeStyle.MaxDimensions[(int)Dimension.Height].Number = maxHeight;
            NodeStyle.MaxDimensions[(int)Dimension.Height].Unit = Unit.Percent;
            if (Flex.FloatIsUndefined(maxHeight))
                NodeStyle.MaxDimensions[(int)Dimension.Height].Unit = Unit.Auto;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // StyleGetMaxHeight gets max height
    public Value StyleGetMaxHeight()
    {
        return NodeStyle.MaxDimensions[(int)Dimension.Height];
    }

    // StyleSetAspectRatio sets axpect ratio
    public void StyleSetAspectRatio(float aspectRatio)
    {
        if (NodeStyle.AspectRatio != aspectRatio)
        {
            NodeStyle.AspectRatio = aspectRatio;
            Flex.NodeMarkDirtyInternal(this);
        }
    }

    // LayoutGetLeft gets left
    public float LayoutGetLeft()
    {
        return NodeLayout.Position[(int)Edge.Left];
    }

    // LayoutGetTop gets top
    public float LayoutGetTop()
    {
        return NodeLayout.Position[(int)Edge.Top];
    }

    // LayoutGetRight gets right
    public float LayoutGetRight()
    {
        return NodeLayout.Position[(int)Edge.Right];
    }

    // LayoutGetBottom gets bottom
    public float LayoutGetBottom()
    {
        return NodeLayout.Position[(int)Edge.Bottom];
    }

    // LayoutGetWidth gets width
    public float LayoutGetWidth()
    {
        return NodeLayout.Dimensions[(int)Dimension.Width];
    }

    // LayoutGetHeight gets height
    public float LayoutGetHeight()
    {
        return NodeLayout.Dimensions[(int)Dimension.Height];
    }

    // LayoutGetMargin gets margin
    public float LayoutGetMargin(Edge edge)
    {
        Flex.AssertWithNode(this, edge < Edge.End, "Cannot get layout properties of multi-edge shorthands");
        switch (edge)
        {
            case Edge.Left:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Margin[(int)Edge.End];
                return NodeLayout.Margin[(int)Edge.Start];
            }
            case Edge.Right:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Margin[(int)Edge.Start];
                return NodeLayout.Margin[(int)Edge.End];
            }
            default:
                return NodeLayout.Margin[(int)edge];
        }
    }

    // LayoutGetBorder gets border
    public float LayoutGetBorder(Edge edge)
    {
        Flex.AssertWithNode(this, edge < Edge.End, "Cannot get layout properties of multi-edge shorthands");
        switch (edge)
        {
            case Edge.Left:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Border[(int)Edge.End];
                return NodeLayout.Border[(int)Edge.Start];
            }
            case Edge.Right:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Border[(int)Edge.Start];
                return NodeLayout.Border[(int)Edge.End];
            }
            default:
                return NodeLayout.Border[(int)edge];
        }
    }

    // LayoutGetPadding gets padding
    public float LayoutGetPadding(Edge edge)
    {
        Flex.AssertWithNode(this, edge < Edge.End, "Cannot get layout properties of multi-edge shorthands");
        switch (edge)
        {
            case Edge.Left:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Padding[(int)Edge.End];
                return NodeLayout.Padding[(int)Edge.Start];
            }
            case Edge.Right:
            {
                if (NodeLayout.Direction == Direction.Rtl)
                    return NodeLayout.Padding[(int)Edge.Start];
                return NodeLayout.Padding[(int)Edge.End];
            }
            default:
                return NodeLayout.Padding[(int)edge];
        }
    }

    public Direction LayoutGetDirection()
    {
        return NodeLayout.Direction;
    }

    public bool LayoutGetHadOverflow()
    {
        return NodeLayout.HadOverflow;
    }

    #endregion

    #region other props

    public void SetMeasureFunc(MeasureFunc measureFunc)
    {
        Flex.SetMeasureFunc(this, measureFunc);
    }

    public MeasureFunc? GetMeasureFunc()
    {
        return MeasureFunc;
    }

    public void SetBaselineFunc(BaselineFunc baselineFunc)
    {
        BaselineFunc = baselineFunc;
    }

    public BaselineFunc? GetBaselineFunc()
    {
        return BaselineFunc;
    }

    #endregion

    #region tree

    public Node? GetParent()
    {
        return Parent;
    }

    public IEnumerable<Node> GetChildrenIter()
    {
        return Children;
    }

    public Node GetChild(int idx)
    {
        return Flex.GetChild(this, idx);
    }

    public void AddChild(Node? child)
    {
        if (child == null || child.Parent == this)
            return;

        Flex.InsertChild(this, child, ChildrenCount);
    }

    public int IndexOfChild(Node child)
    {
        return Children.IndexOf(child);
    }

    public void InsertChild(Node? child, int idx)
    {
        if (child == null)
            return;

        Flex.InsertChild(this, child, idx);
    }

    public void RemoveChild(Node child)
    {
        Flex.RemoveChild(this, child);
    }

    public bool ReplaceChild(int index, Node? child)
    {
        if (child == null)
            return false;

        if (0 <= index && index < ChildrenCount)
        {
            child.Parent = this;
            Children[index] = child;
            MarkAsDirty();
            return true;
        }

        return false;
    }

    public void SetParent(Node parent)
    {
        if (parent == Parent)
            return;

        RemoveParent();
        parent.AddChild(this);
    }

    public void RemoveParent()
    {
        if (Parent != null)
            Parent.RemoveChild(this);
    }

    #endregion
}
