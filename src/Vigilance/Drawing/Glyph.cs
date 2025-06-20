namespace Vigilance.Drawing;

internal readonly struct Glyph(
    byte[] bitmap,
    char character,
    int width,
    int height,
    int advance,
    int bearerX,
    int bearerY,
    int stroke
)
{
    public byte[] Bitmap { get; } = bitmap;
    public char Character { get; } = character;
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int Advance { get; } = advance;
    public int BearerX { get; } = bearerX;
    public int BearerY { get; } = bearerY;
    public int Stroke { get; } = stroke;
}
