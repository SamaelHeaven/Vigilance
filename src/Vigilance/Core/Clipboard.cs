using Raylib_cs;

namespace Vigilance.Core;

public static class Clipboard
{
    private static bool _refresh = false;

    public static string Text
    {
        get
        {
            Game.EnsureRunning();
            _refresh = true;
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
        JSEngine.Run(
            """
                navigator.permissions.query({ name: 'clipboard-read' }).then(result => {
                    Module.Engine.clipboardRead = result.state == 'granted';
                })
            """
        );
        var clipboardRead = bool.Parse(JSEngine.Run("Module.Engine.clipboardRead"));
        if (!_refresh && !clipboardRead)
            return;
        _refresh = false;
        JSEngine.Run(
            """
                navigator.clipboard.readText().then(text => {
                    Module.Engine.clipboardText = text;
                }).catch(() => {
                    Module.Engine.clipboardText = '';
                })
            """
        );
    }
}
