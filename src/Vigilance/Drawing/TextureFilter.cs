namespace Vigilance.Drawing;

public enum TextureFilter : sbyte
{
    Nearest = Raylib_cs.TextureFilter.Point,
    Bilinear = Raylib_cs.TextureFilter.Bilinear,
}
