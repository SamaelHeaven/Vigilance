using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Image
{
    internal Raylib_cs.BleedingEdge.Image RImage;

    internal Image(Raylib_cs.BleedingEdge.Image image)
    {
        Game.EnsureRunning();
        RImage = image;
    }

    public Image(string fileType, IEnumerable<byte> bytes)
    {
        Game.EnsureRunning();
        using var fileTypeBuffer = fileType.ToUtf8Buffer();
        var span = bytes.AsSpan();
        fixed (byte* bytesBuffer = span)
        {
            RImage = Raylib.LoadImageFromMemory(fileTypeBuffer.AsPointer(), bytesBuffer, span.Length);
        }
    }

    public Image(Vector2 size, Color? color = null)
        : this(size.X, size.Y, color) { }

    public Image(float width, float height, Color? color = null)
    {
        Game.EnsureRunning();
        RImage = Raylib.GenImageColor((int)width, (int)height, (color ?? Color.Transparent).RColor);
    }

    public int Width => RImage.Width;

    public int Height => RImage.Height;

    public Vector2 Size => new(Width, Height);

    public bool IsValid => RImage.Data is not null;

    public ImageFormat Format
    {
        get => (ImageFormat)RImage.Format;
        set => Raylib.ImageFormat(ref RImage, (PixelFormat)value);
    }

    public static Image GradientLinear(int width, int height, int direction, Color start, Color end)
    {
        var image = Raylib.GenImageGradientLinear(width, height, direction, start.RColor, end.RColor);
        return new Image(image);
    }

    public static Image GradientRadial(int width, int height, float density, Color inner, Color outer)
    {
        var image = Raylib.GenImageGradientRadial(width, height, density, inner.RColor, outer.RColor);
        return new Image(image);
    }

    public static Image GradientSquare(int width, int height, float density, Color inner, Color outer)
    {
        var image = Raylib.GenImageGradientSquare(width, height, density, inner.RColor, outer.RColor);
        return new Image(image);
    }

    public Texture ToTexture()
    {
        return new Texture(Raylib.LoadTextureFromImage(RImage));
    }

    public Image Copy()
    {
        return new Image(Raylib.ImageCopy(RImage));
    }

    public Color[] GetPixels()
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

    public Image SetPixel(Vector2 position, Color color)
    {
        SetPixel((int)position.X, (int)position.Y, color);
        return this;
    }

    public Image SetPixel(int x, int y, Color color)
    {
        Raylib.ImageDrawPixel(ref RImage, x, y, color.RColor);
        return this;
    }

    public Image ReplaceColor(Color from, Color to)
    {
        Raylib.ImageColorReplace(ref RImage, from.RColor, to.RColor);
        return this;
    }

    public Image Crop(float x, float y, float width, float height)
    {
        return Crop(new Vector2(x, y), new Vector2(width, height));
    }

    public Image Crop(Box box)
    {
        return Crop(box.Position, box.Size);
    }

    public Image Crop(Vector2 position, Vector2 size)
    {
        Raylib.ImageCrop(ref RImage, new Raylib_cs.BleedingEdge.Rectangle(position, size));
        return this;
    }

    public Image Resize(float width, float height, Interpolation? interpolation = null)
    {
        return Resize(new Vector2(width, height), interpolation);
    }

    public Image Resize(Vector2 size, Interpolation? interpolation = null)
    {
        switch (interpolation ?? Interpolation.Nearest)
        {
            case Interpolation.Nearest:
                Raylib.ImageResizeNN(ref RImage, (int)size.X, (int)size.Y);
                break;
            default:
                Raylib.ImageResize(ref RImage, (int)size.X, (int)size.Y);
                break;
        }

        return this;
    }

    public Image FlipHorizontally()
    {
        Raylib.ImageFlipHorizontal(ref RImage);
        return this;
    }

    public Image FlipVertically()
    {
        Raylib.ImageFlipVertical(ref RImage);
        return this;
    }

    public Image KernelConvolution(float[] kernel)
    {
        var span = kernel.AsSpan();
        Raylib.ImageKernelConvolution(ref RImage, span, kernel.Length);
        return this;
    }

    public Image Blur(int blur)
    {
        if (blur > 0)
            Raylib.ImageBlurGaussian(ref RImage, blur);
        return this;
    }

    public Image Tint(Color color)
    {
        Raylib.ImageColorTint(ref RImage, color.RColor);
        return this;
    }

    public Image Invert()
    {
        Raylib.ImageColorInvert(ref RImage);
        return this;
    }

    public Image Grayscale()
    {
        Raylib.ImageColorGrayscale(ref RImage);
        return this;
    }

    public Image Contrast(float contrast)
    {
        Raylib.ImageColorContrast(ref RImage, contrast);
        return this;
    }

    public Image Brightness(int brightness)
    {
        Raylib.ImageColorBrightness(ref RImage, brightness);
        return this;
    }

    public Image Rotate(int angle)
    {
        Raylib.ImageRotate(ref RImage, angle);
        return this;
    }

    public void Export(string path)
    {
        Raylib.ExportImage(RImage, FileSystem.FormatPath(path));
    }

    ~Image()
    {
        Game.Defer(() => Raylib.UnloadImage(RImage));
    }
}
