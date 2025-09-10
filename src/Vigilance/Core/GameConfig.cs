using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed class GameConfig
{
    public string Title { get; set; } = "";
    public Func<Image>? Icon { get; set; } = null;
    public Key ExitKey { get; set; } = Key.Null;
    public Key FullscreenKey { get; set; } = Key.Null;
    public Vector2 Size { get; set; } = new(800, 600);
    public Vector2 ScreenSize { get; set; } = Vector2.Zero;
    public Vector2? MinScreenSize { get; set; } = null;
    public Vector2? MaxScreenSize { get; set; } = null;
    public Viewport Viewport { get; set; } = Viewport.Fit;
    public RenderingMode RenderingMode { get; set; } = RenderingMode.Screen;
    public Color Background { get; set; } = Color.Black;
    public int FpsTarget { get; set; } = 0;
    public bool Fullscreen { get; set; } = false;
    public bool Maximized { get; set; } = false;
    public bool Decorated { get; set; } = true;
    public bool Vsync { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public bool RunMinimized { get; set; } = true;
    public bool Msaa4X { get; set; } = false;
    public Action? QuitAction { get; set; } = null;
    public GameSystemsFunc Systems { get; set; } = Array.Empty<IGameSystem>;
}

public static class GameConfigExtensions
{
    public static ConfigsBuilder Game(this ConfigsBuilder configs, GameConfig config)
    {
        return configs.AddConfig(config);
    }
}
