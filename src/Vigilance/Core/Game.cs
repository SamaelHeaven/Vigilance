using System.Collections.Concurrent;
using System.Globalization;
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

public sealed partial class Game
{
    private static Game? _game;
    private readonly ConcurrentStack<Action> _actions = [];
    private GameConfig _config = null!;
    private Font _defaultFont = null!;
    private Box _previousScreen;
    private bool _quit;
    private bool _resetScreen;
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
            if (Platform.Web.IsCurrent())
                return JSEngine.Eval("!!document.fullscreenElement");
            return Raylib.IsWindowFullscreen();
        }
        set
        {
            if (value != Fullscreen)
                ToggleFullscreen();
        }
    }

    public static int Width => (int)GetGame()._config.Size.X;

    public static int Height => (int)GetGame()._config.Size.Y;

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
            game._config.FpsTarget = value;
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

    internal static GetSystemsDelegate Systems => GetGame()._config.Systems;

    public static Image Screenshot()
    {
        EnsureRunning();
        return Renderer.Buffer.Texture.ToImage();
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

    public static void Defer(Action action)
    {
        var game = GetGame();
        game._actions.Push(action);
    }

    public static void Log<T>(T value)
    {
        Log(LogLevel.Info, value);
    }

    public static void Log<T>(LogLevel level, T value)
    {
        var game = GetGame();
        if (game._config.LogLevel > level)
            return;
        var message = value?.ToString() ?? "";
        if (game._config.Logger is null)
        {
            if (level is > LogLevel.All and < LogLevel.None)
                Console.Write($"{level.ToString().ToUpper()}: ");
            Console.WriteLine(message);
            Console.Out.Flush();
        }
        else
        {
            game._config.Logger.Log(level, message);
        }

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

    public static void Focus()
    {
        EnsureRunning();
        if (Platform.Web.IsCurrent())
        {
            JSEngine.Eval("Module.canvas.focus()");
            return;
        }

        Raylib.SetWindowFocused();
    }

    public static void ToggleFullscreen()
    {
        var game = GetGame();
        if (Platform.Web.IsCurrent())
        {
            JSEngine.Eval(Fullscreen ? "document.exitFullscreen()" : "Module.canvas.requestFullscreen()");
            return;
        }

        if (Fullscreen)
        {
            game._resetScreen = true;
        }
        else
        {
            game._previousScreen = new Box(Raylib.GetWindowPosition(), ScreenSize);
            var monitor = Raylib.GetCurrentMonitor();
            ScreenSize = new Vector2(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
        }

        Raylib.ToggleFullscreen();
    }

    public static void Launch(GameConfig config, Scene scene)
    {
        Running = true;
        var game = GetGame();
        config = config.Clone();
        game._config = config;
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

    private unsafe void InitializeLogging()
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
        Raylib.InitWindow(
            (int)(_config.ScreenSize.X <= 0 || !Platform.Desktop.IsCurrent() ? _config.Size.X : _config.ScreenSize.X),
            (int)(_config.ScreenSize.Y <= 0 || !Platform.Desktop.IsCurrent() ? _config.Size.Y : _config.ScreenSize.Y),
            _config.Title
        );
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

    private unsafe void Loop()
    {
        if (Platform.Web.IsCurrent())
        {
            emscripten_set_main_loop(&UnmanagedFrame, 0, 1);
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
        _scene.Update();
        Renderer.Update();
        Raylib.PollInputEvents();
    }

    private void UpdateSize()
    {
        if (!_resetScreen)
            return;
        _resetScreen = false;
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
            flags |= ConfigFlags.ResizableWindow;
        if (!_config.Decorated)
            flags |= ConfigFlags.UndecoratedWindow;
        if (_config.Vsync)
            flags |= ConfigFlags.VSyncHint;
        return flags;
    }

    private void Dispose()
    {
        _config.QuitAction?.Invoke();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
        Environment.Exit(0);
    }

    // ReSharper disable once UseCollectionExpression
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void UnmanagedFrame()
    {
        GetGame().Frame();
    }

    // ReSharper disable once UseCollectionExpression
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void UnmanagedLog(int logLevel, sbyte* format, sbyte* args)
    {
        var message = Raylib_cs.Logging.GetLogMessage((nint)format, (nint)args);
        Log((LogLevel)logLevel, message);
    }

    [LibraryImport("libc")]
    private static unsafe partial void emscripten_set_main_loop(
        delegate* unmanaged[Cdecl]<void> func,
        int fps,
        sbyte simulateInfiniteLoop
    );
}
