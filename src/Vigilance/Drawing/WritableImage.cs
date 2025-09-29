using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public readonly struct WritableImage
{
    internal readonly Image Image;

    internal WritableImage(Image image)
    {
        Image = image;
    }

    public WritableImage(string fileType, IEnumerable<byte> bytes)
    {
        Image = new Image(fileType, bytes);
    }

    public WritableImage(Vector2 size, Color? color = null)
        : this((int)size.X, (int)size.Y, color) { }

    public WritableImage(int width, int height, Color? color = null)
    {
        Image = new Image(Raylib.GenImageColor(width, height, (color ?? Color.Transparent).RColor));
    }

    public int Width => Image.Width;

    public int Height => Image.Height;

    public Vector2 Size => Image.Size;

    public int PixelCount => Image.PixelCount;

    public int DataSize => Image.DataSize;

    public bool Valid => Image.Valid;

    public PixelFormat Format => Image.Format;

    public WritableTexture ToTexture()
    {
        return Image.ToTexture();
    }

    public WritableImage Copy()
    {
        return Image.Copy();
    }

    public WritableImage<T> Copy<T>()
        where T : unmanaged, IPixel
    {
        return Image.Copy<T>();
    }

    public Color[] GetPixelColors()
    {
        return Image.GetPixelColors();
    }

    public Color GetPixelColor(Vector2 position)
    {
        return Image.GetPixelColor(position);
    }

    public Color GetPixelColor(int x, int y)
    {
        return Image.GetPixelColor(x, y);
    }

    public void Export(string path)
    {
        Image.Export(path);
    }

    public byte[] ExportToMemory(string fileType)
    {
        return Image.ExportToMemory(fileType);
    }

    public static implicit operator Image(WritableImage image)
    {
        return image.Image;
    }

    public void SetPixelColor(Vector2 position, Color color)
    {
        SetPixelColor((int)position.X, (int)position.Y, color);
    }

    public void SetPixelColor(int x, int y, Color color)
    {
        Raylib.ImageDrawPixel(ref Image.RImage, x, y, color.RColor);
    }

    public void ReplaceColor(Color from, Color to)
    {
        Raylib.ImageColorReplace(ref Image.RImage, from.RColor, to.RColor);
    }

    public void Crop(float x, float y, float width, float height)
    {
        Crop(new Vector2(x, y), new Vector2(width, height));
    }

    public void Crop(Box box)
    {
        Crop(box.Position, box.Size);
    }

    public void Crop(Vector2 position, Vector2 size)
    {
        Raylib.ImageCrop(ref Image.RImage, new Raylib_cs.BleedingEdge.Rectangle(position, size));
    }

    public void Resize(float width, float height, Interpolation? interpolation = null)
    {
        Resize(new Vector2(width, height), interpolation);
    }

    public void Resize(Vector2 size, Interpolation? interpolation = null)
    {
        switch (interpolation ?? Interpolation.Nearest)
        {
            case Interpolation.Bilinear:
                Raylib.ImageResize(ref Image.RImage, (int)size.X, (int)size.Y);
                break;
            default:
                Raylib.ImageResizeNN(ref Image.RImage, (int)size.X, (int)size.Y);
                break;
        }
    }

    public void FlipHorizontally()
    {
        Raylib.ImageFlipHorizontal(ref Image.RImage);
    }

    public void FlipVertically()
    {
        Raylib.ImageFlipVertical(ref Image.RImage);
    }

    public void KernelConvolution(IEnumerable<float> kernel)
    {
        KernelConvolutionSpan(kernel.AsSpan());
    }

    public void KernelConvolutionSpan(ReadOnlySpan<float> kernel)
    {
        Raylib.ImageKernelConvolution(ref Image.RImage, kernel, kernel.Length);
    }

    public void Blur(int blur)
    {
        if (blur > 0)
            Raylib.ImageBlurGaussian(ref Image.RImage, blur);
    }

    public void Tint(Color color)
    {
        Raylib.ImageColorTint(ref Image.RImage, color.RColor);
    }

    public void Invert()
    {
        Raylib.ImageColorInvert(ref Image.RImage);
    }

    public void Grayscale()
    {
        Raylib.ImageColorGrayscale(ref Image.RImage);
    }

    public void Contrast(float contrast)
    {
        Raylib.ImageColorContrast(ref Image.RImage, contrast);
    }

    public void Brightness(int brightness)
    {
        Raylib.ImageColorBrightness(ref Image.RImage, brightness);
    }

    public void Rotate(int angle)
    {
        Raylib.ImageRotate(ref Image.RImage, angle);
    }
}

public readonly unsafe struct WritableImage<T>
    : IStructEnumerable<WritableImage<T>.PixelEnumerator, T>,
        IReadOnlyList<T>,
        IReadOnlySpan<T>
    where T : unmanaged, IPixel
{
    private readonly WritableImage _image;

    internal WritableImage(WritableImage image)
    {
        if (image.Format != T.Format)
            Raylib.ImageFormat(ref image.Image.RImage, (Raylib_cs.BleedingEdge.PixelFormat)T.Format);
        _image = image;
    }

    public WritableImage(string fileType, IEnumerable<byte> bytes)
        : this(new WritableImage(fileType, bytes)) { }

    public WritableImage(Vector2 size)
        : this((int)size.X, (int)size.Y) { }

    public WritableImage(int width, int height)
    {
        var size = (uint)(width * height * sizeof(T));
        var data = Raylib.MemAlloc(size);
        _image = new WritableImage(
            new Image(
                new Raylib_cs.BleedingEdge.Image
                {
                    Data = data,
                    Width = width,
                    Height = height,
                    Format = (Raylib_cs.BleedingEdge.PixelFormat)T.Format,
                    Mipmaps = 1,
                }
            )
        );
    }

    public WritableImage(Vector2 size, Color color)
        : this((int)size.X, (int)size.Y, color) { }

    public WritableImage(int width, int height, Color color)
        : this(new WritableImage(width, height, color)) { }

    public int Width => _image.Width;

    public int Height => _image.Height;

    public Vector2 Size => _image.Size;

    public int PixelCount => _image.PixelCount;

    public int DataSize => _image.DataSize;

    public bool Valid => _image.Valid;

    public PixelFormat Format => _image.Format;

    public WritableTexture ToTexture()
    {
        return _image.ToTexture();
    }

    public WritableImage Copy()
    {
        return _image.Copy();
    }

    public WritableImage<TPixel> Copy<TPixel>()
        where TPixel : unmanaged, IPixel
    {
        return _image.Copy<TPixel>();
    }

    public Color[] GetPixelColors()
    {
        return _image.GetPixelColors();
    }

    public Color GetPixelColor(Vector2 position)
    {
        return _image.GetPixelColor(position);
    }

    public Color GetPixelColor(int x, int y)
    {
        return _image.GetPixelColor(x, y);
    }

    public void Export(string path)
    {
        _image.Export(path);
    }

    public byte[] ExportToMemory(string fileType)
    {
        return _image.ExportToMemory(fileType);
    }

    public struct PixelEnumerator : IStructEnumerator<T>
    {
        private readonly WritableImage<T> _image;
        private int _index;

        internal PixelEnumerator(WritableImage<T> image)
        {
            _image = image;
            Reset();
        }

        public bool MoveNext()
        {
            return ++_index < _image.PixelCount;
        }

        public void Reset()
        {
            _index = -1;
        }

        public T Current => _image[_index];

        public void Dispose() { }
    }

    public Span<T> AsSpan()
    {
        return new Span<T>((T*)_image.Image.RImage.Data, PixelCount);
    }

    ReadOnlySpan<T> IReadOnlySpan<T>.AsSpan()
    {
        return AsSpan();
    }

    public PixelEnumerator GetEnumerator()
    {
        return new PixelEnumerator(this);
    }

    ValueEnumerable<StructEnumerator<PixelEnumerator, T>, T> IStructEnumerable<PixelEnumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<PixelEnumerator, T>(GetEnumerator());
    }

    public ValueEnumerable<FromSpan<T>, T> AsValueEnumerable()
    {
        return AsSpan().AsValueEnumerable();
    }

    public int Count => PixelCount;

    public T this[int index]
    {
        get => AsSpan()[index];
        set => AsSpan()[index] = value;
    }

    public T this[Vector2 position]
    {
        get => GetPixel(position);
        set => SetPixel(position, value);
    }

    public T this[int x, int y]
    {
        get => GetPixel(x, y);
        set => SetPixel(x, y, value);
    }

    public static implicit operator WritableImage(WritableImage<T> image)
    {
        return image._image;
    }

    public static implicit operator Image(WritableImage<T> image)
    {
        return image._image.Image;
    }

    public void SetPixelColor(Vector2 position, Color color)
    {
        _image.SetPixelColor(position, color);
    }

    public void SetPixelColor(int x, int y, Color color)
    {
        _image.SetPixelColor(x, y, color);
    }

    public void ReplaceColor(Color from, Color to)
    {
        _image.ReplaceColor(from, to);
    }

    public void Crop(float x, float y, float width, float height)
    {
        _image.Crop(x, y, width, height);
    }

    public void Crop(Box box)
    {
        _image.Crop(box);
    }

    public void Crop(Vector2 position, Vector2 size)
    {
        _image.Crop(position, size);
    }

    public void Resize(float width, float height, Interpolation? interpolation = null)
    {
        _image.Resize(width, height, interpolation);
    }

    public void Resize(Vector2 size, Interpolation? interpolation = null)
    {
        _image.Resize(size, interpolation);
    }

    public void FlipHorizontally()
    {
        _image.FlipHorizontally();
    }

    public void FlipVertically()
    {
        _image.FlipVertically();
    }

    public void KernelConvolution(IEnumerable<float> kernel)
    {
        _image.KernelConvolution(kernel);
    }

    public void KernelConvolutionSpan(ReadOnlySpan<float> kernel)
    {
        _image.KernelConvolutionSpan(kernel);
    }

    public void Blur(int blur)
    {
        _image.Blur(blur);
    }

    public void Tint(Color color)
    {
        _image.Tint(color);
    }

    public void Invert()
    {
        _image.Invert();
    }

    public void Grayscale()
    {
        _image.Grayscale();
    }

    public void Contrast(float contrast)
    {
        _image.Contrast(contrast);
    }

    public void Brightness(int brightness)
    {
        _image.Brightness(brightness);
    }

    public void Rotate(int angle)
    {
        _image.Rotate(angle);
    }

    public T GetPixel(Vector2 position)
    {
        return GetPixel((int)position.X, (int)position.Y);
    }

    public T GetPixel(int x, int y)
    {
        return this[y * _image.Image.RImage.Width + x];
    }

    public T GetPixel(int index)
    {
        return this[index];
    }

    public void SetPixel(Vector2 position, T pixel)
    {
        SetPixel((int)position.X, (int)position.Y, pixel);
    }

    public void SetPixel(int x, int y, T pixel)
    {
        this[y * _image.Image.RImage.Width + x] = pixel;
    }

    public void SetPixel(int index, T pixel)
    {
        this[index] = pixel;
    }
}
