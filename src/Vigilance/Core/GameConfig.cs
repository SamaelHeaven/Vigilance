using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Core;

public sealed record GameConfig : IShallowCloneable
{
    public string Title { get; set; } = "";
    public string WorkingDirectory { get; set; } = "";
    public string WorkingNamespace { get; set; } = "";
    public Func<Image>? Icon { get; set; } = null;
    public Key ExitKey { get; set; } = Key.Null;
    public Key FullscreenKey { get; set; } = Key.Null;
    public Vector2 Size { get; set; } = new(800, 600);
    public Vector2 ScreenSize { get; set; } = Vector2.Zero;
    public Vector2? MinSize { get; set; } = null;
    public Vector2? MaxSize { get; set; } = null;
    public Viewport Viewport { get; set; } = Viewport.Fit;
    public RenderingMode RenderingMode { get; set; } = RenderingMode.Screen;
    public Color Background { get; set; } = Color.Black;
    public int FpsTarget { get; set; } = 0;
    public bool Fullscreen { get; set; } = false;
    public bool Maximized { get; set; } = false;
    public bool Decorated { get; set; } = true;
    public bool Vsync { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public bool Debug { get; set; } = false;
    public bool RunMinimized { get; set; } = true;
    public bool Msaa4X { get; set; } = false;
    public float MasterVolume { get; set; } = 1;
    public CacheType DefaultAssetCacheType { get; set; } = CacheType.Weak;
    public ILogger? Logger { get; set; } = new ConsoleLogger();
    public LogLevel LogLevel { get; set; } = LogLevel.All;
    public int DefaultSoundMaxAliases { get; set; } = 16;
    public int DefaultFontQuality { get; set; } = 128;
    public float DefaultFontSize { get; set; } = 16;
    public TextHeightMode DefaultTextHeightMode { get; set; } = TextHeightMode.Character;
    public Vector2 DefaultTextSpacing { get; set; } = new(0, 4);
    public GameSystemsFunc Systems { get; set; } = Array.Empty<IGameSystem>;
    public Action? QuitAction { get; set; } = null;

    public InputAxis HorizontalInputAxis { get; set; } =
        new()
        {
            NegativeKeys = [Key.Left, Key.A],
            PositiveKeys = [Key.Right, Key.D],
            GamepadAxes = [GamepadAxis.LeftX],
        };

    public InputAxis VerticalInputAxis { get; set; } =
        new()
        {
            NegativeKeys = [Key.Up, Key.W],
            PositiveKeys = [Key.Down, Key.S],
            GamepadAxes = [GamepadAxis.LeftY],
        };

    public Func<Font> DefaultFont { get; set; } =
        () =>
        {
            var assembly = Assemblies.Engine;
            return Asset.FontResource(
                "Font.Default.ttf",
                @namespace: $"{assembly.GetName().Name}.Resources",
                assembly: assembly
            );
        };

    public string DefaultFontCharset { get; set; } =
        "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
}
