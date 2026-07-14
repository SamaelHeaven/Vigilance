using Vigilance.FlexLayout;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIContainer : UIParent
{
    public Direction Direction
    {
        get => (Direction)Node.StyleGetFlexDirection();
        set => Node.StyleSetFlexDirection((FlexDirection)value);
    }

    public Justify Justify
    {
        get => (Justify)Node.StyleGetJustifyContent();
        set => Node.StyleSetJustifyContent((FlexLayout.Justify)value);
    }

    public Align AlignItems
    {
        get => (Align)Node.StyleGetAlignItems();
        set => Node.StyleSetAlignItems((FlexLayout.Align)value);
    }

    public Align AlignContent
    {
        get => (Align)Node.StyleGetAlignContent();
        set => Node.StyleSetAlignContent((FlexLayout.Align)value);
    }

    public Wrap Wrap
    {
        get => (Wrap)Node.StyleGetFlexWrap();
        set => Node.StyleSetFlexWrap((FlexLayout.Wrap)value);
    }

    public Dimensions Gap
    {
        get => new(GapX, GapY);
        set
        {
            GapX = value.X;
            GapY = value.Y;
        }
    }

    public Unit GapX
    {
        get => Unit.FromValue(Node.StyleGetGap(Gutter.Column));
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Gutter.Column,
                (node, gutter, gap) => node.StyleSetGap(gutter, gap),
                (node, gutter, gap) => node.StyleSetGapPercent(gutter, gap)
            );
        }
    }

    public Unit GapY
    {
        get => Unit.FromValue(Node.StyleGetGap(Gutter.Row));
        set
        {
            Unit.SetUnit(
                Node,
                value,
                Gutter.Row,
                (node, gutter, gap) => node.StyleSetGap(gutter, gap),
                (node, gutter, gap) => node.StyleSetGapPercent(gutter, gap)
            );
        }
    }

    protected sealed override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return base.Measure(width, widthMode, height, heightMode);
    }
}
