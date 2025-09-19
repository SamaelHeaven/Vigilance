namespace Vigilance.Drawing;

internal readonly record struct Glyph(
    byte[] Bitmap,
    char Character,
    int Width,
    int Height,
    int Advance,
    int BearerX,
    int BearerY,
    int Stroke
);
