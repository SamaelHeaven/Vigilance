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

    public Image Copy()
    {
        return new Image(Raylib.ImageCopy(RImage));
    }

    public unsafe Color[] GetPixels()
    {
        var colors = Raylib.LoadImageColors(RImage);
        var pixels = new Color[Width * Height];
        for (var i = 0; i < pixels.Length; i++)
        {
            var color = colors[i];
            pixels[i] = new Color(color.R, color.G, color.B, color.A);
        }

        Raylib.UnloadImageColors(colors);
        return pixels;
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

    public void ReplaceColor(Color from, Color to)
    {
        Raylib.ImageColorReplace(ref RImage, from.RColor, to.RColor);
    }

    public void Crop(float x, float y, float width, float height)
    {
        Raylib.ImageCrop(ref RImage, new Raylib_cs.Rectangle(x, y, width, height));
    }

    public void Crop(Vector2 position, Vector2 size)
    {
        Raylib.ImageCrop(ref RImage, new Raylib_cs.Rectangle(position, size));
    }

    public void Crop(Box box)
    {
        Raylib.ImageCrop(ref RImage, new Raylib_cs.Rectangle(box.X, box.Y, box.Width, box.Height));
    }

    public void FlipHorizontally()
    {
        Raylib.ImageFlipHorizontal(ref RImage);
    }

    public void FlipVertically()
    {
        Raylib.ImageFlipVertical(ref RImage);
    }

    public void KernelConvolution(float[] kernel)
    {
        Raylib.ImageKernelConvolution(ref RImage, kernel);
    }

    public void Blur(int blur)
    {
        if (blur <= 0)
            return;
        Raylib.ImageBlurGaussian(ref RImage, blur);
    }

    public void Tint(Color color)
    {
        Raylib.ImageColorTint(ref RImage, color.RColor);
    }

    public void Invert()
    {
        Raylib.ImageColorInvert(ref RImage);
    }

    public void Grayscale()
    {
        Raylib.ImageColorGrayscale(ref RImage);
    }

    public void Contrast(float contrast)
    {
        Raylib.ImageColorContrast(ref RImage, contrast);
    }

    public void Brightness(int brightness)
    {
        Raylib.ImageColorBrightness(ref RImage, brightness);
    }

    public void Rotate(int angle)
    {
        Raylib.ImageRotate(ref RImage, angle);
    }

    public void Export(string path)
    {
        Raylib.ExportImage(RImage, FileSystem.FormatPath(path));
    }

    ~Image()
    {
        Game.RunLater(() => Raylib.UnloadImage(RImage));
    }
}
