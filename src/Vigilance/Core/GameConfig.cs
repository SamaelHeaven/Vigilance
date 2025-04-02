using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;

namespace Vigilance.Core;

public struct GameConfig
{
    public string Title = "";
    public string WorkingDirectory = "";
    public string WorkingModule = "";
    public Func<Image>? Icon = null;
    public Key? ExitKey = null;
    public Key? FullscreenKey = null;
    public int Width = 800;
    public int Height = 600;
    public int ScreenWidth = 0;
    public int ScreenHeight = 0;
    public int FpsTarget = 60;
    public bool Fullscreen = false;
    public bool Maximized = false;
    public bool Decorated = true;
    public bool Vsync = true;
    public bool Resizable = true;
    public bool Debug = false;
    public Interpolation DefaultInterpolation = Interpolation.None;
    public Vector2 DefaultTextSpacing = new(0, 4);
    public int DefaultFontQuality = 128;
    public float DefaultFontSize = 24;

    public Func<Font> DefaultFont = static () =>
        Asset.FontResource(
            "DefaultFont.ttf",
            module: FileSystem.EngineAssembly.GetName().Name! + ".Resources",
            assembly: FileSystem.EngineAssembly
        );

    public string DefaultFontCharset =
        "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    public GameConfig() { }
}
