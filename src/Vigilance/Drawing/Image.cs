using Raylib_cs;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Image
{
    internal Raylib_cs.Image RImage;

    internal Image(Raylib_cs.Image image)
    {
        Game.EnsureRunning();
        RImage = image;
    }

    public Image(string fileType, byte[] bytes)
    {
        Game.EnsureRunning();
        RImage = Raylib.LoadImageFromMemory(fileType, bytes);
    }

    public Image(int width, int height, Color? color = null)
    {
        Game.EnsureRunning();
        RImage = Raylib.GenImageColor(width, height, (color ?? Color.Transparent).RColor);
    }

    public int Width => RImage.Width;

    public int Height => RImage.Height;

    public Vector2 Size => new(Width, Height);

    public Texture ToTexture()
    {
        return new Texture(Raylib.LoadTextureFromImage(RImage));
    }

    public Color GetPixel(Vector2 position)
    {
        return GetPixel((int)position.X, (int)position.Y);
    }

    public Color GetPixel(int x, int y)
    {
        return new Color(Raylib.GetImageColor(RImage, x, y));
    }

    public void SetPixel(Vector2 position, Color color)
    {
        SetPixel((int)position.X, (int)position.Y, color);
    }

    public void SetPixel(int x, int y, Color color)
    {
        Raylib.ImageDrawPixel(ref RImage, x, y, color.RColor);
    }

    ~Image()
    {
        Game.RunLater(() => Raylib.UnloadImage(RImage));
    }
}
