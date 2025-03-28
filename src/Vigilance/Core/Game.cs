using Raylib_cs;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Math;
using Raylib = Raylib_cs.Raylib;

namespace Vigilance.Core;

public sealed class Game
{
    internal static readonly List<GameSystem> Systems = [];
    private static Game? _game;
    private readonly List<Action> _actions = [];
    private GameConfig _config;
    private Vector2 _previousScreenSize = Vector2.Zero;
    private bool _resetSize;
    private Scene _scene = null!;

    static Game()
    {
        System(Camera.System);
        System(Graphics.System);
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

    public static Interpolation DefaultInterpolation
    {
        get => GetGame()._config.DefaultInterpolation;
        set => GetGame()._config.DefaultInterpolation = value;
    }

    public static bool Focused
    {
        get
        {
            EnsureRunning();
            return Raylib.IsWindowFocused();
        }
    }

    public static void System(GameSystem system)
    {
        EnsureNotRunning();
        Systems.Add(system);
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

    public static void Launch(GameConfig gameConfig, Scene scene)
    {
        Running = true;
        var game = GetGame();
        game._config = gameConfig;
        game._scene = scene;
        FileSystem.ChangeDirectory(gameConfig.WorkingDirectory);
        Raylib.SetTraceLogLevel(gameConfig.Debug ? TraceLogLevel.All : TraceLogLevel.Error);
        Raylib.SetConfigFlags(game.GetConfigFlags());
        Raylib.InitWindow(gameConfig.Width, gameConfig.Height, gameConfig.Title);
        Raylib.SetTargetFPS(gameConfig.FpsTarget);
        Raylib.SetExitKey(gameConfig.ExitKey.HasValue ? (KeyboardKey)gameConfig.ExitKey.Value : KeyboardKey.Null);
        Raylib.SetWindowSize(
            gameConfig.ScreenWidth <= 0 ? gameConfig.Width : gameConfig.ScreenWidth,
            gameConfig.ScreenHeight <= 0 ? gameConfig.Height : gameConfig.ScreenHeight
        );
        if (gameConfig.Fullscreen)
            ToggleFullscreen();
        if (FileSystem.FileExists(gameConfig.Icon))
        {
            var icon = Raylib.LoadImage(FileSystem.FormatPath(gameConfig.Icon));
            Raylib.SetWindowIcon(icon);
            Raylib.UnloadImage(icon);
        }

        game.Loop();
    }

    public static void ToggleFullscreen()
    {
        EnsureRunning();
        var game = GetGame();
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
