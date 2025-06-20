namespace Vigilance.Drawing;

public struct GlyphInfo(int x, int y, int width, int height, int advance, int offsetX, int offsetY, int stroke)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public int Advance { get; set; } = advance;
    public int OffsetX { get; set; } = offsetX;
    public int OffsetY { get; set; } = offsetY;
    public int Stroke { get; set; } = stroke;
}
