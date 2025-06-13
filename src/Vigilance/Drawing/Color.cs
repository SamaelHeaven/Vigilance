using System.Runtime.InteropServices;
using Raylib_cs;
using Exception = System.Exception;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public struct Color
{
    public static Color White { get; } = new(255, 255, 255);
    public static Color Black { get; } = new(0, 0, 0);
    public static Color Transparent { get; } = new(0, 0, 0, 0);
    public static Color LightGray { get; } = new(200, 200, 200);
    public static Color Gray { get; } = new(130, 130, 130);
    public static Color DarkGray { get; } = new(80, 80, 80);
    public static Color Yellow { get; } = new(253, 249, 0);
    public static Color Gold { get; } = new(255, 203, 0);
    public static Color Orange { get; } = new(255, 161, 0);
    public static Color Pink { get; } = new(255, 109, 194);
    public static Color Red { get; } = new(230, 41, 55);
    public static Color Maroon { get; } = new(190, 33, 55);
    public static Color Green { get; } = new(0, 228, 48);
    public static Color Lime { get; } = new(0, 158, 47);
    public static Color DarkGreen { get; } = new(0, 117, 44);
    public static Color SkyBlue { get; } = new(102, 191, 255);
    public static Color Blue { get; } = new(0, 121, 241);
    public static Color DarkBlue { get; } = new(0, 82, 172);
    public static Color Purple { get; } = new(200, 122, 255);
    public static Color Violet { get; } = new(135, 60, 190);
    public static Color DarkPurple { get; } = new(112, 31, 126);
    public static Color Beige { get; } = new(211, 176, 131);
    public static Color Brown { get; } = new(127, 106, 79);
    public static Color DarkBrown { get; } = new(76, 63, 47);

    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    internal Color(Raylib_cs.Color color)
        : this(color.R, color.G, color.B, color.A) { }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(uint hex)
    {
        var color = Raylib.GetColor(hex);
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;
    }

    public Color(string hex)
    {
        try
        {
            if (hex.StartsWith('#'))
                hex = hex[1..];
            if (hex.Length != 6 && hex.Length != 8)
                throw new Exception();
            R = Convert.ToByte(hex[..2], 16);
            G = Convert.ToByte(hex[2..4], 16);
            B = Convert.ToByte(hex[4..6], 16);
            A = hex.Length == 8 ? Convert.ToByte(hex[6..8], 16) : (byte)255;
        }
        catch (Exception)
        {
            throw new ArgumentException($"Invalid hexadecimal color code: '{hex}'.");
        }
    }

    public static implicit operator Color(string hex)
    {
        return new Color(hex);
    }

    public static implicit operator (byte R, byte G, byte B)(Color color)
    {
        return (color.G, color.B, color.B);
    }

    public static implicit operator Color((byte R, byte G, byte B) rgb)
    {
        return new Color(rgb.R, rgb.G, rgb.B);
    }

    public static implicit operator (byte R, byte G, byte B, byte A)(Color color)
    {
        return (color.G, color.B, color.B, color.A);
    }

    public static implicit operator Color((byte R, byte G, byte B, byte A) rgba)
    {
        return new Color(rgba.R, rgba.G, rgba.B, rgba.A);
    }

    public void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }

    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    internal Raylib_cs.Color RColor => new(R, G, B, A);

    public override string ToString()
    {
        return $"[{nameof(R)}={R}, {nameof(G)}={G}, {nameof(B)}={B}, {nameof(A)}={A}]";
    }

    public string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public int ToInt()
    {
        return Raylib.ColorToInt(RColor);
    }

    public override bool Equals(object? obj)
    {
        return obj is Color c && c.R == R && c.G == G && c.B == B && c.A == A;
    }

    public static bool operator ==(Color a, Color b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Color a, Color b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public Color Fade(float alpha)
    {
        return new Color(Raylib.ColorAlpha(RColor, alpha));
    }

    public Color Tint(Color color)
    {
        return new Color(Raylib.ColorTint(RColor, color.RColor));
    }

    public Color Brightness(float factor)
    {
        return new Color(Raylib.ColorBrightness(RColor, factor));
    }

    public Color Contrast(float factor)
    {
        return new Color(Raylib.ColorContrast(RColor, factor));
    }

    public Color Alpha(float alpha)
    {
        return new Color(Raylib.ColorAlpha(RColor, alpha));
    }

    public Color AlphaBlend(Color src, Color tint)
    {
        return new Color(Raylib.ColorAlphaBlend(RColor, src.RColor, tint.RColor));
    }

    public Color Lerp(Color color, float factor)
    {
        return new Color(Raylib.ColorLerp(RColor, color.RColor, factor));
    }
}
