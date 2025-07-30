namespace Vigilance.Core;

internal readonly record struct ZIndex(int Value)
{
    public ZIndex()
        : this(0) { }
}
