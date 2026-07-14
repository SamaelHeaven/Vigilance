// ReSharper disable CompareOfFloatsByEqualityOperator

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

using System.Runtime.CompilerServices;
using Vigilance.Core;

namespace Vigilance.FlexLayout;

internal static class Constant
{
    internal const int EdgeCount = 9;

    // This value was chosen based on empiracle data. Even the most complicated
    // layouts should not require more than 16 entries to fit within the cache.
    internal const int MaxCachedResultCount = 16;

    internal const float DefaultFlexGrow = 0;
    internal const float DefaultFlexShrink = 0;
}

internal struct Style
{
    internal ValueBufferEdge Border;
    internal ValueBuffer2 Dimensions;
    internal ValueBufferEdge Margin;
    internal ValueBuffer2 MaxDimensions;
    internal ValueBuffer2 MinDimensions;
    internal ValueBufferEdge Padding;
    internal ValueBufferEdge Position;
    internal Align AlignContent = Align.Start;
    internal Align AlignItems = Align.Stretch;

    internal Align AlignSelf;

    // Yoga specific properties, not compatible with flexbox specification
    internal float AspectRatio = float.NaN;

    internal Direction Direction = Direction.Inherit;
    internal Display Display = Display.Flex;
    internal float Flex = float.NaN;
    internal Value FlexBasis = CreateAutoValue();

    // Spacing between flex items. GapColumn is the horizontal gutter (CSS
    // column-gap), GapRow is the vertical gutter (CSS row-gap). Undefined means
    // no gap.
    internal Value GapColumn = Value.UndefinedValue;
    internal Value GapRow = Value.UndefinedValue;
    internal FlexDirection FlexDirection = FlexDirection.Column;
    internal float FlexGrow = float.NaN;
    internal float FlexShrink = float.NaN;
    internal Wrap FlexWrap;
    internal Justify JustifyContent = Justify.Start;
    internal Overflow Overflow = Overflow.Visible;
    internal PositionType PositionType;

    public Style()
    {
        for (var i = 0; i < Constant.EdgeCount; i++)
        {
            Border[i] = Value.UndefinedValue;
            Margin[i] = Value.UndefinedValue;
            Padding[i] = Value.UndefinedValue;
            Position[i] = Value.UndefinedValue;
        }

        Dimensions[0] = CreateAutoValue();
        Dimensions[1] = CreateAutoValue();
        MinDimensions[0] = Value.UndefinedValue;
        MinDimensions[1] = Value.UndefinedValue;
        MaxDimensions[0] = Value.UndefinedValue;
        MaxDimensions[1] = Value.UndefinedValue;
    }

    internal static Value CreateAutoValue()
    {
        return new Value(float.NaN, Unit.Auto);
    }
}

public static partial class Flex
{
    internal static readonly Value ValueZero = new(0, Unit.Point);
    internal static readonly Value ValueUndefined = new(float.NaN, Unit.Undefined);
    internal static readonly Value ValueAuto = new(float.NaN, Unit.Auto);
    internal static int CurrentGenerationCount = 0;
    internal static readonly InlineArray4<Edge> Leading;
    internal static readonly InlineArray4<Edge> Trailing;
    internal static readonly InlineArray4<Edge> Pos;
    internal static readonly InlineArray4<Dimension> Dim;

    static Flex()
    {
        ReadOnlySpan<Edge> leading = [Edge.Top, Edge.Bottom, Edge.Left, Edge.Right];
        leading.CopyTo(Leading);
        ReadOnlySpan<Edge> trailing = [Edge.Bottom, Edge.Top, Edge.Right, Edge.Left];
        trailing.CopyTo(Trailing);
        ReadOnlySpan<Edge> pos = [Edge.Top, Edge.Bottom, Edge.Left, Edge.Right];
        pos.CopyTo(Pos);
        ReadOnlySpan<Dimension> dim = [Dimension.Height, Dimension.Height, Dimension.Width, Dimension.Width];
        dim.CopyTo(Dim);
    }

    internal static bool Feq(float a, float b)
    {
        if (float.IsNaN(a) && float.IsNaN(b))
            return true;

        return a == b;
    }

    internal static bool ValueEq(Value v1, Value v2)
    {
        if (v1.Unit != v2.Unit)
            return false;
        return Feq(v1.Number, v2.Number);
    }

    internal static Value ComputedEdgeValue(ReadOnlySpan<Value> edges, Edge edge, Value defaultValue)
    {
        if (edges[(int)edge].Unit != Unit.Undefined)
        {
            return edges[(int)edge];
        }

        var isVertEdge = edge is Edge.Top or Edge.Bottom;
        if (isVertEdge && edges[(int)Edge.Vertical].Unit != Unit.Undefined)
        {
            return edges[(int)Edge.Vertical];
        }

        var isHorizEdge = edge is Edge.Left or Edge.Right or Edge.Start or Edge.End;
        if (isHorizEdge && edges[(int)Edge.Horizontal].Unit != Unit.Undefined)
        {
            return edges[(int)Edge.Horizontal];
        }

        if (edges[(int)Edge.All].Unit != Unit.Undefined)
        {
            return edges[(int)Edge.All];
        }

        if (edge is Edge.Start or Edge.End)
        {
            return ValueUndefined;
        }

        return defaultValue;
    }

    internal static float ResolveValue(Value value, float parentSize)
    {
        switch (value.Unit)
        {
            case Unit.Undefined:
            case Unit.Auto:
                break;
            case Unit.Point:
                return value.Number;
            case Unit.Percent:
                return value.Number * parentSize / 100f;
        }

        return float.NaN;
    }

    internal static float ResolveValueMargin(Value value, float parentSize)
    {
        if (value.Unit == Unit.Auto)
        {
            return 0;
        }

        return ResolveValue(value, parentSize);
    }

    internal static float NodeResolveGap<TStorage>(Node<TStorage> node, FlexDirection axis, float availableSize)
        where TStorage : IList<Node<TStorage>>
    {
        var gap = FlexDirectionIsRow(axis) ? node.NodeStyle.GapColumn : node.NodeStyle.GapRow;
        var resolved = ResolveValue(gap, availableSize);
        return FloatIsUndefined(resolved) || resolved < 0 ? 0 : resolved;
    }

    internal static void NodeMarkDirtyInternal<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        if (!node.IsDirty)
        {
            node.IsDirty = true;
            node.NodeLayout.ComputedFlexBasis = float.NaN;
            if (node.Parent != null)
            {
                NodeMarkDirtyInternal(node.Parent);
            }
        }
    }

    // SetMeasureFunc sets measure function
    internal static void SetMeasureFunc<TStorage>(Node<TStorage> node, MeasureFunc<TStorage>? measureFunc)
        where TStorage : IList<Node<TStorage>>
    {
        if (measureFunc == null)
        {
            node.MeasureFunc = null;
            // TODO: t18095186 Move nodeType to opt-in function and mark appropriate places in Litho
            node.NodeType = NodeType.Default;
        }
        else
        {
            Debug.Assert(
                node.Storage.Count == 0,
                "Cannot set measure function: Nodes with measure functions cannot have children."
            );
            node.MeasureFunc = measureFunc;
            // TODO: t18095186 Move nodeType to opt-in function and mark appropriate places in Litho
            node.NodeType = NodeType.Text;
        }
    }

    // InsertChild inserts a child
    internal static void InsertChild<TStorage>(Node<TStorage> node, Node<TStorage> child, int idx)
        where TStorage : IList<Node<TStorage>>
    {
        Debug.Assert(child.Parent == null, "Child already has a parent, it must be removed first.");
        Debug.Assert(node.MeasureFunc == null, "Cannot add child: Nodes with measure functions cannot have children.");

        node.Storage.Insert(idx, child);
        child.Parent = node;
        NodeMarkDirtyInternal(node);
    }

    // RemoveChild removes child node
    internal static void RemoveChild<TStorage>(Node<TStorage> node, Node<TStorage> child)
        where TStorage : IList<Node<TStorage>>
    {
        if (node.Storage.Remove(child))
        {
            child.NodeLayout.ResetToDefault(); // layout is no longer valid
            child.Parent = null;
            NodeMarkDirtyInternal(node);
        }
    }

    // GetChild returns a child at a given index
    internal static Node<TStorage> GetChild<TStorage>(Node<TStorage> node, int idx)
        where TStorage : IList<Node<TStorage>>
    {
        return idx < node.Storage.Count ? node.Storage[idx] : null!;
    }

    internal static bool StyleEq(in Style s1, in Style s2)
    {
        if (
            s1.Direction != s2.Direction
            || s1.FlexDirection != s2.FlexDirection
            || s1.JustifyContent != s2.JustifyContent
            || s1.AlignContent != s2.AlignContent
            || s1.AlignItems != s2.AlignItems
            || s1.AlignSelf != s2.AlignSelf
            || s1.PositionType != s2.PositionType
            || s1.FlexWrap != s2.FlexWrap
            || s1.Overflow != s2.Overflow
            || s1.Display != s2.Display
            || !Feq(s1.Flex, s2.Flex)
            || !Feq(s1.FlexGrow, s2.FlexGrow)
            || !Feq(s1.FlexShrink, s2.FlexShrink)
            || !ValueEq(s1.FlexBasis, s2.FlexBasis)
            || !ValueEq(s1.GapColumn, s2.GapColumn)
            || !ValueEq(s1.GapRow, s2.GapRow)
        )
        {
            return false;
        }

        for (var i = 0; i < Constant.EdgeCount; i++)
        {
            if (
                !ValueEq(s1.Margin[i], s2.Margin[i])
                || !ValueEq(s1.Position[i], s2.Position[i])
                || !ValueEq(s1.Padding[i], s2.Padding[i])
                || !ValueEq(s1.Border[i], s2.Border[i])
            )
            {
                return false;
            }
        }

        for (var i = 0; i < 2; i++)
        {
            if (
                !ValueEq(s1.Dimensions[i], s2.Dimensions[i])
                || !ValueEq(s1.MinDimensions[i], s2.MinDimensions[i])
                || !ValueEq(s1.MaxDimensions[i], s2.MaxDimensions[i])
            )
            {
                return false;
            }
        }

        return true;
    }

    internal static float ResolveFlexGrow<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        // Root nodes flexGrow should always be 0
        if (node.Parent == null)
        {
            return 0;
        }

        if (!FloatIsUndefined(node.NodeStyle.FlexGrow))
        {
            return node.NodeStyle.FlexGrow;
        }

        if (!FloatIsUndefined(node.NodeStyle.Flex) && node.NodeStyle.Flex > 0)
        {
            return node.NodeStyle.Flex;
        }

        return Constant.DefaultFlexGrow;
    }

    internal static float NodeResolveFlexShrink<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        // Root nodes flexShrink should always be 0
        if (node.Parent == null)
        {
            return 0;
        }

        if (!FloatIsUndefined(node.NodeStyle.FlexShrink))
        {
            return node.NodeStyle.FlexShrink;
        }

        if (!FloatIsUndefined(node.NodeStyle.Flex) && node.NodeStyle.Flex < 0)
        {
            return -node.NodeStyle.Flex;
        }

        return Constant.DefaultFlexShrink;
    }

    internal static Value NodeResolveFlexBasisPtr<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        ref readonly var style = ref node.NodeStyle;
        if (style.FlexBasis.Unit != Unit.Auto && style.FlexBasis.Unit != Unit.Undefined)
        {
            return style.FlexBasis;
        }

        if (!FloatIsUndefined(style.Flex) && style.Flex > 0)
        {
            return ValueZero;
        }

        return ValueAuto;
    }

    // FloatIsUndefined returns true if value is undefined
    internal static bool FloatIsUndefined(float value)
    {
        return float.IsNaN(value);
    }

    // ValueEqual returns true if values are equal
    internal static bool ValueEqual(Value a, Value b)
    {
        if (a.Unit != b.Unit)
        {
            return false;
        }

        if (a.Unit == Unit.Undefined)
        {
            return true;
        }

        return System.Math.Abs(a.Number - b.Number) < 0.0001f;
    }

    internal static void ResolveDimensions<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        for (var dim = (int)Dimension.Width; dim <= (int)Dimension.Height; dim++)
        {
            if (
                node.NodeStyle.MaxDimensions[dim].Unit != Unit.Undefined
                && ValueEqual(node.NodeStyle.MaxDimensions[dim], node.NodeStyle.MinDimensions[dim])
            )
            {
                node.ResolvedDimensions[dim] = node.NodeStyle.MaxDimensions[dim];
            }
            else
            {
                node.ResolvedDimensions[dim] = node.NodeStyle.Dimensions[dim];
            }
        }
    }

    internal static bool FlexDirectionIsRow(FlexDirection flexDirection)
    {
        return flexDirection is FlexDirection.Row or FlexDirection.RowReverse;
    }

    internal static bool FlexDirectionIsColumn(FlexDirection flexDirection)
    {
        return flexDirection is FlexDirection.Column or FlexDirection.ColumnReverse;
    }

    internal static float NodeLeadingMargin<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis) && node.NodeStyle.Margin[(int)Edge.Start].Unit != Unit.Undefined)
        {
            return ResolveValueMargin(node.NodeStyle.Margin[(int)Edge.Start], widthSize);
        }

        var v = ComputedEdgeValue(node.NodeStyle.Margin, Leading[(int)axis], ValueZero);
        return ResolveValueMargin(v, widthSize);
    }

    internal static float NodeTrailingMargin<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis) && node.NodeStyle.Margin[(int)Edge.End].Unit != Unit.Undefined)
        {
            return ResolveValueMargin(node.NodeStyle.Margin[(int)Edge.End], widthSize);
        }

        return ResolveValueMargin(ComputedEdgeValue(node.NodeStyle.Margin, Trailing[(int)axis], ValueZero), widthSize);
    }

    internal static float NodeLeadingPadding<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (
            FlexDirectionIsRow(axis)
            && node.NodeStyle.Padding[(int)Edge.Start].Unit != Unit.Undefined
            && ResolveValue(node.NodeStyle.Padding[(int)Edge.Start], widthSize) >= 0
        )
        {
            return ResolveValue(node.NodeStyle.Padding[(int)Edge.Start], widthSize);
        }

        return Fmaxf(
            ResolveValue(ComputedEdgeValue(node.NodeStyle.Padding, Leading[(int)axis], ValueZero), widthSize),
            0
        );
    }

    internal static float NodeTrailingPadding<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (
            FlexDirectionIsRow(axis)
            && node.NodeStyle.Padding[(int)Edge.End].Unit != Unit.Undefined
            && ResolveValue(node.NodeStyle.Padding[(int)Edge.End], widthSize) >= 0
        )
        {
            return ResolveValue(node.NodeStyle.Padding[(int)Edge.End], widthSize);
        }

        return Fmaxf(
            ResolveValue(ComputedEdgeValue(node.NodeStyle.Padding, Trailing[(int)axis], ValueZero), widthSize),
            0
        );
    }

    internal static float NodeLeadingBorder<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        if (
            FlexDirectionIsRow(axis)
            && node.NodeStyle.Border[(int)Edge.Start].Unit != Unit.Undefined
            && node.NodeStyle.Border[(int)Edge.Start].Number >= 0
        )
        {
            return node.NodeStyle.Border[(int)Edge.Start].Number;
        }

        return Fmaxf(ComputedEdgeValue(node.NodeStyle.Border, Leading[(int)axis], ValueZero).Number, 0);
    }

    internal static float NodeTrailingBorder<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        if (
            FlexDirectionIsRow(axis)
            && node.NodeStyle.Border[(int)Edge.End].Unit != Unit.Undefined
            && node.NodeStyle.Border[(int)Edge.End].Number >= 0
        )
        {
            return node.NodeStyle.Border[(int)Edge.End].Number;
        }

        return Fmaxf(ComputedEdgeValue(node.NodeStyle.Border, Trailing[(int)axis], ValueZero).Number, 0);
    }

    internal static float NodeLeadingPaddingAndBorder<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float widthSize
    )
        where TStorage : IList<Node<TStorage>>
    {
        return NodeLeadingPadding(node, axis, widthSize) + NodeLeadingBorder(node, axis);
    }

    internal static float NodeTrailingPaddingAndBorder<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float widthSize
    )
        where TStorage : IList<Node<TStorage>>
    {
        return NodeTrailingPadding(node, axis, widthSize) + NodeTrailingBorder(node, axis);
    }

    internal static float NodeMarginForAxis<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        var leading = NodeLeadingMargin(node, axis, widthSize);
        var trailing = NodeTrailingMargin(node, axis, widthSize);
        return leading + trailing;
    }

    internal static float NodePaddingAndBorderForAxis<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float widthSize
    )
        where TStorage : IList<Node<TStorage>>
    {
        return NodeLeadingPaddingAndBorder(node, axis, widthSize) + NodeTrailingPaddingAndBorder(node, axis, widthSize);
    }

    internal static Align NodeAlignItem<TStorage>(Node<TStorage> node, Node<TStorage> child)
        where TStorage : IList<Node<TStorage>>
    {
        var align = child.NodeStyle.AlignSelf;
        if (child.NodeStyle.AlignSelf == Align.Auto)
        {
            align = node.NodeStyle.AlignItems;
        }

        if (align == Align.Baseline && FlexDirectionIsColumn(node.NodeStyle.FlexDirection))
        {
            return Align.Start;
        }

        return align;
    }

    internal static Direction NodeResolveDirection<TStorage>(Node<TStorage> node, Direction parentDirection)
        where TStorage : IList<Node<TStorage>>
    {
        if (node.NodeStyle.Direction == Direction.Inherit)
        {
            if (parentDirection > Direction.Inherit)
            {
                return parentDirection;
            }

            return Direction.LeftToRight;
        }

        return node.NodeStyle.Direction;
    }

    // Baseline retuns baseline
    internal static float Baseline<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        if (node.BaselineFunc != null)
        {
            var baseline = node.BaselineFunc(
                node,
                node.NodeLayout.MeasuredDimensions[(int)Dimension.Width],
                node.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
            );
            Debug.Assert(!FloatIsUndefined(baseline), "Expect custom baseline function to not return NaN");
            return baseline;
        }
        else
        {
            Node<TStorage>? baselineChild = null;
            foreach (var child in node)
            {
                if (child.LineIndex > 0)
                {
                    break;
                }

                if (child.NodeStyle.PositionType == PositionType.Absolute)
                {
                    continue;
                }

                if (NodeAlignItem(node, child) == Align.Baseline)
                {
                    baselineChild = child;
                    break;
                }

                baselineChild ??= child;
            }

            if (baselineChild == null)
            {
                return node.NodeLayout.MeasuredDimensions[(int)Dimension.Height];
            }

            var baseline = Baseline(baselineChild);
            return baseline + baselineChild.NodeLayout.Position[(int)Edge.Top];
        }
    }

    internal static FlexDirection ResolveFlexDirection(FlexDirection flexDirection, Direction direction)
    {
        if (direction == Direction.RightToLeft)
        {
            switch (flexDirection)
            {
                case FlexDirection.Row:
                    return FlexDirection.RowReverse;
                case FlexDirection.RowReverse:
                    return FlexDirection.Row;
            }
        }

        return flexDirection;
    }

    internal static FlexDirection FlexDirectionCross(FlexDirection flexDirection, Direction direction)
    {
        if (FlexDirectionIsColumn(flexDirection))
        {
            return ResolveFlexDirection(FlexDirection.Row, direction);
        }

        return FlexDirection.Column;
    }

    internal static bool NodeIsFlex<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        return node.NodeStyle.PositionType == PositionType.Relative
            && (ResolveFlexGrow(node) != 0 || NodeResolveFlexShrink(node) != 0);
    }

    internal static bool IsBaselineLayout<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsColumn(node.NodeStyle.FlexDirection))
        {
            return false;
        }

        if (node.NodeStyle.AlignItems == Align.Baseline)
        {
            return true;
        }

        foreach (var child in node)
        {
            if (child.NodeStyle is { PositionType: PositionType.Relative, AlignSelf: Align.Baseline })
            {
                return true;
            }
        }

        return false;
    }

    internal static float NodeDimWithMargin<TStorage>(Node<TStorage> node, FlexDirection axis, float widthSize)
        where TStorage : IList<Node<TStorage>>
    {
        return node.NodeLayout.MeasuredDimensions[(int)Dim[(int)axis]]
            + NodeLeadingMargin(node, axis, widthSize)
            + NodeTrailingMargin(node, axis, widthSize);
    }

    internal static bool NodeIsStyleDimDefined<TStorage>(Node<TStorage> node, FlexDirection axis, float parentSize)
        where TStorage : IList<Node<TStorage>>
    {
        var v = node.ResolvedDimensions[(int)Dim[(int)axis]];
        var isNotDefined =
            v.Unit == Unit.Auto
            || v.Unit == Unit.Undefined
            || v is { Unit: Unit.Point, Number: < 0 }
            || (v.Unit == Unit.Percent && (v.Number < 0 || FloatIsUndefined(parentSize)));
        return !isNotDefined;
    }

    internal static bool NodeIsLayoutDimDefined<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        var value = node.NodeLayout.MeasuredDimensions[(int)Dim[(int)axis]];
        return !FloatIsUndefined(value) && value >= 0;
    }

    internal static bool NodeIsLeadingPosDefined<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        return (
                FlexDirectionIsRow(axis)
                && ComputedEdgeValue(node.NodeStyle.Position, Edge.Start, ValueUndefined).Unit != Unit.Undefined
            )
            || ComputedEdgeValue(node.NodeStyle.Position, Leading[(int)axis], ValueUndefined).Unit != Unit.Undefined;
    }

    internal static bool NodeIsTrailingPosDefined<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        return (
                FlexDirectionIsRow(axis)
                && ComputedEdgeValue(node.NodeStyle.Position, Edge.End, ValueUndefined).Unit != Unit.Undefined
            )
            || ComputedEdgeValue(node.NodeStyle.Position, Trailing[(int)axis], ValueUndefined).Unit != Unit.Undefined;
    }

    internal static float NodeLeadingPosition<TStorage>(Node<TStorage> node, FlexDirection axis, float axisSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis))
        {
            var leadingPosition = ComputedEdgeValue(node.NodeStyle.Position, Edge.Start, ValueUndefined);
            if (leadingPosition.Unit != Unit.Undefined)
            {
                return ResolveValue(leadingPosition, axisSize);
            }
        }

        {
            var leadingPosition = ComputedEdgeValue(node.NodeStyle.Position, Leading[(int)axis], ValueUndefined);

            if (leadingPosition.Unit == Unit.Undefined)
            {
                return 0;
            }

            return ResolveValue(leadingPosition, axisSize);
        }
    }

    internal static float NodeTrailingPosition<TStorage>(Node<TStorage> node, FlexDirection axis, float axisSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis))
        {
            var trailingPosition = ComputedEdgeValue(node.NodeStyle.Position, Edge.End, ValueUndefined);
            if (trailingPosition.Unit != Unit.Undefined)
            {
                return ResolveValue(trailingPosition, axisSize);
            }
        }

        {
            var trailingPosition = ComputedEdgeValue(node.NodeStyle.Position, Trailing[(int)axis], ValueUndefined);

            if (trailingPosition.Unit == Unit.Undefined)
            {
                return 0;
            }

            return ResolveValue(trailingPosition, axisSize);
        }
    }

    internal static float NodeBoundAxisWithinMinAndMax<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float value,
        float axisSize
    )
        where TStorage : IList<Node<TStorage>>
    {
        var min = float.NaN;
        var max = float.NaN;

        if (FlexDirectionIsColumn(axis))
        {
            min = ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Height], axisSize);
            max = ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Height], axisSize);
        }
        else if (FlexDirectionIsRow(axis))
        {
            min = ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Width], axisSize);
            max = ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Width], axisSize);
        }

        var boundValue = value;

        if (!FloatIsUndefined(max) && max >= 0 && boundValue > max)
        {
            boundValue = max;
        }

        if (!FloatIsUndefined(min) && min >= 0 && boundValue < min)
        {
            boundValue = min;
        }

        return boundValue;
    }

    internal static Value MarginLeadingValue<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis) && node.NodeStyle.Margin[(int)Edge.Start].Unit != Unit.Undefined)
        {
            return node.NodeStyle.Margin[(int)Edge.Start];
        }

        return node.NodeStyle.Margin[(int)Leading[(int)axis]];
    }

    internal static Value MarginTrailingValue<TStorage>(Node<TStorage> node, FlexDirection axis)
        where TStorage : IList<Node<TStorage>>
    {
        if (FlexDirectionIsRow(axis) && node.NodeStyle.Margin[(int)Edge.End].Unit != Unit.Undefined)
        {
            return node.NodeStyle.Margin[(int)Edge.End];
        }

        return node.NodeStyle.Margin[(int)Trailing[(int)axis]];
    }

    // nodeBoundAxis is like nodeBoundAxisWithinMinAndMax but also ensures that
    // the value doesn't go below the padding and border amount.
    internal static float NodeBoundAxis<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float value,
        float axisSize,
        float widthSize
    )
        where TStorage : IList<Node<TStorage>>
    {
        return Fmaxf(
            NodeBoundAxisWithinMinAndMax(node, axis, value, axisSize),
            NodePaddingAndBorderForAxis(node, axis, widthSize)
        );
    }

    internal static void NodeSetChildTrailingPosition<TStorage>(
        Node<TStorage> node,
        Node<TStorage> child,
        FlexDirection axis
    )
        where TStorage : IList<Node<TStorage>>
    {
        var size = child.NodeLayout.MeasuredDimensions[(int)Dim[(int)axis]];
        child.NodeLayout.Position[(int)Trailing[(int)axis]] =
            node.NodeLayout.MeasuredDimensions[(int)Dim[(int)axis]]
            - size
            - child.NodeLayout.Position[(int)Pos[(int)axis]];
    }

    // If both left and right are defined, then use left. Otherwise, return
    // +left or -right depending on which is defined.
    internal static float NodeRelativePosition<TStorage>(Node<TStorage> node, FlexDirection axis, float axisSize)
        where TStorage : IList<Node<TStorage>>
    {
        if (NodeIsLeadingPosDefined(node, axis))
        {
            return NodeLeadingPosition(node, axis, axisSize);
        }

        return -NodeTrailingPosition(node, axis, axisSize);
    }

    internal static void ConstrainMaxSizeForMode<TStorage>(
        Node<TStorage> node,
        FlexDirection axis,
        float parentAxisSize,
        float parentWidth,
        ref MeasureMode mode,
        ref float size
    )
        where TStorage : IList<Node<TStorage>>
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        var maxSize =
            ResolveValue(node.NodeStyle.MaxDimensions[(int)Dim[(int)axis]], parentAxisSize)
            + NodeMarginForAxis(node, axis, parentWidth);
        switch (mode)
        {
            case MeasureMode.Exactly:
            case MeasureMode.AtMost:
                if (FloatIsUndefined(maxSize) || size < maxSize) { }
                else
                {
                    size = maxSize;
                }

                break;
            case MeasureMode.Undefined:
                if (!FloatIsUndefined(maxSize))
                {
                    mode = MeasureMode.AtMost;
                    size = maxSize;
                }

                break;
        }
    }

    internal static void NodeSetPosition<TStorage>(
        Node<TStorage> node,
        Direction direction,
        float mainSize,
        float crossSize,
        float parentWidth
    )
        where TStorage : IList<Node<TStorage>>
    {
        // Root nodes should be always layouted as LTR, so we don't return negative values.
        var directionRespectingRoot = Direction.LeftToRight;
        if (node.Parent != null)
        {
            directionRespectingRoot = direction;
        }

        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, directionRespectingRoot);
        var crossAxis = FlexDirectionCross(mainAxis, directionRespectingRoot);

        var relativePositionMain = NodeRelativePosition(node, mainAxis, mainSize);
        var relativePositionCross = NodeRelativePosition(node, crossAxis, crossSize);

        ref var pos = ref node.NodeLayout.Position;
        pos[(int)Leading[(int)mainAxis]] = NodeLeadingMargin(node, mainAxis, parentWidth) + relativePositionMain;
        pos[(int)Trailing[(int)mainAxis]] = NodeTrailingMargin(node, mainAxis, parentWidth) + relativePositionMain;
        pos[(int)Leading[(int)crossAxis]] = NodeLeadingMargin(node, crossAxis, parentWidth) + relativePositionCross;
        pos[(int)Trailing[(int)crossAxis]] = NodeTrailingMargin(node, crossAxis, parentWidth) + relativePositionCross;
    }

    internal static void NodeComputeFlexBasisForChild<TStorage>(
        Node<TStorage> node,
        Node<TStorage> child,
        float width,
        MeasureMode widthMode,
        float height,
        float parentWidth,
        float parentHeight,
        MeasureMode heightMode,
        Direction direction
    )
        where TStorage : IList<Node<TStorage>>
    {
        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);
        var isMainAxisRow = FlexDirectionIsRow(mainAxis);
        var mainAxisSize = height;
        var mainAxisParentSize = parentHeight;
        if (isMainAxisRow)
        {
            mainAxisSize = width;
            mainAxisParentSize = parentWidth;
        }

        var resolvedFlexBasis = ResolveValue(NodeResolveFlexBasisPtr(child), mainAxisParentSize);
        var isRowStyleDimDefined = NodeIsStyleDimDefined(child, FlexDirection.Row, parentWidth);
        var isColumnStyleDimDefined = NodeIsStyleDimDefined(child, FlexDirection.Column, parentHeight);

        if (!FloatIsUndefined(resolvedFlexBasis) && !FloatIsUndefined(mainAxisSize))
        {
            if (FloatIsUndefined(child.NodeLayout.ComputedFlexBasis))
            {
                child.NodeLayout.ComputedFlexBasis = Fmaxf(
                    resolvedFlexBasis,
                    NodePaddingAndBorderForAxis(child, mainAxis, parentWidth)
                );
            }
        }
        else
            switch (isMainAxisRow)
            {
                case true when isRowStyleDimDefined:
                    // The width is definite, so use that as the flex basis.
                    child.NodeLayout.ComputedFlexBasis = Fmaxf(
                        ResolveValue(child.ResolvedDimensions[(int)Dimension.Width], parentWidth),
                        NodePaddingAndBorderForAxis(child, FlexDirection.Row, parentWidth)
                    );
                    break;
                case false when isColumnStyleDimDefined:
                    // The height is definite, so use that as the flex basis.
                    child.NodeLayout.ComputedFlexBasis = Fmaxf(
                        ResolveValue(child.ResolvedDimensions[(int)Dimension.Height], parentHeight),
                        NodePaddingAndBorderForAxis(child, FlexDirection.Column, parentWidth)
                    );
                    break;
                default:
                {
                    // Compute the flex basis and hypothetical main size (i.e. the clamped
                    // flex basis).
                    var childWidth = float.NaN;
                    var childHeight = float.NaN;
                    var childWidthMeasureMode = MeasureMode.Undefined;
                    var childHeightMeasureMode = MeasureMode.Undefined;

                    var marginRow = NodeMarginForAxis(child, FlexDirection.Row, parentWidth);
                    var marginColumn = NodeMarginForAxis(child, FlexDirection.Column, parentWidth);

                    if (isRowStyleDimDefined)
                    {
                        childWidth =
                            ResolveValue(child.ResolvedDimensions[(int)Dimension.Width], parentWidth) + marginRow;
                        childWidthMeasureMode = MeasureMode.Exactly;
                    }

                    if (isColumnStyleDimDefined)
                    {
                        childHeight =
                            ResolveValue(child.ResolvedDimensions[(int)Dimension.Height], parentHeight) + marginColumn;
                        childHeightMeasureMode = MeasureMode.Exactly;
                    }

                    // The W3C spec doesn't say anything about the 'overflow' property,
                    // but all major browsers appear to implement the following logic.
                    if (
                        (!isMainAxisRow && node.NodeStyle.Overflow == Overflow.Scroll)
                        || node.NodeStyle.Overflow != Overflow.Scroll
                    )
                    {
                        if (FloatIsUndefined(childWidth) && !FloatIsUndefined(width))
                        {
                            childWidth = width;
                            childWidthMeasureMode = MeasureMode.AtMost;
                        }
                    }

                    if (
                        (isMainAxisRow && node.NodeStyle.Overflow == Overflow.Scroll)
                        || node.NodeStyle.Overflow != Overflow.Scroll
                    )
                    {
                        if (FloatIsUndefined(childHeight) && !FloatIsUndefined(height))
                        {
                            childHeight = height;
                            childHeightMeasureMode = MeasureMode.AtMost;
                        }
                    }

                    switch (isMainAxisRow)
                    {
                        // If child has no defined size in the cross axis and is set to stretch,
                        // set the cross
                        // axis to be measured exactly with the available inner width
                        case false
                            when !FloatIsUndefined(width)
                                && !isRowStyleDimDefined
                                && widthMode == MeasureMode.Exactly
                                && NodeAlignItem(node, child) == Align.Stretch:
                            childWidth = width;
                            childWidthMeasureMode = MeasureMode.Exactly;
                            break;
                        case true
                            when !FloatIsUndefined(height)
                                && !isColumnStyleDimDefined
                                && heightMode == MeasureMode.Exactly
                                && NodeAlignItem(node, child) == Align.Stretch:
                            childHeight = height;
                            childHeightMeasureMode = MeasureMode.Exactly;
                            break;
                    }

                    if (!FloatIsUndefined(child.NodeStyle.AspectRatio))
                    {
                        switch (isMainAxisRow)
                        {
                            case false when childWidthMeasureMode == MeasureMode.Exactly:
                                child.NodeLayout.ComputedFlexBasis = Fmaxf(
                                    (childWidth - marginRow) / child.NodeStyle.AspectRatio,
                                    NodePaddingAndBorderForAxis(child, FlexDirection.Column, parentWidth)
                                );
                                return;
                            case true when childHeightMeasureMode == MeasureMode.Exactly:
                                child.NodeLayout.ComputedFlexBasis = Fmaxf(
                                    (childHeight - marginColumn) * child.NodeStyle.AspectRatio,
                                    NodePaddingAndBorderForAxis(child, FlexDirection.Row, parentWidth)
                                );
                                return;
                        }
                    }

                    ConstrainMaxSizeForMode(
                        child,
                        FlexDirection.Row,
                        parentWidth,
                        parentWidth,
                        ref childWidthMeasureMode,
                        ref childWidth
                    );
                    ConstrainMaxSizeForMode(
                        child,
                        FlexDirection.Column,
                        parentHeight,
                        parentWidth,
                        ref childHeightMeasureMode,
                        ref childHeight
                    );

                    // Measure the child
                    LayoutNodeInternal(
                        child,
                        childWidth,
                        childHeight,
                        direction,
                        childWidthMeasureMode,
                        childHeightMeasureMode,
                        parentWidth,
                        parentHeight,
                        false
                    );

                    child.NodeLayout.ComputedFlexBasis = Fmaxf(
                        child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]],
                        NodePaddingAndBorderForAxis(child, mainAxis, parentWidth)
                    );
                    break;
                }
            }
    }

    internal static void NodeAbsoluteLayoutChild<TStorage>(
        Node<TStorage> node,
        Node<TStorage> child,
        float width,
        MeasureMode widthMode,
        float height,
        Direction direction
    )
        where TStorage : IList<Node<TStorage>>
    {
        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);
        var crossAxis = FlexDirectionCross(mainAxis, direction);
        var isMainAxisRow = FlexDirectionIsRow(mainAxis);

        var childWidth = float.NaN;
        var childHeight = float.NaN;

        var marginRow = NodeMarginForAxis(child, FlexDirection.Row, width);
        var marginColumn = NodeMarginForAxis(child, FlexDirection.Column, width);

        if (NodeIsStyleDimDefined(child, FlexDirection.Row, width))
        {
            childWidth = ResolveValue(child.ResolvedDimensions[(int)Dimension.Width], width) + marginRow;
        }
        else
        {
            // If the child doesn't have a specified width, compute the width based
            // on the left/right
            // offsets if they're defined.
            if (NodeIsLeadingPosDefined(child, FlexDirection.Row) && NodeIsTrailingPosDefined(child, FlexDirection.Row))
            {
                childWidth =
                    node.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                    - (NodeLeadingBorder(node, FlexDirection.Row) + NodeTrailingBorder(node, FlexDirection.Row))
                    - (
                        NodeLeadingPosition(child, FlexDirection.Row, width)
                        + NodeTrailingPosition(child, FlexDirection.Row, width)
                    );
                childWidth = NodeBoundAxis(child, FlexDirection.Row, childWidth, width, width);
            }
        }

        if (NodeIsStyleDimDefined(child, FlexDirection.Column, height))
        {
            childHeight = ResolveValue(child.ResolvedDimensions[(int)Dimension.Height], height) + marginColumn;
        }
        else
        {
            // If the child doesn't have a specified height, compute the height
            // based on the top/bottom
            // offsets if they're defined.
            if (
                NodeIsLeadingPosDefined(child, FlexDirection.Column)
                && NodeIsTrailingPosDefined(child, FlexDirection.Column)
            )
            {
                childHeight =
                    node.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                    - (NodeLeadingBorder(node, FlexDirection.Column) + NodeTrailingBorder(node, FlexDirection.Column))
                    - (
                        NodeLeadingPosition(child, FlexDirection.Column, height)
                        + NodeTrailingPosition(child, FlexDirection.Column, height)
                    );
                childHeight = NodeBoundAxis(child, FlexDirection.Column, childHeight, height, width);
            }
        }

        // Exactly one dimension needs to be defined for us to be able to do aspect ratio
        // calculation. One dimension being the anchor and the other being flexible.
        if (FloatIsUndefined(childWidth) != FloatIsUndefined(childHeight))
        {
            if (!FloatIsUndefined(child.NodeStyle.AspectRatio))
            {
                if (FloatIsUndefined(childWidth))
                {
                    childWidth =
                        marginRow
                        + Fmaxf(
                            (childHeight - marginColumn) * child.NodeStyle.AspectRatio,
                            NodePaddingAndBorderForAxis(child, FlexDirection.Column, width)
                        );
                }
                else if (FloatIsUndefined(childHeight))
                {
                    childHeight =
                        marginColumn
                        + Fmaxf(
                            (childWidth - marginRow) / child.NodeStyle.AspectRatio,
                            NodePaddingAndBorderForAxis(child, FlexDirection.Row, width)
                        );
                }
            }
        }

        // If we're still missing one or the other dimension, measure the content.
        if (FloatIsUndefined(childWidth) || FloatIsUndefined(childHeight))
        {
            var childWidthMeasureMode = MeasureMode.Exactly;
            if (FloatIsUndefined(childWidth))
            {
                childWidthMeasureMode = MeasureMode.Undefined;
            }

            var childHeightMeasureMode = MeasureMode.Exactly;
            if (FloatIsUndefined(childHeight))
            {
                childHeightMeasureMode = MeasureMode.Undefined;
            }

            // If the size of the parent is defined then try to rain the absolute child to that size
            // as well. This allows text within the absolute child to wrap to the size of its parent.
            // This is the same behavior as many browsers implement.
            if (!isMainAxisRow && FloatIsUndefined(childWidth) && widthMode != MeasureMode.Undefined && width > 0)
            {
                childWidth = width;
                childWidthMeasureMode = MeasureMode.AtMost;
            }

            LayoutNodeInternal(
                child,
                childWidth,
                childHeight,
                direction,
                childWidthMeasureMode,
                childHeightMeasureMode,
                childWidth,
                childHeight,
                false
            );
            childWidth =
                child.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                + NodeMarginForAxis(child, FlexDirection.Row, width);
            childHeight =
                child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                + NodeMarginForAxis(child, FlexDirection.Column, width);
        }

        LayoutNodeInternal(
            child,
            childWidth,
            childHeight,
            direction,
            MeasureMode.Exactly,
            MeasureMode.Exactly,
            childWidth,
            childHeight,
            true
        );

        if (NodeIsTrailingPosDefined(child, mainAxis) && !NodeIsLeadingPosDefined(child, mainAxis))
        {
            var axisSize = height;
            if (isMainAxisRow)
            {
                axisSize = width;
            }

            child.NodeLayout.Position[(int)Leading[(int)mainAxis]] =
                node.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]]
                - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]]
                - NodeTrailingBorder(node, mainAxis)
                - NodeTrailingMargin(child, mainAxis, width)
                - NodeTrailingPosition(child, mainAxis, axisSize);
        }
        else if (!NodeIsLeadingPosDefined(child, mainAxis) && node.NodeStyle.JustifyContent == Justify.Center)
        {
            child.NodeLayout.Position[(int)Leading[(int)mainAxis]] =
                (
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]]
                    - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]]
                ) / 2.0f;
        }
        else if (!NodeIsLeadingPosDefined(child, mainAxis) && node.NodeStyle.JustifyContent == Justify.End)
        {
            child.NodeLayout.Position[(int)Leading[(int)mainAxis]] =
                node.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]]
                - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]];
        }

        if (NodeIsTrailingPosDefined(child, crossAxis) && !NodeIsLeadingPosDefined(child, crossAxis))
        {
            var axisSize = width;
            if (isMainAxisRow)
            {
                axisSize = height;
            }

            child.NodeLayout.Position[(int)Leading[(int)crossAxis]] =
                node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                - NodeTrailingBorder(node, crossAxis)
                - NodeTrailingMargin(child, crossAxis, width)
                - NodeTrailingPosition(child, crossAxis, axisSize);
        }
        else if (!NodeIsLeadingPosDefined(child, crossAxis) && NodeAlignItem(node, child) == Align.Center)
        {
            child.NodeLayout.Position[(int)Leading[(int)crossAxis]] =
                (
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                    - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                ) / 2.0f;
        }
        else if (
            !NodeIsLeadingPosDefined(child, crossAxis)
            && NodeAlignItem(node, child) == Align.End != (node.NodeStyle.FlexWrap == Wrap.WrapReverse)
        )
        {
            child.NodeLayout.Position[(int)Leading[(int)crossAxis]] =
                node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]];
        }
    }

    // nodeWithMeasureFuncSetMeasuredDimensions sets measure dimensions for node with measure func
    internal static void NodeWithMeasureFuncSetMeasuredDimensions<TStorage>(
        Node<TStorage> node,
        float availableWidth,
        float availableHeight,
        MeasureMode widthMeasureMode,
        MeasureMode heightMeasureMode,
        float parentWidth,
        float parentHeight
    )
        where TStorage : IList<Node<TStorage>>
    {
        Debug.Assert(node.MeasureFunc != null, "Expected node to have custom measure function");

        var paddingAndBorderAxisRow = NodePaddingAndBorderForAxis(node, FlexDirection.Row, availableWidth);
        var paddingAndBorderAxisColumn = NodePaddingAndBorderForAxis(node, FlexDirection.Column, availableWidth);
        var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, availableWidth);
        var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, availableWidth);

        // We want to make sure we don't call measure with negative size
        var innerWidth = Fmaxf(0, availableWidth - marginAxisRow - paddingAndBorderAxisRow);
        if (FloatIsUndefined(availableWidth))
        {
            innerWidth = availableWidth;
        }

        var innerHeight = Fmaxf(0, availableHeight - marginAxisColumn - paddingAndBorderAxisColumn);
        if (FloatIsUndefined(availableHeight))
        {
            innerHeight = availableHeight;
        }

        if (widthMeasureMode == MeasureMode.Exactly && heightMeasureMode == MeasureMode.Exactly)
        {
            // Don't bother sizing the text if both dimensions are already defined.
            node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
                node,
                FlexDirection.Row,
                availableWidth - marginAxisRow,
                parentWidth,
                parentWidth
            );
            node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
                node,
                FlexDirection.Column,
                availableHeight - marginAxisColumn,
                parentHeight,
                parentWidth
            );
        }
        else
        {
            // Measure the text under the current raints.
            var measuredSize = node.MeasureFunc!(node, innerWidth, widthMeasureMode, innerHeight, heightMeasureMode);

            var width = availableWidth - marginAxisRow;
            if (widthMeasureMode is MeasureMode.Undefined or MeasureMode.AtMost)
            {
                width = measuredSize.Width + paddingAndBorderAxisRow;
            }

            node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
                node,
                FlexDirection.Row,
                width,
                availableWidth,
                availableWidth
            );

            var height = availableHeight - marginAxisColumn;
            if (heightMeasureMode is MeasureMode.Undefined or MeasureMode.AtMost)
            {
                height = measuredSize.Height + paddingAndBorderAxisColumn;
            }

            node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
                node,
                FlexDirection.Column,
                height,
                availableHeight,
                availableWidth
            );
        }
    }

    // nodeEmptyContainerSetMeasuredDimensions sets measure dimensions for empty container
    // For nodes with no children, use the available values if they were provided,
    // or the minimum size as indicated by the padding and border sizes.
    internal static void NodeEmptyContainerSetMeasuredDimensions<TStorage>(
        Node<TStorage> node,
        float availableWidth,
        float availableHeight,
        MeasureMode widthMeasureMode,
        MeasureMode heightMeasureMode,
        float parentWidth,
        float parentHeight
    )
        where TStorage : IList<Node<TStorage>>
    {
        var paddingAndBorderAxisRow = NodePaddingAndBorderForAxis(node, FlexDirection.Row, parentWidth);
        var paddingAndBorderAxisColumn = NodePaddingAndBorderForAxis(node, FlexDirection.Column, parentWidth);
        var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, parentWidth);
        var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, parentWidth);

        var width = availableWidth - marginAxisRow;
        if (widthMeasureMode is MeasureMode.Undefined or MeasureMode.AtMost)
        {
            width = paddingAndBorderAxisRow;
        }

        node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
            node,
            FlexDirection.Row,
            width,
            parentWidth,
            parentWidth
        );

        var height = availableHeight - marginAxisColumn;
        if (heightMeasureMode is MeasureMode.Undefined or MeasureMode.AtMost)
        {
            height = paddingAndBorderAxisColumn;
        }

        node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
            node,
            FlexDirection.Column,
            height,
            parentHeight,
            parentWidth
        );
    }

    internal static bool NodeFixedSizeSetMeasuredDimensions<TStorage>(
        Node<TStorage> node,
        float availableWidth,
        float availableHeight,
        MeasureMode widthMeasureMode,
        MeasureMode heightMeasureMode,
        float parentWidth,
        float parentHeight
    )
        where TStorage : IList<Node<TStorage>>
    {
        if (
            (widthMeasureMode == MeasureMode.AtMost && availableWidth <= 0)
            || (heightMeasureMode == MeasureMode.AtMost && availableHeight <= 0)
            || (widthMeasureMode == MeasureMode.Exactly && heightMeasureMode == MeasureMode.Exactly)
        )
        {
            var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, parentWidth);
            var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, parentWidth);

            var width = availableWidth - marginAxisRow;
            if (FloatIsUndefined(availableWidth) || (widthMeasureMode == MeasureMode.AtMost && availableWidth < 0))
            {
                width = 0;
            }

            node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
                node,
                FlexDirection.Row,
                width,
                parentWidth,
                parentWidth
            );

            var height = availableHeight - marginAxisColumn;
            if (FloatIsUndefined(availableHeight) || (heightMeasureMode == MeasureMode.AtMost && availableHeight < 0))
            {
                height = 0;
            }

            node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
                node,
                FlexDirection.Column,
                height,
                parentHeight,
                parentWidth
            );

            return true;
        }

        return false;
    }

    // ZeroOutLayoutRecursively zeros out layout recursively
    internal static void ZeroOutLayoutRecursively<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        node.NodeLayout.Dimensions[(int)Dimension.Height] = 0;
        node.NodeLayout.Dimensions[(int)Dimension.Width] = 0;
        node.NodeLayout.Position[(int)Edge.Top] = 0;
        node.NodeLayout.Position[(int)Edge.Bottom] = 0;
        node.NodeLayout.Position[(int)Edge.Left] = 0;
        node.NodeLayout.Position[(int)Edge.Right] = 0;
        node.NodeLayout.CachedLayout.AvailableHeight = 0;
        node.NodeLayout.CachedLayout.AvailableWidth = 0;
        node.NodeLayout.CachedLayout.HeightMeasureMode = MeasureMode.Exactly;
        node.NodeLayout.CachedLayout.WidthMeasureMode = MeasureMode.Exactly;
        node.NodeLayout.CachedLayout.ComputedWidth = 0;
        node.NodeLayout.CachedLayout.ComputedHeight = 0;
        foreach (var child in node)
        {
            ZeroOutLayoutRecursively(child);
        }
    }

    // This is the main routine that implements a subset of the flexbox layout
    // algorithm
    // described in the W3C YG documentation: https://www.w3.org/TR/YG3-flexbox/.
    //
    // Limitations of this algorithm, compared to the full standard:
    //  * Display property is always assumed to be 'flex' except for Text nodes,
    //  which
    //    are assumed to be 'inline-flex'.
    //  * The 'zIndex' property (or any form of z ordering) is not supported. Nodes
    //  are
    //    stacked in document order.
    //  * The 'order' property is not supported. The order of flex items is always
    //  defined
    //    by document order.
    //  * The 'visibility' property is always assumed to be 'visible'. Values of
    //  'collapse'
    //    and 'hidden' are not supported.
    //  * There is no support for forced breaks.
    //  * It does not support vertical inline directions (top-to-bottom or
    //  bottom-to-top text).
    //
    // Deviations from standard:
    //  * Section 4.5 of the spec indicates that all flex items have a default
    //  minimum
    //    main size. For text blocks, for example, this is the width of the widest
    //    word.
    //    Calculating the minimum width is expensive, so we forego it and assume a
    //    default
    //    minimum main size of 0.
    //  * Min/Max sizes in the main axis are not honored when resolving flexible
    //  lengths.
    //  * The spec indicates that the default value for 'flexDirection' is 'row',
    //  but
    //    the algorithm below assumes a default of 'column'.
    //
    // Input parameters:
    //    - node: current node to be sized and layed out
    //    - availableWidth & availableHeight: available size to be used for sizing
    //    the node
    //      or Undefined if the size is not available; interpretation depends on
    //      layout
    //      flags
    //    - parentDirection: the inline (text) direction within the parent
    //    (left-to-right or
    //      right-to-left)
    //    - widthMeasureMode: indicates the sizing rules for the width (see below
    //    for explanation)
    //    - heightMeasureMode: indicates the sizing rules for the height (see below
    //    for explanation)
    //    - performLayout: specifies whether the caller is interested in just the
    //    dimensions
    //      of the node or it requires the entire node and its subtree to be layed
    //      out
    //      (with final positions)
    //
    // Details:
    //    This routine is called recursively to lay out subtrees of flexbox
    //    elements. It uses the
    //    information in node.style, which is treated as a read-only input. It is
    //    responsible for
    //    setting the layout.direction and layout.measuredDimensions fields for the
    //    input node as well
    //    as the layout.position and layout.lineIndex fields for its child nodes.
    //    The
    //    layout.measuredDimensions field includes any border or padding for the
    //    node but does
    //    not include margins.
    //
    //    The spec describes four different layout modes: "fill available", "max
    //    content", "min
    //    content",
    //    and "fit content". Of these, we don't use "min content" because we don't
    //    support default
    //    minimum main sizes (see above for details). Each of our measure modes maps
    //    to a layout mode
    //    from the spec (https://www.w3.org/TR/YG3-sizing/#terms):
    //      - YGMeasureModeUndefined: max content
    //      - YGMeasureModeExactly: fill available
    //      - YGMeasureModeAtMost: fit content
    //
    //    When calling nodelayoutImpl and YGLayoutNodeInternal, if the caller passes
    //    an available size of
    //    undefined then it must also pass a measure mode of YGMeasureModeUndefined
    //    in that dimension.
    internal static void NodeLayoutImpl<TStorage>(
        Node<TStorage> node,
        float availableWidth,
        float availableHeight,
        Direction parentDirection,
        MeasureMode widthMeasureMode,
        MeasureMode heightMeasureMode,
        float parentWidth,
        float parentHeight,
        bool performLayout
    )
        where TStorage : IList<Node<TStorage>>
    {
        // Set the resolved resolution in the node's layout.
        var direction = NodeResolveDirection(node, parentDirection);
        node.NodeLayout.Direction = direction;

        var flexRowDirection = ResolveFlexDirection(FlexDirection.Row, direction);
        var flexColumnDirection = ResolveFlexDirection(FlexDirection.Column, direction);

        node.NodeLayout.Margin[(int)Edge.Start] = NodeLeadingMargin(node, flexRowDirection, parentWidth);
        node.NodeLayout.Margin[(int)Edge.End] = NodeTrailingMargin(node, flexRowDirection, parentWidth);
        node.NodeLayout.Margin[(int)Edge.Top] = NodeLeadingMargin(node, flexColumnDirection, parentWidth);
        node.NodeLayout.Margin[(int)Edge.Bottom] = NodeTrailingMargin(node, flexColumnDirection, parentWidth);

        node.NodeLayout.Border[(int)Edge.Start] = NodeLeadingBorder(node, flexRowDirection);
        node.NodeLayout.Border[(int)Edge.End] = NodeTrailingBorder(node, flexRowDirection);
        node.NodeLayout.Border[(int)Edge.Top] = NodeLeadingBorder(node, flexColumnDirection);
        node.NodeLayout.Border[(int)Edge.Bottom] = NodeTrailingBorder(node, flexColumnDirection);

        node.NodeLayout.Padding[(int)Edge.Start] = NodeLeadingPadding(node, flexRowDirection, parentWidth);
        node.NodeLayout.Padding[(int)Edge.End] = NodeTrailingPadding(node, flexRowDirection, parentWidth);
        node.NodeLayout.Padding[(int)Edge.Top] = NodeLeadingPadding(node, flexColumnDirection, parentWidth);
        node.NodeLayout.Padding[(int)Edge.Bottom] = NodeTrailingPadding(node, flexColumnDirection, parentWidth);

        if (node.MeasureFunc != null)
        {
            NodeWithMeasureFuncSetMeasuredDimensions(
                node,
                availableWidth,
                availableHeight,
                widthMeasureMode,
                heightMeasureMode,
                parentWidth,
                parentHeight
            );
            return;
        }

        var childCount = node.Storage.Count;
        if (childCount == 0)
        {
            NodeEmptyContainerSetMeasuredDimensions(
                node,
                availableWidth,
                availableHeight,
                widthMeasureMode,
                heightMeasureMode,
                parentWidth,
                parentHeight
            );
            return;
        }

        // If we're not being asked to perform a full layout we can skip the algorithm if we already know
        // the size
        if (
            !performLayout
            && NodeFixedSizeSetMeasuredDimensions(
                node,
                availableWidth,
                availableHeight,
                widthMeasureMode,
                heightMeasureMode,
                parentWidth,
                parentHeight
            )
        )
        {
            return;
        }

        // Reset layout flags, as they could have changed.
        node.NodeLayout.HadOverflow = false;

        // STEP 1: CALCULATE VALUES FOR REMAINDER OF ALGORITHM
        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);
        var crossAxis = FlexDirectionCross(mainAxis, direction);
        var isMainAxisRow = FlexDirectionIsRow(mainAxis);
        var justifyContent = node.NodeStyle.JustifyContent;
        var isNodeFlexWrap = node.NodeStyle.FlexWrap != Wrap.NoWrap;

        var mainAxisParentSize = parentHeight;
        var crossAxisParentSize = parentWidth;
        if (isMainAxisRow)
        {
            mainAxisParentSize = parentWidth;
            crossAxisParentSize = parentHeight;
        }

        Node<TStorage>? firstAbsoluteChild = null;
        Node<TStorage>? currentAbsoluteChild = null;

        var leadingPaddingAndBorderMain = NodeLeadingPaddingAndBorder(node, mainAxis, parentWidth);
        var trailingPaddingAndBorderMain = NodeTrailingPaddingAndBorder(node, mainAxis, parentWidth);
        var leadingPaddingAndBorderCross = NodeLeadingPaddingAndBorder(node, crossAxis, parentWidth);
        var paddingAndBorderAxisMain = NodePaddingAndBorderForAxis(node, mainAxis, parentWidth);
        var paddingAndBorderAxisCross = NodePaddingAndBorderForAxis(node, crossAxis, parentWidth);

        var measureModeMainDim = heightMeasureMode;
        var measureModeCrossDim = widthMeasureMode;

        if (isMainAxisRow)
        {
            measureModeMainDim = widthMeasureMode;
            measureModeCrossDim = heightMeasureMode;
        }

        var paddingAndBorderAxisRow = paddingAndBorderAxisCross;
        var paddingAndBorderAxisColumn = paddingAndBorderAxisMain;
        if (isMainAxisRow)
        {
            paddingAndBorderAxisRow = paddingAndBorderAxisMain;
            paddingAndBorderAxisColumn = paddingAndBorderAxisCross;
        }

        var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, parentWidth);
        var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, parentWidth);

        // STEP 2: DETERMINE AVAILABLE SIZE IN MAIN AND CROSS DIRECTIONS
        var minInnerWidth =
            ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Width], parentWidth)
            - marginAxisRow
            - paddingAndBorderAxisRow;
        var maxInnerWidth =
            ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Width], parentWidth)
            - marginAxisRow
            - paddingAndBorderAxisRow;
        var minInnerHeight =
            ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Height], parentHeight)
            - marginAxisColumn
            - paddingAndBorderAxisColumn;
        var maxInnerHeight =
            ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Height], parentHeight)
            - marginAxisColumn
            - paddingAndBorderAxisColumn;

        var minInnerMainDim = minInnerHeight;
        var maxInnerMainDim = maxInnerHeight;
        if (isMainAxisRow)
        {
            minInnerMainDim = minInnerWidth;
            maxInnerMainDim = maxInnerWidth;
        }

        // Max dimension overrides predefined dimension value; Min dimension in turn overrides both of the
        // above
        var availableInnerWidth = availableWidth - marginAxisRow - paddingAndBorderAxisRow;
        if (!FloatIsUndefined(availableInnerWidth))
        {
            // We want to make sure our available width does not violate min and max raints
            availableInnerWidth = Fmaxf(Fminf(availableInnerWidth, maxInnerWidth), minInnerWidth);
        }

        var availableInnerHeight = availableHeight - marginAxisColumn - paddingAndBorderAxisColumn;
        if (!FloatIsUndefined(availableInnerHeight))
        {
            // We want to make sure our available height does not violate min and max raints
            availableInnerHeight = Fmaxf(Fminf(availableInnerHeight, maxInnerHeight), minInnerHeight);
        }

        var availableInnerMainDim = availableInnerHeight;
        var availableInnerCrossDim = availableInnerWidth;
        if (isMainAxisRow)
        {
            availableInnerMainDim = availableInnerWidth;
            availableInnerCrossDim = availableInnerHeight;
        }

        // Spacing inserted between flex items (main axis) and between flex lines
        // (cross axis). Resolved once against the container's inner size.
        var mainAxisGap = NodeResolveGap(node, mainAxis, availableInnerMainDim);
        var crossAxisGap = NodeResolveGap(node, crossAxis, availableInnerCrossDim);

        // If there is only one child with flexGrow + flexShrink it means we can set the
        // computedFlexBasis to 0 instead of measuring and shrinking / flexing the child to exactly
        // match the remaining space
        Node<TStorage>? singleFlexChild = null;
        if (measureModeMainDim == MeasureMode.Exactly)
        {
            foreach (var child in node)
            {
                if (singleFlexChild != null)
                {
                    if (NodeIsFlex(child))
                    {
                        // There is already a flexible child, abort.
                        singleFlexChild = null;
                        break;
                    }
                }
                else if (ResolveFlexGrow(child) > 0 && NodeResolveFlexShrink(child) > 0)
                {
                    singleFlexChild = child;
                }
            }
        }

        float totalOuterFlexBasis = 0;

        // STEP 3: DETERMINE FLEX BASIS FOR EACH ITEM
        foreach (var child in node)
        {
            if (child.NodeStyle.Display == Display.None)
            {
                ZeroOutLayoutRecursively(child);
                child.IsDirty = false;
                continue;
            }

            ResolveDimensions(child);
            if (performLayout)
            {
                // Set the initial position (relative to the parent).
                var childDirection = NodeResolveDirection(child, direction);
                NodeSetPosition(
                    child,
                    childDirection,
                    availableInnerMainDim,
                    availableInnerCrossDim,
                    availableInnerWidth
                );
            }

            // Absolute-positioned children don't participate in flex layout. Add them
            // to a list that we can process later.
            if (child.NodeStyle.PositionType == PositionType.Absolute)
            {
                // Store a private linked list of absolutely positioned children
                // so that we can efficiently traverse them later.
                firstAbsoluteChild ??= child;

                currentAbsoluteChild?.NextChild = child;

                currentAbsoluteChild = child;
                child.NextChild = null;
            }
            else
            {
                if (child == singleFlexChild)
                {
                    child.NodeLayout.ComputedFlexBasis = 0;
                }
                else
                {
                    NodeComputeFlexBasisForChild(
                        node,
                        child,
                        availableInnerWidth,
                        widthMeasureMode,
                        availableInnerHeight,
                        availableInnerWidth,
                        availableInnerHeight,
                        heightMeasureMode,
                        direction
                    );
                }
            }

            totalOuterFlexBasis +=
                child.NodeLayout.ComputedFlexBasis + NodeMarginForAxis(child, mainAxis, availableInnerWidth);
        }

        var flexBasisOverflows = totalOuterFlexBasis > availableInnerMainDim;
        if (measureModeMainDim == MeasureMode.Undefined)
        {
            flexBasisOverflows = false;
        }

        if (isNodeFlexWrap && flexBasisOverflows && measureModeMainDim == MeasureMode.AtMost)
        {
            measureModeMainDim = MeasureMode.Exactly;
        }

        // STEP 4: COLLECT FLEX ITEMS INTO FLEX LINES

        // Indexes of children that represent the first and last items in the line.
        var startOfLineIndex = 0;
        var endOfLineIndex = 0;

        // Number of lines.
        var lineCount = 0;

        // Accumulated cross dimensions of all lines so far.
        float totalLineCrossDim = 0;

        // Max main dimension of all the lines.
        float maxLineMainDim = 0;

        while (endOfLineIndex < childCount)
        {
            // Insert the cross-axis gap ahead of every line after the first. Done
            // before the line is positioned so single-line cross alignment (STEP 7)
            // accounts for it when the container is auto-sized on the cross axis.
            if (lineCount > 0)
            {
                totalLineCrossDim += crossAxisGap;
            }

            // Number of items on the currently line. May be different from the
            // difference
            // between start and end indicates because we skip over absolute-positioned
            // items.
            var itemsOnLine = 0;

            // sizeConsumedOnCurrentLine is accumulation of the dimensions and margin
            // of all the children on the current line. This will be used in order to
            // either set the dimensions of the node if none already exist or to compute
            // the remaining space left for the flexible children.
            float sizeConsumedOnCurrentLine = 0;
            float sizeConsumedOnCurrentLineIncludingMinConstraint = 0;

            float totalFlexGrowFactors = 0;
            float totalFlexShrinkScaledFactors = 0;

            // Maintain a linked list of the child nodes that can shrink and/or grow.
            Node<TStorage>? firstRelativeChild = null;
            Node<TStorage>? currentRelativeChild = null;

            // Add items to the current line until it's full, or we run out of items.
            for (var i = startOfLineIndex; i < childCount; i++)
            {
                var child = node.Storage[i];
                if (child.NodeStyle.Display == Display.None)
                {
                    endOfLineIndex++;
                    continue;
                }

                child.LineIndex = lineCount;

                if (child.NodeStyle.PositionType != PositionType.Absolute)
                {
                    var childMarginMainAxis = NodeMarginForAxis(child, mainAxis, availableInnerWidth);

                    // Every item except the first on a line is preceded by the
                    // main-axis gap.
                    var childLeadingGapMainAxis = itemsOnLine > 0 ? mainAxisGap : 0;
                    var flexBasisWithMaxConstraints = Fminf(
                        ResolveValue(child.NodeStyle.MaxDimensions[(int)Dim[(int)mainAxis]], mainAxisParentSize),
                        child.NodeLayout.ComputedFlexBasis
                    );
                    var flexBasisWithMinAndMaxConstraints = Fmaxf(
                        ResolveValue(child.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]], mainAxisParentSize),
                        flexBasisWithMaxConstraints
                    );

                    // If this is a multi-line flow and this item pushes us over the
                    // available size, we've
                    // hit the end of the current line. Break out of the loop and lay out
                    // the current line.
                    if (
                        sizeConsumedOnCurrentLineIncludingMinConstraint
                            + flexBasisWithMinAndMaxConstraints
                            + childMarginMainAxis
                            + childLeadingGapMainAxis
                            > availableInnerMainDim
                        && isNodeFlexWrap
                        && itemsOnLine > 0
                    )
                    {
                        break;
                    }

                    sizeConsumedOnCurrentLineIncludingMinConstraint +=
                        flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;
                    sizeConsumedOnCurrentLine +=
                        flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;
                    itemsOnLine++;

                    if (NodeIsFlex(child))
                    {
                        totalFlexGrowFactors += ResolveFlexGrow(child);

                        // Unlike the growth factor, the shrink factor is scaled relative to the child dimension.
                        totalFlexShrinkScaledFactors +=
                            -NodeResolveFlexShrink(child) * child.NodeLayout.ComputedFlexBasis;
                    }

                    // Store a private linked list of children that need to be layed out.
                    firstRelativeChild ??= child;

                    currentRelativeChild?.NextChild = child;

                    currentRelativeChild = child;
                    child.NextChild = null;
                }

                endOfLineIndex++;
            }

            // The total flex factor needs to be floored to 1.
            if (totalFlexGrowFactors is > 0 and < 1)
            {
                totalFlexGrowFactors = 1;
            }

            // The total flex shrink factor needs to be floored to 1.
            if (totalFlexShrinkScaledFactors is > 0 and < 1)
            {
                totalFlexShrinkScaledFactors = 1;
            }

            // If we don't need to measure the cross axis, we can skip the entire flex
            // step.
            var canSkipFlex = !performLayout && measureModeCrossDim == MeasureMode.Exactly;

            // In order to position the elements in the main axis, we have two
            // controls. The space between the beginning and the first element
            // and the space between each two elements.
            float leadingMainDim = 0;
            float betweenMainDim = 0;

            // STEP 5: RESOLVING FLEXIBLE LENGTHS ON MAIN AXIS
            // Calculate the remaining available space that needs to be allocated.
            // If the main dimension size isn't known, it is computed based on
            // the line length, so there's no more space left to distribute.

            // If we don't measure with exact main dimension we want to ensure we don't violate min and max
            if (measureModeMainDim != MeasureMode.Exactly)
            {
                if (!FloatIsUndefined(minInnerMainDim) && sizeConsumedOnCurrentLine < minInnerMainDim)
                {
                    availableInnerMainDim = minInnerMainDim;
                }
                else if (!FloatIsUndefined(maxInnerMainDim) && sizeConsumedOnCurrentLine > maxInnerMainDim)
                {
                    availableInnerMainDim = maxInnerMainDim;
                }
                else
                {
                    if (totalFlexGrowFactors == 0 || ResolveFlexGrow(node) == 0)
                    {
                        // If we don't have any children to flex, or we can't flex the node itself,
                        // space we've used is all space we need. Root node also should be shrunk to minimum
                        availableInnerMainDim = sizeConsumedOnCurrentLine;
                    }
                }
            }

            float remainingFreeSpace = 0;
            if (!FloatIsUndefined(availableInnerMainDim))
            {
                remainingFreeSpace = availableInnerMainDim - sizeConsumedOnCurrentLine;
            }
            else if (sizeConsumedOnCurrentLine < 0)
            {
                // availableInnerMainDim is indefinite which means the node is being sized based on its
                // content.
                // sizeConsumedOnCurrentLine is negative which means the node will allocate 0 points for
                // its content. Consequently, remainingFreeSpace is 0 - sizeConsumedOnCurrentLine.
                remainingFreeSpace = -sizeConsumedOnCurrentLine;
            }

            var originalRemainingFreeSpace = remainingFreeSpace;
            float deltaFreeSpace = 0;

            if (!canSkipFlex)
            {
                float childFlexBasis;
                float flexShrinkScaledFactor;
                float flexGrowFactor;

                // Do two passes over the flex items to figure out how to distribute the
                // remaining space.
                // The first pass finds the items whose min/max raints trigger,
                // freezes them at those
                // sizes, and excludes those sizes from the remaining space. The second
                // pass sets the size
                // of each flexible item. It distributes the remaining space amongst the
                // items whose min/max
                // raints didn't trigger in pass 1. For the other items, it sets
                // their sizes by forcing
                // their min/max raints to trigger again.
                //
                // This two pass approach for resolving min/max raints deviates from
                // the spec. The
                // spec (https://www.w3.org/TR/YG-flexbox-1/#resolve-flexible-lengths)
                // describes a process
                // that needs to be repeated a variable number of times. The algorithm
                // implemented here
                // won't handle all cases, but it was simpler to implement, and it mitigates
                // performance
                // concerns because we know exactly how many passes it'll do.

                // First pass: detect the flex items whose min/max raints trigger
                float deltaFlexShrinkScaledFactors = 0;
                float deltaFlexGrowFactors = 0;
                currentRelativeChild = firstRelativeChild;
                while (currentRelativeChild != null)
                {
                    childFlexBasis = Fminf(
                        ResolveValue(
                            currentRelativeChild.NodeStyle.MaxDimensions[(int)Dim[(int)mainAxis]],
                            mainAxisParentSize
                        ),
                        Fmaxf(
                            ResolveValue(
                                currentRelativeChild.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]],
                                mainAxisParentSize
                            ),
                            currentRelativeChild.NodeLayout.ComputedFlexBasis
                        )
                    );

                    float baseMainSize;
                    float boundMainSize;
                    switch (remainingFreeSpace)
                    {
                        case < 0:
                        {
                            flexShrinkScaledFactor = -NodeResolveFlexShrink(currentRelativeChild) * childFlexBasis;

                            // Is this child able to shrink?
                            if (flexShrinkScaledFactor != 0)
                            {
                                baseMainSize =
                                    childFlexBasis
                                    + remainingFreeSpace / totalFlexShrinkScaledFactors * flexShrinkScaledFactor;
                                boundMainSize = NodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    baseMainSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                                if (baseMainSize != boundMainSize)
                                {
                                    // By excluding this item's size and flex factor from remaining,
                                    // this item's
                                    // min/max raints should also trigger in the second pass
                                    // resulting in the
                                    // item's size calculation being identical in the first and second
                                    // passes.
                                    deltaFreeSpace -= boundMainSize - childFlexBasis;
                                    deltaFlexShrinkScaledFactors -= flexShrinkScaledFactor;
                                }
                            }

                            break;
                        }
                        case > 0:
                        {
                            flexGrowFactor = ResolveFlexGrow(currentRelativeChild);

                            // Is this child able to grow?
                            if (flexGrowFactor != 0)
                            {
                                baseMainSize =
                                    childFlexBasis + remainingFreeSpace / totalFlexGrowFactors * flexGrowFactor;
                                boundMainSize = NodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    baseMainSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );

                                if (baseMainSize != boundMainSize)
                                {
                                    // By excluding this item's size and flex factor from remaining,
                                    // this item's
                                    // min/max raints should also trigger in the second pass
                                    // resulting in the
                                    // item's size calculation being identical in the first and second
                                    // passes.
                                    deltaFreeSpace -= boundMainSize - childFlexBasis;
                                    deltaFlexGrowFactors -= flexGrowFactor;
                                }
                            }

                            break;
                        }
                    }

                    currentRelativeChild = currentRelativeChild.NextChild;
                }

                totalFlexShrinkScaledFactors += deltaFlexShrinkScaledFactors;
                totalFlexGrowFactors += deltaFlexGrowFactors;
                remainingFreeSpace += deltaFreeSpace;

                // Second pass: resolve the sizes of the flexible items
                deltaFreeSpace = 0;
                currentRelativeChild = firstRelativeChild;
                while (currentRelativeChild != null)
                {
                    childFlexBasis = Fminf(
                        ResolveValue(
                            currentRelativeChild.NodeStyle.MaxDimensions[(int)Dim[(int)mainAxis]],
                            mainAxisParentSize
                        ),
                        Fmaxf(
                            ResolveValue(
                                currentRelativeChild.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]],
                                mainAxisParentSize
                            ),
                            currentRelativeChild.NodeLayout.ComputedFlexBasis
                        )
                    );
                    var updatedMainSize = childFlexBasis;

                    switch (remainingFreeSpace)
                    {
                        case < 0:
                        {
                            flexShrinkScaledFactor = -NodeResolveFlexShrink(currentRelativeChild) * childFlexBasis;
                            // Is this child able to shrink?
                            if (flexShrinkScaledFactor != 0)
                            {
                                float childSize;

                                if (totalFlexShrinkScaledFactors == 0)
                                {
                                    childSize = childFlexBasis + flexShrinkScaledFactor;
                                }
                                else
                                {
                                    childSize =
                                        childFlexBasis
                                        + remainingFreeSpace / totalFlexShrinkScaledFactors * flexShrinkScaledFactor;
                                }

                                updatedMainSize = NodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    childSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                            }

                            break;
                        }
                        case > 0:
                        {
                            flexGrowFactor = ResolveFlexGrow(currentRelativeChild);

                            // Is this child able to grow?
                            if (flexGrowFactor != 0)
                            {
                                updatedMainSize = NodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    childFlexBasis + remainingFreeSpace / totalFlexGrowFactors * flexGrowFactor,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                            }

                            break;
                        }
                    }

                    deltaFreeSpace -= updatedMainSize - childFlexBasis;

                    var marginMain = NodeMarginForAxis(currentRelativeChild, mainAxis, availableInnerWidth);
                    var marginCross = NodeMarginForAxis(currentRelativeChild, crossAxis, availableInnerWidth);

                    float childCrossSize;
                    var childMainSize = updatedMainSize + marginMain;
                    MeasureMode childCrossMeasureMode;
                    var childMainMeasureMode = MeasureMode.Exactly;

                    if (
                        !FloatIsUndefined(availableInnerCrossDim)
                        && !NodeIsStyleDimDefined(currentRelativeChild, crossAxis, availableInnerCrossDim)
                        && measureModeCrossDim == MeasureMode.Exactly
                        && !(isNodeFlexWrap && flexBasisOverflows)
                        && NodeAlignItem(node, currentRelativeChild) == Align.Stretch
                    )
                    {
                        childCrossSize = availableInnerCrossDim;
                        childCrossMeasureMode = MeasureMode.Exactly;
                    }
                    else if (!NodeIsStyleDimDefined(currentRelativeChild, crossAxis, availableInnerCrossDim))
                    {
                        childCrossSize = availableInnerCrossDim;
                        childCrossMeasureMode = MeasureMode.AtMost;
                        if (FloatIsUndefined(childCrossSize))
                        {
                            childCrossMeasureMode = MeasureMode.Undefined;
                        }
                    }
                    else
                    {
                        childCrossSize =
                            ResolveValue(
                                currentRelativeChild.ResolvedDimensions[(int)Dim[(int)crossAxis]],
                                availableInnerCrossDim
                            ) + marginCross;
                        var isLoosePercentageMeasurement =
                            currentRelativeChild.ResolvedDimensions[(int)Dim[(int)crossAxis]].Unit == Unit.Percent
                            && measureModeCrossDim != MeasureMode.Exactly;
                        childCrossMeasureMode = MeasureMode.Exactly;
                        if (FloatIsUndefined(childCrossSize) || isLoosePercentageMeasurement)
                        {
                            childCrossMeasureMode = MeasureMode.Undefined;
                        }
                    }

                    if (!FloatIsUndefined(currentRelativeChild.NodeStyle.AspectRatio))
                    {
                        var v = (childMainSize - marginMain) * currentRelativeChild.NodeStyle.AspectRatio;
                        if (isMainAxisRow)
                        {
                            v = (childMainSize - marginMain) / currentRelativeChild.NodeStyle.AspectRatio;
                        }

                        childCrossSize = Fmaxf(
                            v,
                            NodePaddingAndBorderForAxis(currentRelativeChild, crossAxis, availableInnerWidth)
                        );
                        childCrossMeasureMode = MeasureMode.Exactly;

                        // Parent size raint should have higher priority than flex
                        if (NodeIsFlex(currentRelativeChild))
                        {
                            childCrossSize = Fminf(childCrossSize - marginCross, availableInnerCrossDim);
                            childMainSize = marginMain;
                            if (isMainAxisRow)
                            {
                                childMainSize += childCrossSize * currentRelativeChild.NodeStyle.AspectRatio;
                            }
                            else
                            {
                                childMainSize += childCrossSize / currentRelativeChild.NodeStyle.AspectRatio;
                            }
                        }

                        childCrossSize += marginCross;
                    }

                    ConstrainMaxSizeForMode(
                        currentRelativeChild,
                        mainAxis,
                        availableInnerMainDim,
                        availableInnerWidth,
                        ref childMainMeasureMode,
                        ref childMainSize
                    );
                    ConstrainMaxSizeForMode(
                        currentRelativeChild,
                        crossAxis,
                        availableInnerCrossDim,
                        availableInnerWidth,
                        ref childCrossMeasureMode,
                        ref childCrossSize
                    );

                    var requiresStretchLayout =
                        !NodeIsStyleDimDefined(currentRelativeChild, crossAxis, availableInnerCrossDim)
                        && NodeAlignItem(node, currentRelativeChild) == Align.Stretch;

                    var childWidth = childCrossSize;
                    if (isMainAxisRow)
                    {
                        childWidth = childMainSize;
                    }

                    var childHeight = childCrossSize;
                    if (!isMainAxisRow)
                    {
                        childHeight = childMainSize;
                    }

                    var childWidthMeasureMode = childCrossMeasureMode;
                    if (isMainAxisRow)
                    {
                        childWidthMeasureMode = childMainMeasureMode;
                    }

                    var childHeightMeasureMode = childCrossMeasureMode;
                    if (!isMainAxisRow)
                    {
                        childHeightMeasureMode = childMainMeasureMode;
                    }

                    // Recursively call the layout algorithm for this child with the updated
                    // main size.
                    LayoutNodeInternal(
                        currentRelativeChild,
                        childWidth,
                        childHeight,
                        direction,
                        childWidthMeasureMode,
                        childHeightMeasureMode,
                        availableInnerWidth,
                        availableInnerHeight,
                        performLayout && !requiresStretchLayout
                    );
                    if (currentRelativeChild.NodeLayout.HadOverflow)
                    {
                        node.NodeLayout.HadOverflow = true;
                    }

                    currentRelativeChild = currentRelativeChild.NextChild;
                }
            }

            remainingFreeSpace = originalRemainingFreeSpace + deltaFreeSpace;
            if (remainingFreeSpace < 0)
            {
                node.NodeLayout.HadOverflow = true;
            }

            // STEP 6: MAIN-AXIS JUSTIFICATION & CROSS-AXIS SIZE DETERMINATION

            // At this point, all the children have their dimensions set in the main
            // axis.
            // Their dimensions are also set in the cross axis except
            // items
            // that are aligned "stretch". We need to compute these stretch values and
            // set the final positions.

            // If we are using "at most" rules in the main axis. Calculate the remaining space when
            // raint by the min size defined for the main axis.

            if (measureModeMainDim == MeasureMode.AtMost && remainingFreeSpace > 0)
            {
                if (
                    node.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]].Unit != Unit.Undefined
                    && ResolveValue(node.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]], mainAxisParentSize) >= 0
                )
                {
                    remainingFreeSpace = Fmaxf(
                        0,
                        ResolveValue(node.NodeStyle.MinDimensions[(int)Dim[(int)mainAxis]], mainAxisParentSize)
                            - (availableInnerMainDim - remainingFreeSpace)
                    );
                }
                else
                {
                    remainingFreeSpace = 0;
                }
            }

            var numberOfAutoMarginsOnCurrentLine = 0;
            for (var i = startOfLineIndex; i < endOfLineIndex; i++)
            {
                var child = node.Storage[i];
                if (child.NodeStyle.PositionType == PositionType.Relative)
                {
                    if (MarginLeadingValue(child, mainAxis).Unit == Unit.Auto)
                    {
                        numberOfAutoMarginsOnCurrentLine++;
                    }

                    if (MarginTrailingValue(child, mainAxis).Unit == Unit.Auto)
                    {
                        numberOfAutoMarginsOnCurrentLine++;
                    }
                }
            }

            if (numberOfAutoMarginsOnCurrentLine == 0)
            {
                switch (justifyContent)
                {
                    case Justify.Center:
                        leadingMainDim = remainingFreeSpace / 2;
                        break;
                    case Justify.End:
                        leadingMainDim = remainingFreeSpace;
                        break;
                    case Justify.SpaceBetween:
                        if (itemsOnLine > 1)
                        {
                            betweenMainDim = Fmaxf(remainingFreeSpace, 0) / (itemsOnLine - 1);
                        }
                        else
                        {
                            betweenMainDim = 0;
                        }

                        break;
                    case Justify.SpaceAround:
                        // Space on the edges is half of the space between elements
                        betweenMainDim = remainingFreeSpace / itemsOnLine;
                        leadingMainDim = betweenMainDim / 2;
                        break;
                    case Justify.SpaceEvenly:
                        // Space on the edges is half of the space between elements
                        betweenMainDim = remainingFreeSpace / (itemsOnLine + 1);
                        leadingMainDim = betweenMainDim;
                        break;
                    case Justify.Start:
                        break;
                }
            }

            var mainDim = leadingPaddingAndBorderMain + leadingMainDim;
            float crossDim = 0;

            // Tracks whether the next in-flow item is the first on the line so the
            // main-axis gap is only inserted between items, never before the first.
            var isFirstInFlowChildOnLine = true;

            for (var i = startOfLineIndex; i < endOfLineIndex; i++)
            {
                var child = node.Storage[i];
                if (child.NodeStyle.Display == Display.None)
                {
                    continue;
                }

                switch (child.NodeStyle.PositionType)
                {
                    case PositionType.Absolute when NodeIsLeadingPosDefined(child, mainAxis):
                    {
                        if (performLayout)
                        {
                            // In case the child is position absolute and has left/top being
                            // defined, we override the position to whatever the user said
                            // (and margin/border).
                            child.NodeLayout.Position[(int)Pos[(int)mainAxis]] =
                                NodeLeadingPosition(child, mainAxis, availableInnerMainDim)
                                + NodeLeadingBorder(node, mainAxis)
                                + NodeLeadingMargin(child, mainAxis, availableInnerWidth);
                        }

                        break;
                    }
                    // Now that we placed the element, we need to update the variables.
                    // We need to do that only for relative elements. Absolute elements
                    // do not take part in that phase.
                    case PositionType.Relative:
                    {
                        // Insert the main-axis gap ahead of every item after the first.
                        if (!isFirstInFlowChildOnLine)
                        {
                            mainDim += mainAxisGap;
                        }

                        isFirstInFlowChildOnLine = false;

                        if (MarginLeadingValue(child, mainAxis).Unit == Unit.Auto)
                        {
                            mainDim += remainingFreeSpace / numberOfAutoMarginsOnCurrentLine;
                        }

                        if (performLayout)
                        {
                            child.NodeLayout.Position[(int)Pos[(int)mainAxis]] += mainDim;
                        }

                        if (MarginTrailingValue(child, mainAxis).Unit == Unit.Auto)
                        {
                            mainDim += remainingFreeSpace / numberOfAutoMarginsOnCurrentLine;
                        }

                        if (canSkipFlex)
                        {
                            // If we skipped the flex step, then we can't rely on the
                            // measuredDims because
                            // they weren't computed. This means we can't call YGNodeDimWithMargin.
                            mainDim +=
                                betweenMainDim
                                + NodeMarginForAxis(child, mainAxis, availableInnerWidth)
                                + child.NodeLayout.ComputedFlexBasis;
                            crossDim = availableInnerCrossDim;
                        }
                        else
                        {
                            // The main dimension is the sum of all the elements dimension plus the spacing.
                            mainDim += betweenMainDim + NodeDimWithMargin(child, mainAxis, availableInnerWidth);

                            // The cross dimension is the max of the elements dimension since
                            // there can only be one element in that cross dimension.
                            crossDim = Fmaxf(crossDim, NodeDimWithMargin(child, crossAxis, availableInnerWidth));
                        }

                        break;
                    }
                    default:
                    {
                        if (performLayout)
                        {
                            child.NodeLayout.Position[(int)Pos[(int)mainAxis]] +=
                                NodeLeadingBorder(node, mainAxis) + leadingMainDim;
                        }

                        break;
                    }
                }
            }

            mainDim += trailingPaddingAndBorderMain;

            var containerCrossAxis = availableInnerCrossDim;
            if (measureModeCrossDim is MeasureMode.Undefined or MeasureMode.AtMost)
            {
                // Compute the cross axis from the max cross dimension of the children.
                containerCrossAxis =
                    NodeBoundAxis(
                        node,
                        crossAxis,
                        crossDim + paddingAndBorderAxisCross,
                        crossAxisParentSize,
                        parentWidth
                    ) - paddingAndBorderAxisCross;
            }

            // If there's no flex wrap, the cross dimension is defined by the container.
            if (!isNodeFlexWrap && measureModeCrossDim == MeasureMode.Exactly)
            {
                crossDim = availableInnerCrossDim;
            }

            // Clamp to the min/max size specified on the container.
            crossDim =
                NodeBoundAxis(node, crossAxis, crossDim + paddingAndBorderAxisCross, crossAxisParentSize, parentWidth)
                - paddingAndBorderAxisCross;

            // STEP 7: CROSS-AXIS ALIGNMENT
            // We can skip child alignment if we're just measuring the container.
            if (performLayout)
            {
                for (var i = startOfLineIndex; i < endOfLineIndex; i++)
                {
                    var child = node.Storage[i];
                    if (child.NodeStyle.Display == Display.None)
                    {
                        continue;
                    }

                    if (child.NodeStyle.PositionType == PositionType.Absolute)
                    {
                        // If the child is absolutely positioned and has a
                        // top/left/bottom/right
                        // set, override all the previously computed positions to set it
                        // correctly.
                        if (NodeIsLeadingPosDefined(child, crossAxis))
                        {
                            child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                NodeLeadingPosition(child, crossAxis, availableInnerCrossDim)
                                + NodeLeadingBorder(node, crossAxis)
                                + NodeLeadingMargin(child, crossAxis, availableInnerWidth);
                        }
                        else
                        {
                            child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                NodeLeadingBorder(node, crossAxis)
                                + NodeLeadingMargin(child, crossAxis, availableInnerWidth);
                        }
                    }
                    else
                    {
                        var leadingCrossDim = leadingPaddingAndBorderCross;

                        // For a relative children, we're either using alignItems (parent) or
                        // alignSelf (child) in order to determine the position in the cross
                        // axis
                        var alignItem = NodeAlignItem(node, child);

                        // If the child uses align stretch, we need to lay it out one more
                        // time, this time
                        // forcing the cross-axis size to be the computed cross size for the
                        // current line.
                        if (
                            alignItem == Align.Stretch
                            && MarginLeadingValue(child, crossAxis).Unit != Unit.Auto
                            && MarginTrailingValue(child, crossAxis).Unit != Unit.Auto
                        )
                        {
                            // If the child defines a definite size for its cross axis, there's
                            // no need to stretch.
                            if (!NodeIsStyleDimDefined(child, crossAxis, availableInnerCrossDim))
                            {
                                var childMainSize = child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]];
                                var childCrossSize = crossDim;
                                if (!FloatIsUndefined(child.NodeStyle.AspectRatio))
                                {
                                    childCrossSize = NodeMarginForAxis(child, crossAxis, availableInnerWidth);
                                    if (isMainAxisRow)
                                    {
                                        childCrossSize += childMainSize / child.NodeStyle.AspectRatio;
                                    }
                                    else
                                    {
                                        childCrossSize += childMainSize * child.NodeStyle.AspectRatio;
                                    }
                                }

                                childMainSize += NodeMarginForAxis(child, mainAxis, availableInnerWidth);

                                var childMainMeasureMode = MeasureMode.Exactly;
                                var childCrossMeasureMode = MeasureMode.Exactly;
                                ConstrainMaxSizeForMode(
                                    child,
                                    mainAxis,
                                    availableInnerMainDim,
                                    availableInnerWidth,
                                    ref childMainMeasureMode,
                                    ref childMainSize
                                );
                                ConstrainMaxSizeForMode(
                                    child,
                                    crossAxis,
                                    availableInnerCrossDim,
                                    availableInnerWidth,
                                    ref childCrossMeasureMode,
                                    ref childCrossSize
                                );

                                var childWidth = childCrossSize;
                                if (isMainAxisRow)
                                {
                                    childWidth = childMainSize;
                                }

                                var childHeight = childCrossSize;
                                if (!isMainAxisRow)
                                {
                                    childHeight = childMainSize;
                                }

                                var childWidthMeasureMode = MeasureMode.Exactly;
                                if (FloatIsUndefined(childWidth))
                                {
                                    childWidthMeasureMode = MeasureMode.Undefined;
                                }

                                var childHeightMeasureMode = MeasureMode.Exactly;
                                if (FloatIsUndefined(childHeight))
                                {
                                    childHeightMeasureMode = MeasureMode.Undefined;
                                }

                                LayoutNodeInternal(
                                    child,
                                    childWidth,
                                    childHeight,
                                    direction,
                                    childWidthMeasureMode,
                                    childHeightMeasureMode,
                                    availableInnerWidth,
                                    availableInnerHeight,
                                    true
                                );
                            }
                        }
                        else
                        {
                            var remainingCrossDim =
                                containerCrossAxis - NodeDimWithMargin(child, crossAxis, availableInnerWidth);

                            if (
                                MarginLeadingValue(child, crossAxis).Unit == Unit.Auto
                                && MarginTrailingValue(child, crossAxis).Unit == Unit.Auto
                            )
                            {
                                leadingCrossDim += Fmaxf(0, remainingCrossDim / 2);
                            }
                            else if (MarginTrailingValue(child, crossAxis).Unit == Unit.Auto)
                            {
                                // No-Op
                            }
                            else if (MarginLeadingValue(child, crossAxis).Unit == Unit.Auto)
                            {
                                leadingCrossDim += Fmaxf(0, remainingCrossDim);
                            }
                            else
                                switch (alignItem)
                                {
                                    case Align.Start:
                                        // No-Op
                                        break;
                                    case Align.Center:
                                        leadingCrossDim += remainingCrossDim / 2;
                                        break;
                                    default:
                                        leadingCrossDim += remainingCrossDim;
                                        break;
                                }
                        }

                        // And we apply the position
                        child.NodeLayout.Position[(int)Pos[(int)crossAxis]] += totalLineCrossDim + leadingCrossDim;
                    }
                }
            }

            totalLineCrossDim += crossDim;
            maxLineMainDim = Fmaxf(maxLineMainDim, mainDim);

            lineCount++;
            startOfLineIndex = endOfLineIndex;
        }

        // STEP 8: MULTI-LINE CONTENT ALIGNMENT
        if (performLayout && (lineCount > 1 || IsBaselineLayout(node)) && !FloatIsUndefined(availableInnerCrossDim))
        {
            var remainingAlignContentDim = availableInnerCrossDim - totalLineCrossDim;

            float crossDimLead = 0;
            var currentLead = leadingPaddingAndBorderCross;

            switch (node.NodeStyle.AlignContent)
            {
                case Align.End:
                    currentLead += remainingAlignContentDim;
                    break;
                case Align.Center:
                    currentLead += remainingAlignContentDim / 2;
                    break;
                case Align.Stretch:
                    if (availableInnerCrossDim > totalLineCrossDim)
                    {
                        crossDimLead = remainingAlignContentDim / lineCount;
                    }

                    break;
                case Align.SpaceAround:
                    if (availableInnerCrossDim > totalLineCrossDim)
                    {
                        currentLead += remainingAlignContentDim / (2 * lineCount);
                        if (lineCount > 1)
                        {
                            crossDimLead = remainingAlignContentDim / lineCount;
                        }
                    }
                    else
                    {
                        currentLead += remainingAlignContentDim / 2;
                    }

                    break;
                case Align.SpaceBetween:
                    if (availableInnerCrossDim > totalLineCrossDim && lineCount > 1)
                    {
                        crossDimLead = remainingAlignContentDim / (lineCount - 1);
                    }

                    break;
                case Align.Auto:
                case Align.Start:
                case Align.Baseline:
                    break;
            }

            var endIndex = 0;
            for (var i = 0; i < lineCount; i++)
            {
                var startIndex = endIndex;
                int ii;

                // compute the line's height and find the endInde.x
                float lineHeight = 0;
                float maxAscentForCurrentLine = 0;
                float maxDescentForCurrentLine = 0;
                for (ii = startIndex; ii < childCount; ii++)
                {
                    var child = node.Storage[ii];
                    if (child.NodeStyle.Display == Display.None)
                    {
                        continue;
                    }

                    if (child.NodeStyle.PositionType == PositionType.Relative)
                    {
                        if (child.LineIndex != i)
                        {
                            break;
                        }

                        if (NodeIsLayoutDimDefined(child, crossAxis))
                        {
                            lineHeight = Fmaxf(
                                lineHeight,
                                child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                                    + NodeMarginForAxis(child, crossAxis, availableInnerWidth)
                            );
                        }

                        if (NodeAlignItem(node, child) == Align.Baseline)
                        {
                            var ascent =
                                Baseline(child) + NodeLeadingMargin(child, FlexDirection.Column, availableInnerWidth);
                            var descent =
                                child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                                + NodeMarginForAxis(child, FlexDirection.Column, availableInnerWidth)
                                - ascent;
                            maxAscentForCurrentLine = Fmaxf(maxAscentForCurrentLine, ascent);
                            maxDescentForCurrentLine = Fmaxf(maxDescentForCurrentLine, descent);
                            lineHeight = Fmaxf(lineHeight, maxAscentForCurrentLine + maxDescentForCurrentLine);
                        }
                    }
                }

                endIndex = ii;
                lineHeight += crossDimLead;

                if (performLayout)
                {
                    for (ii = startIndex; ii < endIndex; ii++)
                    {
                        var child = node.Storage[ii];
                        if (child.NodeStyle.Display == Display.None)
                        {
                            continue;
                        }

                        if (child.NodeStyle.PositionType == PositionType.Relative)
                        {
                            switch (NodeAlignItem(node, child))
                            {
                                case Align.Start:
                                    {
                                        child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                            currentLead + NodeLeadingMargin(child, crossAxis, availableInnerWidth);
                                    }
                                    break;
                                case Align.End:
                                    {
                                        child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                            currentLead
                                            + lineHeight
                                            - NodeTrailingMargin(child, crossAxis, availableInnerWidth)
                                            - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]];
                                    }
                                    break;
                                case Align.Center:
                                    {
                                        var childHeight = child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]];
                                        child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                            currentLead + (lineHeight - childHeight) / 2;
                                    }
                                    break;
                                case Align.Stretch:
                                    {
                                        child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                                            currentLead + NodeLeadingMargin(child, crossAxis, availableInnerWidth);

                                        // Remeasure child with the line height as it as been only measured with the
                                        // parents height yet.
                                        if (!NodeIsStyleDimDefined(child, crossAxis, availableInnerCrossDim))
                                        {
                                            var childWidth = lineHeight;
                                            if (isMainAxisRow)
                                            {
                                                childWidth =
                                                    child.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                                                    + NodeMarginForAxis(child, mainAxis, availableInnerWidth);
                                            }

                                            var childHeight = lineHeight;
                                            if (!isMainAxisRow)
                                            {
                                                childHeight =
                                                    child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                                                    + NodeMarginForAxis(child, crossAxis, availableInnerWidth);
                                            }

                                            if (
                                                !(
                                                    FloatsEqual(
                                                        childWidth,
                                                        child.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                                                    )
                                                    && FloatsEqual(
                                                        childHeight,
                                                        child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                                                    )
                                                )
                                            )
                                            {
                                                LayoutNodeInternal(
                                                    child,
                                                    childWidth,
                                                    childHeight,
                                                    direction,
                                                    MeasureMode.Exactly,
                                                    MeasureMode.Exactly,
                                                    availableInnerWidth,
                                                    availableInnerHeight,
                                                    true
                                                );
                                            }
                                        }
                                    }
                                    break;
                                case Align.Baseline:
                                    {
                                        child.NodeLayout.Position[(int)Edge.Top] =
                                            currentLead
                                            + maxAscentForCurrentLine
                                            - Baseline(child)
                                            + NodeLeadingPosition(child, FlexDirection.Column, availableInnerCrossDim);
                                    }
                                    break;
                                case Align.Auto:
                                case Align.SpaceBetween:
                                case Align.SpaceAround:
                                    break;
                            }
                        }
                    }
                }

                currentLead += lineHeight;

                // Advance past the cross-axis gap before positioning the next line.
                if (i < lineCount - 1)
                {
                    currentLead += crossAxisGap;
                }
            }
        }

        //   STEP 9: COMPUTING FINAL DIMENSIONS
        node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
            node,
            FlexDirection.Row,
            availableWidth - marginAxisRow,
            parentWidth,
            parentWidth
        );
        node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
            node,
            FlexDirection.Column,
            availableHeight - marginAxisColumn,
            parentHeight,
            parentWidth
        );

        // If the user didn't specify a width or height for the node, set the
        // dimensions based on the children.
        if (
            measureModeMainDim == MeasureMode.Undefined
            || (node.NodeStyle.Overflow != Overflow.Scroll && measureModeMainDim == MeasureMode.AtMost)
        )
        {
            // Clamp the size to the min/max size, if specified, and make sure it
            // doesn't go below the padding and border amount.
            node.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]] = NodeBoundAxis(
                node,
                mainAxis,
                maxLineMainDim,
                mainAxisParentSize,
                parentWidth
            );
        }
        else if (measureModeMainDim == MeasureMode.AtMost && node.NodeStyle.Overflow == Overflow.Scroll)
        {
            node.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]] = Fmaxf(
                Fminf(
                    availableInnerMainDim + paddingAndBorderAxisMain,
                    NodeBoundAxisWithinMinAndMax(node, mainAxis, maxLineMainDim, mainAxisParentSize)
                ),
                paddingAndBorderAxisMain
            );
        }

        if (
            measureModeCrossDim == MeasureMode.Undefined
            || (node.NodeStyle.Overflow != Overflow.Scroll && measureModeCrossDim == MeasureMode.AtMost)
        )
        {
            // Clamp the size to the min/max size, if specified, and make sure it
            // doesn't go below the padding and border amount.
            node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]] = NodeBoundAxis(
                node,
                crossAxis,
                totalLineCrossDim + paddingAndBorderAxisCross,
                crossAxisParentSize,
                parentWidth
            );
        }
        else if (measureModeCrossDim == MeasureMode.AtMost && node.NodeStyle.Overflow == Overflow.Scroll)
        {
            node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]] = Fmaxf(
                Fminf(
                    availableInnerCrossDim + paddingAndBorderAxisCross,
                    NodeBoundAxisWithinMinAndMax(
                        node,
                        crossAxis,
                        totalLineCrossDim + paddingAndBorderAxisCross,
                        crossAxisParentSize
                    )
                ),
                paddingAndBorderAxisCross
            );
        }

        // As we only wrapped in normal direction yet, we need to reverse the positions on wrap-reverse.
        if (performLayout && node.NodeStyle.FlexWrap == Wrap.WrapReverse)
        {
            foreach (var child in node)
            {
                if (child.NodeStyle.PositionType == PositionType.Relative)
                {
                    child.NodeLayout.Position[(int)Pos[(int)crossAxis]] =
                        node.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]]
                        - child.NodeLayout.Position[(int)Pos[(int)crossAxis]]
                        - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)crossAxis]];
                }
            }
        }

        if (performLayout)
        {
            // STEP 10: SIZING AND POSITIONING ABSOLUTE CHILDREN
            for (
                currentAbsoluteChild = firstAbsoluteChild;
                currentAbsoluteChild != null;
                currentAbsoluteChild = currentAbsoluteChild.NextChild
            )
            {
                var mode = measureModeCrossDim;
                if (isMainAxisRow)
                {
                    mode = measureModeMainDim;
                }

                NodeAbsoluteLayoutChild(
                    node,
                    currentAbsoluteChild,
                    availableInnerWidth,
                    mode,
                    availableInnerHeight,
                    direction
                );
            }

            // STEP 11: SETTING TRAILING POSITIONS FOR CHILDREN
            var needsMainTrailingPos = mainAxis is FlexDirection.RowReverse or FlexDirection.ColumnReverse;
            var needsCrossTrailingPos = crossAxis is FlexDirection.RowReverse or FlexDirection.ColumnReverse;

            // Set trailing position if necessary.
            if (needsMainTrailingPos || needsCrossTrailingPos)
            {
                foreach (var child in node)
                {
                    if (child.NodeStyle.Display == Display.None)
                    {
                        continue;
                    }

                    if (needsMainTrailingPos)
                    {
                        NodeSetChildTrailingPosition(node, child, mainAxis);
                    }

                    if (needsCrossTrailingPos)
                    {
                        NodeSetChildTrailingPosition(node, child, crossAxis);
                    }
                }
            }
        }
    }

    internal static bool MeasureModeSizeIsExactAndMatchesOldMeasuredSize(
        MeasureMode sizeMode,
        float size,
        float lastComputedSize
    )
    {
        return sizeMode == MeasureMode.Exactly && FloatsEqual(size, lastComputedSize);
    }

    internal static bool MeasureModeOldSizeIsUnspecifiedAndStillFits(
        MeasureMode sizeMode,
        float size,
        MeasureMode lastSizeMode,
        float lastComputedSize
    )
    {
        return sizeMode == MeasureMode.AtMost
            && lastSizeMode == MeasureMode.Undefined
            && (size >= lastComputedSize || FloatsEqual(size, lastComputedSize));
    }

    internal static bool MeasureModeNewMeasureSizeIsStricterAndStillValid(
        MeasureMode sizeMode,
        float size,
        MeasureMode lastSizeMode,
        float lastSize,
        float lastComputedSize
    )
    {
        return lastSizeMode == MeasureMode.AtMost
            && sizeMode == MeasureMode.AtMost
            && lastSize > size
            && (lastComputedSize <= size || FloatsEqual(size, lastComputedSize));
    }

    // nodeCanUseCachedMeasurement returns true if it can use cached measurement
    internal static bool NodeCanUseCachedMeasurement(
        MeasureMode widthMode,
        float width,
        MeasureMode heightMode,
        float height,
        MeasureMode lastWidthMode,
        float lastWidth,
        MeasureMode lastHeightMode,
        float lastHeight,
        float lastComputedWidth,
        float lastComputedHeight,
        float marginRow,
        float marginColumn
    )
    {
        if (lastComputedHeight < 0 || lastComputedWidth < 0)
        {
            return false;
        }

        const bool useRoundedComparison = true;
        float effectiveWidth;
        float effectiveHeight;
        float effectiveLastWidth;
        float effectiveLastHeight;

        if (useRoundedComparison)
        {
            effectiveWidth = RoundValueToPixelGrid(width, 1, false, false);
            effectiveHeight = RoundValueToPixelGrid(height, 1, false, false);
            effectiveLastWidth = RoundValueToPixelGrid(lastWidth, 1, false, false);
            effectiveLastHeight = RoundValueToPixelGrid(lastHeight, 1, false, false);
        }

        var hasSameWidthSpec = lastWidthMode == widthMode && FloatsEqual(effectiveLastWidth, effectiveWidth);
        var hasSameHeightSpec = lastHeightMode == heightMode && FloatsEqual(effectiveLastHeight, effectiveHeight);

        var widthIsCompatible =
            hasSameWidthSpec
            || MeasureModeSizeIsExactAndMatchesOldMeasuredSize(widthMode, width - marginRow, lastComputedWidth)
            || MeasureModeOldSizeIsUnspecifiedAndStillFits(
                widthMode,
                width - marginRow,
                lastWidthMode,
                lastComputedWidth
            )
            || MeasureModeNewMeasureSizeIsStricterAndStillValid(
                widthMode,
                width - marginRow,
                lastWidthMode,
                lastWidth,
                lastComputedWidth
            );

        var heightIsCompatible =
            hasSameHeightSpec
            || MeasureModeSizeIsExactAndMatchesOldMeasuredSize(heightMode, height - marginColumn, lastComputedHeight)
            || MeasureModeOldSizeIsUnspecifiedAndStillFits(
                heightMode,
                height - marginColumn,
                lastHeightMode,
                lastComputedHeight
            )
            || MeasureModeNewMeasureSizeIsStricterAndStillValid(
                heightMode,
                height - marginColumn,
                lastHeightMode,
                lastHeight,
                lastComputedHeight
            );

        return widthIsCompatible && heightIsCompatible;
    }

    // layoutNodeInternal is a wrapper around the YGNodelayoutImpl function. It determines
    // whether the layout request is redundant and can be skipped.
    //
    // Parameters:
    //  Input parameters are the same as YGNodelayoutImpl (see above)
    //  Return parameter is true if layout was performed, false if skipped
    internal static bool LayoutNodeInternal<TStorage>(
        Node<TStorage> node,
        float availableWidth,
        float availableHeight,
        Direction parentDirection,
        MeasureMode widthMeasureMode,
        MeasureMode heightMeasureMode,
        float parentWidth,
        float parentHeight,
        bool performLayout
    )
        where TStorage : IList<Node<TStorage>>
    {
        ref var layout = ref node.NodeLayout;

        var needToVisitNode =
            (node.IsDirty && layout.GenerationCount != CurrentGenerationCount)
            || layout.LastParentDirection != parentDirection;

        if (needToVisitNode)
        {
            // Invalidate the cached results.
            layout.NextCachedMeasurementsIndex = 0;
            layout.CachedLayout.WidthMeasureMode = (MeasureMode)(-1);
            layout.CachedLayout.HeightMeasureMode = (MeasureMode)(-1);
            layout.CachedLayout.ComputedWidth = -1;
            layout.CachedLayout.ComputedHeight = -1;
        }

        var cachedResultsValid = false;
        float cachedComputedWidth = 0;
        float cachedComputedHeight = 0;

        // Determine whether the results are already cached. We maintain a separate
        // cache for layouts and measurements. A layout operation modifies the
        // positions
        // and dimensions for nodes in the subtree. The algorithm assumes that each
        // node
        // gets layed out a maximum of one time per tree layout, but multiple
        // measurements
        // may be required to resolve all the flex dimensions.
        // We handle nodes with measure functions specially here because they are the
        // most
        // expensive to measure, so it's worth avoiding redundant measurements if at
        // all possible.
        if (node.MeasureFunc != null)
        {
            var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, parentWidth);
            var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, parentWidth);

            // First, try to use the layout cache.
            if (
                NodeCanUseCachedMeasurement(
                    widthMeasureMode,
                    availableWidth,
                    heightMeasureMode,
                    availableHeight,
                    layout.CachedLayout.WidthMeasureMode,
                    layout.CachedLayout.AvailableWidth,
                    layout.CachedLayout.HeightMeasureMode,
                    layout.CachedLayout.AvailableHeight,
                    layout.CachedLayout.ComputedWidth,
                    layout.CachedLayout.ComputedHeight,
                    marginAxisRow,
                    marginAxisColumn
                )
            )
            {
                cachedResultsValid = true;
                cachedComputedWidth = layout.CachedLayout.ComputedWidth;
                cachedComputedHeight = layout.CachedLayout.ComputedHeight;
            }
            else
            {
                // Try to use the measurement cache.
                for (var i = 0; i < layout.NextCachedMeasurementsIndex; i++)
                {
                    if (
                        NodeCanUseCachedMeasurement(
                            widthMeasureMode,
                            availableWidth,
                            heightMeasureMode,
                            availableHeight,
                            layout.CachedMeasurements[i].WidthMeasureMode,
                            layout.CachedMeasurements[i].AvailableWidth,
                            layout.CachedMeasurements[i].HeightMeasureMode,
                            layout.CachedMeasurements[i].AvailableHeight,
                            layout.CachedMeasurements[i].ComputedWidth,
                            layout.CachedMeasurements[i].ComputedHeight,
                            marginAxisRow,
                            marginAxisColumn
                        )
                    )
                    {
                        cachedResultsValid = true;
                        cachedComputedWidth = layout.CachedMeasurements[i].ComputedWidth;
                        cachedComputedHeight = layout.CachedMeasurements[i].ComputedHeight;
                        break;
                    }
                }
            }
        }
        else if (performLayout)
        {
            if (
                FloatsEqual(layout.CachedLayout.AvailableWidth, availableWidth)
                && FloatsEqual(layout.CachedLayout.AvailableHeight, availableHeight)
                && layout.CachedLayout.WidthMeasureMode == widthMeasureMode
                && layout.CachedLayout.HeightMeasureMode == heightMeasureMode
            )
            {
                cachedResultsValid = true;
                cachedComputedWidth = layout.CachedLayout.ComputedWidth;
                cachedComputedHeight = layout.CachedLayout.ComputedHeight;
            }
        }
        else
        {
            for (var i = 0; i < layout.NextCachedMeasurementsIndex; i++)
            {
                if (
                    FloatsEqual(layout.CachedMeasurements[i].AvailableWidth, availableWidth)
                    && FloatsEqual(layout.CachedMeasurements[i].AvailableHeight, availableHeight)
                    && layout.CachedMeasurements[i].WidthMeasureMode == widthMeasureMode
                    && layout.CachedMeasurements[i].HeightMeasureMode == heightMeasureMode
                )
                {
                    cachedResultsValid = true;
                    cachedComputedWidth = layout.CachedMeasurements[i].ComputedWidth;
                    cachedComputedHeight = layout.CachedMeasurements[i].ComputedHeight;
                    break;
                }
            }
        }

        if (!needToVisitNode && cachedResultsValid)
        {
            layout.MeasuredDimensions[(int)Dimension.Width] = cachedComputedWidth;
            layout.MeasuredDimensions[(int)Dimension.Height] = cachedComputedHeight;
        }
        else
        {
            NodeLayoutImpl(
                node,
                availableWidth,
                availableHeight,
                parentDirection,
                widthMeasureMode,
                heightMeasureMode,
                parentWidth,
                parentHeight,
                performLayout
            );

            layout.LastParentDirection = parentDirection;

            if (!cachedResultsValid)
            {
                if (layout.NextCachedMeasurementsIndex == Constant.MaxCachedResultCount)
                {
                    layout.NextCachedMeasurementsIndex = 0;
                }

                ref var newCacheEntry = ref layout.CachedLayout;
                if (performLayout)
                {
                    // Use the single layout cache entry.
                    newCacheEntry = ref layout.CachedLayout;
                }
                else
                {
                    // Allocate a new measurement cache entry.
                    newCacheEntry = ref layout.CachedMeasurements[layout.NextCachedMeasurementsIndex];
                    layout.NextCachedMeasurementsIndex++;
                }

                newCacheEntry.AvailableWidth = availableWidth;
                newCacheEntry.AvailableHeight = availableHeight;
                newCacheEntry.WidthMeasureMode = widthMeasureMode;
                newCacheEntry.HeightMeasureMode = heightMeasureMode;
                newCacheEntry.ComputedWidth = layout.MeasuredDimensions[(int)Dimension.Width];
                newCacheEntry.ComputedHeight = layout.MeasuredDimensions[(int)Dimension.Height];
            }
        }

        if (performLayout)
        {
            node.NodeLayout.Dimensions[(int)Dimension.Width] = node.NodeLayout.MeasuredDimensions[(int)Dimension.Width];
            node.NodeLayout.Dimensions[(int)Dimension.Height] = node.NodeLayout.MeasuredDimensions[
                (int)Dimension.Height
            ];
            node.IsDirty = false;
        }

        layout.GenerationCount = CurrentGenerationCount;
        return needToVisitNode || !cachedResultsValid;
    }

    internal static void RoundToPixelGrid<TStorage>(
        Node<TStorage> node,
        float pointScaleFactor,
        float absoluteLeft,
        float absoluteTop
    )
        where TStorage : IList<Node<TStorage>>
    {
        if (pointScaleFactor == 0.0)
        {
            return;
        }

        var nodeLeft = node.NodeLayout.Position[(int)Edge.Left];
        var nodeTop = node.NodeLayout.Position[(int)Edge.Top];

        var nodeWidth = node.NodeLayout.Dimensions[(int)Dimension.Width];
        var nodeHeight = node.NodeLayout.Dimensions[(int)Dimension.Height];

        var absoluteNodeLeft = absoluteLeft + nodeLeft;
        var absoluteNodeTop = absoluteTop + nodeTop;

        var absoluteNodeRight = absoluteNodeLeft + nodeWidth;
        var absoluteNodeBottom = absoluteNodeTop + nodeHeight;

        // If a node has a custom measure function we never want to round down its size as this could
        // lead to unwanted text truncation.
        var textRounding = node.NodeType == NodeType.Text;

        node.NodeLayout.Position[(int)Edge.Left] = RoundValueToPixelGrid(
            nodeLeft,
            pointScaleFactor,
            false,
            textRounding
        );
        node.NodeLayout.Position[(int)Edge.Top] = RoundValueToPixelGrid(nodeTop, pointScaleFactor, false, textRounding);

        // We multiply dimension by scale factor and if the result is close to the whole number, we don't have any fraction
        // To verify if the result is close to whole number we want to check both floor and ceil numbers
        var hasFractionalWidth =
            !FloatsEqual(nodeWidth * pointScaleFactor % 1, 0) && !FloatsEqual(nodeWidth * pointScaleFactor % 1, 1);
        var hasFractionalHeight =
            !FloatsEqual(nodeHeight * pointScaleFactor % 1, 0) && !FloatsEqual(nodeHeight * pointScaleFactor % 1, 1);

        node.NodeLayout.Dimensions[(int)Dimension.Width] =
            RoundValueToPixelGrid(
                absoluteNodeRight,
                pointScaleFactor,
                textRounding && hasFractionalWidth,
                textRounding && !hasFractionalWidth
            ) - RoundValueToPixelGrid(absoluteNodeLeft, pointScaleFactor, false, textRounding);
        node.NodeLayout.Dimensions[(int)Dimension.Height] =
            RoundValueToPixelGrid(
                absoluteNodeBottom,
                pointScaleFactor,
                textRounding && hasFractionalHeight,
                textRounding && !hasFractionalHeight
            ) - RoundValueToPixelGrid(absoluteNodeTop, pointScaleFactor, false, textRounding);

        foreach (var child in node)
        {
            RoundToPixelGrid(child, pointScaleFactor, absoluteNodeLeft, absoluteNodeTop);
        }
    }

    internal static void CalcStartWidth<TStorage>(
        Node<TStorage> node,
        float parentWidth,
        out float outWidth,
        out MeasureMode outMeasureMode
    )
        where TStorage : IList<Node<TStorage>>
    {
        if (NodeIsStyleDimDefined(node, FlexDirection.Row, parentWidth))
        {
            var width = ResolveValue(node.ResolvedDimensions[(int)Dim[(int)FlexDirection.Row]], parentWidth);
            var margin = NodeMarginForAxis(node, FlexDirection.Row, parentWidth);
            outWidth = width + margin;
            outMeasureMode = MeasureMode.Exactly;
            return;
        }

        if (ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Width], parentWidth) >= 0f)
        {
            outWidth = ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Width], parentWidth);
            outMeasureMode = MeasureMode.AtMost;
            return;
        }

        {
            var widthMeasureMode = MeasureMode.Exactly;
            if (FloatIsUndefined(parentWidth))
            {
                widthMeasureMode = MeasureMode.Undefined;
            }

            outWidth = parentWidth;
            outMeasureMode = widthMeasureMode;
        }
    }

    internal static void CalcStartHeight<TStorage>(
        Node<TStorage> node,
        float parentWidth,
        float parentHeight,
        out float outHeight,
        out MeasureMode outMeasureMode
    )
        where TStorage : IList<Node<TStorage>>
    {
        if (NodeIsStyleDimDefined(node, FlexDirection.Column, parentHeight))
        {
            var height = ResolveValue(node.ResolvedDimensions[(int)Dim[(int)FlexDirection.Column]], parentHeight);
            var margin = NodeMarginForAxis(node, FlexDirection.Column, parentWidth);
            outHeight = height + margin;
            outMeasureMode = MeasureMode.Exactly;
            return;
        }

        if (ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Height], parentHeight) >= 0)
        {
            outHeight = ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Height], parentHeight);
            outMeasureMode = MeasureMode.AtMost;
            return;
        }

        {
            var heightMeasureMode = MeasureMode.Exactly;
            if (FloatIsUndefined(parentHeight))
            {
                heightMeasureMode = MeasureMode.Undefined;
            }

            outHeight = parentHeight;
            outMeasureMode = heightMeasureMode;
        }
    }

    internal static float Fmaxf(float a, float b)
    {
        if (float.IsNaN(a))
            return b;
        if (float.IsNaN(b) || a > b)
            return a;
        if (b > a)
            return b;
        if (a == 0.0f)
        {
            if (BitConverter.SingleToInt32Bits(a) >= 0 || BitConverter.SingleToInt32Bits(b) >= 0)
            {
                return 0.0f;
            }

            return -0.0f;
        }

        return a;
    }

    internal static float Fminf(float a, float b)
    {
        if (float.IsNaN(a))
            return b;
        if (float.IsNaN(b) || a < b)
            return a;
        if (b < a)
            return b;
        if (a == 0.0f)
        {
            if (BitConverter.SingleToInt32Bits(a) < 0 || BitConverter.SingleToInt32Bits(b) < 0)
            {
                return -0.0f;
            }

            return 0.0f;
        }

        return a;
    }

    internal struct CachedMeasurement
    {
        internal float AvailableHeight;
        internal float AvailableWidth;
        internal float ComputedHeight = -1;
        internal float ComputedWidth = -1;
        internal MeasureMode HeightMeasureMode = MeasureMode.Undefined;
        internal MeasureMode WidthMeasureMode = MeasureMode.Undefined;

        public CachedMeasurement() { }

        internal void ResetToDefault()
        {
            AvailableHeight = 0;
            AvailableWidth = 0;
            WidthMeasureMode = MeasureMode.Undefined;
            HeightMeasureMode = MeasureMode.Undefined;
            ComputedWidth = -1;
            ComputedHeight = -1;
        }
    }

    internal struct Layout
    {
        internal FloatBuffer6 Border;
        internal CachedMeasurement CachedLayout = new();

        internal CachedMeasurementBuffer CachedMeasurements;

        internal FloatBuffer2 Dimensions;
        internal FloatBuffer6 Margin;
        internal FloatBuffer2 MeasuredDimensions;
        internal FloatBuffer6 Padding;
        internal FloatBuffer4 Position;
        internal float ComputedFlexBasis = float.NaN;
        internal Direction Direction;

        // Instead of recomputing the entire layout every single time, we
        // cache some information to break early when nothing changed
        internal int GenerationCount;
        internal bool HadOverflow = false;
        internal Direction LastParentDirection = Direction.Inherit;
        internal int NextCachedMeasurementsIndex = 0;

        public Layout()
        {
            Dimensions[0] = float.NaN;
            Dimensions[1] = float.NaN;
            MeasuredDimensions[0] = float.NaN;
            MeasuredDimensions[1] = float.NaN;

            for (var i = 0; i < Constant.MaxCachedResultCount; i++)
            {
                CachedMeasurements[i] = new CachedMeasurement();
            }
        }

        internal void ResetToDefault()
        {
            for (var i = 0; i < 4; i++)
            {
                Position[i] = 0;
            }

            for (var i = 0; i < 2; i++)
            {
                Dimensions[i] = float.NaN;
            }

            for (var i = 0; i < 6; i++)
            {
                Margin[i] = 0;
                Border[i] = 0;
                Padding[i] = 0;
            }

            Direction = Direction.Inherit;
            ComputedFlexBasis = float.NaN;
            HadOverflow = false;
            GenerationCount = 0;
            LastParentDirection = Direction.Inherit;
            NextCachedMeasurementsIndex = 0;

            for (var i = 0; i < Constant.MaxCachedResultCount; i++)
            {
                CachedMeasurements[i].ResetToDefault();
            }

            for (var i = 0; i < 2; i++)
            {
                MeasuredDimensions[i] = float.NaN;
            }

            CachedLayout.ResetToDefault();
        }
    }
}
