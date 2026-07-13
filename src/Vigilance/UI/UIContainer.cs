using System.Buffers;
using FlexLayoutSharp;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public class UIContainer : UIParent
{
    private Dimensions _gap;

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

    public Dimensions Gap
    {
        get => _gap;
        set
        {
            _gap = value;
            MarkDirty();
        }
    }

    public Unit GapX
    {
        get => _gap.X;
        set
        {
            _gap = new Dimensions(value, _gap.Y);
            MarkDirty();
        }
    }

    public Unit GapY
    {
        get => _gap.Y;
        set
        {
            _gap = new Dimensions(_gap.X, value);
            MarkDirty();
        }
    }

    internal bool HasGap => IsGapEnabled(_gap.X) || IsGapEnabled(_gap.Y);

    private static bool IsGapEnabled(Unit value)
    {
        return value.Type switch
        {
            UnitType.Fixed => value.Value > 0,
            UnitType.Percent => value.Value > 0,
            _ => false,
        };
    }

    internal bool ApplyGapMargins()
    {
        if (!HasGap)
            return false;
        var children = Children().Deferred(false);
        var flowIndices = ArrayPool<int>.Shared.Rent(children.Count);
        try
        {
            var flowCount = 0;
            for (var i = 0; i < children.Count; i++)
                if (children[i].Position != PositionType.Absolute && children[i].DisplayMode != DisplayMode.None)
                    flowIndices[flowCount++] = i;
            if (flowCount == 0)
                return false;
            var mainGap = Direction.IsHorizontal ? ResolveGap(GapX, LayoutWidth) : ResolveGap(GapY, LayoutHeight);
            var crossGap = Direction.IsHorizontal ? ResolveGap(GapY, LayoutHeight) : ResolveGap(GapX, LayoutWidth);
            if (mainGap <= 0 && (crossGap <= 0 || Wrap == Wrap.NoWrap))
                return false;
            var changed = false;
            var lineStart = 0;
            for (var i = 0; i < flowCount; i++)
            {
                var isLast = i == flowCount - 1;
                if (!isLast && !IsLineBreak(children[flowIndices[i]], children[flowIndices[i + 1]]))
                    continue;
                changed |= ApplyLineMargins(children, flowIndices, lineStart, i, lineStart != 0, mainGap, crossGap);
                lineStart = i + 1;
            }

            return changed;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(flowIndices);
        }
    }

    private bool ApplyLineMargins(
        in ChildEnumerable children,
        int[] flowIndices,
        int start,
        int end,
        bool addCrossGap,
        float mainGap,
        float crossGap
    )
    {
        var changed = false;
        for (var i = start; i <= end; i++)
        {
            var child = children[flowIndices[i]];
            var margin = child.DeclaredMargin;
            if (i > start && mainGap > 0)
                margin = AddMainGap(margin, mainGap);
            if (addCrossGap && crossGap > 0 && Wrap != Wrap.NoWrap)
                margin = AddCrossGap(margin, crossGap);
            changed |= child.ApplyComputedMargin(margin);
        }

        return changed;
    }

    private bool IsLineBreak(UIElement previous, UIElement current)
    {
        if (Wrap == Wrap.NoWrap)
            return false;
        if (previous.Position == PositionType.Absolute || current.Position == PositionType.Absolute)
            return false;
        var previousMain = Direction.IsHorizontal ? previous.LayoutLeft : previous.LayoutTop;
        var currentMain = Direction.IsHorizontal ? current.LayoutLeft : current.LayoutTop;
        const float epsilon = 0.5f;
        return Direction switch
        {
            Direction.LeftToRight or Direction.TopToBottom => currentMain < previousMain - epsilon,
            Direction.RightToLeft or Direction.BottomToTop => currentMain > previousMain + epsilon,
            _ => false,
        };
    }

    private Insets AddMainGap(in Insets margin, float gap)
    {
        return Direction switch
        {
            Direction.LeftToRight => margin with { Left = AddGap(margin.Left, gap, LayoutWidth) },
            Direction.RightToLeft => margin with { Right = AddGap(margin.Right, gap, LayoutWidth) },
            Direction.TopToBottom => margin with { Top = AddGap(margin.Top, gap, LayoutHeight) },
            Direction.BottomToTop => margin with { Bottom = AddGap(margin.Bottom, gap, LayoutHeight) },
            _ => margin,
        };
    }

    private Insets AddCrossGap(in Insets margin, float gap)
    {
        return Direction.IsHorizontal
            ? Wrap == Wrap.WrapReverse
                ? margin with
                {
                    Bottom = AddGap(margin.Bottom, gap, LayoutHeight),
                }
                : margin with
                {
                    Top = AddGap(margin.Top, gap, LayoutHeight),
                }
            : Wrap == Wrap.WrapReverse
                ? margin with
                {
                    Right = AddGap(margin.Right, gap, LayoutWidth),
                }
                : margin with
                {
                    Left = AddGap(margin.Left, gap, LayoutWidth),
                };
    }

    private static Unit AddGap(Unit margin, float gap, float referenceSize)
    {
        return margin.Type switch
        {
            UnitType.Fixed => margin with { Value = margin.Value + gap },
            UnitType.Percent when referenceSize > 0 => margin with
            {
                Value = margin.Value + gap / referenceSize * 100f,
            },
            UnitType.Undefined => Unit.Fixed(gap),
            _ => margin,
        };
    }

    private static float ResolveGap(Unit gap, float referenceSize)
    {
        return gap.Type switch
        {
            UnitType.Fixed => gap.Value.Max(0),
            UnitType.Percent => gap.Calculate(referenceSize).Max(0),
            _ => 0,
        };
    }

    protected sealed override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return base.Measure(width, widthMode, height, heightMode);
    }
}
