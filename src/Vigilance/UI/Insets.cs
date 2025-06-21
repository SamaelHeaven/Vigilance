namespace Vigilance.UI;

public struct Insets
{
    public Unit Top { get; set; }
    public Unit Right { get; set; }
    public Unit Bottom { get; set; }
    public Unit Left { get; set; }

    public static implicit operator Insets(float value)
    {
        return (Unit)value;
    }

    public static implicit operator Insets(Unit value)
    {
        return new Insets
        {
            Top = value,
            Right = value,
            Bottom = value,
            Left = value,
        };
    }
}
