using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Input;

public static class Clipboard
{
    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            return Game.Platform switch
            {
                Platform.Web => "",
                _ => Raylib.GetClipboardText_(),
            };
        }
        set
        {
            Game.EnsureRunning();
            if (Platform.Web.IsCurrent())
                JSEngine.Eval($"navigator.clipboard.writeText({value.ToJson()})");
            else
                Raylib.SetClipboardText(value);
        }
    }
}
