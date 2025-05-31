using Raylib_cs;

namespace Vigilance.Core;

public static class Clipboard
{
    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            return Platform.Web.IsCurrent() ? "" : Raylib.GetClipboardText_();
        }
        set
        {
            Game.EnsureRunning();
            if (Platform.Web.IsCurrent())
            {
                JSEngine.Eval($"navigator.clipboard.writeText({value.ToJson()})");
                return;
            }

            Raylib.SetClipboardText(value);
        }
    }
}
