using Raylib_cs;

namespace Vigilance.Drawing;

public enum Interpolation
{
    None = TextureFilter.Point,
    Bilinear = TextureFilter.Bilinear,
    Trilinear = TextureFilter.Trilinear,
    Anisotropic4X = TextureFilter.Anisotropic4X,
    Anisotropic8X = TextureFilter.Anisotropic8X,
    Anisotropic16X = TextureFilter.Anisotropic16X,
}
