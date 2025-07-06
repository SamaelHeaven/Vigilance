using FlexLayoutSharp;

namespace Vigilance.UI;

public enum Direction
{
    TopToBottom = FlexDirection.Column,
    BottomToTop = FlexDirection.ColumnReverse,
    LeftToRight = FlexDirection.Row,
    RightToLeft = FlexDirection.RowReverse,
}

public static class DirectionExtensions
{
    public static bool IsHorizontal(this Direction direction)
    {
        return direction is Direction.LeftToRight or Direction.RightToLeft;
    }

    public static bool IsVertical(this Direction direction)
    {
        return direction is Direction.TopToBottom or Direction.BottomToTop;
    }
}
