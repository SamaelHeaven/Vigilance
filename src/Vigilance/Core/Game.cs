using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public static unsafe class Game
{
    private static readonly ConcurrentStack<Action> _actions = [];
    private static bool _quit;
    private static Scene _scene = null!;

    public static Scene Scene
    {
        get
        {
            EnsureRunning();
            return _scene;
        }
        set
        {
            EnsureRunning();
            if (_scene == value)
                return;
            Defer(() =>
            {
                _scene.Stop();
                _scene = value;
            });
        }
    }

    public static bool Running { get; private set; }

    public static Config Config { get; private set; } = Config.Empty;

    public static event Action? OnQuit;

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
        _actions.Push(action);
    }

    public static void Launch(Config config, Scene scene)
    {
        EnsureNotRunning();
        Running = true;
        Config = config;
        _scene = scene;
        UpdateActions();
        try
        {
            Loop();
        }
        catch (Exception e)
        {
            Logger.Fatal(e);
        }
    }

    public static void Quit()
    {
        EnsureRunning();
        _quit = true;
    }

    private static void Loop()
    {
        if (Platform.Web.IsCurrent)
        {
            Emscripten.SetMainLoop(&UnmanagedFrame, 0, 1);
            return;
        }

        while (!Raylib.WindowShouldClose() && !_quit)
            Frame();
        Dispose();
    }

    private static void Frame()
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
        var length = _actions.Count;
        if (length == 0)
            return;
        var actions = new Action[length];
        var amount = _actions.TryPopRange(actions, 0, length);
        for (var i = amount - 1; i >= 0; i--)
            actions[i].Invoke();
    }

    private static void Dispose()
    {
        OnQuit?.Invoke();
        Audio.Audio.Dispose();
        Display.Dispose();
    }

    private static void UpdateFullscreen()
    {
        if (Keyboard.IsKeyPressed(Input.Input.FullscreenKey))
            Display.ToggleFullscreen();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedFrame()
    {
        Frame();
    }
}
