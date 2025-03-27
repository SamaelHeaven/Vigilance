using Exception = System.Exception;

namespace Vigilance.Drawing;

public struct Color
{
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public static readonly Color LightGray = new(200, 200, 200);
    public static readonly Color Gray = new(130, 130, 130);
    public static readonly Color DarkGray = new(80, 80, 80);
    public static readonly Color Yellow = new(253, 249, 0);
    public static readonly Color Gold = new(255, 203, 0);
    public static readonly Color Orange = new(255, 161, 0);
    public static readonly Color Pink = new(255, 109, 194);
    public static readonly Color Red = new(230, 41, 55);
    public static readonly Color Maroon = new(190, 33, 55);
    public static readonly Color Green = new(0, 228, 48);
    public static readonly Color Lime = new(0, 158, 47);
    public static readonly Color DarkGreen = new(0, 117, 44);
    public static readonly Color SkyBlue = new(102, 191, 255);
    public static readonly Color Blue = new(0, 121, 241);
    public static readonly Color DarkBlue = new(0, 82, 172);
    public static readonly Color Purple = new(200, 122, 255);
    public static readonly Color Violet = new(135, 60, 190);
    public static readonly Color DarkPurple = new(112, 31, 126);
    public static readonly Color Beige = new(211, 176, 131);
    public static readonly Color Brown = new(127, 106, 79);
    public static readonly Color DarkBrown = new(76, 63, 47);

    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(string hexadecimal)
    {
        try
        {
            if (hexadecimal.StartsWith('#'))
                hexadecimal = hexadecimal[1..];
            if (hexadecimal.Length != 6 && hexadecimal.Length != 8)
                throw new Exception();
            R = Convert.ToByte(hexadecimal[..2], 16);
            G = Convert.ToByte(hexadecimal.Substring(2, 2), 16);
            B = Convert.ToByte(hexadecimal.Substring(4, 2), 16);
            A = hexadecimal.Length == 8 ? Convert.ToByte(hexadecimal.Substring(6, 2), 16) : (byte)255;
        }
        catch (Exception)
        {
            throw new ArgumentException($"Invalid hexadecimal color code: '{hexadecimal}'.");
        }
    }

    public static implicit operator Color(string hexadecimal)
    {
        return new Color(hexadecimal);
    }

    internal Raylib_cs.Color RColor => new(R, G, B, A);

    public override string ToString()
    {
        return $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
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
}
