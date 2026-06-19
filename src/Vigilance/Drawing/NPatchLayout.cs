namespace Vigilance.Drawing;

public enum NPatchLayout : byte
{
    Full = Raylib_cs.NPatchLayout.NinePatch,
    Horizontal = Raylib_cs.NPatchLayout.ThreePatchHorizontal,
    Vertical = Raylib_cs.NPatchLayout.ThreePatchVertical,
}
