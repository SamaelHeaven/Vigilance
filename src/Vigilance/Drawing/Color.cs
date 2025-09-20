using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Exception = System.Exception;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct Color
{
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Transparent => new(0, 0, 0, 0);
    public static Color LightGray => new(200, 200, 200);
    public static Color Gray => new(130, 130, 130);
    public static Color DarkGray => new(80, 80, 80);
    public static Color Yellow => new(253, 249, 0);
    public static Color Gold => new(255, 203, 0);
    public static Color Orange => new(255, 161, 0);
    public static Color Pink => new(255, 109, 194);
    public static Color Red => new(230, 41, 55);
    public static Color Maroon => new(190, 33, 55);
    public static Color Green => new(0, 228, 48);
    public static Color Lime => new(0, 158, 47);
    public static Color DarkGreen => new(0, 117, 44);
    public static Color SkyBlue => new(102, 191, 255);
    public static Color Blue => new(0, 121, 241);
    public static Color DarkBlue => new(0, 82, 172);
    public static Color Purple => new(200, 122, 255);
    public static Color Violet => new(135, 60, 190);
    public static Color DarkPurple => new(112, 31, 126);
    public static Color Beige => new(211, 176, 131);
    public static Color Brown => new(127, 106, 79);
    public static Color DarkBrown => new(76, 63, 47);

    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    internal Color(Raylib_cs.BleedingEdge.Color color)
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

    internal Raylib_cs.BleedingEdge.Color RColor => new(R, G, B, A);

    public override string ToString()
    {
        return $"[R={R}, G={G}, B={B}, A={A}]";
    }

    public string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public int ToInt()
    {
        return Raylib.ColorToInt(RColor);
    }

    public Color Blend(Color color)
    {
        return new Color(
            (byte)((R + color.R) / 2),
            (byte)((G + color.G) / 2),
            (byte)((B + color.B) / 2),
            (byte)((A + color.A) / 2)
        );
    }

    public Color Fade(float alpha)
    {
        var result = this;
        alpha = alpha switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => alpha,
        };
        result.A = (byte)(255 * alpha);
        return result;
    }

    public Color Tint(Color color)
    {
        var result = this;
        result.R = (byte)(R * color.R / 255);
        result.G = (byte)(G * color.G / 255);
        result.B = (byte)(B * color.B / 255);
        result.A = (byte)(A * color.A / 255);
        return result;
    }

    public Color Brightness(float factor)
    {
        var result = this;
        factor = factor switch
        {
            > 1.0f => 1.0f,
            < -1.0f => -1.0f,
            _ => factor,
        };
        var red = R;
        var green = G;
        var blue = B;
        if (factor < 0.0f)
        {
            factor = 1.0f + factor;
            red = (byte)(red * factor);
            green = (byte)(green * factor);
            blue = (byte)(blue * factor);
        }
        else
        {
            red = (byte)((255 - red) * factor + red);
            green = (byte)((255 - green) * factor + green);
            blue = (byte)((255 - blue) * factor + blue);
        }

        result.R = red;
        result.G = green;
        result.B = blue;
        return result;
    }

    public Color Contrast(float factor)
    {
        var result = this;
        factor = factor switch
        {
            < 0.0f => -1.0f,
            > 1.0f => 1.0f,
            _ => factor,
        };
        factor = 1.0f + factor;
        factor *= factor;
        var pR = R / 255.0f;
        pR -= 0.5f;
        pR *= factor;
        pR += 0.5f;
        pR *= 255;
        pR = pR switch
        {
            < 0 => 0,
            > 255 => 255,
            _ => pR,
        };
        var pG = G / 255.0f;
        pG -= 0.5f;
        pG *= factor;
        pG += 0.5f;
        pG *= 255;
        pG = pG switch
        {
            < 0 => 0,
            > 255 => 255,
            _ => pG,
        };
        var pB = B / 255.0f;
        pB -= 0.5f;
        pB *= factor;
        pB += 0.5f;
        pB *= 255;
        pB = pB switch
        {
            < 0 => 0,
            > 255 => 255,
            _ => pB,
        };
        result.R = (byte)pR;
        result.G = (byte)pG;
        result.B = (byte)pB;
        return result;
    }

    public Color Alpha(float alpha)
    {
        var result = this;
        alpha = alpha switch
        {
            < 0.0f => 0.0f,
            > 1.0f => 1.0f,
            _ => alpha,
        };
        result.A = (byte)(255.0f * alpha);
        return result;
    }

    public Color AlphaBlend(Color src, Color tint)
    {
        var result = White;
        src.R = (byte)(src.R * tint.R / 255);
        src.G = (byte)(src.G * tint.G / 255);
        src.B = (byte)(src.B * tint.B / 255);
        src.A = (byte)(src.A * tint.A / 255);
        switch (src.A)
        {
            case 0:
                result = this;
                break;
            case 255:
                result = src;
                break;
            default:
            {
                var alpha = src.A + 1;
                result.A = (byte)((alpha * 256 + A * (256 - alpha)) / 256);
                if (result.A > 0)
                {
                    result.R = (byte)((src.R * alpha * 256 + R * A * (256 - alpha)) / result.A / 256);
                    result.G = (byte)((src.G * alpha * 256 + G * A * (256 - alpha)) / result.A / 256);
                    result.B = (byte)((src.B * alpha * 256 + B * A * (256 - alpha)) / result.A / 256);
                }

                break;
            }
        }

        return result;
    }

    public Color Lerp(Color color, float factor)
    {
        var result = Transparent;
        factor = factor switch
        {
            < 0.0f => 0.0f,
            > 1.0f => 1.0f,
            _ => factor,
        };
        result.R = (byte)((1.0f - factor) * R + factor * color.R);
        result.G = (byte)((1.0f - factor) * G + factor * color.G);
        result.B = (byte)((1.0f - factor) * B + factor * color.B);
        result.A = (byte)((1.0f - factor) * A + factor * color.A);
        return result;
    }

    public Color Or(Color value)
    {
        return this == Transparent ? value : this;
    }
}
