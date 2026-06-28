namespace Vigilance.Drawing;

public enum TextureFilter : byte
{
    Nearest = Raylib_cs.TextureFilter.Point,
    Bilinear = Raylib_cs.TextureFilter.Bilinear,
}
