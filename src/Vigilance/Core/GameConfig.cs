using Vigilance.Drawing;
using Vigilance.Input;

namespace Vigilance.Core;

public struct GameConfig
{
    public string Title = "";
    public string Icon = "";
    public string WorkingDirectory = "";
    public string WorkingNamespace = "";
    public Key? ExitKey = null;
    public Key? FullscreenKey = null;
    public int Width = 800;
    public int Height = 600;
    public int ScreenWidth = 0;
    public int ScreenHeight = 0;
    public int FpsTarget = 60;
    public Interpolation DefaultInterpolation = Interpolation.None;
    public bool Fullscreen = false;
    public bool Decorated = true;
    public bool Vsync = true;
    public bool Resizable = true;
    public bool Debug = false;

    public GameConfig() { }
}
