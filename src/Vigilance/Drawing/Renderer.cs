using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Renderer
{
    private static Renderer? _renderer;
    private WritableTexture? _buffer;
    private readonly Graphics _graphics;
    private Vector2 _offset;
    private Vector2 _scale;

    private Renderer()
    {
        Game.EnsureRunning();
        var mode = Game.RenderingMode;
        if (mode.ModeType == RenderingModeType.Buffer)
            _buffer = new WritableTexture(Game.Size, mode.Scale);
        _graphics = new Graphics(_buffer);
    }

    public static Graphics Graphics => GetRenderer()._graphics;

    public static Vector2 Offset
    {
        get
        {
            var renderer = GetRenderer();
            return Game.RenderingMode.ModeType == RenderingModeType.Buffer ? Vector2.Zero : renderer._offset;
        }
    }

    public static Vector2 Scale
    {
        get
        {
            var renderer = GetRenderer();
            return Game.RenderingMode.ModeType == RenderingModeType.Buffer ? renderer._buffer!.Scale : renderer._scale;
        }
    }

    internal static void BeginDrawing()
    {
        Graphics.Reset();
        Raylib.ClearBackground(Game.Background.RColor);
        var renderer = GetRenderer();
        var screenWidth = (float)Game.ScreenWidth;
        var screenHeight = (float)Game.ScreenHeight;
        var width = Game.Width;
        var height = Game.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = MathF.Min(scaleX, scaleY);
        var maxScale = MathF.Max(scaleX, scaleY);
        renderer._scale = Game.Viewport switch
        {
            Viewport.Fit => new Vector2(minScale),
            Viewport.Stretch => new Vector2(scaleX, scaleY),
            Viewport.Crop => new Vector2(maxScale),
            _ => throw new ArgumentOutOfRangeException(),
        };
        renderer._offset = (
            Game.Viewport switch
            {
                Viewport.Fit => new Vector2(
                    (screenWidth - width * minScale) * 0.5f,
                    (screenHeight - height * minScale) * 0.5f
                ),
                Viewport.Stretch => Vector2.Zero,
                Viewport.Crop => new Vector2(
                    (screenWidth - width * maxScale) * 0.5f,
                    (screenHeight - height * maxScale) * 0.5f
                ),
                _ => throw new ArgumentOutOfRangeException(),
            }
        ).Ceil();
        if (!OperatingSystem.IsMacOS() || Game.RenderingMode.ModeType != RenderingModeType.Screen)
            return;
        if (Game.Fullscreen)
        {
            renderer._buffer ??= new WritableTexture(Game.Size);
            renderer._graphics.SetBuffer(renderer._buffer);
            return;
        }

        renderer._buffer = null;
        renderer._graphics.SetBuffer(renderer._buffer);
    }

    internal static void EndDrawing()
    {
        var renderer = GetRenderer();
        var screenWidth = Game.ScreenWidth;
        var screenHeight = Game.ScreenHeight;
        var width = Game.Width;
        var height = Game.Height;
        var scaleX = renderer._scale.X;
        var scaleY = renderer._scale.Y;
        var offsetX = (int)renderer._offset.X;
        var offsetY = (int)renderer._offset.Y;
        var background = Game.Background.RColor;
        var mode = Game.RenderingMode;
        Graphics.Reset();
        if (renderer._buffer is null)
        {
            Raylib.DrawRectangle(0, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(0, 0, screenWidth, offsetY, background);
            Raylib.DrawRectangle(0, screenHeight - offsetY, screenWidth, offsetY, background);
        }
        else
        {
            var texture = renderer._buffer.Texture.Texture2D;
            var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
            var dest = new Raylib_cs.Rectangle(offsetX, offsetY, width * scaleX, height * scaleY);
            Raylib.SetTextureFilter(texture, (TextureFilter)mode.Interpolation);
            Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
        }

        Graphics.DrawCurrentBuffer();
        Raylib.SwapScreenBuffer();
    }

    private static Renderer GetRenderer()
    {
        return _renderer ??= new Renderer();
    }
}
