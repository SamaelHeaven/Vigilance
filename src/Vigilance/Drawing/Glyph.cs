namespace Vigilance.Drawing;

internal readonly struct Glyph(
    byte[] bitmap,
    char character,
    int width,
    int height,
    int advance,
    int bearerX,
    int bearerY
)
{
    public readonly byte[] Bitmap = bitmap;
    public readonly char Character = character;
    public readonly int Width = width;
    public readonly int Height = height;
    public readonly int Advance = advance;
    public readonly int BearerX = bearerX;
    public readonly int BearerY = bearerY;
}
