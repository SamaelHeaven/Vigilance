// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.FlexLayout;

internal static class Constant
{
    internal const int EdgeCount = 9;
    internal const int MaxCachedResultCount = 16;
    internal const float DefaultFlexGrow = 0;
    internal const float DefaultFlexShrink = 0;
}

[InlineArray(Constant.EdgeCount)]
internal struct EdgeArray
{
    private Value _element0;
}

[InlineArray(Constant.MaxCachedResultCount)]
internal struct CachedMeasurementArray
{
    private Flex.CachedMeasurement _element0;
}

internal struct Style
{
    internal EdgeArray Border;
    internal InlineArray2<Value> Dimensions;
    internal EdgeArray Margin;
    internal InlineArray2<Value> MaxDimensions;
    internal InlineArray2<Value> MinDimensions;
    internal EdgeArray Padding;
    internal EdgeArray Position;
    internal Align AlignContent = Align.Start;
    internal Align AlignItems = Align.Stretch;
    internal Align AlignSelf;
    internal float AspectRatio = float.NaN;
    internal Direction Direction = Direction.Inherit;
    internal Display Display = Display.Flex;
    internal float Flex = float.NaN;
    internal Value FlexBasis = CreateAutoValue();
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
    private const int LCache = 0;
    private const int LSetup = 1;
    private const int LStep3 = 2;
    private const int LLineStart = 3;
    private const int LPass2 = 4;
    private const int LAfterFlex = 5;
    private const int LStep7 = 6;
    private const int LLineEnd = 7;
    private const int LStep8Line = 8;
    private const int LStep8Place = 9;
    private const int LFinalDims = 10;
    private const int LAbsolute = 11;
    private const int LFinish = 12;
    internal static readonly Value ValueZero = new(0, Unit.Point);
    internal static readonly Value ValueUndefined = new(float.NaN, Unit.Undefined);
    internal static readonly Value ValueAuto = new(float.NaN, Unit.Auto);
    internal static int CurrentGenerationCount = 0;

    internal static readonly InlineList<InlineArray4<Edge>, Edge> Leading =
    [
        Edge.Top,
        Edge.Bottom,
        Edge.Left,
        Edge.Right,
    ];

    internal static readonly InlineList<InlineArray4<Edge>, Edge> Trailing =
    [
        Edge.Bottom,
        Edge.Top,
        Edge.Right,
        Edge.Left,
    ];

    internal static readonly InlineList<InlineArray4<Edge>, Edge> Pos = [Edge.Top, Edge.Bottom, Edge.Left, Edge.Right];

    internal static readonly InlineList<InlineArray4<Dimension>, Dimension> Dim =
    [
        Dimension.Height,
        Dimension.Height,
        Dimension.Width,
        Dimension.Width,
    ];

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

    internal static Value ComputedEdgeValue(in ReadOnlySpan<Value> edges, Edge edge, Value defaultValue)
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
        for (var current = node; current is { IsDirty: false }; current = current.Parent)
        {
            current.IsDirty = true;
            current.NodeLayout.ComputedFlexBasis = float.NaN;
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
        float accumulatedTop = 0;
        var current = node;
        while (true)
        {
            if (current.BaselineFunc != null)
            {
                var baseline = current.BaselineFunc(
                    current,
                    current.NodeLayout.MeasuredDimensions[(int)Dimension.Width],
                    current.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                );
                Debug.Assert(!FloatIsUndefined(baseline), "Expect custom baseline function to not return NaN");
                return accumulatedTop + baseline;
            }

            Node<TStorage>? baselineChild = null;
            foreach (var child in current)
            {
                if (child.LineIndex > 0)
                {
                    break;
                }

                if (child.NodeStyle.PositionType == PositionType.Absolute)
                {
                    continue;
                }

                if (NodeAlignItem(current, child) == Align.Baseline)
                {
                    baselineChild = child;
                    break;
                }

                baselineChild ??= child;
            }

            if (baselineChild == null)
            {
                return accumulatedTop + current.NodeLayout.MeasuredDimensions[(int)Dimension.Height];
            }

            accumulatedTop += baselineChild.NodeLayout.Position[(int)Edge.Top];
            current = baselineChild;
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

    // ZeroOutLayoutRecursively zeros out layout for the node and its whole subtree
    internal static void ZeroOutLayoutRecursively<TStorage>(Node<TStorage> node)
        where TStorage : IList<Node<TStorage>>
    {
        foreach (var current in node.DescendantsAndSelf())
        {
            current.NodeLayout.Dimensions[(int)Dimension.Height] = 0;
            current.NodeLayout.Dimensions[(int)Dimension.Width] = 0;
            current.NodeLayout.Position[(int)Edge.Top] = 0;
            current.NodeLayout.Position[(int)Edge.Bottom] = 0;
            current.NodeLayout.Position[(int)Edge.Left] = 0;
            current.NodeLayout.Position[(int)Edge.Right] = 0;
            current.NodeLayout.CachedLayout.AvailableHeight = 0;
            current.NodeLayout.CachedLayout.AvailableWidth = 0;
            current.NodeLayout.CachedLayout.HeightMeasureMode = MeasureMode.Exactly;
            current.NodeLayout.CachedLayout.WidthMeasureMode = MeasureMode.Exactly;
            current.NodeLayout.CachedLayout.ComputedWidth = 0;
            current.NodeLayout.CachedLayout.ComputedHeight = 0;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitLayout<TStorage>(
        ref Frame<TStorage> f,
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
        f.Node = node;
        f.Kind = LayoutTaskKind.Layout;
        f.Phase = LCache;
        ref var d = ref f.Data.Layout;
        d.AvailableWidth = availableWidth;
        d.AvailableHeight = availableHeight;
        d.ParentDirection = parentDirection;
        d.WidthMeasureMode = widthMeasureMode;
        d.HeightMeasureMode = heightMeasureMode;
        d.ParentWidth = parentWidth;
        d.ParentHeight = parentHeight;
        d.PerformLayout = performLayout;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitFlexBasis<TStorage>(
        ref Frame<TStorage> f,
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
        f.Node = node;
        f.Child = child;
        f.Kind = LayoutTaskKind.FlexBasis;
        f.Phase = 0;
        ref var d = ref f.Data.FlexBasis;
        d.Width = width;
        d.WidthMode = widthMode;
        d.Height = height;
        d.ParentWidth = parentWidth;
        d.ParentHeight = parentHeight;
        d.HeightMode = heightMode;
        d.Direction = direction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitAbsolute<TStorage>(
        ref Frame<TStorage> f,
        Node<TStorage> node,
        Node<TStorage> child,
        float width,
        MeasureMode widthMode,
        float height,
        Direction direction
    )
        where TStorage : IList<Node<TStorage>>
    {
        f.Node = node;
        f.Child = child;
        f.Kind = LayoutTaskKind.Absolute;
        f.Phase = 0;
        ref var d = ref f.Data.Absolute;
        d.Width = width;
        d.WidthMode = widthMode;
        d.Height = height;
        d.Direction = direction;
    }

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
        var frames = ArrayPool<Frame<TStorage>>.Shared.Rent(64);
        try
        {
            var count = 0;
            InitLayout(
                ref frames[count],
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
            count++;

            while (count > 0)
            {
                if (count == frames.Length)
                {
                    var bigger = ArrayPool<Frame<TStorage>>.Shared.Rent(frames.Length * 2);
                    Array.Copy(frames, bigger, count);
                    ArrayPool<Frame<TStorage>>.Shared.Return(frames, true);
                    frames = bigger;
                }

                if (!Step(frames, ref count))
                {
                    count--;
                }
            }

            return frames[0].Data.Layout.Result;
        }
        finally
        {
            ArrayPool<Frame<TStorage>>.Shared.Return(frames, true);
        }
    }

    private static bool Step<TStorage>(Frame<TStorage>[] frames, ref int count)
        where TStorage : IList<Node<TStorage>>
    {
        switch (frames[count - 1].Kind)
        {
            case LayoutTaskKind.Layout:
                return StepLayout(frames, ref count);
            case LayoutTaskKind.FlexBasis:
                return StepFlexBasis(frames, ref count);
            default:
                return StepAbsolute(frames, ref count);
        }
    }

    // Iterative equivalent of NodeComputeFlexBasisForChild.
    private static bool StepFlexBasis<TStorage>(Frame<TStorage>[] frames, ref int count)
        where TStorage : IList<Node<TStorage>>
    {
        ref var f = ref frames[count - 1];
        ref var d = ref f.Data.FlexBasis;
        var node = f.Node;
        var child = f.Child;
        var direction = d.Direction;
        var parentWidth = d.ParentWidth;
        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);

        if (f.Phase == 1)
        {
            // Resume after measuring the child (the default branch below).
            child.NodeLayout.ComputedFlexBasis = Fmaxf(
                child.NodeLayout.MeasuredDimensions[(int)Dim[(int)mainAxis]],
                NodePaddingAndBorderForAxis(child, mainAxis, parentWidth)
            );
            return false;
        }

        var width = d.Width;
        var widthMode = d.WidthMode;
        var height = d.Height;
        var heightMode = d.HeightMode;
        var parentHeight = d.ParentHeight;
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

            return false;
        }

        if (isMainAxisRow && isRowStyleDimDefined)
        {
            // The width is definite, so use that as the flex basis.
            child.NodeLayout.ComputedFlexBasis = Fmaxf(
                ResolveValue(child.ResolvedDimensions[(int)Dimension.Width], parentWidth),
                NodePaddingAndBorderForAxis(child, FlexDirection.Row, parentWidth)
            );
            return false;
        }

        if (!isMainAxisRow && isColumnStyleDimDefined)
        {
            // The height is definite, so use that as the flex basis.
            child.NodeLayout.ComputedFlexBasis = Fmaxf(
                ResolveValue(child.ResolvedDimensions[(int)Dimension.Height], parentHeight),
                NodePaddingAndBorderForAxis(child, FlexDirection.Column, parentWidth)
            );
            return false;
        }

        // Compute the flex basis and hypothetical main size (i.e. the clamped flex basis).
        var childWidth = float.NaN;
        var childHeight = float.NaN;
        var childWidthMeasureMode = MeasureMode.Undefined;
        var childHeightMeasureMode = MeasureMode.Undefined;

        var marginRow = NodeMarginForAxis(child, FlexDirection.Row, parentWidth);
        var marginColumn = NodeMarginForAxis(child, FlexDirection.Column, parentWidth);

        if (isRowStyleDimDefined)
        {
            childWidth = ResolveValue(child.ResolvedDimensions[(int)Dimension.Width], parentWidth) + marginRow;
            childWidthMeasureMode = MeasureMode.Exactly;
        }

        if (isColumnStyleDimDefined)
        {
            childHeight = ResolveValue(child.ResolvedDimensions[(int)Dimension.Height], parentHeight) + marginColumn;
            childHeightMeasureMode = MeasureMode.Exactly;
        }

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

        if ((isMainAxisRow && node.NodeStyle.Overflow == Overflow.Scroll) || node.NodeStyle.Overflow != Overflow.Scroll)
        {
            if (FloatIsUndefined(childHeight) && !FloatIsUndefined(height))
            {
                childHeight = height;
                childHeightMeasureMode = MeasureMode.AtMost;
            }
        }

        switch (isMainAxisRow)
        {
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
                    return false;
                case true when childHeightMeasureMode == MeasureMode.Exactly:
                    child.NodeLayout.ComputedFlexBasis = Fmaxf(
                        (childHeight - marginColumn) * child.NodeStyle.AspectRatio,
                        NodePaddingAndBorderForAxis(child, FlexDirection.Row, parentWidth)
                    );
                    return false;
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

        // Measure the child.
        f.Phase = 1;
        InitLayout(
            ref frames[count],
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
        count++;
        return true;
    }

    // Iterative equivalent of NodeAbsoluteLayoutChild.
    private static bool StepAbsolute<TStorage>(Frame<TStorage>[] frames, ref int count)
        where TStorage : IList<Node<TStorage>>
    {
        ref var f = ref frames[count - 1];
        ref var d = ref f.Data.Absolute;
        var node = f.Node;
        var child = f.Child;
        var width = d.Width;
        var height = d.Height;
        var direction = d.Direction;
        var mainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);
        var crossAxis = FlexDirectionCross(mainAxis, direction);
        var isMainAxisRow = FlexDirectionIsRow(mainAxis);

        if (f.Phase == 1)
        {
            // Resume after the first (measurement) layout of the child.
            d.Cw =
                child.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                + NodeMarginForAxis(child, FlexDirection.Row, width);
            d.Ch =
                child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                + NodeMarginForAxis(child, FlexDirection.Column, width);
            f.Phase = 3;
            InitLayout(
                ref frames[count],
                child,
                d.Cw,
                d.Ch,
                direction,
                MeasureMode.Exactly,
                MeasureMode.Exactly,
                d.Cw,
                d.Ch,
                true
            );
            count++;
            return true;
        }

        if (f.Phase == 3)
        {
            // Resume after the final layout of the child; set final positions.
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

            return false;
        }

        // Phase 0.
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

            if (!isMainAxisRow && FloatIsUndefined(childWidth) && d.WidthMode != MeasureMode.Undefined && width > 0)
            {
                childWidth = width;
                childWidthMeasureMode = MeasureMode.AtMost;
            }

            d.Cw = childWidth;
            d.Ch = childHeight;
            f.Phase = 1;
            InitLayout(
                ref frames[count],
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
            count++;
            return true;
        }

        d.Cw = childWidth;
        d.Ch = childHeight;
        f.Phase = 3;
        InitLayout(
            ref frames[count],
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
        count++;
        return true;
    }

    private static bool StepLayout<TStorage>(Frame<TStorage>[] frames, ref int count)
        where TStorage : IList<Node<TStorage>>
    {
        ref var f = ref frames[count - 1];
        ref var d = ref f.Data.Layout;
        var node = f.Node;

        switch (f.Phase)
        {
            case LCache:
            {
                ref var layout = ref node.NodeLayout;

                var needToVisitNode =
                    (node.IsDirty && layout.GenerationCount != CurrentGenerationCount)
                    || layout.LastParentDirection != d.ParentDirection;

                if (needToVisitNode)
                {
                    // Invalidate the cached results.
                    layout.NextCachedMeasurementsIndex = 0;
                    layout.CachedLayout.WidthMeasureMode = (MeasureMode)(-1);
                    layout.CachedLayout.HeightMeasureMode = (MeasureMode)(-1);
                    layout.CachedLayout.ComputedWidth = -1;
                    layout.CachedLayout.ComputedHeight = -1;
                }

                d.NeedToVisitNode = needToVisitNode;

                var cachedResultsValid = false;
                float cachedComputedWidth = 0;
                float cachedComputedHeight = 0;

                if (node.MeasureFunc != null)
                {
                    var marginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, d.ParentWidth);
                    var marginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, d.ParentWidth);

                    if (
                        NodeCanUseCachedMeasurement(
                            d.WidthMeasureMode,
                            d.AvailableWidth,
                            d.HeightMeasureMode,
                            d.AvailableHeight,
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
                        for (var i = 0; i < layout.NextCachedMeasurementsIndex; i++)
                        {
                            if (
                                NodeCanUseCachedMeasurement(
                                    d.WidthMeasureMode,
                                    d.AvailableWidth,
                                    d.HeightMeasureMode,
                                    d.AvailableHeight,
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
                else if (d.PerformLayout)
                {
                    if (
                        FloatsEqual(layout.CachedLayout.AvailableWidth, d.AvailableWidth)
                        && FloatsEqual(layout.CachedLayout.AvailableHeight, d.AvailableHeight)
                        && layout.CachedLayout.WidthMeasureMode == d.WidthMeasureMode
                        && layout.CachedLayout.HeightMeasureMode == d.HeightMeasureMode
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
                            FloatsEqual(layout.CachedMeasurements[i].AvailableWidth, d.AvailableWidth)
                            && FloatsEqual(layout.CachedMeasurements[i].AvailableHeight, d.AvailableHeight)
                            && layout.CachedMeasurements[i].WidthMeasureMode == d.WidthMeasureMode
                            && layout.CachedMeasurements[i].HeightMeasureMode == d.HeightMeasureMode
                        )
                        {
                            cachedResultsValid = true;
                            cachedComputedWidth = layout.CachedMeasurements[i].ComputedWidth;
                            cachedComputedHeight = layout.CachedMeasurements[i].ComputedHeight;
                            break;
                        }
                    }
                }

                d.CachedResultsValid = cachedResultsValid;

                if (!needToVisitNode && cachedResultsValid)
                {
                    layout.MeasuredDimensions[(int)Dimension.Width] = cachedComputedWidth;
                    layout.MeasuredDimensions[(int)Dimension.Height] = cachedComputedHeight;
                    f.Phase = LFinish;
                    goto case LFinish;
                }

                f.Phase = LSetup;
                goto case LSetup;
            }

            case LSetup:
            {
                var direction = NodeResolveDirection(node, d.ParentDirection);
                node.NodeLayout.Direction = direction;
                d.Direction = direction;

                var flexRowDirection = ResolveFlexDirection(FlexDirection.Row, direction);
                var flexColumnDirection = ResolveFlexDirection(FlexDirection.Column, direction);

                node.NodeLayout.Margin[(int)Edge.Start] = NodeLeadingMargin(node, flexRowDirection, d.ParentWidth);
                node.NodeLayout.Margin[(int)Edge.End] = NodeTrailingMargin(node, flexRowDirection, d.ParentWidth);
                node.NodeLayout.Margin[(int)Edge.Top] = NodeLeadingMargin(node, flexColumnDirection, d.ParentWidth);
                node.NodeLayout.Margin[(int)Edge.Bottom] = NodeTrailingMargin(node, flexColumnDirection, d.ParentWidth);

                node.NodeLayout.Border[(int)Edge.Start] = NodeLeadingBorder(node, flexRowDirection);
                node.NodeLayout.Border[(int)Edge.End] = NodeTrailingBorder(node, flexRowDirection);
                node.NodeLayout.Border[(int)Edge.Top] = NodeLeadingBorder(node, flexColumnDirection);
                node.NodeLayout.Border[(int)Edge.Bottom] = NodeTrailingBorder(node, flexColumnDirection);

                node.NodeLayout.Padding[(int)Edge.Start] = NodeLeadingPadding(node, flexRowDirection, d.ParentWidth);
                node.NodeLayout.Padding[(int)Edge.End] = NodeTrailingPadding(node, flexRowDirection, d.ParentWidth);
                node.NodeLayout.Padding[(int)Edge.Top] = NodeLeadingPadding(node, flexColumnDirection, d.ParentWidth);
                node.NodeLayout.Padding[(int)Edge.Bottom] = NodeTrailingPadding(
                    node,
                    flexColumnDirection,
                    d.ParentWidth
                );

                if (node.MeasureFunc != null)
                {
                    NodeWithMeasureFuncSetMeasuredDimensions(
                        node,
                        d.AvailableWidth,
                        d.AvailableHeight,
                        d.WidthMeasureMode,
                        d.HeightMeasureMode,
                        d.ParentWidth,
                        d.ParentHeight
                    );
                    f.Phase = LFinish;
                    goto case LFinish;
                }

                d.ChildCount = node.Storage.Count;
                if (d.ChildCount == 0)
                {
                    NodeEmptyContainerSetMeasuredDimensions(
                        node,
                        d.AvailableWidth,
                        d.AvailableHeight,
                        d.WidthMeasureMode,
                        d.HeightMeasureMode,
                        d.ParentWidth,
                        d.ParentHeight
                    );
                    f.Phase = LFinish;
                    goto case LFinish;
                }

                if (
                    !d.PerformLayout
                    && NodeFixedSizeSetMeasuredDimensions(
                        node,
                        d.AvailableWidth,
                        d.AvailableHeight,
                        d.WidthMeasureMode,
                        d.HeightMeasureMode,
                        d.ParentWidth,
                        d.ParentHeight
                    )
                )
                {
                    f.Phase = LFinish;
                    goto case LFinish;
                }

                node.NodeLayout.HadOverflow = false;

                // STEP 1: CALCULATE VALUES FOR REMAINDER OF ALGORITHM
                d.MainAxis = ResolveFlexDirection(node.NodeStyle.FlexDirection, direction);
                d.CrossAxis = FlexDirectionCross(d.MainAxis, direction);
                d.IsMainAxisRow = FlexDirectionIsRow(d.MainAxis);
                d.JustifyContent = node.NodeStyle.JustifyContent;
                d.IsNodeFlexWrap = node.NodeStyle.FlexWrap != Wrap.NoWrap;

                d.MainAxisParentSize = d.ParentHeight;
                d.CrossAxisParentSize = d.ParentWidth;
                if (d.IsMainAxisRow)
                {
                    d.MainAxisParentSize = d.ParentWidth;
                    d.CrossAxisParentSize = d.ParentHeight;
                }

                d.LeadingPaddingAndBorderMain = NodeLeadingPaddingAndBorder(node, d.MainAxis, d.ParentWidth);
                d.TrailingPaddingAndBorderMain = NodeTrailingPaddingAndBorder(node, d.MainAxis, d.ParentWidth);
                d.LeadingPaddingAndBorderCross = NodeLeadingPaddingAndBorder(node, d.CrossAxis, d.ParentWidth);
                d.PaddingAndBorderAxisMain = NodePaddingAndBorderForAxis(node, d.MainAxis, d.ParentWidth);
                d.PaddingAndBorderAxisCross = NodePaddingAndBorderForAxis(node, d.CrossAxis, d.ParentWidth);

                d.MeasureModeMainDim = d.HeightMeasureMode;
                d.MeasureModeCrossDim = d.WidthMeasureMode;
                if (d.IsMainAxisRow)
                {
                    d.MeasureModeMainDim = d.WidthMeasureMode;
                    d.MeasureModeCrossDim = d.HeightMeasureMode;
                }

                d.PaddingAndBorderAxisRow = d.PaddingAndBorderAxisCross;
                d.PaddingAndBorderAxisColumn = d.PaddingAndBorderAxisMain;
                if (d.IsMainAxisRow)
                {
                    d.PaddingAndBorderAxisRow = d.PaddingAndBorderAxisMain;
                    d.PaddingAndBorderAxisColumn = d.PaddingAndBorderAxisCross;
                }

                d.MarginAxisRow = NodeMarginForAxis(node, FlexDirection.Row, d.ParentWidth);
                d.MarginAxisColumn = NodeMarginForAxis(node, FlexDirection.Column, d.ParentWidth);

                // STEP 2: DETERMINE AVAILABLE SIZE IN MAIN AND CROSS DIRECTIONS
                var minInnerWidth =
                    ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Width], d.ParentWidth)
                    - d.MarginAxisRow
                    - d.PaddingAndBorderAxisRow;
                var maxInnerWidth =
                    ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Width], d.ParentWidth)
                    - d.MarginAxisRow
                    - d.PaddingAndBorderAxisRow;
                var minInnerHeight =
                    ResolveValue(node.NodeStyle.MinDimensions[(int)Dimension.Height], d.ParentHeight)
                    - d.MarginAxisColumn
                    - d.PaddingAndBorderAxisColumn;
                var maxInnerHeight =
                    ResolveValue(node.NodeStyle.MaxDimensions[(int)Dimension.Height], d.ParentHeight)
                    - d.MarginAxisColumn
                    - d.PaddingAndBorderAxisColumn;

                d.MinInnerMainDim = minInnerHeight;
                d.MaxInnerMainDim = maxInnerHeight;
                if (d.IsMainAxisRow)
                {
                    d.MinInnerMainDim = minInnerWidth;
                    d.MaxInnerMainDim = maxInnerWidth;
                }

                d.AvailableInnerWidth = d.AvailableWidth - d.MarginAxisRow - d.PaddingAndBorderAxisRow;
                if (!FloatIsUndefined(d.AvailableInnerWidth))
                {
                    d.AvailableInnerWidth = Fmaxf(Fminf(d.AvailableInnerWidth, maxInnerWidth), minInnerWidth);
                }

                d.AvailableInnerHeight = d.AvailableHeight - d.MarginAxisColumn - d.PaddingAndBorderAxisColumn;
                if (!FloatIsUndefined(d.AvailableInnerHeight))
                {
                    d.AvailableInnerHeight = Fmaxf(Fminf(d.AvailableInnerHeight, maxInnerHeight), minInnerHeight);
                }

                d.AvailableInnerMainDim = d.AvailableInnerHeight;
                d.AvailableInnerCrossDim = d.AvailableInnerWidth;
                if (d.IsMainAxisRow)
                {
                    d.AvailableInnerMainDim = d.AvailableInnerWidth;
                    d.AvailableInnerCrossDim = d.AvailableInnerHeight;
                }

                d.MainAxisGap = NodeResolveGap(node, d.MainAxis, d.AvailableInnerMainDim);
                d.CrossAxisGap = NodeResolveGap(node, d.CrossAxis, d.AvailableInnerCrossDim);

                f.SingleFlexChild = null;
                if (d.MeasureModeMainDim == MeasureMode.Exactly)
                {
                    foreach (var c in node)
                    {
                        if (f.SingleFlexChild != null)
                        {
                            if (NodeIsFlex(c))
                            {
                                f.SingleFlexChild = null;
                                break;
                            }
                        }
                        else if (ResolveFlexGrow(c) > 0 && NodeResolveFlexShrink(c) > 0)
                        {
                            f.SingleFlexChild = c;
                        }
                    }
                }

                // Frames are not zeroed on reuse, so seed the absolute-child list heads.
                f.FirstAbsoluteChild = null;
                f.CurrentAbsoluteChild = null;
                d.TotalOuterFlexBasis = 0;
                d.Step3Index = 0;
                d.Step3Resuming = false;
                f.Phase = LStep3;
                goto case LStep3;
            }

            case LStep3:
            {
                // STEP 3: DETERMINE FLEX BASIS FOR EACH ITEM
                for (; d.Step3Index < d.ChildCount; d.Step3Index++)
                {
                    var child = node.Storage[d.Step3Index];
                    if (!d.Step3Resuming)
                    {
                        if (child.NodeStyle.Display == Display.None)
                        {
                            ZeroOutLayoutRecursively(child);
                            child.IsDirty = false;
                            continue;
                        }

                        ResolveDimensions(child);
                        if (d.PerformLayout)
                        {
                            var childDirection = NodeResolveDirection(child, d.Direction);
                            NodeSetPosition(
                                child,
                                childDirection,
                                d.AvailableInnerMainDim,
                                d.AvailableInnerCrossDim,
                                d.AvailableInnerWidth
                            );
                        }

                        if (child.NodeStyle.PositionType == PositionType.Absolute)
                        {
                            f.FirstAbsoluteChild ??= child;
                            f.CurrentAbsoluteChild?.NextChild = child;
                            f.CurrentAbsoluteChild = child;
                            child.NextChild = null;
                        }
                        else if (child == f.SingleFlexChild)
                        {
                            child.NodeLayout.ComputedFlexBasis = 0;
                        }
                        else
                        {
                            d.Step3Resuming = true;
                            InitFlexBasis(
                                ref frames[count],
                                node,
                                child,
                                d.AvailableInnerWidth,
                                d.WidthMeasureMode,
                                d.AvailableInnerHeight,
                                d.AvailableInnerWidth,
                                d.AvailableInnerHeight,
                                d.HeightMeasureMode,
                                d.Direction
                            );
                            count++;
                            return true;
                        }
                    }

                    d.Step3Resuming = false;
                    d.TotalOuterFlexBasis +=
                        child.NodeLayout.ComputedFlexBasis
                        + NodeMarginForAxis(child, d.MainAxis, d.AvailableInnerWidth);
                }

                d.FlexBasisOverflows = d.TotalOuterFlexBasis > d.AvailableInnerMainDim;
                if (d.MeasureModeMainDim == MeasureMode.Undefined)
                {
                    d.FlexBasisOverflows = false;
                }

                if (d is { IsNodeFlexWrap: true, FlexBasisOverflows: true, MeasureModeMainDim: MeasureMode.AtMost })
                {
                    d.MeasureModeMainDim = MeasureMode.Exactly;
                }

                d.StartOfLineIndex = 0;
                d.EndOfLineIndex = 0;
                d.LineCount = 0;
                d.TotalLineCrossDim = 0;
                d.MaxLineMainDim = 0;
                f.Phase = LLineStart;
                goto case LLineStart;
            }

            case LLineStart:
            {
                // STEP 4: COLLECT FLEX ITEMS INTO FLEX LINES (one iteration of the line loop).
                if (d.EndOfLineIndex >= d.ChildCount)
                {
                    // No more lines to lay out. Set up STEP 8 (multi-line alignment).
                    if (
                        d.PerformLayout
                        && (d.LineCount > 1 || IsBaselineLayout(node))
                        && !FloatIsUndefined(d.AvailableInnerCrossDim)
                    )
                    {
                        var remainingAlignContentDim = d.AvailableInnerCrossDim - d.TotalLineCrossDim;
                        d.CrossDimLead = 0;
                        d.CurrentLead = d.LeadingPaddingAndBorderCross;

                        switch (node.NodeStyle.AlignContent)
                        {
                            case Align.End:
                                d.CurrentLead += remainingAlignContentDim;
                                break;
                            case Align.Center:
                                d.CurrentLead += remainingAlignContentDim / 2;
                                break;
                            case Align.Stretch:
                                if (d.AvailableInnerCrossDim > d.TotalLineCrossDim)
                                {
                                    d.CrossDimLead = remainingAlignContentDim / d.LineCount;
                                }

                                break;
                            case Align.SpaceAround:
                                if (d.AvailableInnerCrossDim > d.TotalLineCrossDim)
                                {
                                    d.CurrentLead += remainingAlignContentDim / (2 * d.LineCount);
                                    if (d.LineCount > 1)
                                    {
                                        d.CrossDimLead = remainingAlignContentDim / d.LineCount;
                                    }
                                }
                                else
                                {
                                    d.CurrentLead += remainingAlignContentDim / 2;
                                }

                                break;
                            case Align.SpaceBetween:
                                if (d.AvailableInnerCrossDim > d.TotalLineCrossDim && d.LineCount > 1)
                                {
                                    d.CrossDimLead = remainingAlignContentDim / (d.LineCount - 1);
                                }

                                break;
                            case Align.Auto:
                            case Align.Start:
                            case Align.Baseline:
                                break;
                        }

                        d.EndIndex = 0;
                        d.Step8I = 0;
                        f.Phase = LStep8Line;
                        goto case LStep8Line;
                    }

                    f.Phase = LFinalDims;
                    goto case LFinalDims;
                }

                if (d.LineCount > 0)
                {
                    d.TotalLineCrossDim += d.CrossAxisGap;
                }

                d.ItemsOnLine = 0;
                d.SizeConsumedOnCurrentLine = 0;
                d.SizeConsumedOnCurrentLineIncludingMinConstraint = 0;
                d.TotalFlexGrowFactors = 0;
                d.TotalFlexShrinkScaledFactors = 0;
                f.FirstRelativeChild = null;
                f.CurrentRelativeChild = null;

                for (var i = d.StartOfLineIndex; i < d.ChildCount; i++)
                {
                    var child = node.Storage[i];
                    if (child.NodeStyle.Display == Display.None)
                    {
                        d.EndOfLineIndex++;
                        continue;
                    }

                    child.LineIndex = d.LineCount;

                    if (child.NodeStyle.PositionType != PositionType.Absolute)
                    {
                        var childMarginMainAxis = NodeMarginForAxis(child, d.MainAxis, d.AvailableInnerWidth);
                        var childLeadingGapMainAxis = d.ItemsOnLine > 0 ? d.MainAxisGap : 0;
                        var flexBasisWithMaxConstraints = Fminf(
                            ResolveValue(
                                child.NodeStyle.MaxDimensions[(int)Dim[(int)d.MainAxis]],
                                d.MainAxisParentSize
                            ),
                            child.NodeLayout.ComputedFlexBasis
                        );
                        var flexBasisWithMinAndMaxConstraints = Fmaxf(
                            ResolveValue(
                                child.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]],
                                d.MainAxisParentSize
                            ),
                            flexBasisWithMaxConstraints
                        );

                        if (
                            d.SizeConsumedOnCurrentLineIncludingMinConstraint
                                + flexBasisWithMinAndMaxConstraints
                                + childMarginMainAxis
                                + childLeadingGapMainAxis
                                > d.AvailableInnerMainDim
                            && d is { IsNodeFlexWrap: true, ItemsOnLine: > 0 }
                        )
                        {
                            break;
                        }

                        d.SizeConsumedOnCurrentLineIncludingMinConstraint +=
                            flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;
                        d.SizeConsumedOnCurrentLine +=
                            flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;
                        d.ItemsOnLine++;

                        if (NodeIsFlex(child))
                        {
                            d.TotalFlexGrowFactors += ResolveFlexGrow(child);
                            d.TotalFlexShrinkScaledFactors +=
                                -NodeResolveFlexShrink(child) * child.NodeLayout.ComputedFlexBasis;
                        }

                        f.FirstRelativeChild ??= child;
                        f.CurrentRelativeChild?.NextChild = child;
                        f.CurrentRelativeChild = child;
                        child.NextChild = null;
                    }

                    d.EndOfLineIndex++;
                }

                if (d.TotalFlexGrowFactors is > 0 and < 1)
                {
                    d.TotalFlexGrowFactors = 1;
                }

                if (d.TotalFlexShrinkScaledFactors is > 0 and < 1)
                {
                    d.TotalFlexShrinkScaledFactors = 1;
                }

                d.CanSkipFlex = d is { PerformLayout: false, MeasureModeCrossDim: MeasureMode.Exactly };

                d.LeadingMainDim = 0;
                d.BetweenMainDim = 0;

                // STEP 5: RESOLVING FLEXIBLE LENGTHS ON MAIN AXIS
                if (d.MeasureModeMainDim != MeasureMode.Exactly)
                {
                    if (!FloatIsUndefined(d.MinInnerMainDim) && d.SizeConsumedOnCurrentLine < d.MinInnerMainDim)
                    {
                        d.AvailableInnerMainDim = d.MinInnerMainDim;
                    }
                    else if (!FloatIsUndefined(d.MaxInnerMainDim) && d.SizeConsumedOnCurrentLine > d.MaxInnerMainDim)
                    {
                        d.AvailableInnerMainDim = d.MaxInnerMainDim;
                    }
                    else
                    {
                        if (d.TotalFlexGrowFactors == 0 || ResolveFlexGrow(node) == 0)
                        {
                            d.AvailableInnerMainDim = d.SizeConsumedOnCurrentLine;
                        }
                    }
                }

                d.RemainingFreeSpace = 0;
                if (!FloatIsUndefined(d.AvailableInnerMainDim))
                {
                    d.RemainingFreeSpace = d.AvailableInnerMainDim - d.SizeConsumedOnCurrentLine;
                }
                else if (d.SizeConsumedOnCurrentLine < 0)
                {
                    d.RemainingFreeSpace = -d.SizeConsumedOnCurrentLine;
                }

                d.OriginalRemainingFreeSpace = d.RemainingFreeSpace;
                d.DeltaFreeSpace = 0;

                if (!d.CanSkipFlex)
                {
                    // First pass: detect the flex items whose min/max constraints trigger.
                    float deltaFlexShrinkScaledFactors = 0;
                    float deltaFlexGrowFactors = 0;
                    var crc = f.FirstRelativeChild;
                    while (crc != null)
                    {
                        var childFlexBasis = Fminf(
                            ResolveValue(crc.NodeStyle.MaxDimensions[(int)Dim[(int)d.MainAxis]], d.MainAxisParentSize),
                            Fmaxf(
                                ResolveValue(
                                    crc.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]],
                                    d.MainAxisParentSize
                                ),
                                crc.NodeLayout.ComputedFlexBasis
                            )
                        );

                        switch (d.RemainingFreeSpace)
                        {
                            case < 0:
                            {
                                var flexShrinkScaledFactor = -NodeResolveFlexShrink(crc) * childFlexBasis;
                                if (flexShrinkScaledFactor != 0)
                                {
                                    var baseMainSize =
                                        childFlexBasis
                                        + d.RemainingFreeSpace
                                            / d.TotalFlexShrinkScaledFactors
                                            * flexShrinkScaledFactor;
                                    var boundMainSize = NodeBoundAxis(
                                        crc,
                                        d.MainAxis,
                                        baseMainSize,
                                        d.AvailableInnerMainDim,
                                        d.AvailableInnerWidth
                                    );
                                    if (baseMainSize != boundMainSize)
                                    {
                                        d.DeltaFreeSpace -= boundMainSize - childFlexBasis;
                                        deltaFlexShrinkScaledFactors -= flexShrinkScaledFactor;
                                    }
                                }

                                break;
                            }
                            case > 0:
                            {
                                var flexGrowFactor = ResolveFlexGrow(crc);
                                if (flexGrowFactor != 0)
                                {
                                    var baseMainSize =
                                        childFlexBasis + d.RemainingFreeSpace / d.TotalFlexGrowFactors * flexGrowFactor;
                                    var boundMainSize = NodeBoundAxis(
                                        crc,
                                        d.MainAxis,
                                        baseMainSize,
                                        d.AvailableInnerMainDim,
                                        d.AvailableInnerWidth
                                    );
                                    if (baseMainSize != boundMainSize)
                                    {
                                        d.DeltaFreeSpace -= boundMainSize - childFlexBasis;
                                        deltaFlexGrowFactors -= flexGrowFactor;
                                    }
                                }

                                break;
                            }
                        }

                        crc = crc.NextChild;
                    }

                    d.TotalFlexShrinkScaledFactors += deltaFlexShrinkScaledFactors;
                    d.TotalFlexGrowFactors += deltaFlexGrowFactors;
                    d.RemainingFreeSpace += d.DeltaFreeSpace;

                    // Second pass: resolve the sizes of the flexible items.
                    d.DeltaFreeSpace = 0;
                    f.CurrentRelativeChild = f.FirstRelativeChild;
                    d.Pass2Resuming = false;
                    f.Phase = LPass2;
                    goto case LPass2;
                }

                f.Phase = LAfterFlex;
                goto case LAfterFlex;
            }

            case LPass2:
            {
                while (f.CurrentRelativeChild != null)
                {
                    if (!d.Pass2Resuming)
                    {
                        var crc = f.CurrentRelativeChild;
                        var childFlexBasis = Fminf(
                            ResolveValue(crc.NodeStyle.MaxDimensions[(int)Dim[(int)d.MainAxis]], d.MainAxisParentSize),
                            Fmaxf(
                                ResolveValue(
                                    crc.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]],
                                    d.MainAxisParentSize
                                ),
                                crc.NodeLayout.ComputedFlexBasis
                            )
                        );
                        var updatedMainSize = childFlexBasis;

                        switch (d.RemainingFreeSpace)
                        {
                            case < 0:
                            {
                                var flexShrinkScaledFactor = -NodeResolveFlexShrink(crc) * childFlexBasis;
                                if (flexShrinkScaledFactor != 0)
                                {
                                    float childSize;
                                    if (d.TotalFlexShrinkScaledFactors == 0)
                                    {
                                        childSize = childFlexBasis + flexShrinkScaledFactor;
                                    }
                                    else
                                    {
                                        childSize =
                                            childFlexBasis
                                            + d.RemainingFreeSpace
                                                / d.TotalFlexShrinkScaledFactors
                                                * flexShrinkScaledFactor;
                                    }

                                    updatedMainSize = NodeBoundAxis(
                                        crc,
                                        d.MainAxis,
                                        childSize,
                                        d.AvailableInnerMainDim,
                                        d.AvailableInnerWidth
                                    );
                                }

                                break;
                            }
                            case > 0:
                            {
                                var flexGrowFactor = ResolveFlexGrow(crc);
                                if (flexGrowFactor != 0)
                                {
                                    updatedMainSize = NodeBoundAxis(
                                        crc,
                                        d.MainAxis,
                                        childFlexBasis + d.RemainingFreeSpace / d.TotalFlexGrowFactors * flexGrowFactor,
                                        d.AvailableInnerMainDim,
                                        d.AvailableInnerWidth
                                    );
                                }

                                break;
                            }
                        }

                        d.DeltaFreeSpace -= updatedMainSize - childFlexBasis;

                        var marginMain = NodeMarginForAxis(crc, d.MainAxis, d.AvailableInnerWidth);
                        var marginCross = NodeMarginForAxis(crc, d.CrossAxis, d.AvailableInnerWidth);

                        float childCrossSize;
                        var childMainSize = updatedMainSize + marginMain;
                        MeasureMode childCrossMeasureMode;
                        var childMainMeasureMode = MeasureMode.Exactly;

                        if (
                            !FloatIsUndefined(d.AvailableInnerCrossDim)
                            && !NodeIsStyleDimDefined(crc, d.CrossAxis, d.AvailableInnerCrossDim)
                            && d.MeasureModeCrossDim == MeasureMode.Exactly
                            && !(d is { IsNodeFlexWrap: true, FlexBasisOverflows: true })
                            && NodeAlignItem(node, crc) == Align.Stretch
                        )
                        {
                            childCrossSize = d.AvailableInnerCrossDim;
                            childCrossMeasureMode = MeasureMode.Exactly;
                        }
                        else if (!NodeIsStyleDimDefined(crc, d.CrossAxis, d.AvailableInnerCrossDim))
                        {
                            childCrossSize = d.AvailableInnerCrossDim;
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
                                    crc.ResolvedDimensions[(int)Dim[(int)d.CrossAxis]],
                                    d.AvailableInnerCrossDim
                                ) + marginCross;
                            var isLoosePercentageMeasurement =
                                crc.ResolvedDimensions[(int)Dim[(int)d.CrossAxis]].Unit == Unit.Percent
                                && d.MeasureModeCrossDim != MeasureMode.Exactly;
                            childCrossMeasureMode = MeasureMode.Exactly;
                            if (FloatIsUndefined(childCrossSize) || isLoosePercentageMeasurement)
                            {
                                childCrossMeasureMode = MeasureMode.Undefined;
                            }
                        }

                        if (!FloatIsUndefined(crc.NodeStyle.AspectRatio))
                        {
                            var v = (childMainSize - marginMain) * crc.NodeStyle.AspectRatio;
                            if (d.IsMainAxisRow)
                            {
                                v = (childMainSize - marginMain) / crc.NodeStyle.AspectRatio;
                            }

                            childCrossSize = Fmaxf(
                                v,
                                NodePaddingAndBorderForAxis(crc, d.CrossAxis, d.AvailableInnerWidth)
                            );
                            childCrossMeasureMode = MeasureMode.Exactly;

                            if (NodeIsFlex(crc))
                            {
                                childCrossSize = Fminf(childCrossSize - marginCross, d.AvailableInnerCrossDim);
                                childMainSize = marginMain;
                                if (d.IsMainAxisRow)
                                {
                                    childMainSize += childCrossSize * crc.NodeStyle.AspectRatio;
                                }
                                else
                                {
                                    childMainSize += childCrossSize / crc.NodeStyle.AspectRatio;
                                }
                            }

                            childCrossSize += marginCross;
                        }

                        ConstrainMaxSizeForMode(
                            crc,
                            d.MainAxis,
                            d.AvailableInnerMainDim,
                            d.AvailableInnerWidth,
                            ref childMainMeasureMode,
                            ref childMainSize
                        );
                        ConstrainMaxSizeForMode(
                            crc,
                            d.CrossAxis,
                            d.AvailableInnerCrossDim,
                            d.AvailableInnerWidth,
                            ref childCrossMeasureMode,
                            ref childCrossSize
                        );

                        var requiresStretchLayout =
                            !NodeIsStyleDimDefined(crc, d.CrossAxis, d.AvailableInnerCrossDim)
                            && NodeAlignItem(node, crc) == Align.Stretch;

                        var childWidth = childCrossSize;
                        if (d.IsMainAxisRow)
                        {
                            childWidth = childMainSize;
                        }

                        var childHeight = childCrossSize;
                        if (!d.IsMainAxisRow)
                        {
                            childHeight = childMainSize;
                        }

                        var childWidthMeasureMode = childCrossMeasureMode;
                        if (d.IsMainAxisRow)
                        {
                            childWidthMeasureMode = childMainMeasureMode;
                        }

                        var childHeightMeasureMode = childCrossMeasureMode;
                        if (!d.IsMainAxisRow)
                        {
                            childHeightMeasureMode = childMainMeasureMode;
                        }

                        d.Pass2Resuming = true;
                        InitLayout(
                            ref frames[count],
                            crc,
                            childWidth,
                            childHeight,
                            d.Direction,
                            childWidthMeasureMode,
                            childHeightMeasureMode,
                            d.AvailableInnerWidth,
                            d.AvailableInnerHeight,
                            d.PerformLayout && !requiresStretchLayout
                        );
                        count++;
                        return true;
                    }

                    d.Pass2Resuming = false;
                    if (f.CurrentRelativeChild!.NodeLayout.HadOverflow)
                    {
                        node.NodeLayout.HadOverflow = true;
                    }

                    f.CurrentRelativeChild = f.CurrentRelativeChild.NextChild;
                }

                f.Phase = LAfterFlex;
                goto case LAfterFlex;
            }

            case LAfterFlex:
            {
                d.RemainingFreeSpace = d.OriginalRemainingFreeSpace + d.DeltaFreeSpace;
                if (d.RemainingFreeSpace < 0)
                {
                    node.NodeLayout.HadOverflow = true;
                }

                // STEP 6: MAIN-AXIS JUSTIFICATION & CROSS-AXIS SIZE DETERMINATION
                if (d is { MeasureModeMainDim: MeasureMode.AtMost, RemainingFreeSpace: > 0 })
                {
                    if (
                        node.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]].Unit != Unit.Undefined
                        && ResolveValue(node.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]], d.MainAxisParentSize)
                            >= 0
                    )
                    {
                        d.RemainingFreeSpace = Fmaxf(
                            0,
                            ResolveValue(node.NodeStyle.MinDimensions[(int)Dim[(int)d.MainAxis]], d.MainAxisParentSize)
                                - (d.AvailableInnerMainDim - d.RemainingFreeSpace)
                        );
                    }
                    else
                    {
                        d.RemainingFreeSpace = 0;
                    }
                }

                d.NumberOfAutoMarginsOnCurrentLine = 0;
                for (var i = d.StartOfLineIndex; i < d.EndOfLineIndex; i++)
                {
                    var child = node.Storage[i];
                    if (child.NodeStyle.PositionType == PositionType.Relative)
                    {
                        if (MarginLeadingValue(child, d.MainAxis).Unit == Unit.Auto)
                        {
                            d.NumberOfAutoMarginsOnCurrentLine++;
                        }

                        if (MarginTrailingValue(child, d.MainAxis).Unit == Unit.Auto)
                        {
                            d.NumberOfAutoMarginsOnCurrentLine++;
                        }
                    }
                }

                if (d.NumberOfAutoMarginsOnCurrentLine == 0)
                {
                    switch (d.JustifyContent)
                    {
                        case Justify.Center:
                            d.LeadingMainDim = d.RemainingFreeSpace / 2;
                            break;
                        case Justify.End:
                            d.LeadingMainDim = d.RemainingFreeSpace;
                            break;
                        case Justify.SpaceBetween:
                            if (d.ItemsOnLine > 1)
                            {
                                d.BetweenMainDim = Fmaxf(d.RemainingFreeSpace, 0) / (d.ItemsOnLine - 1);
                            }
                            else
                            {
                                d.BetweenMainDim = 0;
                            }

                            break;
                        case Justify.SpaceAround:
                            d.BetweenMainDim = d.RemainingFreeSpace / d.ItemsOnLine;
                            d.LeadingMainDim = d.BetweenMainDim / 2;
                            break;
                        case Justify.SpaceEvenly:
                            d.BetweenMainDim = d.RemainingFreeSpace / (d.ItemsOnLine + 1);
                            d.LeadingMainDim = d.BetweenMainDim;
                            break;
                        case Justify.Start:
                            break;
                    }
                }

                d.MainDim = d.LeadingPaddingAndBorderMain + d.LeadingMainDim;
                d.CrossDim = 0;
                d.IsFirstInFlowChildOnLine = true;

                for (var i = d.StartOfLineIndex; i < d.EndOfLineIndex; i++)
                {
                    var child = node.Storage[i];
                    if (child.NodeStyle.Display == Display.None)
                    {
                        continue;
                    }

                    switch (child.NodeStyle.PositionType)
                    {
                        case PositionType.Absolute when NodeIsLeadingPosDefined(child, d.MainAxis):
                            if (d.PerformLayout)
                            {
                                child.NodeLayout.Position[(int)Pos[(int)d.MainAxis]] =
                                    NodeLeadingPosition(child, d.MainAxis, d.AvailableInnerMainDim)
                                    + NodeLeadingBorder(node, d.MainAxis)
                                    + NodeLeadingMargin(child, d.MainAxis, d.AvailableInnerWidth);
                            }

                            break;
                        case PositionType.Relative:
                            if (!d.IsFirstInFlowChildOnLine)
                            {
                                d.MainDim += d.MainAxisGap;
                            }

                            d.IsFirstInFlowChildOnLine = false;

                            if (MarginLeadingValue(child, d.MainAxis).Unit == Unit.Auto)
                            {
                                d.MainDim += d.RemainingFreeSpace / d.NumberOfAutoMarginsOnCurrentLine;
                            }

                            if (d.PerformLayout)
                            {
                                child.NodeLayout.Position[(int)Pos[(int)d.MainAxis]] += d.MainDim;
                            }

                            if (MarginTrailingValue(child, d.MainAxis).Unit == Unit.Auto)
                            {
                                d.MainDim += d.RemainingFreeSpace / d.NumberOfAutoMarginsOnCurrentLine;
                            }

                            if (d.CanSkipFlex)
                            {
                                d.MainDim +=
                                    d.BetweenMainDim
                                    + NodeMarginForAxis(child, d.MainAxis, d.AvailableInnerWidth)
                                    + child.NodeLayout.ComputedFlexBasis;
                                d.CrossDim = d.AvailableInnerCrossDim;
                            }
                            else
                            {
                                d.MainDim +=
                                    d.BetweenMainDim + NodeDimWithMargin(child, d.MainAxis, d.AvailableInnerWidth);
                                d.CrossDim = Fmaxf(
                                    d.CrossDim,
                                    NodeDimWithMargin(child, d.CrossAxis, d.AvailableInnerWidth)
                                );
                            }

                            break;
                        default:
                            if (d.PerformLayout)
                            {
                                child.NodeLayout.Position[(int)Pos[(int)d.MainAxis]] +=
                                    NodeLeadingBorder(node, d.MainAxis) + d.LeadingMainDim;
                            }

                            break;
                    }
                }

                d.MainDim += d.TrailingPaddingAndBorderMain;

                d.ContainerCrossAxis = d.AvailableInnerCrossDim;
                if (d.MeasureModeCrossDim is MeasureMode.Undefined or MeasureMode.AtMost)
                {
                    d.ContainerCrossAxis =
                        NodeBoundAxis(
                            node,
                            d.CrossAxis,
                            d.CrossDim + d.PaddingAndBorderAxisCross,
                            d.CrossAxisParentSize,
                            d.ParentWidth
                        ) - d.PaddingAndBorderAxisCross;
                }

                if (d is { IsNodeFlexWrap: false, MeasureModeCrossDim: MeasureMode.Exactly })
                {
                    d.CrossDim = d.AvailableInnerCrossDim;
                }

                d.CrossDim =
                    NodeBoundAxis(
                        node,
                        d.CrossAxis,
                        d.CrossDim + d.PaddingAndBorderAxisCross,
                        d.CrossAxisParentSize,
                        d.ParentWidth
                    ) - d.PaddingAndBorderAxisCross;

                d.Step7Index = d.StartOfLineIndex;
                d.Step7Resuming = false;
                f.Phase = LStep7;
                goto case LStep7;
            }

            case LStep7:
            {
                // STEP 7: CROSS-AXIS ALIGNMENT
                if (d.PerformLayout)
                {
                    for (; d.Step7Index < d.EndOfLineIndex; d.Step7Index++)
                    {
                        var child = node.Storage[d.Step7Index];
                        if (!d.Step7Resuming)
                        {
                            if (child.NodeStyle.Display == Display.None)
                            {
                                continue;
                            }

                            if (child.NodeStyle.PositionType == PositionType.Absolute)
                            {
                                if (NodeIsLeadingPosDefined(child, d.CrossAxis))
                                {
                                    child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                        NodeLeadingPosition(child, d.CrossAxis, d.AvailableInnerCrossDim)
                                        + NodeLeadingBorder(node, d.CrossAxis)
                                        + NodeLeadingMargin(child, d.CrossAxis, d.AvailableInnerWidth);
                                }
                                else
                                {
                                    child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                        NodeLeadingBorder(node, d.CrossAxis)
                                        + NodeLeadingMargin(child, d.CrossAxis, d.AvailableInnerWidth);
                                }

                                continue;
                            }

                            d.Step7LeadingCrossDim = d.LeadingPaddingAndBorderCross;
                            var alignItem = NodeAlignItem(node, child);

                            if (
                                alignItem == Align.Stretch
                                && MarginLeadingValue(child, d.CrossAxis).Unit != Unit.Auto
                                && MarginTrailingValue(child, d.CrossAxis).Unit != Unit.Auto
                            )
                            {
                                if (!NodeIsStyleDimDefined(child, d.CrossAxis, d.AvailableInnerCrossDim))
                                {
                                    var childMainSize = child.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.MainAxis]];
                                    var childCrossSize = d.CrossDim;
                                    if (!FloatIsUndefined(child.NodeStyle.AspectRatio))
                                    {
                                        childCrossSize = NodeMarginForAxis(child, d.CrossAxis, d.AvailableInnerWidth);
                                        if (d.IsMainAxisRow)
                                        {
                                            childCrossSize += childMainSize / child.NodeStyle.AspectRatio;
                                        }
                                        else
                                        {
                                            childCrossSize += childMainSize * child.NodeStyle.AspectRatio;
                                        }
                                    }

                                    childMainSize += NodeMarginForAxis(child, d.MainAxis, d.AvailableInnerWidth);

                                    var childMainMeasureMode = MeasureMode.Exactly;
                                    var childCrossMeasureMode = MeasureMode.Exactly;
                                    ConstrainMaxSizeForMode(
                                        child,
                                        d.MainAxis,
                                        d.AvailableInnerMainDim,
                                        d.AvailableInnerWidth,
                                        ref childMainMeasureMode,
                                        ref childMainSize
                                    );
                                    ConstrainMaxSizeForMode(
                                        child,
                                        d.CrossAxis,
                                        d.AvailableInnerCrossDim,
                                        d.AvailableInnerWidth,
                                        ref childCrossMeasureMode,
                                        ref childCrossSize
                                    );

                                    var childWidth = childCrossSize;
                                    if (d.IsMainAxisRow)
                                    {
                                        childWidth = childMainSize;
                                    }

                                    var childHeight = childCrossSize;
                                    if (!d.IsMainAxisRow)
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

                                    d.Step7Resuming = true;
                                    InitLayout(
                                        ref frames[count],
                                        child,
                                        childWidth,
                                        childHeight,
                                        d.Direction,
                                        childWidthMeasureMode,
                                        childHeightMeasureMode,
                                        d.AvailableInnerWidth,
                                        d.AvailableInnerHeight,
                                        true
                                    );
                                    count++;
                                    return true;
                                }
                            }
                            else
                            {
                                var remainingCrossDim =
                                    d.ContainerCrossAxis - NodeDimWithMargin(child, d.CrossAxis, d.AvailableInnerWidth);

                                if (
                                    MarginLeadingValue(child, d.CrossAxis).Unit == Unit.Auto
                                    && MarginTrailingValue(child, d.CrossAxis).Unit == Unit.Auto
                                )
                                {
                                    d.Step7LeadingCrossDim += Fmaxf(0, remainingCrossDim / 2);
                                }
                                else if (MarginTrailingValue(child, d.CrossAxis).Unit == Unit.Auto)
                                {
                                    // No-Op
                                }
                                else if (MarginLeadingValue(child, d.CrossAxis).Unit == Unit.Auto)
                                {
                                    d.Step7LeadingCrossDim += Fmaxf(0, remainingCrossDim);
                                }
                                else
                                {
                                    switch (alignItem)
                                    {
                                        case Align.Start:
                                            break;
                                        case Align.Center:
                                            d.Step7LeadingCrossDim += remainingCrossDim / 2;
                                            break;
                                        default:
                                            d.Step7LeadingCrossDim += remainingCrossDim;
                                            break;
                                    }
                                }
                            }
                        }

                        d.Step7Resuming = false;
                        node.Storage[d.Step7Index].NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] +=
                            d.TotalLineCrossDim + d.Step7LeadingCrossDim;
                    }
                }

                f.Phase = LLineEnd;
                goto case LLineEnd;
            }

            case LLineEnd:
            {
                d.TotalLineCrossDim += d.CrossDim;
                d.MaxLineMainDim = Fmaxf(d.MaxLineMainDim, d.MainDim);
                d.LineCount++;
                d.StartOfLineIndex = d.EndOfLineIndex;
                f.Phase = LLineStart;
                goto case LLineStart;
            }

            case LStep8Line:
            {
                // STEP 8: MULTI-LINE CONTENT ALIGNMENT (per-line measurement pass).
                if (d.Step8I >= d.LineCount)
                {
                    f.Phase = LFinalDims;
                    goto case LFinalDims;
                }

                d.StartIndex = d.EndIndex;
                d.LineHeight = 0;
                d.MaxAscentForCurrentLine = 0;
                d.MaxDescentForCurrentLine = 0;

                int ii;
                for (ii = d.StartIndex; ii < d.ChildCount; ii++)
                {
                    var child = node.Storage[ii];
                    if (child.NodeStyle.Display == Display.None)
                    {
                        continue;
                    }

                    if (child.NodeStyle.PositionType == PositionType.Relative)
                    {
                        if (child.LineIndex != d.Step8I)
                        {
                            break;
                        }

                        if (NodeIsLayoutDimDefined(child, d.CrossAxis))
                        {
                            d.LineHeight = Fmaxf(
                                d.LineHeight,
                                child.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]]
                                    + NodeMarginForAxis(child, d.CrossAxis, d.AvailableInnerWidth)
                            );
                        }

                        if (NodeAlignItem(node, child) == Align.Baseline)
                        {
                            var ascent =
                                Baseline(child) + NodeLeadingMargin(child, FlexDirection.Column, d.AvailableInnerWidth);
                            var descent =
                                child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                                + NodeMarginForAxis(child, FlexDirection.Column, d.AvailableInnerWidth)
                                - ascent;
                            d.MaxAscentForCurrentLine = Fmaxf(d.MaxAscentForCurrentLine, ascent);
                            d.MaxDescentForCurrentLine = Fmaxf(d.MaxDescentForCurrentLine, descent);
                            d.LineHeight = Fmaxf(d.LineHeight, d.MaxAscentForCurrentLine + d.MaxDescentForCurrentLine);
                        }
                    }
                }

                d.EndIndex = ii;
                d.LineHeight += d.CrossDimLead;
                d.Step8Ii = d.StartIndex;
                d.Step8Resuming = false;
                f.Phase = LStep8Place;
                goto case LStep8Place;
            }

            case LStep8Place:
            {
                if (d.PerformLayout)
                {
                    for (; d.Step8Ii < d.EndIndex; d.Step8Ii++)
                    {
                        var child = node.Storage[d.Step8Ii];
                        if (!d.Step8Resuming)
                        {
                            if (child.NodeStyle.Display == Display.None)
                            {
                                continue;
                            }

                            if (child.NodeStyle.PositionType == PositionType.Relative)
                            {
                                switch (NodeAlignItem(node, child))
                                {
                                    case Align.Start:
                                        child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                            d.CurrentLead
                                            + NodeLeadingMargin(child, d.CrossAxis, d.AvailableInnerWidth);
                                        break;
                                    case Align.End:
                                        child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                            d.CurrentLead
                                            + d.LineHeight
                                            - NodeTrailingMargin(child, d.CrossAxis, d.AvailableInnerWidth)
                                            - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]];
                                        break;
                                    case Align.Center:
                                    {
                                        var childHeight = child.NodeLayout.MeasuredDimensions[
                                            (int)Dim[(int)d.CrossAxis]
                                        ];
                                        child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                            d.CurrentLead + (d.LineHeight - childHeight) / 2;
                                        break;
                                    }
                                    case Align.Stretch:
                                    {
                                        child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                            d.CurrentLead
                                            + NodeLeadingMargin(child, d.CrossAxis, d.AvailableInnerWidth);

                                        if (!NodeIsStyleDimDefined(child, d.CrossAxis, d.AvailableInnerCrossDim))
                                        {
                                            var childWidth = d.LineHeight;
                                            if (d.IsMainAxisRow)
                                            {
                                                childWidth =
                                                    child.NodeLayout.MeasuredDimensions[(int)Dimension.Width]
                                                    + NodeMarginForAxis(child, d.MainAxis, d.AvailableInnerWidth);
                                            }

                                            var childHeight = d.LineHeight;
                                            if (!d.IsMainAxisRow)
                                            {
                                                childHeight =
                                                    child.NodeLayout.MeasuredDimensions[(int)Dimension.Height]
                                                    + NodeMarginForAxis(child, d.CrossAxis, d.AvailableInnerWidth);
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
                                                d.Step8Resuming = true;
                                                InitLayout(
                                                    ref frames[count],
                                                    child,
                                                    childWidth,
                                                    childHeight,
                                                    d.Direction,
                                                    MeasureMode.Exactly,
                                                    MeasureMode.Exactly,
                                                    d.AvailableInnerWidth,
                                                    d.AvailableInnerHeight,
                                                    true
                                                );
                                                count++;
                                                return true;
                                            }
                                        }

                                        break;
                                    }
                                    case Align.Baseline:
                                        child.NodeLayout.Position[(int)Edge.Top] =
                                            d.CurrentLead
                                            + d.MaxAscentForCurrentLine
                                            - Baseline(child)
                                            + NodeLeadingPosition(
                                                child,
                                                FlexDirection.Column,
                                                d.AvailableInnerCrossDim
                                            );
                                        break;
                                    case Align.Auto:
                                    case Align.SpaceBetween:
                                    case Align.SpaceAround:
                                        break;
                                }
                            }
                        }

                        d.Step8Resuming = false;
                    }
                }

                d.CurrentLead += d.LineHeight;
                if (d.Step8I < d.LineCount - 1)
                {
                    d.CurrentLead += d.CrossAxisGap;
                }

                d.Step8I++;
                f.Phase = LStep8Line;
                goto case LStep8Line;
            }

            case LFinalDims:
            {
                // STEP 9: COMPUTING FINAL DIMENSIONS
                node.NodeLayout.MeasuredDimensions[(int)Dimension.Width] = NodeBoundAxis(
                    node,
                    FlexDirection.Row,
                    d.AvailableWidth - d.MarginAxisRow,
                    d.ParentWidth,
                    d.ParentWidth
                );
                node.NodeLayout.MeasuredDimensions[(int)Dimension.Height] = NodeBoundAxis(
                    node,
                    FlexDirection.Column,
                    d.AvailableHeight - d.MarginAxisColumn,
                    d.ParentHeight,
                    d.ParentWidth
                );

                if (
                    d.MeasureModeMainDim == MeasureMode.Undefined
                    || (node.NodeStyle.Overflow != Overflow.Scroll && d.MeasureModeMainDim == MeasureMode.AtMost)
                )
                {
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.MainAxis]] = NodeBoundAxis(
                        node,
                        d.MainAxis,
                        d.MaxLineMainDim,
                        d.MainAxisParentSize,
                        d.ParentWidth
                    );
                }
                else if (d.MeasureModeMainDim == MeasureMode.AtMost && node.NodeStyle.Overflow == Overflow.Scroll)
                {
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.MainAxis]] = Fmaxf(
                        Fminf(
                            d.AvailableInnerMainDim + d.PaddingAndBorderAxisMain,
                            NodeBoundAxisWithinMinAndMax(node, d.MainAxis, d.MaxLineMainDim, d.MainAxisParentSize)
                        ),
                        d.PaddingAndBorderAxisMain
                    );
                }

                if (
                    d.MeasureModeCrossDim == MeasureMode.Undefined
                    || (node.NodeStyle.Overflow != Overflow.Scroll && d.MeasureModeCrossDim == MeasureMode.AtMost)
                )
                {
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]] = NodeBoundAxis(
                        node,
                        d.CrossAxis,
                        d.TotalLineCrossDim + d.PaddingAndBorderAxisCross,
                        d.CrossAxisParentSize,
                        d.ParentWidth
                    );
                }
                else if (d.MeasureModeCrossDim == MeasureMode.AtMost && node.NodeStyle.Overflow == Overflow.Scroll)
                {
                    node.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]] = Fmaxf(
                        Fminf(
                            d.AvailableInnerCrossDim + d.PaddingAndBorderAxisCross,
                            NodeBoundAxisWithinMinAndMax(
                                node,
                                d.CrossAxis,
                                d.TotalLineCrossDim + d.PaddingAndBorderAxisCross,
                                d.CrossAxisParentSize
                            )
                        ),
                        d.PaddingAndBorderAxisCross
                    );
                }

                // As we only wrapped in normal direction yet, we need to reverse the positions on wrap-reverse.
                if (d.PerformLayout && node.NodeStyle.FlexWrap == Wrap.WrapReverse)
                {
                    foreach (var child in node)
                    {
                        if (child.NodeStyle.PositionType == PositionType.Relative)
                        {
                            child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]] =
                                node.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]]
                                - child.NodeLayout.Position[(int)Pos[(int)d.CrossAxis]]
                                - child.NodeLayout.MeasuredDimensions[(int)Dim[(int)d.CrossAxis]];
                        }
                    }
                }

                if (!d.PerformLayout)
                {
                    f.Phase = LFinish;
                    goto case LFinish;
                }

                f.CurrentAbsoluteChild = f.FirstAbsoluteChild;
                d.Step10Resuming = false;
                f.Phase = LAbsolute;
                goto case LAbsolute;
            }

            case LAbsolute:
            {
                // STEP 10: SIZING AND POSITIONING ABSOLUTE CHILDREN
                while (f.CurrentAbsoluteChild != null)
                {
                    if (!d.Step10Resuming)
                    {
                        var mode = d.MeasureModeCrossDim;
                        if (d.IsMainAxisRow)
                        {
                            mode = d.MeasureModeMainDim;
                        }

                        d.Step10Resuming = true;
                        InitAbsolute(
                            ref frames[count],
                            node,
                            f.CurrentAbsoluteChild,
                            d.AvailableInnerWidth,
                            mode,
                            d.AvailableInnerHeight,
                            d.Direction
                        );
                        count++;
                        return true;
                    }

                    d.Step10Resuming = false;
                    f.CurrentAbsoluteChild = f.CurrentAbsoluteChild.NextChild;
                }

                // STEP 11: SETTING TRAILING POSITIONS FOR CHILDREN
                var needsMainTrailingPos = d.MainAxis is FlexDirection.RowReverse or FlexDirection.ColumnReverse;
                var needsCrossTrailingPos = d.CrossAxis is FlexDirection.RowReverse or FlexDirection.ColumnReverse;

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
                            NodeSetChildTrailingPosition(node, child, d.MainAxis);
                        }

                        if (needsCrossTrailingPos)
                        {
                            NodeSetChildTrailingPosition(node, child, d.CrossAxis);
                        }
                    }
                }

                f.Phase = LFinish;
                goto case LFinish;
            }

            case LFinish:
            {
                // Cache-wrapper finalize (old LayoutNodeInternal tail).
                ref var layout = ref node.NodeLayout;
                layout.LastParentDirection = d.ParentDirection;

                if (!d.CachedResultsValid)
                {
                    if (layout.NextCachedMeasurementsIndex == Constant.MaxCachedResultCount)
                    {
                        layout.NextCachedMeasurementsIndex = 0;
                    }

                    ref var newCacheEntry = ref layout.CachedLayout;
                    if (d.PerformLayout)
                    {
                        newCacheEntry = ref layout.CachedLayout;
                    }
                    else
                    {
                        newCacheEntry = ref layout.CachedMeasurements[layout.NextCachedMeasurementsIndex];
                        layout.NextCachedMeasurementsIndex++;
                    }

                    newCacheEntry.AvailableWidth = d.AvailableWidth;
                    newCacheEntry.AvailableHeight = d.AvailableHeight;
                    newCacheEntry.WidthMeasureMode = d.WidthMeasureMode;
                    newCacheEntry.HeightMeasureMode = d.HeightMeasureMode;
                    newCacheEntry.ComputedWidth = layout.MeasuredDimensions[(int)Dimension.Width];
                    newCacheEntry.ComputedHeight = layout.MeasuredDimensions[(int)Dimension.Height];
                }

                if (d.PerformLayout)
                {
                    node.NodeLayout.Dimensions[(int)Dimension.Width] = node.NodeLayout.MeasuredDimensions[
                        (int)Dimension.Width
                    ];
                    node.NodeLayout.Dimensions[(int)Dimension.Height] = node.NodeLayout.MeasuredDimensions[
                        (int)Dimension.Height
                    ];
                    node.IsDirty = false;
                }

                layout.GenerationCount = CurrentGenerationCount;
                d.Result = d.NeedToVisitNode || !d.CachedResultsValid;
                return false;
            }

            default:
                return false;
        }
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

        var stack = ArrayPool<RoundFrame<TStorage>>.Shared.Rent(node.DescendantsAndSelf().Count());
        try
        {
            var count = 0;
            stack[count++] = new RoundFrame<TStorage>(node, absoluteLeft, absoluteTop);

            while (count > 0)
            {
                var frame = stack[--count];
                var current = frame.Node;

                var nodeLeft = current.NodeLayout.Position[(int)Edge.Left];
                var nodeTop = current.NodeLayout.Position[(int)Edge.Top];

                var nodeWidth = current.NodeLayout.Dimensions[(int)Dimension.Width];
                var nodeHeight = current.NodeLayout.Dimensions[(int)Dimension.Height];

                var absoluteNodeLeft = frame.AbsoluteLeft + nodeLeft;
                var absoluteNodeTop = frame.AbsoluteTop + nodeTop;

                var absoluteNodeRight = absoluteNodeLeft + nodeWidth;
                var absoluteNodeBottom = absoluteNodeTop + nodeHeight;

                // If a node has a custom measure function we never want to round down its size as this could
                // lead to unwanted text truncation.
                var textRounding = current.NodeType == NodeType.Text;

                current.NodeLayout.Position[(int)Edge.Left] = RoundValueToPixelGrid(
                    nodeLeft,
                    pointScaleFactor,
                    false,
                    textRounding
                );
                current.NodeLayout.Position[(int)Edge.Top] = RoundValueToPixelGrid(
                    nodeTop,
                    pointScaleFactor,
                    false,
                    textRounding
                );

                // We multiply dimension by scale factor and if the result is close to the whole number, we don't have any fraction
                // To verify if the result is close to whole number we want to check both floor and ceil numbers
                var hasFractionalWidth =
                    !FloatsEqual(nodeWidth * pointScaleFactor % 1, 0)
                    && !FloatsEqual(nodeWidth * pointScaleFactor % 1, 1);
                var hasFractionalHeight =
                    !FloatsEqual(nodeHeight * pointScaleFactor % 1, 0)
                    && !FloatsEqual(nodeHeight * pointScaleFactor % 1, 1);

                current.NodeLayout.Dimensions[(int)Dimension.Width] =
                    RoundValueToPixelGrid(
                        absoluteNodeRight,
                        pointScaleFactor,
                        textRounding && hasFractionalWidth,
                        textRounding && !hasFractionalWidth
                    ) - RoundValueToPixelGrid(absoluteNodeLeft, pointScaleFactor, false, textRounding);
                current.NodeLayout.Dimensions[(int)Dimension.Height] =
                    RoundValueToPixelGrid(
                        absoluteNodeBottom,
                        pointScaleFactor,
                        textRounding && hasFractionalHeight,
                        textRounding && !hasFractionalHeight
                    ) - RoundValueToPixelGrid(absoluteNodeTop, pointScaleFactor, false, textRounding);

                foreach (var child in current)
                {
                    if (count == stack.Length)
                    {
                        var bigger = ArrayPool<RoundFrame<TStorage>>.Shared.Rent(stack.Length * 2);
                        Array.Copy(stack, bigger, count);
                        ArrayPool<RoundFrame<TStorage>>.Shared.Return(stack, true);
                        stack = bigger;
                    }

                    stack[count++] = new RoundFrame<TStorage>(child, absoluteNodeLeft, absoluteNodeTop);
                }
            }
        }
        finally
        {
            ArrayPool<RoundFrame<TStorage>>.Shared.Return(stack, true);
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

    private enum LayoutTaskKind : byte
    {
        Layout,
        FlexBasis,
        Absolute,
    }

    private struct FlexBasisData
    {
        internal float Width;
        internal float Height;
        internal float ParentWidth;
        internal float ParentHeight;
        internal MeasureMode WidthMode;
        internal MeasureMode HeightMode;
        internal Direction Direction;
    }

    private struct AbsoluteData
    {
        internal float Width;
        internal float Height;
        internal float Cw;
        internal float Ch;
        internal MeasureMode WidthMode;
        internal Direction Direction;
    }

    private struct LayoutFrameData
    {
        internal float AvailableWidth;
        internal float AvailableHeight;
        internal Direction ParentDirection;
        internal MeasureMode WidthMeasureMode;
        internal MeasureMode HeightMeasureMode;
        internal float ParentWidth;
        internal float ParentHeight;
        internal bool PerformLayout;

        internal bool CachedResultsValid;
        internal bool NeedToVisitNode;
        internal bool Result;

        internal Direction Direction;
        internal FlexDirection MainAxis;
        internal FlexDirection CrossAxis;
        internal bool IsMainAxisRow;
        internal Justify JustifyContent;
        internal bool IsNodeFlexWrap;
        internal float MainAxisParentSize;
        internal float CrossAxisParentSize;
        internal float LeadingPaddingAndBorderMain;
        internal float TrailingPaddingAndBorderMain;
        internal float LeadingPaddingAndBorderCross;
        internal float PaddingAndBorderAxisMain;
        internal float PaddingAndBorderAxisCross;
        internal MeasureMode MeasureModeMainDim;
        internal MeasureMode MeasureModeCrossDim;
        internal float PaddingAndBorderAxisRow;
        internal float PaddingAndBorderAxisColumn;
        internal float MarginAxisRow;
        internal float MarginAxisColumn;
        internal float MinInnerMainDim;
        internal float MaxInnerMainDim;
        internal float AvailableInnerWidth;
        internal float AvailableInnerHeight;
        internal float AvailableInnerMainDim;
        internal float AvailableInnerCrossDim;
        internal float MainAxisGap;
        internal float CrossAxisGap;
        internal float TotalOuterFlexBasis;
        internal bool FlexBasisOverflows;
        internal int ChildCount;

        internal int StartOfLineIndex;
        internal int EndOfLineIndex;
        internal int LineCount;
        internal float TotalLineCrossDim;
        internal float MaxLineMainDim;

        internal int ItemsOnLine;
        internal float SizeConsumedOnCurrentLine;
        internal float SizeConsumedOnCurrentLineIncludingMinConstraint;
        internal float TotalFlexGrowFactors;
        internal float TotalFlexShrinkScaledFactors;
        internal bool CanSkipFlex;
        internal float LeadingMainDim;
        internal float BetweenMainDim;
        internal float RemainingFreeSpace;
        internal float OriginalRemainingFreeSpace;
        internal float DeltaFreeSpace;
        internal int NumberOfAutoMarginsOnCurrentLine;
        internal float MainDim;
        internal float CrossDim;
        internal bool IsFirstInFlowChildOnLine;
        internal float ContainerCrossAxis;

        internal int Step3Index;
        internal bool Step3Resuming;
        internal bool Pass2Resuming;
        internal int Step7Index;
        internal bool Step7Resuming;
        internal float Step7LeadingCrossDim;
        internal int Step8I;
        internal int Step8Ii;
        internal bool Step8Resuming;
        internal int EndIndex;
        internal int StartIndex;
        internal float LineHeight;
        internal float MaxAscentForCurrentLine;
        internal float MaxDescentForCurrentLine;
        internal float CrossDimLead;
        internal float CurrentLead;
        internal bool Step10Resuming;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct FrameData
    {
        [FieldOffset(0)]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        internal LayoutFrameData Layout;

        [FieldOffset(0)]
        internal FlexBasisData FlexBasis;

        [FieldOffset(0)]
        internal AbsoluteData Absolute;
    }

    private struct Frame<TStorage>
        where TStorage : IList<Node<TStorage>>
    {
        internal Node<TStorage> Node;
        internal Node<TStorage> Child;
        internal Node<TStorage>? FirstAbsoluteChild;
        internal Node<TStorage>? CurrentAbsoluteChild;
        internal Node<TStorage>? SingleFlexChild;
        internal Node<TStorage>? FirstRelativeChild;
        internal Node<TStorage>? CurrentRelativeChild;
        internal LayoutTaskKind Kind;
        internal int Phase;
        internal FrameData Data;
    }

    private readonly struct RoundFrame<TStorage>(Node<TStorage> node, float absoluteLeft, float absoluteTop)
        where TStorage : IList<Node<TStorage>>
    {
        internal readonly Node<TStorage> Node = node;
        internal readonly float AbsoluteLeft = absoluteLeft;
        internal readonly float AbsoluteTop = absoluteTop;
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
        internal InlineArray6<float> Border;
        internal CachedMeasurement CachedLayout = new();
        internal CachedMeasurementArray CachedMeasurements;
        internal InlineArray2<float> Dimensions;
        internal InlineArray6<float> Margin;
        internal InlineArray2<float> MeasuredDimensions;
        internal InlineArray6<float> Padding;
        internal InlineArray4<float> Position;
        internal float ComputedFlexBasis = float.NaN;
        internal Direction Direction;
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
