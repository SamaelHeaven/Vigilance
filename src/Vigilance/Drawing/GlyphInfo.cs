namespace Vigilance.Drawing;

internal readonly struct GlyphInfo(int x, int y, int width, int height, int advance, int offsetX, int offsetY)
{
    public readonly int X = x;
    public readonly int Y = y;
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly int Advance = advance;
    public readonly int OffsetX = offsetX;
    public readonly int OffsetY = offsetY;
}
