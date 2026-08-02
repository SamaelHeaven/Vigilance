using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;
using Music = Vigilance.Audio.Music;
using Sound = Vigilance.Audio.Sound;

namespace Vigilance.Core;

public static unsafe class Game
{
    private static readonly ConcurrentStack<Job> _jobs = [];
    private static readonly ConcurrentStack<Job> _nextFrameJobs = [];
    private static bool _exit;
    private static Scene _scene = null!;
    private static int _threadId;

    public static bool IsGameThread => Environment.CurrentManagedThreadId == _threadId;

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
            RunNextFrame(() =>
            {
                if (_scene == value)
                    return;
                var oldScene = _scene;
                oldScene.Stop();
                _scene = value;
                _scene.TransitionTo(oldScene);
                // ReSharper disable once RedundantAssignment
                oldScene = null;
                ReclaimAbandonedResources();
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
            throw new InvalidOperationException($"{nameof(Game)} is not running.");
    }

    public static void ThrowIfRunning()
    {
        if (Running)
            throw new InvalidOperationException($"{nameof(Game)} is already running.");
    }

    public static void RunLater(Action action)
    {
        _jobs.Push(Job.From(action));
    }

    public static void RunLater<T>(in T context, Action<T> action)
    {
        _jobs.Push(Job.From(context, action));
    }

    public static void RunNextFrame(Action action)
    {
        _nextFrameJobs.Push(Job.From(action));
    }

    public static void RunNextFrame<T>(in T context, Action<T> action)
    {
        _nextFrameJobs.Push(Job.From(context, action));
    }

    public static void Launch(Config config, Scene scene)
    {
        ThrowIfRunning();
        if (!IsGameThread)
            throw new InvalidOperationException(
                $"{nameof(Game)}.{nameof(Launch)} must be called from the same thread as the engine module initializer."
            );
        Running = true;
        Config = config;
        _scene = scene;
        InvokeJobs();
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

    internal static void Initialize()
    {
        _threadId = Environment.CurrentManagedThreadId;
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
        InvokeJobs();
        InvokeJobs(_nextFrameJobs);
        Renderer.BeginDrawing();
        _scene.Update();
        Renderer.EndDrawing();
        Raylib.PollInputEvents();
    }

    private static void ReclaimAbandonedResources()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        InvokeJobs();
        GC.Collect();
    }

    internal static void InvokeJobs()
    {
        InvokeJobs(_jobs);
    }

    private static void InvokeJobs(ConcurrentStack<Job> jobs)
    {
        var length = jobs.Count;
        if (length == 0)
            return;
        var array = ArrayPool<Job>.Shared.Rent(length);
        try
        {
            var amount = jobs.TryPopRange(array, 0, length);
            for (var i = amount - 1; i >= 0; i--)
                try
                {
                    array[i].Invoke();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
        }
        finally
        {
            ArrayPool<Job>.Shared.Return(array, true);
        }
    }

    private static void Dispose()
    {
        Hooks.OnExit?.SafeInvoke();
        Audio.Audio.Dispose();
        Display.Dispose();
    }

    private static void UpdateExit()
    {
        if (Input.Input.ExitInputs.IsPressed)
            Exit();
    }

    private static void UpdateFullscreen()
    {
        if (Input.Input.FullscreenInputs.IsPressed)
            Display.ToggleFullscreen();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void UnmanagedFrame()
    {
        Frame();
    }
}
