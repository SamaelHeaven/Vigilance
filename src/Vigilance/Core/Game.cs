using Raylib_cs;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;
using Vigilance.Systems;
using Font = Vigilance.Drawing.Font;
using Image = Vigilance.Drawing.Image;

namespace Vigilance.Core;

public sealed class Game
{
    internal static readonly List<ISystem> SystemList = [];
    private static Game? _game;
    private readonly List<Action> _actions = [];
    private GameConfig _config;
    private Vector2 _previousScreenSize = Vector2.Zero;
    private bool _resetSize;
    private Scene _scene = null!;

    static Game()
    {
        Systems([new FpsCounterSystem(), new CameraSystem(), new GraphicsSystem()]);
    }

    private Game()
    {
        EnsureRunning();
    }

    public static bool Running { get; private set; }

    public static bool Fullscreen
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowFullscreen();
        }
        set
        {
            if (value != Fullscreen)
                ToggleFullscreen();
        }
    }

    public static int Width => GetGame()._config.Width;

    public static int Height => GetGame()._config.Height;

    public static Vector2 Size => new(Width, Height);

    public static int ScreenWidth
    {
        get
        {
            EnsureRunning();
            return Raylib.GetScreenWidth();
        }
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            if (Fullscreen && value != Width)
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
            return Raylib.GetScreenHeight();
        }
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            if (Fullscreen && value != Height)
                return;
            if (ScreenHeight == value)
                return;
            Raylib.SetWindowSize(ScreenWidth, value);
        }
    }

    public static Vector2 ScreenSize
    {
        get => new(ScreenWidth, ScreenHeight);
        set
        {
            if (!Platform.Desktop.IsCurrent())
                return;
            var size = value.Round();
            if (Fullscreen && size != Size)
                return;
            if (ScreenSize == size)
                return;
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
        }
    }

    public static Scene Scene
    {
        get => GetGame()._scene;
        set => GetGame()._scene = value;
    }

    public static string Title
    {
        get => GetGame()._config.Title;
        set
        {
            GetGame()._config.Title = value;
            Raylib.SetWindowTitle(value);
        }
    }

    public static Key? ExitKey
    {
        get => GetGame()._config.ExitKey;
        set
        {
            GetGame()._config.ExitKey = value;
            Raylib.SetExitKey(value.HasValue ? (KeyboardKey)value.Value : KeyboardKey.Null);
        }
    }

    public static Key? FullscreenKey
    {
        get => GetGame()._config.FullscreenKey;
        set => GetGame()._config.FullscreenKey = value;
    }

    public static int FpsTarget
    {
        get => GetGame()._config.FpsTarget;
        set
        {
            GetGame()._config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
            Time.Restart();
        }
    }

    public static Interpolation DefaultInterpolation => GetGame()._config.DefaultInterpolation;

    public static Vector2 DefaultTextSpacing => GetGame()._config.DefaultTextSpacing;

    public static int DefaultFontQuality => GetGame()._config.DefaultFontQuality;

    public static float DefaultFontSize => GetGame()._config.DefaultFontSize;

    public static Font DefaultFont => GetGame()._config.DefaultFont();

    public static string DefaultFontCharset => GetGame()._config.DefaultFontCharset;

    public static bool Debug
    {
        get => GetGame()._config.Debug;
        set
        {
            GetGame()._config.Debug = value;
            Raylib.SetTraceLogLevel(value ? TraceLogLevel.All : TraceLogLevel.Error);
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
        set
        {
            EnsureRunning();
            if (!Maximized && value)
                Raylib.MaximizeWindow();
        }
    }

    public static bool Minimized
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowMinimized();
        }
        set
        {
            EnsureRunning();
            if (!Minimized && value)
                Raylib.MinimizeWindow();
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

    public static Image Screenshot()
    {
        return new Image(Raylib.LoadImageFromScreen());
    }

    public static void System(ISystem system)
    {
        EnsureNotRunning();
        SystemList.Add(system);
    }

    public static void Systems(IEnumerable<ISystem> systems)
    {
        EnsureNotRunning();
        SystemList.AddRange(systems);
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

    public static void RunLater(Action action)
    {
        var game = GetGame();
        lock (game._actions)
        {
            game._actions.Add(action);
        }
    }

    public static void Launch(GameConfig config, Scene scene)
    {
        Running = true;
        var game = GetGame();
        game._config = config;
        game._scene = scene;
        FileSystem.WorkingModule = config.WorkingModule;
        FileSystem.ChangeDirectory(config.WorkingDirectory);
        Raylib.SetTraceLogLevel(config.Debug ? TraceLogLevel.All : TraceLogLevel.Error);
        Raylib.SetConfigFlags(game.GetConfigFlags());
        Raylib.InitWindow(config.Width, config.Height, config.Title);
        Raylib.SetTargetFPS(config.FpsTarget);
        Raylib.SetExitKey(config.ExitKey.HasValue ? (KeyboardKey)config.ExitKey.Value : KeyboardKey.Null);
        if (Platform.Desktop.IsCurrent())
            Raylib.SetWindowSize(
                config.ScreenWidth <= 0 ? config.Width : config.ScreenWidth,
                config.ScreenHeight <= 0 ? config.Height : config.ScreenHeight
            );
        if (config.Maximized)
            Maximized = true;
        if (config.Fullscreen)
            ToggleFullscreen();
        if (Platform.Desktop.IsCurrent() && config.Icon != null)
            Raylib.SetWindowIcon(config.Icon!.Invoke().RImage);
        game.Loop();
    }

    public static void ToggleFullscreen()
    {
        var game = GetGame();
        if (Platform.Web.IsCurrent())
            return;
        if (Fullscreen)
        {
            game._resetSize = true;
        }
        else
        {
            game._previousScreenSize = ScreenSize;
            ScreenSize = Size;
        }

        Raylib.ToggleFullscreen();
    }

    private static Game GetGame()
    {
        return _game ??= new Game();
    }

    private void Loop()
    {
        while (!Raylib.WindowShouldClose())
        {
            GC.Collect();
            Time.Update();
            Keyboard.Update();
            Mouse.Update();
            Gamepad.UpdateAll();
            UpdateSize();
            UpdateActions();
            UpdateFullscreen();
            _scene.Update();
            Renderer.Update();
        }
    }

    private void UpdateSize()
    {
        if (!_resetSize)
            return;
        ScreenSize = _previousScreenSize;
        if (ScreenSize == _previousScreenSize)
            _resetSize = false;
    }

    private void UpdateActions()
    {
        lock (_actions)
        {
            foreach (var action in _actions)
                action.Invoke();
            _actions.Clear();
        }
    }

    private void UpdateFullscreen()
    {
        if (_config.FullscreenKey.HasValue && Keyboard.IsKeyPressed(_config.FullscreenKey.Value))
            ToggleFullscreen();
    }

    private ConfigFlags GetConfigFlags()
    {
        ConfigFlags flags = 0;
        if (_config.Resizable)
            flags |= ConfigFlags.ResizableWindow;
        if (!_config.Decorated)
            flags |= ConfigFlags.UndecoratedWindow;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        return flags;
    }
}
