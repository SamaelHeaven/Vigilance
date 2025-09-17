using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Image = Vigilance.Drawing.Image;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public sealed unsafe class Game
{
    private static Game? _game;
    private static Action? _quitAction = null;
    private static readonly ConcurrentStack<Action> Actions = [];
    private Config _config = null!;
    private bool _quit;
    private Scene _scene = null!;
    private GameSystemsFunc _systems = null!;

    private Game()
    {
        EnsureRunning();
    }

    public static bool Running { get; private set; }

    public static Platform Platform { get; } =
        Enum.GetValues<Platform>().FirstOrDefault(platform => platform.IsCurrent());

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

    public static Config Config => _game?._config ?? Config.Empty;

    internal static GameSystemsFunc Systems => GetGame()._systems;

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
        Actions.Push(action);
    }

    public static void OnQuit(Action action)
    {
        _quitAction += action;
    }

    public static Image Screenshot()
    {
        EnsureRunning();
        var width = Display.ScreenWidth;
        var height = Display.ScreenHeight;
        Graphics.Reset();
        Graphics.DrawCurrentBuffer();
        var data = Rlgl.ReadScreenPixels(width, height);
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

    public static void Launch(Config config, Scene scene)
    {
        EnsureNotRunning();
        Running = true;
        var game = GetGame();
        game._config = config;
        game._systems = config.Take<GameSystemsFunc>() ?? (() => []);
        game._scene = scene;
        UpdateActions();
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
        return _game ??= new Game();
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
        Display.Update();
        UpdateActions();
        UpdateFullscreen();
        Renderer.BeginDrawing();
        _scene.Update();
        Renderer.EndDrawing();
        Raylib.PollInputEvents();
    }

    private static void UpdateActions()
    {
        var length = Actions.Count;
        if (length == 0)
            return;
        var actions = new Action[length];
        var amount = Actions.TryPopRange(actions, 0, length);
        for (var i = amount - 1; i >= 0; i--)
            actions[i].Invoke();
    }

    private static void Dispose()
    {
        _quitAction?.Invoke();
        Environment.Exit(0);
    }

    private static void UpdateFullscreen()
    {
        if (Keyboard.IsKeyPressed(Input.Input.FullscreenKey))
            Display.ToggleFullscreen();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedFrame()
    {
        GetGame().Frame();
    }
}
