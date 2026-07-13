namespace Vigilance.FlexLayout;

public enum Align : sbyte
{
    Auto,
    Start,
    Center,
    End,
    Stretch,
    Baseline,
    SpaceBetween,
    SpaceAround,
}

public enum Dimension : sbyte
{
    Width,
    Height,
}

public enum Direction : sbyte
{
    Inherit,
    LeftToRight,
    RightToLeft,
}

public enum Display : sbyte
{
    Flex,
    None,
}

public enum Edge : sbyte
{
    Left,
    Top,
    Right,
    Bottom,
    Start,
    End,
    Horizontal,
    Vertical,
    All,
}

public enum FlexDirection : sbyte
{
    Column,
    ColumnReverse,
    Row,
    RowReverse,
}

public enum Justify : sbyte
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
}

public enum MeasureMode : sbyte
{
    Undefined,
    Exactly,
    AtMost,
}

public enum NodeType : sbyte
{
    Default,
    Text,
}

public enum Overflow : sbyte
{
    Visible,
    Hidden,
    Scroll,
}

public enum PositionType : sbyte
{
    Relative,
    Absolute,
}

public enum Unit : sbyte
{
    Undefined,
    Point,
    Percent,
    Auto,
}

public enum Wrap : sbyte
{
    NoWrap,
    Wrap,
    WrapReverse,
}
