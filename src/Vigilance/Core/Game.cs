using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Vigilance.Math;
using Color = Vigilance.Drawing.Color;
using Image = Vigilance.Drawing.Image;
using Music = Vigilance.Media.Music;
using Sound = Vigilance.Media.Sound;

namespace Vigilance.Core;

public sealed unsafe class Game
{
    private static Game? _game;
    private readonly ConcurrentStack<Action> _actions = [];
    private GameConfig _config = null!;
    private Configs _configs = null!;
    private Box _previousScreen;
    private bool _quit;
    private bool _resetScreen;
    private Scene _scene = null!;

    private Game() { }

    public static bool Running { get; private set; }

    public static Platform Platform { get; } =
        Enum.GetValues<Platform>().FirstOrDefault(platform => platform.IsCurrent());

    public static string Title
    {
        get => GetGame()._config.Title;
        set
        {
            GetGame()._config.Title = value;
            Raylib.SetWindowTitle(value);
        }
    }

    public static Key ExitKey
    {
        get => GetGame()._config.ExitKey;
        set
        {
            GetGame()._config.ExitKey = value;
            Raylib.SetExitKey((KeyboardKey)value);
        }
    }

    public static Key FullscreenKey
    {
        get => GetGame()._config.FullscreenKey;
        set => GetGame()._config.FullscreenKey = value;
    }

    public static Vector2 Size => GetGame()._config.Size;

    public static float Width => GetGame()._config.Size.X;

    public static float Height => GetGame()._config.Size.Y;

    public static Vector2 ScreenSize
    {
        get => new(ScreenWidth, ScreenHeight);
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            if (Fullscreen)
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
            EnsureRunning();
            if (Platform.Desktop.IsCurrent() && Fullscreen)
                return Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor());
            return Raylib.GetScreenWidth();
        }
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            if (Fullscreen)
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
            EnsureRunning();
            if (Platform.Desktop.IsCurrent() && Fullscreen)
                return Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor());
            return Raylib.GetScreenHeight();
        }
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            if (Fullscreen)
                return;
            if (ScreenHeight == value)
                return;
            Raylib.SetWindowSize(ScreenWidth, value);
        }
    }

    public static Viewport Viewport
    {
        get => GetGame()._config.Viewport;
        set => Defer(() => GetGame()._config.Viewport = value);
    }

    public static RenderingMode RenderingMode => GetGame()._config.RenderingMode;

    public static Color Background
    {
        get => GetGame()._config.Background;
        set => GetGame()._config.Background = value;
    }

    public static int FpsTarget
    {
        get => GetGame()._config.FpsTarget;
        set
        {
            var game = GetGame();
            if (value < 1)
                value = 0;
            if (value == FpsTarget)
                return;
            game._config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
        }
    }

    public static bool Fullscreen
    {
        get
        {
            EnsureRunning();
            return Platform switch
            {
                Platform.Web => JSEngine.Eval("!!document.fullscreenElement"),
                _ => Raylib.IsWindowFullscreen()
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
            EnsureRunning();
            return Raylib.IsWindowHidden();
        }
    }

    public static bool Maximized
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowMaximized();
        }
    }

    public static bool Minimized
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowMinimized();
        }
    }

    public static bool Vsync => GetGame()._config.Vsync;

    public static Scene Scene
    {
        get => GetGame()._scene;
        set
        {
            var game = GetGame();
            if (game._scene == value)
                return;
            Defer(() =>
            {
                game._scene.Stop();
                game._scene = value;
            });
        }
    }

    public static bool Focused
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowFocused();
        }
    }

    public static Configs Configs => _game?._configs ?? Configs.Empty;

    internal static GameSystemsFunc Systems => GetGame()._config.Systems;

    public static void OpenUrl(string url)
    {
        EnsureRunning();
        Raylib.OpenURL(url);
    }

    public static void EnsureRunning()
    {
        if (!Running)
            throw new InvalidOperationException("Game is not running.");
    }

    public static void EnsureNotRunning()
    {
        if (Running)
            throw new InvalidOperationException("Game is already running.");
    }

    public static void Defer(Action action)
    {
        var game = _game ??= new Game();
        game._actions.Push(action);
    }

    public static void Maximize()
    {
        EnsureRunning();
        if (!Maximized && Platform.Desktop.IsCurrent())
            Raylib.MaximizeWindow();
    }

    public static void Minimize()
    {
        EnsureRunning();
        if (!Minimized && Platform.Desktop.IsCurrent())
            Raylib.MinimizeWindow();
    }

    public static void Restore()
    {
        EnsureRunning();
        if (Platform.Desktop.IsCurrent() && (Maximized || Minimized))
            Raylib.RestoreWindow();
    }

    public static void Focus()
    {
        EnsureRunning();
        if (Platform == Platform.Web)
            JSEngine.Eval("Module.canvas.focus()");
        else
            Raylib.SetWindowFocused();
    }

    public static Image Screenshot()
    {
        var width = ScreenWidth;
        var height = ScreenHeight;
        Graphics.Reset();
        Graphics.DrawCurrentBuffer();
        var data = Rlgl.ReadScreenPixels(ScreenWidth, ScreenHeight);
        var rImage = new Raylib_cs.BleedingEdge.Image
        {
            Data = data,
            Width = width,
            Height = height,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8
        };
        return new Image(rImage);
    }

    public static void ToggleFullscreen()
    {
        var game = GetGame();
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
                game._resetScreen = true;
            }
            else
            {
                game._previousScreen = new Box(Raylib.GetWindowPosition(), ScreenSize);
                if (OperatingSystem.IsMacOS() && !Raylib.IsWindowMaximized())
                    Raylib.MaximizeWindow();
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

    public static void Launch(Configs configs, Scene scene)
    {
        EnsureNotRunning();
        Running = true;
        var game = _game ??= new Game();
        game._configs = configs;
        game._config = configs.Take<GameConfig>() ?? new GameConfig();
        game._scene = scene;
        Logger.Initialize();
        game.InitializeWindow();
        ExitKey = game._config.ExitKey;
        FpsTarget = game._config.FpsTarget;
        game.UpdateActions();
        try
        {
            game.Loop();
        }
        catch (Exception e)
        {
            Logger.Fatal(e);
        }
    }

    public static void Quit()
    {
        GetGame()._quit = true;
    }

    private static Game GetGame()
    {
        EnsureRunning();
        return _game ??= new Game();
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
        if (Platform.Desktop.IsCurrent())
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
            ToggleFullscreen();
        if (!Platform.Desktop.IsCurrent() || OperatingSystem.IsMacOS() || _config.Icon is null)
            return;
        var image = _config.Icon!.Invoke().Copy();
        image.Format = ImageFormat.UncompressedR8G8B8A8;
        Raylib.SetWindowIcon(image.RImage);
    }

    private void Loop()
    {
        if (Platform.Web.IsCurrent())
        {
            Emscripten.SetMainLoop(&UnmanagedFrame, 0, 1);
            return;
        }

        while (!Raylib.WindowShouldClose() && !_quit)
            Frame();
        Dispose();
    }

    private void Frame()
    {
        Time.Update();
        Keyboard.Update();
        Mouse.Update();
        Gamepad.UpdateAll();
        Music.UpdateAll();
        Sound.UpdateAll();
        UpdateSize();
        UpdateActions();
        UpdateFullscreen();
        Renderer.BeginDrawing();
        _scene.Update();
        Renderer.EndDrawing();
        Raylib.PollInputEvents();
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

    private void UpdateActions()
    {
        var length = _actions.Count;
        if (length == 0)
            return;
        var actions = new Action[length];
        var amount = _actions.TryPopRange(actions, 0, length);
        for (var i = amount - 1; i >= 0; i--)
            actions[i].Invoke();
    }

    private void UpdateFullscreen()
    {
        if (Keyboard.IsKeyPressed(_config.FullscreenKey))
            ToggleFullscreen();
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
        return flags;
    }

    private void Dispose()
    {
        _config.QuitAction?.Invoke();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
        Environment.Exit(0);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedFrame()
    {
        GetGame().Frame();
    }
}
