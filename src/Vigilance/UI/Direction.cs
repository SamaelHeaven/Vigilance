using FlexLayoutSharp;

namespace Vigilance.UI;

public enum Direction
{
    TopToBottom = FlexDirection.Column,
    BottomToTop = FlexDirection.ColumnReverse,
    LeftToRight = FlexDirection.Row,
    RightToLeft = FlexDirection.RowReverse,
}
