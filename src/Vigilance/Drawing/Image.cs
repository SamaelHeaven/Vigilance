using System.Runtime.InteropServices;
using Raylib_cs;
using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed unsafe class Image : IDisposable
{
    internal Raylib_cs.Image RImage;

    internal Image(in Raylib_cs.Image image)
    {
        RImage = image;
    }

    public Image(in ReadOnlySpan<char> fileType, in ReadOnlySpan<byte> bytes)
    {
        using var fileTypeBuffer = fileType.ToUtf8Ptr();
        fixed (byte* bytesBuffer = bytes)
        {
            RImage = Raylib.LoadImageFromMemory(fileTypeBuffer, bytesBuffer, bytes.Length);
        }
    }

    public int Width => RImage.Width;

    public int Height => RImage.Height;

    public Vector2 Size => new(Width, Height);

    public int PixelCount => Width * Height;

    public int DataSize => Raylib.GetPixelDataSize(Width, Height, RImage.Format);

    public bool IsValid => RImage.Data is not null;

    public PixelFormat Format => (PixelFormat)RImage.Format;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        RImage = default;
    }

    public WritableTexture ToTexture()
    {
        var logLevel = Log.SetLogLevel(Log.LogLevel.Max(LogLevel.Info));
        var image = Raylib.LoadTextureFromImage(RImage);
        Log.LogLevel = logLevel;
        return new WritableTexture(new Texture(image));
    }

    public WritableImage Copy()
    {
        return new WritableImage(new Image(Raylib.ImageCopy(RImage)));
    }

    public WritableImage<T> Copy<T>()
        where T : unmanaged, IPixel
    {
        return new WritableImage<T>(Copy());
    }

    public Color[] GetPixelColors()
    {
        var colors = Raylib.LoadImageColors(RImage);
        var pixels = new Color[PixelCount];
        for (var i = 0; i < pixels.Length; i++)
        {
            var color = colors[i];
            pixels[i] = new Color(color.R, color.G, color.B, color.A);
        }

        Raylib.UnloadImageColors(colors);
        return pixels;
    }

    public Color GetPixelColor(Vector2 position)
    {
        return GetPixelColor((int)position.X, (int)position.Y);
    }

    public Color GetPixelColor(int x, int y)
    {
        return new Color(Raylib.GetImageColor(RImage, x, y));
    }

    public bool Export(string path)
    {
        return Raylib.ExportImage(RImage, path);
    }

    public byte[] ExportToMemory(string fileType)
    {
        TryExportToMemory(fileType, out var bytes);
        return bytes;
    }

    public bool TryExportToMemory(string fileType, out byte[] bytes)
    {
        var size = 0;
        using var fileTypeBuffer = fileType.ToUtf8Ptr();
        var bytesBuffer = Raylib.ExportImageToMemory(RImage, fileTypeBuffer, &size);
        if (bytesBuffer is null)
        {
            bytes = [];
            return false;
        }

        bytes = new byte[size];
        Marshal.Copy((nint)bytesBuffer, bytes, 0, size);
        Raylib.MemFree(bytesBuffer);
        return true;
    }

    public static WritableImage<PixelR8G8B8A8> GradientLinear(
        int width,
        int height,
        int direction,
        Color start,
        Color end
    )
    {
        var image = Raylib.GenImageGradientLinear(width, height, direction, start.RColor, end.RColor);
        return new WritableImage<PixelR8G8B8A8>(new WritableImage(new Image(image)));
    }

    public static WritableImage<PixelR8G8B8A8> GradientRadial(
        int width,
        int height,
        float density,
        Color inner,
        Color outer
    )
    {
        var image = Raylib.GenImageGradientRadial(width, height, density, inner.RColor, outer.RColor);
        return new WritableImage<PixelR8G8B8A8>(new WritableImage(new Image(image)));
    }

    public static WritableImage<PixelR8G8B8A8> GradientSquare(
        int width,
        int height,
        float density,
        Color inner,
        Color outer
    )
    {
        var image = Raylib.GenImageGradientSquare(width, height, density, inner.RColor, outer.RColor);
        return new WritableImage<PixelR8G8B8A8>(new WritableImage(new Image(image)));
    }

    private void ReleaseUnmanagedResources()
    {
        Raylib.UnloadImage(RImage);
    }

    ~Image()
    {
        Game.Defer(ReleaseUnmanagedResources);
    }
}
