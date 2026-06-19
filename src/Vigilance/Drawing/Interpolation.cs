using Raylib_cs;

namespace Vigilance.Drawing;

public enum Interpolation : byte
{
    Nearest = TextureFilter.Point,
    Bilinear = TextureFilter.Bilinear,
}
