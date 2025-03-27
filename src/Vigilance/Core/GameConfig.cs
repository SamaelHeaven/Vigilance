using Vigilance.Drawing;
using Vigilance.Input;

namespace Vigilance.Core;

public struct GameConfig
{
    public string Title = "";
    public string Icon = "";
    public string WorkingDirectory = "";
    public string WorkingModule = "";
    public Key? ExitKey = null;
    public Key? FullscreenKey = null;
    public int Width = 800;
    public int Height = 600;
    public int ScreenWidth = 0;
    public int ScreenHeight = 0;
    public int FpsTarget = 60;
    public bool Fullscreen = false;
    public bool Decorated = true;
    public bool Vsync = true;
    public bool Resizable = true;
    public bool Debug = false;
    public Interpolation DefaultInterpolation = Interpolation.None;
    public int DefaultFontQuality = 128;
    public float DefaultFontSize = 32;

    public Func<Font> DefaultFont = static () =>
        Asset.FontResource("DefaultFont.ttf", module: "Vigilance.Resources", assembly: FileSystem.VigilanceAssembly);

    public string DefaultFontCharset =
        "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    public GameConfig() { }
}
