using Raylib_cs;

namespace Vigilance.Core;

public static class Clipboard
{
    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.GetClipboardText_();
        }
        set
        {
            Game.EnsureRunning();
            Raylib.SetClipboardText(value);
        }
    }
}
