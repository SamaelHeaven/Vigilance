using Raylib_cs;

namespace Vigilance.Input;

public static class Clipboard
{
    public static string Text
    {
        get
        {
            Game.ThrowIfNotRunning();
            return Platform.Current switch
            {
                Platform.Web => "",
                _ => Raylib.GetClipboardText_(),
            };
        }
        set
        {
            Game.ThrowIfNotRunning();
            if (Platform.Web.IsCurrent)
                JSEngine.Run($"void navigator.clipboard.writeText({value.ToJson()})");
            else
                Raylib.SetClipboardText(value);
        }
    }
}
