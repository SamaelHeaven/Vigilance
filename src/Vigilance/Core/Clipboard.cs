using Raylib_cs;

namespace Vigilance.Core;

public static class Clipboard
{
    private static bool _needed = false;

    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            _needed = true;
            return Platform.Web.IsCurrent()
                ? JSEngine.Run("Module.Engine.clipboardText ?? ''")
                : Raylib.GetClipboardText_();
        }
        set
        {
            Game.EnsureRunning();
            Raylib.SetClipboardText(value);
        }
    }

    internal static void Update()
    {
        if (!Platform.Web.IsCurrent())
            return;
        if (bool.Parse(JSEngine.Run("!!Module.Engine.clipboardError")))
            _needed = false;
        if (!_needed)
            return;
        JSEngine.Run(
            """
                navigator.clipboard.readText().then(text => {
                    Module.Engine.clipboardText = text;
                    Module.Engine.clipboardError = false;
                }).catch(() => {
                    Module.Engine.clipboardText = '';
                    Module.Engine.clipboardError = true;
                })
            """
        );
    }
}
