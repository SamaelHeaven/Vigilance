using System.ComponentModel;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public static class Renderer
{
    private static RenderTexture? _buffer;
    private static Vector2 _offset;
    private static Vector2 _scale;

    static Renderer()
    {
        Game.ThrowIfNotRunning();
        var mode = Display.RenderingMode;
        if (mode.Type == RenderingModeType.Buffer)
            _buffer = new RenderTexture(Display.Size, mode.Scale);
        Graphics = new Graphics(_buffer);
    }

    public static Graphics Graphics { get; }

    public static Vector2 Offset => _offset;

    public static Vector2 Scale => _scale;

    internal static void BeginDrawing()
    {
        var mode = Display.RenderingMode;
        if (
            mode.Type == RenderingModeType.Buffer
            && (_buffer is null || _buffer.ScaledSize != (Display.Size * mode.Scale).Floor())
        )
        {
            _buffer?.Dispose();
            _buffer = new RenderTexture(Display.Size, mode.Scale);
            Graphics.Buffer = _buffer;
        }
        else if (mode.Type != RenderingModeType.Buffer)
        {
            _buffer?.Dispose();
            _buffer = null;
            Graphics.Buffer = null;
        }

        Graphics.Reset();
        Raylib.ClearBackground(Display.Background.RColor);
        if (_buffer is not null)
            Graphics.ClearBackground(Display.Background);
        var screenWidth = (float)Display.ScreenWidth;
        var screenHeight = (float)Display.ScreenHeight;
        var width = Display.Width;
        var height = Display.Height;
        var scaleX = screenWidth / width;
        var scaleY = screenHeight / height;
        var minScale = scaleX.Min(scaleY);
        var maxScale = scaleX.Max(scaleY);
        var viewport = Display.Viewport;
        _scale = viewport switch
        {
            Viewport.Fit => new Vector2(minScale),
            Viewport.Stretch => new Vector2(scaleX, scaleY),
            Viewport.Crop => new Vector2(maxScale),
            Viewport.Native => Vector2.One,
            _ => throw new InvalidEnumArgumentException(
                $"{nameof(Game)}.{nameof(Display.Viewport)}",
                (int)viewport,
                typeof(Viewport)
            ),
        };
        _offset = (
            Display.Viewport switch
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
                Viewport.Native => Vector2.Zero,
                _ => throw new InvalidEnumArgumentException(
                    $"{nameof(Game)}.{nameof(Display.Viewport)}",
                    (int)viewport,
                    typeof(Viewport)
                ),
            }
        ).Ceil();
    }

    internal static void EndDrawing()
    {
        var screenWidth = Display.ScreenWidth;
        var screenHeight = Display.ScreenHeight;
        var width = Display.Width;
        var height = Display.Height;
        var scaleX = _scale.X;
        var scaleY = _scale.Y;
        var offsetX = (int)_offset.X;
        var offsetY = (int)_offset.Y;
        var background = Display.Background.RColor;
        var mode = Display.RenderingMode;
        Graphics.Reset();
        if (_buffer is null)
        {
            Raylib.DrawRectangle(0, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(screenWidth - offsetX, 0, offsetX, screenHeight, background);
            Raylib.DrawRectangle(0, 0, screenWidth, offsetY, background);
            Raylib.DrawRectangle(0, screenHeight - offsetY, screenWidth, offsetY, background);
        }
        else
        {
            var texture = _buffer.Texture.Texture2D;
            var source = new Raylib_cs.Rectangle(0, 0, texture.Width, -texture.Height);
            var dest = new Raylib_cs.Rectangle(offsetX, offsetY, width * scaleX, height * scaleY);
            Raylib.SetTextureFilter(texture, (TextureFilter)mode.Interpolation);
            Raylib.DrawTexturePro(texture, source, dest, Vector2.Zero, 0, Raylib_cs.Color.White);
        }

        Graphics.DrawCurrentBuffer();
        Raylib.SwapScreenBuffer();
    }
}
