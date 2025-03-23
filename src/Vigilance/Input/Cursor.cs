using Raylib_cs;

namespace Vigilance.Input;

public enum Cursor
{
    Default = MouseCursor.Default,
    Arrow = MouseCursor.Arrow,
    Text = MouseCursor.IBeam,
    Crosshair = MouseCursor.Crosshair,
    Pointer = MouseCursor.PointingHand,
    ResizeEw = MouseCursor.ResizeEw,
    ResizeNs = MouseCursor.ResizeNs,
    ResizeNwse = MouseCursor.ResizeNwse,
    ResizeNesw = MouseCursor.ResizeNesw,
    ResizeAll = MouseCursor.ResizeAll,
    NotAllowed = MouseCursor.NotAllowed,
    None,
}
