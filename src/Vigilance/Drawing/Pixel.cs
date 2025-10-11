using System.Runtime.InteropServices;

namespace Vigilance.Drawing;

public interface IPixel
{
    static abstract PixelFormat Format { get; }

    Color ToColor();
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelGrayscale(byte R) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedGrayscale;

    public Color ToColor()
    {
        return new Color(R, R, R);
    }

    public static implicit operator Color(PixelGrayscale pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelGrayAlpha(byte R, byte A = 255) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedGrayAlpha;

    public Color ToColor()
    {
        return new Color(R, R, R, A);
    }

    public static implicit operator Color(PixelGrayAlpha pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR5G6B5(ushort Value) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR5G6B5;

    public Color ToColor()
    {
        return new Color(
            (byte)((Value >> 11) * 255 / 31),
            (byte)(((Value >> 5) & 0b0000000000111111) * 255 / 63),
            (byte)((Value & 0b0000000000011111) * 255 / 31)
        );
    }

    public static implicit operator Color(PixelR5G6B5 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct PixelR8G8B8(byte R, byte G, byte B) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR8G8B8;

    public Color ToColor()
    {
        return new Color(R, G, B);
    }

    public static implicit operator Color(PixelR8G8B8 pixel)
    {
        return pixel.ToColor();
    }

    public static implicit operator PixelR8G8B8(Color color)
    {
        return new PixelR8G8B8(color.R, color.G, color.B);
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR5G5B5A1(ushort Value) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR5G5B5A1;

    public Color ToColor()
    {
        return new Color(
            (byte)((Value >> 11) * 255 / 31),
            (byte)(((Value >> 6) & 0b0000000000011111) * 255 / 31),
            (byte)((Value & 0b0000000000011111) * 255 / 31),
            (Value & 0b0000000000000001) != 0 ? (byte)255 : (byte)0
        );
    }

    public static implicit operator Color(PixelR5G5B5A1 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR4G4B4A4(ushort Value) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR4G4B4A4;

    public Color ToColor()
    {
        return new Color(
            (byte)((Value >> 12) * 255 / 15),
            (byte)(((Value >> 8) & 0b0000000000001111) * 255 / 15),
            (byte)(((Value >> 4) & 0b0000000000001111) * 255 / 15),
            (byte)((Value & 0b0000000000001111) * 255 / 15)
        );
    }

    public static implicit operator Color(PixelR4G4B4A4 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR8G8B8A8(byte R, byte G, byte B, byte A = 255) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR8G8B8A8;

    public Color ToColor()
    {
        return new Color(R, G, B, A);
    }

    public static implicit operator Color(PixelR8G8B8A8 pixel)
    {
        return pixel.ToColor();
    }

    public static implicit operator PixelR8G8B8A8(Color color)
    {
        return new PixelR8G8B8A8(color.R, color.G, color.B, color.A);
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR32(float R) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR32;

    public Color ToColor()
    {
        return new Color((byte)(R * 255), (byte)(R * 255), (byte)(R * 255));
    }

    public static implicit operator Color(PixelR32 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct PixelR32G32B32(float R, float G, float B) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR32G32B32;

    public Color ToColor()
    {
        return new Color((byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
    }

    public static implicit operator Color(PixelR32G32B32 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR32G32B32A32(float R, float G, float B, float A = 1.0f) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR32G32B32A32;

    public Color ToColor()
    {
        return new Color((byte)(R * 255), (byte)(G * 255), (byte)(B * 255), (byte)(A * 255));
    }

    public static implicit operator Color(PixelR32G32B32A32 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR16(ushort R) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR16;

    public Color ToColor()
    {
        return new Color(
            (byte)((float)BitConverter.UInt16BitsToHalf(R) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(R) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(R) * 255)
        );
    }

    public static implicit operator Color(PixelR16 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct PixelR16G16B16(ushort R, ushort G, ushort B) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR16G16B16;

    public Color ToColor()
    {
        return new Color(
            (byte)((float)BitConverter.UInt16BitsToHalf(R) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(G) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(B) * 255)
        );
    }

    public static implicit operator Color(PixelR16G16B16 pixel)
    {
        return pixel.ToColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
public record struct PixelR16G16B16A16(ushort R, ushort G, ushort B, ushort A = 0x3C00) : IPixel
{
    public static PixelFormat Format => PixelFormat.UncompressedR16G16B16A16;

    public Color ToColor()
    {
        return new Color(
            (byte)((float)BitConverter.UInt16BitsToHalf(R) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(G) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(B) * 255),
            (byte)((float)BitConverter.UInt16BitsToHalf(A) * 255)
        );
    }

    public static implicit operator Color(PixelR16G16B16A16 pixel)
    {
        return pixel.ToColor();
    }
}
