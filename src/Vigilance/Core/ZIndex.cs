namespace Vigilance.Core;

internal readonly struct ZIndex(int value = 0)
{
    public int Value { get; } = value;
}
