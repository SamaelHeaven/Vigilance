using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;
using Vigilance.Drawing;
using Vigilance.Input;
using Vigilance.Logging;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public static unsafe class Game
{
    private static readonly ConcurrentStack<Action> _actions = [];
    private static bool _exit;
    private static Scene _scene = null!;

    public static Scene Scene
    {
        get
        {
            ThrowIfNotRunning();
            return _scene;
        }
        set
        {
            ThrowIfNotRunning();
            if (_scene == value)
                return;
            Defer(() =>
            {
                var oldScene = _scene;
                _scene.Stop();
                _scene = value;
                _scene.TransitionTo(oldScene);
            });
        }
    }

    public static bool Running { get; private set; }

    public static Config Config { get; private set; } = Config.Empty;

    public static void OpenUrl(string url)
    {
        ThrowIfNotRunning();
        Raylib.OpenURL(url);
    }

    public static void ThrowIfNotRunning()
    {
        if (!Running)
            throw new InvalidOperationException("Game is not running.");
    }

    public static void ThrowIfRunning()
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
        ThrowIfRunning();
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
            Log.Fatal(e);
        }
    }

    public static void Exit()
    {
        ThrowIfNotRunning();
        _exit = true;
    }

    private static void Loop()
    {
        if (Platform.Web.IsCurrent)
        {
            Emscripten.SetMainLoop(&UnmanagedFrame, 0, 1);
            return;
        }

        while (!Raylib.WindowShouldClose() && !_exit)
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
        RenderTexturePool.Update();
        UpdateExit();
        UpdateFullscreen();
        UpdateActions();
        Renderer.BeginDrawing();
        try
        {
            _scene.Update();
        }
        catch (Exception e)
        {
            var rethrow = true;
            Hooks.OnException?.Invoke(e, out rethrow);
            if (rethrow)
                throw;
        }

        Renderer.EndDrawing();
        Raylib.PollInputEvents();
    }

    private static void UpdateActions()
    {
        var length = _actions.Count;
        if (length == 0)
            return;
        var actions = ArrayPool<Action>.Shared.Rent(length);
        try
        {
            var amount = _actions.TryPopRange(actions, 0, length);
            for (var i = amount - 1; i >= 0; i--)
                try
                {
                    actions[i].Invoke();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
        }
        finally
        {
            ArrayPool<Action>.Shared.Return(actions);
        }
    }

    private static void Dispose()
    {
        Hooks.OnExit?.Invoke();
        Audio.Audio.Dispose();
        Display.Dispose();
    }

    private static void UpdateExit()
    {
        if (Input.Input.ExitButton?.IsPressed ?? false)
            Exit();
    }

    private static void UpdateFullscreen()
    {
        if (Input.Input.FullscreenButton?.IsPressed ?? false)
            Display.ToggleFullscreen();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedFrame()
    {
        Frame();
    }
}
