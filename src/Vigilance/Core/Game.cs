using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Raylib_cs.BleedingEdge.Interop;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Vigilance.Math;
using Color = Vigilance.Drawing.Color;
using Font = Vigilance.Drawing.Font;
using Image = Vigilance.Drawing.Image;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public sealed unsafe class Game
{
    private static Game? _game;
    private readonly ConcurrentStack<Action> _actions = [];
    private GameConfig _config = null!;
    private Font _defaultFont = null!;
    private GameConfig _launchConfig = null!;
    private Box _previousScreen;
    private bool _quit;
    private bool _resetScreen;
    private Scene _scene = null!;

    private Game()
    {
        EnsureRunning();
    }

    public static bool Running { get; private set; }

    public static Platform Platform { get; } =
        Enum.GetValues<Platform>().FirstOrDefault(platform => platform.IsCurrent());

    public static bool Fullscreen
    {
        get
        {
            EnsureRunning();
            return Platform switch
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

    public static float Width => GetGame()._config.Size.X;

    public static float Height => GetGame()._config.Size.Y;

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

    public static Viewport Viewport
    {
        get => GetGame()._config.Viewport;
        set => Defer(() => GetGame()._config.Viewport = value);
    }

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
            var game = GetGame();
            if (value < 1)
                value = 0;
            if (value == FpsTarget)
                return;
            game._config.FpsTarget = value;
            Raylib.SetTargetFPS(value);
        }
    }

    public static RenderingMode RenderingMode => GetGame()._config.RenderingMode;

    public static Color Background
    {
        get => GetGame()._config.Background;
        set => GetGame()._config.Background = value;
    }

    public static InputAxis HorizontalInputAxis
    {
        get => GetGame()._config.HorizontalInputAxis;
        set => GetGame()._config.HorizontalInputAxis = value;
    }

    public static InputAxis VerticalInputAxis
    {
        get => GetGame()._config.VerticalInputAxis;
        set => GetGame()._config.VerticalInputAxis = value;
    }

    public static Vector2 DefaultTextSpacing
    {
        get => GetGame()._config.DefaultTextSpacing;
        set => GetGame()._config.DefaultTextSpacing = value;
    }

    public static int DefaultFontQuality
    {
        get => GetGame()._config.DefaultFontQuality;
        set => GetGame()._config.DefaultFontQuality = value;
    }

    public static float DefaultFontSize
    {
        get => GetGame()._config.DefaultFontSize;
        set => GetGame()._config.DefaultFontSize = value;
    }

    public static Font DefaultFont
    {
        get => GetGame()._defaultFont;
        set => GetGame()._defaultFont = value;
    }

    public static string DefaultFontCharset
    {
        get => GetGame()._config.DefaultFontCharset;
        set => GetGame()._config.DefaultFontCharset = value;
    }

    public static CacheType DefaultAssetCacheType
    {
        get => GetGame()._config.DefaultAssetCacheType;
        set => GetGame()._config.DefaultAssetCacheType = value;
    }

    public static int DefaultSoundMaxAliases
    {
        get => GetGame()._config.DefaultSoundMaxAliases;
        set => GetGame()._config.DefaultSoundMaxAliases = value;
    }

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
            var game = GetGame();
            game._config.LogLevel = value;
            Raylib.SetTraceLogLevel((TraceLogLevel)game._config.LogLevel);
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

    public static GameConfig Config => GetGame()._launchConfig.ShallowClone();

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
        var game = GetGame();
        game._actions.Push(action);
    }

    public static void Log(object? value)
    {
        Log(value is Exception ? LogLevel.Error : LogLevel.Info, value);
    }

    public static void Log(LogLevel level, object? value)
    {
        var game = GetGame();
        if (game._config.LogLevel > level)
            return;
        var message = value is Exception e
            ? $"{e.GetType()}: {e.Message}{(e.StackTrace is null ? "" : $"\n{e.StackTrace}")}"
            : value?.ToString() ?? "";
        if (game._config.Logger is null)
            lock (Console.Out)
            {
                if (level is > LogLevel.All and < LogLevel.None)
                    Console.Write($"{level.ToString().ToUpper()}: ");
                Console.WriteLine(message);
                Console.Out.Flush();
            }
        else
            game._config.Logger.Log(level, message);

        if (level == LogLevel.Fatal)
            Environment.Exit(1);
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
            Format = PixelFormat.UncompressedR8G8B8A8,
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

    public static void Launch(GameConfig config, Scene scene)
    {
        EnsureNotRunning();
        Running = true;
        var game = GetGame();
        game._launchConfig = config.ShallowClone();
        game._config = config.ShallowClone();
        game._scene = scene;
        InitializeCultureInfo();
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

    public static void Quit()
    {
        GetGame()._quit = true;
    }

    private static Game GetGame()
    {
        return _game ??= new Game();
    }

    private static void InitializeCultureInfo()
    {
        var cultureInfo = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private void InitializeLogging()
    {
        var engine = Assemblies.Engine.GetName();
        var message = $"Initializing {engine.Name} {engine.Version}";
        Raylib.SetTraceLogLevel((TraceLogLevel)_config.LogLevel);
        try
        {
            if (Platform.Web.IsCurrent())
                throw new PlatformNotSupportedException();
            Raylib.SetTraceLogCallback(&UnmanagedLog);
            Raylib.TraceLog(TraceLogLevel.Info, message);
        }
        catch
        {
            _config.Logger = null;
            Raylib.SetTraceLogCallback(null);
            Log(LogLevel.Warning, "Failed to initialize custom logging");
            Log(message);
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

        if (Platform.Desktop.IsCurrent() && _config.MinSize.HasValue)
            Raylib.SetWindowMinSize((int)_config.MinSize.Value.X, (int)_config.MinSize.Value.Y);
        if (Platform.Desktop.IsCurrent() && _config.MaxSize.HasValue)
            Raylib.SetWindowMaxSize((int)_config.MaxSize.Value.X, (int)_config.MaxSize.Value.Y);
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

    private static void InitializeAudio()
    {
        Raylib.SetAudioStreamBufferSizeDefault(8192);
        if (!OperatingSystem.IsWindows())
        {
            Raylib.InitAudioDevice();
            return;
        }

        var thread = new Thread(Raylib.InitAudioDevice);
        thread.Start();
        thread.Join();
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedLog(TraceLogLevel logLevel, sbyte* format, nint args)
    {
        var message = NativeStringFormatter.Format((nint)format, args);
        Log((LogLevel)logLevel, message);
    }
}
