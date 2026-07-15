using Raylib_cs;
using Vigilance.Drawing;
using Vigilance.Logging;
using Vigilance.Math;
using Color = Vigilance.Drawing.Color;
using Image = Vigilance.Drawing.Image;
using PixelFormat = Raylib_cs.PixelFormat;

namespace Vigilance.Core;

public static unsafe class Display
{
    private static DisplayConfig _config = new();
    private static Vector2 _size = _config.Size;
    private static Vector2 _screenSize = _config.ScreenSize;
    private static Vector2? _position = _config.Position;
    private static Viewport _viewport = _config.Viewport;
    private static RenderingMode _renderingMode = _config.RenderingMode;
    private static Vector2 _renderSize;
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
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            Raylib.SetWindowTitle(value);
        }
    } = _config.Title;

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
                Raylib.SetWindowIcons([]);
                return;
            }

            Raylib.SetWindowIcon(_icon.RImage);
        }
    }

    public static Vector2 Size
    {
        get => Viewport == Viewport.Native ? ScreenSize : _size;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenSize = value;
                return;
            }

            _size = value;
        }
    }

    public static float Width
    {
        get => Viewport == Viewport.Native ? ScreenWidth : _size.X;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenWidth = (int)value;
                return;
            }

            _size = new Vector2(value, _size.Y);
        }
    }

    public static float Height
    {
        get => Viewport == Viewport.Native ? ScreenHeight : _size.Y;
        set
        {
            if (Viewport == Viewport.Native)
            {
                ScreenHeight = (int)value;
                return;
            }

            _size = new Vector2(_size.X, value);
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
            var size = value.Floor();
            if (ScreenSize == size)
                return;
            _screenSize = size;
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
        }
    }

    public static int ScreenWidth
    {
        get => (int)_screenSize.X;
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            if (ScreenWidth == value)
                return;
            _screenSize = new Vector2(value, ScreenHeight);
            Raylib.SetWindowSize(value, ScreenHeight);
        }
    }

    public static int ScreenHeight
    {
        get => (int)_screenSize.Y;
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            if (ScreenHeight == value)
                return;
            _screenSize = new Vector2(ScreenWidth, value);
            Raylib.SetWindowSize(ScreenWidth, value);
        }
    }

    public static Vector2? MinScreenSize
    {
        get => field;
        set
        {
            value = value?.Floor();
            if (value == field)
                return;
            field = value;
            if (Platform.Desktop.IsCurrent)
                Raylib.SetWindowMinSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    } = _config.MinScreenSize;

    public static Vector2? MaxScreenSize
    {
        get => field;
        set
        {
            value = value?.Floor();
            if (value == field)
                return;
            field = value;
            if (Platform.Desktop.IsCurrent)
                Raylib.SetWindowMaxSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    } = _config.MaxScreenSize;

    public static Vector2 Position
    {
        get => _position ?? Vector2.Zero;
        set
        {
            if (!Platform.Desktop.IsCurrent)
                return;
            value = value.Floor();
            if (value == _position)
                return;
            if (Fullscreen)
            {
                _previousScreen.Position = value;
            }
            else
            {
                if (Maximized)
                    Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                else if (OperatingSystem.IsMacOS() && ScreenSize == MonitorSize)
                    return;
                Raylib.SetWindowPosition((int)value.X, (int)value.Y);
            }

            _position = Raylib.GetWindowPosition();
        }
    }

    public static Viewport Viewport
    {
        get => _viewport;
        set { Game.Defer(() => _viewport = value); }
    }

    public static RenderingMode RenderingMode
    {
        get => _renderingMode;
        set { Game.Defer(() => _renderingMode = value); }
    }

    public static Color Background { get; set; } = _config.Background;

    public static int FpsTarget
    {
        get => field;
        set
        {
            if (value < 1)
                value = 0;
            if (value == field)
                return;
            field = value;
            Raylib.SetTargetFPS(value);
        }
    } = _config.FpsTarget;

    public static bool Fullscreen
    {
        get => _fullscreen;
        set
        {
            if (value != Fullscreen)
                ToggleFullscreen();
        }
    }

    public static bool DefaultFullscreenBorderless { get; set; } = _config.DefaultFullscreenBorderless;

    public static bool Hidden { get; private set; }

    public static bool Maximized { get; private set; }

    public static bool Minimized { get; private set; }

    public static bool Decorated
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.UndecoratedWindow, !value);
        }
    } = _config.Decorated;

    public static bool Vsync
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.VSyncHint, value);
        }
    } = _config.Vsync;

    public static bool Resizable
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.ResizableWindow, value);
        }
    } = _config.Resizable;

    public static bool TopMost
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.TopmostWindow, value);
        }
    } = _config.TopMost;

    public static bool Transparent
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.TransparentWindow, value);
        }
    } = _config.Transparent;

    public static bool Passthrough
    {
        get => field;
        set
        {
            if (value == field)
                return;
            field = value;
            ToggleWindowState(ConfigFlags.MousePassthroughWindow, value);
        }
    } = _config.Passthrough;

    public static bool RunMinimized { get; private set; } = _config.RunMinimized;

    public static bool Msaa4X { get; private set; } = _config.Msaa4X;

    public static bool Focused { get; private set; }

    public static Graphics Graphics { get; internal set; } = null!;

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
            JSEngine.Run("Module.canvas.focus()"u8);
        else
            Raylib.SetWindowFocused();
        Focused = Raylib.IsWindowFocused();
    }

    public static void ToggleFullscreen(bool? borderless = null)
    {
        var borderlessValue = borderless ?? DefaultFullscreenBorderless;
        if (Platform.Web.IsCurrent)
        {
            JSEngine.Run(Fullscreen ? "document.exitFullscreen()"u8 : "Module.canvas.requestFullscreen()"u8);
            _fullscreen = JSEngine.Eval("!!document.fullscreenElement"u8);
        }
        else if (Platform.Desktop.IsCurrent)
        {
            var fullscreen = _fullscreen;
            if (fullscreen)
            {
                _resetScreen = true;
            }
            else
            {
                var monitorSize = MonitorSize;
                var maximized = (bool)Raylib.IsWindowMaximized();
                _previousScreen = new Box(maximized ? Vector2.Zero : Raylib.GetWindowPosition(), ScreenSize);
                if (OperatingSystem.IsMacOS())
                    switch (maximized)
                    {
                        case false when ScreenSize == monitorSize:
                            return;
                        case true:
                            Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
                            break;
                        default:
                            Raylib.MaximizeWindow();
                            break;
                    }
            }

            _fullscreen = !fullscreen;
            if (borderlessValue)
            {
                Raylib.ToggleBorderlessWindowed();
                if (_fullscreen && !Decorated)
                    Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
            }
            else
            {
                Raylib.ToggleFullscreen();
            }
        }
    }

    public static WritableImage<PixelR8G8B8A8> Screenshot()
    {
        var width = (int)_renderSize.X;
        var height = (int)_renderSize.Y;
        Graphics.ResetCurrentBuffer();
        Graphics.DrawCurrentBuffer();
        var image = new Raylib_cs.Image
        {
            Data = Rlgl.ReadScreenPixels(width, height),
            Width = width,
            Height = height,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8,
        };
        return new WritableImage<PixelR8G8B8A8>(new WritableImage(new Image(image)));
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<DisplayConfig>() ?? _config;
        _size = _config.Size;
        _screenSize = _config.ScreenSize;
        _position = _config.Position;
        _viewport = _config.Viewport;
        _renderingMode = _config.RenderingMode;
        Background = _config.Background;
        DefaultFullscreenBorderless = _config.DefaultFullscreenBorderless;
        RunMinimized = _config.RunMinimized;
        Msaa4X = _config.Msaa4X;
        InitializeWindow();
        Title = _config.Title;
        MinScreenSize = _config.MinScreenSize;
        MaxScreenSize = _config.MaxScreenSize;
        FpsTarget = _config.FpsTarget;
        Decorated = _config.Decorated;
        Vsync = _config.Vsync;
        Resizable = _config.Resizable;
        TopMost = _config.TopMost;
        Transparent = _config.Transparent;
        Passthrough = _config.Passthrough;
    }

    internal static void Update()
    {
        if (OperatingSystem.IsWindows() && !Raylib.IsWindowFocused() && Raylib.IsWindowFullscreen())
            Raylib.MinimizeWindow();
        if (Platform.Web.IsCurrent)
        {
            var previousFullscreen = _fullscreen;
            _fullscreen = JSEngine.Eval("!!document.fullscreenElement"u8);
            if (_fullscreen != previousFullscreen)
                JSEngine.Run(
                    """
                    Module.canvas.blur();
                    Module.canvas.focus();
                    """u8
                );
            RefreshRate = 0;
            MonitorWidth = JSEngine.Eval("screen.width"u8);
            MonitorHeight = JSEngine.Eval("screen.height"u8);
            Focused = JSEngine.Eval("document.activeElement === Module.canvas"u8);
        }
        else
        {
            var monitor = Raylib.GetCurrentMonitor();
            RefreshRate = Raylib.GetMonitorRefreshRate(monitor);
            MonitorWidth = Raylib.GetMonitorWidth(monitor);
            MonitorHeight = Raylib.GetMonitorHeight(monitor);
            Focused = Raylib.IsWindowFocused();
        }

        _position = Raylib.GetWindowPosition();
        Hidden = Raylib.IsWindowHidden();
        Maximized = Raylib.IsWindowMaximized();
        Minimized = Raylib.IsWindowMinimized();
        _screenSize =
            Platform.Desktop.IsCurrent && Fullscreen
                ? new Vector2(MonitorWidth, MonitorHeight)
                : new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        _renderSize = new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
        if (!_resetScreen)
            return;
        _resetScreen = false;
        if (Maximized && _previousScreen.Position != Vector2.Zero)
            Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
        if (OperatingSystem.IsMacOS())
            Raylib.SetWindowPosition(1, 1);
        Raylib.SetWindowPosition((int)_previousScreen.Position.X, (int)_previousScreen.Position.Y);
        if (OperatingSystem.IsMacOS())
            ScreenSize = Vector2.One;
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
        var width = (int)(_screenSize.X <= 0 || !Platform.Desktop.IsCurrent ? _size.X : _screenSize.X);
        var height = (int)(_screenSize.Y <= 0 || !Platform.Desktop.IsCurrent ? _size.Y : _screenSize.Y);
        var logLevel = Log.SetLogLevel(Log.LogLevel.Max(LogLevel.Info));
        Raylib.InitWindow(width, height, _config.Title);
        Log.LogLevel = logLevel;
        if (OperatingSystem.IsWindows())
        {
            if (!_position.HasValue)
            {
                var monitor = Raylib.GetCurrentMonitor();
                var monitorSize = new Vector2(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
                var windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Raylib.SetWindowPosition(
                    (int)((monitorSize.X - windowSize.X) / 2),
                    (int)((monitorSize.Y - windowSize.Y) / 2)
                );
            }

            if (!_config.Hidden)
                Raylib.ClearWindowState(ConfigFlags.HiddenWindow);
        }

        if (Platform.Desktop.IsCurrent && _position.HasValue)
            Raylib.SetWindowPosition((int)_position.Value.X, (int)_position.Value.Y);
        if (_config.Maximized)
            Maximize();
        if (_config.Fullscreen)
            ToggleFullscreen();
        if (!Platform.Desktop.IsCurrent || OperatingSystem.IsMacOS() || _config.Icon is null)
            return;
        _icon = _config.Icon.Copy<PixelR8G8B8A8>();
        Raylib.SetWindowIcon(_icon.RImage);
    }

    private static ConfigFlags GetConfigFlags()
    {
        ConfigFlags flags = 0;
        if (_config.Resizable)
            flags |= ConfigFlags.ResizableWindow;
        if (!_config.Decorated)
            flags |= ConfigFlags.UndecoratedWindow;
        if (!_config.Focused)
            flags |= ConfigFlags.UnfocusedWindow;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        if (RunMinimized)
            flags |= ConfigFlags.AlwaysRunWindow;
        if (Msaa4X)
            flags |= ConfigFlags.Msaa4xHint;
        if (_config.Hidden || OperatingSystem.IsWindows())
            flags |= ConfigFlags.HiddenWindow;
        if (_config.TopMost)
            flags |= ConfigFlags.TopmostWindow;
        if (_config.Transparent)
            flags |= ConfigFlags.TransparentWindow;
        if (_config.Passthrough)
            flags |= ConfigFlags.MousePassthroughWindow;
        return flags;
    }
}
