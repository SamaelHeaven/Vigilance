namespace Vigilance.Core;

public sealed class DisplayConfig
{
    public string Title { get; set; } = "";
    public Image? Icon { get; set; } = null;
    public Vector2 Size { get; set; } = new(800, 600);
    public Vector2 ScreenSize { get; set; } = Vector2.Zero;
    public Vector2? MinScreenSize { get; set; } = null;
    public Vector2? MaxScreenSize { get; set; } = null;
    public Vector2? Position { get; set; } = null;
    public Viewport Viewport { get; set; } = Viewport.Fit;
    public RenderingMode RenderingMode { get; set; } = RenderingMode.Native();
    public Color Background { get; set; } = Color.Black;
    public int FpsTarget { get; set; } = 0;
    public bool Fullscreen { get; set; } = false;
    public bool DefaultFullscreenBorderless { get; set; } = false;
    public bool Maximized { get; set; } = false;
    public bool Decorated { get; set; } = true;
    public bool Focused { get; set; } = true;
    public bool Vsync { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public bool RunMinimized { get; set; } = true;
    public bool Msaa4X { get; set; } = false;
    public bool Hidden { get; set; } = false;
    public bool TopMost { get; set; } = false;
    public bool Transparent { get; set; } = false;
    public bool Passthrough { get; set; } = false;
}

public static class DisplayConfigExtensions
{
    public static ConfigBuilder Display(this ConfigBuilder configs, Action<DisplayConfig> config)
    {
        return configs.Add(config);
    }
}
