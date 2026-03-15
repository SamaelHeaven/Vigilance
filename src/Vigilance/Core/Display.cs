using Raylib_cs.BleedingEdge;
using Vigilance.Drawing;
using Vigilance.Math;
using Color = Vigilance.Drawing.Color;
using Image = Vigilance.Drawing.Image;
using PixelFormat = Raylib_cs.BleedingEdge.PixelFormat;

namespace Vigilance.Core;

public static unsafe class Display
{
    private static DisplayConfig _config = null!;
    private static Image? _icon;
    private static Box _previousScreen;
    private static bool _resetScreen;
    private static bool _fullscreen;

    static Display()
    {
        Game.ThrowIfNotRunning();
    }

    public static string Title
    {
        get => _config.Title;
        set
        {
            if (value == Title)
                return;
            _config.Title = value;
            Raylib.SetWindowTitle(value);
        }
    }

    public static Image? Icon
    {
        get => _icon;
        set
        {
            if (_icon == value)
                return;
            _icon = value?.Copy<PixelR8G8B8A8>();
            if (!Platform.Desktop.IsCurrent || OperatingSystem.IsMacOS())
                return;
            if (_icon is null)
            {
                Raylib.SetWindowIcons(ReadOnlySpan<Raylib_cs.BleedingEdge.Image>.Empty);
                return;
            }

            Raylib.SetWindowIcon(_icon.RImage);
        }
    }

    public static Vector2 Size
    {
        get => Viewport == Viewport.Native ? ScreenSize : _config.Size;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenSize = value;
                return;
            }

            _config.Size = value;
        }
    }

    public static float Width
    {
        get => Viewport == Viewport.Native ? ScreenWidth : _config.Size.X;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenWidth = (int)value;
                return;
            }

            _config.Size = new Vector2(value, _config.Size.Y);
        }
    }

    public static float Height
    {
        get => Viewport == Viewport.Native ? ScreenHeight : _config.Size.Y;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenHeight = (int)value;
                return;
            }

            _config.Size = new Vector2(_config.Size.X, value);
        }
    }

    public static int RefreshRate { get; private set; }

    public static int MonitorWidth { get; private set; }

    public static int MonitorHeight { get; private set; }

    public static Vector2 MonitorSize => new(MonitorWidth, MonitorHeight);

    public static Vector2 ScreenSize
    {
        get => new(ScreenWidth, ScreenHeight);
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            var size = value.Round();
            if (ScreenSize == size)
                return;
            _config.ScreenSize = size;
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
        }
    }

    public static int ScreenWidth
    {
        get => (int)_config.ScreenSize.X;
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            if (ScreenWidth == value)
                return;
            _config.ScreenSize = new Vector2(value, ScreenHeight);
            Raylib.SetWindowSize(value, ScreenHeight);
        }
    }

    public static int ScreenHeight
    {
        get => (int)_config.ScreenSize.Y;
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            if (ScreenHeight == value)
                return;
            _config.ScreenSize = new Vector2(ScreenWidth, value);
            Raylib.SetWindowSize(ScreenWidth, value);
        }
    }

    public static Vector2? MinScreenSize
    {
        get => _config.MinScreenSize;
        set
        {
            value = value?.Round();
            if (value == _config.MinScreenSize)
                return;
            _config.MinScreenSize = value;
            if (Platform.Desktop.IsCurrent)
                Raylib.SetWindowMinSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    }

    public static Vector2? MaxScreenSize
    {
        get => _config.MaxScreenSize;
        set
        {
            value = value?.Round();
            if (value == _config.MaxScreenSize)
                return;
            _config.MaxScreenSize = value;
            if (Platform.Desktop.IsCurrent)
                Raylib.SetWindowMaxSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    }

    public static Viewport Viewport
    {
        get => _config.Viewport;
        set { Game.Defer(() => _config.Viewport = value); }
    }

    public static RenderingMode RenderingMode
    {
        get => _config.RenderingMode;
        set { Game.Defer(() => _config.RenderingMode = value); }
    }

    public static Color Background
    {
        get => _config.Background;
        set => _config.Background = value;
    }

    public static int FpsTarget
    {
        get => _config.FpsTarget;
        set
        {
            if (value < 1)
                value = 0;
            if (value == FpsTarget)
                return;
            _config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
        }
    }

    public static bool Fullscreen
    {
        get => _fullscreen;
        set
        {
            if (value != Fullscreen)
                ToggleFullscreen();
        }
    }

    public static bool DefaultFullscreenResize
    {
        get => _config.DefaultFullscreenResize;
        set => _config.DefaultFullscreenResize = value;
    }

    public static bool DefaultFullscreenBorderless
    {
        get => _config.DefaultFullscreenBorderless;
        set => _config.DefaultFullscreenBorderless = value;
    }

    public static bool Hidden { get; private set; }

    public static bool Maximized { get; private set; }

    public static bool Minimized { get; private set; }

    public static bool Decorated
    {
        get => _config.Decorated;
        set
        {
            if (value == _config.Decorated)
                return;
            _config.Decorated = value;
            ToggleWindowState(ConfigFlags.WindowUndecorated, !value);
        }
    }

    public static bool Vsync
    {
        get => _config.Vsync;
        set
        {
            if (value == _config.Vsync)
                return;
            _config.Vsync = value;
            ToggleWindowState(ConfigFlags.VSyncHint, value);
        }
    }

    public static bool Resizable
    {
        get => _config.Resizable;
        set
        {
            if (value == _config.Resizable)
                return;
            _config.Resizable = value;
            ToggleWindowState(ConfigFlags.WindowResizable, value);
        }
    }

    public static bool TopMost
    {
        get => _config.TopMost;
        set
        {
            if (value == _config.TopMost)
                return;
            _config.TopMost = value;
            ToggleWindowState(ConfigFlags.WindowTopMost, value);
        }
    }

    public static bool Transparent
    {
        get => _config.Transparent;
        set
        {
            if (value == _config.Transparent)
                return;
            _config.Transparent = value;
            ToggleWindowState(ConfigFlags.WindowTransparent, value);
        }
    }

    public static bool Passthrough
    {
        get => _config.Passthrough;
        set
        {
            if (value == _config.Passthrough)
                return;
            _config.Passthrough = value;
            ToggleWindowState(ConfigFlags.WindowMousePassthrough, value);
        }
    }

    public static bool RunMinimized => _config.RunMinimized;

    public static bool Msaa4X => _config.Msaa4X;

    public static bool Focused { get; private set; }

    public static void Maximize()
    {
        if (Maximized || !Platform.Desktop.IsCurrent)
            return;
        Raylib.MaximizeWindow();
        Minimized = Raylib.IsWindowMaximized();
        Maximized = Raylib.IsWindowMinimized();
    }

    public static void Minimize()
    {
        if (Minimized || !Platform.Desktop.IsCurrent)
            return;
        Raylib.MinimizeWindow();
        Minimized = Raylib.IsWindowMaximized();
        Maximized = Raylib.IsWindowMinimized();
    }

    public static void Restore()
    {
        if (Platform.Desktop.IsCurrent && (Maximized || Minimized))
            Raylib.RestoreWindow();
    }

    public static void Focus()
    {
        if (Focused)
            return;
        if (Platform.Web.IsCurrent)
            JSEngine.Eval("Module.canvas.focus()");
        else
            Raylib.SetWindowFocused();
        Focused = Raylib.IsWindowFocused();
    }

    public static void ToggleFullscreen(bool? resize = null, bool? borderless = null)
    {
        var resizeValue = resize ?? DefaultFullscreenResize;
        var borderlessValue = borderless ?? DefaultFullscreenBorderless;
        if (Platform.Web.IsCurrent)
        {
            JSEngine.Eval(Fullscreen ? "document.exitFullscreen()" : "Module.canvas.requestFullscreen()");
            _fullscreen = JSEngine.Eval("!!document.fullscreenElement");
        }
        else if (Platform.Desktop.IsCurrent)
        {
            var monitorSize = MonitorSize;
            var fullscreen = _fullscreen;
            if (fullscreen)
            {
                _resetScreen = resizeValue;
            }
            else
            {
                _previousScreen = new Box(Raylib.GetWindowPosition(), ScreenSize);
                if (OperatingSystem.IsMacOS() && !Raylib.IsWindowMaximized())
                    Raylib.MaximizeWindow();
                if (resizeValue && !(borderlessValue && OperatingSystem.IsWindows()))
                    ScreenSize = monitorSize;
            }

            var screenSize = ScreenSize;
            if (
                OperatingSystem.IsMacOS()
                && !fullscreen
                && screenSize == monitorSize
                && (Vector2)Raylib.GetWindowPosition() == Vector2.Zero
            )
                return;
            _fullscreen = !fullscreen;
            if (borderlessValue && OperatingSystem.IsWindows())
            {
                Raylib.ToggleBorderlessWindowed();
                if (_fullscreen && !Decorated)
                    Raylib.SetWindowState(ConfigFlags.WindowUndecorated);
            }
            else
            {
                Raylib.ToggleFullscreen();
            }
        }
    }

    public static WritableImage<PixelR8G8B8A8> Screenshot()
    {
        var width = ScreenWidth;
        var height = ScreenHeight;
        Graphics.Reset();
        Graphics.DrawCurrentBuffer();
        var data = Rlgl.ReadScreenPixels(width, height);
        var image = new Raylib_cs.BleedingEdge.Image
        {
            Data = data,
            Width = width,
            Height = height,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8,
        };
        return new WritableImage<PixelR8G8B8A8>(new WritableImage(new Image(image)));
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<DisplayConfig>() ?? new DisplayConfig();
        InitializeWindow();
    }

    internal static void Update()
    {
        if (Platform.Web.IsCurrent)
        {
            _fullscreen = JSEngine.Eval("!!document.fullscreenElement");
            RefreshRate = 0;
            MonitorWidth = Raylib.GetScreenWidth();
            MonitorHeight = Raylib.GetScreenHeight();
            Focused = JSEngine.Eval("document.activeElement === Module.canvas");
        }
        else
        {
            var monitor = Raylib.GetCurrentMonitor();
            RefreshRate = Raylib.GetMonitorRefreshRate(monitor);
            MonitorWidth = Raylib.GetMonitorWidth(monitor);
            MonitorHeight = Raylib.GetMonitorHeight(monitor);
            Focused = Raylib.IsWindowFocused();
        }

        Hidden = Raylib.IsWindowHidden();
        Maximized = Raylib.IsWindowMaximized();
        Minimized = Raylib.IsWindowMinimized();
        _config.ScreenSize =
            Platform.Desktop.IsCurrent && Fullscreen
                ? new Vector2(MonitorWidth, MonitorHeight)
                : new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        if (!_resetScreen)
            return;
        _resetScreen = false;
        if (OperatingSystem.IsMacOS())
            Raylib.SetWindowPosition(1, 1);
        Raylib.SetWindowPosition((int)_previousScreen.Position.X, (int)_previousScreen.Position.Y);
        ScreenSize = _previousScreen.Size;
    }

    internal static void Dispose()
    {
        Raylib.CloseWindow();
    }

    private static void ToggleWindowState(ConfigFlags flag, bool value)
    {
        if (!Platform.Desktop.IsCurrent)
            return;
        if (value)
        {
            Raylib.SetWindowState(flag);
            return;
        }

        Raylib.ClearWindowState(flag);
    }

    private static void InitializeWindow()
    {
        Raylib.SetConfigFlags(GetConfigFlags());
        var width = (int)(
            _config.ScreenSize.X <= 0 || !Platform.Desktop.IsCurrent ? _config.Size.X : _config.ScreenSize.X
        );
        var height = (int)(
            _config.ScreenSize.Y <= 0 || !Platform.Desktop.IsCurrent ? _config.Size.Y : _config.ScreenSize.Y
        );
        if (OperatingSystem.IsMacOS())
        {
            Raylib.InitWindow(0, 0, _config.Title);
            Raylib.SetWindowPosition((Raylib.GetScreenWidth() - width) / 2, (Raylib.GetScreenHeight() - height) / 2);
            Raylib.SetWindowSize(width, height);
        }
        else
        {
            Raylib.InitWindow(width, height, _config.Title);
        }

        if (Platform.Desktop.IsCurrent && _config.MinScreenSize.HasValue)
            Raylib.SetWindowMinSize((int)_config.MinScreenSize.Value.X, (int)_config.MinScreenSize.Value.Y);
        if (Platform.Desktop.IsCurrent && _config.MaxScreenSize.HasValue)
            Raylib.SetWindowMaxSize((int)_config.MaxScreenSize.Value.X, (int)_config.MaxScreenSize.Value.Y);
        if (_config.Maximized)
            Maximize();
        if (_config.Fullscreen)
            ToggleFullscreen();
        if (_config.FpsTarget > 0)
            Raylib.SetTargetFPS(_config.FpsTarget);
        if (!Platform.Desktop.IsCurrent || OperatingSystem.IsMacOS() || _config.Icon is null)
            return;
        _icon = _config.Icon.Copy<PixelR8G8B8A8>();
        Raylib.SetWindowIcon(_icon.RImage);
    }

    private static ConfigFlags GetConfigFlags()
    {
        ConfigFlags flags = 0;
        if (_config.Resizable)
            flags |= ConfigFlags.WindowResizable;
        if (!_config.Decorated)
            flags |= ConfigFlags.WindowUndecorated;
        if (!_config.Focused)
            flags |= ConfigFlags.WindowUnfocused;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        if (_config.RunMinimized)
            flags |= ConfigFlags.WindowAlwaysRun;
        if (_config.Msaa4X)
            flags |= ConfigFlags.Msaa4XHint;
        if (_config.Hidden)
            flags |= ConfigFlags.WindowHidden;
        if (_config.TopMost)
            flags |= ConfigFlags.WindowTopMost;
        if (_config.Transparent)
            flags |= ConfigFlags.WindowTransparent;
        if (_config.Passthrough)
            flags |= ConfigFlags.WindowMousePassthrough;
        return flags;
    }
}
