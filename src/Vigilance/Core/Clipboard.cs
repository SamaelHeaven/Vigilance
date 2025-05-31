using Raylib_cs;
using Vigilance.Logging;

namespace Vigilance.Core;

public static class Clipboard
{
    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            if (!Platform.Web.IsCurrent())
                return Raylib.GetClipboardText_();
            Game.Log(LogLevel.Warn, "GetClipboardText() not implemented on target platform");
            return "";
        }
        set
        {
            Game.EnsureRunning();
            if (Platform.Web.IsCurrent())
            {
                JSEngine.Run($"navigator.clipboard.writeText({value.ToJson()})");
                return;
            }

            Raylib.SetClipboardText(value);
        }
    }
}
