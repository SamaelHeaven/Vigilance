using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Vigilance.Math;
using Font = Vigilance.Drawing.Font;
using Image = Vigilance.Drawing.Image;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public sealed class Game
{
    private static Game? _game;
    private readonly ConcurrentStack<Action> _actions = [];
    private GameConfig _config;
    private Font _defaultFont = null!;
    private Vector2 _previousScreenSize = Vector2.Zero;
    private bool _resetSize;
    private Scene _scene = null!;

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

    public static int FpsTarget
    {
        get => GetGame()._config.FpsTarget;
        set
        {
            GetGame()._config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
        }
    }

    public static InputAxis HorizontalInputAxis => GetGame()._config.HorizontalInputAxis;

    public static InputAxis VerticalInputAxis => GetGame()._config.VerticalInputAxis;

    public static Interpolation DefaultInterpolation => GetGame()._config.DefaultInterpolation;

    public static Vector2 DefaultTextSpacing => GetGame()._config.DefaultTextSpacing;

    public static int DefaultFontQuality => GetGame()._config.DefaultFontQuality;

    public static float DefaultFontSize => GetGame()._config.DefaultFontSize;

    public static Font DefaultFont => GetGame()._defaultFont;

    public static string DefaultFontCharset => GetGame()._config.DefaultFontCharset;

    public static CacheType DefaultAssetCacheType => GetGame()._config.DefaultAssetCacheType;

    public static int DefaultSoundMaxAliases => System.Math.Max(GetGame()._config.DefaultSoundMaxAliases, 1);

    public static IImmutableList<ISystem> Systems => GetGame()._config.Systems;

    public static float MasterVolume
    {
        get
        {
            EnsureRunning();
            return Raylib.GetMasterVolume();
        }
        set
        {
            EnsureRunning();
            Raylib.SetMasterVolume(System.Math.Clamp(value, 0, 1));
        }
    }

    public static bool Debug
    {
        get => GetGame()._config.Debug;
        set => GetGame()._config.Debug = value;
    }

    public static LogLevel LogLevel
    {
        get => GetGame()._config.LogLevel;
        set
        {
            GetGame()._config.LogLevel = value;
            Raylib.SetTraceLogLevel((TraceLogLevel)value);
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
        EnsureRunning();
        return new Image(Raylib.LoadImageFromScreen());
    }

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

    public static void RunLater(Action action)
    {
        var game = GetGame();
        game._actions.Push(action);
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        var game = GetGame();
        if (game._config.LogLevel > level)
            return;
        if (Platform.Desktop.IsCurrent() && game._config.Logger != null)
        {
            game._config.Logger.Log(message, level);
            return;
        }

        Console.WriteLine($"{level.ToString().ToUpperInvariant()}: {message}");
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
            var monitor = Raylib.GetCurrentMonitor();
            ScreenSize = new Vector2(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
        }

        Raylib.ToggleFullscreen();
    }

    public static void Launch(GameConfig config, Scene scene)
    {
        Running = true;
        var game = GetGame();
        game._config = config;
        game._scene = scene;
        game.InitializeLogging();
        game.InitializeFileSystem();
        game.InitializeWindow();
        InitializeAudio();
        ExitKey = config.ExitKey;
        FpsTarget = config.FpsTarget;
        MasterVolume = config.MasterVolume;
        game._defaultFont = config.DefaultFont.Invoke();
        game.Loop();
    }

    private static Game GetGame()
    {
        return _game ??= new Game();
    }

    private unsafe void InitializeLogging()
    {
        var engine = Assemblies.Engine.GetName();
        Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        try
        {
            if (!Platform.Desktop.IsCurrent())
                throw new PlatformNotSupportedException();
            Raylib_cs.Logging.GetLogMessage(IntPtr.Zero, IntPtr.Zero);
            Raylib.SetTraceLogCallback(&TraceLog);
        }
        catch
        {
            _config.Logger = null;
            Log("Failed to initialize custom logging", LogLevel.Error);
        }
        finally
        {
            Log($"Initializing {engine.Name} {engine.Version}");
        }
    }

    private void InitializeFileSystem()
    {
        FileSystem.WorkingNamespace = _config.WorkingNamespace;
        FileSystem.ChangeDirectory(_config.WorkingDirectory);
    }

    private void InitializeWindow()
    {
        Raylib.SetConfigFlags(GetConfigFlags());
        Raylib.InitWindow(
            _config.ScreenWidth <= 0 || !Platform.Desktop.IsCurrent() ? _config.Width : _config.ScreenWidth,
            _config.ScreenHeight <= 0 || !Platform.Desktop.IsCurrent() ? _config.Height : _config.ScreenHeight,
            _config.Title
        );
        if (_config.Maximized)
            Maximize();
        if (_config.Fullscreen)
            ToggleFullscreen();
        if (Platform.Desktop.IsCurrent() && _config.Icon != null)
            Raylib.SetWindowIcon(_config.Icon!.Invoke().RImage);
    }

    private static void InitializeAudio()
    {
        Raylib.SetAudioStreamBufferSizeDefault(8192);
        Raylib.InitAudioDevice();
    }

    private void Loop()
    {
        Renderer.Initialize();
        while (!Raylib.WindowShouldClose())
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
            _scene.Update();
            Renderer.Update();
        }

        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    private void UpdateSize()
    {
        if (!_resetSize)
            return;
        _resetSize = false;
        ScreenSize = _previousScreenSize;
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
        if (_config.Msaa4X)
            flags |= ConfigFlags.Msaa4xHint;
        if (_config.Resizable)
            flags |= ConfigFlags.ResizableWindow;
        if (!_config.Decorated)
            flags |= ConfigFlags.UndecoratedWindow;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        return flags;
    }

    // ReSharper disable once UseCollectionExpression
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void TraceLog(int logLevel, sbyte* format, sbyte* args)
    {
        var message = Raylib_cs.Logging.GetLogMessage((nint)format, (nint)args);
        GetGame()._config.Logger?.Log(message, (LogLevel)logLevel);
    }
}
