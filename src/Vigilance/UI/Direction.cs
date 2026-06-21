using FlexLayoutSharp;

namespace Vigilance.UI;

public enum Direction : byte
{
    TopToBottom = FlexDirection.Column,
    BottomToTop = FlexDirection.ColumnReverse,
    LeftToRight = FlexDirection.Row,
    RightToLeft = FlexDirection.RowReverse,
}

public static class DirectionExtensions
{
    extension(Direction direction)
    {
        public bool IsHorizontal => direction is Direction.LeftToRight or Direction.RightToLeft;

        public bool IsVertical => direction is Direction.TopToBottom or Direction.BottomToTop;
    }
}
