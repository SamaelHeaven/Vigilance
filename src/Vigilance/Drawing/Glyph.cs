using System.Text;

namespace Vigilance.Drawing;

internal readonly record struct Glyph(
    byte[] Bitmap,
    Rune Rune,
    int Width,
    int Height,
    int Advance,
    int BearerX,
    int BearerY,
    int Stroke
);
