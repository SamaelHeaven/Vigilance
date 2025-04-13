using Raylib_cs;

namespace Vigilance.Drawing;

public enum Interpolation
{
    Nearest = TextureFilter.Point,
    Bilinear = TextureFilter.Bilinear,
}
