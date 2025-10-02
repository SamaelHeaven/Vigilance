using Raylib_cs.BleedingEdge;
using Vigilance.Drawing;
using Vigilance.Math;
using Color = Vigilance.Drawing.Color;
using Image = Vigilance.Drawing.Image;
using PixelFormat = Raylib_cs.BleedingEdge.PixelFormat;

namespace Vigilance.Core;

public sealed unsafe class Display
{
    private static Display? _display;
    private DisplayConfig _config = null!;
    private Image? _icon;
    private Box _previousScreen;
    private bool _resetScreen;

    private Display()
    {
        Game.EnsureRunning();
    }

    public static string Title
    {
        get => GetDisplay()._config.Title;
        set
        {
            GetDisplay()._config.Title = value;
            Raylib.SetWindowTitle(value);
        }
    }

    public static Image? Icon
    {
        get => GetDisplay()._icon;
        set
        {
            var display = GetDisplay();
            if (display._icon == value)
                return;
            display._icon = value?.Copy<PixelR8G8B8A8>();
            if (!Platform.Desktop.IsCurrent() || OperatingSystem.IsMacOS())
                return;
            if (display._icon is null)
            {
                Raylib.SetWindowIcons(ReadOnlySpan<Raylib_cs.BleedingEdge.Image>.Empty);
                return;
            }

            Raylib.SetWindowIcon(display._icon.RImage);
        }
    }

    public static Vector2 Size => GetDisplay()._config.Size;

    public static float Width => GetDisplay()._config.Size.X;

    public static float Height => GetDisplay()._config.Size.Y;

    public static int RefreshRate
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor());
        }
    }

    public static Vector2 ScreenSize
    {
        get => new(ScreenWidth, ScreenHeight);
        set
        {
            Game.EnsureRunning();
            if (!Platform.Desktop.IsCurrent())
                return;
            var size = value.Round();
            if (ScreenSize == size)
                return;
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
        }
    }

    public static int ScreenWidth
    {
        get
        {
            Game.EnsureRunning();
            if (Platform.Desktop.IsCurrent() && Fullscreen)
                return Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor());
            return Raylib.GetScreenWidth();
        }
        set
        {
            Game.EnsureRunning();
            if (!Platform.Desktop.IsCurrent())
                return;
            if (ScreenWidth == value)
                return;
            Raylib.SetWindowSize(value, ScreenHeight);
        }
    }

    public static int ScreenHeight
    {
        get
        {
            Game.EnsureRunning();
            if (Platform.Desktop.IsCurrent() && Fullscreen)
                return Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor());
            return Raylib.GetScreenHeight();
        }
        set
        {
            Game.EnsureRunning();
            if (!Platform.Desktop.IsCurrent())
                return;
            if (ScreenHeight == value)
                return;
            Raylib.SetWindowSize(ScreenWidth, value);
        }
    }

    public static Vector2? MinScreenSize
    {
        get => GetDisplay()._config.MinScreenSize;
        set
        {
            value = value?.Round();
            var display = GetDisplay();
            if (value == display._config.MinScreenSize)
                return;
            display._config.MinScreenSize = value;
            if (Platform.Desktop.IsCurrent())
                Raylib.SetWindowMinSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    }

    public static Vector2? MaxScreenSize
    {
        get => GetDisplay()._config.MaxScreenSize;
        set
        {
            value = value?.Round();
            var display = GetDisplay();
            if (value == display._config.MaxScreenSize)
                return;
            display._config.MaxScreenSize = value;
            if (Platform.Desktop.IsCurrent())
                Raylib.SetWindowMaxSize((int)(value?.X ?? 0), (int)(value?.Y ?? 0));
        }
    }

    public static Viewport Viewport
    {
        get => GetDisplay()._config.Viewport;
        set
        {
            Game.EnsureRunning();
            Game.Defer(() => GetDisplay()._config.Viewport = value);
        }
    }

    public static RenderingMode RenderingMode => GetDisplay()._config.RenderingMode;

    public static Color Background
    {
        get => GetDisplay()._config.Background;
        set => GetDisplay()._config.Background = value;
    }

    public static int FpsTarget
    {
        get => GetDisplay()._config.FpsTarget;
        set
        {
            var display = GetDisplay();
            if (value < 1)
                value = 0;
            if (value == FpsTarget)
                return;
            display._config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
        }
    }

    public static bool Fullscreen
    {
        get
        {
            Game.EnsureRunning();
            return Game.Platform switch
            {
                Platform.Web => JSEngine.Eval("!!document.fullscreenElement"),
                _ => Raylib.IsWindowFullscreen(),
            };
        }
        set
        {
            if (value != Fullscreen)
                ToggleFullscreen();
        }
    }

    public static bool Hidden
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.IsWindowHidden();
        }
    }

    public static bool Maximized
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.IsWindowMaximized();
        }
    }

    public static bool Minimized
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.IsWindowMinimized();
        }
    }

    public static bool Decorated
    {
        get => GetDisplay()._config.Decorated;
        set
        {
            var display = GetDisplay();
            if (value == display._config.Decorated)
                return;
            display._config.Decorated = value;
            ToggleWindowState(ConfigFlags.WindowUndecorated, !value);
        }
    }

    public static bool Vsync
    {
        get => GetDisplay()._config.Vsync;
        set
        {
            var display = GetDisplay();
            if (value == display._config.Vsync)
                return;
            display._config.Vsync = value;
            ToggleWindowState(ConfigFlags.VSyncHint, value);
        }
    }

    public static bool Resizable
    {
        get => GetDisplay()._config.Resizable;
        set
        {
            var display = GetDisplay();
            if (value == display._config.Resizable)
                return;
            display._config.Resizable = value;
            ToggleWindowState(ConfigFlags.WindowResizable, value);
        }
    }

    public static bool RunMinimized => GetDisplay()._config.RunMinimized;

    public static bool Msaa4X => GetDisplay()._config.Msaa4X;

    public static bool Focused
    {
        get
        {
            Game.EnsureRunning();
            return Raylib.IsWindowFocused();
        }
    }

    public static void Maximize()
    {
        Game.EnsureRunning();
        if (!Maximized && Platform.Desktop.IsCurrent())
            Raylib.MaximizeWindow();
    }

    public static void Minimize()
    {
        Game.EnsureRunning();
        if (!Minimized && Platform.Desktop.IsCurrent())
            Raylib.MinimizeWindow();
    }

    public static void Restore()
    {
        Game.EnsureRunning();
        if (Platform.Desktop.IsCurrent() && (Maximized || Minimized))
            Raylib.RestoreWindow();
    }

    public static void Focus()
    {
        Game.EnsureRunning();
        if (Platform.Web.IsCurrent())
            JSEngine.Eval("Module.canvas.focus()");
        else
            Raylib.SetWindowFocused();
    }

    public static void ToggleFullscreen(bool resizeScreen = true)
    {
        var display = GetDisplay();
        if (Platform.Web.IsCurrent())
        {
            JSEngine.Eval(Fullscreen ? "document.exitFullscreen()" : "Module.canvas.requestFullscreen()");
        }
        else if (Platform.Desktop.IsCurrent())
        {
            var monitor = Raylib.GetCurrentMonitor();
            var monitorSize = new Vector2(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
            if (Fullscreen)
            {
                display._resetScreen = resizeScreen;
            }
            else
            {
                display._previousScreen = new Box(Raylib.GetWindowPosition(), ScreenSize);
                if (OperatingSystem.IsMacOS() && !Raylib.IsWindowMaximized())
                    Raylib.MaximizeWindow();
                if (resizeScreen)
                    ScreenSize = monitorSize;
            }

            var fullscreen = Fullscreen;
            var screenSize = ScreenSize;
            if (
                !OperatingSystem.IsMacOS()
                || fullscreen
                || screenSize != monitorSize
                || (Vector2)Raylib.GetWindowPosition() != Vector2.Zero
            )
                Raylib.ToggleFullscreen();
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
        var display = GetDisplay();
        display._config = Game.Config.Take<DisplayConfig>() ?? new DisplayConfig();
        display.InitializeWindow();
    }

    internal static void Update()
    {
        var display = GetDisplay();
        display.UpdateSize();
    }

    internal static void Dispose()
    {
        Raylib.CloseWindow();
    }

    private static Display GetDisplay()
    {
        return _display ??= new Display();
    }

    private static void ToggleWindowState(ConfigFlags flag, bool value)
    {
        if (!Platform.Desktop.IsCurrent())
            return;
        if (value)
        {
            Raylib.SetWindowState(flag);
            return;
        }

        Raylib.ClearWindowState(flag);
    }

    private void InitializeWindow()
    {
        Raylib.SetConfigFlags(GetConfigFlags());
        var width = (int)(
            _config.ScreenSize.X <= 0 || !Platform.Desktop.IsCurrent() ? _config.Size.X : _config.ScreenSize.X
        );
        var height = (int)(
            _config.ScreenSize.Y <= 0 || !Platform.Desktop.IsCurrent() ? _config.Size.Y : _config.ScreenSize.Y
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

        if (Platform.Desktop.IsCurrent() && _config.MinScreenSize.HasValue)
            Raylib.SetWindowMinSize((int)_config.MinScreenSize.Value.X, (int)_config.MinScreenSize.Value.Y);
        if (Platform.Desktop.IsCurrent() && _config.MaxScreenSize.HasValue)
            Raylib.SetWindowMaxSize((int)_config.MaxScreenSize.Value.X, (int)_config.MaxScreenSize.Value.Y);
        if (_config.Maximized)
            Maximize();
        if (_config.Fullscreen)
            ToggleFullscreen(_config.ScreenSize.X <= 0 || _config.ScreenSize.Y <= 0);
        if (_config.FpsTarget > 0)
            Raylib.SetTargetFPS(_config.FpsTarget);
        if (!Platform.Desktop.IsCurrent() || OperatingSystem.IsMacOS() || _config.Icon is null)
            return;
        _icon = _config.Icon.Copy<PixelR8G8B8A8>();
        Raylib.SetWindowIcon(_icon.RImage);
    }

    private ConfigFlags GetConfigFlags()
    {
        ConfigFlags flags = 0;
        if (_config.Resizable)
            flags |= ConfigFlags.WindowResizable;
        if (!_config.Decorated)
            flags |= ConfigFlags.WindowUndecorated;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        if (_config.RunMinimized)
            flags |= ConfigFlags.WindowAlwaysRun;
        if (_config.Msaa4X)
            flags |= ConfigFlags.Msaa4XHint;
        if (_config.Hidden)
            flags |= ConfigFlags.WindowHidden;
        return flags;
    }

    private void UpdateSize()
    {
        if (!_resetScreen)
            return;
        _resetScreen = false;
        if (OperatingSystem.IsMacOS())
            Raylib.SetWindowPosition(1, 1);
        Raylib.SetWindowPosition((int)_previousScreen.Position.X, (int)_previousScreen.Position.Y);
        ScreenSize = _previousScreen.Size;
    }
}
