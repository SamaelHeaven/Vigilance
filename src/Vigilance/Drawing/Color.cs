using System.Runtime.InteropServices;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct Color
{
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Transparent => default;
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

    public static implicit operator Color(string hex)
    {
        return FromHex(hex);
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

    public static Color FromInt(uint hex)
    {
        return new Color
        {
            R = (byte)((hex >> 24) & 0xff),
            G = (byte)((hex >> 16) & 0xff),
            B = (byte)((hex >> 8) & 0xff),
            A = (byte)(hex & 0xff),
        };
    }

    public static Color FromHex(string hex)
    {
        return TryFromHex(hex, out var color)
            ? color
            : throw new ArgumentException($"Invalid hexadecimal color code: '{hex}'.");
    }

    public static Color FromHsv(float hue, float saturation, float value)
    {
        return FromHsva(hue, saturation, value, 1.0f);
    }

    public static Color FromHsva(float hue, float saturation, float value, float alpha)
    {
        var result = new Color();
        var k = (5.0f + hue / 60.0f) % 6;
        var t = 4.0f - k;
        k = t < k ? t : k;
        k = k < 1 ? k : 1;
        k = k > 0 ? k : 0;
        result.R = (byte)((value - value * saturation * k) * 255.0f);
        k = (3.0f + hue / 60.0f) % 6;
        t = 4.0f - k;
        k = t < k ? t : k;
        k = k < 1 ? k : 1;
        k = k > 0 ? k : 0;
        result.G = (byte)((value - value * saturation * k) * 255.0f);
        k = (1.0f + hue / 60.0f) % 6;
        t = 4.0f - k;
        k = t < k ? t : k;
        k = k < 1 ? k : 1;
        k = k > 0 ? k : 0;
        result.B = (byte)((value - value * saturation * k) * 255.0f);
        result.A = (byte)(255 * alpha);
        return result;
    }

    public static bool TryFromHex(string? hex, out Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(hex))
            return false;
        var start = 0;
        if (hex[0] == '#')
        {
            if (hex.Length == 1)
                return false;
            start = 1;
        }

        var len = hex.Length - start;
        if (len != 6 && len != 8)
            return false;
        if (
            !TryParseByte(hex, start + 0, out var r)
            || !TryParseByte(hex, start + 2, out var g)
            || !TryParseByte(hex, start + 4, out var b)
        )
            return false;
        byte a = 255;
        if (len == 8 && !TryParseByte(hex, start + 6, out a))
            return false;
        color = new Color
        {
            R = r,
            G = g,
            B = b,
            A = a,
        };
        return true;

        static bool TryParseByte(string hex, int index, out byte value)
        {
            value = 0;
            if (index + 1 >= hex.Length)
                return false;
            var hi = HexValue(hex[index]);
            var lo = HexValue(hex[index + 1]);
            if (hi < 0 || lo < 0)
                return false;
            value = (byte)((hi << 4) | lo);
            return true;
        }

        static int HexValue(char c)
        {
            return c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => -1,
            };
        }
    }

    public readonly void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }

    public readonly void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    internal readonly Raylib_cs.BleedingEdge.Color RColor => new(R, G, B, A);

    public override string ToString()
    {
        return $"[R={R}, G={G}, B={B}, A={A}]";
    }

    public readonly string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public readonly uint ToInt()
    {
        return ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;
    }

    public readonly (float H, float S, float V) ToHsv()
    {
        (float H, float S, float V) hsv = (0, 0, 0);
        (float R, float G, float B) rgb = (R / 255.0f, G / 255.0f, B / 255.0f);
        var min = rgb.R < rgb.G ? rgb.R : rgb.G;
        min = min < rgb.B ? min : rgb.B;
        var max = rgb.R > rgb.G ? rgb.R : rgb.G;
        max = max > rgb.B ? max : rgb.B;
        hsv.V = max;
        var delta = max - min;
        if (delta < 0.00001f)
        {
            hsv.S = 0.0f;
            hsv.H = 0.0f;
            return hsv;
        }

        if (max > 0.0f)
        {
            hsv.S = delta / max;
        }
        else
        {
            hsv.S = 0.0f;
            hsv.H = float.NaN;
            return hsv;
        }

        if (rgb.R >= max)
        {
            hsv.H = (rgb.G - rgb.B) / delta;
        }
        else
        {
            if (rgb.G >= max)
                hsv.H = 2.0f + (rgb.B - rgb.R) / delta;
            else
                hsv.H = 4.0f + (rgb.R - rgb.G) / delta;
        }

        hsv.H *= 60.0f;
        if (hsv.H < 0.0f)
            hsv.H += 360.0f;
        return hsv;
    }

    public readonly (float H, float S, float V, float A) ToHsva()
    {
        var (h, s, v) = ToHsv();
        return (h, s, v, A / 255.0f);
    }

    public readonly Color Blend(Color color)
    {
        return new Color(
            (byte)((R + color.R) / 2),
            (byte)((G + color.G) / 2),
            (byte)((B + color.B) / 2),
            (byte)((A + color.A) / 2)
        );
    }

    public readonly Color Fade(float alpha)
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

    public readonly Color Tint(Color color)
    {
        var result = this;
        result.R = (byte)(R * color.R / 255);
        result.G = (byte)(G * color.G / 255);
        result.B = (byte)(B * color.B / 255);
        result.A = (byte)(A * color.A / 255);
        return result;
    }

    public readonly Color Brightness(float factor)
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

    public readonly Color Contrast(float factor)
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

    public readonly Color Alpha(float alpha)
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

    public readonly Color AlphaBlend(Color src, Color tint)
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

    public readonly Color Lerp(Color color, float factor)
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

    public readonly (float R, float G, float B, float A) Normalize()
    {
        return (R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
    }

    public readonly Color Or(Color value)
    {
        return this == default ? value : this;
    }
}
